using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Net.WebSockets;
using System.Threading;

namespace MLOKit.Modules.AzureML
{
    class ExecNotebookCode
    {

        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string computeID, string script)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("exec-notebook-code", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals("") || computeID.Equals("") || script.Equals(""))
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Missing one of required command arguments");
                Console.WriteLine("");
                return;
            }

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing exec-notebook-code module for " + platform);
                Console.WriteLine("");

                if (!File.Exists(script))
                {
                    Console.WriteLine("\n[-] ERROR: Script file provided does not exist. Please check the path again.\n");
                    return;
                }

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                // if creds valid, then provide message
                if (await Utilities.AzureML.WebUtils.credsValid(credential, "https://management.azure.com/subscriptions?api-version=2022-12-01"))
                {
                    Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                    Console.WriteLine("");

                    try
                    {

                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,10} | {2,50}", "Compute Name", "State", "Jupyter Endpoint");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get your target compute instance
                        Objects.AzureML.Compute compute = await Utilities.AzureML.ComputeUtils.getSingleComputeInstance(credential, subscriptionID, region, resourceGroup, workspace, computeID);
                        Console.WriteLine("{0,30} | {1,10} | {2,50}", compute.computeName, compute.state, compute.jupyterEndpoint);
                        Console.WriteLine("");

                        // get our code from the provided script file
                        var code = File.ReadAllText(script, Encoding.UTF8);

                        Uri notebookInstanceUri = new Uri(compute.jupyterEndpoint);

                        // Bootstrap HTTP session (cookies + final URL)
                        var handler = new HttpClientHandler
                        {
                            AllowAutoRedirect = true,
                            UseCookies = true,
                            CookieContainer = new CookieContainer()
                        };
                        var http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(30) };

                        // Compute origin correctly (scheme://host[:port]) 
                        var origin = notebookInstanceUri.Scheme + "://" + notebookInstanceUri.Host +
                                     (notebookInstanceUri.IsDefaultPort ? "" : ":" + notebookInstanceUri.Port);

                        var basePath = "/";
                        var baseAPI = new Uri(origin + basePath);
                        string token = credential;

                        // List existing kernels 
                        var kernelList = await Utilities.AzureML.NotebookUtils.getAllKernels(credential, subscriptionID, region, resourceGroup, workspace, baseAPI);

                        string tableHeaderKernels = string.Format("{0,40} | {1,30} | {2,15}", "Kernel ID", "Kernel Name", "State");
                        Console.WriteLine(tableHeaderKernels);
                        Console.WriteLine(new string('-', tableHeaderKernels.Length));


                        // if there is an existing kernel, choose one at random to execute against
                        string kernelIDToTarget = null;
                        if (kernelList.Count > 0)
                        {
                            foreach (var kernel in kernelList)
                            {
                                if (kernel.id != "")
                                {
                                    Console.WriteLine("{0,40} | {1,30} | {2,15}", kernel.id, kernel.name, kernel.state);
                                    kernelIDToTarget = kernel.id;

                                }
                            }
                        }

                        // If no kernels, create one
                        if (kernelList.Count == 0)
                        {
                            kernelIDToTarget = await Utilities.AzureML.JupyterUtils.createKernel(handler.CookieContainer, baseAPI, token, http);
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
                        await Utilities.SageMaker.JupyterUtils.SendTextAsync(webSock, Utilities.AzureML.JupyterUtils.BuildKernelInfoText(sessionId));

                        // Execute script contents
                        await Utilities.SageMaker.JupyterUtils.SendTextAsync(webSock, Utilities.AzureML.JupyterUtils.BuildExecuteText(code, sessionId));

                        Console.WriteLine("");
                        Console.WriteLine("[*] INFO: Executing script against Notebook instance");
                        Console.WriteLine("");

                        // Receive & print outputs until idle + execute_reply
                        var ok = await Utilities.AzureML.JupyterUtils.ReceiveUntilIdleAsync(webSock);
                        Console.WriteLine("");
                        Console.WriteLine(ok ? "[+] SUCCESS: Execution Completed" : "[-] ERROR: Execution Failed");

                        try
                        {
                            await webSock.CloseAsync(WebSocketCloseStatus.NormalClosure, "done", CancellationToken.None);
                        }
                        catch
                        {
                            /* ignore */
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("");
                        Console.WriteLine("[-] ERROR: " + ex.Message);
                        Console.WriteLine("");
                    }

                }

                // if creds not valid, display message
                else
                {
                    Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
                    Console.WriteLine("");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }


        }


    }
}
