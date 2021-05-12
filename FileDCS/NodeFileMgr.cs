using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDCS
{

    /// <summary>
    /// 文件操作
    /// </summary>
  public  class NodeFileMgr
    {
        public static Lazy<NodeFileMgr> obj
           = new Lazy<NodeFileMgr>();

        public static NodeFileMgr Instance
        {
            get { return obj.Value; }
        }

        static NodeFileMgr()
        {
            FileMgr.Instance.Init();
        }

        #region  网络关联

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string Copy(string file)
        {
            List<Task<RepFile>> tsks = new List<Task<RepFile>>();
            foreach (var p in LocalNode.RemoteAddress)
            {
                var rep = Task.Run(() =>
                {
                    NetTransfer transfer = new NetTransfer();
                    return transfer.SendReq(p.CurrentAddress, file, nameof(FileOption.Copy));
                });
                tsks.Add(rep);
            }

            Task.WaitAll(tsks.ToArray(), 5000);
            var repRet = tsks.Where(X => !string.IsNullOrEmpty(X.Result.File)).FirstOrDefault();
            if (repRet != null)
            {
                return repRet.Result.File;
            }
            return null;
        }


        public List<string> CopyAll(string file)
        {
            List<Task<RepFile>> tsks = new List<Task<RepFile>>();
            List<string> lst = new List<string>();
            foreach (var p in LocalNode.RemoteAddress)
            {
                var rep = Task.Run(() =>
                {
                    NetTransfer transfer = new NetTransfer();
                    return transfer.SendReq(p.CurrentAddress, file, nameof(FileOption.Copy));
                });
                tsks.Add(rep);
            }

            Task.WaitAll(tsks.ToArray(), 5000);
            var repRet = tsks.Where(X => !string.IsNullOrEmpty(X.Result.File)).Select(X => X.Result);
            foreach (var p in repRet)
            {
                lst.Add(p.File);
            }
            return lst;
        }

        /// <summary>
        /// 获取文件内容
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public List<byte[]> GetAll(string file)
        {
            List<Task<RepFile>> tsks = new List<Task<RepFile>>();
            List<byte[]> lst = new List<byte[]>();
            foreach (var p in LocalNode.RemoteAddress)
            {
                var rep = Task.Run(() =>
                {
                    NetTransfer transfer = new NetTransfer();
                    return transfer.SendReq(p.CurrentAddress, file, nameof(FileOption.Get));
                });
                tsks.Add(rep);
            }

            Task.WaitAll(tsks.ToArray(), 5000);
            var repRet = tsks.Where(X => X.Result.Data != null).Select(X => X.Result);
            foreach (var p in repRet)
            {
                lst.Add(p.Data);
            }
            return lst;
        }

        public byte[] Get(string file)
        {
            List<Task<RepFile>> tsks = new List<Task<RepFile>>();
            foreach (var p in LocalNode.RemoteAddress)
            {
                var rep = Task.Run(() =>
                {
                    NetTransfer transfer = new NetTransfer();
                    return transfer.SendReq(p.CurrentAddress, file, nameof(FileOption.Get)); ;
                });
                tsks.Add(rep);
            }

            Task.WaitAll(tsks.ToArray(), 5000);
            var repRet = tsks.Where(X => X.Result.Data != null).FirstOrDefault();
            if (repRet != null)
            {
                return repRet.Result.Data;
            }
            return null;
        }

        public void Delete(string file)
        {
            List<Task<string>> tsks = new List<Task<string>>();
            List<byte[]> lst = new List<byte[]>();
            foreach (var p in LocalNode.RemoteAddress)
            {
                var rep = Task.Run(() =>
                {
                    NetTransfer transfer = new NetTransfer();
                    return transfer.SendDelete(p.CurrentAddress, file);
                });
                tsks.Add(rep);
            }

            Task.WaitAll(tsks.ToArray(), 5000);
           

        }

        #endregion

        #region  本地关联

        public void Add(string file)
        {
            FileMgr.Instance.Add(file);
        }

        public void Put(string file,byte[]buf)
        {
            FileMgr.Instance.Put(buf,  file);
        }
        #endregion
    }
}
