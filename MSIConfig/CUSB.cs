using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using USB2SMBUS;

internal class CUSB
{
	private const ushort __KORYO_VENDOR_ID__ = 6000;

	private const ushort __KORYO_PRODUCT_ID__ = 65280;

	public const ushort __REPORT_FEATURE_COMMAND_SIZE__ = 8;

	public const ushort __REPORT_FEATURE_BLOCK_SIZE__ = 256;

	private const string __USB_IMPLEMENT_DEMO_VERSION__ = "UF1.0";

	public const byte USER_DEFINE_READ = 1;

	public const byte USER_DEFINE_WRITE = 2;

	public const byte USER_DEFINE_VERSION = 16;

	public const byte USER_DEFINE_BLC_READ = 64;

	public const byte USER_DEFINE_BLC_WRITE = 128;

	public const byte USB_ID_COMMAND = 1;

	public const byte USB_ID_DATA = 2;

	public const byte USB_ID_BIOS_DATA = 3;

	private const byte __STATUS_BYTE__ = 7;

	private static int __USB_RETRY_COUNT__ = 500;

	private IntPtr m_intPtrUSB;

	public void Open(string strPortName)
	{
		this.m_intPtrUSB = FIO.CreateFile(strPortName, 2147483648u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
		if (-1 == this.m_intPtrUSB.ToInt32())
		{
			throw new Exception(CMessages.GetLastWin32ErrorToString());
		}
	}

	public void Close()
	{
		FIO.CloseHandle(this.m_intPtrUSB);
	}

	public bool WriteBlock(byte[] blk, ref byte[] dataByte, ref byte status)
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] array3 = new byte[blk.Length + 1];
		array[0] = 1;
		array[1] = 128;
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = 0;
		}
		array2[0] = 1;
		array2[1] = 128;
		array[2] = dataByte[0];
		array[3] = dataByte[1];
		array[4] = dataByte[2];
		array[5] = dataByte[3];
		array[6] = dataByte[4];
		array[7] = dataByte[5];
		Array.Copy(blk, 0, array3, 1, blk.Length);
		array3[0] = 2;
		if (!HID.HidD_SetFeature(this.m_intPtrUSB, array, (uint)array.Length) || !HID.HidD_SetFeature(this.m_intPtrUSB, array3, (uint)array3.Length))
		{
			return false;
		}
		if (HID.HidD_GetFeature(this.m_intPtrUSB, array2, (uint)array2.Length))
		{
			status = array2[7];
			return true;
		}
		return false;
	}

	public bool ReadBlock(ref byte[] blk, ref byte[] dataByte, ref byte status)
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		byte[] array3 = new byte[blk.Length + 1];
		array[0] = 1;
		array[1] = 64;
		array[2] = dataByte[0];
		array[3] = dataByte[1];
		array[4] = dataByte[2];
		array[5] = dataByte[3];
		array[6] = dataByte[4];
		array[7] = dataByte[5];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = 0;
		}
		array2[0] = 1;
		array2[1] = 64;
		for (int j = 0; j < array3.Length; j++)
		{
			array3[j] = 0;
		}
		array3[0] = 2;
		if (!HID.HidD_SetFeature(this.m_intPtrUSB, array, (uint)array.Length))
		{
			return false;
		}
		Thread.Sleep(1);
		if (HID.HidD_GetFeature(this.m_intPtrUSB, array3, (uint)array3.Length))
		{
			Array.Copy(array3, 0, blk, 0, blk.Length);
			return true;
		}
		return false;
	}

	public bool CmdWrite(byte[] dat, ref byte status)
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		array[0] = 1;
		array[1] = 2;
		Array.Copy(dat, 0, array, 2, dat.Length);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = 0;
		}
		array2[0] = 1;
		array2[1] = 2;
		if (!HID.HidD_SetFeature(this.m_intPtrUSB, array, (uint)array.Length))
		{
			return false;
		}
		Thread.Sleep(5);
		int j;
		for (j = 0; j < CUSB.__USB_RETRY_COUNT__; j++)
		{
			if (HID.HidD_GetFeature(this.m_intPtrUSB, array2, (uint)array2.Length))
			{
				status = array2[7];
				return true;
			}
			HID.HidD_FlushQueue(this.m_intPtrUSB);
			Thread.Sleep(5);
		}
		if (j == CUSB.__USB_RETRY_COUNT__)
		{
			throw new Exception(CMessages.GetLastWin32ErrorToString());
		}
		return false;
	}

	public void CmdRead(ref byte[] dat)
	{
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		array[0] = 1;
		array[1] = 1;
		Array.Copy(dat, 0, array, 2, dat.Length);
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = 0;
		}
		array2[0] = 1;
		array2[1] = 1;
		int j;
		for (j = 0; j < CUSB.__USB_RETRY_COUNT__; j++)
		{
			if (HID.HidD_SetFeature(this.m_intPtrUSB, array, (uint)array.Length) && HID.HidD_GetFeature(this.m_intPtrUSB, array2, (uint)array2.Length))
			{
				Array.Copy(array2, 2, dat, 0, dat.Length);
				break;
			}
			HID.HidD_FlushQueue(this.m_intPtrUSB);
		}
		if (j == CUSB.__USB_RETRY_COUNT__) throw new Exception(CMessages.GetLastWin32ErrorToString());
	}

	private static bool GetVersion(IntPtr intPtrUSB, ref string strVersion)
	{
		bool result = false;
		byte[] array = new byte[8];
		byte[] array2 = new byte[8];
		array[0] = 1;
		array[1] = 16;
		array2[0] = 1;
		array2[1] = 16;
		for (int i = 0; i < CUSB.__USB_RETRY_COUNT__; i++)
		{
			if (HID.HidD_SetFeature(intPtrUSB, array, (uint)array.Length) && HID.HidD_GetFeature(intPtrUSB, array2, (uint)array2.Length))
			{
				strVersion = string.Format("{0:c}{1:c}{2:c}{3:c}{4:c}", new object[]
				{
					(char)array2[2],
					(char)array2[3],
					(char)array2[4],
					(char)array2[5],
					(char)array2[6]
				});
				result = true;
				break;
			}
			HID.HidD_FlushQueue(intPtrUSB);
			Thread.Sleep(1);
		}
		return result;
	}

	private static string[] GetHidDevInterface(ushort uVendorId, ushort uProductId)
	{
		string[] allHIDInterFace;
		try
		{
			allHIDInterFace = CUSB.GetAllHIDInterFace();
			if (allHIDInterFace == null || allHIDInterFace.Length == 0)
			{
				string[] result = null;
				return result;
			}
		}
		catch
		{
			string[] result = null;
			return result;
		}
		ArrayList arrayList = new ArrayList();
		for (int i = 0; i < allHIDInterFace.Length; i++)
		{
			IntPtr intPtr = FIO.CreateFile(allHIDInterFace[i], 2147483648u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
			if (-1 != intPtr.ToInt32())
			{
				HID.HIDD_ATTRIBUTES hIDD_ATTRIBUTES = default(HID.HIDD_ATTRIBUTES);
				hIDD_ATTRIBUTES.Size = (uint)Marshal.SizeOf(hIDD_ATTRIBUTES);
				if (HID.HidD_GetAttributes(intPtr, ref hIDD_ATTRIBUTES) && uVendorId == hIDD_ATTRIBUTES.VendorID && uProductId == hIDD_ATTRIBUTES.ProductID) arrayList.Add(allHIDInterFace[i]);
				FIO.CloseHandle(intPtr);
			}
		}
		return (string[])arrayList.ToArray(typeof(string));
	}

	private static string[] GetAllHIDInterFace()
	{
		uint num = 0u;
		ArrayList arrayList = new ArrayList();
		try
		{
			Kernel32.SYSTEM_INFO sYSTEM_INFO = default(Kernel32.SYSTEM_INFO);
			Kernel32.GetSystemInfo(out sYSTEM_INFO);
			bool flag = sYSTEM_INFO.sysInfoEx.wProcessorArchitecture == 9 || sYSTEM_INFO.sysInfoEx.wProcessorArchitecture == 6;
			Guid guid = default(Guid);
			HID.HidD_GetHidGuid(ref guid);
			DM.SP_DEVINFO_DATA sP_DEVINFO_DATA = default(DM.SP_DEVINFO_DATA);
			sP_DEVINFO_DATA.cbSize = Marshal.SizeOf(sP_DEVINFO_DATA);
			IntPtr deviceInfoSet = DM.SetupDiGetClassDevs(ref guid, null, 0, 18u);
			if (!Environment.Is64BitOperatingSystem)
			{
				throw new Exception(CMessages.GetLastWin32ErrorToString());
			}
			else if (-1L == deviceInfoSet.ToInt64())
			{
				throw new Exception(CMessages.GetLastWin32ErrorToString());
			}
			while (DM.SetupDiEnumDeviceInfo(deviceInfoSet, num, ref sP_DEVINFO_DATA))
			{
				DM.SP_DEVICE_INTERFACE_DATA sP_DEVICE_INTERFACE_DATA = default(DM.SP_DEVICE_INTERFACE_DATA);
				sP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(sP_DEVICE_INTERFACE_DATA);
				uint num2 = 0u;
				while (DM.SetupDiEnumDeviceInterfaces(deviceInfoSet, ref sP_DEVINFO_DATA, ref guid, num2++, ref sP_DEVICE_INTERFACE_DATA))
				{
					int num3 = 0;
					if (!DM.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, IntPtr.Zero, 0, ref num3, IntPtr.Zero) && 122 != Marshal.GetLastWin32Error()) throw new Exception(CMessages.GetLastWin32ErrorToString());
					int num4 = 0;
					IntPtr intPtr = Marshal.AllocHGlobal(num3);
					Marshal.WriteInt32(intPtr, flag ? 8 : (Marshal.SizeOf(0u) + Marshal.SystemDefaultCharSize));
					if (!DM.SetupDiGetDeviceInterfaceDetail(deviceInfoSet, ref sP_DEVICE_INTERFACE_DATA, intPtr, num3, ref num4, IntPtr.Zero)) throw new Exception(CMessages.GetLastWin32ErrorToString());
					IntPtr ptr;
					if (Environment.Is64BitOperatingSystem)
					{
						ptr = new IntPtr(intPtr.ToInt64() + (long)Marshal.SizeOf(0u));
					}
					else
					{
						ptr = new IntPtr(intPtr.ToInt32() + Marshal.SizeOf(0u));
					}
					arrayList.Add(Marshal.PtrToStringAuto(ptr));
					Marshal.FreeHGlobal(intPtr);
				}
				num += 1u;
			}
			DM.SetupDiDestroyDeviceInfoList(deviceInfoSet);
		}
		catch (Exception ex)
		{
			MessageBox.Show(ex.ToString());
		}
		return (string[])arrayList.ToArray(typeof(string));
	}

	public static string[] _EnumPorts_()
	{
		List<string> arrayList = new List<string>();
		string[] hidDevInterface = CUSB.GetHidDevInterface(6000, 65280);
		if (hidDevInterface == null) return arrayList.ToArray();
		for (int i = 0; i < hidDevInterface.Length; i++)
		{
			IntPtr intPtr = FIO.CreateFile(hidDevInterface[i], 2147483648u, 3u, IntPtr.Zero, 3u, 0u, IntPtr.Zero);
			if (intPtr.ToInt32() == -1) continue;
			string a = "";
			if (CUSB.GetVersion(intPtr, ref a) && a == "UF1.0") arrayList.Add(hidDevInterface[i]);
			FIO.CloseHandle(intPtr);
		}
		return arrayList.ToArray();
	}
}