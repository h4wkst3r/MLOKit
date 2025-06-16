using Amazon.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace MLOKit.Utilities.AzureML
{
    class ModelUtils
    {

        // get listing of all models
        public static async Task<List<Objects.AzureML.Model>> getAllModels(string credentials, string subscriptionID, string region, string resourceGroup, string workspace)
        {
            List<Objects.AzureML.Model> modelList = new List<Objects.AzureML.Model>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to get list of models
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://" + region + ".modelmanagement.azureml.net/modelmanagement/v1.0/subscriptions/" + subscriptionID + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.MachineLearningServices/workspaces/" + workspace + "/models?api-version=2023-10-01");
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
                    string modelName = "";
                    string assetID = "";
                    string createdTime = "";
                    string modifiedTime = "";
                    string provisioningState = "";
                    string modelType = "";
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
                                if (!doesModelAlreadyExistInList(modelID, modelList) && modelID != "" && modelName != "" && assetID != "" && createdTime != "" && modifiedTime != "" && provisioningState != "" && modelType != "")
                                {
                                    modelList.Add(new Objects.AzureML.Model(modelID, modelName, assetID, createdTime, modifiedTime, provisioningState, modelType));
                                    modelID = "";
                                    modelName = "";
                                    assetID = "";
                                    createdTime = "";
                                    modifiedTime = "";
                                    provisioningState = "";
                                    modelType = "";
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
                                if (propName.ToLower().Equals("id"))
                                {
                                    modelID = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("name"))
                                {
                                    modelName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("provisioningstate"))
                                {
                                    provisioningState = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("modeltype"))
                                {
                                    modelType = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("url"))
                                {
                                    // format of json output is - aml://asset/[ASSET_ID]
                                    assetID = jsonResult.Value.ToString().Split('/')[3];
                                }

                                break;
                            case "Date":
                                if (propName.ToLower().Equals("createdtime"))
                                {
                                    createdTime = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("modifiedtime"))
                                {
                                    modifiedTime = jsonResult.Value.ToString();
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
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");
                
            }


            return modelList;

        }


        // get a model my model ID
        public static async Task<Objects.AzureML.Model> getSingleModel(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, string modID)
        {
            //List<Objects.AzureML.Model> modelList = new List<Objects.AzureML.Model>();

            Objects.AzureML.Model model = null;

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to get list of models
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://" + region + ".modelmanagement.azureml.net/modelmanagement/v1.0/subscriptions/" + subscriptionID + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.MachineLearningServices/workspaces/" + workspace + "/models/" + modID + "?api-version=2023-10-01");
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
                    string modelName = "";
                    string assetID = "";
                    string createdTime = "";
                    string modifiedTime = "";
                    string provisioningState = "";
                    string modelType = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // create the model object to return
                                if (modelID != "" && modelName != "" && assetID != "" && createdTime != "" && modifiedTime != "" && provisioningState != "" && modelType != "")
                                {
                                    model = new Objects.AzureML.Model(modelID, modelName, assetID, createdTime, modifiedTime, provisioningState, modelType);
                                    
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
                                if (propName.ToLower().Equals("id"))
                                {
                                    modelID = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("name"))
                                {
                                    modelName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("provisioningstate"))
                                {
                                    provisioningState = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("modeltype"))
                                {
                                    modelType = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("url"))
                                {
                                    // format of json output is - aml://asset/[ASSET_ID]
                                    assetID = jsonResult.Value.ToString().Split('/')[3];
                                }

                                break;
                            case "Date":
                                if (propName.ToLower().Equals("createdtime"))
                                {
                                    createdTime = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("modifiedtime"))
                                {
                                    modifiedTime = jsonResult.Value.ToString();
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
                Console.WriteLine("");
                Console.WriteLine("[-] ERROR: " + ex.Message);
                Console.WriteLine("");

            }


            return model;

        }

        // get all the asset prefixes for the assets
        public static async Task<List<string>> getAssetPrefixes(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, string assetID)
        {
            List<string> assetPrefixList = new List<string>();

            Objects.AzureML.Model model = null;

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to get assets
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://" + region + ".modelmanagement.azureml.net/modelmanagement/v1.0/subscriptions/" + subscriptionID + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.MachineLearningServices/workspaces/" + workspace + "/assets/" + assetID + "?api-version=2023-10-01");
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

                    string assetPrefix = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the asset prefix to our list
                                if (assetPrefix != "")
                                {
                                    assetPrefixList.Add(assetPrefix);
                                    assetPrefix = "";

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
                                if (propName.ToLower().Equals("prefix"))
                                {
                                    assetPrefix = jsonResult.Value.ToString();
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


            return assetPrefixList;

        }

        // get all the content URI's for a given artifact prefix
        public static async Task<List<string>> getContentURIs(string credentials, string subscriptionID, string region, string resourceGroup, string workspace, string artifactPrefix)
        {
            List<string> contentURIList = new List<string>();

            Objects.AzureML.Model model = null;

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to get content URI's
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://" + region + ".experiments.azureml.net/artifact/v2.0/subscriptions/" + subscriptionID + "/resourceGroups/" + resourceGroup + "/providers/Microsoft.MachineLearningServices/workspaces/" + workspace + "/artifacts/prefix/contentinfo/" + artifactPrefix + "?api-version=2023-10-01");
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

                    string contentURI = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add the content URI to our list
                                if (contentURI != "")
                                {
                                    contentURIList.Add(contentURI);
                                    contentURI = "";

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
                                if (propName.ToLower().Equals("contenturi"))
                                {
                                    contentURI = jsonResult.Value.ToString();
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


            return contentURIList;

        }


        // Download a file
        public static async Task<byte[]> downloadFile(string credentials, string contentURI)
        {
            byte[] fileContent = null;

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to download model
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(contentURI);
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";


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




        // determine whether we already have a model in our list by the given unique ID for that model
        public static bool doesModelAlreadyExistInList(string modelID, List<Objects.AzureML.Model> modelList)
        {
            bool doesItExist = false;

            foreach (Objects.AzureML.Model model in modelList)
            {
                if (model.modelID.Equals(modelID))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }

    }
}
