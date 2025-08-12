using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

namespace MLOKit.Modules.Palantir
{
    class DownloadDataset
    {
        public static async Task execute(string credential, string platform, string datasetID)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("download-dataset", credential, platform));

            // check for additional required arguments
            if (datasetID.Equals(""))
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: Missing one of required command arguments");
                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Use /dataset-id:[DATASET_RID] to specify the dataset to download");
                Console.WriteLine("");
                return;
            }

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Performing download-dataset module for " + platform);
            Console.WriteLine("");

            try
            {
                Console.WriteLine("[*] INFO: Downloading dataset with RID " + datasetID + " to the current working directory of " + Environment.CurrentDirectory);
                Console.WriteLine("");

                // first try to get dataset metadata
                Console.WriteLine("[*] INFO: Retrieving dataset metadata...");
                string metadataJson = await Utilities.Palantir.DatasetUtils.getDatasetDetails(credential, datasetID);

                string datasetName = "Unknown";
                if (!string.IsNullOrEmpty(metadataJson))
                {
                    // Try to extract dataset name from metadata for better file naming
                    try
                    {
                        var jsonDoc = Newtonsoft.Json.JsonConvert.DeserializeObject<dynamic>(metadataJson);
                        if (jsonDoc?.name != null)
                        {
                            datasetName = jsonDoc.name.ToString();
                        }
                    }
                    catch (Exception)
                    {
                        // If we can't parse, use default name
                    }
                }

                // download the dataset and store as binary content
                Console.WriteLine("[*] INFO: Downloading dataset content as CSV...");
                byte[] datasetContent = await Utilities.Palantir.DatasetUtils.downloadDataset(credential, datasetID);

                // if we got dataset back, then proceed
                if (datasetContent != null && datasetContent.Length > 0)
                {
                    // create random directory name using the standard MLOKit pattern
                    string dirOut = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                    DirectoryInfo outputDir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + dirOut);

                    // write dataset file to the directory using dataset name where possible
                    string sanitizedName = System.Text.RegularExpressions.Regex.Replace(datasetName, @"[<>:""/\\|?*]", "_");
                    string csvFileName = sanitizedName != "Unknown" ? sanitizedName + ".csv" : "dataset.csv";
                    string datasetFileName = Path.Combine(outputDir.FullName, csvFileName);
                    File.WriteAllBytes(datasetFileName, datasetContent);

                    Console.WriteLine("[+] SUCCESS: Dataset written to: " + datasetFileName);
                    Console.WriteLine("[*] INFO: File size: " + (datasetContent.Length / 1024.0).ToString("F2") + " KB");
                    Console.WriteLine("[*] INFO: Dataset RID: " + datasetID);
                    Console.WriteLine("");

                    // also save metadata if we have it
                    if (!string.IsNullOrEmpty(metadataJson))
                    {
                        string metadataFileName = Path.Combine(outputDir.FullName, "metadata.json");
                        File.WriteAllText(metadataFileName, metadataJson);
                        Console.WriteLine("[+] SUCCESS: Dataset metadata written to: " + metadataFileName);
                        Console.WriteLine("");
                    }
                }
                else
                {
                    Console.WriteLine("[-] ERROR: Failed to download dataset content. The dataset may be empty or access may be restricted.");
                    Console.WriteLine("");

                    // If CSV download failed, try to save metadata at least
                    if (!string.IsNullOrEmpty(metadataJson))
                    {
                        Console.WriteLine("[*] INFO: Saving dataset metadata as fallback...");
                        string dirOut = "MLOKit-" + Utilities.FileUtils.generateRandomName();
                        DirectoryInfo outputDir = Directory.CreateDirectory(Environment.CurrentDirectory + "\\" + dirOut);
                        
                        string metadataFileName = Path.Combine(outputDir.FullName, "metadata.json");
                        File.WriteAllText(metadataFileName, metadataJson);
                        Console.WriteLine("[+] SUCCESS: Dataset metadata written to: " + metadataFileName);
                        Console.WriteLine("[*] INFO: Dataset RID: " + datasetID);
                        Console.WriteLine("");
                    }
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
