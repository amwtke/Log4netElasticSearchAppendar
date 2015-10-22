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
        private static IndexSettings _bizSettings = new IndexSettings()
        {
            NumberOfReplicas = 1,
            NumberOfShards = 2,
        };

        static TransferToLocal()
        {
            if(!prepareIndex())
                throw new Exception("初始化索引失败！可能是网络原因。");
        }

        private static bool prepareIndex()
        {
            IGetMappingResponse mapping = ConnectionManager.RemoteClient.GetMapping<BizObject>();
            if (mapping != null && (mapping.Mappings == null || mapping.Mappings.Count != 0)) return true;
            //var response = ConnectionManager.RemoteClient.CreateIndex(
            //    c => c.Index(BIZ_INDEX_NAME).InitializeUsing(_bizSettings).AddMapping<BizObject>(m => m.MapFromAttributes()));

            var response = ConnectionManager.RemoteClient.Map<BizObject>(x => x.Index(BIZ_INDEX_NAME));
            return response.Acknowledged;
        }

        public static int BeginTransferAsync(int size_per_round, DateTime from, DateTime to)
        {
            //if (prepareIndex())
            //{
                return ScanScrollHelper.TransferAsync(size_per_round, from, to, ProcessObject);
            //}
            return 0;
        }

        public static int BeginDependonFile()
        {
            DateTime? dt = GetTimeFromFile();
            if (dt != null)
            {
                Console.WriteLine("from:" + dt.GetValueOrDefault().ToString() + " TO:" + DateTime.Now.ToString());

                DateTime dt_now = DateTime.Now;
                int total =  BeginTransferAsync(1000, dt.GetValueOrDefault().ToUniversalTime(), dt_now.ToUniversalTime());
                MarkTimeTofile(dt_now);
                return total;
            }
            return 0;
        }
        [Obsolete]
        public static int BeginTransfer(int size_per_round, DateTime from, DateTime to)
        {
            //if (prepareIndex())
            //{
                return ScanScrollHelper.Transfer(size_per_round, from, DateTime.Now, ProcessObject);
            //}
            return 0;
        }

        private static void ProcessObject<Object>(Object o)
        {
            if (o != null)
            {
                BizObject bizRemote = JsonConvert.DeserializeObject<BizObject>(o.ToString());
                if (string.IsNullOrEmpty(bizRemote.UUID))
                {
                    bizRemote.UUID = Guid.NewGuid().ToString();
                    LogHelper.LogBizAsync(bizRemote);
                }
                else
                {
                    if(GetLocalBizObjectByUUID(bizRemote.UUID)==null)
                        LogHelper.LogBizAsync(bizRemote);
                    else
                    {
                        //LogToFile("怎么回事？+uuid:"+ bizRemote.UUID);
                    }
                }
            }
        }

        public static BizObject GetLocalBizObjectByUUID(string uuid)
        {
            var result = ConnectionManager.LocalClient.Search<BizObject>(s => s
                .From(0)
                .Size(10)
                .Query(q=>q.QueryString(ss=>ss.Query("UUID:"+"\""+uuid+"\"")))
                );
            if (result.Total >=1)
                return result.Documents.First();
            return null;
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
            string date = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return date;
        }

        private static void LogToFile(string content)
        {
            FileOperation.WriteFileAppand("TransferLog.txt", "========================"+DateTime.Now.ToString("yyyy-mm-dd hh:mm:ss.yyy")+"========================");
            FileOperation.WriteFileAppand("TransferLog.txt", content);
            FileOperation.WriteFileAppand("TransferLog.txt", "====================================");
        }
    }
}
