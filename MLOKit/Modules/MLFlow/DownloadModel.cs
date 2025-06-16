using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;

namespace MLOKit.Modules.MLFlow
{
    class DownloadModel
    {

        public static async Task execute(string credential, string platform, string url, string modelID)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("download-model", credential, platform));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Performing download-model module for " + platform);
            Console.WriteLine("");


            // check if credentials provided are valid
            Console.WriteLine("[*] INFO: Checking credentials provided");
            Console.WriteLine("");

            // if creds valid, then provide message and continue
            if (await Utilities.MLFlow.WebUtils.credsValid(credential, url))
            {
                Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                Console.WriteLine("");

                try
                {

                    // create table header
                    string tableHeader = string.Format("{0,60}", "Artifact");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // get a listing of all models the user has access to in MLFlow
                    List<Objects.MLFlow.Model> modelList = await Utilities.MLFlow.ModelUtils.getAllModels(credential, url);

                    // iterate through the list of models
                    foreach (Objects.MLFlow.Model modelSet in modelList)
                    {
                        // get the model matching the model ID provided by user
                        if (modelSet.modelName.ToLower().Equals(modelID.ToLower()))
                        {
                            // get the run ID for the associated model
                            string runID = modelSet.associatedRun;

                            // get the artifact directory for the associated run
                            string directory = await Utilities.MLFlow.RunUtils.getArtifactDirectory(credential, url, runID);

                            // get listing of artifacts for a given run and the artifact root directory
                            List<Objects.MLFlow.Artifact> artifactList = await Utilities.MLFlow.RunUtils.getArtifactListing(credential, url, runID, directory);

                            // iterate through the list of artifacts
                            foreach (Objects.MLFlow.Artifact artifactSet in artifactList)
                            {

                                Console.WriteLine("{0,60}", artifactSet.path);


                            }

                            Console.WriteLine("");
                            Console.WriteLine("");

                            // create random directory name in current working directory
                            string dirOut = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                            DirectoryInfo outputDir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + dirOut);

                            // download each file
                            foreach (Objects.MLFlow.Artifact artifactSet in artifactList)
                            {

                                Console.WriteLine("");
                                Console.WriteLine("[*] INFO: Downloading " + artifactSet.path);
                                Console.WriteLine("");

                                byte [] fileContent = await Utilities.MLFlow.RunUtils.downloadRunArtifact(credential, url, runID, artifactSet.path);

                                // if we got file back, then proceed to write it
                                if (fileContent != null)
                                {
                                    // create directory structure
                                    int lastIndexSlash = artifactSet.path.LastIndexOf("/");
                                    string fileName = artifactSet.path.Substring(lastIndexSlash + 1);
                                    string folderPath = artifactSet.path.Substring(0, artifactSet.path.Length - fileName.Length);
                                    Directory.CreateDirectory(outputDir.FullName + "\\" + folderPath);

                                    // write file
                                    File.WriteAllBytes(outputDir.FullName + "\\" + artifactSet.path, fileContent);
                                    Console.WriteLine("[+] SUCCESS: " + artifactSet.path + " written to: " + outputDir.FullName);
                                    Console.WriteLine("");
                                }

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
