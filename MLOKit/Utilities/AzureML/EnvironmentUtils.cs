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

namespace MLOKit.Utilities.AzureML
{
    class EnvironmentUtils
    {

        // get listing of all environments
        public static async Task<List<Objects.AzureML.Environment>> getAllEnvironments(
     string credentials,
     string subscriptionID,
     string region,          // unused; kept for signature compatibility
     string resourceGroup,
     string workspace)
        {
            var environmentList = new List<Objects.AzureML.Environment>();

            string nextLink =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/environments?api-version=2025-06-01&$top=100";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                while (!string.IsNullOrEmpty(nextLink))
                {
                    var webRequest = (HttpWebRequest)WebRequest.Create(nextLink);
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
                        var root = Newtonsoft.Json.Linq.JObject.Parse(content);

                        var items = root["value"] as Newtonsoft.Json.Linq.JArray;
                        if (items != null)
                        {
                            foreach (var item in items)
                            {
                                var name = item["name"]?.ToString() ?? string.Empty;
                                var props = item["properties"] as Newtonsoft.Json.Linq.JObject;
                                var description = props?["description"]?.ToString() ?? string.Empty;
                                var latestVersion = props?["latestVersion"]?.ToString() ?? string.Empty;

                                // Only add well-formed entries & avoid dups
                                if (!string.IsNullOrEmpty(name) &&
                                    !string.IsNullOrEmpty(latestVersion))
                                {
                                    // get details of the environment by making REST API call to the specific version
                                    Objects.AzureML.Environment theEnv = await getEnvironmentBuildInfo(credentials, subscriptionID, region, resourceGroup, workspace, name, latestVersion);

                                    if (theEnv != null)
                                    {
                                        environmentList.Add(
                                        new Objects.AzureML.Environment(
                                            name,
                                            theEnv.description,
                                            latestVersion, theEnv.buildContextUri, theEnv.buildFilePath));
                                    }

                                }
                            }
                        }

                        // Follow pagination
                        nextLink = root["nextLink"]?.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }

            return environmentList;
        }


        // get the build info for an environment
        public static async Task<Objects.AzureML.Environment> getEnvironmentBuildInfo(
     string credentials,
     string subscriptionID,
     string region,          // unused; kept for signature compatibility
     string resourceGroup,
     string workspace, string environmentName, string version)
        {
            Objects.AzureML.Environment environment = null;

            string baseURL =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/environments/{environmentName}/versions/{version}?api-version=2025-06-01&$top=100";

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
                    var root = Newtonsoft.Json.Linq.JObject.Parse(content);


                    var name = environmentName;
                    var props = root["properties"] as Newtonsoft.Json.Linq.JObject;
                    var description = props?["description"]?.ToString() ?? string.Empty;
                    var latestVersion = version;
                    var buildInfo = props["build"] as Newtonsoft.Json.Linq.JObject;
                    var buildUri = buildInfo?["contextUri"]?.ToString() ?? string.Empty;
                    var buildFilePath = buildInfo?["dockerfilePath"]?.ToString() ?? string.Empty;

                    // Only add well-formed entries & avoid dups
                    if (!string.IsNullOrEmpty(name) &&
                        !string.IsNullOrEmpty(latestVersion))
                    {
                        environment =
                            new Objects.AzureML.Environment(
                                name,
                                description,
                                latestVersion, buildUri, buildFilePath);
                    }



                }
            }
            catch (Exception ex)
            {
            }

            return environment;
        }



    }
}
