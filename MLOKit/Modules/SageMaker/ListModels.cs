using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using Amazon;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;

namespace MLOKit.Modules.SageMaker
{
    class ListModels
    {

        public static async Task execute(string credential, string platform, RegionEndpoint endpoint)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("list-models", credential, platform));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing list-models module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                string[] splitCreds = credential.Split(';');
                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];

                AmazonSageMakerClient sagemakerClient = new AmazonSageMakerClient(accessKey, secretKey, endpoint);

                string nextToken = null;
                do
                {
                    var request = new ListModelsRequest
                    {
                        MaxResults = 100, // Optional: control page size (max 100)
                        NextToken = nextToken
                    };

                    ListModelsResponse response = await sagemakerClient.ListModelsAsync(request);

                    // valid credentials
                    if (response.HttpStatusCode.ToString().ToLower().Equals("ok"))
                    {
                        Console.WriteLine("[+] SUCCESS: Credentials are valid");
                        Console.WriteLine("");

                        // create table header
                        string tableHeader = string.Format("{0,50} | {1,20} | {2,50}", "Model Name", "Creation Date", "Model ARN");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                    }


                    foreach (var modelSummary in response.Models)
                    {
                        string creationTime = modelSummary.CreationTime.ToShortDateString();
                        Console.WriteLine("{0,50} | {1,20} | {2,50}", modelSummary.ModelName, creationTime, modelSummary.ModelArn);
                    }

                    nextToken = response.NextToken;

                } while (!string.IsNullOrEmpty(nextToken));



    


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
