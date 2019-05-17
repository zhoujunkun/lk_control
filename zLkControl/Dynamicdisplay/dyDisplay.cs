using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zLkControl.Dynamicdisplay
{
    class dyDisplay : RingArray<Points>
    {
        public dyDisplay() : base(100)
        {
        }
        private const int TOTAL_POINTS = 100;
    }
}
