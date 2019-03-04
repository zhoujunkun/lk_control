using System;

namespace WINCC_TFMini
{
	public class OrderSet
	{
		public static byte[] cmdConfigOn = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			1,
			2
		};

		public static byte[] cmdConfigOff = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			0,
			2
		};

		public static byte[] cmdPixOutput_Mini = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			4,
			6
		};

		public static byte[] cmd8o9Output_Mini = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			1,
			6
		};

		public static byte[] cmdOutputFormat_Mini = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			1,
			26
		};

		public static byte[] cmdPixOutput_01 = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			1,
			235
		};

		public static byte[] cmdStandardOutput_01 = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			0,
			235
		};

		public static byte[] cmdReset_02 = new byte[]
		{
			66,
			87,
			2,
			0,
			0,
			0,
			0,
			1
		};

		public static byte[] cmdSNReset_02 = new byte[]
		{
			170,
			85,
			240,
			0,
			255,
			255,
			255,
			255
		};

		public static byte[] cmdConfigOn_02 = new byte[]
		{
			170,
			85,
			240,
			0,
			1,
			0,
			0,
			2
		};

		public static byte[] cmdConfigOff_02 = new byte[]
		{
			170,
			85,
			240,
			0,
			0,
			0,
			0,
			2
		};

		public static byte[] cmdAutoWorking_02 = new byte[]
		{
			170,
			85,
			240,
			0,
			0,
			0,
			0,
			113
		};

		public static byte[] cmdAutoParameter_02 = new byte[]
		{
			1,
			255,
			231,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			0,
			240,
			155,
			71,
			98
		};
	}
}
