using FileDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileDCS
{

    /// <summary>
    /// 文件关联
    /// </summary>
    public class FileMgr
    {
        public static Lazy<FileMgr> obj
            = new Lazy<FileMgr>();
        private DbAccess db_Access = new DbAccess();

        private NetTransfer netTransfer = new NetTransfer();
        public static FileMgr Instance
        {
            get { return obj.Value; }
        }

        /// <summary>
        /// 初始化节点信息
        /// </summary>
        public void Init()
        {
            DbAccess.SetUpEnv(LocalNode.MyNode, "data");
            db_Access.dbFileName = "filenode.db";
            db_Access.Init();
            var v= db_Access.GetData("nodename");
            if (string.IsNullOrEmpty(v))
            {
                string str = Guid.NewGuid().ToString("N");
                db_Access.Put("nodename", LocalNode.MyNodeFlage);
                LocalNode.MyNodeFlage = db_Access.GetData("nodename");
            }
            else
            {
                LocalNode.MyNodeFlage = v;
            }

            //初始化网络接收
            netTransfer.Init();
            //
        }

     
        /// <summary>
        /// 添加文件关联
        /// </summary>
        /// <param name="file"></param>
        /// <param name="flage"></param>
        public void Add(string file,string flage=null)
        {
            db_Access.Put(file, flage);
        }

        /// <summary>
        /// 获取文件关联信息
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetData(string file)
        {
           return db_Access.GetData(file);
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string Remove(string file)
        {
           var str=  db_Access.GetData(file);
            db_Access.Delete(file);
            return str;
        }

       /// <summary>
       /// 保存文件
       /// </summary>
       /// <param name="buf"></param>
       /// <param name="file"></param>
        public void Put(byte[]buf,string file)
        {
            string path = Path.Combine(LocalNode.MyNodeTmp,"Remote", file);
            string dir = Path.Combine(LocalNode.MyNodeTmp, "Remote");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate))
            {
                fs.Write(buf);
            }
            this.Add(path);
        }


       

    }
}
