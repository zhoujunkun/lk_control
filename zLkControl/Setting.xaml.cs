using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        
        public Setting(MainWindow MyMainWindow)
        {
            
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
    }
}
