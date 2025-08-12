using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace MLOKit.Modules.Palantir
{
    class Check
    {
        public static async Task execute(string credential, string platform)
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

                // if creds valid, then provide message
                if (await Utilities.Palantir.WebUtils.credsValid(credential))
                {
                    Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                    Console.WriteLine("");

                    // Parse credentials to show tenant info
                    string[] splitCreds = credential.Split(';');
                    if (splitCreds.Length >= 2)
                    {
                        Console.WriteLine("[*] INFO: Connected to tenant: " + splitCreds[1]);
                        if (splitCreds.Length >= 3 && !string.IsNullOrEmpty(splitCreds[2]))
                        {
                            Console.WriteLine("[*] INFO: App RID: " + splitCreds[2]);
                        }
                        else
                        {
                            Console.WriteLine("[*] INFO: No App RID provided - will use Spaces API for dataset discovery");
                        }
                        Console.WriteLine("");
                    }
                }
                // if creds not valid, display message
                else
                {
                    Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
                    Console.WriteLine("");
                    Console.WriteLine("[*] INFO: Expected format: token;tenant[;apprid]");
                    Console.WriteLine("[*] INFO: App RID is optional - if not provided, Spaces API will be used for discovery");
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
