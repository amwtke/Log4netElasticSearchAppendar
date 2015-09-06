using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace LogHelper
{
    public delegate void SendHandler(WebRequest wr, String requestJson);
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
