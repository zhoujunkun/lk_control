using System;
using System.IO;
namespace zLkControl
{
    class ErrorLog
    {
        public static void WriteLog(Exception ex, string LogAddress = "")
        {
            if (LogAddress == "")
            {
                LogAddress = string.Concat(new object[]
                {
                    Environment.CurrentDirectory,
                    "\\",
                    DateTime.Now.Year,
                    "-",
                    DateTime.Now.Month,
                    "-",
                    DateTime.Now.Day,
                    "_Log.log"
                });
            }
            StreamWriter streamWriter = new StreamWriter(LogAddress, true);
            streamWriter.WriteLine("Current Time: " + DateTime.Now.ToString());
            streamWriter.WriteLine("Error Info: " + ex.Message);
            streamWriter.WriteLine("Error Source: " + ex.Source);
            streamWriter.WriteLine("Error Stack: " + ex.StackTrace.Trim());
            streamWriter.WriteLine("Trigger Mode: " + ex.TargetSite);
            streamWriter.WriteLine();
            streamWriter.Close();
        }
    }
}
