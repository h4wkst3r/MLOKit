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
    class RunUtils
    {

        // get a list of all runs for an experiment
        public static async Task<List<Objects.MLFlow.Run>> getAllRuns(string credentials, string url, string experimentID)
        {
            List<Objects.MLFlow.Run> runList = new List<Objects.MLFlow.Run>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to get list of runs
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/2.0/mlflow/runs/search");
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

                        string json = "{\"experiment_ids\":\"[" + experimentID +  "]\"}";
                        streamWriter.Write(json);
                    }



                    // get web response and status code
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();


                    // parse the JSON output and display results
                    JsonTextReader jsonResult = new JsonTextReader(new StringReader(content));

                    string runID = "";
                    string runName = "";
                    string userID = "";
                    string status = "";
                    string artifactLocation = "";
                    string associatedExperiment = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // if run already doesn't exist in our list, add it
                                if (!doesRunAlreadyExistInList(runID, runList) && runID != "" && runName != "" && userID != "" && status != "" && artifactLocation != "" && associatedExperiment != "")
                                {
                                    runList.Add(new Objects.MLFlow.Run(runName,runID,userID,status,artifactLocation,associatedExperiment));
                                    runName = "";
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

                                if (propName.ToLower().Equals("run_name"))
                                {
                                    runName = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("user_id"))
                                {
                                    userID = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("artifact_uri"))
                                {
                                    artifactLocation = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("run_id"))
                                {
                                    runID = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("status"))
                                {
                                    status = jsonResult.Value.ToString();
                                }
                                if (propName.ToLower().Equals("experiment_id"))
                                {
                                    associatedExperiment = jsonResult.Value.ToString();
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


            return runList;

        }


        // get the directory for the artifacts in a given run
        public static async Task<string> getArtifactDirectory(string credentials, string url, string runID)
        {

            string artifactDirectory = "";

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to get the artifact directory
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/2.0/mlflow/artifacts/list?run_id=" + runID);
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
                                if (propName.ToLower().Equals("path"))
                                {
                                    artifactDirectory = jsonResult.Value.ToString();
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

            return artifactDirectory;

        }

        // get listing of all artifacts for a given run and directory
        public static async Task<List<Objects.MLFlow.Artifact>> getArtifactListing(string credentials, string url, string runID, string directory)
        {

            List<Objects.MLFlow.Artifact> artifactList = new List<Objects.MLFlow.Artifact>();

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to get list of artifacts
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/api/2.0/mlflow/artifacts/list?run_id=" + runID + "&path=" + directory);
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

                    string path = "";
                    string isDirectory = "";
                    string propName = "";

                    // read the json results
                    while (jsonResult.Read())
                    {

                        switch (jsonResult.TokenType.ToString())
                        {
                            case "StartObject":
                                break;
                            case "EndObject":

                                // add artifact to the list
                                if (path != "" && isDirectory != "")
                                {
                                    // if it is not a directory, then add
                                    if (isDirectory.Equals("False"))
                                    {
                                        artifactList.Add(new Objects.MLFlow.Artifact(path, isDirectory));
                                        path = "";
                                    }
                                    // if it is directory, then recurse
                                    else if(isDirectory.Equals("True"))
                                    {
                                        artifactList = await getArtifactListing(credentials, url, runID, path);
                                    }
                                }
                                break;
                            case "StartArray":
                                break;
                            case "EndArray":
                                break;
                            case "PropertyName":
                                propName = jsonResult.Value.ToString();
                                break;
                            case "Boolean":

                                if (propName.ToLower().Equals("is_dir"))
                                {
                                    isDirectory = jsonResult.Value.ToString();
                                }
                                break;
                            case "String":

                                if (propName.ToLower().Equals("path"))
                                {
                                    path = jsonResult.Value.ToString();
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


            return artifactList;

        }

        // download a run artifact
        public static async Task<byte[]> downloadRunArtifact(string credentials, string url, string runID, string path)
        {

            byte[] fileContent = null;

            try
            {

                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');

                // web request to download an artifact
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create(url + "/get-artifact?path=" + path + "&run_id=" + runID);
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



            // determine whether we already have a run in our list by the given unique ID for that run
            public static bool doesRunAlreadyExistInList(string runID, List<Objects.MLFlow.Run> runList)
        {
            bool doesItExist = false;

            foreach (Objects.MLFlow.Run runSet in runList)
            {
                if (runSet.runID.Equals(runID))
                {
                    doesItExist = true;
                }
            }

            return doesItExist;
        }



    }
}
