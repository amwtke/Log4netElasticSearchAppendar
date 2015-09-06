using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Web.Script.Serialization;

namespace log4net.ElasticSearch
{
    public delegate void SendHandler(WebRequest wr, String requestJson);
    public delegate WebRequest SendHandler2(String url,String requestJson,string credential);
    public delegate WebRequest SendHandler3(String url, IEnumerable<InnerBulkOperation> bulk, string credential);
    public class HTTPHelper
    {
        public static String SendWebHttpRequest(String url,String verb,String json)
        {
            try
            {
                String response = null;
                WebRequest wr = WebRequest.Create(url);
                wr.ContentType = "text/plain";
                wr.Method = verb;
                wr.Timeout = 10000;
                
                SendRequest(wr, json);

                using (var httpReponse = (HttpWebResponse)wr.GetResponse())
                {
                    response = CheckResponse(httpReponse);
                }
                return response;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region beginInboke

        /// <summary>
        /// 全异步，invoke与callback都在一个线程中。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="verb"></param>
        /// <param name="requestJson"></param>
        /// <param name="callBack"></param>
        public static void SendWebHttpRequestFullAsync(String url, String verb, String requestJson, AsyncCallback callBack)
        {
            WebRequest wr = WebRequest.Create(url);
            wr.ContentType = "text/plain";
            wr.Method = verb;
            wr.Timeout = 10000;
            SendHandler handler = new SendHandler(SendRequest);
            handler.BeginInvoke(wr, requestJson, callBack, wr);
        }

        public static void SendWebHttpRequestFullAsync(WebRequest wr, String requestJson,AsyncCallback callBack)
        {
            SendHandler handler = new SendHandler(SendRequest);
            handler.BeginInvoke(wr, requestJson, callBack, wr);
        }

        public static void SendWebHttpRequestFullAsync2(string url,String requestJson,string credential, AsyncCallback callBack)
        {
            SendHandler2 handler = new SendHandler2(SendRequest2);
            handler.BeginInvoke(url, requestJson,credential ,callBack, handler);
        }

        public static void SendWebHttpRequestFullAsync3(string url, IEnumerable<InnerBulkOperation> bulk, string credential, AsyncCallback callBack)
        {
            SendHandler3 handler = new SendHandler3(SendRequest3);
            handler.BeginInvoke(url, bulk, credential, callBack, handler);
        }
        #endregion 
        /// <summary>
        /// 半异步，invoke在其他线程，而callback回到主线程。
        /// 使用await方式做的异步。
        /// </summary>
        /// <param name="url"></param>
        /// <param name="verb"></param>
        /// <param name="requestJson"></param>
        /// <param name="callBack"></param>
        public static void SendWebHttpRequestAsync(String url, String verb, String requestJson,Action<WebRequest> callBack)
        {
            Func<WebRequest> httpRequestFuncWrapper = () =>
                {
                    WebRequest wr = WebRequest.Create(url);
                    wr.ContentType = "text/plain";
                    wr.Method = verb;
                    wr.Timeout = 10000;
                    SendRequest(wr, requestJson);
                    Console.WriteLine("HH threadID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                    return wr;
                };
            AsyncHelper.RunAsync<WebRequest>(httpRequestFuncWrapper, callBack);
        }



        public static void DefaultCallBack(WebRequest wr)
        {
            if (wr != null)
            {
                using(var httpResp = (HttpWebResponse)wr.GetResponse())
                {
                    string s = CheckResponse(httpResp);
                    Console.WriteLine(s);
                }
            }
            throw new Exception("WebRequest is null!");
        }

        private static void SendRequest(WebRequest webRequest, string requestString)
        {
            Console.WriteLine("sendRequest threadID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            using (var stream = new StreamWriter(webRequest.GetRequestStream()))
            {
                stream.Write(requestString);
            }
        }

        
        private static WebRequest SendRequest2(string url, string requestString,string credential)
        {
            Console.WriteLine("sendRequest threadID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            var webRequest = WebRequest.Create(string.Concat(url, "_bulk"));
            webRequest.ContentType = "text/plain";
            webRequest.Method = "POST";
            webRequest.Timeout = 10000;
            SetBasicAuthHeader(webRequest,credential);

            using (var stream = new StreamWriter(webRequest.GetRequestStream()))
            {
                stream.Write(requestString);
            }
            return webRequest;
        }
        private static WebRequest SendRequest3(string url, IEnumerable<InnerBulkOperation> bulk, string credential)
        {
            Console.WriteLine("sendRequest threadID:" + System.Threading.Thread.CurrentThread.ManagedThreadId);

            var webRequest = WebRequest.Create(string.Concat(url, "_bulk"));
            webRequest.ContentType = "text/plain";
            webRequest.Method = "POST";
            webRequest.Timeout = 10000;
            SetBasicAuthHeader(webRequest, credential);

            string requestString = PrepareBulk(bulk);

            using (var stream = new StreamWriter(webRequest.GetRequestStream()))
            {
                stream.Write(requestString);
            }
            return webRequest;
        }

        private static string PrepareBulk(IEnumerable<InnerBulkOperation> bulk)
        {
            var sb = new StringBuilder();
            foreach (var operation in bulk)
            {
                sb.AppendFormat(
                    @"{{ ""index"" : {{ ""_index"" : ""{0}"", ""_type"" : ""{1}""}} }}",
                    operation.IndexName, operation.IndexType);
                sb.Append("\n");

                string json = new JavaScriptSerializer().Serialize(operation.Document);
                sb.Append(json);

                sb.Append("\n");
            }
            return sb.ToString();
        }

        private static void SetBasicAuthHeader(WebRequest request,string credential)
        {
            if (!string.IsNullOrEmpty(credential))
            {
                request.Headers[HttpRequestHeader.Authorization] = credential;
            }
        }

        private static String CheckResponse(HttpWebResponse httpResponse)
        {
            if (httpResponse.StatusCode != HttpStatusCode.OK && httpResponse.StatusCode != HttpStatusCode.Created)
            {
                var buff = new byte[httpResponse.ContentLength];
                using (var response = httpResponse.GetResponseStream())
                {
                    if (response != null)
                    {
                        response.Read(buff, 0, (int)httpResponse.ContentLength);
                    }
                }

                throw new InvalidOperationException(
                    string.Format("Some error occurred while sending request to Elasticsearch.{0}{1}",
                        Environment.NewLine, Encoding.UTF8.GetString(buff)));
            }
            using (var sr = new StreamReader(httpResponse.GetResponseStream(),Encoding.UTF8))
            {
                string s = sr.ReadToEnd();
                return s;
            }
        }
    }
}
