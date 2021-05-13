using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace FileDCS
{

    /// <summary>
    /// 本地节点信息
    /// </summary>
    public class LocalNode
    {
        /// <summary>
        /// 本地地址
        /// </summary>
        public static string LocalAddress { get; set; } = "tcp://*:0";

        /// <summary>
        /// 本地所有IP
        /// </summary>
        public static List<NetWorkInfo> LocalAddressFamily { get; internal set; }

        /// <summary>
        /// 本地全部地址
        /// </summary>
        public static List<string> AllAddress { get; set; }

       /// <summary>
       /// 系统目录
       /// </summary>
        public static string MyNode = Environment.SystemDirectory;

        /// <summary>
        /// 节点标记
        /// </summary>
        public static string MyNodeFlage = "";

        /// <summary>
        /// 临时目录
        /// </summary>
        public static string MyNodeTmp = Environment.GetFolderPath(Environment.SpecialFolder.Templates);

        public static string NodeWatchDir = "";

       /// <summary>
       /// 加入的地址
       /// </summary>
        public static List<RemoteAddress> RemoteAddress { get; set; }

        /// <summary>
        /// 获取所有IP地址
        /// </summary>
        public static void GetNetworkInterface()
        {
            try
            {
                LocalAddressFamily = new List<NetWorkInfo>();
                NetworkInterface[] adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface ni in adapters)
                {
                    IPInterfaceProperties ipProperties = ni.GetIPProperties();

                    foreach (var curIP in ipProperties.UnicastAddresses)
                    {

                        var v = new NetWorkInfo() { IPV4 = curIP.Address.ToString(), Mask = curIP.IPv4Mask.ToString(), DnsAddress = ipProperties.DnsAddresses.ToString() };
                        if (v.IPV4.Contains(":"))
                        {
                            v.IPV6 = v.IPV4;
                            v.IPV4 = "127.0.0.1";
                        }
                        if (ipProperties.GatewayAddresses.Count > 0)
                            v.GatewayAddress = ipProperties.GatewayAddresses[0].Address.ToString();

                        LocalAddressFamily.Add(v);
                    }


                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

    }

   /// <summary>
   /// 所有信息
   /// </summary>
    public class RemoteAddress
    {
         private int index = 0;
        public string NodeFlage { get; set; }

        public int Count { get { return AllAddress.Count; } }

        /// <summary>
        /// 所有地址
        /// </summary>
        public List<string> AllAddress { get; set; }

        /// <summary>
        /// 当前可用地址
        /// </summary>
        public string CurrentAddress { get; set; }


        /// <summary>
        /// 获取地址
        /// </summary>
        /// <returns></returns>
        public string GetAddress()
        {
            if (AllAddress.Count == 0)
            {
                return "";
            }
            else if (AllAddress.Count == 1)
            {
                return CurrentAddress=AllAddress[0];
            }
            else
            {
                return CurrentAddress=AllAddress[index++ % AllAddress.Count];
            }
        }
    }
}