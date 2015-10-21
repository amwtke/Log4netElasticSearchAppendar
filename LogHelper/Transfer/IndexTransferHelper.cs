using Nest;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elasticsearch.Net;
using log4net.ElasticSearch;
using Newtonsoft.Json;
namespace LogManager
{
    delegate ISearchResponse<Object> TransferHandler(string scrollId, Action<object> process);
    
    public static class ScanScrollHelper
    {
       static string SrcIndexName = ConfigurationManager.AppSettings["SrcIndexName"];
       static string SrcIndexType = ConfigurationManager.AppSettings["SrcIndexType"];
        private static string ScrollTime = "1m";

        public static int TransferAll(Action<object> action)
        {
            int count = 0;
            var scanResults = ConnectionManager.RemoteClient.Search<Object>(s => s
                .Index(SrcIndexName)
                .Type(SrcIndexType)
                .From(0)
                .Size(1000)
                .MatchAll()
                .SearchType(SearchType.Scan)
                .Scroll(ScrollTime)
            );

            var results = ConnectionManager.RemoteClient.Scroll<Object>(ScrollTime, scanResults.ScrollId);
            {
                foreach (object o in results.Documents)
                {
                    action(o);
                    count++;
                }
            }
            while (results.Documents.Any())
            {
                results = ConnectionManager.RemoteClient.Scroll<Object>(ScrollTime, results.ScrollId);

                foreach (object o in results.Documents)
                {
                    action(o);
                    count++;
                }
            }
            return count;
        }

        public static int Transfer(int size, DateTime from, DateTime to, Action<object> process)
        {
            int count = 0;
            var scanResults = ConnectionManager.RemoteClient.Search<Object>(s => s
                .Index(SrcIndexName)
                .Type(SrcIndexType)
                .From(0)
                .Size(size)
                .Query(q => q.Range(r => r.OnField("TimeStamp").Greater(from).LowerOrEquals(to)))
                .SearchType(SearchType.Scan)
                .Scroll(ScrollTime)
            );

            var results = ConnectionManager.RemoteClient.Scroll<Object>(ScrollTime, scanResults.ScrollId);
            {
                foreach (object o in results.Documents)
                {
                    process(o);
                    count++;
                }
            }
            while (results.Documents.Any())
            {
                results = ConnectionManager.RemoteClient.Scroll<Object>(ScrollTime, results.ScrollId);

                foreach (object o in results.Documents)
                {
                    process(o);
                    count++;
                }
            }
            return count;
        }

        public static int TransferAsyncAll(int size, Action<object> process)
        {
            var scanResults = ConnectionManager.RemoteClient.Search<Object>(s => s
                .Index(SrcIndexName)
                .Type(SrcIndexType)
                .From(0)
                .Size(size)
                .MatchAll()
                .SearchType(SearchType.Scan)
                .Scroll(ScrollTime)
            );
            if (scanResults == null) return 0;
            TransferHandler handler = new TransferHandler(TransferCall);
            AnsycState state = new AnsycState
            {
                Handler = handler,
                Process = process

            };
            if (string.IsNullOrEmpty(scanResults.ScrollId)) return 0;
            handler.BeginInvoke(scanResults.ScrollId, process, TransferCallback, state);
            return Convert.ToInt32(scanResults.Total);
        }

        public static int TransferAsync(int size, DateTime from,DateTime to,Action<object> process)
        {
            //from = from.AddHours(-8);
            //to = to.AddHours(-8);
            var scanResults = ConnectionManager.RemoteClient.Search<Object>(s => s
                .Index(SrcIndexName)
                .Type(SrcIndexType)
                .From(0)
                .Size(size)
                .Query(q=>q.Range(r=>r.OnField("TimeStamp").Greater(from).LowerOrEquals(to)))
                .SearchType(SearchType.Scan)
                .Scroll(ScrollTime)
            );
            if (scanResults == null) return 0;
            TransferHandler handler = new TransferHandler(TransferCall);
            AnsycState state = new AnsycState
            {
                Handler = handler,
                Process = process

            };
            if (string.IsNullOrEmpty(scanResults.ScrollId)) return 0;
            handler.BeginInvoke(scanResults.ScrollId, process, TransferCallback, state);
            return Convert.ToInt32(scanResults.Total);
        }


        private static ISearchResponse<Object> TransferCall(string scrollId, Action<object> process)
        {
            var results = ConnectionManager.RemoteClient.Scroll<Object>(ScrollTime, scrollId);
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
