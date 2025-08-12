using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace MLOKit.Utilities.Palantir
{
    class DatasetUtils
    {
        // get a list of all datasets using App folder or Spaces API discovery
        public static async Task<List<Objects.Palantir.Dataset>> getAllDatasets(string credentials)
        {
            List<Objects.Palantir.Dataset> datasetList = new List<Objects.Palantir.Dataset>();

            try
            {
                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');
                if (splitCreds.Length < 2) return datasetList;

                string token = splitCreds[0];
                string tenant = splitCreds[1];
                string apprid = splitCreds.Length >= 3 ? splitCreds[2] : "";

                // Use APP RID if provided
                if (!string.IsNullOrEmpty(apprid))
                {
                    var datasetsFromApp = await exploreFromAppFolder(token, tenant, apprid);
                    datasetList.AddRange(datasetsFromApp);
                }

                // Fallback to Spaces API discovery if no APP RID or no datasets found
                if (string.IsNullOrEmpty(apprid) || datasetList.Count == 0)
                {
                    var datasetsFromSpaces = await discoverAllProjects(token, tenant);
                    datasetList.AddRange(datasetsFromSpaces);
                }

                // Remove duplicates based on RID
                var uniqueDatasets = removeDuplicates(datasetList);

                return uniqueDatasets;
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] ERROR: " + ex.Message);
            }

            return datasetList;
        }

        // explore datasets from App folder (original functionality)
        private static async Task<List<Objects.Palantir.Dataset>> exploreFromAppFolder(string token, string tenant, string apprid)
        {
            List<Objects.Palantir.Dataset> datasetList = new List<Objects.Palantir.Dataset>();

            try
            {
                // Get App folder details first
                var appFolder = await getFolderByRid(token, tenant, apprid);
                string folderName = appFolder?.displayName ?? "App Folder";
                
                // Recursively find datasets in the App folder
                datasetList = await findDatasetsRecursively(token, tenant, apprid, folderName, 3, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] ERROR exploring APP folder: {ex.Message}");
            }

            return datasetList;
        }

        // discover all projects using Spaces API
        private static async Task<List<Objects.Palantir.Dataset>> discoverAllProjects(string token, string tenant)
        {
            List<Objects.Palantir.Dataset> datasetList = new List<Objects.Palantir.Dataset>();

            try
            {
                // Get list of spaces
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create($"https://{tenant}/api/v2/filesystem/spaces?preview=true");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers.Add("Authorization", "Bearer " + token);

                    // get web response and status code
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // Parse spaces and find datasets in each
                    var spaces = parseSpacesResponse(content);
                    foreach (var space in spaces)
                    {
                        var datasetsInSpace = await findDatasetsRecursively(token, tenant, space.rid, space.displayName, 4, 0);
                        datasetList.AddRange(datasetsInSpace);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] ERROR discovering projects: {ex.Message}");
            }

            return datasetList;
        }

        // recursively find datasets in folder hierarchy
        private static async Task<List<Objects.Palantir.Dataset>> findDatasetsRecursively(string token, string tenant, string folderRid, string path, int maxDepth, int currentDepth)
        {
            List<Objects.Palantir.Dataset> datasetList = new List<Objects.Palantir.Dataset>();

            if (currentDepth >= maxDepth)
            {
                return datasetList;
            }

            try
            {
                // Get folder children
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create($"https://{tenant}/api/v2/filesystem/folders/{folderRid}/children?preview=true");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers.Add("Authorization", "Bearer " + token);

                    // get web response and status code
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();

                    // Parse folder contents
                    var items = parseFolderContents(content, path);
                    
                    foreach (var item in items)
                    {
                        string itemPath = string.IsNullOrEmpty(path) ? item.displayName : $"{path}/{item.displayName}";

                        // Skip example content
                        if (isExampleContent(item.displayName, itemPath))
                        {
                            continue;
                        }

                        if (item.type == "FOUNDRY_DATASET")
                        {
                            // Add dataset with full path
                            var dataset = new Objects.Palantir.Dataset(
                                item.displayName, 
                                item.rid, 
                                item.rid, 
                                itemPath, 
                                item.type, 
                                "N/A", 
                                item.createdTime ?? "Unknown", 
                                item.updatedTime ?? "Unknown", 
                                item.parentFolderRid ?? folderRid
                            );
                            datasetList.Add(dataset);
                        }
                        else if (item.type == "FOLDER" || item.type == "PROJECT" || item.type == "SPACE" || item.type == "COMPASS_FOLDER")
                        {
                            // Recursively explore subfolders
                            var subDatasets = await findDatasetsRecursively(token, tenant, item.rid, itemPath, maxDepth, currentDepth + 1);
                            datasetList.AddRange(subDatasets);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] ERROR exploring folder {path}: {ex.Message}");
            }

            return datasetList;
        }

        // download dataset as binary content
        public static async Task<byte[]> downloadDataset(string credentials, string datasetRid)
        {
            byte[] fileContent = null;

            try
            {
                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');
                if (splitCreds.Length != 3) return null;

                string token = splitCreds[0];
                string tenant = splitCreds[1];

                // web request to download dataset as CSV
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create($"https://{tenant}/api/v2/datasets/{datasetRid}/readTable?format=csv");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers.Add("Authorization", "Bearer " + token);

                    // get web response 
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    MemoryStream ms = new MemoryStream();
                    myWebResponse.GetResponseStream().CopyTo(ms);
                    fileContent = ms.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] ERROR: " + ex.Message);
            }

            return fileContent;
        }

        // get dataset details/metadata
        public static async Task<string> getDatasetDetails(string credentials, string datasetRid)
        {
            string detailsJson = "";

            try
            {
                // ignore SSL errors
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                string[] splitCreds = credentials.Split(';');
                if (splitCreds.Length != 3) return detailsJson;

                string token = splitCreds[0];
                string tenant = splitCreds[1];

                // web request to get dataset details
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create($"https://{tenant}/api/v2/datasets/{datasetRid}");
                if (webRequest != null)
                {
                    // set header values
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers.Add("Authorization", "Bearer " + token);

                    // get web response and content
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();
                    detailsJson = content;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[-] ERROR: " + ex.Message);
            }

            return detailsJson;
        }

        // get folder details by RID
        private static async Task<FolderInfo> getFolderByRid(string token, string tenant, string folderRid)
        {
            try
            {
                HttpWebRequest webRequest = (HttpWebRequest)System.Net.WebRequest.Create($"https://{tenant}/api/v2/filesystem/folders/{folderRid}?preview=true");
                if (webRequest != null)
                {
                    webRequest.Method = "GET";
                    webRequest.ContentType = "application/json";
                    webRequest.UserAgent = "MLOKit-e977ac02118a3cb2c584d92a324e41e9";
                    webRequest.Headers.Add("Authorization", "Bearer " + token);

                    // get web response and status code
                    HttpWebResponse myWebResponse = (HttpWebResponse)await webRequest.GetResponseAsync();
                    string content;
                    var reader = new StreamReader(myWebResponse.GetResponseStream());
                    content = reader.ReadToEnd();
                    return parseFolderInfo(content);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] ERROR getting folder details: {ex.Message}");
            }
            return null;
        }

        // helper method to parse spaces response
        private static List<SpaceInfo> parseSpacesResponse(string jsonContent)
        {
            List<SpaceInfo> spaces = new List<SpaceInfo>();
            
            try
            {
                JsonTextReader jsonResult = new JsonTextReader(new StringReader(jsonContent));
                
                string displayName = "";
                string rid = "";
                string propName = "";
                bool inDataArray = false;

                while (jsonResult.Read())
                {
                    switch (jsonResult.TokenType.ToString())
                    {
                        case "PropertyName":
                            propName = jsonResult.Value.ToString();
                            if (propName == "data")
                            {
                                inDataArray = true;
                            }
                            break;
                        case "StartObject":
                            if (inDataArray)
                            {
                                displayName = "";
                                rid = "";
                            }
                            break;
                        case "EndObject":
                            if (inDataArray && !string.IsNullOrEmpty(rid))
                            {
                                spaces.Add(new SpaceInfo { displayName = displayName, rid = rid });
                            }
                            break;
                        case "String":
                            if (inDataArray)
                            {
                                if (propName == "displayName")
                                {
                                    displayName = jsonResult.Value.ToString();
                                }
                                else if (propName == "rid")
                                {
                                    rid = jsonResult.Value.ToString();
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] ERROR parsing spaces response: {ex.Message}");
            }

            return spaces;
        }

        // helper method to parse folder contents
        private static List<FolderItem> parseFolderContents(string jsonContent, string parentPath)
        {
            List<FolderItem> items = new List<FolderItem>();
            
            try
            {
                JsonTextReader jsonResult = new JsonTextReader(new StringReader(jsonContent));
                
                string displayName = "";
                string rid = "";
                string type = "";
                string createdTime = "";
                string updatedTime = "";
                string parentFolderRid = "";
                string propName = "";
                bool inDataArray = false;

                while (jsonResult.Read())
                {
                    switch (jsonResult.TokenType.ToString())
                    {
                        case "PropertyName":
                            propName = jsonResult.Value.ToString();
                            if (propName == "data")
                            {
                                inDataArray = true;
                            }
                            break;
                        case "StartObject":
                            if (inDataArray)
                            {
                                displayName = "";
                                rid = "";
                                type = "";
                                createdTime = "";
                                updatedTime = "";
                                parentFolderRid = "";
                            }
                            break;
                        case "EndObject":
                            if (inDataArray && !string.IsNullOrEmpty(rid))
                            {
                                items.Add(new FolderItem 
                                { 
                                    displayName = displayName, 
                                    rid = rid, 
                                    type = type,
                                    createdTime = createdTime,
                                    updatedTime = updatedTime,
                                    parentFolderRid = parentFolderRid
                                });
                            }
                            break;
                        case "String":
                            if (inDataArray)
                            {
                                if (propName == "displayName")
                                {
                                    displayName = jsonResult.Value.ToString();
                                }
                                else if (propName == "rid")
                                {
                                    rid = jsonResult.Value.ToString();
                                }
                                else if (propName == "type")
                                {
                                    type = jsonResult.Value.ToString();
                                }
                                else if (propName == "createdTime")
                                {
                                    createdTime = jsonResult.Value.ToString();
                                }
                                else if (propName == "updatedTime")
                                {
                                    updatedTime = jsonResult.Value.ToString();
                                }
                                else if (propName == "parentFolderRid")
                                {
                                    parentFolderRid = jsonResult.Value.ToString();
                                }
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] ERROR parsing folder contents: {ex.Message}");
            }

            return items;
        }

        // helper method to parse folder info
        private static FolderInfo parseFolderInfo(string jsonContent)
        {
            try
            {
                JsonTextReader jsonResult = new JsonTextReader(new StringReader(jsonContent));
                
                string displayName = "";
                string path = "";
                string type = "";
                string propName = "";

                while (jsonResult.Read())
                {
                    switch (jsonResult.TokenType.ToString())
                    {
                        case "PropertyName":
                            propName = jsonResult.Value.ToString();
                            break;
                        case "String":
                            if (propName == "displayName")
                            {
                                displayName = jsonResult.Value.ToString();
                            }
                            else if (propName == "path")
                            {
                                path = jsonResult.Value.ToString();
                            }
                            else if (propName == "type")
                            {
                                type = jsonResult.Value.ToString();
                            }
                            break;
                    }
                }

                return new FolderInfo { displayName = displayName, path = path, type = type };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[-] ERROR parsing folder info: {ex.Message}");
            }

            return null;
        }

        // check if content should be skipped (example content)
        private static bool isExampleContent(string itemName, string itemPath)
        {
            return itemName.Contains("AIP Now Ontology") || 
                   itemName.Contains("[Example]") || 
                   itemPath.Contains("[Example]");
        }

        // remove duplicate datasets based on RID
        private static List<Objects.Palantir.Dataset> removeDuplicates(List<Objects.Palantir.Dataset> datasets)
        {
            var uniqueDatasets = new List<Objects.Palantir.Dataset>();
            var seenRids = new HashSet<string>();

            foreach (var dataset in datasets)
            {
                if (!string.IsNullOrEmpty(dataset.datasetRID) && !seenRids.Contains(dataset.datasetRID))
                {
                    seenRids.Add(dataset.datasetRID);
                    uniqueDatasets.Add(dataset);
                }
            }

            return uniqueDatasets;
        }

        // helper classes for parsing JSON responses
        private class SpaceInfo
        {
            public string displayName { get; set; }
            public string rid { get; set; }
        }

        private class FolderItem
        {
            public string displayName { get; set; }
            public string rid { get; set; }
            public string type { get; set; }
            public string createdTime { get; set; }
            public string updatedTime { get; set; }
            public string parentFolderRid { get; set; }
        }

        private class FolderInfo
        {
            public string displayName { get; set; }
            public string path { get; set; }
            public string type { get; set; }
        }
    }
}
