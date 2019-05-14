using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
namespace ZSeial
{
    class z_serial
    {
        private SerialPort zSerPort;

        long dataCounts { set; get; }
        public  z_serial(string comport, int baudrate)
        {
            zSerPort = new SerialPort(comport, baudrate);
            zSerPort.DataReceived += SerialPortDataReceived;
        }
        private void SerialPortDataReceived(object sender, SerialDataReceivedEventArgs e)
        {

        }
        //检查串口是否打开
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
        public void sendDATA(byte[]buff)
        {

        }

    }
}
