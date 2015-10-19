using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Newtonsoft.Json;
namespace LogManager
{
    delegate ISearchResponse<Object> TransferHandler(string scrollId, Action<object> process);

    public static class ScanScrollHelper
    {
        static ElasticClient _client;
        private static readonly string Address, Port, SrcIndexName, SrcIndexType;
        static Object _syncObject = new object();

        static ScanScrollHelper()
        {
            if (_client == null)
            {
                lock (_syncObject)
                {
                    if (_client == null)
                    {
                        try
                        {
                            Address = ConfigurationManager.AppSettings["src_address"];
                            Port = ConfigurationManager.AppSettings["src_port"];

                            SrcIndexName = ConfigurationManager.AppSettings["SrcIndexName"];
                            SrcIndexType = ConfigurationManager.AppSettings["SrcIndexType"];

                            //DesIndexName = ConfigurationManager.AppSettings["DesIndexName"];
                            //DesIndexType = ConfigurationManager.AppSettings["DesIndexType"];

                            var node = new Uri(@"http://" + Address + ":" + Port);

                            var settings = new ConnectionSettings(
                                node,
                                defaultIndex: "log"
                            );

                            _client = new ElasticClient(settings);
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                }
            }
        }
        public static void Transfer(Action<object> action)
        {
            var scanResults = _client.Search<Object>(s => s
                .Index(SrcIndexName)
                .Type(SrcIndexType)
                .From(0)
                .Size(1000)
                .MatchAll()
                .SearchType(SearchType.Scan)
                .Scroll("2s")
            );

            var results = _client.Scroll<Object>("2s", scanResults.ScrollId);
            {
                foreach (object o in results.Documents)
                {
                    action(o);
                }
            }
            while (results.Documents.Any())
            {
                results = _client.Scroll<Object>("2s", results.ScrollId);

                foreach (object o in results.Documents)
                {
                    action(o);
                }
            }
        }

        public static void TransferAsync(int size, Action<object> process)
        {
            var scanResults = _client.Search<Object>(s => s
                .Index(SrcIndexName)
                .Type(SrcIndexType)
                .From(0)
                .Size(size)
                .MatchAll()
                .SearchType(SearchType.Scan)
                .Scroll("2s")
            );
            if (scanResults == null) return;
            TransferHandler handler = new TransferHandler(TransferCall);
            AnsycState state = new AnsycState
            {
                Handler = handler,
                Process = process

            };
            if (string.IsNullOrEmpty(scanResults.ScrollId)) return;
            handler.BeginInvoke(scanResults.ScrollId, process, TransferCallback, state);
        }

        private static ISearchResponse<Object> TransferCall(string scrollId, Action<object> process)
        {
            var results = _client.Scroll<Object>("2s", scrollId);
            if (results.Total != 0)
                foreach (object o in results.Documents)
                {
                    process(o);
                }
            return results;
        }
        private static void TransferCallback(IAsyncResult ar)
        {
            //Console.WriteLine("call back:" + System.Threading.Thread.CurrentThread.ManagedThreadId);
            if (ar == null)
                throw new Exception("TransferCallback fails,because ar is null!");
            if (ar.AsyncState == null) return;
            AnsycState state = ar.AsyncState as AnsycState;

            ISearchResponse<Object> r = state.Handler.EndInvoke(ar);
            if (r.Total != 0)
            {
                state.Handler.BeginInvoke(r.ScrollId, state.Process, TransferCallback, state);
            }
        }

        private class AnsycState
        {
            public TransferHandler Handler { get; set; }
            public Action<object> Process { get; set; }
        }
    }
}
