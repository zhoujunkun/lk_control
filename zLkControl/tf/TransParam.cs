namespace zLkControl
{
    class TransParam
    {
        public static bool IsPixOn
        {
            get
            {
                return TransParam.isPixOn;
            }
            set
            {
                TransParam.isPixOn = value;
            }
        }
        public static bool IsConfigOn
        {
            get
            {
                return TransParam.isConfigOn;
            }
            set
            {
                TransParam.isConfigOn = value;
            }
        }
        public static string PdtMode
        {
            get;
            set;
        }

        private static bool isPixOn = false;
        private static bool isConfigOn = false;

    }
}
