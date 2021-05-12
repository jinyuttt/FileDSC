namespace FileDCS
{
    /// <summary>
    /// 文件反馈
    /// </summary>
    public class RepFile
    {
        public byte[] Data { get; internal set; }
        public string File { get; internal set; }

        public string NodeFlage { get; set; }
    }
}