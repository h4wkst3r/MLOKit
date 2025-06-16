using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace MLOKit.Modules.MLFlow
{
    class Check
    {

        public static async Task execute(string credential, string platform, string url)
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

                if(await Utilities.MLFlow.WebUtils.credsValid(credential, url))
                {
                    Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                    Console.WriteLine("");

                }

                // if creds not valid, display message
                else
                {
                    Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
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
