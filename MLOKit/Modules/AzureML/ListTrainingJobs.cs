using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace MLOKit.Modules.AzureML
{
    class ListTrainingJobs
    {
        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string trainingJobName)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("list-training-jobs", credential, platform));

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
                Console.WriteLine("[*] INFO: Performing list-training-jobs module for " + platform);
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
                        string tableHeader = string.Format("{0,40} | {1,10} | {2,20} | {3,10} | {4,20} | {5,20}", "Name", "Status", "Compute Target", "Job Type", "Job Output Type", "Environment");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));


                        // if no job ID given - display all training jobs
                        if (trainingJobName.Equals(""))
                        {
                            // get a listing of all training jobs the user has access to in Azure
                            List<Objects.AzureML.TrainingJob> trainingJobList = await Utilities.AzureML.TrainingJobUtils.getAllTrainingJobs(credential, subscriptionID, region, resourceGroup, workspace);

                            // iterate through the list of training jobs 
                            foreach (Objects.AzureML.TrainingJob trainingJob in trainingJobList)
                            {
                                Console.WriteLine("{0,40} | {1,10} | {2,20} | {3,10} | {4,20} | {5,20}", trainingJob.jobName, trainingJob.jobStatus, trainingJob.computeTarget, trainingJob.jobType, trainingJob.jobOutputType, trainingJob.environment);

                            }
                        }

                        // otherwise, display specific training job
                        else
                        {
                            // get a listing of all training jobs the user has access to in Azure
                            Objects.AzureML.TrainingJob trainingJob = await Utilities.AzureML.TrainingJobUtils.getSingleTrainingJob(credential, subscriptionID, region, resourceGroup, workspace, trainingJobName);

                            Console.WriteLine("{0,40} | {1,10} | {2,20} | {3,10} | {4,20} | {5,20}", trainingJob.jobName, trainingJob.jobStatus, trainingJob.computeTarget, trainingJob.jobType, trainingJob.jobOutputType, trainingJob.environment);

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
