using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Windows.Controls;

namespace ZSeial
{
    class z_serial
    {
        private SerialPort zSerPort;
        public object LockThis = new object();
        long dataCounts { set; get; }
        public delegate void serialDataChangeHandle(byte[] buff);
        serialDataChangeHandle func_serial;
        public  z_serial(string comport, int baudrate)
        {
            zSerPort = new SerialPort(comport, baudrate);
            zSerPort.DataReceived += SerialPortDataReceived;
        }
        public void changedBaud(int baud)
        {
            zSerPort.BaudRate = baud;
        }

        public void SerialPinChanged(serialDataChangeHandle func)
        {
            func_serial = func;
        }

        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int sizes = zSerPort.BytesToRead;
            byte[] buf = new byte[sizes];
            zSerPort.Read(buf, 0, sizes);
            object lockThis = LockThis;
            lock (lockThis)
            {
                func_serial(buf);
            }
        }
        /// <summary>
        /// Combox 控制显示默认波特率{9600, 14400, 19200, 38400, 57600, 115200}
        /// </summary>
        /// <param name="sender"></param>
        public void defConfigBaudRate(object sender)
        {
            ComboBox box = (ComboBox)sender;
            int[] bauds = new int[] { 9600, 14400, 19200, 38400, 57600, 115200 };
            for (int i = 0; i < bauds.Length; i++)
            {
                object baud = bauds[i].ToString();
                box.Items.Add(baud);
            }

        }
        public void addListener()
        {

        }
       /// <summary>
       ///  检查串口是否打开
       /// </summary>
        public bool check()
        {
            bool result;
            try
            {
                if (zSerPort.IsOpen)
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
                result = false;
            }
            return result;
        }
        /// <summary>
        /// 发送串口数据
        /// </summary>
        /// <param name="buff"></param>
        public void sendDATA(byte[]buff)
        {
            if(check())
            {
                object lockThis = LockThis;
                lock(lockThis)
                {
                    zSerPort.Write(buff,0, buff.Length);
                }
            }
        }
        /// <summary>
        /// 关闭串口
        /// </summary>
        public void Close()
        {
            zSerPort.Close();
        }
        /// <summary>
        /// 串口启动
        /// </summary>
        /// <returns></returns>
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


    }
}
