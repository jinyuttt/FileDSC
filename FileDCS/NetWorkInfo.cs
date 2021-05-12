namespace FileDCS
{
    /// <summary>
    /// 网卡信息
    /// </summary>
    public class NetWorkInfo
    {
        /// <summary>
        /// IP4网络
        /// </summary>
        public string IPV4 { get; set; }

        /// <summary>
        /// IP6网络
        /// </summary>
        public string IPV6 { get; set; }

        /// <summary>
        /// 掩码
        /// </summary>
        public string Mask { get; set; }

        /// <summary>
        /// 路由地址
        /// </summary>
        public string GatewayAddress { get; set; }

        /// <summary>
        /// Dns地址
        /// </summary>
        public string DnsAddress { get; set; }
    }
}