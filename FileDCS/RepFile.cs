namespace FileDCS
{
    /// <summary>
    /// 文件反馈
    /// </summary>
    public class RepFile
    {
        /// <summary>
        /// 数据
        /// </summary>
        public byte[] Data { get; internal set; }

       /// <summary>
       /// 文件名称
       /// </summary>
        public string File { get; internal set; }

        /// <summary>
        /// 机器标记
        /// </summary>
        public string NodeFlage { get; set; }

        /// <summary>
        /// 传输标记
        /// </summary>
        public int TmpFlage { get; set; }
    }
}