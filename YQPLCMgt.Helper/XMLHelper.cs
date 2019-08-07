using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MyLogLib;

namespace YQPLCMgt.Helper
{
    public class XMLHelper<T>
    {
        public static void SaveList(List<T> list)
        {
            try
            {
                XmlSerializer write = new XmlSerializer(typeof(List<T>));
                using (StreamWriter sw = new StreamWriter(typeof(T).Name + ".xml"))
                {
                    write.Serialize(sw, list);
                }
            }
            catch
            {
                MyLog.WriteLog("写入xml文件时失败！");
            }
        }

        public static List<T> Getlist()
        {
            try
            {
                XmlSerializer read = new XmlSerializer(typeof(List<T>));
                using (StreamReader sr = new StreamReader(typeof(T).Name + ".xml"))
                {
                    return (List<T>)read.Deserialize(sr);
                }
            }
            catch
            {
                MyLog.WriteLog("读取xml文件时失败！");
                return null;
            }
        }

        public static bool exit()
        {
            string s = (typeof(T).Name);
            return Directory.Exists(s);
        }
        public static void del()
        {
            try
            {

                Directory.Delete((typeof(T).Name));
            }
            catch (Exception ex)
            {
                MyLog.WriteLog("删除" + typeof(T).Name + "文件时失败！", ex);
            }
        }
    }
}
