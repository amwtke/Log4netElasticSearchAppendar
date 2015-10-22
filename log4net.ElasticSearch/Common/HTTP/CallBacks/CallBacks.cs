using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;


namespace log4net.ElasticSearch
{
    public static class WebRequestsCallBacks
    {
        public static void WebRequestCallBacks(IAsyncResult ar)
        {
            Console.WriteLine("call backs:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            System.Net.WebRequest wr = ar.AsyncState as System.Net.WebRequest;
            using (var httpResp = (HttpWebResponse)wr.GetResponse())
            {
                CheckResponse(httpResp);
                using (var sr = new StreamReader(httpResp.GetResponseStream(), Encoding.UTF8))
                {
                    string s = sr.ReadToEnd();
                    new MyProcess().Process(s);
                }
            }
            
        }

        public static void WebRequestCallBacks2(IAsyncResult ar)
        {
            Console.WriteLine("call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("WebRequestCallBacks2 fails,because ar is null!");

            SendHandler2 handler = ar.AsyncState as SendHandler2;
            WebRequest wr = handler.EndInvoke(ar);
            using (var httpResp = (HttpWebResponse)wr.GetResponse())
            {
                CheckResponse(httpResp);
                using (var sr = new StreamReader(httpResp.GetResponseStream(), Encoding.UTF8))
                {
                    string s = sr.ReadToEnd();
                    new MyProcess().Process(s);
                }
            }
        }

        public static void WebRequestCallBacks3(IAsyncResult ar)
        {
            Console.WriteLine("call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("WebRequestCallBacks3 fails,because ar is null!");

            try
            {
                SendHandler3 handler = ar.AsyncState as SendHandler3;
                WebRequest wr = handler.EndInvoke(ar);
                using (var httpResp = (HttpWebResponse)wr.GetResponse())
                {
                    CheckResponse(httpResp);
                    string output = System.Configuration.ConfigurationManager.AppSettings["trace_response"];
                    if (!string.IsNullOrEmpty(output) && output.Equals("true"))
                    {
                        using (var sr = new StreamReader(httpResp.GetResponseStream(), Encoding.UTF8))
                        {
                            string s = sr.ReadToEnd();
                            new MyProcess().Process(s);
                        }
                    }
                }
            }
            catch
            {

            }
        }

        private static void CheckResponse(HttpWebResponse httpResp)
        {
            if (httpResp.StatusCode == HttpStatusCode.BadRequest)
            {
                var buff = new byte[httpResp.ContentLength];
                using (var response = httpResp.GetResponseStream())
                {
                    if (response != null)
                    {
                        response.Read(buff, 0, (int)httpResp.ContentLength);
                    }
                }

                throw new InvalidOperationException(
                    string.Format("Some error occurred while sending request to Elasticsearch.{0}{1}",
                        Environment.NewLine, Encoding.UTF8.GetString(buff)));

            }
        }
    }
}
