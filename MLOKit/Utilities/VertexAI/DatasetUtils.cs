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
    class DatasetUtils
    {

        // get a list of all datasets
        public static async Task<List<Objects.VertexAI.Dataset>> getAllDatasets (string credentials, string region, string projectName)
        {
            List<Objects.VertexAI.Dataset> datasetList = new List<Objects.VertexAI.Dataset>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                // web request to get list of datasets
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://" + region + "-aiplatform.googleapis.com/v1/projects/" + projectName + "/locations/" + region + "/datasets");
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

                    string datasetID = "";
                    string datasetDisplayName = "";
                    string createTime = "";
                    string updateTime = "";
                    string uri = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // if dataset already doesn't exist in our list, add it
                                if (!doesDatasetAlreadyExistInList(datasetID, datasetList) && datasetID != "" && datasetDisplayName != "" && createTime != "" && updateTime != "" && uri != "" && region != "")
                                {
                                    datasetList.Add(new Objects.VertexAI.Dataset(datasetID, datasetDisplayName, createTime, updateTime, uri, region));
                                    datasetID = "";
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
                                    // dataset ID will be last in this name field
                                    string [] splitNameValue = jsonResult.Value.ToString().Split('/');
                                    datasetID = splitNameValue[5];
                                }
                                if (propName.ToLower().Equals("displayname"))
                                {
                                    datasetDisplayName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("uri"))
                                {
                                    uri = jsonResult.Value.ToString();
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


            return datasetList;

        }


        // Download a dataset
        public static async Task<byte[]> downloadDataset(string credentials, string mediaLink)
        {
            byte[] fileContent = null;

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to download dataset
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


        // determine whether we already have a dataset in our list by the given unique ID for that dataset
        public static bool doesDatasetAlreadyExistInList(string datasetID, List<Objects.VertexAI.Dataset> datasetList)
        {
            bool doesItExist = false;

            foreach (Objects.VertexAI.Dataset datSet in datasetList)
            {
                if (datSet.datasetID.Equals(datasetID))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }

    }

}