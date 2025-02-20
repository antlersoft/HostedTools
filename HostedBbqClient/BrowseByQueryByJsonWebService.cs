using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace com.antlersoft.BBQClient
{
    public class BrowseByQueryByJsonWebService : IBrowseByQuery, IDisposable
    {
        private HttpClient httpClient;
        private string url;

        public BrowseByQueryByJsonWebService(string url, string user, string password)
        {
            httpClient = new HttpClient();
            this.url = url;
            if (!String.IsNullOrEmpty(user))
            {
                var byteArray = Encoding.ASCII.GetBytes($"{user}:{password}");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
        }

        public QueryResponse PerformQuery(QueryRequest request)
        {
            HttpContent content = new StringContent(JsonConvert.SerializeObject(request));
            QueryResponse result;
            try
            {
                HttpResponseMessage msg = httpClient.PostAsync(url, content).Result;
                if (msg.IsSuccessStatusCode)
                {
                    var ro = new JsonSerializer().Deserialize<ResultObject>(new JsonTextReader(new StreamReader(msg.Content.ReadAsStreamAsync().Result)));
                    if (ro.Result != null)
                    {
                        result = ro.Result;
                    }
                    else
                    {
                        result = new QueryResponse() { RequestException = new RequestException() { Message = $"Server handled request: {ro.Code} {ro.ErrorMessage}" } };
                    }
                }
                else
                {
                    string anyContent = msg.Content.ReadAsStringAsync().Result??"no content";
                    result = new QueryResponse() { RequestException = new RequestException() { Message = $"{msg.StatusCode} {msg.ReasonPhrase} --> {anyContent}", StackTrace = string.Empty } };
                }
            }
            catch (Exception e)
            {
                result = new QueryResponse() { RequestException = new RequestException() { Message = e.Message, StackTrace = e.StackTrace } };
            }
            return result;
        }

        public void Dispose()
        {
            httpClient.Dispose();
        }
    }
}
