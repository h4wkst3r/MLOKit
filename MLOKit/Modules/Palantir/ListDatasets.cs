using System;
using System.Net;
using System.Threading.Tasks;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Collections.Generic;

namespace MLOKit.Modules.Palantir
{
    class ListDatasets
    {
        public static async Task execute(string credential, string platform)
        {
            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            Console.WriteLine("");
            Console.WriteLine("[*] INFO: Performing list-datasets module for " + platform);
            Console.WriteLine("");

            // Generate module header
            Console.WriteLine(Utilities.ArgUtils.GenerateHeader("list-datasets", credential, platform));

            try
            {
                // create table header
                string tableHeader = string.Format("{0,40} | {1,15} | {2,25} | {3,50}", "Name", "Type", "Creation Date", "Dataset RID");
                Console.WriteLine(tableHeader);
                Console.WriteLine(new String('-', tableHeader.Length));

                // get a listing of all datasets the user has access to in Palantir
                List<Objects.Palantir.Dataset> datasetList = await Utilities.Palantir.DatasetUtils.getAllDatasets(credential);

                // iterate through the list of datasets 
                foreach (Objects.Palantir.Dataset dataset in datasetList)
                {
                    // truncate long names for display
                    string displayName = dataset.datasetName.Length > 38 ? dataset.datasetName.Substring(0, 35) + "..." : dataset.datasetName;
                    string displayDate = dataset.dateCreated.Length > 23 ? dataset.dateCreated.Substring(0, 23) : dataset.dateCreated;

                    Console.WriteLine("{0,40} | {1,15} | {2,25} | {3,50}", displayName, dataset.type, displayDate, dataset.datasetRID);
                }

                Console.WriteLine("");
                Console.WriteLine("[*] INFO: Found " + datasetList.Count + " dataset(s)");
                Console.WriteLine("");
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
