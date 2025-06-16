using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;

namespace MLOKit.Modules.AzureML
{
    class DownloadModel
    {
        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string modelID)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("download-model", credential, platform));

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
                Console.WriteLine("[*] INFO: Performing download-model module for " + platform);
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


                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,30} | {2,15} | {3,25} | {4,25}", "Name", "ID", "Model Type", "Creation Time", "Update Time");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get the model by model ID
                        Objects.AzureML.Model model = await Utilities.AzureML.ModelUtils.getSingleModel(credential, subscriptionID, region, resourceGroup, workspace, modelID);
                        Console.WriteLine("{0,30} | {1,30} | {2,15} | {3,25} | {4,25}", model.modelName, model.modelID, model.modelType, model.createdTime, model.modifiedTime);


                        // get the asset prefix based on the model's asset ID
                        Console.WriteLine("");
                        Console.WriteLine("");
                        Console.WriteLine("");
                        Console.WriteLine("[*] INFO: Getting asset prefixes based on model's asset ID");
                        Console.WriteLine("");
                        List<string> assetPrefixList = await Utilities.AzureML.ModelUtils.getAssetPrefixes(credential, subscriptionID, region, resourceGroup, workspace, model.assetID);
                        foreach (string assetPrefix in assetPrefixList)
                        {
                            // get the SAS tokens and contentURI's based on the artifact prefix
                            Console.WriteLine("");
                            Console.WriteLine("[*] INFO: Getting content URI's based on asset prefixes");
                            Console.WriteLine("");
                            List<string> contentURIList = await Utilities.AzureML.ModelUtils.getContentURIs(credential, subscriptionID, region, resourceGroup, workspace, assetPrefix);

                            // create random directory name in current working directory
                            string dirOut = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                            DirectoryInfo outputDir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + dirOut);

                            Console.WriteLine("");
                            Console.WriteLine("[*] INFO: Downloading model files for model with ID " + modelID + " to the directory of: " + outputDir.FullName);
                            Console.WriteLine("");

                            // go through each content URI that includes the SAS token and download the file
                            foreach (string contentURI in contentURIList)
                            {
                                // parse the actual file name to be downloaded
                                int startIndex = contentURI.LastIndexOf('/') + 1;
                                int endIndex = contentURI.IndexOf('?');
                                string fileName = contentURI.Substring(startIndex, endIndex - startIndex);                        

                                // download the file
                                Console.WriteLine("");
                                Console.WriteLine("[*] INFO: Downloading file:");
                                Console.WriteLine("");
                                Console.WriteLine(contentURI);
                                Console.WriteLine("");
                                byte [] fileContent = await Utilities.AzureML.ModelUtils.downloadFile(credential, contentURI);

                                // if we got file back, then proceed to write it
                                if (fileContent != null)
                                {

                                    File.WriteAllBytes(outputDir.FullName + "\\" + fileName, fileContent);
                                    Console.WriteLine("");
                                    Console.WriteLine("[+] SUCCESS: " + fileName + " written to: " + outputDir.FullName);
                                    Console.WriteLine("");
                                }

                            }

                        }


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
