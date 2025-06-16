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
    class ModelUtils
    {

        // get a list of all registered models
        public static async Task<List<Objects.MLFlow.Model>> getAllModels(string credentials, string url)
        {
            List<Objects.MLFlow.Model> modelList = new List<Objects.MLFlow.Model>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to get list of models
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/2.0/mlflow/model-versions/search");
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
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers["Authorization"] = "Basic " + authInfo;

 

                    // get web response and status code
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();


                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string modelName = "";
                    string modelVersion = "";
                    string status = "";
                    string description = "";
                    string artifactLocation = "";
                    string associatedRun = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // if model already doesn't exist in our list, add it
                                if (!doesModelAlreadyExistInList(modelName, modelList) && modelName != "" && modelVersion != "" && status != "" && artifactLocation != "" && associatedRun != "")
                                {
                                    modelList.Add(new Objects.MLFlow.Model(modelName,modelVersion,status,description,artifactLocation,associatedRun));
                                    modelName = "";
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
                                    modelName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("version"))
                                {
                                    modelVersion = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("source"))
                                {
                                    artifactLocation = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("run_id"))
                                {
                                    associatedRun = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("status"))
                                {
                                    status = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("description"))
                                {
                                    description = jsonResult.Value.ToString();
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


            return modelList;

        }



        // determine whether we already have a model in our list by the given model name for that run
        public static bool doesModelAlreadyExistInList(string modelName, List<Objects.MLFlow.Model> modelList)
        {
            bool doesItExist = false;

            foreach (Objects.MLFlow.Model modelSet in modelList)
            {
                if (modelSet.modelName.Equals(modelName))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }


    }
}
