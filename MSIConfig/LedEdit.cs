using Microsoft.Win32;
using MSIConfig;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;

namespace MSI_Command_Center
{
	public class LedEdit : INotifyPropertyChanged
	{
		public enum LedMode
		{
			LED_Off,
			Breathing,
			Keep_Original
		}

		#region 常量
		private const uint MEMORY_READ_WRITE_LENGTH = 4;

		private const int __MAX_CMD_FRAME__ = 6;

		private const int __MODE_DISABLE__ = 0;

		private const int __MODE_STD_NORMAL__ = 1;

		private const int __MODE_STD_GAMING__ = 2;

		private const int __MODE_STD_BREATHING__ = 3;

		private const int __MODE_STD_AUDIO__ = 4;

		private const int __MODE_STD_WAVE__ = 5;

		private const int __MODE_STD_DUAL_COLOR__ = 6;

		private const int __MODE_IDLE_OFF__ = 7;

		private const int __MODE_IDLE_BREATHING__ = 8;

		private const int __MODE_IDLE_WAVE__ = 9;

		private const int __AREA_TURN_OFF_WHOLE__ = 0;

		private const int __AREA_LEFT__ = 1;

		private const int __AREA_MIDDLE__ = 2;

		private const int __AREA_RIGHT__ = 3;

		private const int __AREA_DUAL_COLOR_LEFT_BALL__ = 4;

		private const int __AREA_DUAL_COLOR_RIGHT_BALL__ = 5;

		private const int __AREA_WHOLE__ = 6;

		private const int __AREA_LEFT_LIGHT_BAR = 5;

		private const int __AREA_RIGHT_LIGHT_BAR = 6;

		private const int __AREA_TOUCHPAD__ = 7;

		private const int __AREA_4_LOGO_FOR_CMD__ = 10;

		private const int __AREA_5_LIGHTBAR_FOR_CMD__ = 13;

		private const int __AREA_5_LEFT_BAR_FOR_CMD__ = 13;

		private const int __AREA_6_RIGHT_BAR_FOR_CMD__ = 16;

		private const int __AREA_7_TOUCHPAD_FOR_CMD__ = 19;

		private const int RampValue = 10;

		private const int __TIMER_INTERVAL_FOR_AUDIO__ = 200;

		private const int __TIMER_INTERVAL_FOR_SENDING_SYSTEM_VOLUMN__ = 5000;

		private const int __TIMER_INTERVAL_FOR_OTHER__ = 20;
		#endregion

		#region 变量
		private int index;

		private bool existsLED;

		private bool canControl_LED_keyboard;

		private bool isChangeLEDcolor;

		public bool m_isEnteringSuspend;

		public bool m_is1T11Mode;

		private bool isSupportLEDOrNnot = true;

		private string[] m_strUSBSymbolicLinks = new string[0];

		private bool m_bIsSelectedModel;

		private bool m_applyWorkStationUI;

		private bool m_bIs1T11Model;

		private bool m_bIs17A1Model;

		private int[] m_iCMD2send;

		private int[] m_iReadDataBuf;

		private int m_iUsbCmdDelay;

		private bool m_bIsInitializing_NotSending0x41CMD;

		private bool m_bIsMinimized_NotSendingAnyCMD;

		private int __USB_COMMAND_DELAY__;

		private int m_iCurrentMode;

		private int m_iCurrentArea;

		private int m_iPreviousArea;

		private int m_iBeforeLockedArea;

		private CUSB m_usb;

		private double EndR;

		private double EndG;

		private double EndB;

		private double StartR;

		private double StartG;

		private double StartB;

		private double[] RampR;

		private double[] RampG;

		private double[] RampB;

		private int m_iTimerInterval;

		private bool isInitializeComponent;

		private string isSupportSingleKB;

		private string inputFilePath;

		private string ouputFilePath;

		private string donputFilePath;

		private string copySourceFilePath;

		private string copySourceFileDestinationPath;

		private string copyWorkstationFilePath;

		private string copyWorkstationFileDestinationPath;

		public string marketingName;

