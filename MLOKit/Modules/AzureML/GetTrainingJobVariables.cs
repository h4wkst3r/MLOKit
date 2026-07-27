using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace MLOKit.Modules.AzureML
{
    class GetTrainingJobVariables
    {


        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string trainingJobName)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("get-training-job-vars", credential, platform));

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
                Console.WriteLine("[*] INFO: Performing get-training-job-vars module for " + platform);
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


                        // get a listing all training jobs that have parameters or environment variables in use
                        List<Objects.AzureML.TrainingJob> trainingJobList = await Utilities.AzureML.TrainingJobUtils.getTrainingJobVars(credential, subscriptionID, region, resourceGroup, workspace);

                        Console.WriteLine("[*] INFO: Only displaying training jobs that have job variables present");
                        Console.WriteLine("");

                        // iterate through the list of discovered training jobs
                        foreach (Objects.AzureML.TrainingJob trainingJob in trainingJobList)
                        {

                            Console.WriteLine("Training Job Name - " + trainingJob.jobName);
                            Console.WriteLine("Training Job Variables - " + trainingJob.jobVars);
                            Console.WriteLine("");

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
