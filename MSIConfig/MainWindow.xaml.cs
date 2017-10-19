using MSI_Command_Center;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSIConfig
{
	/// <summary>
	/// MainWindow.xaml 的交互逻辑
	/// </summary>
	public partial class MainWindow : Window
	{

		private static CancellationTokenSource CTS = null;
		private void TestQQ(object CTS)
		{
			CancellationTokenSource LetCTS = CTS as CancellationTokenSource;
			TencentQQ.qqOnlineWebServiceSoapClient QQOWSC = null;
			QQOWSC = new TencentQQ.qqOnlineWebServiceSoapClient();
			while (!LetCTS.IsCancellationRequested)
			{
				try
				{
					string Result = QQOWSC.qqCheckOnline("805614612");
					QQOnline = Result == "Y";
				}
				catch
				{
					QQOnline = false;
				}
				Thread.Sleep(1000);
			}
		}
		public MainWindow()
		{
			InitializeComponent();
		}

		LedEdit LE = new LedEdit();

		private bool QQOnline = false;

		/// <summary>
		/// 快速全色流水
		/// </summary>
		private void Type1()
		{
			int Iime = 500;
			while (true)
			{
				while (QQOnline)
				{
					LE.SetLEDColor(Colors.Blue, Colors.Green, Colors.Blue);
					Thread.Sleep(Iime);
					LE.SetLEDColor(Colors.Green, Colors.Blue, Colors.Blue);
					Thread.Sleep(Iime);
					LE.SetLEDColor(Colors.Blue, Colors.Blue, Colors.Green);
					Thread.Sleep(Iime);
				}
				while (!QQOnline)
				{
					LE.SetLEDColor(Colors.Red, Colors.Red, Colors.Green);
					Thread.Sleep(Iime);
					LE.SetLEDColor(Colors.Green, Colors.Red, Colors.Red);
					Thread.Sleep(Iime);
					LE.SetLEDColor(Colors.Red, Colors.Green, Colors.Red);
					Thread.Sleep(Iime);
				}
			}
		}

		/// <summary>
		/// 双色慢速来回
		/// </summary>
		private void Type2()
		{
			Color C1 = Colors.Green;
			Color C2 = Colors.Blue;
			while (true)
			{
				LE.SetLEDColor(LedEdit.BoardPartition.Middle, C1);
				LE.SetLEDColor(LedEdit.BoardPartition.Left, C2);
				Thread.Sleep(400);
				LE.SetLEDColor(LedEdit.BoardPartition.Middle, C2);
				LE.SetLEDColor(LedEdit.BoardPartition.Left, C1);
				Thread.Sleep(400);
				LE.SetLEDColor(LedEdit.BoardPartition.Right, C2);
				LE.SetLEDColor(LedEdit.BoardPartition.Middle, C1);
				Thread.Sleep(400);
				LE.SetLEDColor(LedEdit.BoardPartition.Right, C1);
				LE.SetLEDColor(LedEdit.BoardPartition.Middle, C2);
				Thread.Sleep(400);
			}
		}

		/// <summary>
		/// 快速随机
		/// </summary>
		private void Type3()
		{
			Random R = new Random((int)DateTime.Now.Ticks);
			while (true)
			{
				LE.SetLEDColor(LedEdit.BoardPartition.Right, Color.FromArgb(0xFF, (byte)(R.Next() % 256), (byte)(R.Next() % 256), (byte)(R.Next() % 256)));
				Thread.Sleep(500);
				LE.SetLEDColor(LedEdit.BoardPartition.Middle, Color.FromArgb(0xFF, (byte)(R.Next() % 256), (byte)(R.Next() % 256), (byte)(R.Next() % 256)));
				Thread.Sleep(500);
				LE.SetLEDColor(LedEdit.BoardPartition.Left, Color.FromArgb(0xFF, (byte)(R.Next() % 256), (byte)(R.Next() % 256), (byte)(R.Next() % 256)));
				Thread.Sleep(500);
			}
		}

		/// <summary>
		/// 鼠标指向
		/// </summary>
		private void Type4()
		{
			Random R = new Random((int)DateTime.Now.Ticks);
			while (true)
			{
				LE.SetLEDColor(LedEdit.BoardPartition.ALL,ColorPickerManager.GetColor(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
				Thread.Sleep(500);
			}
		}


		private void Grid_Loaded(object sender, RoutedEventArgs e)
		{
			LE.STD_Normal_Mode();
			LE.SetLEDColor(LedEdit.BoardPartition.ALL, Colors.Black);
			Task.Factory.StartNew(Type4);
			CTS = new CancellationTokenSource();
			Task.Factory.StartNew(TestQQ, CTS);
			//Thread[] Ts = (from ProcessThread t in Process.GetCurrentProcess().Threads select t).ToArray();
			//Thread.
			//Ts = Ts;
			//LE.Build_And_Send_CMD(64, 1, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, this.__USB_COMMAND_DELAY__);
		}
	}
}
