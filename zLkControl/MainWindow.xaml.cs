using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO.Ports;
using System.Management;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Windows.Media;
using zLkControl.Dynamicdisplay;
using System.Linq;
[assembly: SuppressIldasm()]
namespace zLkControl
{
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    /// 
    public partial class MainWindow : MetroWindow
    {
        private dyDisplay dd = new dyDisplay();
        private dyDisplay sighal_dd = new dyDisplay();
        private dyDisplay Gain603_dd = new dyDisplay();
        private LineGraph chart;
        //QC
        LK03QC lk03_qc = new LK03QC();
        //正则匹配
        Regex reg = new Regex(@"COM[0-9]*");  //正则表达式提取COM
        //tf
        float dist;  //距离值
        private info infoWin;
        private Setting settingWin;
        public SensorDataAcquirer Lk_Serial = new SensorDataAcquirer();
        LKSensorCmd LKSensorCmd = new LKSensorCmd();
        NotifyBase models = new NotifyBase();
        private System.Timers.Timer timerEftPtsCounter;
        private int eftPtsPerSec;
        private bool flagSP;
        private string strPort = string.Empty;   //端口号
        int txCunt;  //发送计数
        public String[] Product = { "lk02", "lk03" };
        //serial
        SerialPort zjkPort;
        private string[] ports;
        private string receiveData;
        public bool isConnected = false;
        int count_texbox; //显示帧数
        //cmd
        public sendDataitem send_msg = new sendDataitem();
        //lk
        public MainWindow()
        {
            InitializeComponent();
            base.Loaded += new RoutedEventHandler(this.Window_Loaded);
            DataContext = lk03_qc;
        }
    
        //lk general listenr
        void genralFunc( SensorDataItem sensor)
        {
            byte[] lkData = sensor.buf;
            LKSensorCmd.FRAME_TYPE frame_type = (LKSensorCmd.FRAME_TYPE)(sensor.type);
            switch (frame_type)
            {
                case LKSensorCmd.FRAME_TYPE.DataGet:
                    {

                    }break;
                case LKSensorCmd.FRAME_TYPE.ParmsSave:
                    {

                    }
                    break;
                case LKSensorCmd.FRAME_TYPE.ParamGet:
                    {

                    }
                    break;
                case LKSensorCmd.FRAME_TYPE.Upload:
                    {

                    }
                    break;
                case LKSensorCmd.FRAME_TYPE.ACK:
                    {
                        LKSensorCmd.FRAME_AckID ack_id = (LKSensorCmd.FRAME_AckID)(sensor.id);
                        settingWin.ackCallback(ack_id);
                    }
                    break;
                case LKSensorCmd.FRAME_TYPE.QC:
                    { 
                        LKSensorCmd.FRAME_QCcmdID qc_id = (LKSensorCmd.FRAME_QCcmdID)(sensor.id);
                        QC_ID_Func(qc_id, lkData);
                    }
                    break;
                case LKSensorCmd.FRAME_TYPE.Erro:
                    {

                    }
                    break;
                default:
                    break;
            }

        }
        static int size_stand = 3;
       
        /// <summary>
        /// QC 接收处理函数，
        /// </summary>
        /// <param name="id">传入id</param>
        /// <param name="buff">数据</param>
        public void QC_ID_Func(LKSensorCmd.FRAME_QCcmdID id,byte [] buff)
        {
            UInt16[] test = new UInt16[size_stand * 3];   //保存获取到的标定数据
            switch(id)
            {
                case LKSensorCmd.FRAME_QCcmdID.GetParam:
                    {
                        for(int i=0;i< size_stand; i++)
                        {
                            test[0 + i * size_stand] = (UInt16)(buff[0+ i * 5] << 8 | buff[1+ i * 5]);
                            test[1 + i * size_stand] = (UInt16)(buff[2+ i * 5] << 8 | buff[3+ i * 5]);
                            test[2 + i * size_stand] = (UInt16)(buff[4+ i * 5]);
                            lk_distCalibration[i] = test[0+i*3];
                            lk_gain[i] = test[1+ i * 3];
                            lk_ifLkHavedStand[i] = (test[2+ i * 3] == 1);
                        }
                    }
                    break;
            }
        }
     
