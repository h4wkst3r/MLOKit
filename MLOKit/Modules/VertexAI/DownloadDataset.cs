using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;

namespace MLOKit.Modules.VertexAI
{
    class DownloadDataset
    {
        public static async Task execute(string credential, string platform, string projectName, string datsetID)
        {

            bool datasetFound = false;

            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("download-dataset", credential, platform));

            // check for additional required arguments
            if (projectName.Equals("") || datsetID.Equals(""))
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

            // check if credentials provided are valid
            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Checking credentials provided");
            Console.WriteLine("");

            // if creds valid, then provide message and continue
            if (await Utilities.VertexAI.WebUtils.credsValid(credential, "https://cloudresourcemanager.googleapis.com/v1/projects?alt=json&filter=lifecycleState%3AACTIVE&pageSize=500"))
            {
                Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                Console.WriteLine("");
                string mediaLink = "";



                // get a listing of all regions for a given project
                Console.WriteLine("[*] INFO: Getting all regions for the " + projectName + " project");
                Console.WriteLine("");
                List<string> regionList = await Utilities.VertexAI.WebUtils.getAllRegions(credential, projectName);

                // get all the datasets in each region
                foreach (string region in regionList)
                {

                    try
                    {

                        List<Objects.VertexAI.Dataset> datasetList = await Utilities.VertexAI.DatasetUtils.getAllDatasets(credential, region, projectName);

                        // iterate through the list of datasets and get the dataset we are looking for by ID
                        foreach (Objects.VertexAI.Dataset datSet in datasetList)
                        {

                            // we found the dataset by ID, now let's get the media link and download it
                            if (datSet.datasetID == datsetID)
                            {

                                datasetFound = true;

                                // get the mediaLink for the file
                                Console.WriteLine("[*] INFO: Getting mediaLink for " + datSet.uri);
                                Console.WriteLine("");

                                string filePath = datSet.uri.Replace("gs://", "");
                                string fileName = "";
                                string bucket = "";

                                string[] splitFilePath = filePath.Split('/');
                                bucket = splitFilePath[0];
                                fileName = splitFilePath[splitFilePath.Length - 1];
                                filePath = filePath.Replace(bucket, "");
                                filePath = filePath.Substring(1);


                                List<string> mediaLinkList = await Utilities.VertexAI.BucketUtils.getMediaLinks(credential, bucket, filePath);

                                foreach (string link in mediaLinkList)
                                {
                                    // if media link contains the file name we are trying to download, then grab it
                                    if (link.Contains(fileName))
                                    {
                                        mediaLink = link;

                                        // download the dataset and store in base64
                                        byte [] datasetContent = await Utilities.VertexAI.DatasetUtils.downloadDataset(credential, mediaLink);

                                        // if we got dataset back, then proceed
                                        if (datasetContent != null)
                                        {
                                            string fileOut = Utilities.FileUtils.generateRandomName();
                                            fileOut = "MLOKit-" + fileOut;

                                            // write dataset file to disk
                                            File.WriteAllBytes(fileOut, datasetContent);


                                            Console.WriteLine("[+] SUCCESS: Dataset written to: " + Environment.CurrentDirectory + "\\" + fileOut);
                                            Console.WriteLine("");

                                            Console.WriteLine("");
                                        }



                                    }
                                }

                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                }  // end getting all datasets in each region

                if (!datasetFound)
                {
                    Console.WriteLine("[-] ERROR: Dataset with ID of " + datsetID +  " not found.");
                    Console.WriteLine("");

                }

                Console.WriteLine("");



            }

            // if creds not valid, display message and return
            else
            {
                Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
                Console.WriteLine("");
                return;
            }


        }
    }
}
