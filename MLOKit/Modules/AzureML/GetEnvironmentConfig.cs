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
    class GetEnvironmentConfig
    {

        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string environmentName)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("get-environment-config", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals(""))
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
                Console.WriteLine("[*] INFO: Performing get-environment-config module for " + platform);
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

                        // if getting env config for all environments
                        if(environmentName.Equals("all"))
                        {

                            Console.WriteLine("[*] INFO: Only listing environments with custom Docker build images");
                            Console.WriteLine("");

                            // create table header
                            string tableHeader = string.Format("{0,50} | {1,20} | {2,80}", "Name", "Docker File Path", "Docker Build URL");

                            // get a listing of all training jobs the user has access to in Azure
                            List<Objects.AzureML.Environment> environmentList = await Utilities.AzureML.EnvironmentUtils.getAllEnvironments(credential, subscriptionID, region, resourceGroup, workspace);

                            // iterate through the list of environments
                            foreach (Objects.AzureML.Environment environment in environmentList)
                            {
                                if (environment.buildFilePath != "" && environment.buildContextUri != "")
                                {
                                    Console.WriteLine(tableHeader);
                                    Console.WriteLine(new String('-', tableHeader.Length));
                                    Console.WriteLine("{0,50} | {1,20} | {2,80}", environment.name, environment.buildFilePath, environment.buildContextUri);
                                    Console.WriteLine("");

                                    string[] splitDockerBuildURL = environment.buildContextUri.Split('/');
                                    string storageEndpoint = splitDockerBuildURL[2];
                                    string storageContainer = splitDockerBuildURL[3];
                                    string relativePath = "";

                                    for (int i = 4; i < splitDockerBuildURL.Length; i++)
                                    {
                                        relativePath += splitDockerBuildURL[i] + "/";
                                    }

                                    relativePath += environment.buildFilePath;
                                    relativePath = relativePath.Replace("//", "/");


                                    // list all datastores and find associated datastore for where the environment config is stored
                                    List<Objects.AzureML.Datastore> dataStoreList = await Utilities.AzureML.DatastoreUtils.getAllDataStores(credential, subscriptionID, region, resourceGroup, workspace);

                                    // go through each datastore and find the matching one
                                    foreach (Objects.AzureML.Datastore datStore in dataStoreList)
                                    {
                                        if (storageEndpoint.Contains(datStore.accountName) && storageContainer.Equals(datStore.containerName))
                                        {

                                            Objects.AzureML.Datastore matchingStore = await Utilities.AzureML.DatastoreUtils.getSingleDatastore(credential, subscriptionID, region, resourceGroup, workspace, datStore.datstoreName);

                                            // download the docker build file
                                            byte[] buildFileContent = await Utilities.AzureML.StorageBlobUtils.downloadFileFromStorageBlob(matchingStore.accountName, matchingStore.datastoreCredential, storageContainer, relativePath, CancellationToken.None);

                                            // if we got file back, then proceed
                                            if (buildFileContent != null)
                                            {
                                                string fileOut = environment.name + "-" + environment.buildFilePath;

                                                // write dataset file to disk
                                                File.WriteAllBytes(fileOut, buildFileContent);


                                                Console.WriteLine("[+] SUCCESS: Docker build file written to: " + Environment.CurrentDirectory + "\\" + fileOut);
                                                Console.WriteLine("");
                                                Console.WriteLine("");
                                            }

                                        }

                                    }
                                }


                            }

                        }

                        // if getting env config for a specific environment
                        else
                        {
                            Console.WriteLine("[*] INFO: Only listing environments with custom Docker build images");
                            Console.WriteLine("");

                            // create table header
                            string tableHeader = string.Format("{0,50} | {1,20} | {2,80}", "Name", "Docker File Path", "Docker Build URL");
                            Console.WriteLine(tableHeader);
                            Console.WriteLine(new String('-', tableHeader.Length));


                            // get a listing of all training jobs the user has access to in Azure
                            List<Objects.AzureML.Environment> environmentList = await Utilities.AzureML.EnvironmentUtils.getAllEnvironments(credential, subscriptionID, region, resourceGroup, workspace);

                            // iterate through the list of environments
                            foreach (Objects.AzureML.Environment environment in environmentList)
                            {

                                // if we match the environment name we are looking for
                                if (environment.buildFilePath != "" && environment.buildContextUri != "" && environment.name.Equals(environmentName))
                                {
                                    Console.WriteLine("{0,50} | {1,20} | {2,80}", environment.name, environment.buildFilePath, environment.buildContextUri);
                                    Console.WriteLine("");

                                    string[] splitDockerBuildURL = environment.buildContextUri.Split('/');
                                    string storageEndpoint = splitDockerBuildURL[2];
                                    string storageContainer = splitDockerBuildURL[3];
                                    string relativePath = "";

                                    for(int i=4;i<splitDockerBuildURL.Length;i++)
                                    {
                                        relativePath += splitDockerBuildURL[i] + "/";
                                    }

                                    relativePath += environment.buildFilePath;
                                    relativePath = relativePath.Replace("//", "/");


                                    // list all datastores and find associated datastore for where the environment config is stored
                                    List<Objects.AzureML.Datastore> dataStoreList = await Utilities.AzureML.DatastoreUtils.getAllDataStores(credential, subscriptionID, region, resourceGroup, workspace);
 
                                    // go through each datastore and find the matching one
                                    foreach (Objects.AzureML.Datastore datStore in dataStoreList)
                                    {
                                        if (storageEndpoint.Contains(datStore.accountName) && storageContainer.Equals(datStore.containerName))
                                        {

                                            Objects.AzureML.Datastore matchingStore = await Utilities.AzureML.DatastoreUtils.getSingleDatastore(credential, subscriptionID, region, resourceGroup, workspace, datStore.datstoreName);

                                            // download the docker build file
                                            byte[] buildFileContent = await Utilities.AzureML.StorageBlobUtils.downloadFileFromStorageBlob(matchingStore.accountName, matchingStore.datastoreCredential, storageContainer, relativePath, CancellationToken.None);

                                            // if we got file back, then proceed
                                            if (buildFileContent != null)
                                            {
                                                string fileOut = environment.name + "-" + environment.buildFilePath;

                                                // write dataset file to disk
                                                File.WriteAllBytes(fileOut, buildFileContent);


                                                Console.WriteLine("[+] SUCCESS: Docker build file written to: " + Environment.CurrentDirectory + "\\" + fileOut);
                                                Console.WriteLine("");

                                                Console.WriteLine("");
                                            }

                                        }

                                    }


                                }


                            }



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
