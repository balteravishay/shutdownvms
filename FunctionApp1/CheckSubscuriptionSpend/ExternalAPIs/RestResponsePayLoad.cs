using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Text;

namespace FunctionApp1.CheckSubscuriptionSpend.ExternalApis
{
    class RestResponsePayLoad
    {
        protected static TRes GetResponse<TRes>(HttpWebRequest request, string serviceName, TraceWriter log)
        {
            try
            {
                // Call the REST endpoint
                log.Info($"Calling {serviceName} service...");
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                log.Info(String.Format($"{serviceName} service response status: {0}", response.StatusDescription));
                Stream receiveStream = response.GetResponseStream();
                if (receiveStream == null)
                    throw new Exception("Stream not received");
                // Pipes the stream to a higher level stream reader with the required encoding format. 
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                var rawResponse = readStream.ReadToEnd();
                log.Info($"{serviceName} stream received.");
                log.Info("Raw output complete");
                // Convert the Stream to a strongly typed object.  
                TRes payload = JsonConvert.DeserializeObject<TRes>(rawResponse);
                response.Close();
                readStream.Close();
                log.Info("JSON output complete");
                return payload;
            }
            catch (Exception e)
            {
                log.Error(String.Format("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : ""));
                throw;
            }
        }

        protected static HttpWebRequest BuildRquest(string nextLink, string token) =>
            AddAuthToRequest((HttpWebRequest)WebRequest.Create(nextLink), token);

        protected static HttpWebRequest BuildRquest(string armBillingService, string subscriptionId,
            string filter, string token) =>
            AddAuthToRequest((HttpWebRequest)WebRequest.Create($"{armBillingService}/subscriptions/{subscriptionId}/{filter}"), token);

        private static HttpWebRequest AddAuthToRequest(HttpWebRequest request, string token)
        {
            // Add the OAuth Authorization header, and Content Type header
            request.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            request.ContentType = "application/json";
            return request;
        }
    }
}
