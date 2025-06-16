using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MLOKit.Modules.VertexAI
{
    class DownloadModel
    {

        public static async Task execute(string credential, string platform, string project, string modelID)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("download-model", credential, platform));

            // check for additional required arguments
            if (modelID.Equals("") || project.Equals(""))
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

                // get a listing of all regions for a given project    
                List<string> regionList = await Utilities.VertexAI.WebUtils.getAllRegions(credential, project);

                // the model to download
                Objects.VertexAI.Model modelToDownload = null;

                Console.WriteLine("[*] INFO: Finding model with ID of " + modelID);
                Console.WriteLine("");

                // get all the models in each region
                foreach (string region in regionList)
                {
                    List<Objects.VertexAI.Model> modelList = await Utilities.VertexAI.ModelUtils.getAllModels(credential, region, project);

                    // iterate through the list of models 
                    foreach (Objects.VertexAI.Model mod in modelList)
                    {
                        // if we found our model, grab the info for it
                        if (mod.modelID.Equals(modelID))
                        {
                            modelToDownload = mod;
                            string tableHeader = string.Format("{0,30} | {1,20} | {2,25} | {3,15} | {4,15} | {5,15}", "Name", "Model ID", "Creation Date", "Region", "Model Type", "Export Format");
                            Console.WriteLine(tableHeader);
                            Console.WriteLine(new String('-', tableHeader.Length));
                            Console.WriteLine("{0,30} | {1,20} | {2,25} | {3,15} | {4,15} | {5,15}", mod.modelDisplayName, mod.modelID, mod.createTime, mod.region, mod.sourceType, mod.exportableFormat);
                            Console.WriteLine("");
                        }

                    }
                }

                // if model was not found display error message
                if (modelToDownload == null)
                {
                    Console.WriteLine("[-] ERROR: Unable to find model with the model ID given");
                    Console.WriteLine("");
                }

                // if model was found continue with exporting and downloading it
                else
                {

                    Console.WriteLine("[*] INFO: Exporting model to Cloud Storage");
                    Console.WriteLine("");

                    // get listing of all buckets for the project provided
                    List<string> buckets = await Utilities.VertexAI.BucketUtils.getBuckets(credential, project);
                    string outputArtifactUri = "";
                    string theBucket = "";
                    foreach (string bucket in buckets)
                    {
                        // if export was not successful, try the next bucket
                        if (outputArtifactUri.Equals(""))
                        {
                            outputArtifactUri = await Utilities.VertexAI.ModelUtils.exportModel(credential, modelToDownload.modelID, modelToDownload.region, bucket, modelToDownload.exportableFormat, project);

                            // if export was successful, capture the bucket name as it will be needed for subsequent requests
                            if (!outputArtifactUri.Equals(""))
                            {
                                theBucket = bucket;
                            }
                        
                        }
                    }

                    // if unable to export model
                    if (outputArtifactUri.Equals(""))
                    {
                        Console.WriteLine("[-] ERROR: Unable to export model");
                        Console.WriteLine("");
                    }

                    // if able to export model
                    else
                    {

                        Console.WriteLine("[+] SUCCESS: Successfully exported model to: ");
                        Console.WriteLine("");
                        Console.WriteLine(outputArtifactUri);
                        Console.WriteLine("");


                        // get the mediaLink for each file in the exported model folder
                        Console.WriteLine("[*] INFO: Getting mediaLinks for files in the exported model folder");
                        Console.WriteLine("");

                        // parse out the folder path
                        string folderPath = outputArtifactUri.Replace("gs://", "");
                        folderPath = folderPath.Replace(theBucket, "");

                        // sleep 15 seconds to give GCloud enough time to be able to populate the list of files within the exported model folder
                        Thread.Sleep(15000);
                        
                        // recursively list each file in a given folder
                        List<String> fileList = await Utilities.VertexAI.BucketUtils.recursiveListFolder(credential, theBucket, folderPath);

                        // this is where we will store the media links for the files we need to download
                        List<string> links = new List<string>();

                        // go through each file in the exported model folder and get the media links
                        foreach (string file in fileList)
                        {
                            // store media links for a given file here
                            List<string> mediaLinkList = await Utilities.VertexAI.BucketUtils.getMediaLinks(credential, theBucket, file);

                            // go through media links
                            foreach (string link in mediaLinkList)
                            {
                                // if we don't already have the link, add it to the list and print it
                                if (!links.Contains(link))
                                {
                                    links.Add(link);

                                }

                            } /// end for each media link


                        } // end for each file in the exported model folder

                        // create a directory and download each file to a folder in our current working directory

                        // create random directory name
                        string dirOut = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                        DirectoryInfo outputDir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + dirOut);

                        Console.WriteLine("[*] INFO: Downloading files");
                        Console.WriteLine("");


                        // go through each mediaLink and download file to the working directory we created
                        foreach (string theLink in links)
                        {

                            // clean up the link to get proper file name and path
                            string[] splitMediaLink = theLink.Split('/');
                            string fileName = splitMediaLink[splitMediaLink.Length - 1];
                            fileName = fileName.Replace("%2F", "\\");
                            int indexOfGetVar = fileName.LastIndexOf('?');
                            fileName = fileName.Substring(0, indexOfGetVar);
                            fileName = fileName.Replace(":", "-");

                            Console.WriteLine("[*] INFO: Downloading file at: " + theLink);
                            Console.WriteLine("");

                            // download the file
                            byte [] downloadedFile = await Utilities.VertexAI.ModelUtils.downloadModel(credential, theLink);

                            // do some cleanup on the folder and file name so it looks readable
                            string finalFolderPath = "";
                            int indexOfLastFolderPath = fileName.LastIndexOf('\\');
                            finalFolderPath = fileName.Substring(0, indexOfLastFolderPath);
                            string finalFileName = fileName.Replace(finalFolderPath, "");
                            finalFileName = finalFileName.Replace("\\", "");

                            // write downloaded file to disk in proper directory structure to mimic what is in GCloud storage
                            Directory.CreateDirectory(outputDir.FullName + "\\" + finalFolderPath);
                            File.WriteAllBytes(outputDir.FullName + "\\" + finalFolderPath + "\\" +  finalFileName, downloadedFile);

                        }

                        Console.WriteLine("[+] SUCCESS: Model files written to: " + outputDir.FullName);
                        Console.WriteLine("");


                    } // end if able to export model

                } // end if model was found

            } // if creds valid

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
