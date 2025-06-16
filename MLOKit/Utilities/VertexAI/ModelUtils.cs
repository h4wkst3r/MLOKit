using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MLOKit.Utilities.VertexAI
{
    class ModelUtils
    {

        // get a list of all models
        public static async Task<List<Objects.VertexAI.Model>> getAllModels(string credentials, string region, string projectName)
        {
            List<Objects.VertexAI.Model> modelList = new List<Objects.VertexAI.Model>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to get list of models
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://" + region + "-aiplatform.googleapis.com/v1/projects/" + projectName + "/locations/" + region + "/models");
                if (webRequest != null)
                {

                    // set header values
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

                    string modelID = "";
                    string modelDisplayName = "";
                    string createTime = "";
                    string updateTime = "";
                    string sourceType = "";
                    string exportableFormat = "";
                    string propName = "";
                    List<string> exportFormatList = new List<string>();
                    exportFormatList.Add("tflite");
                    exportFormatList.Add("edgetpu-tflite");
                    exportFormatList.Add("tf-saved-model");
                    exportFormatList.Add("tf-js");
                    exportFormatList.Add("core-ml");
                    exportFormatList.Add("custom-trained");


                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // if model already doesn't exist in our list, add it
                                if (!doesModelAlreadyExistInList(modelID, modelList) && modelID != "" && modelDisplayName != "" && exportableFormat != "" && createTime != "" && updateTime != "" && sourceType != "" && region != "")
                                {
                                    modelList.Add(new Objects.VertexAI.Model(modelID, modelDisplayName, createTime, updateTime, sourceType, region, exportableFormat));
                                    modelID = "";
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
                                    // model ID will be last in this name field
                                    string[] splitNameValue = jsonResult.Value.ToString().Split('/');
                                    modelID = splitNameValue[5];
                                }
                                if (propName.ToLower().Equals("displayname"))
                                {
                                    modelDisplayName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("sourcetype"))
                                {
                                    sourceType = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("id"))
                                {
                                    if (exportFormatList.Contains(jsonResult.Value.ToString().ToLower()))
                                    {
                                        exportableFormat = jsonResult.Value.ToString();
                                    }
                                }
                                break;
                            case "Date":
                                if (propName.ToLower().Equals("createtime"))
                                {
                                    createTime = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("updatetime"))
                                {
                                    updateTime = jsonResult.Value.ToString();
                                }
                                break;
                            case "Boolean":
                                break;
                            default:
                                break;

                        }

                    }
                }
            }
            catch (Exception ex)
            {

            }


            return modelList;

        }



        // Download a model
        public static async Task<byte[]> downloadModel(string credentials, string mediaLink)
        {
            byte[] fileContent = null;

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to download model
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(mediaLink);
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers.Add("Authorization", "Bearer " + credentials);


                    // get web response 
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    MemoryStream ms = new MemoryStream();
                    myWebResponse.GetResponseStream().CopyTo(ms);
                    fileContent = ms.ToArray();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
            }


            return fileContent;

        }

        // export model
        public static async Task<string> exportModel(string credentials, string modelID, string region, string bucket, string exportFormat, string project)
        {

            string artifactOutputUri = "";


            try
            {
                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to export model
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://" + region + "-aiplatform.googleapis.com/v1/projects/" + project + "/locations/" + region + "/models/" + modelID + ":export");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "POST";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers.Add("Authorization", "Bearer " + credentials);

                    // set body and send request
                    using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                    {

                        string json = "{\"outputConfig\":{\"exportFormatId\":\"" + exportFormat + "\",\"artifactDestination\":{\"outputUriPrefix\":\"gs://" + bucket + "\"}}}";
                        streamWriter.Write(json);
                    }


                    // get web response 
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {
                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":
                                break;
                            case "StartArray":
                                break;
                            case "EndArray":
                                break;
                            case "PropertyName":
                                propName = jsonResult.Value.ToString();
                                break;
                            case "String":
                                if (propName.ToLower().Equals("artifactoutputuri"))
                                {
                                    artifactOutputUri = jsonResult.Value.ToString();
                                }
                                break;
                            case "Date":
                                break;
                            case "Boolean":
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

            return artifactOutputUri;

        }


        // determine whether we already have a model in our list by the given unique ID for that model
        public static bool doesModelAlreadyExistInList(string modelID, List<Objects.VertexAI.Model> modelList)
        {
            bool doesItExist = false;

            foreach (Objects.VertexAI.Model mod in modelList)
            {
                if (mod.modelID.Equals(modelID))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }
    }
}
