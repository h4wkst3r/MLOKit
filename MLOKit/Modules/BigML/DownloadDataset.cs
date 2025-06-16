using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.IO;

namespace MLOKit.Modules.BigML
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
                return;
            }

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Performing download-dataset module for " + platform);
            Console.WriteLine("");


            // check if credentials provided are valid
            Console.WriteLine("[*] INFO: Checking credentials provided");
            Console.WriteLine("");

            // if creds valid, then provide message and continue
            if (await Utilities.BigML.WebUtils.credsValid(credential, "https://bigml.io"))
            {
                Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                Console.WriteLine("");

                try
                {


                    Console.WriteLine("[*] INFO: Downloading dataset with ID " + datasetID + " to the current working directory of " + Environment.CurrentDirectory);
                    Console.WriteLine("");

                    // download the dataset and store in base64
                    byte [] datasetContent = await Utilities.BigML.DatasetUtils.downloadDataset(credential, datasetID);

                    // if we got dataset back, then proceed
                    if (datasetContent != null)
                    {

                        // create random file name
                        Random rd = new Random();
                        const string allowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
                        char[] chars = new char[8];

                        for (int i = 0; i < 8; i++)
                        {
                            chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
                        }
                        string fileName = new string(chars);
                        fileName = "MLOKit-" + fileName;

                        // write dataset file to disk
                        File.WriteAllBytes(fileName, datasetContent);


                        Console.WriteLine("[+] SUCCESS: Dataset written to: " + Environment.CurrentDirectory + "\\" + fileName);
                        Console.WriteLine("");

                        Console.WriteLine("");
                    }



                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: " + ex.Message);
                    Console.WriteLine("");
                    return;
                }


            }

            // if creds not valid, display message and return
            else
            {
                Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
                Console.WriteLine("");
                return;
            }




        }
    }
}
