using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

namespace MLOKit.Utilities.MLFlow
{
    class ExperimentUtils
    {

        // get a list of all experiments
        public static async Task<List<Objects.MLFlow.Experiment>> getAllExperiments(string credentials, string url)
        {
            List<Objects.MLFlow.Experiment> experimentList = new List<Objects.MLFlow.Experiment>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to get list of experiments
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/2.0/mlflow/experiments/search");
                if (webRequest != null)
                {

                    string authInfo = "";

                    // if credentials given, base64 encode them for basic auth
                    if (credentials != "")
                    {
                        authInfo = splitCreds[0] + ":" + splitCreds[1];
                        authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
                    }

                    // set header values
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers["Authorization"] = "Basic " + authInfo;

                    // set body and send request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {

                        string json = "{\"max_results\":\"5000\"}";
                        streamWriter.Write(json);
                    }



                    // get web response and status code
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();


                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string experimentID = "";
                    string experimentName = "";
                    string artifactLocation = "";
                    string status = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // if experiment already doesn't exist in our list, add it
                                if (!doesExperimentAlreadyExistInList(experimentID, experimentList) && experimentID != "" && experimentName != "" && artifactLocation != "" && status != "")
                                {
                                    experimentList.Add(new Objects.MLFlow.Experiment(experimentName,experimentID,status,artifactLocation));
                                    experimentName = "";
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

                                if (propName.ToLower().Equals("experiment_id"))
                                {
                                    experimentID = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("name"))
                                {
                                    experimentName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("artifact_location"))
                                {
                                    artifactLocation = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("lifecycle_stage"))
                                {
                                    status = jsonResult.Value.ToString();
                                }
                                break;
                            default:
                                break;

                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }


            return experimentList;

        }



        // determine whether we already have a experiment in our list by the given unique ID for that experiment
        public static bool doesExperimentAlreadyExistInList(string experimentID, List<Objects.MLFlow.Experiment> experimentList)
        {
            bool doesItExist = false;

            foreach (Objects.MLFlow.Experiment experimentSet in experimentList)
            {
                if (experimentSet.experimentID.Equals(experimentID))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }


    }
}
