using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MLOKit.Modules.AzureML
{
    class SearchTrainingJobLogs
    {

        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string trainingJobName, string searchString)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("search-training-job-logs", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals("") || trainingJobName.Equals("") || searchString.Equals(""))
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
                Console.WriteLine("[*] INFO: Performing search-training-job-logs module for " + platform);
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
                        // create random directory name in current working directory
                        string dirOut = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                        DirectoryInfo outputDir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + dirOut);

                        // if searching all training jobs
                        if (trainingJobName.Equals("all"))
                        {
                            Console.WriteLine("[*] INFO: Listing training job where logs will be searched");
                            Console.WriteLine("");

                            // get all training jobs
                            List<Objects.AzureML.TrainingJob> trainingJobList = await Utilities.AzureML.TrainingJobUtils.getAllTrainingJobs(credential, subscriptionID, region, resourceGroup, workspace);

                            // go through each training job and search logs for search string
                            foreach (Objects.AzureML.TrainingJob trainingJob in trainingJobList)
                            {

                                // only proceed if there is an associated datastore for the training job
                                if (!string.IsNullOrEmpty(trainingJob.datastoreUri))
                                {

                                    // create table header
                                    string tableHeader = string.Format("{0,40} | {1,10} | {2,20} | {3,10} | {4,20} | {5,20}", "Name", "Status", "Compute Target", "Job Type", "Job Output Type", "Datastore");
                                    Console.WriteLine(tableHeader);
                                    Console.WriteLine(new String('-', tableHeader.Length));


                                    // get a listing of all training jobs the user has access to in Azure
                                    string justDatastoreName = trainingJob.datastoreUri;
                                    justDatastoreName = justDatastoreName.Replace("azureml://datastores/", "");
                                    int endIndex = justDatastoreName.IndexOf("ExperimentRun");
                                    justDatastoreName = justDatastoreName.Substring(0, (justDatastoreName.Length + 4) - ("ExperimentRun".Length + "azureml://datastores/".Length));
                                    int lastIndexSlash = justDatastoreName.LastIndexOf("/");
                                    justDatastoreName = justDatastoreName.Substring(0, lastIndexSlash);
                                    Console.WriteLine("{0,40} | {1,10} | {2,20} | {3,10} | {4,20} | {5,20}", trainingJob.jobName, trainingJob.jobStatus, trainingJob.computeTarget, trainingJob.jobType, trainingJob.jobOutputType, justDatastoreName);
                                    Console.WriteLine("");

                                    Objects.AzureML.Datastore datStore = await Utilities.AzureML.DatastoreUtils.getSingleDatastore(credential, subscriptionID, region, resourceGroup, workspace, justDatastoreName);
                                    string assetPrefix = trainingJob.datastoreUri;
                                    assetPrefix = assetPrefix.Replace("azureml://datastores/", "");
                                    assetPrefix = assetPrefix.Replace(justDatastoreName + "/", "");
                                    List<string> contentURIList = await Utilities.AzureML.ModelUtils.getContentURIs(credential, subscriptionID, region, resourceGroup, workspace, assetPrefix);

                                    foreach (string contentURI in contentURIList)
                                    {
                                        // only search log files
                                        if ((contentURI.Contains("user_logs") || contentURI.Contains("system_logs")) && (contentURI.Contains(".log") || contentURI.Contains(".txt")))
                                        {

                                            byte[] fileContent = await Utilities.AzureML.ModelUtils.downloadFile(credential, contentURI);
                                            string asciiString = Encoding.ASCII.GetString(fileContent);

                                            string[] lines = asciiString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                            foreach (string line in lines)
                                            {
                                                if (line.Contains(searchString))
                                                {
                                                    Console.WriteLine("[+] SUCCESS: MATCH on search string of " + searchString + " - " + "Downloading Log File with the match");
                                                    Console.WriteLine("");
                                                    Console.WriteLine(line);
                                                    Console.WriteLine("");

                                                    // parse the actual file name to be downloaded
                                                    int startIndexMatchFile = contentURI.LastIndexOf('/') + 1;
                                                    int endIndexMatchFile = contentURI.IndexOf('?');
                                                    string fileName = contentURI.Substring(startIndexMatchFile, endIndexMatchFile - startIndexMatchFile);
                                                    File.WriteAllBytes(outputDir.FullName + "\\" + fileName, fileContent);
                                                    Console.WriteLine("[*] INFO: " + fileName + " written to: " + outputDir.FullName);
                                                    Console.WriteLine("");
                                                    Console.WriteLine("");
                                                    Console.WriteLine("");

                                                }

                                            }

                                        }
                                    }

                                }

                            }

                        }

                        // if searching a specific job
                        else
                        {
                            Console.WriteLine("[*] INFO: Listing training job where logs will be searched");
                            Console.WriteLine("");



                            // create table header
                            string tableHeader = string.Format("{0,40} | {1,10} | {2,20} | {3,10} | {4,20} | {5,20}", "Name", "Status", "Compute Target", "Job Type", "Job Output Type", "Datastore");
                            Console.WriteLine(tableHeader);
                            Console.WriteLine(new String('-', tableHeader.Length));

                            // get a listing of all training jobs the user has access to in Azure
                            Objects.AzureML.TrainingJob trainingJob = await Utilities.AzureML.TrainingJobUtils.getSingleTrainingJob(credential, subscriptionID, region, resourceGroup, workspace, trainingJobName);

                            // only proceed if there is an associated datastore for the training job
                            if (!string.IsNullOrEmpty(trainingJob.datastoreUri))
                            {
                                string justDatastoreName = trainingJob.datastoreUri;
                                justDatastoreName = justDatastoreName.Replace("azureml://datastores/", "");
                                int endIndex = justDatastoreName.IndexOf("ExperimentRun");
                                justDatastoreName = justDatastoreName.Substring(0, (justDatastoreName.Length + 4) - ("ExperimentRun".Length + "azureml://datastores/".Length));
                                int lastIndexSlash = justDatastoreName.LastIndexOf("/");
                                justDatastoreName = justDatastoreName.Substring(0, lastIndexSlash);
                                Console.WriteLine("{0,40} | {1,10} | {2,20} | {3,10} | {4,20} | {5,20}", trainingJob.jobName, trainingJob.jobStatus, trainingJob.computeTarget, trainingJob.jobType, trainingJob.jobOutputType, justDatastoreName);
                                Console.WriteLine("");

                                Objects.AzureML.Datastore datStore = await Utilities.AzureML.DatastoreUtils.getSingleDatastore(credential, subscriptionID, region, resourceGroup, workspace, justDatastoreName);
                                string assetPrefix = trainingJob.datastoreUri;
                                assetPrefix = assetPrefix.Replace("azureml://datastores/", "");
                                assetPrefix = assetPrefix.Replace(justDatastoreName + "/", "");
                                List<string> contentURIList = await Utilities.AzureML.ModelUtils.getContentURIs(credential, subscriptionID, region, resourceGroup, workspace, assetPrefix);

                                foreach (string contentURI in contentURIList)
                                {
                                    // only search log files
                                    if ((contentURI.Contains("user_logs") || contentURI.Contains("system_logs")) && (contentURI.Contains(".log") || contentURI.Contains(".txt")))
                                    {

                                        byte[] fileContent = await Utilities.AzureML.ModelUtils.downloadFile(credential, contentURI);
                                        string asciiString = Encoding.ASCII.GetString(fileContent);

                                        string[] lines = asciiString.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                                        foreach (string line in lines)
                                        {
                                            if (line.Contains(searchString))
                                            {
                                                Console.WriteLine("[+] SUCCESS: MATCH on search string of " + searchString + " - " + "Downloading Log File with the match");
                                                Console.WriteLine("");
                                                Console.WriteLine(line);
                                                Console.WriteLine("");

                                                // parse the actual file name to be downloaded
                                                int startIndexMatchFile = contentURI.LastIndexOf('/') + 1;
                                                int endIndexMatchFile = contentURI.IndexOf('?');
                                                string fileName = contentURI.Substring(startIndexMatchFile, endIndexMatchFile - startIndexMatchFile);
                                                File.WriteAllBytes(outputDir.FullName + "\\" + fileName, fileContent);
                                                Console.WriteLine("[*] INFO: " + fileName + " written to: " + outputDir.FullName);
                                                Console.WriteLine("");
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