		public List<string> column1_BIOSProductName;

		public List<string> column2_Model;

		public List<string> column3_VGA_MKT;

		public List<string> column4_LED_Wizard_Adjustable;

		public List<string> column5_Backlit_KB;

		public List<string> column6_Multi_Color;

		public List<string> column7_Touchpad_Frame;

		public List<string> column8_Light_Bar;

		public List<string> column9_Default_Color;

		public List<string> column10_Support_USB_Light;

		public int array_length;

		private Visibility supportUSBLED;

		private System.Timers.Timer timersTimer;

		private int LED_Select_mode;

		private int LED_SelectProfileIndex;

		private bool setCurrentLED;

		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		#region 属性
		public int Index
		{
			get
			{
				return this.index;
			}
			set
			{
				this.index = value;
			}
		}

		public bool ExistsLED
		{
			get
			{
				return this.existsLED;
			}
			set
			{
				this.existsLED = value;
				this.OnPropertyChanged("ExistsLED");
			}
		}

		public bool CanControl_LED_keyboard
		{
			get
			{
				return this.canControl_LED_keyboard;
			}
			set
			{
				this.canControl_LED_keyboard = value;
				this.OnPropertyChanged("CanControl_LED_keyboard");
			}
		}

		/*
		public bool LedOnOff
		{
			get
			{
				return Settings.Default.IsLedON;
			}
			set
			{
				Settings.Default.IsLedON = value;
				Helpers.SaveConfig();
				this.OnPropertyChanged("IsLedON");
			}
		}
		// */

		public bool IsChangeLEDcolor
		{
			get
			{
				return this.isChangeLEDcolor;
			}
			set
			{
				this.isChangeLEDcolor = value;
			}
		}

		public bool IsSupportLEDOrNnot
		{
			get
			{
				return this.isSupportLEDOrNnot;
			}
			set
			{
				this.isSupportLEDOrNnot = value;
				this.OnPropertyChanged("IsSupportLEDOrNnot");
			}
		}
		#endregion

		protected virtual void OnPropertyChanged(string propertyName)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		public bool IsInitializeComponent
		{
			get
			{
				return this.isInitializeComponent;
			}
			set
			{
				this.isInitializeComponent = value;
			}
		}

		public string IsSupportSingleKB_Color
		{
			get
			{
				return this.isSupportSingleKB;
			}
			set
			{
				this.isSupportSingleKB = value;
			}
		}

		public Visibility SupportUSBLED
		{
			get
			{
				return this.supportUSBLED;
			}
			set
			{
				this.supportUSBLED = value;
				this.OnPropertyChanged("SupportUSBLED");
			}
		}

