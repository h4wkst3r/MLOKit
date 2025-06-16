using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace MLOKit.Modules.MLFlow
{
    class ListModels
    {

        public static async Task execute(string credential, string platform, string url)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("list-models", credential, platform));


            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Performing list-models module for " + platform);
            Console.WriteLine("");


            // check if credentials provided are valid
            Console.WriteLine("[*] INFO: Checking credentials provided");
            Console.WriteLine("");

            // if creds valid, then provide message and continue
            if (await Utilities.MLFlow.WebUtils.credsValid(credential, url))
            {
                Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                Console.WriteLine("");

                try
                {

                    // create table header
                    string tableHeader = string.Format("{0,30} | {1,7} | {2,10} | {3,30} | {4,40}", "Name", "Version", "Status", "Description", "Artifact Location");
                    Console.WriteLine(tableHeader);
                    Console.WriteLine(new String('-', tableHeader.Length));

                    // get a listing of all models the user has access to in MLFlow
                    List<Objects.MLFlow.Model> modelList = await Utilities.MLFlow.ModelUtils.getAllModels(credential, url);

                    // iterate through the list of models
                    foreach (Objects.MLFlow.Model modelSet in modelList)
                    {
                        Console.WriteLine("{0,30} | {1,7} | {2,10} | {3,30} | {4,40}", modelSet.modelName, modelSet.modelVersion, modelSet.status, modelSet.description,modelSet.artifactLocation);

                    }

                    Console.WriteLine("");



                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[-] ERROR: " + ex.Message);
                    Console.WriteLine("");
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
