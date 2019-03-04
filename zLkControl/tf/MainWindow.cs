using MahApps.Metro.Controls;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WINCC_TFMini
{
	public class MainWindow : MetroWindow, IComponentConnector
	{
		[CompilerGenerated]
		[Serializable]
		private class sealed()  
		{
			public static readonly MainWindow = new MainWindow();

			public static Func<Points, double>;

			public static Func<Points, double> ;

			internal double <Window_Loaded>b__39_0(Points x)
			{
				return x.Counter;
			}

			internal double <Window_Loaded>b__39_1(Points y)
			{
				return y.Value;
			}
		}

		private const int Baudrate = 115200;

		private const double Y_MARGIN_TOP = 20.0;

		private const double Y_MARGIN_BOT = 21.0;

		private int offset;

		private int ddCounter;

		private int eftPtsPerSec;

		private bool flagSP;

		private bool isCMDSent;

		private bool isPixMode;

		private bool flagFreeze;

		private bool flagRecord;

		private bool flagInitUI = true;

		private byte prdtMode;

		private byte sensorMode = 2;

		private long I;

		private long PointNum;

		private double MDist;

		private double MeanDist;

		private double MeanRange = 5.0;

		private double count;

		private double range;

		private Queue<double> DistHistory = new Queue<double>();

		private double cvsHeight = 420.0;

		private string strPort = string.Empty;

		private string strNaming = string.Empty;

		private string strRoot = "Data\\";

		private string strRootCpld = string.Empty;

		private About aboutWin;

		private System.Timers.Timer timerEftPtsCounter;

		private FileStream fs;

		private StreamWriter sw;

		private LineGraph chart;

		private DynamicDisplay dd = new DynamicDisplay();

		private SensorDataAcquirer MainDataAcquirer = new SensorDataAcquirer();

		private ObservableDataSource<Point> DataSource = new ObservableDataSource<Point>();

		internal ComboBox cmbProduct;

		internal ComboBox cmbPort;

		internal Button btnConnect;

		internal CheckBox ckbPixMode;

		internal Button btnFreeze;

		internal Button btnClear;

		internal TextBox txbxDataAmout;

		internal TextBox txbxDeviceCMD;

		internal Button btnCMDSend;

		internal TextBox txbxNaming;

		internal Button btnRecord;

		internal Button btnFolder;

		internal TextBlock txbcDist;

		internal TextBlock txbxEffectivePoi;

		internal TextBlock txbxStrength;

		internal ChartPlotter plotterTimeLine;

		internal VerticalAxisTitle axisTitle;

		internal Canvas cvsDymCursor;

		internal Line lnYAxis;

		internal Line lnXAixs5;

		internal Line lnXAixs4;

		internal Line lnXAixs3;

		internal Line lnXAixs2;

		internal Line lnXAixs1;

		internal Line lnXAixs0;

		internal TextBlock range1;

		internal TextBlock range2;

		internal TextBlock range3;

		internal TextBlock range4;

		internal TextBlock range5;

		internal TextBlock range6;

		private bool _contentLoaded;

		public bool IsPixMode
		{
			get
			{
				return this.isPixMode;
			}
			set
			{
				this.isPixMode = value;
				TransParam.IsPixOn = value;
				this.MainDataAcquirer.EmptyBuffer();
				this.axisTitle.Content = (value ? "Distance(m)" : "Distance(cm)");
				this.ddCounter = 0;
				this.dd.Clear();
				switch (TransParam.PdtMode)
				{
				case TransParam.Product.TF01:
					this.MainDataAcquirer.SendCommand(value ? OrderSet.cmdPixOutput_01 : OrderSet.cmdStandardOutput_01);
					break;
				case TransParam.Product.TF02:
					this.prdtMode = (value ? 2 : 0);
					new Thread(new ThreadStart(this.TDSwitch))
					{
						IsBackground = true
					}.Start();
					return;
				case TransParam.Product.TFMini:
				{
					Thread thread;
					if (value)
					{
						thread = new Thread(new ThreadStart(this.TDPixModeOn));
					}
					else
					{
						thread = new Thread(new ThreadStart(this.TD8o9ModeOn));
					}
					thread.IsBackground = true;
					thread.Start();
					return;
				}
				case TransParam.Product.TFp64:
					if (value)
					{
						MessageBox.Show("This type of product does not surpport PIX mode!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
						this.ckbPixMode.IsChecked = new bool?(false);
						return;
					}
					break;
				default:
					return;
				}
			}
		}

		public MainWindow()
		{
			this.InitializeComponent();
			base.DataContext = this;
			base.Loaded += new RoutedEventHandler(this.Window_Loaded);
			base.Closed += new EventHandler(this.MainWindow_Closed);
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			try
			{
				this.txbxNaming.KeyDown += new KeyEventHandler(this.TxbxNaming_KeyDown);
				this.txbxDataAmout.KeyDown += new KeyEventHandler(this.TxbxDataAmout_KeyDown);
				this.txbxDeviceCMD.KeyDown += new KeyEventHandler(this.TxbxDeviceCMD_KeyDown);
				this.InitBaudrate();
				SensorDataAcquirer expr_51 = this.MainDataAcquirer;
				expr_51.SensorDataChangedEvent = (SensorDataAcquirer.SensorDataChangedHandler)Delegate.Combine(expr_51.SensorDataChangedEvent, new SensorDataAcquirer.SensorDataChangedHandler(this.MainDataAcquirer_SensorDataChangedEvent));
				EnumerableDataSource<Points> enumerableDataSource = new EnumerableDataSource<Points>(this.dd);
				EnumerableDataSource<Points> arg_9E_0 = enumerableDataSource;
				Func<Points, double> arg_9E_1;
				if ((arg_9E_1 = MainWindow.<>c.<>9__39_0) == null)
				{
					arg_9E_1 = (MainWindow.<>c.<>9__39_0 = new Func<Points, double>(MainWindow.<>c.<>9.<Window_Loaded>b__39_0));
				}
				arg_9E_0.SetXMapping(arg_9E_1);
				EnumerableDataSource<Points> arg_C3_0 = enumerableDataSource;
				Func<Points, double> arg_C3_1;
				if ((arg_C3_1 = MainWindow.<>c.<>9__39_1) == null)
				{
					arg_C3_1 = (MainWindow.<>c.<>9__39_1 = new Func<Points, double>(MainWindow.<>c.<>9.<Window_Loaded>b__39_1));
				}
				arg_C3_0.SetYMapping(arg_C3_1);
				this.chart = this.plotterTimeLine.AddLineGraph(enumerableDataSource, Colors.Blue, 2.0);
				this.plotterTimeLine.LegendVisible = false;
				this.timerEftPtsCounter = new System.Timers.Timer();
				this.timerEftPtsCounter.Interval = 1000.0;
				this.timerEftPtsCounter.Elapsed += new ElapsedEventHandler(this.TimerEftPtsCounter_Elapsed);
				this.timerEftPtsCounter.Enabled = true;
				(PresentationSource.FromVisual(this) as HwndSource).AddHook(new HwndSourceHook(this.HwndProc));
			}
			catch (Exception arg_159_0)
			{
				ErrorLog.WriteLog(arg_159_0, "");
				MessageBox.Show("Unknow error, exit!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				Application.Current.Shutdown(-1);
			}
		}

		private void MainWindow_Closed(object sender, EventArgs e)
		{
			this.timerEftPtsCounter.Close();
			this.timerEftPtsCounter.Dispose();
		}

		private void MnW_SCd_Resize(object sender, SizeChangedEventArgs e)
		{
			if (this.flagInitUI)
			{
				this.flagInitUI = false;
				return;
			}
			this.cvsHeight = this.cvsDymCursor.RenderSize.Height - 21.0 - 20.0;
			this.lnYAxis.Y2 = this.cvsHeight + 20.0;
			this.lnXAixs0.Y1 = (this.lnXAixs0.Y2 = this.cvsHeight + 20.0);
			this.lnXAixs1.Y1 = (this.lnXAixs1.Y2 = 4.0 * this.cvsHeight / 5.0 + 20.0);
			this.lnXAixs2.Y1 = (this.lnXAixs2.Y2 = 3.0 * this.cvsHeight / 5.0 + 20.0);
			this.lnXAixs3.Y1 = (this.lnXAixs3.Y2 = 2.0 * this.cvsHeight / 5.0 + 20.0);
			this.lnXAixs4.Y1 = (this.lnXAixs4.Y2 = 1.0 * this.cvsHeight / 5.0 + 20.0);
			this.lnXAixs5.Y1 = (this.lnXAixs5.Y2 = 20.0);
			Thickness margin = new Thickness(77.0, this.lnXAixs0.Y1 - 10.0, 0.0, 0.0);
			this.range1.Margin = margin;
			margin = new Thickness(77.0, this.lnXAixs1.Y1 - 10.0, 0.0, 0.0);
			this.range2.Margin = margin;
			margin = new Thickness(77.0, this.lnXAixs2.Y1 - 10.0, 0.0, 0.0);
			this.range3.Margin = margin;
			margin = new Thickness(77.0, this.lnXAixs3.Y1 - 10.0, 0.0, 0.0);
			this.range4.Margin = margin;
			margin = new Thickness(77.0, this.lnXAixs4.Y1 - 10.0, 0.0, 0.0);
			this.range5.Margin = margin;
			margin = new Thickness(77.0, this.lnXAixs5.Y1 - 10.0, 0.0, 0.0);
			this.range6.Margin = margin;
		}

		private void MainDataAcquirer_SensorDataChangedEvent(SensorDataItem data, long counter)
		{
			this.eftPtsPerSec++;
			if (data.isCMDReceived)
			{
				this.isCMDSent = true;
			}
			if (!this.flagFreeze)
			{
				if (!TransParam.IsPixOn)
				{
					data.dist = (double)(data.D - this.offset);
				}
				else
				{
					data.dist *= 100.0;
				}
				if ((double)this.DistHistory.Count < this.MeanRange)
				{
					this.MeanDist = (this.MeanDist * (double)this.DistHistory.Count + data.dist) / (double)(this.DistHistory.Count + 1);
					this.DistHistory.Enqueue(data.dist);
				}
				else if ((double)this.DistHistory.Count == this.MeanRange)
				{
					this.MeanDist = this.MeanDist - this.DistHistory.Dequeue() / this.MeanRange + data.dist / this.MeanRange;
					this.DistHistory.Enqueue(data.dist);
				}
				else if ((double)this.DistHistory.Count > this.MeanRange)
				{
					do
					{
						int num = this.DistHistory.Count;
						this.MeanDist = (this.MeanDist * (double)num - this.DistHistory.Dequeue()) / (double)(num - 1);
					}
					while ((double)this.DistHistory.Count > this.MeanRange);
					this.MeanDist = this.MeanDist - this.DistHistory.Dequeue() / this.MeanRange + data.dist / this.MeanRange;
					this.DistHistory.Enqueue(data.dist);
				}
				this.PointNum += 1L;
				try
				{
					this.count += 1.0;
					if (this.count == this.MeanRange)
					{
						ObservableDataSource<Point> arg_248_0 = this.DataSource;
						Dispatcher arg_248_1 = base.Dispatcher;
						long i = this.I;
						this.I = i + 1L;
						arg_248_0.AppendAsync(arg_248_1, new Point((double)i, this.MeanDist));
						this.count = 0.0;
						base.Dispatcher.BeginInvoke(new ThreadStart(delegate
						{
							this.ddCounter++;
							this.dd.Add(new Points((uint)this.ddCounter, TransParam.IsPixOn ? (data.dist / 100.0) : data.dist));
						}), new object[0]);
					}
				}
				catch (Exception arg_281_0)
				{
					ErrorLog.WriteLog(arg_281_0, "");
				}
				base.Dispatcher.BeginInvoke(new ThreadStart(delegate
				{
					Line line = new Line();
					Polygon polygon = new Polygon();
					line.StrokeThickness = 3.0;
					polygon.StrokeThickness = 1.0;
					if (this.MeanDist <= this.range)
					{
						this.MDist = this.cvsHeight + 20.0 - this.MeanDist / this.range * this.cvsHeight;
						line.Stroke = new SolidColorBrush(Colors.Blue);
						polygon.Stroke = Brushes.Blue;
						polygon.Fill = Brushes.Blue;
					}
					else
					{
						this.MDist = 20.0;
						line.Stroke = new SolidColorBrush(Colors.Red);
						polygon.Stroke = Brushes.Red;
						polygon.Fill = Brushes.Red;
					}
					line.X1 = 62.0;
					line.Y1 = this.MDist;
					line.X2 = 82.0;
					line.Y2 = this.MDist;
					polygon.Points = new PointCollection
					{
						new Point(42.0, this.MDist),
						new Point(47.0, this.MDist - 3.0),
						new Point(57.0, this.MDist),
						new Point(47.0, this.MDist + 3.0)
					};
					this.cvsDymCursor.Children.Clear();
					this.cvsDymCursor.Children.Add(line);
					this.cvsDymCursor.Children.Add(polygon);
					this.txbcDist.Text = string.Format("{0, 1:0}", this.MeanDist);
					this.txbxStrength.Text = string.Format("{0}", data.Amp);
				}), new object[0]);
			}
			if (this.flagRecord)
			{
				if (this.isPixMode)
				{
					this.sw.WriteLine((data.dist / 100.0).ToString());
					return;
				}
				this.sw.WriteLine(string.Format(data.dist + "\t" + data.Amp, new object[0]));
			}
		}

		private IntPtr HwndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			try
			{
				if (msg == 537)
				{
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
					string[] valueNames = registryKey.GetValueNames();
					this.cmbPort.Items.Clear();
					string[] array = valueNames;
					for (int i = 0; i < array.Length; i++)
					{
						string name = array[i];
						string newItem = (string)registryKey.GetValue(name);
						this.cmbPort.Items.Add(newItem);
					}
					if (this.flagSP)
					{
						int num = 0;
						bool flag = true;
						array = valueNames;
						for (int i = 0; i < array.Length; i++)
						{
							string name2 = array[i];
							if (this.MainDataAcquirer.GetPort() == (string)registryKey.GetValue(name2))
							{
								flag = false;
								this.cmbPort.SelectedIndex = num;
							}
							num++;
						}
						if (flag)
						{
							this.MainDataAcquirer.Close();
							base.Dispatcher.BeginInvoke(new Action(delegate
							{
								this.flagSP = false;
								this.btnConnect.Content = "Connect";
							}), new object[0]);
							MessageBox.Show("Serial port has been removed. Please check!");
						}
					}
				}
			}
			catch
			{
			}
			return IntPtr.Zero;
		}

		private void InitBaudrate()
		{
			this.cmbProduct.Items.Add("Benewake TF01");
			this.cmbProduct.Items.Add("Benewake TF02");
			this.cmbProduct.Items.Add("Benewake TFp64");
			this.cmbProduct.Items.Add("Benewake TFmini");
		}

		private void Btn_Click_Connect(object sender, RoutedEventArgs e)
		{
			if (this.flagSP)
			{
				this.btnConnect.Content = "Connect";
				this.flagSP = false;
				this.MainDataAcquirer.Close();
				this.ShowOrHideComponents(false);
				return;
			}
			if (this.strPort == string.Empty)
			{
				MessageBox.Show("Please choose a COM Port first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			if (TransParam.PdtMode == TransParam.Product.NULL)
			{
				MessageBox.Show("Please choose Product Type first!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			this.MainDataAcquirer.Initialize(this.strPort, 115200);
			this.MainDataAcquirer.Start();
			if (this.MainDataAcquirer.Check())
			{
				this.flagSP = true;
				this.btnConnect.Content = "Disconnect";
				this.ShowOrHideComponents(true);
				return;
			}
			this.MainDataAcquirer.Close();
			MessageBox.Show("Open Serial Port Failed!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
		}

		private void Btn_Click_DataFolder(object sender, RoutedEventArgs e)
		{
			if (!Directory.Exists(this.strRoot))
			{
				Directory.CreateDirectory(this.strRoot);
			}
			string text = AppDomain.CurrentDomain.BaseDirectory;
			text += "Data";
			Process.Start("explorer.exe", text);
		}

		private void Btn_Click_Record(object sender, RoutedEventArgs e)
		{
			if (!this.flagRecord)
			{
				this.btnRecord.Content = "Finished";
				if (!Directory.Exists(this.strRoot))
				{
					Directory.CreateDirectory(this.strRoot);
				}
				string text = DateTime.Now.ToString("yyyy-MM-dd");
				this.strRootCpld = string.Concat(new string[]
				{
					this.strRoot,
					this.strNaming,
					"-",
					text,
					".txt"
				});
				this.fs = new FileStream(this.strRootCpld, FileMode.Create);
				this.sw = new StreamWriter(this.fs);
				this.sw.WriteLine("Dist\tStrength");
				this.flagRecord = true;
				return;
			}
			this.flagRecord = false;
			this.btnRecord.Content = "Record";
			this.sw.Close();
			this.sw.Dispose();
			this.fs.Close();
			this.fs.Dispose();
		}

		private void Btn_Clicked_Freeze(object sender, RoutedEventArgs e)
		{
			if (!this.flagFreeze)
			{
				this.flagFreeze = true;
				this.btnFreeze.Content = "Defreeze";
				return;
			}
			this.flagFreeze = false;
			this.btnFreeze.Content = "Freeze";
		}

		private void btn_Click_Clear(object sender, RoutedEventArgs e)
		{
			this.ddCounter = 0;
			this.dd.Clear();
		}

		private void Btn_Click_CMD(object sender, RoutedEventArgs e)
		{
			if (this.txbxDeviceCMD.Text.Length < 23)
			{
				MessageBox.Show("Wrong Command, please check!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
				return;
			}
			this.MainDataAcquirer.SendMessage(this.txbxDeviceCMD.Text);
		}

		private void Btn_Clicked_About(object sender, RoutedEventArgs e)
		{
			this.aboutWin = new About(this);
			this.aboutWin.ShowDialog();
		}

		private void TxbxNaming_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				string text = this.txbxNaming.Text;
				if (text != null)
				{
					this.strNaming = text;
				}
			}
		}

		private void TxbxDataAmout_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				int num = 0;
				if (int.TryParse(this.txbxDataAmout.Text, out num))
				{
					if (num >= 1 && num <= 5000)
					{
						this.MeanRange = (double)num;
						this.count = 0.0;
						return;
					}
					this.txbxDataAmout.Text = string.Format("{0}", this.MeanRange);
					MessageBox.Show("Please set the value between 1~5000.", "Value error", MessageBoxButton.OK, MessageBoxImage.Hand);
				}
			}
		}

		private void TxbxDeviceCMD_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Back)
			{
				e.Handled = false;
				return;
			}
			if (e.Key == Key.Space)
			{
				e.Handled = false;
				return;
			}
			if ((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9))
			{
				e.Handled = false;
				return;
			}
			if (e.Key >= Key.A && e.Key <= Key.F)
			{
				e.Handled = false;
				return;
			}
			e.Handled = true;
		}

		private void GetComList(ComboBox s)
		{
			RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("Hardware\\DeviceMap\\SerialComm");
			if (registryKey != null)
			{
				string[] arg_24_0 = registryKey.GetValueNames();
				s.Items.Clear();
				string[] array = arg_24_0;
				for (int i = 0; i < array.Length; i++)
				{
					string name = array[i];
					string newItem = (string)registryKey.GetValue(name);
					s.Items.Add(newItem);
				}
			}
		}

		private void ShowOrHideComponents(bool v)
		{
			this.btnFreeze.IsEnabled = v;
			this.btnClear.IsEnabled = v;
			this.txbxDataAmout.IsEnabled = v;
			this.txbxDeviceCMD.IsEnabled = v;
			this.btnRecord.IsEnabled = v;
			this.btnCMDSend.IsEnabled = v;
			this.ckbPixMode.IsEnabled = v;
			this.cmbPort.IsEnabled = !v;
			this.cmbProduct.IsEnabled = !v;
		}

		private void TimerEftPtsCounter_Elapsed(object sender, ElapsedEventArgs e)
		{
			base.Dispatcher.BeginInvoke(new ThreadStart(delegate
			{
				this.eftPtsPerSec = ((this.eftPtsPerSec > 100) ? 100 : this.eftPtsPerSec);
				this.txbxEffectivePoi.Text = string.Format("{0}", this.eftPtsPerSec);
				this.eftPtsPerSec = 0;
			}), new object[0]);
		}

		private void CMB_PMD_COM(object sender, MouseButtonEventArgs e)
		{
			this.GetComList(this.cmbPort);
		}

		private void CMB_SCd_COM(object sender, SelectionChangedEventArgs e)
		{
			try
			{
				this.strPort = this.cmbPort.Items[this.cmbPort.SelectedIndex].ToString();
			}
			catch
			{
			}
		}

		private void CMB_SCd_Product(object sender, SelectionChangedEventArgs e)
		{
			double rg2;
			double rg3;
			double rg4;
			double rg5;
			double rg1 = rg2 = (rg3 = (rg4 = (rg5 = 0.0)));
			try
			{
				string a = (string)this.cmbProduct.Items[this.cmbProduct.SelectedIndex];
				if (!(a == "Benewake TF01"))
				{
					if (!(a == "Benewake TF02"))
					{
						if (!(a == "Benewake TFmini"))
						{
							if (a == "Benewake TFp64")
							{
								rg1 = 20.0;
								rg2 = 40.0;
								rg3 = 60.0;
								rg4 = 80.0;
								rg5 = 100.0;
								TransParam.PdtMode = TransParam.Product.TFp64;
								this.range = 10000.0;
							}
						}
						else
						{
							rg1 = 2.4;
							rg2 = 4.8;
							rg3 = 7.2;
							rg4 = 9.6;
							rg5 = 12.0;
							TransParam.PdtMode = TransParam.Product.TFMini;
							this.range = 1200.0;
						}
					}
					else
					{
						TransParam.PdtMode = TransParam.Product.TF02;
						rg1 = 4.4;
						rg2 = 8.8;
						rg3 = 13.2;
						rg4 = 17.6;
						rg5 = 22.0;
						this.range = 2200.0;
					}
				}
				else
				{
					TransParam.PdtMode = TransParam.Product.TF01;
					rg1 = 2.0;
					rg2 = 4.0;
					rg3 = 6.0;
					rg4 = 8.0;
					rg5 = 10.0;
					this.range = 1000.0;
				}
				base.Dispatcher.BeginInvoke(new ThreadStart(delegate
				{
					this.range2.Text = string.Format("{0}m", rg1);
					this.range3.Text = string.Format("{0}m", rg2);
					this.range4.Text = string.Format("{0}m", rg3);
					this.range5.Text = string.Format("{0}m", rg4);
					this.range6.Text = string.Format("{0}m", rg5);
				}), new object[0]);
			}
			catch (Exception arg_254_0)
			{
				ErrorLog.WriteLog(arg_254_0, "");
			}
		}

		private void TDPixModeOn()
		{
			TransParam.IsConfigOn = true;
			this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOn);
			while (!this.isCMDSent)
			{
				this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOn);
				Thread.Sleep(100);
			}
			this.isCMDSent = false;
			this.MainDataAcquirer.SendCommand(OrderSet.cmdPixOutput_Mini);
			while (!this.isCMDSent)
			{
				this.MainDataAcquirer.SendCommand(OrderSet.cmdPixOutput_Mini);
				Thread.Sleep(100);
			}
			this.isCMDSent = false;
			Thread.Sleep(100);
			this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOff);
			TransParam.IsConfigOn = false;
		}

		private void TD8o9ModeOn()
		{
			TransParam.IsConfigOn = true;
			this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOn);
			while (!this.isCMDSent)
			{
				this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOn);
				Thread.Sleep(100);
			}
			this.isCMDSent = false;
			this.MainDataAcquirer.SendCommand(OrderSet.cmd8o9Output_Mini);
			while (!this.isCMDSent)
			{
				this.MainDataAcquirer.SendCommand(OrderSet.cmd8o9Output_Mini);
				Thread.Sleep(100);
			}
			this.isCMDSent = false;
			this.MainDataAcquirer.SendCommand(OrderSet.cmdOutputFormat_Mini);
			while (!this.isCMDSent)
			{
				this.MainDataAcquirer.SendCommand(OrderSet.cmdOutputFormat_Mini);
				Thread.Sleep(100);
			}
			this.isCMDSent = false;
			Thread.Sleep(100);
			this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOff);
			TransParam.IsConfigOn = false;
		}

		private void TDSwitch()
		{
			TransParam.IsConfigOn = true;
			this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOn_02);
			Thread.Sleep(100);
			this.MainDataAcquirer.EmptyBuffer();
			this.MainDataAcquirer.SendCommand(OrderSet.cmdAutoWorking_02);
			Thread.Sleep(10);
			this.MainDataAcquirer.SendCommand(OrderSet.cmdAutoParameter_02, this.sensorMode, this.prdtMode);
			Thread.Sleep(100);
			this.MainDataAcquirer.SendCommand(OrderSet.cmdReset_02);
			Thread.Sleep(100);
			int num = 0;
			while (true)
			{
				object lockThis = this.MainDataAcquirer.LockThis;
				lock (lockThis)
				{
					num++;
					if (this.MainDataAcquirer.SerialPortReadBuffer.Count >= 8)
					{
						byte[] array = new byte[8];
						for (int i = 0; i < 8; i++)
						{
							array[i] = this.MainDataAcquirer.SerialPortReadBuffer.Dequeue();
						}
						if (array[7] == 144)
						{
							this.MainDataAcquirer.SendCommand(OrderSet.cmdSNReset_02);
							TransParam.IsConfigOn = false;
						}
						else if (array[7] == 145)
						{
							MessageBox.Show("Failed: Pram!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
							this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOff_02);
							TransParam.IsConfigOn = false;
						}
						else if (array[7] == 158)
						{
							MessageBox.Show("Failed: CRC!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
							this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOff_02);
							TransParam.IsConfigOn = false;
						}
						else
						{
							MessageBox.Show("Failed: Unknown" + array[7].ToString("X2"), "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
							TransParam.IsConfigOn = false;
						}
					}
					else
					{
						if (num != 5)
						{
							Thread.Sleep(200);
							continue;
						}
						MessageBox.Show("Failed: no response!", "Error", MessageBoxButton.OK, MessageBoxImage.Hand);
						this.MainDataAcquirer.SendCommand(OrderSet.cmdConfigOff_02);
						TransParam.IsConfigOn = false;
					}
				}
				break;
			}
		}

		[GeneratedCode("PresentationBuildTasks", "4.0.0.0"), DebuggerNonUserCode]
		public void InitializeComponent()
		{
			if (this._contentLoaded)
			{
				return;
			}
			this._contentLoaded = true;
			Uri resourceLocator = new Uri("/WINCC_TF;component/mainwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}

		[GeneratedCode("PresentationBuildTasks", "4.0.0.0"), EditorBrowsable(EditorBrowsableState.Never), DebuggerNonUserCode]
		void IComponentConnector.Connect(int connectionId, object target)
		{
			switch (connectionId)
			{
			case 1:
				((MainWindow)target).SizeChanged += new SizeChangedEventHandler(this.MnW_SCd_Resize);
				return;
			case 2:
				((Button)target).Click += new RoutedEventHandler(this.Btn_Clicked_About);
				return;
			case 3:
				this.cmbProduct = (ComboBox)target;
				this.cmbProduct.SelectionChanged += new SelectionChangedEventHandler(this.CMB_SCd_Product);
				return;
			case 4:
				this.cmbPort = (ComboBox)target;
				this.cmbPort.PreviewMouseDown += new MouseButtonEventHandler(this.CMB_PMD_COM);
				this.cmbPort.SelectionChanged += new SelectionChangedEventHandler(this.CMB_SCd_COM);
				return;
			case 5:
				this.btnConnect = (Button)target;
				this.btnConnect.Click += new RoutedEventHandler(this.Btn_Click_Connect);
				return;
			case 6:
				this.ckbPixMode = (CheckBox)target;
				return;
			case 7:
				this.btnFreeze = (Button)target;
				this.btnFreeze.Click += new RoutedEventHandler(this.Btn_Clicked_Freeze);
				return;
			case 8:
				this.btnClear = (Button)target;
				this.btnClear.Click += new RoutedEventHandler(this.btn_Click_Clear);
				return;
			case 9:
				this.txbxDataAmout = (TextBox)target;
				return;
			case 10:
				this.txbxDeviceCMD = (TextBox)target;
				return;
			case 11:
				this.btnCMDSend = (Button)target;
				this.btnCMDSend.Click += new RoutedEventHandler(this.Btn_Click_CMD);
				return;
			case 12:
				this.txbxNaming = (TextBox)target;
				return;
			case 13:
				this.btnRecord = (Button)target;
				this.btnRecord.Click += new RoutedEventHandler(this.Btn_Click_Record);
				return;
			case 14:
				this.btnFolder = (Button)target;
				this.btnFolder.Click += new RoutedEventHandler(this.Btn_Click_DataFolder);
				return;
			case 15:
				this.txbcDist = (TextBlock)target;
				return;
			case 16:
				this.txbxEffectivePoi = (TextBlock)target;
				return;
			case 17:
				this.txbxStrength = (TextBlock)target;
				return;
			case 18:
				this.plotterTimeLine = (ChartPlotter)target;
				return;
			case 19:
				this.axisTitle = (VerticalAxisTitle)target;
				return;
			case 20:
				this.cvsDymCursor = (Canvas)target;
				return;
			case 21:
				this.lnYAxis = (Line)target;
				return;
			case 22:
				this.lnXAixs5 = (Line)target;
				return;
			case 23:
				this.lnXAixs4 = (Line)target;
				return;
			case 24:
				this.lnXAixs3 = (Line)target;
				return;
			case 25:
				this.lnXAixs2 = (Line)target;
				return;
			case 26:
				this.lnXAixs1 = (Line)target;
				return;
			case 27:
				this.lnXAixs0 = (Line)target;
				return;
			case 28:
				this.range1 = (TextBlock)target;
				return;
			case 29:
				this.range2 = (TextBlock)target;
				return;
			case 30:
				this.range3 = (TextBlock)target;
				return;
			case 31:
				this.range4 = (TextBlock)target;
				return;
			case 32:
				this.range5 = (TextBlock)target;
				return;
			case 33:
				this.range6 = (TextBlock)target;
				return;
			default:
				this._contentLoaded = true;
				return;
			}
		}
	}
}
