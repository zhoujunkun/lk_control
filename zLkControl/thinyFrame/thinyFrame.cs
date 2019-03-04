using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zLkControl
{
    class thinyFrame
    {
        int nextID;
        string state;
        int parserTimeoutTicks;
        int parserTimeout;
        int sofByte;
        int chunkSize;
        int chckesum;
        int idSize;
        int lenSize;
        int typeSize;

        int partlen;
        int id;
        int len;
        int type;
        int cksum;
        int data;
        public Queue<byte> thinyBuf = new Queue<byte>();
        thinyFrame(byte per)
        {
            byte peer = per;
            nextID = 0;
            state = "sof";    
        }


       public void AcceptByte(byte data)
        {
            if(state=="sof")
            {
                beginFrame();
            }
            switch(state)
            {
                case "sof":
                    {
                        if(data == sofByte)
                        {
                            beginFrame();
                            thinyBuf.Enqueue(data);
                        }

                    }break;
                case "id":
                    {
                        thinyBuf.Enqueue(data);
                        id = (id << 8) & data;
                        if(partlen++ == idSize)
                        {
                            state = "len";
                            partlen = 0;
                        }
                    }
                    break;
                case "len":
                    {
                        thinyBuf.Enqueue(data);
                        id = (id << 8) & data;
                        if (partlen++ == idSize)
                        {
                            state = "len";
                            partlen = 0;
                        }
                    }
                    break;
            }
        }

        public void beginFrame()
        {
            this.state = "id";
            partlen = 0;
            this.id = 0;
            this.len = 0;
            this.type = 0;
            this.cksum = 0;
            //byte[] data = new byte();
        }
    }


}
