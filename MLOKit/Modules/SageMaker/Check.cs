using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Amazon;
using Amazon.Runtime.CredentialManagement;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;
using Amazon.Util;

namespace MLOKit.Modules.SageMaker
{
    class Check
    {

        public static async Task execute(string credential, string platform, RegionEndpoint endpoint)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("check", credential, platform));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing check module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                string[] splitCreds = credential.Split(';');
                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];

                

                AmazonSageMakerClient sagemakerClient = new AmazonSageMakerClient(accessKey, secretKey,endpoint);
                ListModelsResponse response = await sagemakerClient.ListModelsAsync(new ListModelsRequest());

                // valid credentials
                if (response.HttpStatusCode.ToString().ToLower().Equals("ok")){
                    Console.WriteLine("[+] SUCCESS: Credentials are valid");
                    Console.WriteLine("");
                }

                





            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Please check your credentials and try again");
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }

        }


    }
}
