using System;

namespace zLkControl
{
   public class sendDataitem
    {
        public object LockThis = new object();
        public bool ifHeadOnly
        {
            set;
            get;
        }
        public byte Type
        {
            set;
            get;
        }
        public byte id
        {
            set;
            get;
        }
        public UInt16 len
        {
            set;
            get;
        }
        public bool isRsponse
        {
            set;
            get;
        }
        public byte[] sendFrame;
        public byte[] sendbuf;
        public sendDataitem()
        {
            ifHeadOnly = false;
            Type = 0xff;
            id = 0;
            isRsponse = true;
        }
         

    }
}
