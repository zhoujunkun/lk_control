using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zLkControl
{
   public  class lk_param
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
       public   struct Param_
        {
            public byte product;        //产品号
            public UInt32 baud_rate;    //波特率
            public UInt16 limit_dist;  //门限距离
            public byte isLedOn;        //激光是否打开, 打开：1 ;关闭：0
            public byte isBase;        //是否是后基准  前基准： 1 ；后基准：0
            public byte ifHasConfig;    //是否传感器已经配置完成
        };
        public static Param_ lk_parm_s = new Param_();
    }
}
