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
    class GetTrainingJobVariables
    {
        public static async Task execute(string credential, string platform, RegionEndpoint endpoint, string trainingJobName)
        {
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("get-training-job-vars", credential, platform));

            ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };

            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            try
            {
                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing get-training-job-vars module for " + platform);
                Console.WriteLine("");

                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                string[] splitCreds = credential.Split(';');

                if (splitCreds.Length < 2)
                {
                    Console.WriteLine("[-] ERROR: Invalid AWS credential format. Expected ACCESS_KEY;SECRET_KEY");
                    Console.WriteLine("");
                    return;
                }

                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];

                AmazonSageMakerClient sagemakerClient = new AmazonSageMakerClient(accessKey, secretKey, endpoint);

                Console.WriteLine("[+] SUCCESS: Credentials are valid");
                Console.WriteLine("");

                if (!string.IsNullOrEmpty(trainingJobName))
                {
                    Console.WriteLine("[*] INFO: Getting variables for training job: " + trainingJobName);
                    Console.WriteLine("");

                    DescribeTrainingJobRequest describeJobRequest = new DescribeTrainingJobRequest();
                    describeJobRequest.TrainingJobName = trainingJobName;

                    DescribeTrainingJobResponse describeJobResponse =
                        await sagemakerClient.DescribeTrainingJobAsync(describeJobRequest);

                    DisplayTrainingJobVariables(trainingJobName, describeJobResponse, true);

                    return;
                }

                Console.WriteLine("[*] INFO: Only displaying training jobs that have environment variables or hyperparameters present");
                Console.WriteLine("");

                string nextToken = null;

                do
                {
                    ListTrainingJobsRequest request = new ListTrainingJobsRequest();
                    request.MaxResults = 100;
                    request.NextToken = nextToken;
                    request.SortBy = SortBy.CreationTime;
                    request.SortOrder = SortOrder.Descending;

                    ListTrainingJobsResponse response = await sagemakerClient.ListTrainingJobsAsync(request);

                    foreach (TrainingJobSummary trainingJobSummary in response.TrainingJobSummaries)
                    {
                        DescribeTrainingJobRequest describeJobRequest = new DescribeTrainingJobRequest();
                        describeJobRequest.TrainingJobName = trainingJobSummary.TrainingJobName;

                        DescribeTrainingJobResponse describeJobResponse =
                            await sagemakerClient.DescribeTrainingJobAsync(describeJobRequest);

                        DisplayTrainingJobVariables(trainingJobSummary.TrainingJobName, describeJobResponse, false);
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

        private static void DisplayTrainingJobVariables(string trainingJobName, DescribeTrainingJobResponse describeJobResponse, bool showEmpty)
        {
            bool hasEnvironment =
                describeJobResponse.Environment != null &&
                describeJobResponse.Environment.Count > 0;

            bool hasHyperParameters =
                describeJobResponse.HyperParameters != null &&
                describeJobResponse.HyperParameters.Count > 0;

            if (!hasEnvironment && !hasHyperParameters && !showEmpty)
            {
                return;
            }

            Console.WriteLine("Training Job Name - " + trainingJobName);

            if (hasEnvironment)
            {
                Console.WriteLine("Training Job Environment Variables -");

                foreach (var envVar in describeJobResponse.Environment)
                {
                    Console.WriteLine(envVar.Key + " = " + envVar.Value);
                }
            }
            else
            {
                Console.WriteLine("Training Job Environment Variables - NONE");
            }

            if (hasHyperParameters)
            {
                Console.WriteLine("Training Job HyperParameters -");

                foreach (var hyperParam in describeJobResponse.HyperParameters)
                {
                    Console.WriteLine(hyperParam.Key + " = " + hyperParam.Value);
                }
            }
            else
            {
                Console.WriteLine("Training Job HyperParameters - NONE");
            }

            Console.WriteLine("");
        }
    }
}