		public LedEdit()
		{
			int[] iCMD2send = new int[5];
			this.m_iCMD2send = iCMD2send;
			int[] iReadDataBuf = new int[5];
			this.m_iReadDataBuf = iReadDataBuf;
			this.m_iUsbCmdDelay = 190;
			this.__USB_COMMAND_DELAY__ = 10;
			this.m_iCurrentMode = 1;
			this.m_iCurrentArea = 1;
			this.m_iPreviousArea = 1;
			this.m_iBeforeLockedArea = 1;
			double[] rampR = new double[7];
			this.RampR = rampR;
			double[] rampG = new double[7];
			this.RampG = rampG;
			double[] rampB = new double[7];
			this.RampB = rampB;
			this.m_iTimerInterval = 20;
			this.inputFilePath = System.Windows.Forms.Application.StartupPath + "\\DCLED.csv";
			this.ouputFilePath = System.Windows.Forms.Application.StartupPath + "\\DDCLED.csv";
			this.donputFilePath = "";
			this.copySourceFilePath = "C:\\Windows\\System32\\oobe\\OEM\\DCLED.csv";
			this.copySourceFileDestinationPath = System.Windows.Forms.Application.StartupPath + "\\DCLED.csv";
			this.copyWorkstationFilePath = "C:\\Windows\\System32\\oobe\\OEM\\workstation.txt";
			this.copyWorkstationFileDestinationPath = System.Windows.Forms.Application.StartupPath + "\\workstation.txt";
			this.marketingName = "";
			this.column1_BIOSProductName = new List<string>();
			this.column2_Model = new List<string>();
			this.column3_VGA_MKT = new List<string>();
			this.column4_LED_Wizard_Adjustable = new List<string>();
			this.column5_Backlit_KB = new List<string>();
			this.column6_Multi_Color = new List<string>();
			this.column7_Touchpad_Frame = new List<string>();
			this.column8_Light_Bar = new List<string>();
			this.column9_Default_Color = new List<string>();
			this.column10_Support_USB_Light = new List<string>();
			this.array_length = 8;
			this.supportUSBLED = Visibility.Hidden;

			this.m_strUSBSymbolicLinks = CUSB._EnumPorts_();
			this.ExistsLED = this.SoftwareList();
			if (!this.existsLED)
			{
				this.CanControl_LED_keyboard = true;
			}
		}
		private bool SoftwareList()
		{
			new ArrayList();
			try
			{
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Uninstall");
				if (registryKey == null) return false;
				string[] subKeyNames = registryKey.GetSubKeyNames();
				foreach (var name in subKeyNames)
				{
					RegistryKey registryKey2 = registryKey.OpenSubKey(name);
					if (registryKey2 == null) continue;
					string text = registryKey2.GetValue("DisplayName", "Nothing").ToString();
					if (text.ToLower().Contains("steelseries")) return true;
				}
			}
			catch
			{
			}
			return false;
		}
		
		public enum BoardPartition : byte
		{
			Left = 1,
			Middle = 2,
			Right = 4,
			LeftMiddle = Left | Middle,
			LeftRight = Left | Right,
			MiddleRight = Middle | Right,
			ALL = Left | Middle | Right,
		}

