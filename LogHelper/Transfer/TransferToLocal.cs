using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using log4net.ElasticSearch;
using LogManager;
using Nest;
using Newtonsoft.Json;

namespace LogManager
{
    public static class TransferToLocal
    {
        private const string BIZ_INDEX_NAME = "biz";
        private const string MARK_FILE_PATH = "Transport";
        private static int count = 0;
        private static IndexSettings _bizSettings = new IndexSettings()
        {
            NumberOfReplicas = 1,
            NumberOfShards = 2,
        };

        private static bool prepareIndex()
        {
            IGetMappingResponse mapping = ConnectionManager.LocalClient.GetMapping<BizObject>();
            if (mapping != null && (mapping.Mappings == null || mapping.Mappings.Count != 0)) return true;
            var response = ConnectionManager.LocalClient.CreateIndex(
                c => c.Index(BIZ_INDEX_NAME).InitializeUsing(_bizSettings).AddMapping<BizObject>(m => m.MapFromAttributes()));
            return response.Acknowledged;
        }

        public static int BeginTransferAsync(int size_per_round, DateTime from, DateTime to)
        {
            if (prepareIndex())
            {
                return ScanScrollHelper.TransferAsync(size_per_round, from, DateTime.Now, ProcessObject);
            }
            return 0;
        }

        public static int BeginDependonFile()
        {
            DateTime? dt = GetTimeFromFile();
            if (dt != null)
            {
                DateTime dt_now = DateTime.Now;
                Console.WriteLine("from:"+dt.GetValueOrDefault().ToString()+" TO:"+dt_now.ToString());
                MarkTimeTofile(dt_now);
                return BeginTransferAsync(1000, dt.GetValueOrDefault(), dt_now);
            }
            return 0;
        }
        [Obsolete]
        public static int BeginTransfer(int size_per_round, DateTime from, DateTime to)
        {
            if (prepareIndex())
            {
                return ScanScrollHelper.Transfer(size_per_round, from, DateTime.Now, ProcessObject);
            }
            return 0;
        }

        private static void ProcessObject<Object>(Object o)
        {
            if (o != null)
            {
                BizObject biz = JsonConvert.DeserializeObject<BizObject>(o.ToString());

                //Console.WriteLine(biz.SessionId);
                LogManager.LogHelper.LogBizAsync(biz);

                Interlocked.Increment(ref count);
                Console.WriteLine(count);
            }
        }

        private static void MarkTimeTofile(DateTime dt)
        {
            string date = formateDateTime(dt);
            FileOperation.WriteFile(MARK_FILE_PATH,date);
        }

        private static DateTime GetTimeFromFile()
        {
            try
            {
                string date = FileOperation.ReadFile(MARK_FILE_PATH);
                return string.IsNullOrEmpty(date) ? DateTime.Now.AddDays(-1) : Convert.ToDateTime(date);
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }
        private static string formateDateTime(DateTime dt)
        {
            string date = DateTime.Now.ToString("yyyy-MM-dd'T'HH:mm:ss.fff");
            return date;
        }
    }
}
