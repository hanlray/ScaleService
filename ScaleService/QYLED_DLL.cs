using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace ScaleService
{
    class QYLED_DLL
    {
        private const string Ddll = "QYLED.dll";

        //public delegate void mydelegate(int comMsg);
        //串口通讯回调函数
        //[DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        // public static extern int SetComCallBack(mydelegate TComCallBack); 

        //设置超时
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetReceiveTimeOut(int nTimeOut);

        //设置失败重发次数
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetReSendTimes(int nReSendTimes);

        //设置中文编码
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern void SetChnCodeMode(int nCodeMode);

        //开启服务（TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int OpenServer(int TIPPort);

        //关闭服务（TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int CloseServer();

        //发送实时采集（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendCollectionData_Net(string TshowContent, string TIP, int TnetProtocol, int TtypeNo, int TfontColor, int TfontBody, int TfontSize);
        //发送实时采集（RS232；RS485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendCollectionData_Com(string TshowContent, string Trs485Address, int TrsType, int TcomPort, int TbaudRate, int TtypeNo, int TfontColor, int TfontBody, int TfontSize);

        //实时采集批量发送（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendMulCollectionData_Net(string TshowContent, string TIP, int TnetProtocol, int TtypeNo, int TscreenColor, int TfontColor, int TfontBody, int TfontSize, int TdataIndex, int TdataCount);
        //实时采集批量发送（RS232）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendMulCollectionData_Com(string TshowContent, string Trs485Address, int TrsType, int TcomPort, int TtypeNo, int TscreenColor, int TfontColor, int TfontBody, int TfontSize, int TdataIndex, int TdataCount);

        //发送内码文字（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern byte SendInternalText_Net(string TshowContent, string TcardIP, int TnetProtocol, int TareaWidth, int TareaHigth, int Tuid, int TscreenColor, int TshowStyle, int TshowSpeed, int TstopTime, int TfontColor, int TfontBody, int TfontSize, int TupdateStyle, bool TpowerOffSave);
        
        //发送内码文字（RS232；RS485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendInternalText_Com(string TshowContent, string Trs485Address, int rsType, int TcomPort, int TbaudRate, int TareaWidth, int TareaHigth, int Tuid, int TscreenColor, int TshowStyle, int TshowSpeed, int TstopTime, int TfontColor, int TfontBody, int TfontSize, int TupdateStyle, bool TpowerOffSave);
        //内码文字批量发送（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendMulInternalText_Net(string TshowContent, string TcardIP, int TnetProtocol, int TareaWidth, int TareaHigth, int Tuid, int TscreenColor, int TshowStyle, int TshowSpeed, int TstopTime, int TfontColor, int TfontBody, int TfontSize, int TupdateStyle, bool TpowerOffSave, int TtextIndex, int TtextCount);
        //内码文字批量发送（RS232）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendMulInternalText_Com(string TshowContent, string Trs485Address, int rsType, int TcomPort, int TbaudRat, int TareaWidth, int TareaHigth, int Tuid, int TscreenColor, int TshowStyle, int TshowSpeed, int TstopTime, int TfontColor, int TfontBody, int TfontSize, int TupdateStyle, bool TpowerOffSave, int TtextIndex, int TtextCount);

        //语音播放（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int PlayVoice_Net(string TplayContent, string TcardIP, string Trs485Address, int TnetProtocol, int TrsPort);
        //语音播放（RS232；RS485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int PlayVoice_Com(string TplayContent, string Trs485Address, int TrsType, int TcomPort, int TbaudRate);

        //点播显示页（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int PlayShowPage_Net(string TcardIP, int TnetProtocol, int TshowPageNo);
        //点播显示页（RS232；RS485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int PlayShowPage_Com(string Trs485Address, int TrsType, int Tcomport, int TbaudRate, int TshowPageNo);

        //继电器控制（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int RelaySwitch_Net(string szCardIP, int nNetProtocol, int nCircuitNo, int nSwitchStatus);
        //继电器控制（串口；485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int RelaySwitch_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate, int nCircuitNo, int nSwitchStatus);

        //发送继电器延时（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int RelayDelay_Net(string szCardIP, int nNetProtocol, int nCircuitNo, int nDelayTime);
        //发送继电器延时（串口；485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int RelayDelay_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate, int nCircuitNo, int nDelayTime);

        //设置控制卡亮度（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetBright_Net(string szCardIP, int nNetProtocol, int nPriority, int nBrightValue);

        //发送设置亮度（串口）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SetBright_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate, int nPriority, int nBrightValue);

        //发送开始播放（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int StartPlay_Net(string szCardIP, int nNetProtocol);

        //发送开始播放（串口）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int StartPlay_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate);

        //发送停止播放（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int StopPlay_Net(string szCardIP, int nNetProtocol);

        //发送停止播放（串口）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int StopPlay_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate);

        //发送排队叫号（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendLineUp_Net(string szShowContent, string szCardIP, int nNetProtocol, int nStopTime, int nFontColor, int nLineUpWinAddrNo, bool bLineUpFlash);

        //发送排队叫号（串口）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendLineUp_Com(string szShowContent, string szRS485Address, int nRSType, int nComPort, int nBaudRate, int nStopTime, int nFontColor, int nLineUpWinAddrNo, bool bLineUpFlash);

        //发送图片组（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendImageGroup_Net(string szImageFilePath, string szCardIP, int nNetProtocol, int nAreaWidth, int nAreaHigth, int nUID, int nScreenColor, int nShowStyle, int nShowSpeed,
            int nStopTime, int nUpdateStyle, bool bPowerOffSave, int nImageIndex, int nImageCount);

        //发送图片组（串口）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendImageGroup_Com(string szImageFilePath, string szRS485Address, int nRSType, int nComPort, int nBaudRate, int nAreaWidth, int nAreaHigth, int nUID, int nScreenColor, int nShowStyle, int nShowSpeed, int nStopTime, int nUpdateStyle, bool bPowerOffSave, int nImageIndex, int nImageCount);

        //点播图片组（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int PlayImageGroup_Net(string szCardIP, int nNetProtocol, int nPlayType, int nItemNum, int nAreaNo, int nImageStartNo, int nImageNum, int nShowStyle, int nShowSpeed,
            int nStopTime, bool bUpdateNow, bool nPowerOffSave);

        //点播图片组（串口；485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int PlayImageGroup_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate, int nPlayType, int nItemNum, int nAreaNo, int nImageStartNo, int nImageNum, int nShowStyle, int nShowSpeed, int nStopTime, bool bUpdateNow, bool nPowerOffSave);

        //发送日期时间（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendDateTime_Net(string szCardIP, int nNetProtocol, int nUID, int nScreenColor, int nNumColor, int nChrColor, int nFontBody, int nFontSize, int nYearLen, int nTimeFormat, int nShowFormat, int nTimeDifSet, int nHourSpan, int nMinSpan, int nStopTime);

        //发送日期时间（串口）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendDateTime_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate, int nUID, int nScreenColor, int nNumColor, int nChrColor, int nFontBody, int nFontSize, int nYearLen, int nTimeFormat, int nShowFormat, int nTimeDifSet, int nHourSpan, int nMinSpan, int nStopTime);

        //时间校验（UDP；TCP）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int TimeCheck_Net(string szCardIP, int nNetProtocol);

        //时间校验（串口；485）
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int TimeCheck_Com(string szRS485Address, int nRSType, int nComPort, int nBaudRate);

        //添加显示页
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddShowPage(string szStartTime, string szStopTime, int nWeekDay);

        //添加区域
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddArea(int nXPos, int nYPos, int nAreaWidth, int nAreaHigth);

        //添加内码文本模板
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddTemplate_InternalText(string szShowContent, int nUID, int nScreenColor, int nShowStyle, int nShowSpeed, int nStopTime, int nFontColor, int nFontBody, int nFontSize, bool bPowerOffSave);

        //添加实时采集模板
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddTemplate_CollectData(string szShowContent, int nTypeNo, int nScreenColor, int nFontColor, int nFontBody, int nFontSize);

        //添加日期时间模板
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddTemplate_DateTime(int nUID, int nScreenColor, int nNumColor, int nChrColor, int nFontBody, int nFontSize, int nYearLen, int nTimeFormat, int nShowFormat,
        int nTimeDifSet, int nHourSpan, int nMinSpan, int nStopTime, bool bPowerOffSave);

        //添加图片组模板
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddTemplate_ImageGroup(string szImageFilePaths, int nUID, int nScreenColor, int nShowStyle, int nShowSpeed, int nStopTime);

        //添加排队叫号模板
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int AddTemplate_LineUp(string szShowContent, int nStopTime, int nFontColor, int nFontBody, int nFontSize, int nLineUpWinAddrNo, bool bLineUpFlash);

        //发送模板
        [DllImport(Ddll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SendTemplateData_Net(string szCardIP, int nNetProtocol);
    }
}
