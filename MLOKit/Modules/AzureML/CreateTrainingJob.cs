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
    class CreateTrainingJob
    {

        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string configFilePath)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("create-training-job", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals("") || configFilePath.Equals(""))
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
                Console.WriteLine("[*] INFO: Performing create-training-job module for " + platform);
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

                        Console.WriteLine("[*] INFO: Validating config file supplied exists");
                        Console.WriteLine("");

                        if (File.Exists(configFilePath))
                        {
                            Console.WriteLine("[+] SUCCESS: Configuration file supplied exists");
                            Console.WriteLine("");

                            Dictionary<string,string> parsedConfig = Utilities.AzureML.TrainingJobUtils.parseAzureMLJobConfig(configFilePath);

                            /**
                            foreach (var kv in parsedConfig)
                            {
                                Console.WriteLine($"{kv.Key}: {kv.Value}");
                            }
                            **/

                            Console.WriteLine("[*] INFO: Creating training job");
                            Console.WriteLine("");

                            bool createdJob = await Utilities.AzureML.TrainingJobUtils.createTrainingJob(credential, subscriptionID, region, resourceGroup, workspace, parsedConfig);

                            if (createdJob)
                            {
                                Console.WriteLine("");
                                Console.WriteLine("[+] SUCCESS:Training job created successfully");
                                Console.WriteLine("");
                            }

                            else
                            {
                                Console.WriteLine("");
                                Console.WriteLine("[-] ERROR: Training job creation failed");
                                Console.WriteLine("");
                            }


                        }
                        else
                        {

                            Console.WriteLine("[-] ERROR: Configuration file supplied does NOT exist. Double check your file path.");
                            Console.WriteLine("");
                            return;


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
