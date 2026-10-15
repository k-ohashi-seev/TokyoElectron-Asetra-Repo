Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports RFIDAM_WebService.Controllers
Imports System.Data.Entity

<TestClass()> Public Class HtEntryInventDataCmd

    <TestMethod()> Public Sub TestHtEntryInventDataCmd()

        'log4netの設定
        log4net.Config.XmlConfigurator.Configure()
        Dim logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Dim a As HtEntryInventDataCmdReqParameter = New HtEntryInventDataCmdReqParameter
        a.ie = New EntryInventDataDetail() {
            New EntryInventDataDetail With {.an = "00000001", .ps = "N", .lc = "200", .mc = "110", .sc = "101", .ec = "1234", .pt = "20190724170000"}}
        '    New EntryInventDataDetail With {.assetNo = "00000002", .phyinvStatus = "N", .phyinvLocLCd = "200", .phyinvLocMCd = "110", .phyinvLocSCd = "101", .phyinvEmployeeCd = "123", .phyinvTs = "20190716133000"}
        '}
        a.td = CommonConst.PROCESS_TOOL_H

        Dim controlloer = New HtEntryInventDataCmdController()
        Dim ret = controlloer.Post(a)
        logger.Info($"戻り値：{Newtonsoft.Json.JsonConvert.SerializeObject(ret)}")
    End Sub

End Class