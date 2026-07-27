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
    class TrainingJobUtils
    {

        // get listing of all training jobs
        public static async Task<List<Objects.AzureML.TrainingJob>> getAllTrainingJobs(
     string credentials,
     string subscriptionID,
     string region,          // unused; kept for signature compatibility
     string resourceGroup,
     string workspace)
        {
            var trainingJobList = new List<Objects.AzureML.TrainingJob>();

            string nextLink =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/jobs?api-version=2025-06-01&$top=100";

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
                                var status = props?["status"]?.ToString() ?? string.Empty;
                                var experimentName = props?["experimentName"]?.ToString() ?? string.Empty;
                                var jobType = props?["jobType"]?.ToString() ?? string.Empty;
                                var environment = props?["environmentId"]?.ToString() ?? string.Empty;

                                string justEnvironmentName = "None";

                                if (!string.IsNullOrEmpty(environment))
                                {
                                    int indexStart = environment.IndexOf("environments");
                                    int indexEnd = environment.IndexOf("versions");
                                    justEnvironmentName = environment.Substring(indexStart + "environments".Length + 1);
                                    int indexEndFilteredString = justEnvironmentName.IndexOf("versions");
                                    justEnvironmentName = justEnvironmentName.Substring(0, indexEndFilteredString - 1);
                                }


                                // Compute target: prefer the friendly name from computeId's tail
                                string computeTarget = string.Empty;
                                var computeId = props?["computeId"]?.ToString();
                                if (!string.IsNullOrEmpty(computeId))
                                {
                                    // e.g. .../computes/cpu-cluster  -> "cpu-cluster"
                                    var idx = computeId.LastIndexOf("/computes/", StringComparison.OrdinalIgnoreCase);
                                    computeTarget = (idx >= 0 && idx + 10 < computeId.Length)
                                        ? computeId.Substring(idx + 10)
                                        : computeId;
                                }
                                else
                                {
                                    // Fallback: some system jobs don’t have computeId; try nested telemetry (“properties” bag)
                                    var innerProps = props?["properties"] as Newtonsoft.Json.Linq.JObject;
                                    computeTarget = innerProps?["_azureml.ClusterName"]?.ToString() ?? string.Empty;
                                }

                                // Job output type: try "model" then "default"
                                string jobOutputType = string.Empty;
                                var outputs = props?["outputs"] as Newtonsoft.Json.Linq.JObject;
                                var modelOut = outputs?["model"]?["jobOutputType"]?.ToString();
                                var datastoreUri = outputs?["default"]?["uri"]?.ToString() ?? string.Empty;
                                var defaultOut = outputs?["default"]?["jobOutputType"]?.ToString();
                                jobOutputType = !string.IsNullOrEmpty(modelOut) ? modelOut : (defaultOut ?? string.Empty);

                                // If no compute target, then it is serverless
                                if (computeTarget == "")
                                {
                                    computeTarget = "Serverless";
                                }

                                // Only add well-formed entries & avoid dups
                                if (!string.IsNullOrEmpty(name) &&
                                    !string.IsNullOrEmpty(status) &&
                                    !string.IsNullOrEmpty(experimentName) &&
                                    !string.IsNullOrEmpty(jobType) &&
                                    !string.IsNullOrEmpty(computeTarget) &&
                                    !doesTrainingJobAlreadyExistInList(name, trainingJobList))
                                {
                                    trainingJobList.Add(
                                        new Objects.AzureML.TrainingJob(
                                            name,
                                            status,
                                            experimentName,
                                            computeTarget,
                                            jobType,
                                            jobOutputType,
                                            justEnvironmentName,"", datastoreUri));
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

            return trainingJobList;
        }


        // get training jobs with any environment variables or parameters so they can be analyzed for credentials
        public static async Task<List<Objects.AzureML.TrainingJob>> getTrainingJobVars(
     string credentials,
     string subscriptionID,
     string region,          // unused; kept for signature compatibility
     string resourceGroup,
     string workspace)
        {
            var trainingJobList = new List<Objects.AzureML.TrainingJob>();

            string nextLink =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/jobs?api-version=2025-06-01&$top=100";

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
                                var status = props?["status"]?.ToString() ?? string.Empty;
                                var experimentName = props?["experimentName"]?.ToString() ?? string.Empty;
                                var jobType = props?["jobType"]?.ToString() ?? string.Empty;
                                var environment = props?["environmentId"]?.ToString() ?? string.Empty;

                                string justEnvironmentName = "None";

                                if (!string.IsNullOrEmpty(environment))
                                {
                                    int indexStart = environment.IndexOf("environments");
                                    int indexEnd = environment.IndexOf("versions");
                                    justEnvironmentName = environment.Substring(indexStart + "environments".Length + 1);
                                    int indexEndFilteredString = justEnvironmentName.IndexOf("versions");
                                    justEnvironmentName = justEnvironmentName.Substring(0, indexEndFilteredString - 1);
                                }


                                // Compute target: prefer the friendly name from computeId's tail
                                string computeTarget = string.Empty;
                                var computeId = props?["computeId"]?.ToString();
                                if (!string.IsNullOrEmpty(computeId))
                                {
                                    // e.g. .../computes/cpu-cluster  -> "cpu-cluster"
                                    var idx = computeId.LastIndexOf("/computes/", StringComparison.OrdinalIgnoreCase);
                                    computeTarget = (idx >= 0 && idx + 10 < computeId.Length)
                                        ? computeId.Substring(idx + 10)
                                        : computeId;
                                }
                                else
                                {
                                    // Fallback: some system jobs don’t have computeId; try nested telemetry (“properties” bag)
                                    var innerProps = props?["properties"] as Newtonsoft.Json.Linq.JObject;
                                    computeTarget = innerProps?["_azureml.ClusterName"]?.ToString() ?? string.Empty;
                                }

                                // Job output type: try "model" then "default"
                                string jobOutputType = string.Empty;
                                var outputs = props?["outputs"] as Newtonsoft.Json.Linq.JObject;
                                var modelOut = outputs?["model"]?["jobOutputType"]?.ToString();
                                var datastoreUri = outputs?["default"]?["uri"]?.ToString() ?? string.Empty;
                                var defaultOut = outputs?["default"]?["jobOutputType"]?.ToString();
                                jobOutputType = !string.IsNullOrEmpty(modelOut) ? modelOut : (defaultOut ?? string.Empty);
                                string jobVars = "";

                                // display parameters
                                if (props?["properties"] != null)
                                {
                                    var jobParams = props?["properties"] as Newtonsoft.Json.Linq.JObject;
                                    foreach (var par in jobParams)
                                    {
                                        if (par.Key.Equals("azureml.parameters"))
                                        {
                                            jobVars += par.Value;
                                        }
                                        
                                    }
                                }

                                // environment vars
                                
                                if (props?["environmentVariables"] != null)
                                {
                                    
                                    var jobEnvVars = props?["environmentVariables"] as Newtonsoft.Json.Linq.JObject;

                                    if (jobEnvVars != null)
                                    {
                                        if (jobEnvVars.Count > 0)
                                        {
                                            
                                            foreach (var envVar in jobEnvVars)
                                            {

                                                jobVars += envVar.Value;
                                            }
                                            
                                        }
                                    }
                                    
                                }
                                

                                // If no compute target, then it is serverless
                                if (computeTarget == "")
                                {
                                    computeTarget = "Serverless";
                                }

                                // Only add well-formed entries & avoid dups
                                if (!string.IsNullOrEmpty(name) &&
                                    !string.IsNullOrEmpty(status) &&
                                    !string.IsNullOrEmpty(experimentName) &&
                                    !string.IsNullOrEmpty(jobType) &&
                                    !string.IsNullOrEmpty(computeTarget) &&
                                    !string.IsNullOrEmpty(jobVars) &&
                                    !doesTrainingJobAlreadyExistInList(name, trainingJobList))
                                {
                                    trainingJobList.Add(
                                        new Objects.AzureML.TrainingJob(
                                            name,
                                            status,
                                            experimentName,
                                            computeTarget,
                                            jobType,
                                            jobOutputType,
                                            justEnvironmentName, jobVars, datastoreUri));
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

            return trainingJobList;
        }



        // get a single training job
        public static async Task<Objects.AzureML.TrainingJob> getSingleTrainingJob(
     string credentials,
     string subscriptionID,
     string region,          // unused; kept for signature compatibility
     string resourceGroup,
     string workspace,
     string jobName)
        {
            Objects.AzureML.TrainingJob trainingJob = null;

            string baseURL =
                $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/jobs/{jobName}?api-version=2025-06-01";


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
                    var name = root["name"]?.ToString() ?? string.Empty;
                    var props = root["properties"] as Newtonsoft.Json.Linq.JObject;
                    var status = props?["status"]?.ToString() ?? string.Empty;
                    var experimentName = props?["experimentName"]?.ToString() ?? string.Empty;
                    var jobType = props?["jobType"]?.ToString() ?? string.Empty;
                    var environment = props?["environmentId"]?.ToString() ?? string.Empty;
                    string justEnvironmentName = "None";

                    if (!string.IsNullOrEmpty(environment))
                    {
                        int indexStart = environment.IndexOf("environments");
                        int indexEnd = environment.IndexOf("versions");
                        justEnvironmentName = environment.Substring(indexStart + "environments".Length + 1);
                        int indexEndFilteredString = justEnvironmentName.IndexOf("versions");
                        justEnvironmentName = justEnvironmentName.Substring(0, indexEndFilteredString - 1);
                    }


                    // Compute target: prefer the friendly name from computeId's tail
                    string computeTarget = string.Empty;
                    var computeId = props?["computeId"]?.ToString();
                    if (!string.IsNullOrEmpty(computeId))
                    {
                        // e.g. .../computes/cpu-cluster  -> "cpu-cluster"
                        var idx = computeId.LastIndexOf("/computes/", StringComparison.OrdinalIgnoreCase);
                        computeTarget = (idx >= 0 && idx + 10 < computeId.Length)
                            ? computeId.Substring(idx + 10)
                            : computeId;
                    }
                    else
                    {
                        // Fallback: some system jobs don’t have computeId; try nested telemetry (“properties” bag)
                        var innerProps = props?["properties"] as Newtonsoft.Json.Linq.JObject;
                        computeTarget = innerProps?["_azureml.ClusterName"]?.ToString() ?? string.Empty;
                    }

                    // Job output type: try "model" then "default"
                    string jobOutputType = string.Empty;
                    var outputs = props?["outputs"] as Newtonsoft.Json.Linq.JObject;
                    var modelOut = outputs?["model"]?["jobOutputType"]?.ToString();
                    var datastoreUri = outputs?["default"]?["uri"]?.ToString() ?? string.Empty;
                    var defaultOut = outputs?["default"]?["jobOutputType"]?.ToString();
                    jobOutputType = !string.IsNullOrEmpty(modelOut) ? modelOut : (defaultOut ?? string.Empty);

                    // If no compute target, then it is serverless
                    if (computeTarget == "")
                    {
                        computeTarget = "Serverless";
                    }



                    // Only add well-formed entries & avoid dups
                    if (!string.IsNullOrEmpty(name))
                    {

                        trainingJob =
                            new Objects.AzureML.TrainingJob(
                                name,
                                status,
                                experimentName,
                                computeTarget,
                                jobType,
                                jobOutputType,
                                justEnvironmentName,"", datastoreUri);
                    }


                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }

            return trainingJob;
        }




        // create a training job
        public static async Task<bool> createTrainingJob(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, Dictionary<string, string> configFileVars)
        {
            bool trainingJobCreated = false;
            string randomJobName = "MLOKit-" + Utilities.FileUtils.generateRandomName();

            string baseUrl = $"https://management.azure.com/subscriptions/{subscriptionID}/resourceGroups/{resourceGroup}/providers/Microsoft.MachineLearningServices/workspaces/{workspace}/jobs/{randomJobName}?api-version=2025-01-01-preview";

            try
            {
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;


                HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
                webRequest.Method = "PUT";
                webRequest.ContentType = "application/json";
                webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                webRequest.Headers.Add("Authorization", "Bearer " + credentials);

                // set body and send request
                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {

                    string json = "";
                    string environment = configFileVars["environment"];
                    environment = environment.Replace("azureml:", "");
                    environment = environment.Replace("@latest", "");

                    // if using compute (not serverless)
                    if (configFileVars.ContainsKey("compute") && !configFileVars["compute"].Contains("serverless"))
                    {
                        json = "{\"properties\": {\"properties\": {}, \"tags\": {}, \"computeId\": \"/subscriptions/" + subscriptionID + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.MachineLearningServices/workspaces/" + workspace + "/computes/" + configFileVars["compute"].Replace("azureml:", "") + "\", \"displayName\": \"" + randomJobName + "\", \"experimentName\": \"" + randomJobName + "\", \"isArchived\": false, \"jobType\": \"" + configFileVars["type"] + "\", \"command\": \"" + configFileVars["command"] + "\", \"environmentId\": \"/subscriptions/" + subscriptionID + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.MachineLearningServices/workspaces/" + workspace + "/environments/" + environment + "/versions/1\", \"environmentVariables\": {}, \"inputs\": {}, \"outputs\": {\"model\": {\"jobOutputType\": \"uri_folder\"}}, \"resources\": {\"instanceCount\": 1, \"properties\": {}}}}";
                        Console.WriteLine(json);


                    }

                    // if serverless
                    else
                    {
                        json = "{\"properties\": {\"properties\": {}, \"tags\": {}, \"displayName\": \"" + randomJobName + "\", \"experimentName\": \"" + randomJobName + "\", \"isArchived\": false, \"jobType\": \"" + configFileVars["type"] + "\", \"command\": \"" + configFileVars["command"] + "\", \"environmentId\": \"/subscriptions/" + subscriptionID + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.MachineLearningServices/workspaces/" + workspace + "/environments/" + environment + "/versions/1\", \"environmentVariables\": {}, \"inputs\": {}, \"outputs\": {\"model\": {\"jobOutputType\": \"uri_folder\"}}, \"resources\": {\"instanceCount\": 1, \"properties\": {}}}}";
                        Console.WriteLine(json);
                    }

                    streamWriter.Write(json);
                }


                // get web response and status code
                HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                string content;
                var reader = new StreamReader(myWebResponse.GetResponseStream());

                // if successful
                if (myWebResponse.StatusCode.Equals(HttpStatusCode.Created))
                {
                    trainingJobCreated = true;
                }

                // if unsuccessful
                else
                {
                    Console.WriteLine("[-] ERROR: Unable to create training job - HTTP STATUS CODE - " + myWebResponse.StatusCode + " - " + myWebResponse.StatusDescription);
                    Console.WriteLine("");
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine("\n[-] ERROR: " + ex.Message + "\n");
            }

            return trainingJobCreated;

        }


        // determine whether we already have a training job in our list by the given unique name for that training job name
        public static bool doesTrainingJobAlreadyExistInList(string trainingJobName, List<Objects.AzureML.TrainingJob> trainingJobList)
        {
            bool doesItExist = false;

            foreach (Objects.AzureML.TrainingJob trainingJob in trainingJobList)
            {
                if (trainingJob.jobName.Equals(trainingJobName))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }


        /// Reads an Azure ML YAML config file and returns attributes as a dictionary.
        /// Keys are property names, values are deserialized objects (string, int, list, nested dict).
        public static Dictionary<string, string> parseAzureMLJobConfig(string filePath)
        {
            var yamlText = File.ReadAllText(filePath);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            object root = deserializer.Deserialize<object>(yamlText);

            var flat = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            FlattenInto(flat, string.Empty, root);
            return flat;
        }

        public static void FlattenInto(Dictionary<string, string> acc, string prefix, object value)
        {
            if (value == null)
            {
                if (!string.IsNullOrEmpty(prefix))
                    acc[prefix] = string.Empty;
                return;
            }

            // Handle mappings
            var dict = value as IDictionary;
            if (dict != null)
            {
                foreach (DictionaryEntry entry in dict)
                {
                    var key = entry.Key != null ? entry.Key.ToString() : string.Empty;
                    if (string.IsNullOrEmpty(key)) continue;

                    var childPrefix = string.IsNullOrEmpty(prefix) ? key : prefix + "." + key;
                    FlattenInto(acc, childPrefix, entry.Value);
                }
                return;
            }

            // Handle sequences
            var enumerable = value as IEnumerable;
            if (enumerable != null && !(value is string))
            {
                int i = 0;
                bool any = false;
                foreach (var item in enumerable)
                {
                    any = true;
                    var childPrefix = prefix + "[" + i + "]";
                    FlattenInto(acc, childPrefix, item);
                    i++;
                }

                if (!any && !string.IsNullOrEmpty(prefix))
                    acc[prefix] = "[]";

                return;
            }

            // Scalar
            if (!string.IsNullOrEmpty(prefix))
            {
                acc[prefix] = ConvertToString(value);
            }
        }

        public static string ConvertToString(object val)
        {
            if (val == null) return string.Empty;

            if (val is bool)
                return (bool)val ? "true" : "false";

            if (val is DateTime)
                return ((DateTime)val).ToString("o");

            return val.ToString().Trim();
        }




    }
}
