Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports RFIDAM_WebService.Controllers
Imports System.Data.Entity

<TestClass()> Public Class HtEntryLogCmd

    <TestMethod()> Public Sub TestHtEntryLogDataCmd()

        'log4netの設定
        log4net.Config.XmlConfigurator.Configure()
        Dim logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Dim a As HtEntryLogCmdReqParameter = New HtEntryLogCmdReqParameter
        a.logDataDetailList = New EntryLogDataDetail() {
            New EntryLogDataDetail With {.opeLogId = "1", .processDt = "20190723", .processTm = "202010000", .processCat = "EV", .processContents = "単体テスト2", .opeEmployeeCd = "1234"}}
        'New EntryLogDataDetail With {.processLogId = "3", .processDt = "20190717", .processTm = "104230000", .processCat = "EV", .locLCd = "200", .locMCd = "110", .processContents = "テスト3", .opeEmployeeCd = "1234"},
        'New EntryLogDataDetail With {.processLogId = "2", .processDt = "20190717", .processTm = "104023000", .processCat = "EV", .locLCd = "200", .processContents = "テスト2", .opeEmployeeCd = "1234"}
        '}

        Dim controlloer = New HtEntryLogCmdController()
        Dim ret = controlloer.Post(a)
        logger.Info($"戻り値：{Newtonsoft.Json.JsonConvert.SerializeObject(ret)}")
    End Sub

End Class