using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MLOKit.Utilities.AzureML
{
    class ComputeUtils
    {
        // get listing of all compute instances
        public static async Task<List<Objects.AzureML.Compute>> getAllCompute(string credentials, string subscriptionID, string region, string resourceGroup, string workspace)
        {
            List<Objects.AzureML.Compute> computeList = new List<Objects.AzureML.Compute>();
            string baseUrl = $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/computes?api-version=2025-06-01";

            string nextLink = baseUrl;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(nextLink);
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                webRequest.Headers.Add("Authorization", "Bearer " + credentials);

                // get web response and status code
                HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                string content;
                var reader = new StreamReader(myWebResponse.GetResponseStream());
                content = reader.ReadToEnd();

                // parse the JSON output and display results
                JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                string computeName = "";
                string publicIP = "";
                string sshUserName = "";
                string sshPubKey = "";
                string sshPort = "";
                string state = "";
                string propName = "";
                bool containsJupyterEnv = false;
                string jupyterEndpoint = "";

                // read the json results
                while (jsonResult.Read())
                {

                    switch (jsonResult.TokenType.ToString())
                    {
                        case "StartObject":
                            break;
                        case "EndObject":

                            // if compute already doesn't exist in our list, add it
                            if (!doesComputeAlreadyExistInList(computeName, computeList) && computeName != "" && publicIP != "" && sshUserName != "" && sshPort != "" && sshPubKey != "" && state != "")
                            {
                                computeList.Add(new Objects.AzureML.Compute(computeName, publicIP, sshUserName, sshPort, sshPubKey, state, containsJupyterEnv, jupyterEndpoint));
                                computeName = "";
                                publicIP = "";
                                sshUserName = "";
                                sshPubKey = "";
                                sshPort = "";
                                state = "";
                                containsJupyterEnv = false;
                                jupyterEndpoint = "";
                            }
                            break;
                        case "StartArray":
                            break;
                        case "EndArray":
                            break;
                        case "PropertyName":
                            propName = jsonResult.Value.ToString();
                            break;
                        case "String":
                            if (propName.ToLower().Equals("name"))
                            {
                                computeName = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("publicipaddress"))
                            {
                                publicIP = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("adminusername"))
                            {
                                sshUserName = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("adminpublickey"))
                            {
                                sshPubKey = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("state"))
                            {
                                state = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("displayname"))
                            {
                                if (jsonResult.Value.ToString().Equals("jupyter"))
                                {
                                    containsJupyterEnv = true;
                                }

                            }
                            if (propName.ToLower().Equals("endpointuri") && containsJupyterEnv)
                            {
                                jupyterEndpoint = jsonResult.Value.ToString();
                            }

                            break;
                        case "Date":
                            break;
                        case "Boolean":
                            break;
                        case "Integer":
                            if (propName.ToLower().Equals("sshport"))
                            {
                                sshPort = jsonResult.Value.ToString();
                            }
                            break;
                        default:
                            break;

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }

            return computeList;
        }



        // get listing of all compute instances
        public static async Task<List<Objects.AzureML.Compute>> getAllNotebookCompute(string credentials, string subscriptionID, string region, string resourceGroup, string workspace)
        {
            List<Objects.AzureML.Compute> computeList = new List<Objects.AzureML.Compute>();
            string baseUrl = $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/computes?api-version=2025-06-01";

            string nextLink = baseUrl;

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(nextLink);
                webRequest.Method = "GET";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                webRequest.Headers.Add("Authorization", "Bearer " + credentials);

                // get web response and status code
                HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                string content;
                var reader = new StreamReader(myWebResponse.GetResponseStream());
                content = reader.ReadToEnd();

                // parse the JSON output and display results
                JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                string computeName = "";
                string publicIP = "";
                string sshUserName = "";
                string sshPubKey = "";
                string sshPort = "";
                string state = "";
                string propName = "";
                bool containsJupyterEnv = false;
                string jupyterEndpoint = "";

                // read the json results
                while (jsonResult.Read())
                {

                    switch (jsonResult.TokenType.ToString())
                    {
                        case "StartObject":
                            break;
                        case "EndObject":

                            // if compute already doesn't exist in our list, add it
                            if (!doesComputeAlreadyExistInList(computeName, computeList) && computeName != "" && publicIP != "" && sshUserName != "" && sshPort != "" && sshPubKey != "" && state != "" && containsJupyterEnv && jupyterEndpoint != "")
                            {
                                computeList.Add(new Objects.AzureML.Compute(computeName, publicIP, sshUserName, sshPort, sshPubKey, state, containsJupyterEnv, jupyterEndpoint));
                                computeName = "";
                                publicIP = "";
                                sshUserName = "";
                                sshPubKey = "";
                                sshPort = "";
                                state = "";
                                containsJupyterEnv = false;
                                jupyterEndpoint = "";
                            }
                            break;
                        case "StartArray":
                            break;
                        case "EndArray":
                            break;
                        case "PropertyName":
                            propName = jsonResult.Value.ToString();
                            break;
                        case "String":
                            if (propName.ToLower().Equals("name"))
                            {
                                computeName = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("publicipaddress"))
                            {
                                publicIP = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("adminusername"))
                            {
                                sshUserName = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("adminpublickey"))
                            {
                                sshPubKey = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("state"))
                            {
                                state = jsonResult.Value.ToString();
                            }
                            if (propName.ToLower().Equals("displayname"))
                            {
                                if (jsonResult.Value.ToString().Equals("Jupyter"))
                                {
                                    containsJupyterEnv = true;
                                    
                                }

                            }
                            if (propName.ToLower().Equals("endpointuri") && containsJupyterEnv)
                            {
                                jupyterEndpoint = jsonResult.Value.ToString();
                            }

                            break;
                        case "Date":
                            break;
                        case "Boolean":
                            break;
                        case "Integer":
                            if (propName.ToLower().Equals("sshport"))
                            {
                                sshPort = jsonResult.Value.ToString();
                            }
                            break;
                        default:
                            break;

                    }

                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }

            return computeList;
        }


        // get a single compute instance
        public static async Task<Objects.AzureML.Compute> getSingleComputeInstance(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, string computeID)
        {
            Objects.AzureML.Compute compute = null;

            string url =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/computes/{computeID}?api-version=2025-12-01";

            try
            {
                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", credentials);

                    client.DefaultRequestHeaders.UserAgent.ParseAdd("MLOKit-e977ac02118a3cb2c584d92a324e41e9");

                    var response = await client.GetAsync(url);
                    string content = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[-] ERROR: Unable to get compute instance - HTTP STATUS CODE - " + (int)response.StatusCode + " - " + response.ReasonPhrase);
                        Console.WriteLine(content);
                        Console.WriteLine("");
                        return null;
                    }

                    JObject root = JObject.Parse(content);

                    string computeName = GetFirstString(root, "name");
                    string publicIP = GetFirstString(root, "publicIpAddress");
                    string sshUserName = GetFirstString(root, "adminUserName");
                    string sshPubKey = GetFirstString(root, "adminPublicKey");
                    string sshPort = GetFirstString(root, "sshPort");
                    string state = GetFirstString(root, "state");

                    bool containsJupyterEnv = false;
                    string jupyterEndpoint = "";

                    JArray applications = root.SelectToken("properties.properties.applications") as JArray;
                    if (applications != null)
                    {
                        foreach (JObject app in applications)
                        {
                            string displayName = GetFirstString(app, "displayName");

                            if (displayName.Equals("Jupyter", StringComparison.OrdinalIgnoreCase))
                            {
                                containsJupyterEnv = true;
                                jupyterEndpoint = GetFirstString(app, "endpointUri");
                                break;
                            }
                        }
                    }

                    if (computeName != "" &&
                        publicIP != "" &&
                        sshUserName != "" &&
                        sshPort != "" &&
                        sshPubKey != "" &&
                        state != "")
                    {
                        compute = new Objects.AzureML.Compute(
                            computeName,
                            publicIP,
                            sshUserName,
                            sshPort,
                            sshPubKey,
                            state,
                            containsJupyterEnv,
                            jupyterEndpoint
                        );
                    }
                    else
                    {
                        Console.WriteLine("[-] ERROR: Compute instance response did not contain expected SSH/state fields");
                        Console.WriteLine("[*] DEBUG: name=" + computeName);
                        Console.WriteLine("[*] DEBUG: publicIP=" + publicIP);
                        Console.WriteLine("[*] DEBUG: sshUserName=" + sshUserName);
                        Console.WriteLine("[*] DEBUG: sshPort=" + sshPort);
                        Console.WriteLine("[*] DEBUG: sshPubKey=" + sshPubKey);
                        Console.WriteLine("[*] DEBUG: state=" + state);
                        Console.WriteLine("");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR in getSingleComputeInstance: " + ex.GetType().FullName);
                Console.WriteLine("[-] ERROR message: " + ex.Message + "\n");
            }

            return compute;
        }


        // add SSH key to a given compute instance
        public static async Task<bool> addSSHKey(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, string computeID, string sshKey)
        {
            bool keyAdded = false;

            string baseUrl = $"https://management.azure.com/batch?api-version=2020-06-01";
            string relativeUrl = $"/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/computes/{computeID}/updateSshPublicKey?api-version=2022-10-01";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                webRequest.Headers.Add("Authorization", "Bearer " + credentials);

                // set body and send request
                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {

                    string json = "{\"requests\": [ { \"" + "content" + "\": \"" + sshKey + "\",\"RelativeUrl\":\"" + relativeUrl + "\", \"HttpMethod\": \"post\"} ] }";
                    streamWriter.Write(json);
                }


                // get web response and status code
                HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                string content;
                var reader = new StreamReader(myWebResponse.GetResponseStream());

                // if successful
                if (myWebResponse.StatusCode.Equals(HttpStatusCode.OK))
                {
                    Console.WriteLine("[+] SUCCESS: Successfully added SSH key to compute instance");
                    Console.WriteLine("");
                    keyAdded = true;
                }

                // if unsuccessful
                else
                {
                    Console.WriteLine("[-] ERROR: Unable to add SSH key to compute instance - HTTP STATUS CODE - " + myWebResponse.StatusCode + " - " +  myWebResponse.StatusDescription);
                    Console.WriteLine("");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }

            return keyAdded;

        }

        // start a given compute instance
        public static async Task<bool> startCompute(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, string computeID)
        {
            string url =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/computes/{computeID}/start?api-version=2025-12-01";

            try
            {
                //Console.WriteLine("[*] DEBUG: startCompute URL: " + url);

                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", credentials);

                    client.DefaultRequestHeaders.UserAgent.ParseAdd("MLOKit-e977ac02118a3cb2c584d92a324e41e9");

                    //Console.WriteLine("[*] DEBUG: Sending start request");

                    using (var content = new System.Net.Http.StringContent("", System.Text.Encoding.UTF8, "application/json"))
                    {
                        var response = await client.PostAsync(url, content);
                        string body = await response.Content.ReadAsStringAsync();

                        //Console.WriteLine("[*] DEBUG: Start status: " + (int)response.StatusCode + " " + response.ReasonPhrase);

                        /**
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            Console.WriteLine("[*] DEBUG: Start response body:");
                            Console.WriteLine(body);
                        }
                        **/

                        if (response.StatusCode == HttpStatusCode.Accepted ||
                            response.StatusCode == HttpStatusCode.OK ||
                            response.StatusCode == HttpStatusCode.NoContent)
                        {
                            Console.WriteLine("[+] SUCCESS: Successfully initiated start request for compute instance");
                            Console.WriteLine("");
                            return true;
                        }

                        Console.WriteLine("[-] ERROR: Unable to initiate start request for compute instance");
                        Console.WriteLine("");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR in startCompute: " + ex.GetType().FullName);
                Console.WriteLine("[-] ERROR message: " + ex.Message + "\n");
                return false;
            }
        }


        // stop a given compute instance
        public static async Task<bool> stopCompute(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, string computeID)
        {
            string url =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/computes/{computeID}/stop?api-version=2025-12-01";

            try
            {
                //Console.WriteLine("[*] DEBUG: stopCompute URL: " + url);

                using (var client = new System.Net.Http.HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    client.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", credentials);

                    client.DefaultRequestHeaders.UserAgent.ParseAdd("MLOKit-e977ac02118a3cb2c584d92a324e41e9");

                    //Console.WriteLine("[*] DEBUG: Sending stop request");

                    using (var content = new System.Net.Http.StringContent("", System.Text.Encoding.UTF8, "application/json"))
                    {
                        var response = await client.PostAsync(url, content);
                        string body = await response.Content.ReadAsStringAsync();

                        //Console.WriteLine("[*] DEBUG: Stop status: " + (int)response.StatusCode + " " + response.ReasonPhrase);

                        /**
                        if (!string.IsNullOrWhiteSpace(body))
                        {
                            Console.WriteLine("[*] DEBUG: Stop response body:");
                            Console.WriteLine(body);
                        }
                        **/

                        if (response.StatusCode == HttpStatusCode.Accepted ||
                            response.StatusCode == HttpStatusCode.OK ||
                            response.StatusCode == HttpStatusCode.NoContent)
                        {
                            Console.WriteLine("[+] SUCCESS: Successfully initiated stop request for compute instance");
                            Console.WriteLine("");
                            return true;
                        }

                        Console.WriteLine("[-] ERROR: Unable to initiate stop request for compute instance");
                        Console.WriteLine("");
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR in stopCompute: " + ex.GetType().FullName);
                Console.WriteLine("[-] ERROR message: " + ex.Message + "\n");
                return false;
            }
        }


        // determine whether we already have a compute in our list by the given unique name for that compute
        public static bool doesComputeAlreadyExistInList(string computeName, List<Objects.AzureML.Compute> computeList)
        {
            bool doesItExist = false;

            foreach (Objects.AzureML.Compute compute in computeList)
            {
                if (compute.computeName.Equals(computeName))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }

        private static string GetFirstString(JToken token, string propertyName)
        {
            if (token == null)
            {
                return "";
            }

            if (token.Type == JTokenType.Object)
            {
                JObject obj = (JObject)token;

                foreach (JProperty prop in obj.Properties())
                {
                    if (prop.Name.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        return prop.Value.ToString();
                    }

                    string nestedValue = GetFirstString(prop.Value, propertyName);
                    if (!string.IsNullOrEmpty(nestedValue))
                    {
                        return nestedValue;
                    }
                }
            }
            else if (token.Type == JTokenType.Array)
            {
                foreach (JToken child in token.Children())
                {
                    string nestedValue = GetFirstString(child, propertyName);
                    if (!string.IsNullOrEmpty(nestedValue))
                    {
                        return nestedValue;
                    }
                }
            }

            return "";
        }


    }
}
