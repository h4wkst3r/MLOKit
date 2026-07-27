using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace MLOKit.Modules.AzureML
{
    class ListCompute
    {

        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("list-compute", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals(""))
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

            try
            {

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Performing list-compute module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                // if creds valid, then provide message
                if (await Utilities.AzureML.WebUtils.credsValid(credential, "https://management.azure.com/subscriptions?api-version=2022-12-01"))
                {
                    Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                    Console.WriteLine("");


                    try
                    {


                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,10} | {2,15} | {3,20} | {4,10} | {5,30}", "Name", "State", "Public IP", "SSH Username", "SSH Port", "Public SSH Key");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get a listing of all compute the user has access to in Azure
                        List<Objects.AzureML.Compute> computeList = await Utilities.AzureML.ComputeUtils.getAllCompute(credential, subscriptionID, region, resourceGroup, workspace);


                        // iterate through the list of compute 
                        foreach (Objects.AzureML.Compute compute in computeList)
                        {
                            string sshKey = compute.sshPubKey.Substring(compute.sshPubKey.Length - 30, 30);
                            Console.WriteLine("{0,30} | {1,10} | {2,15} | {3,20} | {4,10} | {5,30}", compute.computeName, compute.state, compute.publicIP, compute.sshUsername, compute.sshPort, sshKey);

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

                // if creds not valid, display message
                else
                {
                    Console.WriteLine("[-] ERROR: Credentials provided are INVALID. Check the credentials again.");
                    Console.WriteLine("");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }


        }

    }
}
