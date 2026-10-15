Imports RFIDAM_WebService.Controllers

<TestClass()> Public Class HtGetMasterDataCmd

    <TestMethod()> Public Sub TestHtGetMasterDataCmd()
        'log4netの設定
        log4net.Config.XmlConfigurator.Configure()
        Dim logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)

        Dim masterData As HtGetMasterDataCmdReqParameter = New HtGetMasterDataCmdReqParameter
        masterData.LocCode = "200"
        masterData.masterDataList = New GetMasterDataDetail() {
            New GetMasterDataDetail With {.masterName = "M_ASSET"},
            New GetMasterDataDetail With {.masterName = "T_INVENTORY"}
        }
        'masterData.masterDataList = New GetMasterDataDetail() {
        '    New GetMasterDataDetail With {.masterName = "M_ASSET"},
        '    New GetMasterDataDetail With {.masterName = "M_LEASE_PLACE"},
        '    New GetMasterDataDetail With {.masterName = "M_LOCATION"},
        '    New GetMasterDataDetail With {.masterName = "M_WORD"},
        '    New GetMasterDataDetail With {.masterName = "M_EMPLOYEE"},
        '    New GetMasterDataDetail With {.masterName = "M_SETTING"}
        '}

        Dim controlloer = New HtGetMasterDataCmdController()
        Dim ret = controlloer.Post(masterData)
        logger.Info($"戻り値：{Newtonsoft.Json.JsonConvert.SerializeObject(ret)}")
    End Sub

End Class