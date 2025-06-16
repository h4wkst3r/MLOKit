using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace MLOKit.Modules.AzureML
{
    class DownloadDataset
    {
        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string datasetID)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("download-dataset", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals("") || datasetID.Equals(""))
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


            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Performing download-datasets module for " + platform);
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

                    // get the full azure file path for a dataset
                    Console.WriteLine("[*] INFO: Getting Azure file path for dataset with ID: " + datasetID);
                    Console.WriteLine("");


                    Objects.AzureML.Dataset dataset = await Utilities.AzureML.DatasetUtils.getSingleDataset(credential, subscriptionID, region, resourceGroup, workspace, datasetID);
                    string azureFilePath = dataset.datasetFilePath;
                    Console.WriteLine(azureFilePath);
                    Console.WriteLine("");

                    string[] splitFilePath = azureFilePath.Split('/');
                    string storageAccount = splitFilePath[2];
                    string storageContainer = splitFilePath[3];

                    string relativePath = "";
                    for (int i = 4; i < splitFilePath.Length; i++)
                    {
                        relativePath += "/" + splitFilePath[i];
                    }
                    relativePath = relativePath.Substring(1);

                    Console.WriteLine("[*] INFO: Storage Account: ");
                    Console.WriteLine(storageAccount);
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Storage Container: ");
                    Console.WriteLine(storageContainer);
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Storage Relative Path: ");
                    Console.WriteLine(relativePath);
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Datastore Name: ");
                    Console.WriteLine(dataset.datastoreName);
                    Console.WriteLine("");

                    // get the account key for a given datastore
                    Objects.AzureML.Datastore datastore = await Utilities.AzureML.DatastoreUtils.getSingleDatastore(credential, subscriptionID, region, resourceGroup, workspace, dataset.datastoreName);


                    // form the authorization header based on the account name and key - https://github.com/Azure-Samples/storage-dotnet-rest-api-with-auth/blob/master/StorageRestApiAuth/AzureStorageAuthenticationHelper.cs#L33
                    // enumerate storage blob container - https://github.com/Azure-Samples/storage-dotnet-rest-api-with-auth/blob/master/StorageRestApiAuth/Program.cs
                    // get our target file
                    byte [] datasetContent = await Utilities.AzureML.StorageBlobUtils.downloadFileFromStorageBlob(datastore.accountName, datastore.datastoreCredential, storageContainer, relativePath, CancellationToken.None);

                    // if we got dataset back, then proceed
                    if (datasetContent != null)
                    {
                        string fileOut = Utilities.FileUtils.generateRandomName();
                        fileOut = "MLOKit-" + fileOut;

                        // write dataset file to disk
                        File.WriteAllBytes(fileOut, datasetContent);


                        Console.WriteLine("");
                        Console.WriteLine("[+] SUCCESS: Dataset written to: " + Environment.CurrentDirectory + "\\" + fileOut);
                        Console.WriteLine("");

                        Console.WriteLine("");
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
    }
}
