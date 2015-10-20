using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace LogManager
{
    public class FileOperation
    {
        public static void WriteFile(string filePath, string content)
        {
            try
            {
                using (FileStream aFile = new FileStream(filePath, //"Log.txt", 
                    FileMode.OpenOrCreate))
                {
                    using (StreamWriter sw = new StreamWriter(aFile))
                    {
                        sw.WriteLine(content);
                    }
                }
                    
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 只读一行
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string ReadFile(string filePath)
        {
            try
            {
                using (FileStream aFile = new FileStream(filePath,//"Log.txt", 
                    FileMode.Open))
                {
                    using (StreamReader sr = new StreamReader(aFile))
                    {
                        return sr.ReadLine();
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
