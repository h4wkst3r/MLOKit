using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net;
using Amazon;
using Amazon.SageMaker;
using Amazon.SageMaker.Model;
using System.Net.Http;

namespace MLOKit.Utilities.SageMaker
{
    class NotebookUtils
    {

        // get number of kernels for a given notebook instance
        public static async Task<List<Objects.SageMaker.Kernel>> getKernels(AmazonSageMakerClient sagemakerClient, Uri notebookInstanceUri, HttpClientHandler handler, HttpClient http)
        {

            var kernelList = new List<Objects.SageMaker.Kernel>();

            // Derive base path (SageMaker NB often prefixes with /jupyter/default)
            var basePath = getBasePath(notebookInstanceUri, handler, http);
            var baseApi = getBaseAPI(notebookInstanceUri, basePath);


            // Try to get token from the URL if present (optional)
            var token = System.Web.HttpUtility.ParseQueryString(notebookInstanceUri.Query).Get("token");

            // Helper to add auth headers for REST calls
            void AddAuthHeaders(HttpRequestMessage req)
            {
                if (!string.IsNullOrEmpty(token))
                {
                    // Token-based auth is simplest
                    req.Headers.Add("Authorization", $"token {token}");
                }
                else
                {
                    // Fall back to XSRF header from cookie if token not available
                    var cookieHeader = handler.CookieContainer.GetCookieHeader(baseApi);
                    // Jupyter commonly sets one of these; try both names
                    var xsrf = GetCookieValue(cookieHeader, "_xsrf") ?? GetCookieValue(cookieHeader, "XSRF-TOKEN");
                    if (!string.IsNullOrEmpty(xsrf))
                        req.Headers.Add("X-XSRFToken", xsrf);
                }
            }

            // request to get the listing of kernels
            var getKernelsReq = new HttpRequestMessage(HttpMethod.Get, new Uri(baseApi, "api/kernels"));
            AddAuthHeaders(getKernelsReq);
            var getKernelsResp = await http.SendAsync(getKernelsReq);
            getKernelsResp.EnsureSuccessStatusCode();

            // parse the content and display it - along with counting number of kernels
            var content = await getKernelsResp.Content.ReadAsStringAsync();

            // Parse this page
            var items = Newtonsoft.Json.Linq.JArray.Parse(content);

            if (items != null)
            {
                foreach (var item in items)
                {
                    var id = item["id"]?.ToString() ?? string.Empty;
                    var name = item["name"]?.ToString() ?? string.Empty;
                    var executionState = item["execution_state"]?.ToString() ?? string.Empty;

                    // Only add well-formed entries & avoid dups
                    if (!string.IsNullOrEmpty(id) &&
                        !string.IsNullOrEmpty(name) &&
                        !string.IsNullOrEmpty(executionState))
                    {
                        kernelList.Add(
                            new Objects.SageMaker.Kernel(
                                id,
                                name, executionState));
                    }

                }

            }

            return kernelList;

        }

        public static async Task<string> getNotebookStatus(AmazonSageMakerClient sagemakerClient, string notebookName)
        {
            string status = "";

            DescribeNotebookInstanceRequest req = new DescribeNotebookInstanceRequest();
            req.NotebookInstanceName = notebookName;
            DescribeNotebookInstanceResponse response = await sagemakerClient.DescribeNotebookInstanceAsync(req);
            status = response.NotebookInstanceStatus;
  
            return status;

        }

        public static async Task<string> startNotebook(AmazonSageMakerClient sagemakerClient, string notebookName)
        {
            string status = "";

            StartNotebookInstanceRequest req = new StartNotebookInstanceRequest();
            req.NotebookInstanceName = notebookName;
            StartNotebookInstanceResponse response = await sagemakerClient.StartNotebookInstanceAsync(req);
            status = response.HttpStatusCode.ToString();

            return status;

        }


        public static string getBasePath(Uri notebookInstanceUri, HttpClientHandler handler, HttpClient http)
        {
            var basePath = "/";
            var path = notebookInstanceUri.AbsolutePath;
            var marker = "/jupyter/default";
            if (path.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
                basePath = marker + "/";

            return basePath;
        }

        public static Uri getBaseAPI(Uri notebookInstanceUri, string basePath)
        {

            var origin = $"{notebookInstanceUri.Scheme}://{notebookInstanceUri.Host}{(notebookInstanceUri.IsDefaultPort ? "" : ":" + notebookInstanceUri.Port)}";
            var baseApi = new Uri($"{origin}{basePath}");

            return baseApi;
        }



        public static string GetCookieValue(string cookieHeader, string name)
        {
            foreach (var part in cookieHeader.Split(';'))
            {
                var kv = part.Trim().Split('=');
                if (kv.Length == 2 && kv[0].Trim().Equals(name, StringComparison.OrdinalIgnoreCase))
                    return WebUtility.UrlDecode(kv[1]);
            }
            return null;
        }




    }
}