        const int LK03_DISPLAY_TYPE = (int)(LKSensorCmd.FRAME_TYPE.DataGet);
        const int LK03_QC_TYPE = (int)(LKSensorCmd.FRAME_TYPE.QC);
        const int LK03_PARM_TYPE = (int) (LKSensorCmd.FRAME_TYPE.ParamGet);
        int[] distBuf = new int[100];
        int[] gainBuf = new int[100];
        private UInt16[] lk_distCalibration = new UInt16[3] ; //标定完校准
        private UInt16[] lk_gain = new UInt16[3] ;  //标定完当前距离的增益
        private UInt16[] lk_average = new UInt16[3];  //平均值
        private bool[] lk_ifLkHavedStand = new bool[3];   //是否已经标定过
        /// <summary>
        /// lk type listenr
        /// </summary>
        /// <param name="sensor"></param>
        void typefunc( SensorDataItem sensor)
        {
            byte[] lkData = sensor.buf;
            eftPtsPerSec++;
            base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
            {
                UInt16 signal_vol = (UInt16)( lkData[0] << 8 | lkData[1]);
                UInt16 dist = (UInt16)(lkData[2] << 8 | lkData[3]);
                UInt16 ad603_agc = (UInt16)(lkData[4] << 8 | lkData[5]);
                this.ddCounter++;
                int xax = ddCounter - 100;
                if (xax <= 0)
                {
                    xax = 0;
                }
                // plotterTimeLine.Viewport.Visible = new System.Windows.Rect(xax, 0, 50, data.dist+1000);
                //  plotterTimeLine.Viewport.Width = (xax);
                this.dd.Add(new Points((uint)this.ddCounter, dist));
                this.sighal_dd.Add(new Points((uint)this.ddCounter, signal_vol));
                Gain603_dd.Add(new Points((uint)this.ddCounter, ad603_agc));
                DistTextBlock.Text = dist.ToString();
                if(ifStandDistStart)
                {
                    for (int i = 0; i < 100; i++)
                    {
                        distBuf[i] = (int)dd[i].Value;
                        gainBuf[i] = (int)Gain603_dd[i].Value;
                    }
                    ifStandDistStart = false;
                    if(lk03_qc.ifRadioFirstStand)
                    {
                        convertAnddisplay(STAND_INDEX.First_stand);
                    } else if(lk03_qc.ifRadioSecondStand)
                    {
                        convertAnddisplay(STAND_INDEX.Second_stand);
                    }
                    else if(lk03_qc.ifRadioThirdStand)
                    {
                        convertAnddisplay(STAND_INDEX.Third_stand);
                    }

                }    
            }), new object[0]);
        }
        public void QC_func(SensorDataItem sensor)
        {

        }
      public enum STAND_INDEX { First_stand=0,Second_stand,Third_stand} ;   //档位选择
        /// <summary>
        /// 三挡选择和对应数据显示
        /// </summary>
        /// <param name="select">档位选择 STAND_INDEX</param>
        public void  convertAnddisplay(STAND_INDEX select)
        {
            byte index = (byte)select;
            lk_average[index] = (UInt16)data_standValculate(distBuf);
            lk_gain[index] = (UInt16)data_standValculate(gainBuf);
            lk_distCalibration[index] = (UInt16)(lk_average[index] - stand_slider.Value * 100);
            textBox_calibration.Text = lk_distCalibration[index].ToString(); // 校准值显示
            textBox_average.Text = lk_average[index].ToString();  //平均值显示  
            textBox_gain.Text = lk_gain[index].ToString();//增益值显示
         }

        /// <summary>
        /// 标定数据计算
        /// </summary>
        /// <param name="buf"></param>
     private double data_standValculate(int [] arry)
        {
            Array.Sort(arry);  //排序
            return arry.Average();
        }
        /// <summary>
        /// 开机参数显示
        /// </summary>
        /// <param name="sensor"></param>
        void ParmaRevFunc(SensorDataItem sensor)
        {
            byte[] lkData = sensor.buf;
            
            lk_param.lk_parm_s = sensor.structHelper.ByteToStruct<lk_param.Param_>(sensor.buf);
            base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
            {
                string baud= lk_param.lk_parm_s.baud_rate.ToString();
                ledStatuShake();   //提示参数接收指示灯
                //波特率
                settingWin.sliderDist.Value = lk_param.lk_parm_s.limit_dist;
                settingWin.BaudRateParm.Text = lk_param.lk_parm_s.baud_rate.ToString();  //这里改变了波特率
                LKSensorCmd.parmBarudIndex = settingWin.BaudRateParm.SelectedIndex;
                settingWin.BaudRateParm.SelectionChanged += Baud_Rate_MoseDown; //波特率改变触发事件，在这调用防止上位机获取参数时候触发
                //输出频率
                sliderFreq.Value = lk_param.lk_parm_s.dataFreq;
                if (lk_param.lk_parm_s.product==0x02)
                {
                    labelProduct.Content = "LK03";
                }
                if(lk_param.lk_parm_s.isLedOn == 0x01)  //红外激光
                {
                    checkBox_red.IsChecked = true;
                    LKSensorCmd.isRedLedOn = true;
                }
                else
                {
                    LKSensorCmd.isRedLedOn = false;
                    checkBox_red.IsChecked = false;
                }
                if(lk_param.lk_parm_s.isBase == 0x00)    //后基准
                {
                    RadioBtn_Front.IsChecked = false;
                    RadioBtn_Base.IsChecked = true;
                    LKSensorCmd.isbase = true;
                }
                else
                {
                    LKSensorCmd.isbase = false;
                    RadioBtn_Base.IsChecked = false;
                    RadioBtn_Front.IsChecked = true;
                }
            }), new object[0]);
        }
        
        //window load
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                //setting windows
                settingWin = new Setting(this);
                settingWin.Owner = this;
                //
                sendTextBox.KeyDown += send_TxBx_kendown;
                initBaudRate(BarudRate);
                BarudRate.Text = "115200";
                SensorDataAcquirer mainDataAcquirer = this.Lk_Serial;
                //add id listen

                mainDataAcquirer.lkFrame.addTYPElistener(LK03_DISPLAY_TYPE, typefunc);
                mainDataAcquirer.lkFrame.addTYPElistener(LK03_PARM_TYPE, ParmaRevFunc);
                mainDataAcquirer.lkFrame.addGenralListener(genralFunc);
                //plot
                //Value = 160;
                //
                mainDataAcquirer.SensorDataChangedEvent = SensorDataChangedHandle;
                //显示
                EnumerableDataSource<Points> ds = new EnumerableDataSource<Points>(this.dd);
                ds.SetXMapping((Points x) => x.Counter);
                ds.SetYMapping((Points y) => y.Value);
                this.chart = plotterTimeLine.AddLineGraph(ds, Colors.Blue, 2.0,"距离");
                

                EnumerableDataSource<Points> dsighal = new EnumerableDataSource<Points>(this.sighal_dd);
                dsighal.SetXMapping((Points x) => x.Counter);
                dsighal.SetYMapping((Points y) => y.Value);
                this.chart = plotterTimeLine.AddLineGraph(dsighal, Colors.Red, 2.0,"峰值电压");

                EnumerableDataSource<Points> d603Gain = new EnumerableDataSource<Points>(this.Gain603_dd);
                d603Gain.SetXMapping((Points x) => x.Counter);
                d603Gain.SetYMapping((Points y) => y.Value);
                this.chart = plotterTimeLine.AddLineGraph(d603Gain, Colors.DarkGreen, 2.0, "增益值");

                this.plotterTimeLine.LegendVisible = true;
                //  plotterTimeLine.Viewport.FitToView();
                //plotterTimeLine.Children.Remove(plotterTimeLine.MouseNavigation); //取消鼠标拖动及缩放绘图仪
                this.timerEftPtsCounter = new System.Timers.Timer();
                this.timerEftPtsCounter.Interval = 1000;
                this.timerEftPtsCounter.Elapsed += TimerEftPtsCounter_Elapsed1;
                this.timerEftPtsCounter.Enabled = true;
                (PresentationSource.FromVisual(this) as HwndSource).AddHook(new HwndSourceHook(this.HwndProc));
                //com list
                GetComList(SerPort);
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex, "");

                MessageBox.Show("Unknow error, exit!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                Application.Current.Shutdown(-1);
            }
        }

        /// <summary>
        /// textbox 发送窗口
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_Cmd(object sender, RoutedEventArgs e)
        {
            strToSend = sendTextBox.Text;
            if (ckBoxStr.IsChecked==true)//字符串发送
            {
                Lk_Serial.SendMsg(send_msg);
            }
            else  //16进制发送
            {
                try
                {
                    strToSend.Replace("0x", ""); //去掉0x
                    strToSend.Replace("0X", ""); //去掉0X
                    int decNum = 0;
                    int i = 0;
                    string[] strArray = strToSend.Split(new char[] { ',', '，', '\r', '\n', ' ', '\t' });
                    byte[] sendBuffer = new byte[strArray.Length];
                    foreach(string str in strArray)
                    {
                        try
                        {
                            decNum = Convert.ToInt16(str, 16);
                            sendBuffer[i] = Convert.ToByte(decNum);
                            i++;    
                        }
                        catch    //无法转换成16进制
                        {
                            MessageBox.Show("hex !!");
                            return;
                        }
                    }
                    Lk_Serial.SendMsg(send_msg);
                }
                catch 
                {
                    
                }
            }
           
        }
        //数据转换
        public string fromHexToString(byte[] hex)
        {
            string hexStr = null;
            foreach (byte str in hex)
            {
                hexStr += string.Format("{0:X2} ", str);
            }
            return hexStr;
        }
