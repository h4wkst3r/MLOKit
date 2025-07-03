using System;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;

namespace MLOKit.Modules.SageMaker
{
    class DownloadModel
    {

        public static async Task execute(string credential, string platform, string modelID, RegionEndpoint endpoint)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("download-model", credential, platform));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing download-model module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                string[] splitCreds = credential.Split(';');
                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];


                AmazonSageMakerClient sagemakerClient = new AmazonSageMakerClient(accessKey, secretKey, endpoint);
                bool doesModelExist = false;


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


                        if (modelSummary.ModelName.ToLower().Equals(modelID.ToLower()))
                        {
                            doesModelExist = true;
                            Console.WriteLine("{0,50} | {1,20} | {2,50}", modelSummary.ModelName, creationTime,
                                modelSummary.ModelArn);
                        }
                    }

                    nextToken = response.NextToken;

                } while (!string.IsNullOrEmpty(nextToken));

                // if model exists, proceed
                if (doesModelExist)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Downloading model artifacts");
                    Console.WriteLine("");

                    // create the describe model request using the model name provided
                    DescribeModelRequest request = new DescribeModelRequest();
                    request.ModelName = modelID;
                    DescribeModelResponse describeResponse = await sagemakerClient.DescribeModelAsync(request);

                    Console.WriteLine("[*] INFO: Model artifacts location");
                    Console.WriteLine("");

                    string bucketName = "";
                    string modelDataURL = "";

                    // get the model data URL and parse out the bucket name
                    foreach (var modelContainer in describeResponse.Containers)
                    {
                        modelDataURL = modelContainer.ModelDataUrl;
                        Console.WriteLine(modelDataURL);
                        string[] splitModelDataURL = modelDataURL.Split('/');
                        bucketName = splitModelDataURL[2];

                    }

                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Checking access to S3 bucket with name: " + bucketName);
                    Console.WriteLine("");

                    AmazonS3Client s3Client = new AmazonS3Client(accessKey, secretKey, endpoint);
                    GetBucketVersioningResponse versionResponse = await s3Client.GetBucketVersioningAsync(bucketName);

                    // you have access to the bucket, proceed
                    if (versionResponse.HttpStatusCode.ToString().ToLower().Equals("ok"))
                    {

                        string fullPath = modelDataURL.Replace("s3://", "");
                        int lastIndexSlash = fullPath.LastIndexOf('/');
                        fullPath = fullPath.Substring(0, lastIndexSlash + 1);
                        string thePrefix = fullPath.Replace(bucketName + "/", "");

                        Console.WriteLine("[+] SUCCESS: You have access to S3 bucket with name: " + bucketName);
                        Console.WriteLine("");

                        // list all objects in the bucket
                        Console.WriteLine("[*] INFO: Listing all files in prefix of: " + thePrefix);
                        Console.WriteLine("");

                        // get listing of all objects in the bucket and prefix we need to download
                        List<String> objectsToDownload = new List<String>();
                        ListObjectsRequest listObjects = new ListObjectsRequest();
                        listObjects.BucketName = bucketName;
                        listObjects.Prefix = thePrefix;
                        ListObjectsResponse listObjectsResponse = await s3Client.ListObjectsAsync(listObjects);
                        foreach (var objectResponse in listObjectsResponse.S3Objects)
                        {
                            Console.WriteLine(objectResponse.Key);
                            objectsToDownload.Add(objectResponse.Key);
                            Console.WriteLine("");
                        }

                        // create random directory name in current working directory
                        string dirOut = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                        DirectoryInfo outputDir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + dirOut);

                        // go through and download each file from the S3 bucket and prefix - all the model artifacts
                        foreach (string fileToDownload in objectsToDownload)
                        {
                            Console.WriteLine("[*] INFO: Downloading file at: " + fileToDownload);
                            Console.WriteLine("");

                            // get just the file name so we maintain that when downloading
                            int lstIndex = modelDataURL.LastIndexOf('/');
                            string fileName = modelDataURL.Substring(lstIndex + 1, modelDataURL.Length - lstIndex - 1);

                            // download the file
                            GetObjectRequest getObject = new GetObjectRequest();
                            getObject.BucketName = bucketName;
                            getObject.Key = fileToDownload;
                            GetObjectResponse getObjectResponse = await s3Client.GetObjectAsync(getObject);
                            await getObjectResponse.WriteResponseStreamToFileAsync(outputDir.FullName + "\\" + fileName, false, CancellationToken.None);

                            Console.WriteLine("[+] SUCCESS: " + fileName + " written to: " + outputDir.FullName);
                            Console.WriteLine("");

                        }


                    }
                    // if you don't have access, print error and return
                    else
                    {
                        Console.WriteLine("[-] ERROR: You do not have access to the S3 bucket containing the model artifacts");
                        Console.WriteLine("");
                    }

                }
                // if model doesn't exist, print error and return
                else
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: Model provided does not exist. Please check model name again.");
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
