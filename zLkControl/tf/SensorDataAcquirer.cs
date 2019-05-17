using System;
using System.Collections.Generic;
using System.IO.Ports;
namespace zLkControl
{
   public class SensorDataAcquirer
    {
        private SerialPort zSerPort;
        private long SensorDataFrmaegCounter;  //记录接收数据帧计数
        private Queue<SensorDataItem> SensorDataFrmaeBuffer = new Queue<SensorDataItem>(); // 帧缓存
        public Queue<byte> SerialPortReadBuffer = new Queue<byte>();
        public sensorDataChangedHandler SensorDataChangedEvent;
        public delegate void sensorDataChangedHandler(byte[]buff);
        public object LockThis = new object();
        public thinyFrame lkFrame = new thinyFrame(1);
        sendFrame lk_sendHandle = new sendFrame();
        SensorDataItem lkSensor = new SensorDataItem();
        /*初始化串口，新建一个串口*/
        public void IniteSerial(string comport, int baudrate)
        {
            zSerPort = new SerialPort(comport, baudrate);
            zSerPort.DataReceived += SerialPortDataReceived;
        }

        /*
         * 串口接收事件
         */
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
                        SerialPortReadBuffer.Enqueue(item);  //添加接收的数据到缓存末尾
                    }
                    byte[] revBuf = SerialPortReadBuffer.ToArray(); //将缓存转换字节数组
                    if (SerialPortReadBuffer.Count == 8)
                    {
                        SensorDataChangedEvent(buf);
                        SerialPortReadBuffer.Clear();  //清除缓存数据
                    }

                    
                    //for (int i = 0; i < revBuf.Length; i++)  //协议解析
                    //{
                    //    lkFrame.AcceptByte(revBuf[i], lkSensor);
                    //}
                   
                    if (lkSensor.isReceveSucceed)
                    {
                        ProcessSensorDataItem(lkSensor);  //处理完成后回调函数
                    }
                }

            }

        }
        private void ProcessSensorDataItem(SensorDataItem sensorDataItem)
        {
            SensorDataFrmaegCounter++;

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


    }
}