/// <summary>
/// 显示数据
/// </summary>
/// <param name="data"></param>
/// <param name="sensor"></param>
        private void ShowData(byte[]data, SensorDataItem sensor)
        {
            if (sensor == null) //txbox显示
            {
                txCunt++;
                string txmsg = fromHexToString(data);
                sendTextBox.AppendText("*********  TxMsg *********" 
                +"\nCounts:" + txCunt.ToString()
                +"\n"+ txmsg);
                sendTextBox.ScrollToEnd();
                if (TxscrollViewer.IsEnabled)
                {
                    TxscrollViewer.ScrollToEnd();
                }
            }
            else
            {
                count_texbox++;
                recieveTextBox.AppendText("******** MaiChe  Technology RxMsg ********" +
                                          "\n" + " Dist: " + dist.ToString() +
                                          "\nTYPE: 0x" + sensor.stringByteToString(sensor.type) +
                                          "\nLens: 0x" + sensor.stringByteToString((byte)(sensor.len >> 8)) + "0x" + sensor.stringByteToString((byte)(sensor.len &0xff)) +
                                          "\nid: " + sensor.stringByteToString(sensor.id) + " Counts: " + count_texbox.ToString() +
                                          "\nFrame:" + sensor.Frame);
                recieveTextBox.AppendText("\n"); // 换行显示

                if (receiveScrollViewer.IsEnabled)
                {
                    receiveScrollViewer.ScrollToEnd();
                }
            }

        }
        //text box
        private Size MeasureTextWidth(string text,FontFamily fontFamily,
                                               FontStyle fontStyle,FontWeight fontWeight,
                                               FontStretch fontStretch, double fontsize)
        {
            FormattedText formattedText = new FormattedText(recieveTextBox.Text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                                                            new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                                                            fontsize, Brushes.Black);
            return new Size(formattedText.Width, formattedText.Height);
        }


        //设置滚动条显示到末尾
        private void ReceiveTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
               // receiveScrollViewer.ScrollToEnd();
            }
            catch
            {
               
            }
        }
        //清空接收数据
        private void ClearReceiveButton_Click(object sender, RoutedEventArgs e)
        {

            count_texbox = 0;
            recieveTextBox.Clear();
        }

        public string strToSend
        {
            set;
            get;
        }
    
        public void send_TxBx_kendown(object sender, KeyEventArgs e)
        {
            strToSend = sendTextBox.Text;
        }

        private void SerPortPmd(object sender, MouseButtonEventArgs e)
        {
           // this.GetComList(SerPort);
        }
        private void SerPortSelC(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                this.strPort = SerPort.Items[SerPort.SelectedIndex].ToString();
            }
            catch
            {
            }
        }
        /// <summary>
        /// 获取串口端口号
        /// </summary>
        /// <param name="s"></param>
        private void GetComList(ComboBox s)
        {
            //通过WMI获取COM端口
            string[] ss = GetSerialPortArray();
            s.Items.Clear();
            foreach (string sName in ss)
            {
                s.Items.Add(sName);
            }



        }
        /// <summary>
        /// 串口连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Btn_Click_Connect(object sender, RoutedEventArgs e)
        {
            if(flagSP)
            {
               BtnConnect.Content = "Connect";
                flagSP = false;         
                Lk_Serial.Close();  //串口关闭
                 ShowOrHideComponents(false);
                return;
                
            }
            if (this.strPort == string.Empty)
            {
                MessageBox.Show("Please choose a Seial Port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (TransParam.PdtMode == string.Empty)
            {
                MessageBox.Show("Please choose Product Type first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            if (BarudRate.SelectedItem == null)
            {
                MessageBox.Show("Please choose BarudRate first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
                return;
            }
            int barate = int.Parse((string)BarudRate.SelectedItem);
            string com = reg.Match(strPort).Value;
            Lk_Serial.IniteSerial(com, barate);
            Lk_Serial.Start();
            if(Lk_Serial.check())
            {
                flagSP = true;
                BtnConnect.Content = "Disconnect";
                ShowOrHideComponents(true);
                getSensorParm(); //获取传感器参数
                return;
            }

            Lk_Serial.Close();
            MessageBox.Show("Open Serial Port Failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
        }

        private void ShowOrHideComponents(bool v)
        {
            sendTextBox.IsEnabled = v;
            SerPort.IsEnabled = !v;
            BarudRate.IsEnabled = !v;
            checkBox_red.IsEnabled = v;
            RadioBtn_Front.IsEnabled = v;
            RadioBtn_Base.IsEnabled = v;
            Btn_Get_Parm.IsEnabled = v;
            Btn_Once.IsEnabled = v;
            Btn_Continue.IsEnabled = v;
            checkBox_Atuo_Che.IsEnabled = v;
        }
        


        private void TimerEftPtsCounter_Elapsed1(object sender, System.Timers.ElapsedEventArgs e)
        {
            base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
            {
                this.eftPtsPerSec = ((this.eftPtsPerSec > 100) ? 100 : this.eftPtsPerSec);
               // this.txbxEffectivePoi.Text = string.Format("{0}", this.eftPtsPerSec);
                this.eftPtsPerSec = 0;
            }), new object[0]);
        }
        private IntPtr HwndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            try
            {
                if (msg == 537)
                {
                  
                    GetComList(SerPort);
                }
            }
            catch
            {
            }
            return IntPtr.Zero;
        }
        private int ddCounter;
        private void SensorDataChangedHandle(byte[]buff)
        {

                        eftPtsPerSec++;
                        UInt16 signal_vol = (UInt16)(buff[1] << 8 | buff[2]);
                        UInt16 dist = (UInt16)(buff[3] << 8 | buff[4]);
                        UInt16 ad603_agc = (UInt16)(buff[5] << 8 | buff[6]);
                        base.Dispatcher.BeginInvoke(new ThreadStart(delegate ()
                        {
                            this.ddCounter++;
                            int xax = ddCounter - 100;
                            if (xax <= 0)
                            {
                                xax = 0;
                            }
                            // plotterTimeLine.Viewport.Visible = new System.Windows.Rect(xax, 0, 50, data.dist+1000);
                            //  plotterTimeLine.Viewport.Width = (xax);
                            this.dd.Add(new Points((uint)this.ddCounter, dist));
                            this.sighal_dd.Add(new Points((uint)this.ddCounter, signal_vol));
                        }), new object[0]);
                   
                
            
           
           
         

        }

   /*产品选择*/
        private void MaiCheProdut(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                TransParam.PdtMode = (string)labelProduct.Content;
               
            }
            catch (Exception ex)
            {
                ErrorLog.WriteLog(ex, "");
            }
        }


        #region 命令按钮

        //单次测量
        public void Btn_Once_Cmd(object sender, RoutedEventArgs e)
        {
            if(Lk_Serial.check())
            {
                send_msg.Type = (byte) (LKSensorCmd.FRAME_TYPE.DataGet);
                send_msg.id = (byte)(LKSensorCmd.FRAME_GetDataID.DistOnce);
                send_msg.ifHeadOnly = true;
                Lk_Serial.SendMsg(send_msg);
                ShowData(send_msg.sendFrame, null);
            }
            else
            {
                MessageBox.Show("Serial port has not connected. Please check!");
            }
        }
        //停止测量
        private void Btn_Click_Stop(object sender, RoutedEventArgs e)
        {
            if (Lk_Serial.check())
            {
                send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.DataGet);
                send_msg.id = (byte)(LKSensorCmd.FRAME_GetDataID.DistStop);
                send_msg.ifHeadOnly = true;
                Lk_Serial.SendMsg(send_msg);
                ShowData(send_msg.sendFrame, null);
            }
            else
            {
                MessageBox.Show("Serial port has not connected. Please check!");
            }
        }
        //连续测量
        private bool ifcontinueDist= true;
        public void Btn_Contitue_Cmd(object sender, RoutedEventArgs e)
        {
            if(ifcontinueDist)
            {
                send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.DataGet);
                send_msg.id = (byte)(LKSensorCmd.FRAME_GetDataID.DistContinue);
                send_msg.ifHeadOnly = true;
                Lk_Serial.SendMsg(send_msg);
                Btn_Continue.Content = "停止";
                ifcontinueDist = false;
                ShowOrHideComponentsConti(false);
            }
            else
            {
                Btn_Continue.Content = "连续";
                ifcontinueDist = true;
                send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.DataGet);
                send_msg.id = (byte)(LKSensorCmd.FRAME_GetDataID.DistStop);
                send_msg.ifHeadOnly = true;
                Lk_Serial.SendMsg(send_msg);
                ShowOrHideComponentsConti(true);
            }
            ShowData(send_msg.sendFrame, null);

        }
        bool ifStandDistStart = false;  //开始标定标记
        private void Btn_Clicked_Stand(object sender, RoutedEventArgs e)
        {
            if (Lk_Serial.check())
            {
                if((lk03_qc.ifRadioFirstStand)|(lk03_qc.ifRadioSecondStand)|(lk03_qc.ifRadioThirdStand))
                {
                    ifStandDistStart = true;
                }
                else
                {
                    MessageBox.Show("请先选择档位!");
                }

            }
            else
            {
                MessageBox.Show("Serial port has not connected. Please check!");
            }
        }

        private void ShowOrHideComponentsConti(bool v)
        {
            sendTextBox.IsEnabled = v;
            SerPort.IsEnabled = v;
            BarudRate.IsEnabled = v;
            checkBox_red.IsEnabled = v;
            RadioBtn_Front.IsEnabled = v;
            RadioBtn_Base.IsEnabled = v;
            Btn_Get_Parm.IsEnabled = v;
            Btn_Once.IsEnabled = v;
            checkBox_Atuo_Che.IsEnabled = v;
            BtnConnect.IsEnabled = v;
        }

        //前基准
        private void radioBtn_front_click(object sender, RoutedEventArgs e)
        {
            if (LKSensorCmd.isbase == false)  //是否已经选中则提示
            {
                MessageBox.Show("当前已是前基准模式，不用设置", "提示");
            }
            else
            {
                MessageBoxResult dr = MessageBox.Show("是否配置前基准模式", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    LKSensorCmd.isbase = false;
                    //发送消息 前基准
                    send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParmsSave);
                    send_msg.id = (byte)(LKSensorCmd.FRAME_ParmSaveID.FrontOrBase);
                    send_msg.ifHeadOnly = false;  //含有数据帧
                    byte frontOrBase = (byte)(LKSensorCmd.FRAME_FRONT_BASE.FRONT);
                    send_msg.sendbuf = BitConverter.GetBytes(frontOrBase);    //数据帧缓存
                    send_msg.len = LKSensorCmd.parmFrontBaseByteSize; //数据帧字节长度
                    Lk_Serial.SendMsg(send_msg);
                    ShowData(send_msg.sendFrame, null);
                }
                else
                {
                    RadioBtn_Base.IsChecked = true;
                    RadioBtn_Front.IsChecked = false;

                }
            }
        }
        //后基准
        private void radioBtn_base_click(object sender, RoutedEventArgs e)
        {
            if (LKSensorCmd.isbase == true)  //是否已经选中则提示
            {
                MessageBox.Show("当前已是后基准模式，不用设置", "提示");
            }
            else
            {
                MessageBoxResult dr = MessageBox.Show("是否配置后基准模式", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    LKSensorCmd.isbase = true;
                    //发送消息 后基准
                    send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParmsSave);
                    send_msg.id = (byte)(LKSensorCmd.FRAME_ParmSaveID.FrontOrBase);
                    send_msg.ifHeadOnly = false;  //含有数据帧
                    byte frontOrBase = (byte)(LKSensorCmd.FRAME_FRONT_BASE.BASE);
                    send_msg.sendbuf = BitConverter.GetBytes(frontOrBase);    //数据帧缓存
                    send_msg.len = LKSensorCmd.parmFrontBaseByteSize; //数据帧字节长度
                    Lk_Serial.SendMsg(send_msg);
                    ShowData(send_msg.sendFrame, null);
                }
                else
                {
                    RadioBtn_Base.IsChecked = false;
                    RadioBtn_Front.IsChecked = true;
                }
            }
        }
        //红外激光瞄准测试
        private void checkBox_red_click(object sender, RoutedEventArgs e)
        {
            if (LKSensorCmd.isRedLedOn == true)  //是否已经选中则提示
            {
                MessageBoxResult dr = MessageBox.Show("当前红外辅助激光已经打开，是否关闭？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    LKSensorCmd.isRedLedOn = false;
                    //发送消息 关闭红外辅助
                    send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParmsSave);
                    send_msg.id = (byte)(LKSensorCmd.FRAME_ParmSaveID.RedLightSave);
                    send_msg.ifHeadOnly = false;  //含有数据帧
                    byte setByte = (byte)(LKSensorCmd.FRAME_RED_LIGHT.OFF);
                    send_msg.sendbuf = BitConverter.GetBytes(setByte);    //数据帧缓存
                    send_msg.len = LKSensorCmd.parmRedLightByteSize; //数据帧字节长度
                    Lk_Serial.SendMsg(send_msg);
                   // ShowData(send_msg.sendFrame, null);
                }
                else
                {
                    checkBox_red.IsChecked = true;
                }
            }
            else
            {
                MessageBoxResult dr = MessageBox.Show("是否打开红外辅助激光", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    LKSensorCmd.isRedLedOn = true;
                    checkBox_red.IsChecked = true;
                    //发送消息 打开红外辅助
                    send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParmsSave);
                    send_msg.id = (byte)(LKSensorCmd.FRAME_ParmSaveID.RedLightSave);
                    send_msg.ifHeadOnly = false;  //含有数据帧
                    byte setByte = (byte)(LKSensorCmd.FRAME_RED_LIGHT.ON);
                    send_msg.sendbuf = BitConverter.GetBytes(setByte);    //数据帧缓存
                    send_msg.len = LKSensorCmd.parmRedLightByteSize; //数据帧字节长度
                    Lk_Serial.SendMsg(send_msg);
                    ShowData(send_msg.sendFrame, null);

                }
                else
                {
                    checkBox_red.IsChecked = false;
                }
            }

        }
       //获取当前传感器参数
        private void Btn_Click_getParam(object sender, RoutedEventArgs e)
        {
            send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParamGet);  //获取传感器参数
            send_msg.id = (byte)(LKSensorCmd.FRAME_GetParamID.ParamAll);
            send_msg.ifHeadOnly = true;
            Lk_Serial.SendMsg(send_msg);
            ShowData(send_msg.sendFrame, null);
        }

        /*获取传感器参数*/
        public void getSensorParm()
        {
            send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParamGet);  //获取传感器参数
            send_msg.id= (byte)(LKSensorCmd.FRAME_GetParamID.ParamAll);
            send_msg.ifHeadOnly = true;
            Lk_Serial.SendMsg(send_msg);
        }
        /*波特率设置*/
        private void Baud_Rate_MoseDown(object sender, SelectionChangedEventArgs e)
        {

         

        }
        /*开机自动测量*/
        private void checkBox_Atuo_click(object sender, RoutedEventArgs e)
        {
            if (LKSensorCmd.isSensosAutoCel == true)  //是否已经选中则提示
            {
                MessageBoxResult dr = MessageBox.Show("当前开机自动测量模式，是否关闭？", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    LKSensorCmd.isRedLedOn = false;
                    //发送消息 关闭开机自动测量
                    send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParmsSave);
                    send_msg.id = (byte)(LKSensorCmd.FRAME_ParmSaveID.AutoMel);
                    send_msg.ifHeadOnly = false;  //含有数据帧
                    byte setByte = (byte)(LKSensorCmd.FRAME_AUTO_MEL.OFF);
                    send_msg.sendbuf = BitConverter.GetBytes(setByte);    //数据帧缓存
                    send_msg.len = LKSensorCmd.parmRedLightByteSize; //数据帧字节长度
                    Lk_Serial.SendMsg(send_msg);
                    // ShowData(send_msg.sendFrame, null);
                }
                else
                {
                    checkBox_red.IsChecked = true;
                }
            }
            else
            {
                MessageBoxResult dr = MessageBox.Show("是否自动测量模式", "提示", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (dr == MessageBoxResult.OK)
                {
                    LKSensorCmd.isRedLedOn = true;
                    checkBox_red.IsChecked = true;
                    //发送消息 打开红外辅助
                    send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.ParmsSave);
                    send_msg.id = (byte)(LKSensorCmd.FRAME_ParmSaveID.AutoMel);
                    send_msg.ifHeadOnly = false;  //含有数据帧
                    byte setByte = (byte)(LKSensorCmd.FRAME_AUTO_MEL.ON);
                    send_msg.sendbuf = BitConverter.GetBytes(setByte);    //数据帧缓存
                    send_msg.len = LKSensorCmd.parmRedLightByteSize; //数据帧字节长度
                    Lk_Serial.SendMsg(send_msg);
                    ShowData(send_msg.sendFrame, null);

                }
                else
                {
                    checkBox_red.IsChecked = false;
                }
            }
        }
        #endregion

        #region  串口设置
        /*     baud setting   */

        public void zkSerialPortInit()
        {
            ports = SerialPort.GetPortNames();
            foreach (string port in ports)
            {
                SerPort.Items.Add(port.ToString());
            }
            zjkPort = new SerialPort();
            zjkPort.DataReceived += new SerialDataReceivedEventHandler(ReceiveData);
            zjkPort.Parity = Parity.None;
            zjkPort.StopBits = StopBits.One;
            zjkPort.DataBits = 8;
            zjkPort.BaudRate = 115200;
            zjkPort.ReadTimeout = 200;
            BarudRate.Text = zjkPort.BaudRate.ToString();
            if (SerPort.Items.Count > 0)
            {
                zjkPort.PortName = SerPort.Items[0].ToString();
            }
            else
            {
                BtnConnect.IsEnabled = false;
            }

        }

        public void ReceiveData(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            SerialPort serial = (SerialPort)sender;
            receiveData = serial.ReadExisting();
            //tinyFrame.ThinyFameRecv(receiveData);    //协议解析
            //Dispatcher.Invoke(DispatcherPriority.Send, new UpdateUiTextDelegate(ShowData), receiveData);
        }

        private void @new(object sender, SerialDataReceivedEventArgs e)
        {
            throw new NotImplementedException();
        }


        //Change comport
        private void changePort(object sender, SelectionChangedEventArgs e)
        {
            if (SerPort.SelectedItem != null)
            {
                zjkPort.PortName = SerPort.SelectedItem.ToString();
            }
        }
        private void initBaudRate(object sender)
        {
            ComboBox box = (ComboBox)sender;
            int[] bauds = new int[] { 9600, 14400, 19200, 38400, 57600, 115200 };
            for (int i = 0; i < bauds.Length; i++)
            {
                object baud = bauds[i].ToString();
                box.Items.Add(baud);
            }
          
        }
        private void changeBaud(object sender, SelectionChangedEventArgs e)
        {
            ComboBox box = (ComboBox)sender;
            int newBaud = int.Parse((string)box.SelectedItem);
            Console.WriteLine("Setting Baud to" + newBaud.ToString());
            try
            {
                zjkPort.BaudRate = newBaud;
            }
            catch (ArgumentNullException) { }
        }

        #endregion
        public enum HardwareEnum
        {
            //硬件
            Win32_Processor,//cpu 处理器
            Win32_SerialPort,//串口
            Win32_SerialPortConfiguration,// 串口配置
            Win32_USBController,//usb控制器
            Win32_PnPEntity, //all device
        }
        public static string[] MulGetHardwareinfo(HardwareEnum hardType, string propKey)
        {

            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from" + hardType))
                {
                    var hardinfos = searcher.Get();
                    foreach(var hardinfo in hardinfos)
                    {
                        if(hardinfo.Properties[propKey].Value.ToString().Contains("COM"))
                        {
                            strs.Add(hardinfo.Properties[propKey].Value.ToString());
                        }
                    }
                    searcher.Dispose();
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            finally
            {
                strs = null;
            }

        }

        //恢复出厂设置提示
        private void reset_button_click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult dr = MessageBox.Show("是否确定恢复出厂设置","提示",MessageBoxButton.OKCancel,MessageBoxImage.Question);
            if(dr==MessageBoxResult.OK)
            {

            }
        }

        private void Updata_Click(object sender, RoutedEventArgs e)
        {
            progressBar();
        }

        //进度条多线程使用防止UI卡顿
        private void progressBar()
        {
            Thread thread = new Thread(new ThreadStart(() =>   //多线程
            {
                for (int i = 0; i < 100; i++)
                {
                    progressBarUpdata.Dispatcher.BeginInvoke((ThreadStart)delegate
                    {
                        progressBarUpdata.Value = i;
                    });
                    Thread.Sleep(100);
                }
            }));
            thread.Start();
        }
        public void ledStatuShake()
        {
            Thread thread = new Thread(new ThreadStart(() =>   //多线程
            {
                models.HeartBeat = true;
                Thread.Sleep(100);
                models.HeartBeat = false;
                Thread.Sleep(100);
                models.HeartBeat = true;
                Thread.Sleep(100);
                models.HeartBeat = false;
            }));
            thread.Start();
        }

        //about
        private void Btn_Clicked_About(object sender, RoutedEventArgs e)
        {
            infoWin = new info(this);
            infoWin.Owner = this;
            infoWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            infoWin.ShowDialog();
            
        }




        private void Btn_Clicked_Setting(object sender, RoutedEventArgs e)
        {     
            settingWin.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            settingWin.ShowDialog();
        }
        #region 动态获取串口
        /// <summary>
        /// 枚举win32 api
        /// </summary>
        public enum HardwareEnum_T
        {
            // 硬件
            Win32_Processor, // CPU 处理器
            Win32_PhysicalMemory, // 物理内存条
            Win32_Keyboard, // 键盘
            Win32_PointingDevice, // 点输入设备，包括鼠标。
            Win32_FloppyDrive, // 软盘驱动器
            Win32_DiskDrive, // 硬盘驱动器
            Win32_CDROMDrive, // 光盘驱动器
            Win32_BaseBoard, // 主板
            Win32_BIOS, // BIOS 芯片
            Win32_ParallelPort, // 并口
            Win32_SerialPort, // 串口
            Win32_SerialPortConfiguration, // 串口配置
            Win32_SoundDevice, // 多媒体设置，一般指声卡。
            Win32_SystemSlot, // 主板插槽 (ISA & PCI & AGP)
            Win32_USBController, // USB 控制器
            Win32_NetworkAdapter, // 网络适配器
            Win32_NetworkAdapterConfiguration, // 网络适配器设置
            Win32_Printer, // 打印机
            Win32_PrinterConfiguration, // 打印机设置
            Win32_PrintJob, // 打印机任务
            Win32_TCPIPPrinterPort, // 打印机端口
            Win32_POTSModem, // MODEM
            Win32_POTSModemToSerialPort, // MODEM 端口
            Win32_DesktopMonitor, // 显示器
            Win32_DisplayConfiguration, // 显卡
            Win32_DisplayControllerConfiguration, // 显卡设置
            Win32_VideoController, // 显卡细节。
            Win32_VideoSettings, // 显卡支持的显示模式。

            // 操作系统
            Win32_TimeZone, // 时区
            Win32_SystemDriver, // 驱动程序
            Win32_DiskPartition, // 磁盘分区
            Win32_LogicalDisk, // 逻辑磁盘
            Win32_LogicalDiskToPartition, // 逻辑磁盘所在分区及始末位置。
            Win32_LogicalMemoryConfiguration, // 逻辑内存配置
            Win32_PageFile, // 系统页文件信息
            Win32_PageFileSetting, // 页文件设置
            Win32_BootConfiguration, // 系统启动配置
            Win32_ComputerSystem, // 计算机信息简要
            Win32_OperatingSystem, // 操作系统信息
            Win32_StartupCommand, // 系统自动启动程序
            Win32_Service, // 系统安装的服务
            Win32_Group, // 系统管理组
            Win32_GroupUser, // 系统组帐号
            Win32_UserAccount, // 用户帐号
            Win32_Process, // 系统进程
            Win32_Thread, // 系统线程
            Win32_Share, // 共享
            Win32_NetworkClient, // 已安装的网络客户端
            Win32_NetworkProtocol, // 已安装的网络协议
            Win32_PnPEntity,//all device
        }
        /// <summary>
        /// WMI取硬件信息
        /// </summary>
        /// <param name="hardType"></param>
        /// <param name="propKey"></param>
        /// <returns></returns>
        public static string[] MulGetHardwareInfo(HardwareEnum_T hardType, string propKey)
        {

            List<string> strs = new List<string>();
            try
            {
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + hardType))
                {
                    var hardInfos = searcher.Get();
                    foreach (var hardInfo in hardInfos)
                    {
                        if (hardInfo.Properties[propKey].Value != null)
                        {
                            if (hardInfo.Properties[propKey].Value.ToString().Contains("COM"))
                            {
                                strs.Add(hardInfo.Properties[propKey].Value.ToString());
                            }
                        }
                    }
                    searcher.Dispose();
                }
                return strs.ToArray();
            }
            catch
            {
                return null;
            }
            //finally
            //{ strs = null; }
        }

        /// <summary>
        /// 获取串口列表
        /// </summary>
        /// <returns></returns>
        private static string[] GetSerialPortList()
        {
            return MulGetHardwareInfo(HardwareEnum_T.Win32_PnPEntity, "Name");
        }
        /// <summary>
        /// 获取串口列表线程
        /// </summary>
        private void OnGetSerialPortList()
        {

            try
            {
                PortNameArray = GetSerialPortList();
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            ReadOver = true;
        }
        string[] PortNameArray;
        bool ReadOver;
        public Thread threadReadValue { get; private set; }
        /// <summary>
        /// 通过线程获取串口列表
        /// </summary>
        public string[] GetSerialPortArray()
        {
            PortNameArray = null;
            try
            {
                threadReadValue = new System.Threading.Thread(OnGetSerialPortList);
                threadReadValue.IsBackground = true;
                ReadOver = false;
                threadReadValue.Start();

                while (ReadOver == false)
                {
                    System.Threading.Thread.Sleep(200);
                }
                threadReadValue = null;
            }
            catch (Exception exp)
            {
                System.Diagnostics.Debug.WriteLine(exp.Message);
            }
            return PortNameArray;
        }

        /// <summary>
        /// 标定值改变事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void slider_value_change(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            LKSensorCmd.stand_distance =(byte) stand_slider.Value;
        }

        private void main_windowClose(object sender, EventArgs e)
        {
            if(Lk_Serial.check())
            {
                Lk_Serial.Close();

            }
        }

        private void Btn_Clicked_saveParam(object sender, RoutedEventArgs e)
        {

            //发送保存参数
            //发送消息 前基准
            send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.QC);
            send_msg.ifHeadOnly = false;  //含有数据帧
            byte[] setByte = new byte[4];
            if (lk03_qc.ifRadioFirstStand)
            {
                send_msg.id = (byte)(LKSensorCmd.FRAME_QCcmdID.StandParamFirst);  //校准参数
                setByte[0] = (byte)(lk_distCalibration[0] >> 8);
                setByte[1] = (byte)(lk_distCalibration[0] & 0xff);
                setByte[2] = (byte)(lk_gain[0] >> 8);
                setByte[3] = (byte)(lk_gain[0] & 0xff);
            }
            else if(lk03_qc.ifRadioSecondStand)
            {
                send_msg.id = (byte)(LKSensorCmd.FRAME_QCcmdID.StandParamSecond);  //校准参数
                setByte[0] = (byte)(lk_distCalibration[1] >> 8);
                setByte[1] = (byte)(lk_distCalibration[1] & 0xff);
                setByte[2] = (byte)(lk_gain[1] >> 8);
                setByte[3] = (byte)(lk_gain[1] & 0xff);
            }
            else if (lk03_qc.ifRadioThirdStand)
            {
                send_msg.id = (byte)(LKSensorCmd.FRAME_QCcmdID.StandParamThird);  //校准参数
                setByte[0] = (byte)(lk_distCalibration[2] >> 8);
                setByte[1] = (byte)(lk_distCalibration[2] & 0xff);
                setByte[2] = (byte)(lk_gain[2] >> 8);
                setByte[3] = (byte)(lk_gain[2] & 0xff);
            }


            send_msg.sendbuf = setByte;    //数据帧缓存
            send_msg.len = (UInt16)setByte.Length; //数据帧字节长度
            Lk_Serial.SendMsg(send_msg);

        }

        private void Btn_Clicked_getStandParam(object sender, RoutedEventArgs e)
        {
            send_msg.Type = (byte)(LKSensorCmd.FRAME_TYPE.QC);
            send_msg.id = (byte)(LKSensorCmd.FRAME_QCcmdID.GetParam);
            send_msg.ifHeadOnly = true;
            Lk_Serial.SendMsg(send_msg);
        }

        private void radioBtn_first_click(object sender, RoutedEventArgs e)
        {   
            if(lk_ifLkHavedStand[0])
            {
                btn_stand.Content = "已标定";
            }
            else
            {
                btn_stand.Content = "未标定";
            }
            textBox_calibration.Text = lk_distCalibration[0].ToString(); // 校准值  
            textBox_gain.Text = lk_gain[0].ToString();
            textBox_average.Text = lk_average[0].ToString();
        }

        private void radioBtn_second_click(object sender, RoutedEventArgs e)
        {
            if (lk_ifLkHavedStand[1])
            {
                btn_stand.Content = "已标定";
            }
            else
            {
                btn_stand.Content = "未标定";
            }
            textBox_calibration.Text = lk_distCalibration[1].ToString(); // 校准值  
            textBox_gain.Text = lk_gain[1].ToString();
            textBox_average.Text = lk_average[1].ToString();
        }

        private void radioBtn_third_click(object sender, RoutedEventArgs e)
        {
            if (lk_ifLkHavedStand[2])
            {
                btn_stand.Content = "已标定";
            }
            else
            {
                btn_stand.Content = "未标定";
            }
            textBox_calibration.Text = lk_distCalibration[2].ToString(); // 校准值  
            textBox_gain.Text = lk_gain[2].ToString();
            textBox_average.Text = lk_average[2].ToString();
        }
    }
    #endregion
    class NotifyBase 
    {

        private bool heartBeat;

        public bool HeartBeat
        {
            get { return heartBeat; }
            set
            {
                heartBeat = value;
             //   OnPropertyChange("HeartBeat");
            }
        }
    }

    public class MeasureModel
    {
        public int DateTime { get; set; }
        public UInt16 Value { get; set; }
    }
    public class LK03QC: INotifyPropertyChanged
    {
        public bool ifSelctFirstStand;
        public bool ifSelctSecondStand;
        public bool ifSelctThirdStand;
        public bool ifRadioFirstStand
        {
            get
            {
                return ifSelctFirstStand;
            }
            set
            {
                ifSelctFirstStand = value;
                OnPropertyChange("ifRadioFirstStand");
            }
        }
        public bool ifRadioSecondStand
        {
            get
            {
                return ifSelctSecondStand;
            }
            set
            {
                ifSelctSecondStand = value;
                OnPropertyChange("ifSelctSecondStand");
            }
        }
        public bool ifRadioThirdStand
        {
            get
            {
                return ifSelctThirdStand;
            }
            set
            {
                ifSelctThirdStand = value;
                OnPropertyChange("ifSelctThirdStand");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChange(string propertyName)
        {

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
