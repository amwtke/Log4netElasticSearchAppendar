﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web.Script.Serialization;

namespace log4net.ElasticSearch
{
    public interface IElasticsearchClient : IDisposable
    {
        string Server { get; }
        int Port { get; }
        bool Ssl { get; }
        bool AllowSelfSignedServerCert { get; }
        string BasicAuthUsername { get; }
        string BasicAuthPassword { get; }
        void PutTemplateRaw(string templateName, string rawBody);
        void IndexBulk(IEnumerable<InnerBulkOperation> bulk);
        IAsyncResult IndexBulkAsync(IEnumerable<InnerBulkOperation> bulk);
    }
    
    public class InnerBulkOperation 
    {
        public string IndexName { get; set; }
        public string IndexType { get; set; }
        public object Document { get; set; }
    }

    public class WebElasticClient : IElasticsearchClient
    {
        public string Server { get; private set; }
        public int Port { get; private set; }
        public bool Ssl { get; private set; }
        public bool AllowSelfSignedServerCert { get; private set; }
        public string BasicAuthUsername { get; private set; }
        public string BasicAuthPassword { get; private set; }
        public string Url { get { return _url; } }

        private readonly string _url;
        private readonly string _credentials;

        public WebElasticClient(string server, int port)
            : this(server, port, false, false, string.Empty, string.Empty)
        {
        }

        public WebElasticClient(string server, int port,
                                bool ssl, bool allowSelfSignedServerCert, 
                                string basicAuthUsername, string basicAuthPassword)
        {
            Server = server;
            Port = port;
            ServicePointManager.Expect100Continue = false;

            // SSL related properties
            Ssl = ssl;
            AllowSelfSignedServerCert = allowSelfSignedServerCert;
            BasicAuthPassword = basicAuthPassword;
            BasicAuthUsername = basicAuthUsername;

            if (Ssl && AllowSelfSignedServerCert)
            {
                ServicePointManager.ServerCertificateValidationCallback += AcceptSelfSignedServerCertCallback;
            }

            if(BasicAuthUsername != null && !string.IsNullOrEmpty(BasicAuthUsername.Trim()))
            {
                string authInfo = string.Format("{0}:{1}", BasicAuthUsername, BasicAuthPassword);
                string encodedAuthInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
                _credentials = string.Format("{0} {1}", "Basic", encodedAuthInfo);
            }

            _url = string.Format("{0}://{1}:{2}/", Ssl ? "https" : "http", Server, Port);
        }

        public void PutTemplateRaw(string templateName, string rawBody)
        {
            var webRequest = WebRequest.Create(string.Concat(_url, "_template/", templateName));
            webRequest.ContentType = "text/json";
            webRequest.Method = "PUT";
            SetBasicAuthHeader(webRequest);
            SendRequest(webRequest, rawBody);
            using (var httpResponse = (HttpWebResponse)webRequest.GetResponse())
            {
                CheckResponse(httpResponse);
            }
        }

        public void IndexBulk(IEnumerable<InnerBulkOperation> bulk)
        {
            var webRequest = PrepareBulkAndSend(bulk);
            using (var httpResponse = (HttpWebResponse) webRequest.GetResponse())
            {
                CheckResponse(httpResponse);
            }
        }
        
        public IAsyncResult IndexBulkAsync(IEnumerable<InnerBulkOperation> bulk)
        {
            var webRequest = PrepareBulkAndSend(bulk);
            return webRequest.BeginGetResponse(FinishGetResponse, webRequest);
        }

        private void FinishGetResponse(IAsyncResult result)
        {
            var webRequest = (WebRequest)result.AsyncState;
            using (var httpResponse = (HttpWebResponse)webRequest.EndGetResponse(result))
            {
                CheckResponse(httpResponse);
                StreamReader r = new StreamReader(httpResponse.GetResponseStream(), Encoding.UTF8);
                Console.WriteLine(r.ReadToEnd());
            }
        }

        private WebRequest PrepareBulkAndSend(IEnumerable<InnerBulkOperation> bulk)
        {
            var requestString = PrepareBulk(bulk);

            var webRequest = WebRequest.Create(string.Concat(_url, "_bulk"));
            webRequest.ContentType = "text/plain";
            webRequest.Method = "POST";
            webRequest.Timeout = 10000;
            SetBasicAuthHeader(webRequest);
            SendRequest(webRequest, requestString);
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

        private static void SendRequest(WebRequest webRequest, string requestString)
        {
            using (var stream = new StreamWriter(webRequest.GetRequestStream()))
            {
                stream.Write(requestString);
            }
        }

        private void SetBasicAuthHeader(WebRequest request)
        {
            if (!string.IsNullOrEmpty(_credentials)) 
            {
                request.Headers[HttpRequestHeader.Authorization] = _credentials;
            }
        }

        private bool AcceptSelfSignedServerCertCallback(
            object sender,
            X509Certificate certificate,
            X509Chain chain,
            SslPolicyErrors sslPolicyErrors)
        {
            var certificate2 = (certificate as X509Certificate2);

            string subjectCn = certificate2.GetNameInfo(X509NameType.DnsName, false);
            string issuerCn = certificate2.GetNameInfo(X509NameType.DnsName, true);
            if (sslPolicyErrors == SslPolicyErrors.None
                || (Server.Equals(subjectCn) && subjectCn.Equals(issuerCn)))
            {
                return true;
            }

            return false;
        }

        private static void CheckResponse(HttpWebResponse httpResponse)
        {
            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                var buff = new byte[httpResponse.ContentLength];
                using (var response = httpResponse.GetResponseStream())
                {
                    if (response != null)
                    {
                        response.Read(buff, 0, (int) httpResponse.ContentLength);
                    }
                }

                throw new InvalidOperationException(
                    string.Format("Some error occurred while sending request to Elasticsearch.{0}{1}",
                        Environment.NewLine, Encoding.UTF8.GetString(buff)));
            }
        }

        public void Dispose()
        {
            ServicePointManager.ServerCertificateValidationCallback -= AcceptSelfSignedServerCertCallback;
        }
    }
}
