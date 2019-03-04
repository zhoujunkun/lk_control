using System;

namespace zLkControl
{
	public class Points
	{
		public uint Counter
		{
			get;
			set;
		}

		public double Value
		{
			get;
			set;
		}

		public Points(uint Counter, double Value)
		{
			this.Counter = Counter;
			this.Value = Value;
		}
	}
}
