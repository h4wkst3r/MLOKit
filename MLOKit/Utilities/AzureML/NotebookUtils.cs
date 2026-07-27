using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.Collections;
using System.Linq;
using System.Net.Http;

namespace MLOKit.Utilities.AzureML
{
    class NotebookUtils
    {

        // get listing of all kernels
        public static async Task<List<Objects.AzureML.Kernel>> getAllKernels(
     string credentials,
     string subscriptionID,
     string region,          // unused; kept for signature compatibility
     string resourceGroup,
     string workspace, Uri baseAPI)
        {
            var kernelList = new List<Objects.AzureML.Kernel>();
            string rootURL = baseAPI.ToString();
            string baseURL = $"{rootURL}api/kernels";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var webRequest = (HttpWebRequest)WebRequest.Create(baseURL);
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                webRequest.Headers.Add("Authorization", "Bearer " + credentials);

                using (var response = (HttpWebResponse)await webRequest.GetResponseAsync())
                using (var stream = response.GetResponseStream())
                using (var reader = new StreamReader(stream))
                {
                    var content = await reader.ReadToEndAsync();

                    // Parse this page
                    var items = Newtonsoft.Json.Linq.JArray.Parse(content);

                    if (items != null)
                    {
                        foreach (var item in items)
                        {
                            var id = item["id"]?.ToString() ?? string.Empty;
                            var name = item["name"]?.ToString() ?? string.Empty;
                            var executionState = item["execution_state"]?.ToString() ?? string.Empty;

                            // Only add well-formed entries & avoid dups
                            if (!string.IsNullOrEmpty(id) &&
                                !string.IsNullOrEmpty(name) &&
                                !string.IsNullOrEmpty(executionState))
                            {
                                kernelList.Add(
                                    new Objects.AzureML.Kernel(
                                        id,
                                        name, executionState));
                            }

                        }

                    }

                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }
            return kernelList;
        }



        public static string getBasePath(Uri notebookInstanceUri, HttpClientHandler handler, HttpClient http)
        {
            var basePath = "/";
            var path = notebookInstanceUri.AbsolutePath;
            var marker = "/jupyter/default";
            if (path.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                basePath = marker + "/";

            return basePath;
        }

        public static Uri getBaseAPI(Uri notebookInstanceUri, string basePath)
        {

            var origin = $"{notebookInstanceUri.Scheme}://{notebookInstanceUri.Host}{(notebookInstanceUri.IsDefaultPort ? "" : ":" + notebookInstanceUri.Port)}";
            var baseApi = new Uri($"{origin}{basePath}");

            return baseApi;
        }



        public static string GetCookieValue(string cookieHeader, string name)
        {
            foreach (var part in cookieHeader.Split(';'))
            {
                var kv = part.Trim().Split('=');
                if (kv.Length == 2 && kv[0].Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return WebUtility.UrlDecode(kv[1]);
            }
            return null;
        }

    }
}
