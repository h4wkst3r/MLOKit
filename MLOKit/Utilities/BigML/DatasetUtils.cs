using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MLOKit.Utilities.BigML
{
    class DatasetUtils
    {

        // get a list of all datasets
        public static async Task<List<Objects.BigML.Dataset>> getAllDatasets(string credentials, string url)
        {
            List<Objects.BigML.Dataset> datasetList = new List<Objects.BigML.Dataset>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to get list of datasets
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/dataset?username=" + splitCreds[0] + "&api_key=" + splitCreds[1]);
                if (webRequest != null)
                {

                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";


                    // get web response and status code
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();


                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string datasetName = "";
                    string datasetID = "";
                    string associatedDataSource = "";
                    string associatedProject = "";
                    string visibility = "";
                    string dateCreated = "";
                    string dateUpdated = "";
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
                                if (!doesDatasetAlreadyExistInList(datasetID, datasetList) && datasetID != "" && datasetName != "" && associatedDataSource != "" && associatedProject != "" && visibility != "" && dateCreated != "" && dateUpdated != "")
                                {
                                    datasetList.Add(new Objects.BigML.Dataset(datasetName, datasetID, associatedDataSource, associatedProject, visibility, dateCreated, dateUpdated));
                                    datasetName = "";
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

                                if (propName.ToLower().Equals("source"))
                                {
                                    associatedDataSource = jsonResult.Value.ToString();
                                    associatedDataSource = associatedDataSource.Split('/')[1];
                                }
                                if (propName.ToLower().Equals("project"))
                                {
                                    associatedProject = jsonResult.Value.ToString();
                                    associatedProject = associatedProject.Split('/')[1];
                                }
                                if (propName.ToLower().Equals("name") && datasetName == "")
                                {
                                    datasetName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("resource"))
                                {
                                    datasetID = jsonResult.Value.ToString();
                                    datasetID = datasetID.Split('/')[1];
                                }
                                break;
                            case "Date":
                                if (propName.ToLower().Equals("created"))
                                {
                                    dateCreated = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("updated"))
                                {
                                    dateUpdated = jsonResult.Value.ToString();
                                }
                                break;
                            case "Boolean":
                                if (propName.ToLower().Equals("private"))
                                {
                                    visibility = jsonResult.Value.ToString();

                                    if (visibility.ToLower().Equals("true"))
                                    {
                                        visibility = "Private";
                                    }
                                    else
                                    {
                                        visibility = "Public";
                                    }
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


            return datasetList;

        }



        // Download a dataset
        public static async Task<byte[]> downloadDataset(string credentials, string datasetID)
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
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create("https://bigml.io/dataset/" + datasetID + "/download?username=" + splitCreds[0] + "&api_key=" + splitCreds[1]);
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


        // determine whether we already have a dataset in our list by the given unique ID for that dataset
        public static bool doesDatasetAlreadyExistInList(string datasetID, List<Objects.BigML.Dataset> datasetList)
        {
            bool doesItExist = false;

            foreach (Objects.BigML.Dataset datSet in datasetList)
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
