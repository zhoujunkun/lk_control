using System;
using System.Collections.Generic;
using System.IO.Ports;
namespace zLkControl
{
    class SensorDataAcquirer
    {
        private SerialPort zSerPort;
        private long SensorDataFrmaegCounter;  //记录接收数据帧计数
        private Queue<SensorDataItem> SensorDataFrmaeBuffer = new Queue<SensorDataItem>(); // 帧缓存
        public Queue<byte> SerialPortReadBuffer = new Queue<byte>();
        public SensorDataAcquirer.SensorDataChangedHandler SensorDataChangedEvent;
        public delegate void SensorDataChangedHandler(SensorDataItem sensorDataItem, long counter);
        public object LockThis = new object();
        public thinyFrame lkFrame = new thinyFrame(1);
        sendFrame lk_sendHandle = new sendFrame();
        SensorDataItem lkSensor = new SensorDataItem();
        public void IniteSerial(string comport, int baudrate)
        {
            zSerPort = new SerialPort(comport, baudrate);
            zSerPort.DataReceived += SerialPortDataReceived;
        }
        frameAnysDeleg frameFucn;

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if(check())
           {
                int sizes = zSerPort.BytesToRead;
                byte[] buf = new byte[sizes];
                zSerPort.Read(buf, 0, sizes);
                object lockThis = LockThis;
                lock (lockThis)
                {
                    foreach (byte item in buf)
                    {
                        SerialPortReadBuffer.Enqueue(item);
                    }
                    byte[] revBuf = SerialPortReadBuffer.ToArray();
                    //if(revBuf[revBuf.Length-1] ==0xff)
                    //{
                    //     frameFucn.Invoke(revBuf);
                    //     SerialPortReadBuffer.Clear();
                    //}

                    for (int i = 0; i < revBuf.Length; i++)
                    {
                        lkFrame.AcceptByte(revBuf[i], lkSensor);
                    }
                    SerialPortReadBuffer.Clear();
                    if (lkSensor.isReceveSucceed)
                    {
                        ProcessSensorDataItem(lkSensor);  //处理完成后回调函数
                    }
                }

            }

        }
        public delegate void frameAnysDeleg(byte[] buf);

        public void addFrameAnys(frameAnysDeleg fuc)
        {
            frameFucn = fuc;
        }
        private SensorDataItem ExtractSensorDataFromString(byte[] lineInBuffer)
        {
            SensorDataItem result = null;

            return result;
        }
        private void ProcessSensorDataItem(SensorDataItem sensorDataItem)
        {
            SensorDataFrmaegCounter++;
            SensorDataChangedEvent(sensorDataItem, SensorDataFrmaegCounter);
        }
        private void OnSensorDataChangedEvent(SensorDataItem sensorDataItem, long counter)
        {

        }
        public void AddItemToRecordingFrmaeBuffer(SensorDataItem sensorDataItem)
        {
            SensorDataFrmaeBuffer.Enqueue(sensorDataItem);

        }
        public bool Start()
        {
            bool result;
            try
            {
                zSerPort.Open();
                result = true;
            }
            catch (Exception)
            {
                result = false;
            }

            return result;
        }

        //检查串口是否打开
        public bool check()
        {
            bool result;
            try
            {
                if(zSerPort.IsOpen)
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex, "");
                result = false;
            }
            return result;
        }
        public void SendMsg(sendDataitem sendMsg)
        {

            if (check())
            {
                object lockThis = LockThis;
                lock(lockThis)
                {
                    sendMsg.sendFrame = lk_sendHandle.sendFrame_compend(sendMsg);
                    int send_lens = sendMsg.sendFrame.Length;
                    zSerPort.Write(sendMsg.sendFrame, 0, send_lens);
                }
            }
               

        }
        public void SendCmd(byte[] cmd)
        {

        }
        public void SendCmd(byte[] cmd, byte par1, byte par2)
        {

        }

        public string GetPort()
        {
           return zSerPort.PortName;
        }
        public void Close()
        {
            zSerPort.Close();
        }

        public void EmptyBuffer()
        {
            zSerPort.DiscardInBuffer();
            SerialPortReadBuffer.Clear();
        }

        private void SaveRecordData(string fileName)
        {

        }
        private bool CompareBytesAry(byte[] source, byte[] refe)
        {
            return true;
        }
        public static byte[] ComputeCRC(byte[] _Data)
        {
            byte[] tmpData = new byte[_Data.Length - 4];
            for (int i = 0; i < _Data.Length - 4; i++)
            {
                tmpData[i] = _Data[i];
            }
            uint crc = uint.MaxValue;
            for (int j = 0; j < _Data.Length / 4 - 1; j++)
            {
                uint tmp = BitConverter.ToUInt32(tmpData, 4 * j);
                for (int k = 0; k < 32; k++)
                {
                    if (((crc ^ tmp) & 2147483648u) != 0u)
                    {
                        crc = (79764919u ^ crc << 1);
                    }
                    else
                    {
                        crc <<= 1;
                    }
                    tmp <<= 1;
                }
            }
            byte[] CRCByte = BitConverter.GetBytes(crc);
            return new byte[]
            {
                CRCByte[0],
                CRCByte[1],
                CRCByte[2],
                CRCByte[3]
            };
        }



    }
}
