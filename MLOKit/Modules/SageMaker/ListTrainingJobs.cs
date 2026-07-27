using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using Amazon;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;

namespace MLOKit.Modules.SageMaker
{
    class ListTrainingJobs
    {

        public static async Task execute(string credential, string platform, RegionEndpoint endpoint, string trainingJobName)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("list-training-jobs", credential, platform));

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

                string[] splitCreds = credential.Split(';');
                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];

                AmazonSageMakerClient sagemakerClient = new AmazonSageMakerClient(accessKey, secretKey, endpoint);

                string nextToken = null;
                do
                {
                    var request = new ListTrainingJobsRequest
                    {
                        MaxResults = 100, // Optional: control page size (max 100)
                        NextToken = nextToken
                    };

                    ListTrainingJobsResponse response = await sagemakerClient.ListTrainingJobsAsync(request);

                    // valid credentials
                    if (response.HttpStatusCode.ToString().ToLower().Equals("ok"))
                    {
                        Console.WriteLine("[+] SUCCESS: Credentials are valid");
                        Console.WriteLine("");

                        // create table header
                        string tableHeader = string.Format("{0,80} | {1,20}", "Job Name", "Job Status");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                    }

                    foreach (var trainingJobSummary in response.TrainingJobSummaries)
                    {

                        Console.WriteLine("{0,80} | {1,20}", trainingJobSummary.TrainingJobName, trainingJobSummary.TrainingJobStatus);
                    }

                    nextToken = response.NextToken;

                } while (!string.IsNullOrEmpty(nextToken));




            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }


        }

    }
}
