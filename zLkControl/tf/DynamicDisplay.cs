using Microsoft.Research.DynamicDataDisplay.Common;
using System;

namespace WINCC_TFMini
{
	public class DynamicDisplay : RingArray<Points>
	{
		private const int TOTAL_POINTS = 1000;

		public DynamicDisplay() : base(1000)
		{
		}
	}
}
