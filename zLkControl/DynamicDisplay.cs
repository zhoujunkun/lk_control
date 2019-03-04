using Microsoft.Research.DynamicDataDisplay.Common;
namespace zLkControl
{
    class DynamicDisplay :RingArray<Points>
    {
        public DynamicDisplay() : base(100)
        {
        }
        private const int TOTAL_POINTS = 100;
    }
}
