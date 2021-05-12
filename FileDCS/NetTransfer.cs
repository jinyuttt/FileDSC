using System;
using ZeroMQ;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FileDCS
{

    /// <summary>
    /// 网络通信
    /// </summary>
    public class NetTransfer
    {
       
        public string LocalAddress { get; set; } = LocalNode.LocalAddress;

        public string MultAddress { get; set; } = "239.192.1.1:7781";

        public List<CheckLis> lstBind = new List<CheckLis>();
       

   

        /// <summary>
        /// 接收发布列表更新
        /// </summary>
        public void PgmSub()
        {
            using (var subscriber = new ZSocket(ZSocketType.SUB))
            {

                if (LocalNode.LocalAddress.Contains("*"))
                {
                    //绑定本地所有IP
                    foreach (var p in LocalNode.LocalAddressFamily)
                    {
                        string addr = "epgm://" + p.IPV4 + ";" + MultAddress;
                        subscriber.Bind(addr);
                    }

                }
                else
                {
                    string tmp = LocalNode.LocalAddress.Substring(6);
                    int index = tmp.IndexOf(':');
                    string addr = "epgm://" + tmp.Substring(0, index + 1) + ";" + MultAddress;
                    subscriber.Bind(addr);

                }
                subscriber.Subscribe("FileDCS");
                while (true)
                {
                   
                    using (ZFrame reply = subscriber.ReceiveFrame())
                    {
                        Console.WriteLine(" Received: {1}!", reply.ReadString());
                        if (LocalNode.AllAddress.Contains(reply.ReadString()))
                        {
                            //本节点
                            continue;
                        }
                        //接收IP地址
                        //探测IP可用性
                        var rep= SendTest(reply.ReadString());
                        if (!string.IsNullOrEmpty(rep))
                        {
                            //LocalNode.RemoteAddress.Add(reply.ReadString());
                            var addr = LocalNode.RemoteAddress.Find(X => X.NodeFlage == rep);
                            if (addr == null)
                            {
                                addr = new RemoteAddress() { NodeFlage = rep, AllAddress = new System.Collections.Generic.List<string>() };
                                addr.AllAddress.Add(reply.ReadString());
                                LocalNode.RemoteAddress.Add(addr);
                            }
                            else
                            {
                                if(!addr.AllAddress.Contains(reply.ReadString()))
                                {
                                    addr.AllAddress.Add(reply.ReadString());
                                }
                            }
                        }
                    }
                    //接收到加入节点，则需要把本节点发布一次；
                    Task.Run(() =>
                    {
                        PgmPub();
                    });
                    
                }

            }

        }

        /// <summary>
        /// 通知主题发布地址
        /// </summary>
        /// <param name="topic"></param>
        public void PgmPub()
        {
            using (var publisher = new ZSocket(ZSocketType.PUB))
            {

                //如果绑定了所有网卡接收
                if (LocalNode.LocalAddress.Contains("*"))
                {
                    //绑定本地所有IP
                    foreach (var p in LocalNode.LocalAddressFamily)
                    {
                        try
                        {
                            //当前只考虑IPV4
                            if (!string.IsNullOrEmpty(p.IPV4))
                            {
                                //采用epgm协议发送数据
                                string addr = "epgm://" + p.IPV4 + ";" + MultAddress;
                                publisher.Bind(addr);

                                foreach (var point in LocalNode.AllAddress)
                                {
                                    using (var message = new ZMessage())
                                    {
                                        message.Add(new ZFrame("FileDCS"));//这里定一个主题
                                        message.Add(new ZFrame(point));//主题数据，只使用数据
                                        publisher.Send(message);
                                    }
                                }
                               
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex);
                        }
                    }

                }
                else
                {
                    try
                    {
                        string tmp = LocalNode.LocalAddress.Substring(6);
                        int index = tmp.IndexOf(':');
                        string addr = "epgm://" + tmp.Substring(0, index + 1) + ";" + MultAddress;
                        publisher.Bind(addr);


                        foreach (var point in LocalNode.AllAddress)
                        {
                            using (var message = new ZMessage())
                            {
                                message.Add(new ZFrame("FileDCS"));//这里定一个主题
                                message.Add(new ZFrame(point));//主题数据，只使用数据
                                publisher.Send(message);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }


            }
        }

        private (string,string,string) GetAddressProtol()
        {
            string tmp = LocalNode.LocalAddress;
            string pto = tmp.Substring(0, tmp.LastIndexOf(":"));
            string ip = tmp.Substring(tmp.IndexOf("//")+1, tmp.LastIndexOf(":"));
            string point = tmp.Substring(tmp.LastIndexOf(":")+1);
            return (pto, ip, point);
        }
       
        /// <summary>
        /// 绑定所有地址
        /// </summary>

        public  void InitAddress()
        {
            var tmp = this.GetAddressProtol();
            if (LocalNode.LocalAddress.Contains("*"))
            {
                if (LocalNode.LocalAddressFamily == null || LocalNode.LocalAddressFamily.Count == 0)
                {
                    LocalNode.GetNetworkInterface();
                }
                LocalNode.AllAddress = new List<string>();
                //绑定本地所有IP
                foreach (var p in LocalNode.LocalAddressFamily)
                {
                    try
                    {
                        //当前只考虑IPV4
                        if (!string.IsNullOrEmpty(p.IPV4))
                        {
                            //采用tcp协议发送数据
                            string addr =tmp.Item1+ "://" + p.IPV4+":"+tmp.Item3;
                            var add=  this.Recvice(addr);
                            if(!string.IsNullOrEmpty(add))
                            {
                                LocalNode.AllAddress.Add(add);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }

            }
            else
            {
                try
                {
                    var add = this.Recvice(LocalNode.LocalAddress);
                    if (!string.IsNullOrEmpty(add))
                    {
                        LocalNode.AllAddress.Add(add);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        public void Init()
        {
            InitAddress();
            PgmSub();
            PgmPub();
        }


        /// <summary>
        /// 发送探测
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private string SendTest(string address)
        {
            string rep = "";
            try
            {
                using (var requester = new ZSocket(ZSocketType.REQ))
                {

                    requester.Connect(address);
                    var rd = new Random();
                    using (var message = new ZMessage())
                    {
                        message.Add(new ZFrame("-1"));
                        requester.Send(message);
                    }
                    var ret = requester.ReceiveMessage();
                    rep = ret.PopString();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }
            return rep;
        }
      
        /// <summary>
        /// 写入文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="pre"></param>
        /// <param name="buf"></param>
        /// <returns></returns>
        private string Write(string file,string pre,byte[] buf)
        {
            FileInfo fileInfo = new FileInfo(file);
            var path = Path.Combine(LocalNode.MyNodeTmp,pre, fileInfo.Name);
            using (FileStream fs = new FileStream(path, FileMode.Append))
            {
                fs.Write(buf);
            }
            return path;
        }

        /// <summary>
        /// 接收所有
        /// </summary>
        /// <param name="address"></param>
        public string Recvice(string address)
        {
                 var repSocket = new ZSocket(ZSocketType.REP);
                 repSocket.Bind(address);
                 var lis = new CheckLis() { Socket = repSocket };
                 lstBind.Add(lis);
                string point = "";
                repSocket.GetOption(ZSocketOption.LAST_ENDPOINT, out point);
                Thread pointThread = new Thread(() =>
                 {
                     while (true)
                     {
                         var msg = repSocket.ReceiveMessage();
                         string file = msg.PopString();
                         string opt = msg.PopString();
                         if (file == "-1")
                         {
                             //表明是探测请求
                             using (var message = new ZMessage())
                             {
                                 message.Add(new ZFrame(LocalNode.MyNodeFlage));
                                 repSocket.Send(message);
                             }
                             lis.IsCheck = true;
                             lis.CheckTime = DateTime.Now;
                         }
                         //查询发送文件
                         var tmp = FileMgr.Instance.GetData(file);
                         if (!string.IsNullOrEmpty(tmp))
                         {
                             if(opt== nameof(FileOption.Delete))
                             {
                                    FileMgr.Instance.Remove(file);
                                    File.Delete(file);
                                    var repinfo = new ZFrame(file);
                                    repSocket.Send(repinfo);
                                    continue;
                                 
                             }
                             //存在文件
                             using (var fs = new FileStream(tmp, FileMode.Open))
                             {
                                 var buf = new byte[1024 * 1024 * 10];
                                 int len = 0;
                                 while (len <= buf.Length)
                                 {
                                     len = fs.Read(new byte[1024 * 1024 * 10], 0, buf.Length);

                                     using (var message = new ZMessage())
                                     {
                                         message.Add(new ZFrame(LocalNode.MyNodeFlage));
                                         message.Add(new ZFrame(file));
                                         message.Add(new ZFrame(len < buf.Length ? 1 : 0));
                                         message.Add(new ZFrame(buf));//主题数据，只使用数据
                                         repSocket.Send(message);
                                     }
                                 }
                             }
                         }

                     }
                 });
            pointThread.IsBackground = true;
            pointThread.Start();
            return point;

        }
   
    
       public  RepFile SendReq(string address,string file,string opt)
        {
            RepFile repFile = new RepFile();
            repFile.Data = null;
            MemoryStream stream = new MemoryStream();
            using (var requester = new ZSocket(ZSocketType.REQ))
            {
              
                requester.Connect(address);
                var rd = new Random();
                using (var message = new ZMessage())
                {


                    message.Add(new ZFrame(file));
                    message.Add(new ZFrame(opt));
                    requester.Send(message);
                    string pre = rd.Next(1000) + "_";
                    while (true)
                    {
                        var msg = requester.ReceiveMessage();
                        var flage = msg.PopString();
                        var tmp = msg.PopString();
                        var iscomp = msg.PopInt32();
                        var buf = msg.Pop().Read();
                        repFile.NodeFlage = flage;
                        if (opt == "copy")
                        {
                           repFile.File= Write(file,pre, buf);
                        }
                        else
                        {
                            stream.Write(buf, 0, buf.Length);
                        }
                        if(iscomp==1)
                        {
                            break;
                        }
                    }
                    
                }
            }
            repFile.Data = stream.ToArray();
            return repFile;
        }

        public string  SendDelete(string address, string file)
        {
            string rep = "";
            using (var requester = new ZSocket(ZSocketType.REQ))
            {

                requester.Connect(address);
                var rd = new Random();
                using (var message = new ZMessage())
                {


                    message.Add(new ZFrame(file));
                    message.Add(new ZFrame(nameof(FileOption.Delete)));
                    requester.Send(message);

                    rep = requester.ReceiveFrame().ReadString();

                }
            }
            return rep;
        }


    }
}
