using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace zLkControl
{
    /// <summary>
    /// Setting.xaml 的交互逻辑
    /// </summary>
    public partial class Setting : MetroWindow, IComponentConnector
    {
        internal Setting settingWindow = null;
        SensorDataAcquirer serial;
        public bool ifBeginUpdata { set; get; }
        
        public Setting(MainWindow MyMainWindow)
        {
            serial = MyMainWindow.Lk_Serial;
            
            this.InitializeComponent();
        }
        
        private void Limit_Dist_PamClick(object sender, RoutedEventArgs e)
        {
            
            MessageBoxResult dr = MessageBox.Show("是否配置触发距离" + (string)sliderDist.Value.ToString() + "m", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (dr == MessageBoxResult.OK)
            {

            }
            else
            {     
                sliderDist.Value = lk_param.lk_parm_s.limit_dist;
            }
        }

        private void textBox_Enter(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                sliderDist.Value = int.Parse(textBox_LimitTrig.Text);

                MessageBoxResult dr = MessageBox.Show("是否配置触发距离" + textBox_LimitTrig.Text + "m", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {

                }
                else
                {
                    sliderDist.Value = lk_param.lk_parm_s.limit_dist;
                }
            }
        }

        private void baudInit(object sender, EventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            int[] bauds = new int[] { 9600, 14400, 19200, 38400, 57600, 115200 };
            for (int i = 0; i < bauds.Length; i++)
            {
                object baud = bauds[i].ToString();
                box.Items.Add(baud);
            }
        }

        private void buadMousedown(object sender, MouseButtonEventArgs e)
        {
          /*  MessageBoxResult dr = MessageBox.Show(Application.Current.MainWindow, "是否确定波特率" + (string)BaudRateParm.SelectedItem, "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);

            if (dr == MessageBoxResult.OK)
            {
                if (Lk_Serial.check())
                {
                    send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParmsSave);
                    send_msg.id = (byte)(LKSensorCmd.FRAME_ParmSaveID.BarudSave);
                    send_msg.ifHeadOnly = false;  //含有数据帧
                    int baud = int.Parse((string)BaudRateParm.SelectedItem);
                    send_msg.sendbuf = BitConverter.GetBytes(baud);    //数据帧缓存
                    send_msg.len = LKSensorCmd.parmBarudRateByteSize; //数据帧字节长度
                    Lk_Serial.SendMsg(send_msg);
                }
            }
            else
            {
                BaudRateParm.SelectionChanged -= Baud_Rate_MoseDown;
                BaudRateParm.SelectedIndex = LKSensorCmd.parmBarudIndex;
                BaudRateParm.SelectionChanged += Baud_Rate_MoseDown;
            }
            */
        }
        string filePath, fileName;
        UInt16 fileSize;
        byte[] binBuf;  //bin文件存放
        UInt16 binCrc;  //binCrc校验码
        int packedSize = 1024; //包大小1024
        VersionApp versionApp;
        private void openFileUpdataClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog(); 
            ofd.DefaultExt = ".zmc";

            ofd.Filter = "zjk@MaiCe 固件|*.zmc";

            if (ofd.ShowDialog() == true)
            {
                filePath = ofd.FileName;
                fileName = texbockFileName.Text = ofd.SafeFileName;
                fileStream = new FileStream(@filePath, FileMode.Open, FileAccess.Read);
                BinaryReader fileRead = new BinaryReader(fileStream);
                byte[] jsonLenBuf = fileRead.ReadBytes(2);
                int jsonLenghts = jsonLenBuf[0] << 8 | jsonLenBuf[1];
                byte[] jsonBuf = fileRead.ReadBytes(jsonLenghts);
                string jsonCfg = Encoding.Default.GetString(jsonBuf);
                versionApp = JsonConvert.DeserializeObject<VersionApp>(jsonCfg);
                binBuf = fileRead.ReadBytes(versionApp.Filesize);
                crc16Module crc_check = new crc16Module();
                binCrc = crc_check.crc16(binBuf);
                fileSize = Convert.ToUInt16(versionApp.Filesize);
                //Match mc = Regex.Match(versionApp.TimeCreate, @"\d{4}/\d{1,2}/\d{1,2}");
                //string time = mc.ToString().Replace("/","");
                textBlockFileSize.Text = versionApp.Filesize.ToString();
                if (binCrc == versionApp.Crc16Modulbus)
                {
                    MessageBoxResult dr = MessageBox.Show("文件校验成功，点击确认升级", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                    if (dr == MessageBoxResult.OK)
                    {
                        btn_upload();
                    }
                }
                else
                {
                    MessageBox.Show("文件校验失败。。。");
                }
        
                //此处做你想做的事 ...=ofd.FileName; 

            }

        }
        /// <summary>
        /// 重写OnClosing事件 解决窗口关闭不能再开的bug。
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            e.Cancel = true;
        }
        System.Timers.Timer timerTOA;
        FileStream fileStream;
        int packetCnt = 0;
        private void btn_upload()
        {
            if(serial.check())
            {

                timerTOA = new System.Timers.Timer();
                timerTOA.Interval = 2000; //200ms超时
                timerTOA.AutoReset = false;
                timerTOA.Enabled = false;
                timerTOA.Elapsed += new System.Timers.ElapsedEventHandler(endtime);            
                packetCnt = (fileSize % packedSize) == 0 ? (int)(fileSize / packedSize) : (int)(fileSize / packedSize) + 1;
                progressUpload.Maximum = packetCnt;
                sendAck();
                timerTOA.Start(); //定时器用于超时处理
            }

        }
        enum Package_enum_ { firstPackage=1,dataPackaged};
        Package_enum_ package_statu = Package_enum_.firstPackage;
        /// <summary>
        /// 时间结束时触发的方法
        /// </summary>
        private void endtime(Object sender, EventArgs e)
        {
            MessageBox.Show("time out");
        }
        /// <summary>
        /// 首包帧
        /// </summary>
        /// <returns></returns>
        private byte[] packageBegin()
        {
           
            byte[] firstPackage = new byte[versionApp.TimeCreate.Length+5];   //time+size(2)+crc(file 2)
            /*add file name to first package*/
            for (int i = 0; i < versionApp.TimeCreate.Length; i++)
            {
                firstPackage[i]= (byte)versionApp.TimeCreate.ToCharArray()[i];
            }
            firstPackage[versionApp.TimeCreate.Length + 1] = (byte)(fileSize >> 8);
            firstPackage[versionApp.TimeCreate.Length + 2] = (byte)(fileSize &0xff);
            firstPackage[versionApp.TimeCreate.Length + 3] = (byte)(binCrc >> 8);
            firstPackage[versionApp.TimeCreate.Length + 4] = (byte)(binCrc & 0xff);
            sendPakagedFrame(firstPackage, 0);
            return firstPackage;
        }
        int packageCnt = 0;
        int packageCntTrans = 0;
        private bool packageSend()
        {
            /* data: 1024 bytes */
            byte[] packaedTosend;
            /* send packets with a cycle until we send the last byte */
            int filePackedSize = binBuf.Length - packageCntTrans++ * packedSize;
            if (packageCntTrans == (packetCnt + 1))
                return false;
            if (filePackedSize < packedSize)   //小于packedSize 
            {
                packaedTosend = new byte[filePackedSize];
                Array.Copy(binBuf, packedSize* (packageCntTrans - 1), packaedTosend, 0, filePackedSize);
            }
            else
            {
                packaedTosend = new byte[packedSize];
                Array.Copy(binBuf, packedSize* (packageCntTrans - 1), packaedTosend, 0, packedSize);
            }

            /* calculate packetNumber */
            progressBar(packageCntTrans);
            sendPakagedFrame(packaedTosend,(byte)packageCntTrans);
            return true;
        }

        private void sendPakagedFrame(byte[] buff, byte id)
        {
            sendDataitem sendFrame = new sendDataitem();
            sendFrame.Type = (byte)(LKSensorCmd.FRAME_TYPE.Upload);
            sendFrame.id = id;
            sendFrame.ifHeadOnly = false;  //含有数据帧
            sendFrame.sendbuf = buff;
            sendFrame.len = (UInt16)buff.Length; //数据帧字节长度
            serial.SendMsg(sendFrame);
        }

        private void sendAck()
        {
            sendDataitem sendFrame = new sendDataitem();
            sendFrame.Type = (byte)(LKSensorCmd.FRAME_TYPE.ACK);
            sendFrame.id = (byte)(LKSensorCmd.FRAME_AckID.downLoadBegin);
            sendFrame.ifHeadOnly = true;  //含有数据帧
            serial.SendMsg(sendFrame);
        }
        public void ackCallback(LKSensorCmd.FRAME_AckID ackID)
        {
            LKSensorCmd.FRAME_AckID _ack = ackID;
            switch (_ack)
            {
                case LKSensorCmd.FRAME_AckID.downLoadBegin:
                    {
                        ifBeginUpdata = true;
                        timerTOA.Stop();
                        switch (package_statu)
                        {
                            case Package_enum_.firstPackage:
                                {
                                    packageCntTrans = 0;
                                    packageBegin();
                                    package_statu = Package_enum_.dataPackaged;
                                }
                                break;
                            case Package_enum_.dataPackaged:
                                {
                                   if( packageSend() == true)
                                    {
                                       
                                        timerTOA.Start();
                                    }
                                   else
                                    {
                                        MessageBox.Show("send succeed");
                                    }
                                }
                                break;
                        }
                    }
                    break;
                case LKSensorCmd.FRAME_AckID.upload:
                    {

                    }
                    break;
            }
        }
        //进度条
        private void progressBar(int count)
        {
            Thread thread = new Thread(new ThreadStart(() =>   //多线程
            {
               progressUpload.Dispatcher.BeginInvoke((ThreadStart)delegate
               {
                        progressUpload.Value = count;
                });
            }));
            thread.Start();
        }
        public class VersionApp
        {
            public string Name { set; get; }
            public string Version { set; get; }
            public int Filesize { set; get; }
            public string TimeCreate { set; get; }
            public int Crc16Modulbus { set; get; }
        }

    }

}
