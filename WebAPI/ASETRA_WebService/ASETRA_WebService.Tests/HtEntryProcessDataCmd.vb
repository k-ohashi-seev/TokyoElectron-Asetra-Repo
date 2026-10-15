Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports RFIDAM_WebService.Controllers

<TestClass()> Public Class HtEntryProcessDataCmd

    <TestMethod()> Public Sub TestHtEntryProcessDataCmd()

        'log4netの設定
        log4net.Config.XmlConfigurator.Configure()
        Dim logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Dim test As HtEntryProcessDataCmdReqParameter = New HtEntryProcessDataCmdReqParameter
        Dim dt = Date.Now.ToString("yyyyMMdd")
        Dim tm = Date.Now.ToString("HHmmss") + "000"

        'test.processDataList = New EntryProcessDataDetail() {
        '    New EntryProcessDataDetail With {.processLogId = "901", .processDt = "20220311", .processTm = "120000000" _
        '    , .assetNo = "870001", .assetId = "341530C29C0000000000204B", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220311", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "902", .processDt = "20220311", .processTm = "120000000" _
        '    , .assetNo = "870002", .assetId = "341530C29C0000000000204C", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220311", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "903", .processDt = "20220314", .processTm = "120000000" _
        '    , .assetNo = "870003", .assetId = "341530C29C0000000000204D", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220314", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "904", .processDt = "20220316", .processTm = "120000000" _
        '    , .assetNo = "870004", .assetId = "341530C29C0000000000204E", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220316", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "905", .processDt = "20220316", .processTm = "120000000" _
        '    , .assetNo = "870005", .assetId = "341530C29C0000000000204F", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220316", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "906", .processDt = "20220307", .processTm = "120000000" _
        '    , .assetNo = "870006", .assetId = "341530C29C00000000002050", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220307", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "907", .processDt = "20220311", .processTm = "120000000" _
        '    , .assetNo = "870007", .assetId = "341530C29C00000000002051", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220311", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "908", .processDt = "20220314", .processTm = "120000000" _
        '    , .assetNo = "870008", .assetId = "341530C29C00000000002052", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220314", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "909", .processDt = "20220316", .processTm = "120000000" _
        '    , .assetNo = "870009", .assetId = "341530C29C00000000002053", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220316", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "910", .processDt = "20220317", .processTm = "120000000" _
        '    , .assetNo = "870010", .assetId = "341530C29C00000000002054", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220317", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "911", .processDt = "20220321", .processTm = "120000000" _
        '    , .assetNo = "870011", .assetId = "341530C29C00000000002055", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220321", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "912", .processDt = "20220322", .processTm = "120000000" _
        '    , .assetNo = "870012", .assetId = "341530C29C00000000002056", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220322", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"} _
        '    , New EntryProcessDataDetail With {.processLogId = "913", .processDt = "20220322", .processTm = "120000000" _
        '    , .assetNo = "870013", .assetId = "341530C29C00000000002057", .processDiv = "BI", .leasePlaceCd = "100000" _
        '    , .leaseDueDt = "20220322", .remark = "TEST", .leaseEmployeeCd = "3240", .employeeCd = "3240", .processToolDiv = "H"}
        '}

        ''貸出
        'test.processDataList = New EntryProcessDataDetail() {
        '    New EntryProcessDataDetail With {.processLogId = "10", .processDt = "20230828", .processTm = "000000000", .assetNo = "IP0010", .assetId = "", .processDiv = "BO", .leasePlaceCd = "", .leaseDueDt = "20230830", .remark = "TEST", .leaseEmployeeCd = "ADMIN", .employeeCd = "1001", .processToolDiv = "H"},
        '    New EntryProcessDataDetail With {.processLogId = "11", .processDt = "20230828", .processTm = "000000000", .assetNo = "IP0010", .assetId = "", .processDiv = "BO", .leasePlaceCd = "", .leaseDueDt = "20230831", .remark = "TEST", .leaseEmployeeCd = "ADMIN", .employeeCd = "4563", .processToolDiv = "H"}
        '}
        ''返却
        'test.processDataList = New EntryProcessDataDetail() {
        '    New EntryProcessDataDetail With {.processLogId = "20", .processDt = "20230829", .processTm = "000000000", .assetNo = "IP0010", .assetId = "", .processDiv = "BI", .leasePlaceCd = "", .leaseDueDt = "", .remark = "", .leaseEmployeeCd = "ADMIN", .employeeCd = "ADMIN", .processToolDiv = "H"},
        '    New EntryProcessDataDetail With {.processLogId = "21", .processDt = "20230830", .processTm = "000000000", .assetNo = "IP0001", .assetId = "", .processDiv = "BI", .leasePlaceCd = "", .leaseDueDt = "", .remark = "", .leaseEmployeeCd = "4563", .employeeCd = "4563", .processToolDiv = "H"},
        '    New EntryProcessDataDetail With {.processLogId = "22", .processDt = "20230831", .processTm = "000000000", .assetNo = "IP0010", .assetId = "", .processDiv = "BI", .leasePlaceCd = "", .leaseDueDt = "", .remark = "", .leaseEmployeeCd = "4563", .employeeCd = "4563", .processToolDiv = "H"}
        '}
        '貸出・返却・貸出
        test.processDataList = New EntryProcessDataDetail() {
            New EntryProcessDataDetail With {.processLogId = "10", .processDt = "20251028", .processTm = "000000001", .assetNo = "IP0010", .assetId = "", .processDiv = "BO", .leasePlaceCd = "", .leaseDueDt = "20251030", .remark = "TEST", .leaseEmployeeCd = "ADMIN", .employeeCd = "1001", .processToolDiv = "H"},
            New EntryProcessDataDetail With {.processLogId = "11", .processDt = "20251028", .processTm = "000000002", .assetNo = "IP0001", .assetId = "", .processDiv = "BO", .leasePlaceCd = "", .leaseDueDt = "20251031", .remark = "TEST", .leaseEmployeeCd = "ADMIN", .employeeCd = "4563", .processToolDiv = "H"},
            New EntryProcessDataDetail With {.processLogId = "12", .processDt = "20251029", .processTm = "000000003", .assetNo = "IP0010", .assetId = "", .processDiv = "BO", .leasePlaceCd = "", .leaseDueDt = "20251030", .remark = "TEST", .leaseEmployeeCd = "ADMIN", .employeeCd = "1001", .processToolDiv = "H"},
            New EntryProcessDataDetail With {.processLogId = "13", .processDt = "20251030", .processTm = "000000004", .assetNo = "IP0010", .assetId = "", .processDiv = "BI", .leasePlaceCd = "", .leaseDueDt = "", .remark = "", .leaseEmployeeCd = "4563", .employeeCd = "4563", .processToolDiv = "H"},
            New EntryProcessDataDetail With {.processLogId = "14", .processDt = "20251030", .processTm = "000000005", .assetNo = "IP0010", .assetId = "", .processDiv = "BO", .leasePlaceCd = "", .leaseDueDt = "20251031", .remark = "TEST", .leaseEmployeeCd = "ADMIN", .employeeCd = "4563", .processToolDiv = "H"},
            New EntryProcessDataDetail With {.processLogId = "15", .processDt = "20251031", .processTm = "000000006", .assetNo = "IP0001", .assetId = "", .processDiv = "BI", .leasePlaceCd = "", .leaseDueDt = "", .remark = "", .leaseEmployeeCd = "4563", .employeeCd = "4563", .processToolDiv = "H"},
            New EntryProcessDataDetail With {.processLogId = "16", .processDt = "20251030", .processTm = "000000007", .assetNo = "IP0010", .assetId = "", .processDiv = "BI", .leasePlaceCd = "", .leaseDueDt = "", .remark = "", .leaseEmployeeCd = "4563", .employeeCd = "4563", .processToolDiv = "H"}
        }
        ''貸出・貸出・返却
        'test.processDataList = New EntryProcessDataDetail() {
        'New EntryProcessDataDetail With {.processLogId = "7", .processDt = "20190101", .processTm = "000000000", .assetNo = "H05", .assetId = "994153323032303030303031", .processDiv = "BO", .leasePlaceCd = "L01", .leaseDueDt = "20190131", .remark = "TEST", .leaseEmployeeCd = "1001", .employeeCd = "1001", .processToolDiv = "H"},
        'New EntryProcessDataDetail With {.processLogId = "8", .processDt = "20190102", .processTm = "000000000", .assetNo = "H05", .assetId = "994153323032303030303031", .processDiv = "BO", .leasePlaceCd = "L02", .leaseDueDt = "20190201", .remark = "TEST2", .leaseEmployeeCd = "1002", .employeeCd = "1001", .processToolDiv = "H"},
        'New EntryProcessDataDetail With {.processLogId = "10", .processDt = "20190203", .processTm = "000000000", .assetNo = "H05", .assetId = "1", .processDiv = "BI", .leasePlaceCd = "L01", .leaseDueDt = "20190725", .remark = "TEST", .leaseEmployeeCd = "1001", .employeeCd = "1001", .processToolDiv = "H"}
        '}
        ''返却のみ
        'test.processDataList = New EntryProcessDataDetail() {
        'New EntryProcessDataDetail With {.processLogId = "10", .processDt = "20230412", .processTm = "121500000", .assetNo = "A0003", .assetId = "413030303300000000000000", .processDiv = "BI", .leasePlaceCd = "L02", .leaseDueDt = "20230414", .remark = "TESTです", .leaseEmployeeCd = "KT001", .employeeCd = "KT001", .processToolDiv = "C", .locLCd = "L02", .dueQty = "80", .workTime = "60", .workOutline = "作業概要", .retRemark1 = "返却備考１", .retRemark2 = "返却備考２"}
        '}
        'New EntryProcessDataDetail With {.processLogId = "10", .processDt = "20230412", .processTm = "121000000", .assetNo = "A0003", .assetId = "413030303300000000000000", .processDiv = "BI", .leasePlaceCd = "L02", .leaseDueDt = "20230415", .remark = "TESTです", .leaseEmployeeCd = "KT001", .employeeCd = "KT001", .processToolDiv = "H", .locLCd = "L02"}
        'New EntryProcessDataDetail With {.processLogId = "10", .processDt = "20230412", .processTm = "121200000", .assetNo = "A0003", .assetId = "413030303300000000000000", .processDiv = "BO", .leasePlaceCd = "L02", .leaseDueDt = "20230414", .remark = "TESTです", .leaseEmployeeCd = "KT001", .employeeCd = "KT001", .processToolDiv = "C", .locLCd = "L02", .dueQty = "", .workTime = "", .workOutline = "", .retRemark1 = "", .retRemark2 = ""}
        'New EntryProcessDataDetail With {.processLogId = "7", .processDt = dt, .processTm = tm, .assetNo = "INO001", .assetId = "994153323032303030303031", .processDiv = "BO", .leasePlaceCd = "L01", .leaseDueDt = "20190725", .remark = "TEST", .leaseEmployeeCd = "1001", .employeeCd = "1001", .processToolDiv = "H"},
        'New EntryProcessDataDetail With {.processLogId = "8", .processDt = dt, .processTm = tm, .assetNo = "INO002", .assetId = "994153323032303030303031", .processDiv = "BO", .leasePlaceCd = "L01", .leaseDueDt = "20190725", .remark = "TEST", .leaseEmployeeCd = "1001", .employeeCd = "1001", .processToolDiv = "H"},
        '    New EntryProcessDataDetail With {.processLogId = "2", .processDt = "20200226", .processTm = "192500000", .assetNo = "BANGO096no1", .assetId = "42414E474F3039366E6F3100", .processDiv = "BI", .employeeCd = "1002", .processToolDiv = "H"},
        '    New EntryProcessDataDetail With {.processLogId = "3", .processDt = "20200226", .processTm = "192600000", .assetNo = "BANGO096no1", .assetId = "42414E474F3039366E6F3100", .processDiv = "BI", .employeeCd = "1002", .processToolDiv = "H"},
        'New EntryProcessDataDetail With {.processLogId = "4", .processDt = "20200226", .processTm = "201500000", .assetNo = "BANGO096no1", .assetId = "42414E474F3039366E6F3100", .processDiv = "BO", .leasePlaceCd = "L01", .leaseDueDt = "20200725", .locLCd = "L01", .locMCd = "M01", .locSCd = "S01", .employeeCd = "ADMIN", .leaseEmployeeCd = "1001", .processToolDiv = "H", .remark = "てすと"},
        '    New EntryProcessDataDetail With {.processLogId = "5", .processDt = "20200226", .processTm = "202000000", .assetNo = "BANGO096no1", .assetId = "42414E474F3039366E6F3100", .processDiv = "BI", .employeeCd = "1002", .processToolDiv = "H"},
        '    New EntryProcessDataDetail With {.processLogId = "6", .processDt = "20200226", .processTm = "202000000", .assetNo = "BANGO096no2", .assetId = "42414E474F3039366E6F3100", .processDiv = "BI", .employeeCd = "1002", .processToolDiv = "H"}}
        'New EntryProcessDataDetail With {.processLogId = "1", .processDt = "20200223", .processTm = "191500000", .assetNo = "BANGO096no1", .assetId = "42414E474F3039366E6F3100", .processDiv = "BO", .leasePlaceCd = "L01", .leaseDueDt = "20200725", .locLCd = "L01", .locMCd = "M01", .locSCd = "S01", .employeeCd = "ADMIN", .leaseEmployeeCd = "1001", .processToolDiv = "H", .remark = "てすと"},
        'New EntryProcessDataDetail With {.processLogId = "1", .processDt = "20190723", .processTm = "091500000", .assetNo = "00000003", .assetId = "TEST02", .processDiv = "BI", .leasePlaceCd = "1", .leaseDueDt = "20190725", .locLCd = "200", .locMCd = "110", .updEmployeeCd = "1234", .updDatetime = "20190722", .leaseEmployeeCd = "1234"},
        'New EntryProcessDataDetail With {.processLogId = "4", .processDt = "20190723", .processTm = "111500000", .assetNo = "00000004", .assetId = "TEST03", .processDiv = "BR", .leasePlaceCd = "1", .leaseDueDt = "20190725", .locLCd = "100", .locMCd = "110", .locSCd = "101", .updEmployeeCd = "1234", .updDatetime = "20190722", .leaseEmployeeCd = "1234"},
        'New EntryProcessDataDetail With {.processLogId = "2", .processDt = "20190723", .processTm = "093000000", .assetNo = "00000005", .assetId = "TEST04", .processDiv = "BS", .leasePlaceCd = "1", .leaseDueDt = "20190725", .locLCd = "200", .updEmployeeCd = "1234", .updDatetime = "20190722", .leaseEmployeeCd = "1234"},
        'New EntryProcessDataDetail With {.processLogId = "5", .processDt = "20190723", .processTm = "112000000", .assetNo = "00000006", .assetId = "TEST05", .processDiv = "T2", .leasePlaceCd = "1", .leaseDueDt = "20190725", .locLCd = "200", .updEmployeeCd = "1234", .updDatetime = "20190722", .leaseEmployeeCd = "1234"},
        'New EntryProcessDataDetail With {.processLogId = "6", .processDt = "20190723", .processTm = "112200000", .assetNo = "00000008", .assetId = "TEST07", .processDiv = "T2", .leasePlaceCd = "1", .leaseDueDt = "20190725", .locLCd = "200", .updEmployeeCd = "1234", .updDatetime = "20190722", .leaseEmployeeCd = "1234"}}

        Dim controlloer = New HtEntryProcessDataCmdController
        Dim ret = controlloer.Post(test)
        logger.Info($"戻り値：{Newtonsoft.Json.JsonConvert.SerializeObject(ret)}")


        Console.WriteLine($"RET:{ret.returnCode} wordCd:{ret.wordCd }")
        Console.WriteLine($"戻り値：{Newtonsoft.Json.JsonConvert.SerializeObject(ret)}")


    End Sub

End Class