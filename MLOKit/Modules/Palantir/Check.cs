using System;
using System.Threading.Tasks;

namespace MLOKit.Modules.Palantir
{
    class Check
    {
        public static async Task execute(string credential, string platform)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("check", credential, platform));

            try
            {
                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing check module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                string[] splitCreds = credential.Split(';');
                string token = splitCreds[0];
                string tenant = splitCreds[1];
               
                // if creds valid, then provide message
                if (await Utilities.Palantir.WebUtils.credsValid(token, $"https://{tenant}/api/v1/ontologies"))
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
