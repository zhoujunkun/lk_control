using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace zLkControl
{
   public class thinyFrame
    {
        /*
        ,-----+-----+-----+------+------------+- - - -+-------------,
        | SOF | ID  | LEN | TYPE | HEAD_CKSUM | DATA  | DATA_CKSUM  |
        | 0-1 | 1-4 | 1-4 | 1-4  | 0-4        | ...   | 0-4         | <- size (bytes)
        '-----+-----+-----+------+------------+- - - -+-------------'

        SOF ......... start of frame, usually 0x01 (optional, configurable)
        ID  ......... the frame ID (MSb is the peer bit)
        LEN ......... number of data bytes in the frame
        TYPE ........ message type (used to run Type Listeners, pick any values you like)
        HEAD_CKSUM .. header checksum

        DATA ........ LEN bytes of data
        DATA_CKSUM .. data checksum (left out if LEN is 0)
         * */
        static int MAX_TYPE_SIZE = 10;
        static int MAX_GENERAL_SIZE = 10;
        checksum check_crc = new checksum();
        string state;
        int partlen;
        int cksum;
        byte peer;
        _idListenr idListenr = new _idListenr();
        _generalListenr[] generalListenr = new _generalListenr[MAX_GENERAL_SIZE];
        _typeListenr[] typeListener = new _typeListenr[MAX_TYPE_SIZE];
        _message msg = new _message();
        public Queue<byte> headBuf = new Queue<byte>();
        public Queue<byte> dataBuf = new Queue<byte>();
        public Queue<byte> frameBuf = new Queue<byte>();
        public int idSize { set; get;}
        public int lenSize { set; get; }
        public int typeSize { set; get; }
        public byte sofByte { set; get; }
        public thinyFrame(byte per)
        {
            peer = per;
            state = "sof";
            sofByte = 0x01;   //start of frame, usually 0x01 (optional, configurable)
            idSize = 1;   //
            lenSize = 2;
            typeSize = 1;       
        }

        public void resetParser()
        {
            state = "sof";
            partlen = 0;
            cksum = 0;
            headBuf.Clear();
            dataBuf.Clear();
            frameBuf.Clear();
        }

        struct _message
        {
           public byte frame_id ;   // the frame ID (MSb is the peer bit)
           public UInt16 len ;  //number of data bytes in the frame
           public byte type; //message type (used to run Type Listeners, pick any values you like)
           
        }

        //数据解析
        public void AcceptByte(byte data, SensorDataItem lkSensor)
        {
            frameBuf.Enqueue(data);
            switch (state)
            {
                case "sof":
                    {
                        if(data == sofByte)
                        {
                            beginFrame();
                            headBuf.Enqueue(data);
                        }

                    }break;
                case "id":
                    {
                        headBuf.Enqueue(data);
                        msg.frame_id = (byte)((msg.frame_id << 8) | data);
                        if(++partlen == idSize)
                        {   
                          state = "len";
                          partlen = 0;
                        }
                    }
                    break;
                case "len":
                    {
                        
                        headBuf.Enqueue(data);
                        msg.len = (UInt16)((msg.len << 8) | data);
                        if (++partlen == lenSize)
                        {

                            state = "type";
                            partlen = 0;
                        }
                    }
                    break;
                case "type":
                    {
                        headBuf.Enqueue(data);
                        msg.type = (byte)((msg.type << 8) | data);
                        if(++partlen == typeSize)
                        {
                            state = "headcksum";
                            partlen = 0;
                        }
                    }break;
                case "headcksum":
                    {
                        cksum = (cksum << 8) | data;
                        if (++partlen == check_crc.size)
                        {
                            byte[] buf = headBuf.ToArray();
                            UInt32 crc = 0;
                            check_crc.crcReset();
                            crc = check_crc.crc16(buf);
                            if (crc == cksum)
                            {
                                if (msg.len == 0)  //无数据包
                                {
                                    resetParser();
                                    lkSensor.isReceveSucceed = true;
                                    lkSensor.Frame = lkSensor.fromHexToString(headBuf.ToArray());
                                    lkSensor.buf = dataBuf.ToArray();
                                    handleReceived(lkSensor);
                                }
                                else
                                {
                                    state = "data";
                                    partlen = 0;
                                }
                            }
                            else
                            {
                                resetParser();
                                break;
                            }
                        }
                       
                    }
                    break;
                case "data":
                    {
                        dataBuf.Enqueue(data);
                        if (++partlen == msg.len)
                        {
                            state = "datachksum";
                            partlen = 0;
                            cksum = 0;
                        }
                    }break;
                case "datachksum":
                    {
                        cksum = (cksum << 8) | data;
                        if(++partlen == check_crc.size)
                        {
                            byte[] buf = dataBuf.ToArray();
                            UInt16 crc = 0;
                            check_crc.crcReset();
                            crc = check_crc.crc16(buf);
                            if (crc == cksum)
                            {    
                                lkSensor.isReceveSucceed = true;
                                lkSensor.Frame = lkSensor.fromHexToString(frameBuf.ToArray());
                                lkSensor.buf= dataBuf.ToArray();
                                handleReceived(lkSensor);
                            }
                            else
                            {
                                lkSensor.isReceveSucceed = false;
                            }
                            cksum = 0;
                            partlen = 0;
                            dataBuf.Clear();
                            resetParser();
                        }
                    }break;
            }
            
        }
        //添加回调函数
        public void handleReceived(SensorDataItem lkSensor )
        {
            if (lkSensor.isReceveSucceed)
            {
                lkSensor.id = msg.frame_id;
                lkSensor.type = msg.type;
                lkSensor.len = msg.len;
                if (msg.frame_id == idListenr.id)
                {
                    idListenr.Func.Invoke( lkSensor);     //回调函数运行
                }
                for (int i = 0; i < MAX_TYPE_SIZE; i++)
                {
                    if(msg.type== typeListener[i].type)
                    {
                        typeListener[i].Func.Invoke( lkSensor);
                    }
                }
                for (int i = 0; i < MAX_GENERAL_SIZE; i++)
                {
                    if(generalListenr[i].Func == null)
                    {
                        return;
                    }
                    else
                    {
                        generalListenr[i].Func.Invoke( lkSensor);
                    }
                }
            }


        }
        //添加id listen
        struct _idListenr
        {
            public int id;
            public myDelegate Func;
        };

        struct _generalListenr
        {
            public myDelegate Func;
        };
        struct _typeListenr
        {
            public int type;
            public myDelegate Func;
        };

        public  delegate void myDelegate(SensorDataItem lkSensor);
        public void addIDlistener(int _id, myDelegate listener)
        {
            idListenr.id = _id;
            idListenr.Func = listener;
        }
        public void addGenralListener( myDelegate listener)
        {
            for (int i = 0; i < MAX_GENERAL_SIZE; i++)
            {
                if (generalListenr[i].Func == null)
                {
                    generalListenr[i].Func = listener;
                    return;
                }

            }

        }

        public void addTYPElistener(int _type, myDelegate listener)
        {
            
            for (int i = 0; i < MAX_TYPE_SIZE; i++)
            {
                if(typeListener[i].Func == null)
                {
                    typeListener[i].type = _type;
                    typeListener[i].Func = listener;
                    return;
                }

            }
        }

        public void beginFrame()
        {
            this.state = "id";
            partlen = 0;
            msg.frame_id = 0;
            msg.len = 0;
            msg.type = 0;
            this.cksum = 0;
        }
    }
   public class checksum
    {
         UInt16[] CRC16_TABLE = {
    0x0000, 0xC0C1, 0xC181, 0x0140, 0xC301, 0x03C0, 0x0280, 0xC241,
    0xC601, 0x06C0, 0x0780, 0xC741, 0x0500, 0xC5C1, 0xC481, 0x0440,
    0xCC01, 0x0CC0, 0x0D80, 0xCD41, 0x0F00, 0xCFC1, 0xCE81, 0x0E40,
    0x0A00, 0xCAC1, 0xCB81, 0x0B40, 0xC901, 0x09C0, 0x0880, 0xC841,
    0xD801, 0x18C0, 0x1980, 0xD941, 0x1B00, 0xDBC1, 0xDA81, 0x1A40,
    0x1E00, 0xDEC1, 0xDF81, 0x1F40, 0xDD01, 0x1DC0, 0x1C80, 0xDC41,
    0x1400, 0xD4C1, 0xD581, 0x1540, 0xD701, 0x17C0, 0x1680, 0xD641,
    0xD201, 0x12C0, 0x1380, 0xD341, 0x1100, 0xD1C1, 0xD081, 0x1040,
    0xF001, 0x30C0, 0x3180, 0xF141, 0x3300, 0xF3C1, 0xF281, 0x3240,
    0x3600, 0xF6C1, 0xF781, 0x3740, 0xF501, 0x35C0, 0x3480, 0xF441,
    0x3C00, 0xFCC1, 0xFD81, 0x3D40, 0xFF01, 0x3FC0, 0x3E80, 0xFE41,
    0xFA01, 0x3AC0, 0x3B80, 0xFB41, 0x3900, 0xF9C1, 0xF881, 0x3840,
    0x2800, 0xE8C1, 0xE981, 0x2940, 0xEB01, 0x2BC0, 0x2A80, 0xEA41,
    0xEE01, 0x2EC0, 0x2F80, 0xEF41, 0x2D00, 0xEDC1, 0xEC81, 0x2C40,
    0xE401, 0x24C0, 0x2580, 0xE541, 0x2700, 0xE7C1, 0xE681, 0x2640,
    0x2200, 0xE2C1, 0xE381, 0x2340, 0xE101, 0x21C0, 0x2080, 0xE041,
    0xA001, 0x60C0, 0x6180, 0xA141, 0x6300, 0xA3C1, 0xA281, 0x6240,
    0x6600, 0xA6C1, 0xA781, 0x6740, 0xA501, 0x65C0, 0x6480, 0xA441,
    0x6C00, 0xACC1, 0xAD81, 0x6D40, 0xAF01, 0x6FC0, 0x6E80, 0xAE41,
    0xAA01, 0x6AC0, 0x6B80, 0xAB41, 0x6900, 0xA9C1, 0xA881, 0x6840,
    0x7800, 0xB8C1, 0xB981, 0x7940, 0xBB01, 0x7BC0, 0x7A80, 0xBA41,
    0xBE01, 0x7EC0, 0x7F80, 0xBF41, 0x7D00, 0xBDC1, 0xBC81, 0x7C40,
    0xB401, 0x74C0, 0x7580, 0xB541, 0x7700, 0xB7C1, 0xB681, 0x7640,
    0x7200, 0xB2C1, 0xB381, 0x7340, 0xB101, 0x71C0, 0x7080, 0xB041,
    0x5000, 0x90C1, 0x9181, 0x5140, 0x9301, 0x53C0, 0x5280, 0x9241,
    0x9601, 0x56C0, 0x5780, 0x9741, 0x5500, 0x95C1, 0x9481, 0x5440,
    0x9C01, 0x5CC0, 0x5D80, 0x9D41, 0x5F00, 0x9FC1, 0x9E81, 0x5E40,
    0x5A00, 0x9AC1, 0x9B81, 0x5B40, 0x9901, 0x59C0, 0x5880, 0x9841,
    0x8801, 0x48C0, 0x4980, 0x8941, 0x4B00, 0x8BC1, 0x8A81, 0x4A40,
    0x4E00, 0x8EC1, 0x8F81, 0x4F40, 0x8D01, 0x4DC0, 0x4C80, 0x8C41,
    0x4400, 0x84C1, 0x8581, 0x4540, 0x8701, 0x47C0, 0x4680, 0x8641,
    0x8201, 0x42C0, 0x4380, 0x8341, 0x4100, 0x81C1, 0x8081, 0x4040
        };
        public byte size;
        public checksum()
        {
            size = 2;  //crc32
        }
        UInt16 crc = 0;
        public void crcReset()
        {
            crc = 0xffff;
        }
        public UInt16 crc16(byte[] data)
        {
            UInt16 iCount = (UInt16)(data.Length);
            UInt16 tableIndex = 0;
            for (int i = 0; i < iCount; i++)
            {
                tableIndex = (UInt16)((crc & 0xff) ^ (data[i] & 0xff));
                crc = (UInt16)((crc >>8) ^ CRC16_TABLE[tableIndex]);
            }
            return crc;
        }
    }

    //发送帧
    class sendFrame
    {

        byte next_id;
        byte id;
        byte sofbyte = 0x01;
        public delegate void myDelegate(byte[] buf);
        public Queue<byte> sendBuf = new Queue<byte>();
        checksum sendChecksum = new checksum();

       public byte[] sendFrame_compend(sendDataitem sendMsg)
        {   
   
            id = sendMsg.isRsponse ? sendMsg.id : getNextID();
            //add listener       
            sendBuf.Enqueue(sofbyte);
            sendBuf.Enqueue(id);
            sendBuf.Enqueue((byte)(sendMsg.len>>8));
            sendBuf.Enqueue((byte)(sendMsg.len));
            sendBuf.Enqueue(sendMsg.Type);
            sendChecksum.crcReset();
            UInt16 head_crc = sendChecksum.crc16(sendBuf.ToArray());
            byte high_crc = (byte)(head_crc >> 8);
            byte low_crc = (byte)head_crc;
            sendBuf.Enqueue(high_crc);
            sendBuf.Enqueue(low_crc);
            if(!sendMsg.ifHeadOnly)
            {
                byte[] data = new byte[sendMsg.len];
                for (int i = 0; i < sendMsg.len; i++)
                {
                    data[i] = sendMsg.sendbuf[i];
                    sendBuf.Enqueue(sendMsg.sendbuf[i]);
                }
                sendChecksum.crcReset();
                UInt16 data_crc = sendChecksum.crc16(data);
                byte high_data_crc = (byte)(data_crc >> 8);
                byte low_data_crc = (byte)data_crc;

                sendBuf.Enqueue(high_data_crc);
                sendBuf.Enqueue(low_data_crc);
            }
            else
            {
                sendMsg.len = 0;
            }
            byte[] re_buf = sendBuf.ToArray();
            sendBuf.Clear();
            return re_buf;
        }

        public byte getNextID()
        {
            if(next_id>=125)
            {
                next_id = 0;
            }
           return ++next_id;
        }
    }


}





