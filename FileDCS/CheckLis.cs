using System;
using ZeroMQ;

namespace FileDCS
{

    /// <summary>
    /// 预留关联无效socket
    /// </summary>
    public class CheckLis
    {
        public bool IsCheck { get; set; }

        public ZSocket Socket { get; set; }

        public DateTime CheckTime { get; set; }
    }
}
