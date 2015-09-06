using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;


namespace LogHelper
{
    public static class CallBacks
    {
        public static void WebRequestCallBacks(IAsyncResult ar,IProcess process)
        {
            System.Net.WebRequest wr = ar.AsyncState as System.Net.WebRequest;
            using (var httpResp = (HttpWebResponse)wr.GetResponse())
            {
                CheckResponse(httpResp);
                using (var sr = new StreamReader(httpResp.GetResponseStream(), Encoding.UTF8))
                {
                    string s = sr.ReadToEnd();
                    process.Process(s);
                }
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
