using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MLOKit.Utilities.AzureML
{
    class StorageBlobUtils
    {


        // modified code from - https://github.com/Azure-Samples/storage-dotnet-rest-api-with-auth
        public static async Task<byte[]> downloadFileFromStorageBlob(string storageAccountName, string storageAccountKey, string storageContainer, string relativePath, CancellationToken cancellationToken)
        {
            byte[] fileToReturn = null;

            
            // Construct the URI. This will look like this:
            string uri = "https://" +  storageAccountName + ".blob.core.windows.net/" + storageContainer + "/" +  relativePath;

            // Set this to whatever payload you desire. Ours is null because 
            //   we're not passing anything in.
            Byte[] requestPayload = null;

            //Instantiate the request message with a null payload.
            using (var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, uri)
            { Content = (requestPayload == null) ? null : new ByteArrayContent(requestPayload) })
            {

                // Add the request headers for x-ms-date and x-ms-version.
                DateTime now = DateTime.UtcNow;
                httpRequestMessage.Headers.Add("x-ms-date", now.ToString("R", CultureInfo.InvariantCulture));
                httpRequestMessage.Headers.Add("x-ms-version", "2021-08-06");
                httpRequestMessage.Headers.Add("User-Agent", "MLOKit-e977ac02118a3cb2c584d92a324e41e9");

                // If you need any additional headers, add them here before creating
                //   the authorization header. 

                // Add the authorization header.
                httpRequestMessage.Headers.Authorization = Utilities.AzureML.DatastoreUtils.GetAuthorizationHeader(
                   storageAccountName, storageAccountKey, now, httpRequestMessage);



                // Send the request.
                using (HttpResponseMessage httpResponseMessage = await new HttpClient().SendAsync(httpRequestMessage, cancellationToken))
                {
                    // If successful (status code = 200), get file
                    if (httpResponseMessage.StatusCode == HttpStatusCode.OK)
                    {
                        String responseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                        fileToReturn = System.Text.Encoding.UTF8.GetBytes(responseContent);

                        

                    }
                }
            }


            return fileToReturn;
        }


    }
}
