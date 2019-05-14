namespace zLkControl
{
   public class LKSensorCmd
    {
        //id 类型命令分配
/*
,---------+-----------+------------+-----------+------------+- - - -+-------------,
| PROUDCT | BAUD_RATE | LIMIR_TRIG | RED_LIGHT | FRONT_BASE |
|    1    |     4     |      2     |     1     |      1     | ...   |          | <- size (bytes)
'---------+-----------+------------+-----------+------------+- - - -+-------------'         
*/
        public enum  FRAME_TYPE { DataGet = 1, ParmsSave,ParamGet,Upload,ACK,QC,Erro };
        public enum FRAME_GetDataID { DistOnce = 1, DistContinue,DistStop};
        public enum FRAME_AckID {   downLoadBegin=1, upload };
        public enum FRAME_GetParamID {ParamAll=1};
        public enum FRAME_SpeeCtlID { START = 1,STOP };
        public enum FRAME_QCcmdID { stand_start = 1, stand_stop };  //标定开始
        public enum FRAME_ParmSaveID { BarudSave = 1, RedLightSave, FrontOrBase,AutoMel};

        public enum FRAME_FRONT_BASE { BASE=0, FRONT};
        public enum FRAME_RED_LIGHT { OFF = 0, ON };
        public enum FRAME_AUTO_MEL{ OFF = 0, ON };

        public bool isbase;  //后基准标记
        public bool isRedLedOn; //红外激光是否打开
        public int parmBarudIndex;   //参数波特率选择
        public bool isSensosAutoCel;   //是否自动开机测量

        public byte parmBarudRateByteSize = 4;
        public byte parmProductByteSize = 1;
        public byte parmLimitTrigByteSize = 1;
        public byte parmRedLightByteSize = 1;
        public byte parmFrontBaseByteSize = 1;

        public byte parmStandDistByteSize = 1;

        public byte stand_distance { set; get; }   //b标定距离

    }
}
