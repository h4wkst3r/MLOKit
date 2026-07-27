using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;
using System.Threading;

namespace MLOKit.Modules.AzureML
{
    class AddSSHKey
    {
        public static async Task execute(string credential, string platform, string subscriptionID, string region, string resourceGroup, string workspace, string computeID, string sshKey)
        {
            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("add-ssh-key", credential, platform));

            // check for additional required arguments
            if (subscriptionID.Equals("") || region.Equals("") || resourceGroup.Equals("") || workspace.Equals("") || computeID.Equals("") || sshKey.Equals(""))
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
                Console.WriteLine("[*] INFO: Performing add-ssh-key module for " + platform);
                Console.WriteLine("");

                // check if credentials provided are valid
                Console.WriteLine("[*] INFO: Checking credentials provided");
                Console.WriteLine("");

                // if creds valid, then provide message
                if (await Utilities.AzureML.WebUtils.credsValid(credential, "https://management.azure.com/subscriptions?api-version=2022-12-01"))
                {
                    Console.WriteLine("[+] SUCCESS: Credentials provided are VALID.");
                    Console.WriteLine("");

                    Console.WriteLine("[*] INFO: Listing compute instance where SSH key will be added");
                    Console.WriteLine("");


                    try
                    {


                        // create table header
                        string tableHeader = string.Format("{0,30} | {1,10} | {2,15} | {3,20} | {4,10} | {5,30}", "Name", "State", "Public IP", "SSH Username", "SSH Port", "Public SSH Key");
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));

                        // get the compute instance
                        Objects.AzureML.Compute computeInstance = await Utilities.AzureML.ComputeUtils.getSingleComputeInstance(credential, subscriptionID, region, resourceGroup, workspace,computeID);
                        string pubKey = computeInstance.sshPubKey.Substring(computeInstance.sshPubKey.Length - 30, 30);
                        Console.WriteLine("{0,30} | {1,10} | {2,15} | {3,20} | {4,10} | {5,30}", computeInstance.computeName, computeInstance.state, computeInstance.publicIP, computeInstance.sshUsername, computeInstance.sshPort, pubKey);

                        // add SSH key to the compute instance
                        Console.WriteLine("");
                        Console.WriteLine("[*] INFO: Adding SSH Key to compute instance");
                        Console.WriteLine("");
                        bool addedSSHKey = await Utilities.AzureML.ComputeUtils.addSSHKey(credential, subscriptionID, region, resourceGroup, workspace, computeID, sshKey);


                        Console.WriteLine("[*] INFO: Restarting compute instance");
                        Console.WriteLine("");

                        // if compute instance is already stopped, then start it
                        if (computeInstance.state.Equals("Stopped"))
                        {
                            Console.WriteLine("[*] INFO: Starting compute instance");
                            Console.WriteLine("");
                            bool started = await Utilities.AzureML.ComputeUtils.startCompute(credential, subscriptionID, region, resourceGroup, workspace, computeID);

                        }

                        // otherwise, stop and then start the instance
                        else
                        {
                            Console.WriteLine("[*] INFO: Stopping compute instance");
                            Console.WriteLine("");
                            bool stopped = await Utilities.AzureML.ComputeUtils.stopCompute(credential, subscriptionID, region, resourceGroup, workspace, computeID);

                            if (!stopped)
                            {
                                Console.WriteLine("[-] ERROR: Stop request failed. Not attempting restart.");
                                return;
                            }


                            Console.WriteLine("[*] INFO: Sleeping for 3 minutes to allow time for compute instance to stop");
                            Console.WriteLine("");
                            Thread.Sleep(180000);

                            Console.WriteLine("[*] INFO: Starting compute instance");
                            Console.WriteLine("");
                            bool started = await Utilities.AzureML.ComputeUtils.startCompute(credential, subscriptionID, region, resourceGroup, workspace, computeID);

                        }


                        // sleep 3 minutes seconds to allow change to take affect
                        Console.WriteLine("[*] INFO: Sleeping for 3 minutes to allow time for compute instance to restart");
                        Console.WriteLine("");
                        Thread.Sleep(180000);

                        bool restarted = false;


                        // display the compute instance again after SSH key has been added
                        Console.WriteLine(tableHeader);
                        Console.WriteLine(new String('-', tableHeader.Length));
                        Objects.AzureML.Compute computeInstanceThree = await Utilities.AzureML.ComputeUtils.getSingleComputeInstance(credential, subscriptionID, region, resourceGroup, workspace, computeID);
                        string pubKeyThree = computeInstanceThree.sshPubKey.Substring(computeInstanceThree.sshPubKey.Length - 30, 30);
                        Console.WriteLine("{0,30} | {1,10} | {2,15} | {3,20} | {4,10} | {5,30}", computeInstanceThree.computeName, computeInstanceThree.state, computeInstanceThree.publicIP, computeInstanceThree.sshUsername, computeInstanceThree.sshPort, pubKeyThree);

                        // if compute is running, set restarted bool to true
                        if (computeInstanceThree.state.Equals("Running"))
                        {
                            restarted = true;
                        }

                        // display whether compute was restarted or not
                        if (restarted)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("[+] SUCCESS: Compute instance successfully restarted");
                            Console.WriteLine("");
                        }
                        else
                        {
                            Console.WriteLine("");
                            Console.WriteLine("[-] ERROR: Compute instance was NOT restarted");
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
