using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

namespace MLOKit.Modules.Palantir
{
    class UploadDataset
    {
        public static async Task execute(string credential, string platform, string datasetName, string sourceDir)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("upload-dataset", credential, platform));

            // check for additional required arguments
            if (datasetName.Equals("") || sourceDir.Equals(""))
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Missing one of required command arguments");
                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Use /dataset-name:[DATASET_NAME] to specify the dataset name");
                Console.WriteLine("[*] INFO: Use /source-dir:[LOCAL_FILE_PATH] to specify the local file to upload");
                Console.WriteLine("");
                return;
            }

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Performing upload-dataset module for " + platform);
            Console.WriteLine("");

            try
            {
                // Check if source file exists
                if (!File.Exists(sourceDir))
                {
                    Console.WriteLine("[-] ERROR: Source file does not exist: " + sourceDir);
                    Console.WriteLine("");
                    return;
                }

                Console.WriteLine("[*] INFO: Uploading dataset file: " + sourceDir);
                Console.WriteLine("[*] INFO: Dataset name: " + datasetName);
                Console.WriteLine("");

                // Read the file content
                byte[] fileContent = File.ReadAllBytes(sourceDir);
                Console.WriteLine("[*] INFO: File size: " + (fileContent.Length / 1024.0).ToString("F2") + " KB");
                Console.WriteLine("");

                // Upload the dataset
                string datasetRid = await Utilities.Palantir.DatasetUtils.uploadDataset(credential, datasetName, fileContent, sourceDir);

                if (!string.IsNullOrEmpty(datasetRid))
                {
                    Console.WriteLine("[+] SUCCESS: Dataset uploaded successfully with RID: " + datasetRid);
                    Console.WriteLine("");

                    // Extract tenant from credentials for URL construction
                    string[] splitCreds = credential.Split(';');
                    if (splitCreds.Length >= 2)
                    {
                        string tenant = splitCreds[1];
                        Console.WriteLine("[*] INFO: Dataset available at: https://" + tenant + "/workspace/dataset/" + datasetRid);
                        Console.WriteLine("");
                    }
                }
                else
                {
                    Console.WriteLine("[-] ERROR: Failed to upload dataset. The upload may have been rejected or there may be insufficient permissions.");
                    Console.WriteLine("");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }
        }
    }
}
