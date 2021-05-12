using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDCS
{
    public enum FileOption
    {
        Add,//管理
        Get,//获取字节数据
        Delete,//删除文件
        Put,//保存文件
        Copy//复制文件
    }
}
