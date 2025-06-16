using System;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace MLOKit.Utilities.MLFlow
{
    class WebUtils
    {

        // determine whether credentials provided are valid or not
        public static async Task<bool> credsValid(string credentials, string url)
        {
            // value to return whether credentials provided were valid or not
            bool areCredsValid = false;

            // ignore SSL errors
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) { return true; };
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string[] splitCreds = credentials.Split(';');

            try
            {

                // web request to check auth
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
                    string statusCode = myWebResponse.StatusCode.ToString();

                    // if we get 200 OK status code back, creds are valid
                    if (statusCode.Equals("OK"))
                    {
                        areCredsValid = true;
                    }
                }
            }
            catch (Exception ex)
            {
                return areCredsValid;
            }

            return areCredsValid;

        }


    }
}
