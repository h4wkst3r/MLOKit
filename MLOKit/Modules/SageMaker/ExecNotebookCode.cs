using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;
using System.Net.Http;
using System.Text;
using System.Net.WebSockets;

namespace MLOKit.Modules.SageMaker
{
    class ExecNotebookCode
    {
        public static async Task execute(string credential, string platform, string notebookName, string script, RegionEndpoint endpoint)
        {
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("exec-notebook-code", credential, platform));

            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                Console.WriteLine("\n[*] INFO: Performing exec-notebook-code module for " + platform + "\n");
                Console.WriteLine("[*] INFO: Checking credentials provided\n");

                string[] splitCreds = credential.Split(';');
                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];
                string sessionToken = (splitCreds.Length >= 3) ? splitCreds[2] : null; // NEW

                var sessionSeconds = 4 * 60 * 60; // up to 12h

                AmazonSageMakerClient sagemakerClient =
                    string.IsNullOrEmpty(sessionToken)
                    ? new AmazonSageMakerClient(accessKey, secretKey, endpoint)
                    : new AmazonSageMakerClient(accessKey, secretKey, sessionToken, endpoint); // NEW

                if (!File.Exists(script))
                {
                    Console.WriteLine("\n[-] ERROR: Script file provided does not exist. Please check the path again.\n");
                    return;
                }

                // get notebook status
                string notebookStatus = await Utilities.SageMaker.NotebookUtils.getNotebookStatus(sagemakerClient, notebookName);

                // if notebook is not "InService" then start it
                if (!notebookStatus.ToLower().Equals("inservice"))
                {
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Starting Notebook instance with name - " + notebookName);

                    string result = await Utilities.SageMaker.NotebookUtils.startNotebook(sagemakerClient, notebookName);

                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Waiting for notebook instance to start before proceeding (can take up to 10 minutes)");

                    // wait to proceed until notebook comes online
                    while (await Utilities.SageMaker.NotebookUtils.getNotebookStatus(sagemakerClient, notebookName) != "InService")
                    {
                        // do nothing
                    }

                    Console.WriteLine("");
                    Console.WriteLine("[+] SUCCESS: Notebook has been started");
                }

                // get our code from the provided script file
                var code = File.ReadAllText(script, Encoding.UTF8);

                // Get presigned URL
                var pre = await sagemakerClient.CreatePresignedNotebookInstanceUrlAsync(
                    new CreatePresignedNotebookInstanceUrlRequest
                    {
                        NotebookInstanceName = notebookName,
                        SessionExpirationDurationInSeconds = sessionSeconds
                    });
                var authorizedUrl = pre.AuthorizedUrl;

                // Bootstrap HTTP session (cookies + final URL)
                var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    UseCookies = true,
                    CookieContainer = new CookieContainer()
                };
                var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

                var bootstrap = await http.GetAsync(authorizedUrl);
                bootstrap.EnsureSuccessStatusCode();
                var notebookInstanceUri = bootstrap.RequestMessage.RequestUri;

                // Compute origin correctly (scheme://host[:port]) 
                var origin = notebookInstanceUri.Scheme + "://" + notebookInstanceUri.Host +
                             (notebookInstanceUri.IsDefaultPort ? "" : ":" + notebookInstanceUri.Port);

                // Detect base path (fallback to Utilities; otherwise try /jupyter/default/)
                var basePath = Utilities.SageMaker.NotebookUtils.getBasePath(notebookInstanceUri, handler, http);
                if (string.IsNullOrEmpty(basePath))
                {
                    basePath = "/jupyter/default/";
                }
                var baseAPI = new Uri(origin + basePath);

                // Pull token from final URL (optional)
                var token = System.Web.HttpUtility.ParseQueryString(notebookInstanceUri.Query).Get("token");

                // List existing kernels 
                var kernelList = await Utilities.SageMaker.NotebookUtils.getKernels(sagemakerClient, notebookInstanceUri, handler, http);

                string tableHeader = string.Format("{0,40} | {1,30} | {2,15}", "Kernel ID", "Kernel Name", "State");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new string('-', tableHeader.Length));

                // if there is an existing kernel, choose one at random to execute against
                string kernelIDToTarget = null;
                foreach (var kernel in kernelList)
                {
                    if (kernel.id != "")
                    {
                        Console.WriteLine("{0,40} | {1,30} | {2,15}", kernel.id, kernel.name, kernel.state);
                        kernelIDToTarget = kernel.id;

                    }
                }

                // If no kernels, create one
                if (kernelList.Count == 0)
                {

                    kernelIDToTarget = await Utilities.SageMaker.JupyterUtils.createKernel(handler.CookieContainer, baseAPI, token, http);

                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Created kernel: " + kernelIDToTarget);
                }

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Kernel ID to target: " + kernelIDToTarget);

                // initiate web socket connection to /api/kernels/{id}/channels
                var sessionId = Guid.NewGuid().ToString();
                var wsScheme = notebookInstanceUri.Scheme == "https" ? "wss" : "ws";
                var wsUri = new UriBuilder($"{wsScheme}://{notebookInstanceUri.Host}{(notebookInstanceUri.IsDefaultPort ? "" : ":" + notebookInstanceUri.Port)}{basePath}api/kernels/{kernelIDToTarget}/channels");
                ClientWebSocket webSock = await Utilities.SageMaker.JupyterUtils.webSocketConnection(notebookInstanceUri, handler, basePath, token, baseAPI, origin, sessionId, kernelIDToTarget, wsUri);
                await webSock.ConnectAsync(wsUri.Uri, CancellationToken.None);
                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Connected to Notebook instance via WebSocket");

                // Probe kernel
                await Utilities.SageMaker.JupyterUtils.SendTextAsync(webSock, Utilities.SageMaker.JupyterUtils.BuildKernelInfoText(sessionId));

                // Execute script contents
                await Utilities.SageMaker.JupyterUtils.SendTextAsync(webSock, Utilities.SageMaker.JupyterUtils.BuildExecuteText(code, sessionId));

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Executing script against Notebook instance");
                Console.WriteLine("");

                // Receive & print outputs until idle + execute_reply
                var ok = await Utilities.SageMaker.JupyterUtils.ReceiveUntilIdleAsync(webSock);
                Console.WriteLine("");
                Console.WriteLine(ok ? "[+] SUCCESS: Execution Completed" : "[-] ERROR: Execution Failed");

                try { 
                    await webSock.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None); 
                } catch { 
                    /* ignore */ 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }
        }

    }
}