		public void SetLEDColor(BoardPartition partition, Color color)
		{
			//this.Build_And_Send_CMD(65, 1, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			if (partition.HasFlag(BoardPartition.Left)) this.Build_And_Send_CMD(64, 1, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
			if (partition.HasFlag(BoardPartition.Middle)) this.Build_And_Send_CMD(64, 2, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
			if (partition.HasFlag(BoardPartition.Right)) this.Build_And_Send_CMD(64, 3, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);

		}

		public void SetLEDColor(Color LeftColor, Color MiddleColor, Color RightColor)
		{
			//this.Build_And_Send_CMD(65, 1, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 1, (int)LeftColor.R, (int)LeftColor.G, (int)LeftColor.B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 2, (int)MiddleColor.R, (int)MiddleColor.G, (int)MiddleColor.B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 3, (int)RightColor.R, (int)RightColor.G, (int)RightColor.B, this.__USB_COMMAND_DELAY__);

		}

		public bool STD_Normal_Mode()
		{
			Color[] areaColors = new Color[] { Colors.White, Colors.Red, Colors.White };
			try
			{
				string text = (string)Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Dragon Center", "First Launch", "False");
				if (text == "False" || text == null)
				{
					Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Dragon Center", "First Launch", "True", RegistryValueKind.String);
					string text2 = this.column1_BIOSProductName.Find((string item) => item == this.marketingName);
					string oProp = this.marketingName;
					int num = this.column1_BIOSProductName.FindIndex((string item) => item == oProp);
					if (text2 != null && this.array_length >= 9)
					{
						if (this.column9_Default_Color[num] == "Multi (R/G/B)")
						{
							for (int i = 0; i <= 4; i++)
							{
								if (i == 0)
								{
									areaColors[i] = (Color)ColorConverter.ConvertFromString("#FF0000");
								}
								else if (i == 1)
								{
									areaColors[i] = (Color)ColorConverter.ConvertFromString("#00FF00");
								}
								else if (i == 2)
								{
									areaColors[i] = (Color)ColorConverter.ConvertFromString("#0000FF");
								}
								else
								{
									areaColors[i] = (Color)ColorConverter.ConvertFromString("#FF0000");
								}
							}
						}
						else if (this.column9_Default_Color[num] == "Blue")
						{
							for (int j = 0; j <= 4; j++)
							{
								areaColors[j] = (Color)ColorConverter.ConvertFromString("#0000FF");
							}
						}
						else if (this.column9_Default_Color[num] == "Green")
						{
							for (int k = 0; k <= 4; k++)
							{
								areaColors[k] = (Color)ColorConverter.ConvertFromString("#00FF00");
							}
						}
					}
				}
			}
			catch
			{
			}
			this.m_iCurrentMode = 1;
			this.Build_And_Send_CMD(65, 1, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 1, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 2, (int)areaColors[1].R, (int)areaColors[1].G, (int)areaColors[1].B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 3, (int)areaColors[2].R, (int)areaColors[2].G, (int)areaColors[2].B, this.__USB_COMMAND_DELAY__);
			if (this.m_bIsSelectedModel)
			{
				this.Build_And_Send_CMD(64, 4, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 7, (int)areaColors[3].R, (int)areaColors[3].G, (int)areaColors[3].B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 5, (int)areaColors[4].R, (int)areaColors[4].G, (int)areaColors[4].B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 6, (int)areaColors[4].R, (int)areaColors[4].G, (int)areaColors[4].B, this.__USB_COMMAND_DELAY__);
			}
			if (this.m_bIs17A1Model)
			{
				this.Build_And_Send_CMD(64, 4, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 7, (int)areaColors[3].R, (int)areaColors[3].G, (int)areaColors[3].B, this.__USB_COMMAND_DELAY__);
			}
			return true;
		}

		private bool STD_Audio_Mode()
		{
			Color[] areaColors = new[] { Colors.Red, Colors.White, Colors.Red };
			this.m_iCurrentMode = 4;
			this.Build_And_Send_CMD(64, 1, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 2, (int)areaColors[1].R, (int)areaColors[1].G, (int)areaColors[1].B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 3, (int)areaColors[2].R, (int)areaColors[2].G, (int)areaColors[2].B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(65, 4, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			return true;
		}

		private bool STD_Gaming_Mode()
		{
			Color[] areaColors = new[] { Colors.Red, Colors.White, Colors.Red };
			this.m_iCurrentMode = 2;
			this.Build_And_Send_CMD(65, 2, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 1, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, this.__USB_COMMAND_DELAY__);
			return true;
		}

		private bool STD_Wave_Mode()
		{
			this.m_iCurrentMode = 5;
			this.Calculate_RampParameter_and_Set_To_EC();
			this.Build_And_Send_CMD(65, 5, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			return true;
		}

		private double Compute_Ramp_Speed(double StartColor, double EndColor)
		{
			if (StartColor - EndColor == 0.0)
			{
				return 0.0;
			}
			return Math.Round(1000.0 / Math.Round((StartColor - EndColor) / 10.0, 0) / 20.0, 1);
		}

		private void Calculate_RampParameter_and_Set_To_EC()
		{
			Color[] areaColors = new[] { Colors.Red, Colors.White, Colors.Red };
			this.RampR[0] = this.Compute_Ramp_Speed((double)areaColors[0].R, this.EndR);
			this.RampG[0] = this.Compute_Ramp_Speed((double)areaColors[0].G, this.EndR);
			this.RampB[0] = this.Compute_Ramp_Speed((double)areaColors[0].B, this.EndR);
			this.RampR[1] = this.Compute_Ramp_Speed((double)areaColors[1].R, this.EndR);
			this.RampG[1] = this.Compute_Ramp_Speed((double)areaColors[1].G, this.EndR);
			this.RampB[1] = this.Compute_Ramp_Speed((double)areaColors[1].B, this.EndR);
			this.RampR[2] = this.Compute_Ramp_Speed((double)areaColors[2].R, this.EndR);
			this.RampG[2] = this.Compute_Ramp_Speed((double)areaColors[2].G, this.EndR);
			this.RampB[2] = this.Compute_Ramp_Speed((double)areaColors[2].B, this.EndR);
			if (this.m_bIsSelectedModel)
			{
				this.RampR[3] = this.Compute_Ramp_Speed((double)areaColors[0].R, this.EndR);
				this.RampG[3] = this.Compute_Ramp_Speed((double)areaColors[0].G, this.EndR);
				this.RampB[3] = this.Compute_Ramp_Speed((double)areaColors[0].B, this.EndR);
				this.RampR[4] = this.Compute_Ramp_Speed((double)areaColors[4].R, this.EndR);
				this.RampG[4] = this.Compute_Ramp_Speed((double)areaColors[4].G, this.EndR);
				this.RampB[4] = this.Compute_Ramp_Speed((double)areaColors[4].B, this.EndR);
				this.RampR[5] = this.Compute_Ramp_Speed((double)areaColors[4].R, this.EndR);
				this.RampG[5] = this.Compute_Ramp_Speed((double)areaColors[4].G, this.EndR);
				this.RampB[5] = this.Compute_Ramp_Speed((double)areaColors[4].B, this.EndR);
				this.RampR[6] = this.Compute_Ramp_Speed((double)areaColors[3].R, this.EndR);
				this.RampG[6] = this.Compute_Ramp_Speed((double)areaColors[3].G, this.EndR);
				this.RampB[6] = this.Compute_Ramp_Speed((double)areaColors[3].B, this.EndR);
			}
			if (this.m_bIs17A1Model)
			{
				this.RampR[3] = this.Compute_Ramp_Speed((double)areaColors[0].R, this.EndR);
				this.RampG[3] = this.Compute_Ramp_Speed((double)areaColors[0].G, this.EndR);
				this.RampB[3] = this.Compute_Ramp_Speed((double)areaColors[0].B, this.EndR);
				this.RampR[4] = this.Compute_Ramp_Speed((double)areaColors[4].R, this.EndR);
				this.RampG[4] = this.Compute_Ramp_Speed((double)areaColors[4].G, this.EndR);
				this.RampB[4] = this.Compute_Ramp_Speed((double)areaColors[4].B, this.EndR);
				this.RampR[5] = this.Compute_Ramp_Speed((double)areaColors[4].R, this.EndR);
				this.RampG[5] = this.Compute_Ramp_Speed((double)areaColors[4].G, this.EndR);
				this.RampB[5] = this.Compute_Ramp_Speed((double)areaColors[4].B, this.EndR);
				this.RampR[6] = this.Compute_Ramp_Speed((double)areaColors[3].R, this.EndR);
				this.RampG[6] = this.Compute_Ramp_Speed((double)areaColors[3].G, this.EndR);
				this.RampB[6] = this.Compute_Ramp_Speed((double)areaColors[3].B, this.EndR);
			}
			this.SendCmdWithoutThreadToSetExtraAreaColor(1, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[0], (int)this.RampG[0], (int)this.RampB[0]);
			this.SendCmdWithoutThreadToSetExtraAreaColor(4, (int)areaColors[1].R, (int)areaColors[1].G, (int)areaColors[1].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[1], (int)this.RampG[1], (int)this.RampB[1]);
			this.SendCmdWithoutThreadToSetExtraAreaColor(7, (int)areaColors[2].R, (int)areaColors[2].G, (int)areaColors[2].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[2], (int)this.RampG[2], (int)this.RampB[2]);
			if (this.m_bIsSelectedModel)
			{
				this.SendCmdWithoutThreadToSetExtraAreaColor(10, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[3], (int)this.RampG[3], (int)this.RampB[3]);
				this.SendCmdWithoutThreadToSetExtraAreaColor(13, (int)areaColors[4].R, (int)areaColors[4].G, (int)areaColors[4].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[4], (int)this.RampG[4], (int)this.RampB[4]);
				this.SendCmdWithoutThreadToSetExtraAreaColor(16, (int)areaColors[4].R, (int)areaColors[4].G, (int)areaColors[4].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[5], (int)this.RampG[5], (int)this.RampB[5]);
				this.SendCmdWithoutThreadToSetExtraAreaColor(19, (int)areaColors[3].R, (int)areaColors[3].G, (int)areaColors[3].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[6], (int)this.RampG[6], (int)this.RampB[6]);
			}
			if (this.m_bIs17A1Model)
			{
				this.SendCmdWithoutThreadToSetExtraAreaColor(10, (int)areaColors[0].R, (int)areaColors[0].G, (int)areaColors[0].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[3], (int)this.RampG[3], (int)this.RampB[3]);
				this.SendCmdWithoutThreadToSetExtraAreaColor(13, (int)areaColors[4].R, (int)areaColors[4].G, (int)areaColors[4].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[4], (int)this.RampG[4], (int)this.RampB[4]);
				this.SendCmdWithoutThreadToSetExtraAreaColor(16, (int)areaColors[4].R, (int)areaColors[4].G, (int)areaColors[4].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[5], (int)this.RampG[5], (int)this.RampB[5]);
				this.SendCmdWithoutThreadToSetExtraAreaColor(19, (int)areaColors[3].R, (int)areaColors[3].G, (int)areaColors[3].B, (int)this.EndR, (int)this.EndG, (int)this.EndB, (int)this.RampR[6], (int)this.RampG[6], (int)this.RampB[6]);
			}
		}

		private void SendCmdWithRetry(int cmd, int area, int data2, int data3, int data4)
		{
			int usbCmdDelay = this.__USB_COMMAND_DELAY__;
			int num = 1;
			while (!this.Build_And_Send_CMD(cmd, area, data2, data3, data4, usbCmdDelay) && num < 3)
			{
				num++;
				usbCmdDelay = this.__USB_COMMAND_DELAY__ * num;
			}
		}

		private void SendCmdWithoutThreadToSetExtraAreaColor(int area, int StartR, int StartG, int StartB, int EndR, int EndG, int EndB, int RampR, int RampG, int RampB)
		{
			this.SendCmdWithRetry(68, area, StartR, StartG, StartB);
			this.SendCmdWithRetry(68, area + 1, EndR, EndG, EndB);
			this.SendCmdWithRetry(68, area + 2, RampR, RampG, RampB);
		}

		private bool STD_Breathing_Mode()
		{
			this.STD_Normal_Mode();
			this.m_iCurrentMode = 3;
			this.Calculate_RampParameter_and_Set_To_EC();
			this.Build_And_Send_CMD(65, 3, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			return true;
		}

		private void Execute_Selected_Mode(int select_mode, int selectedProfileIndex)
		{
			if (!this.CanControl_LED_keyboard)
			{
				return;
			}
			if (select_mode < 7)
			{
				this.timersTimer.Enabled = false;
				this.timersTimer.Enabled = true;
				this.LED_Select_mode = select_mode;
				this.LED_SelectProfileIndex = selectedProfileIndex;
				return;
			}
			switch (select_mode)
			{
				case 1:
					this.STD_Normal_Mode();
					return;
				case 2:
					this.STD_Gaming_Mode();
					return;
				case 3:
					this.STD_Breathing_Mode();
					return;
				case 4:
					this.STD_Audio_Mode();
					return;
				case 5:
					this.STD_Wave_Mode();
					return;
				case 6:
					break;
				case 7:
					this.IDLE_Off_Mode();
					return;
				case 8:
					this.IDLE_Breathing_Mode();
					return;
				case 9:
					this.IDLE_Wave_Mode();
					break;
				default:
					return;
			}
		}

		private bool IDLE_Off_Mode()
		{
			this.Build_And_Send_CMD(65, 7, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			return true;
		}

		private void SetExtraAreaColor_SingleColor(int area, int StartR, int StartG, int StartB)
		{
			this.SetExtraAreaColor(area, StartR, StartG, StartB, StartR, StartG, StartB, 26, 26, 26);
		}

		private void SetExtraAreaColor(int area, int StartR, int StartG, int StartB, int EndR, int EndG, int EndB, int RampR, int RampG, int RampB)
		{
			this.SendCmdWithoutThreadToSetExtraAreaColor(area, StartR, StartG, StartB, EndR, EndG, EndB, RampR, RampG, RampB);
			if (area == 13)
			{
				this.SendCmdWithoutThreadToSetExtraAreaColor(area + 3, StartR, StartG, StartB, EndR, EndG, EndB, RampR, RampG, RampB);
			}
		}

		private bool IDLE_Wave_Mode()
		{
			this.SetExtraAreaColor_SingleColor(19, 0, 0, 0);
			this.SetExtraAreaColor_SingleColor(13, 0, 0, 0);
			this.SetExtraAreaColor_SingleColor(10, 0, 0, 0);
			this.Calculate_RampParameter_and_Set_To_EC();
			this.Build_And_Send_CMD(65, 9, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			return true;
		}

		private bool IDLE_Breathing_Mode()
		{
			this.Calculate_RampParameter_and_Set_To_EC();
			this.Build_And_Send_CMD(65, 8, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			return true;
		}

		private void usbDelay()
		{
			Thread.Sleep(this.m_iUsbCmdDelay);
		}

		private bool Build_And_Send_CMD(int cmd, int data1, int data2, int data3, int data4, int? usbCmdDelay = null)
		{
			this.m_iCMD2send[0] = cmd;
			this.m_iCMD2send[1] = data1;
			this.m_iCMD2send[2] = data2;
			this.m_iCMD2send[3] = data3;
			this.m_iCMD2send[4] = data4;
			if (usbCmdDelay == null) return true;
			return this.SendCMD(usbCmdDelay.Value);
		}

		public void SetDefaultLED_color()
		{
			if (string.IsNullOrWhiteSpace(this.isSupportSingleKB)) return;
			Color color = (Color)ColorConverter.ConvertFromString(this.isSupportSingleKB);
			this.Build_And_Send_CMD(65, 1, 0, 0, 0, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 1, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 2, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
			this.Build_And_Send_CMD(64, 3, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
			if (this.m_bIsSelectedModel)
			{
				this.Build_And_Send_CMD(64, 4, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 7, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 5, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 6, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
			}
			if (this.m_bIs17A1Model)
			{
				this.Build_And_Send_CMD(64, 4, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
				this.Build_And_Send_CMD(64, 7, (int)color.R, (int)color.G, (int)color.B, this.__USB_COMMAND_DELAY__);
			}
		}

		private void SendCMD()
		{
			this.usbDelay();
			try
			{
				byte b = 0;
				byte[] array = new byte[6];
				array[0] = (byte)this.m_iCMD2send[0];
				array[1] = (byte)this.m_iCMD2send[1];
				array[2] = (byte)this.m_iCMD2send[2];
				array[3] = (byte)this.m_iCMD2send[3];
				array[4] = (byte)this.m_iCMD2send[4];
				if ((array[0] != 65 || !this.m_bIsInitializing_NotSending0x41CMD) && !this.m_bIsMinimized_NotSendingAnyCMD)
				{
					this.m_usb = new CUSB();
					this.m_usb.Open(this.m_strUSBSymbolicLinks[0]);
					this.m_usb.CmdWrite(array, ref b);
					this.m_usb.Close();
				}
			}
			catch
			{
			}
		}

		private bool SendCMD(int usbCmdDelay)
		{
			bool result = false;
			Thread.Sleep(usbCmdDelay);
			try
			{
				byte b = 0;
				byte[] array = new byte[6];
				array[0] = (byte)this.m_iCMD2send[0];
				array[1] = (byte)this.m_iCMD2send[1];
				array[2] = (byte)this.m_iCMD2send[2];
				array[3] = (byte)this.m_iCMD2send[3];
				array[4] = (byte)this.m_iCMD2send[4];
				if ((array[0] != 65 || !this.m_bIsInitializing_NotSending0x41CMD) && !this.m_bIsMinimized_NotSendingAnyCMD)
				{
					this.m_usb = new CUSB();
					this.m_usb.Open(this.m_strUSBSymbolicLinks[0]);
					result = this.m_usb.CmdWrite(array, ref b);
					this.m_usb.Close();
				}
			}
			catch
			{
			}
			return result;
		}
	}//End Class
}