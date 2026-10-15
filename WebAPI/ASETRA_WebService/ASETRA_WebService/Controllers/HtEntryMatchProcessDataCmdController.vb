Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' 処理データを登録するコントローラー
    ''' </summary>
    Public Class HtEntryMatchProcessDataCmdController
        Inherits ApiController

#Region "固定値定義"
        ''' <summary>
        ''' 処理名
        ''' </summary>
        Private TASK_NAME As String = Me.GetType.Name
#End Region

#Region "変数定義"

        ''' <summary>
        ''' ログ出力クラス
        ''' </summary>
        Private logger As log4net.ILog = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType)
#End Region

#Region "Public メソッド"
        ''' <summary>
        ''' 処理データ登録処理
        ''' </summary>
        ''' <param name="value">処理データ情報</param>
        ''' <returns>戻り値クラス</returns>
        ''' <remarks>api/HtEntryProcessDataCmd呼び出し</remarks>
        <HttpPost>
        <EnableCors("*", "*", "*")>
        Public Function Post(<FromBody> ByVal value As HtEntryMatchProcessDataCmdReqParameter) As HtEntryMatchProcessDataCmdResParameter
            Dim oldMst As Boolean = False
            Dim ret As HtEntryMatchProcessDataCmdResParameter = New HtEntryMatchProcessDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            Dim outErrorList = New List(Of outErrorList)
            Dim tempIdList = New List(Of tempIdList)
            ''設定マスタ取得(照合機能)
            'setting = db.M_SETTING.Find(CommonConst.ASSET_REGISTRATION_LIMIT, "**", "**", "**")

            ''設定マスタ取得(照合予約機能)
            'setting = db.M_SETTING.Find(CommonConst.ASSET_REGISTRATION_LIMIT, "**", "**", "**")

            ''設定マスタ取得(仮置き機能)
            'setting = db.M_SETTING.Find(CommonConst.ASSET_REGISTRATION_LIMIT, "**", "**", "**")
            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                Return New HtEntryMatchProcessDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_ERR_DB}
            End If

            logger.Info($"SEND COUNT {value.matchProcessDataList.Count}")

            If value.matchProcessDataList.Count <= 0 Then
                '送信情報が0件は正常終了
                Return ret
            End If

            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")

            If Not ModelState.IsValid Then
                '送信情報が不正
                Return New HtEntryMatchProcessDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_EXCEPTION}
            End If

            Try
                '設定マスタ取得(照合予約機能)
                Dim MatchingReserveFunction As M_SETTING = db.M_SETTING.Find(CommonConst.KEY_MATCHING_RESERVE_FUNCTION, "**", "**", "**")

                '設定マスタ取得(照合機能)
                Dim MatchingFunction As M_SETTING = db.M_SETTING.Find(CommonConst.KEY_MATCHING_FUNCTION, "**", "**", "**")

                '照合機能OFFの場合　文言をセットし返す
                If MatchingFunction IsNot Nothing AndAlso MatchingFunction.DATA1 = CommonConst.MATCHING_FUNCTION.NO Then
                    Return New HtEntryMatchProcessDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN), .wordCd = CommonWord.MSG_RETURN_NOTMATCHING_FUNCTION_ERR}
                End If

                Dim param As List(Of SqlParameter)
                Dim sql As String
                '最終データのログイン担当者の名称を入手する
                Dim emp As M_EMPLOYEE = db.M_EMPLOYEE.Find(value.matchProcessDataList.Last.employeeCd)

                Using tx = db.Database.BeginTransaction((System.Data.IsolationLevel.Serializable))
                    logger.Info("TRANSACTION START")
                    Dim processList = value.matchProcessDataList.OrderByDescending(Function(x) x.processLogId).ToList    '後ろから実績データを確認するためのリスト
                    Dim processDataList As List(Of EntryMatchProcessDataDetail) = New List(Of EntryMatchProcessDataDetail)           '実績登録するためのリスト

                    Dim assetMap As Hashtable = New Hashtable()
                    For i As Integer = 0 To processList.Count - 1
                        Dim p = processList(i)

                        processDataList.Add(p)
                    Next

                    value.matchProcessDataList = processDataList.ToArray                    '送信された実績データから重複した実績を削除したリストで上書き

                    '照合リスト一時表の作成
                    db.Database.ExecuteSqlCommand(CreateTempTable())
                    '貸出リスト一時表の作成
                    db.Database.ExecuteSqlCommand(CreateLentTempTable())
                    '返却リスト一時表の作成
                    db.Database.ExecuteSqlCommand(CreateRetTempTable())
                    '照合リスト一時表の作成
                    db.Database.ExecuteSqlCommand(CreateMatchTempTable())
                    '仮置きリスト一時表の作成
                    db.Database.ExecuteSqlCommand(CreateTempTempTable())
                    '仮置き資産リスト一時表の作成
                    db.Database.ExecuteSqlCommand(CreateTempAssetTempTable())

                    '一時表へ処理データ情報を登録する
                    For Each item As EntryMatchProcessDataDetail In value.matchProcessDataList
                        '照合リスト.一時表データ登録
                        param = New List(Of SqlParameter)
                        sql = LentInfoInsertTempTable(item, param)
                        db.Database.ExecuteSqlCommand(sql, param.ToArray)

                        'BOの場合　貸出リストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_BO Then
                            '貸出リストがあるかチェック
                            If item.lentAssetList.Length <> 0 Then
                                '貸出リストの情報で登録
                                For Each subItem As EntryLentAssetListDetail In item.lentAssetList
                                    param = New List(Of SqlParameter)
                                    sql = LentInsertTempTable(item, subItem, param)
                                    db.Database.ExecuteSqlCommand(sql, param.ToArray)
                                Next

                            End If
                        End If

                        'BIの場合　返却リストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_BI Then
                            '返却リストがあるかチェック
                            If item.retAssetList.Length <> 0 Then
                                '返却リストの情報で登録
                                For Each subItem As EntryRetAssetListDetail In item.retAssetList
                                    param = New List(Of SqlParameter)
                                    sql = RetInfoInsertTempTable(item, subItem, param)
                                    db.Database.ExecuteSqlCommand(sql, param.ToArray)
                                Next

                            End If
                        End If

                        'BMの場合　返却リストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_BM Then
                            '返却リストがあるかチェック
                            If item.retAssetList.Length <> 0 Then
                                '返却リストの情報で登録
                                For Each subItem As EntryRetAssetListDetail In item.retAssetList
                                    param = New List(Of SqlParameter)
                                    sql = RetInfoInsertTempTable(item, subItem, param)
                                    db.Database.ExecuteSqlCommand(sql, param.ToArray)
                                Next

                            End If
                        End If

                        'MI/MOの場合　照合資産リストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_MI OrElse item.processDiv = CommonConst.PROCESS_DIV_MO Then
                            '照合リストがあるかチェック
                            If item.matchAssetList.Length <> 0 Then
                                '照合リストの情報で登録
                                For Each subItem As EntryMatchAssetListDetail In item.matchAssetList
                                    param = New List(Of SqlParameter)
                                    sql = MatchAssetInfoInsertTempTable(item, subItem, param)
                                    db.Database.ExecuteSqlCommand(sql, param.ToArray)
                                Next

                            End If
                        End If

                        'TMの場合　仮置きリストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_TM Then
                            '仮置きリストがあるかチェック
                            If item.tempList.Length <> 0 Then
                                '仮置きリストの情報で登録
                                For Each subItem As EntryTempListDetail In item.tempList
                                    param = New List(Of SqlParameter)
                                    sql = TempInfoInsertTempTable(item, subItem, param)
                                    db.Database.ExecuteSqlCommand(sql, param.ToArray)

                                    '仮置き資産リストがあるかチェック
                                    If subItem.tempAssetList.Length <> 0 Then
                                        '仮置き資産リストの情報で登録
                                        For Each subAssetItem As EntryTempAssetListDetail In subItem.tempAssetList
                                            param = New List(Of SqlParameter)
                                            sql = TempAssetInfoInsertTempTable(item, subItem, subAssetItem, param)
                                            db.Database.ExecuteSqlCommand(sql, param.ToArray)
                                        Next

                                    End If

                                Next

                            End If
                        End If

                    Next


                    'ログイン担当者名をセット(照合処理リストT)
                    db.Database.ExecuteSqlCommand(UpdateEmployeeName())

                    'ロケ名をセット(照合処理リストT)
                    db.Database.ExecuteSqlCommand(UpdateLargeLocName())
                    db.Database.ExecuteSqlCommand(UpdateMiddleLocName())
                    db.Database.ExecuteSqlCommand(UpdateSmallLocName())

                    '工事情報をセット(照合処理リストT)
                    db.Database.ExecuteSqlCommand(UpdateConstRuctionInfomation())

                    '予約情報をセット(照合処理リストT)
                    db.Database.ExecuteSqlCommand(UpdateReserveInfomation())

                    ''貸出先場所名をセット(貸出リストT)
                    'db.Database.ExecuteSqlCommand(UpdateLeasePlaceName())

                    ''貸出先担当者をセット(貸出リストT)
                    'db.Database.ExecuteSqlCommand(UpdateLeaseEmployeeName())


                    '資産情報をセット(貸出リストT)
                    db.Database.ExecuteSqlCommand(UpdateAssetInfomation("LENT_ASSET_LIST_TEMP"))
                    '資産情報をセット(返却リストT)
                    db.Database.ExecuteSqlCommand(UpdateAssetInfomation("RET_ASSET_LIST_TEMP"))
                    '資産情報をセット(照合資産リストT)
                    db.Database.ExecuteSqlCommand(UpdateAssetInfomation("MATCH_ASSET_LIST_TEMP"))
                    '資産情報をセット(仮置き資産リストT)
                    db.Database.ExecuteSqlCommand(UpdateAssetInfomation("TEMP_ASSET_LIST_TEMP"))


                    '一時表のデータを全て取得する
                    Dim matchProcessTmpList As EntryMatchProcessTmpDataDetail()
                    '貸出リスト.一時表のデータを全て取得する
                    Dim matchLentTmpList As EntryMatchLentTmpDataDetail()
                    '返却リスト.一時表のデータを全て取得する
                    Dim matchRetTmpList As EntryMatchReturnTmpDataDetail()
                    '照合資産リスト.一時表のデータを全て取得する
                    Dim matchAssetTmpList As EntryMatchAssetTmpDataDetail()
                    '仮置きリスト.一時表のデータを全て取得する
                    Dim matchTempTmpList As EntryTempTmpDataDetail()
                    '仮置き資産リスト.一時表のデータを全て取得する
                    Dim matchTempAssetTmpList As EntryTempAssetTmpDataDetail()

                    matchLentTmpList = CType(db.Database.SqlQuery(Of EntryMatchLentTmpDataDetail)(GetAllLengData()).ToArray(), EntryMatchLentTmpDataDetail())
                    matchRetTmpList = CType(db.Database.SqlQuery(Of EntryMatchReturnTmpDataDetail)(GetAllRetData()).ToArray(), EntryMatchReturnTmpDataDetail())
                    matchAssetTmpList = CType(db.Database.SqlQuery(Of EntryMatchAssetTmpDataDetail)(GetAllMatchAssetData()).ToArray(), EntryMatchAssetTmpDataDetail())
                    matchTempTmpList = CType(db.Database.SqlQuery(Of EntryTempTmpDataDetail)(GetAllTempData()).ToArray(), EntryTempTmpDataDetail())
                    matchTempAssetTmpList = CType(db.Database.SqlQuery(Of EntryTempAssetTmpDataDetail)(GetAllTempAssetData()).ToArray(), EntryTempAssetTmpDataDetail())

                    matchProcessTmpList = CType(db.Database.SqlQuery(Of EntryMatchProcessTmpDataDetail)(GetAllProcessData()).ToArray(), EntryMatchProcessTmpDataDetail())

                    '資産状態が0,9のものはデータから削除
                    matchLentTmpList = matchLentTmpList.Where(Function(item) item.assetStatus.ToString <> CommonConst.ASSET_STATUS_NOT_LINK AndAlso item.assetStatus.ToString <> CommonConst.ASSET_STATUS_RETIRE).ToArray()
                    matchRetTmpList = matchRetTmpList.Where(Function(item) item.assetStatus.ToString <> CommonConst.ASSET_STATUS_NOT_LINK AndAlso item.assetStatus.ToString <> CommonConst.ASSET_STATUS_RETIRE).ToArray()
                    matchAssetTmpList = matchAssetTmpList.Where(Function(item) item.assetStatus.ToString <> CommonConst.ASSET_STATUS_NOT_LINK AndAlso item.assetStatus.ToString <> CommonConst.ASSET_STATUS_RETIRE).ToArray()
                    matchTempAssetTmpList = matchTempAssetTmpList.Where(Function(item) item.assetStatus.ToString <> CommonConst.ASSET_STATUS_NOT_LINK AndAlso item.assetStatus.ToString <> CommonConst.ASSET_STATUS_RETIRE).ToArray()

                    '貸出状態が0のデータ削除
                    matchLentTmpList = matchLentTmpList.Where(Function(item) item.leaseStatus.ToString <> CommonConst.LEASE_STATUS_NOT).ToArray()
                    matchRetTmpList = matchRetTmpList.Where(Function(item) item.leaseStatus.ToString <> CommonConst.LEASE_STATUS_NOT).ToArray()
                    matchAssetTmpList = matchAssetTmpList.Where(Function(item) item.leaseStatus.ToString <> CommonConst.LEASE_STATUS_NOT).ToArray()
                    matchTempAssetTmpList = matchTempAssetTmpList.Where(Function(item) item.leaseStatus.ToString <> CommonConst.LEASE_STATUS_NOT).ToArray()

                    ''処理日時 > HT.処理日時+処理時刻のデータ削除
                    'matchProcessTmpList = matchProcessTmpList.Where(Function(item) item.taskTs.ToString <= ).ToArray()
                    For Each item As EntryMatchProcessTmpDataDetail In matchProcessTmpList
                        Dim taskIs As String = item.processDt + item.processTm

                        Select Case item.processDiv
                            Case CommonConst.PROCESS_DIV_BO
                                matchLentTmpList = matchLentTmpList.Where(Function(subItem) subItem.taskTs.ToString <= taskIs.Substring(0, 14).ToString OrElse subItem.processLogId <> item.processLogId).ToArray()
                            Case CommonConst.PROCESS_DIV_BI
                                matchRetTmpList = matchRetTmpList.Where(Function(subItem) subItem.taskTs.ToString <= taskIs.Substring(0, 14).ToString OrElse subItem.processLogId <> item.processLogId).ToArray()
                            Case CommonConst.PROCESS_DIV_BM
                                matchRetTmpList = matchRetTmpList.Where(Function(subItem) subItem.taskTs.ToString <= taskIs.Substring(0, 14).ToString OrElse subItem.processLogId <> item.processLogId).ToArray()
                            Case CommonConst.PROCESS_DIV_MO, CommonConst.PROCESS_DIV_MI
                                matchAssetTmpList = matchAssetTmpList.Where(Function(subItem) subItem.taskTs.ToString <= taskIs.Substring(0, 14).ToString OrElse subItem.processLogId <> item.processLogId).ToArray()
                            Case CommonConst.PROCESS_DIV_TM
                                matchTempAssetTmpList = matchTempAssetTmpList.Where(Function(subItem) subItem.taskTs.ToString <= taskIs.Substring(0, 14).ToString OrElse subItem.processLogId <> item.processLogId).ToArray()

                        End Select


                    Next

                    '仮置きリストと仮置き資産リスト結合
                    For Each item In matchTempTmpList
                        item.matchTempAssetList = matchTempAssetTmpList.Where(Function(d) d.processLogId = item.processLogId AndAlso d.constNo = item.constNo AndAlso d.tempId = item.tempId).ToArray()
                    Next

                    '紐づいてるデータを結合
                    For Each item In matchProcessTmpList
                        'BOの場合　貸出リストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_BO Then
                            item.matchLentList = matchLentTmpList.Where(Function(d) d.processLogId = item.processLogId AndAlso d.constNo = item.constNo).ToArray()
                        End If

                        'BIの場合　返却リストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_BI Then
                            item.matchRetList = matchRetTmpList.Where(Function(d) d.processLogId = item.processLogId AndAlso d.constNo = item.constNo).ToArray()
                        End If

                        'BMの場合　照合返却の数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_BM Then
                            item.matchRetList = matchRetTmpList.Where(Function(d) d.processLogId = item.processLogId AndAlso d.constNo = item.constNo).ToArray()
                        End If

                        'MI/MOの場合　照合資産リストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_MI OrElse item.processDiv = CommonConst.PROCESS_DIV_MO Then
                            item.matchAssetList = matchAssetTmpList.Where(Function(d) d.processLogId = item.processLogId AndAlso d.constNo = item.constNo).ToArray()
                        End If

                        'TMの場合　仮置きリストの数でループ
                        If item.processDiv = CommonConst.PROCESS_DIV_TM Then
                            item.matchTempList = matchTempTmpList.Where(Function(d) d.processLogId = item.processLogId AndAlso d.constNo = item.constNo).ToArray()
                        End If
                    Next

                    'データ処理ループ
                    For Each item As EntryMatchProcessTmpDataDetail In matchProcessTmpList
                        ''照合予約機能ONの場合
                        'If MatchingReserveFunction IsNot Nothing AndAlso MatchingFunction.DATA1 = CommonConst.RESERVATION_FUNCTION_YES Then
                        '    '処理区分がBOの場合
                        '    If item.processDiv = CommonConst.PROCESS_DIV_BO Then
                        '        '一時テーブルから品番ごとの件数を取得
                        '        Dim reservTmpList As EntryReservDetail()
                        '        reservTmpList = CType(db.Database.SqlQuery(Of EntryReservDetail)(GetAllProductNoData()).ToArray(), EntryReservDetail())
                        '        '貸出リストでループ
                        '        For Each reservItem As EntryReservDetail In reservTmpList
                        '            '指定品番予約可能数ストアドを呼び出す
                        '            Dim result As Integer = GetModelReservePossibleSf(db, reservItem.productNo, reservItem.processDt, reservItem.assetLeaseDueDt, reservItem.mainteDayWorkFlag, reservItem.constNo)

                        '            '抽出した同一品番件数 > 戻り値.予約可能数の場合
                        '            If reservItem.recordCount > result Then
                        '                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                        '                ret.wordCd = CommonWord.MSG_RETURN_TAKEOUT_OVER_ERR
                        '                outErrorList.Add(New outErrorList With {.rep1 = reservItem.productNo, .rep2 = result})
                        '                ret.outErrorList = outErrorList.ToArray
                        '                Return ret
                        '            End If
                        '        Next

                        '    End If
                        'End If

                        '処理区分(BO)貸出
                        If item.processDiv = CommonConst.PROCESS_DIV_BO Then
                            '予約関連の登録
                            '予約IDがNULLの場合登録
                            If item.reserveId Is Nothing Then
                                '予約IDシーケンスの値取得
                                Dim reserveId As Long = db.Database.SqlQuery(Of Long)(GetReserveIdSequence()).FirstOrDefault()

                                '予約情報登録
                                db.Database.ExecuteSqlCommand(InsertReserve(item, reserveId))

                                '一時処理リストの予約IDを同じ工事情報のものをシーケンスの値で更新
                                For Each updateItem As EntryMatchProcessTmpDataDetail In matchProcessTmpList
                                    '今行ってる工事番号と同じデータの場合
                                    If updateItem.constNo = item.constNo Then
                                        updateItem.reserveId = reserveId
                                    End If
                                Next

                            End If

                            '照合機能がONの場合
                            If MatchingFunction IsNot Nothing AndAlso MatchingFunction.DATA1 = CommonConst.MATCHING_FUNCTION.YES Then
                                '貸出リストでループ
                                For Each lestItem As EntryMatchLentTmpDataDetail In item.matchLentList
                                    '代替品番予約更新ストアドプロシージャを呼び出す
                                    Dim result As String = GetReplaceReservePossibleSf(db, item, lestItem.productNo, lestItem.assetNo, item.processDt, item.leaseDueDt, lestItem.mainteDayWorkFlag, item.constNo, item.reserveId).ToString

                                    '8予約エラーの場合
                                    If result = CommonConst.RESERV_UPDATE_ERR_DIV.RESERV_ERR Then
                                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                                        ret.wordCd = CommonWord.MSG_RETURN_RESERV_ERR
                                        outErrorList.Add(New outErrorList With {.rep1 = lestItem.assetNo, .rep2 = ""})
                                        ret.outErrorList = outErrorList.ToArray
                                        Return ret
                                    End If

                                    '9期限エラーの場合
                                    If result = CommonConst.RESERV_UPDATE_ERR_DIV.LIMIT_ERRT Then
                                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                                        ret.wordCd = CommonWord.MSG_RETURN_LIMIT_OVER_ERR
                                        outErrorList.Add(New outErrorList With {.rep1 = lestItem.assetNo, .rep2 = ""})
                                        ret.outErrorList = outErrorList.ToArray
                                        Return ret
                                    End If

                                    '予約機能利用の場合
                                    If MatchingFunction.DATA1 = CommonConst.RESERVATION_FUNCTION_YES Then
                                        '予約明細テーブルを更新する
                                        db.Database.ExecuteSqlCommand(UpdateReservationDetailLease(item, lestItem, item.reserveId))

                                    End If
                                Next

                                '貸出リストでループ
                                For Each lestItem As EntryMatchLentTmpDataDetail In item.matchLentList
                                    '工事資産履歴を登録する
                                    db.Database.ExecuteSqlCommand(InsertConstAssetLogLease(item, lestItem,
                                                 CommonConst.CONST_MATCH_DIV.NO,
                                                 CommonConst.ASSET_STATUS_LEASE,
                                                 CommonConst.TASK_LENT_REGIST))

                                    '工事資産を取得する  存在してるかチェックするために取得
                                    Dim constAssetCnt As Integer = db.Database.SqlQuery(Of Integer)(GetConstAsset(lestItem.constNo, lestItem.assetNo)).FirstOrDefault()

                                    'constAssetCntが0件より大きい場合更新
                                    If constAssetCnt > 0 Then
                                        'データ存在してる　更新
                                        '工事資産を更新する
                                        db.Database.ExecuteSqlCommand(UpdateConstAssetLease(item, lestItem,
                                                     CommonConst.CONST_ASSET_STATUS.CONST_LENT,
                                                     CommonConst.TASK_LENT_REGIST))
                                    Else
                                        'データ存在していない　登録
                                        '工事資産を登録する
                                        db.Database.ExecuteSqlCommand(InsertConstAssetLease(item, lestItem,
                                                     CommonConst.CONST_ASSET_STATUS.CONST_LENT,
                                                     CommonConst.PREV_TEMP_FLAG.NO,
                                                     CommonConst.TASK_LENT_REGIST))
                                    End If
                                Next


                                '貸出リストでループ
                                For Each lestItem As EntryMatchLentTmpDataDetail In item.matchLentList
                                    '処理履歴登録
                                    ''貸出先担当者コードが入っていて、貸出先担当者名が入っていない場合は、マスタ不整合扱い
                                    'If String.IsNullOrEmpty(lestItem.leaseEmployeeCd) = False AndAlso
                                    '   String.IsNullOrEmpty(lestItem.leaseEmployeeName) = True Then
                                    '    oldMst = True
                                    '    logger.Info($"NOT FOUND LEASE_EMPLOYEE_CD={lestItem.leaseEmployeeCd}")

                                    'End If

                                    ''貸出先場所コードが入っていて、貸出先場所名が入っていない場合は、マスタ不整合扱い
                                    'If String.IsNullOrEmpty(lestItem.leasePlaceCd) = False AndAlso
                                    '   String.IsNullOrEmpty(lestItem.leasePlaceName) = True Then
                                    '    oldMst = True
                                    '    logger.Info($"NOT FOUND LEASE_PLACE_CD={lestItem.leasePlaceCd}")
                                    'End If

                                    '資産マスタを更新する
                                    db.Database.ExecuteSqlCommand(UpdateProcessLease(item, lestItem, (MatchingFunction.DATA1 = CommonConst.RESERVATION_FUNCTION_YES)))

                                    '資産履歴を登録する。
                                    db.Database.ExecuteSqlCommand(InsertProcessLog(item, lestItem, CommonConst.TASK_RESULT_N))
                                Next

                                '工事関連を登録する
                                '前回の工事履歴を取得する
                                Dim constLog As EntryConstLogDetail = db.Database.SqlQuery(Of EntryConstLogDetail)(GetConstLog(item.constNo)).FirstOrDefault()

                                '工事履歴を登録する
                                db.Database.ExecuteSqlCommand(InsertConstLogLease(item, constLog, item.matchLentList.Count,
                                                                             CommonConst.CONST_WORK_DIV.LENT,
                                                                             CommonConst.CONST_MATCH_DIV.NO,
                                                                             CommonConst.TASK_LENT_REGIST))

                                '工事マスタを更新する
                                db.Database.ExecuteSqlCommand(UpdateConstRuctionRet(item,
                                                     CommonConst.PREV_TEMP_FLAG.NO,
                                                     CommonConst.TASK_LENT_REGIST))

                                '予約機能利用の場合
                                If MatchingFunction.DATA1 = CommonConst.RESERVATION_FUNCTION_YES Then
                                    '予約テーブルを更新する
                                    db.Database.ExecuteSqlCommand(UpdateReservationLease(item, item.reserveId))

                                End If

                            End If
                        End If

                        '処理区分(BI)返却
                        If item.processDiv = CommonConst.PROCESS_DIV_BI Then

                            '返却リストでループ
                            For Each retItem As EntryMatchReturnTmpDataDetail In item.matchRetList
                                '予約明細から工事番号を抽出する
                                Dim constRes As String = db.Database.SqlQuery(Of String)(GetReserveDetail(item, retItem)).FirstOrDefault()

                                If Not String.IsNullOrEmpty(constRes) Then
                                    '取得できた場合
                                    '取得できた工事番号を入れ込む
                                    item.constNo = constRes
                                    '工事関連を登録する
                                    '前回の工事履歴を取得する
                                    Dim constLog As EntryConstLogDetail = db.Database.SqlQuery(Of EntryConstLogDetail)(GetConstLog(item.constNo)).FirstOrDefault()


                                    '工事資産を取得する  存在してるかチェックするために取得
                                    Dim constAssetCnt As Integer = db.Database.SqlQuery(Of Integer)(GetConstAsset(item.constNo, retItem.assetNo)).FirstOrDefault()


                                    'constAssetCntが0件より大きい場合更新
                                    If constAssetCnt > 0 Then
                                        'データ存在してる　更新
                                        '工事資産を更新する
                                        db.Database.ExecuteSqlCommand(UpdateConstAssetRet(item, retItem,
                                                     CommonConst.CONST_ASSET_STATUS.CONST_RETURN,
                                                     CommonConst.TASK_RETURN_REGIST))
                                    Else
                                        'データ存在していない　登録
                                        '工事資産を登録する
                                        db.Database.ExecuteSqlCommand(InsertConstAssetRet(item, retItem,
                                                     CommonConst.CONST_ASSET_STATUS.CONST_RETURN,
                                                     CommonConst.PREV_TEMP_FLAG.NO,
                                                     CommonConst.TASK_RETURN_REGIST))
                                    End If


                                    '工事履歴を登録する
                                    Dim taskTs As String = item.processDt + item.processTm
                                    Dim constLogCnt As Integer = db.Database.SqlQuery(Of Integer)(GetConstLog(item.constNo, taskTs, CommonConst.CONST_WORK_DIV.RETURN)).FirstOrDefault()
                                    If constLogCnt = 0 Then
                                        '工事履歴が存在しなければ登録する
                                        db.Database.ExecuteSqlCommand(InsertConstLogRet(item,
                                                                                 CommonConst.CONST_WORK_DIV.RETURN,
                                                                                 CommonConst.CONST_MATCH_DIV.NO,
                                                                                 CommonConst.TASK_RETURN_REGIST))
                                    Else
                                        '工事履歴が存在する場合は更新(返却数)する
                                        db.Database.ExecuteSqlCommand(UpdateConstLogRet(item, constLog,
                                                                                 CommonConst.CONST_WORK_DIV.RETURN,
                                                                                 CommonConst.TASK_RETURN_REGIST))
                                    End If

                                    '工事資産履歴を登録する
                                    db.Database.ExecuteSqlCommand(InsertConstAssetLogRet(item, retItem,
                                                 CommonConst.CONST_MATCH_DIV.NO,
                                                 CommonConst.ASSET_STATUS_INV,
                                                 CommonConst.TASK_RETURN_REGIST))



                                    '工事マスタを更新する
                                    db.Database.ExecuteSqlCommand(UpdateConstRuctionRet(item,
                                                        CommonConst.PREV_TEMP_FLAG.NO,
                                                        CommonConst.TASK_RETURN_REGIST))

                                    '工事マスタの完了処理を行う
                                    db.Database.ExecuteSqlCommand(UpdateConstFinish(item,
                                                        CommonConst.TASK_RETURN_REGIST))
                                End If

                                '資産マスタを更新する
                                db.Database.ExecuteSqlCommand(UpdateProcessRet(item, retItem, (GetLocationUpdInfo(db) = CommonConst.RESERVATION_FUNCTION_YES)))

                                '資産履歴を登録する。
                                db.Database.ExecuteSqlCommand(InsertProcessLog(item, retItem, CommonConst.TASK_RESULT_N))

                                '資産履歴(異常更新)を登録する。
                                If retItem.inspeFlg = "9" Then
                                    '異常時
                                    db.Database.ExecuteSqlCommand(InsertInspectionProcessLog(item, retItem, CommonConst.TASK_RESULT_N))
                                End If

                                ''全て返却済みになった仮置き情報があった場合は完了(他工事に紐付済へ更新)にする。
                                'db.Database.ExecuteSqlCommand(UpdateTemporaryFinish(item, CommonConst.TASK_RETURN_REGIST))

                                '予約機能利用の場合
                                If MatchingFunction.DATA1 = CommonConst.RESERVATION_FUNCTION_YES Then

                                    'メンテナンスIDを取得する　1件のみ取得できる
                                    Dim dataList As MatReservationData() = CType(db.Database.SqlQuery(Of MatReservationData)(GetReserveIdReturn(retItem.assetNo)).ToArray(), MatReservationData())

                                    Dim rsvFuncInfoList As ReservationFunctionInfo() = CType(db.Database.SqlQuery(Of ReservationFunctionInfo)(GetReservationFunction()).ToArray(), ReservationFunctionInfo())
                                    Dim rsvFuncInfo As ReservationFunctionInfo = rsvFuncInfoList(0)

                                    '予約明細テーブルを更新する
                                    'db.Database.ExecuteSqlCommand(UpdateReservationDetailRet(item, retItem))

                                    Dim reserveId As Long = db.Database.SqlQuery(Of Long)(UpdateReservationDetailRet(item, retItem)).FirstOrDefault()

                                    If Not String.IsNullOrEmpty(reserveId) Then

                                        '予約テーブルを更新する
                                        db.Database.ExecuteSqlCommand(UpdateReservationMatRet(item.employeeCd, item.employeeName, reserveId))

                                        ' メンテ予約があり、メンテ内の最新行の場合、更新
                                        If dataList.Count > 0 AndAlso Not String.IsNullOrEmpty(dataList(0).mainteId) _
                                            AndAlso dataList(0).mainteOrder = "1" Then

                                            Dim nextDt As NextMenteUpdData() = CType(db.Database.SqlQuery(Of NextMenteUpdData)(GetNextReserveInfoReturn(item.processLogId, dataList(0).mainteId, rsvFuncInfo.reservationDataDiv)).ToArray(), NextMenteUpdData())
                                            If nextDt.Length > 0 Then
                                                If nextDt(0).mainteCnt > 0 Then
                                                    '予約テーブルを更新する
                                                    db.Database.ExecuteSqlCommand(UpdateReturnMeinte(item.processLogId, nextDt(0)))
                                                Else
                                                    '予約テーブルを削除する
                                                    db.Database.ExecuteSqlCommand(DeleteReturnMeinte(nextDt(0)))
                                                End If
                                            End If
                                        End If
                                    End If
                                End If
                            Next

                        End If

                        '処理区分(BM)照合返却
                        If item.processDiv = CommonConst.PROCESS_DIV_BM Then
                            '照合返却リストでループ
                            For Each retItem As EntryMatchReturnTmpDataDetail In item.matchRetList
                                '工事資産履歴を登録する
                                db.Database.ExecuteSqlCommand(InsertConstAssetLogMatRet(item, retItem,
                                             retItem.contMatchDiv,
                                             CommonConst.ASSET_STATUS_INV,
                                             CommonConst.TASK_MATCH_RETURN_REGIST))
                            Next


                            '照合返却リストでループ
                            For Each retItem As EntryMatchReturnTmpDataDetail In item.matchRetList

                                If Not (retItem.contMatchDiv = "1" OrElse retItem.contMatchDiv = "2") Then
                                    '照合OKまたは照合リスト外の返却意外は何もしない
                                    Continue For
                                End If

                                '工事資産を取得する  存在してるかチェックするために取得
                                Dim constAssetCnt As Integer = db.Database.SqlQuery(Of Integer)(GetConstAsset(retItem.constNo, retItem.assetNo)).FirstOrDefault()

                                'constAssetCntが0件より大きい場合更新
                                If constAssetCnt > 0 Then
                                    'データ存在してる　更新
                                    '工事資産を更新する
                                    db.Database.ExecuteSqlCommand(UpdateConstAssetMatRet(item, retItem,
                                                 CommonConst.CONST_ASSET_STATUS.CONST_RETURN,
                                                 CommonConst.TASK_MATCH_RETURN_REGIST))
                                Else
                                    'データ存在していない　登録
                                    '工事資産を登録する
                                    db.Database.ExecuteSqlCommand(InsertConstAssetMatRet(item, retItem,
                                                 CommonConst.CONST_ASSET_STATUS.CONST_RETURN,
                                                 CommonConst.PREV_TEMP_FLAG.NO,
                                                 CommonConst.TASK_MATCH_RETURN_REGIST))
                                End If

                                '資産マスタを更新する
                                db.Database.ExecuteSqlCommand(UpdateProcessMatRet(item, retItem, (GetLocationUpdInfo(db) = CommonConst.RESERVATION_FUNCTION_YES)))

                                '資産履歴を登録する。
                                db.Database.ExecuteSqlCommand(InsertProcessLog(item, retItem, CommonConst.TASK_RESULT_N))

                                '予約機能利用の場合
                                If MatchingFunction.DATA1 = CommonConst.RESERVATION_FUNCTION_YES Then

                                    'メンテナンスIDを取得する 1件のみ取得できる
                                    Dim dataList As MatReservationData() = CType(db.Database.SqlQuery(Of MatReservationData)(GetReserveIdReturn(retItem.assetNo)).ToArray(), MatReservationData())

                                    Dim rsvFuncInfoList As ReservationFunctionInfo() = CType(db.Database.SqlQuery(Of ReservationFunctionInfo)(GetReservationFunction()).ToArray(), ReservationFunctionInfo())
                                    Dim rsvFuncInfo As ReservationFunctionInfo = rsvFuncInfoList(0)

                                    '予約明細テーブルを更新する
                                    Dim reserveId As Long = db.Database.SqlQuery(Of Long)(UpdateReservationDetailMatRet(item, retItem)).FirstOrDefault()

                                    If Not String.IsNullOrEmpty(reserveId) Then

                                        '予約テーブルを更新する
                                        db.Database.ExecuteSqlCommand(UpdateReservationMatRet(item.employeeCd, item.employeeName, reserveId))


                                        ' メンテ予約があり、メンテ内の最新行の場合、更新
                                        If dataList.Count > 0 AndAlso Not String.IsNullOrEmpty(dataList(0).mainteId) _
                                            AndAlso dataList(0).mainteOrder = "1" Then

                                            Dim nextDt As NextMenteUpdData() = CType(db.Database.SqlQuery(Of NextMenteUpdData)(GetNextReserveInfoReturn(item.processLogId, dataList(0).mainteId, rsvFuncInfo.reservationDataDiv)).ToArray(), NextMenteUpdData())
                                            If nextDt.Length > 0 Then
                                                If nextDt(0).mainteCnt > 0 Then
                                                    '予約テーブルを更新する
                                                    db.Database.ExecuteSqlCommand(UpdateReturnMeinte(item.processLogId, nextDt(0)))
                                                Else
                                                    '予約テーブルを削除する
                                                    db.Database.ExecuteSqlCommand(DeleteReturnMeinte(nextDt(0)))
                                                End If
                                            End If
                                        End If
                                    End If

                                End If

                            Next

                            '工事関連を登録する
                            '前回の工事履歴を取得する
                            Dim constLog As EntryConstLogDetail = db.Database.SqlQuery(Of EntryConstLogDetail)(GetConstLog(item.constNo)).FirstOrDefault()

                            '工事履歴を登録する
                            db.Database.ExecuteSqlCommand(InsertConstLogMatRet(item, constLog, item.matchRetList.Count(Function(v) v.contMatchDiv = "1"), item.matchRetList.Count(Function(v) v.contMatchDiv = "1" Or v.contMatchDiv = "9"),
                                                                         CommonConst.CONST_WORK_DIV.MATCH_RETURN,
                                                                         item.contMatchDiv,
                                                                         CommonConst.TASK_MATCH_RETURN_REGIST))

                            '工事マスタを更新する
                            db.Database.ExecuteSqlCommand(UpdateConstRuctionRet(item,
                                                 CommonConst.PREV_TEMP_FLAG.NO,
                                                 CommonConst.TASK_MATCH_RETURN_REGIST))

                            '照合OKの場合のみ工事完了を行う
                            If (item.contMatchDiv = "1") Then
                                '工事マスタの完了処理を行う
                                db.Database.ExecuteSqlCommand(UpdateConstFinish(item,
                                                     CommonConst.TASK_MATCH_RETURN_REGIST))
                            End If

                        End If

                        '処理区分(MO/MI)持出/持帰照合
                        If item.processDiv = CommonConst.PROCESS_DIV_MO OrElse item.processDiv = CommonConst.PROCESS_DIV_MI Then

                            '工事関連を登録する
                            '前回の工事履歴を取得する
                            Dim constLog As EntryConstLogDetail = db.Database.SqlQuery(Of EntryConstLogDetail)(GetConstLog(item.constNo)).FirstOrDefault()

                            If item.processDiv = CommonConst.PROCESS_DIV_MO Then
                                '持出照合
                                '工事履歴を登録する
                                db.Database.ExecuteSqlCommand(InsertConstLogMatIn(item, constLog, item.matchAssetList.Count(Function(v) v.contMatchDiv = "1"),
                                                                             item.matchAssetList.Count(Function(v) v.contMatchDiv = "1" Or v.contMatchDiv = "9"),
                                                                             CommonConst.CONST_WORK_DIV.MATCH_IN,
                                                                             item.contMatchDiv,
                                                                             CommonConst.TASK_MATCH_REGIST))
                            ElseIf (item.processDiv = CommonConst.PROCESS_DIV_MI) Then
                                '持帰照合
                                '工事履歴を登録する
                                db.Database.ExecuteSqlCommand(InsertConstLogMatOut(item, constLog, item.matchAssetList.Count(Function(v) v.contMatchDiv = "1"),
                                                                             item.matchAssetList.Count(Function(v) v.contMatchDiv = "1" Or v.contMatchDiv = "9"),
                                                                             CommonConst.CONST_WORK_DIV.MATCH_OUT,
                                                                             item.contMatchDiv,
                                                                             CommonConst.TASK_MATCH_REGIST))
                            End If

                            '照合リストでループ
                            For Each matItem As EntryMatchAssetTmpDataDetail In item.matchAssetList
                                Dim constAssetLog As EntryConstAssetLogDetail = db.Database.SqlQuery(Of EntryConstAssetLogDetail)(GetConstAssetLog(item.constNo, matItem.assetNo)).FirstOrDefault()

                                '工事資産履歴を登録する
                                db.Database.ExecuteSqlCommand(InsertConstAssetLogMat(item, matItem, constAssetLog,
                                                                                    CommonConst.TASK_MATCH_REGIST))
                            Next


                        End If

                        '処理区分(TM)仮置き
                        If item.processDiv = CommonConst.PROCESS_DIV_TM Then

                            Dim intBeforeTempData As Integer = 0

                            '仮置きリストでループ
                            For Each tempItem As EntryTempTmpDataDetail In item.matchTempList
                                '仮置きIDがある時
                                If Not String.IsNullOrEmpty(tempItem.tempId) Then
                                    '仮置きIDがある場合
                                    '仮置き資産テーブルから仮置きIDに該当する件数を取得して、登録済みの仮置き数とする
                                    intBeforeTempData += db.Database.SqlQuery(Of Integer)(GetTempAssetCNT(tempItem.tempId)).FirstOrDefault()

                                    '工事資産を更新する
                                    db.Database.ExecuteSqlCommand(UpdateConstAssetTemp(item, CommonConst.TASK_TEMP_REGIST))

                                    '仮置き資産を削除する
                                    db.Database.ExecuteSqlCommand(DeleteTempAssetTemp(tempItem))

                                End If
                            Next

                            Dim tempAssetCnt As Integer = 0
                            '仮置き資産の合計数を取得
                            '仮置きリストでループ
                            For Each tempItem As EntryTempTmpDataDetail In item.matchTempList
                                tempAssetCnt = tempAssetCnt + tempItem.matchTempAssetList.Count
                            Next

                            '仮置きリストのループ
                            For Each tempItem As EntryTempTmpDataDetail In item.matchTempList
                                '仮置きID
                                Dim tempId As Long? = Nothing

                                '仮置きIDがある時
                                If Not String.IsNullOrEmpty(tempItem.tempId) Then
                                    '仮置きを更新する
                                    db.Database.ExecuteSqlCommand(UpdateTemporaryTemp(item, tempItem,
                                                                         CommonConst.TASK_TEMP_REGIST))
                                    '棚卸ID保存
                                    tempId = tempItem.tempId


                                Else
                                    '仮置きを登録する
                                    db.Database.ExecuteSqlCommand(InsertTemporaryTemp(item, tempItem,
                                                                         CommonConst.TASK_TEMP_REGIST))

                                    '仮置き登録で登録されたTempId取得
                                    tempId = db.Database.SqlQuery(Of Decimal)(GetTempIdIdentity()).FirstOrDefault()

                                    'HT仮置きID更新用のパラメーターをセット
                                    tempIdList.Add(New tempIdList With {.beforeTempId = tempItem.beforeTempId, .afterTempId = tempId, .constId = tempItem.constNo})

                                End If

                                '仮置き資産リストのループ
                                For Each tempAssetItem As EntryTempAssetTmpDataDetail In tempItem.matchTempAssetList
                                    '仮置き資産を登録する
                                    db.Database.ExecuteSqlCommand(InsertTempAssetTemp(item, tempItem, tempAssetItem, tempId,
                                     CommonConst.TASK_TEMP_REGIST))

                                    '工事資産を登録する
                                    db.Database.ExecuteSqlCommand(InsertConstAssetLogTemp(item, tempItem, tempAssetItem, tempId,
                                                                                          CommonConst.CONST_MATCH_DIV.NO,
                                                                                          CommonConst.ASSET_STATUS_LEASE,
                                                                                          CommonConst.TEMP_FLAG.YES,
                                                                                          CommonConst.TASK_TEMP_REGIST))

                                    '工事資産を取得する
                                    Dim constAssetCnt As Integer = db.Database.SqlQuery(Of Integer)(GetConstAsset(tempAssetItem.constNo, tempAssetItem.assetNo)).FirstOrDefault()
                                    tempAssetItem.tempId = tempId
                                    If constAssetCnt > 0 Then
                                        'データ存在してる　更新
                                        '工事資産を更新する
                                        db.Database.ExecuteSqlCommand(UpdateConstAssetTemp(item, tempAssetItem, CommonConst.TASK_TEMP_REGIST))
                                    Else
                                        'データ存在していない　登録
                                        '工事資産を登録する
                                        db.Database.ExecuteSqlCommand(InsertConstAssetTemp(item, tempAssetItem,
                                                     CommonConst.CONST_ASSET_STATUS.CONST_LENT,
                                                     CommonConst.PREV_TEMP_FLAG.NO,
                                                     CommonConst.TASK_TEMP_REGIST))
                                    End If
                                Next

                            Next

                            ret.tempIdList = tempIdList.ToArray

                            '工事関連を編集する
                            '前回の工事履歴を取得する
                            Dim constLog As EntryConstLogDetail = db.Database.SqlQuery(Of EntryConstLogDetail)(GetConstLog(item.constNo)).FirstOrDefault()

                            '工事履歴を登録する
                            db.Database.ExecuteSqlCommand(InsertConstLogTemp(item, constLog, tempAssetCnt, intBeforeTempData,
                                                                         CommonConst.CONST_WORK_DIV.TEMP_REGIST,
                                                                         CommonConst.CONST_MATCH_DIV.NO,
                                                                         CommonConst.TASK_TEMP_REGIST))


                            ''工事マスタを更新する
                            'db.Database.ExecuteSqlCommand(UpdateConstRuctionTemp(item, CommonConst.TASK_TEMP_REGIST))
                            '工事マスタを更新する
                            db.Database.ExecuteSqlCommand(UpdateConstRuctionRet(item,
                                                     CommonConst.PREV_TEMP_FLAG.NO,
                                                     CommonConst.TASK_TEMP_REGIST))

                            '工事マスタの完了処理を行う
                            db.Database.ExecuteSqlCommand(UpdateConstFinish(item,
                                                 CommonConst.TASK_TEMP_REGIST))

                        End If

                    Next

                    '棚卸管理テーブルへ実棚件数を反映する。
                    param = New List(Of SqlParameter)
                    sql = UpdateInventoryNum(emp, param)
                    db.Database.ExecuteSqlCommand(sql, param.ToArray)

                    db.SaveChanges()
                    tx.Commit()
                    logger.Info("COMMIT")
                End Using

            Catch ex As System.Data.Common.DbException
                'DB系の例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_ERR_DB
            Catch ex As Exception
                '予期せぬ例外
                logger.Error(ex)
                CommonFunction.ErrorLogExecuteSql(db, TASK_NAME, ex.ToString)
                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                ret.wordCd = CommonWord.MSG_EXCEPTION
            End Try

            db.subTerminate()
            Return ret
        End Function
#End Region

#Region "Private メソッド"

#Region "一時データCreate"
        ''' <summary>
        ''' 照合処理データ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>処理データ更新用一時表DDL</returns>
        Private Function CreateTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #MATCH_PROCESS_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] [BIGINT] NOT NULL ,                /* 処理ID */")
            sb.AppendLine("	[PROCESS_DT] [CHAR] (8) NULL ,                      /* 処理日 */")
            sb.AppendLine("	[PROCESS_TM] [CHAR] (9) NULL ,                      /* 処理時刻 */")
            sb.AppendLine("	[EMPLOYEE_CD] [NVARCHAR] (15) NULL ,	            /* ログイン担当者コード */")
            sb.AppendLine("	[EMPLOYEE_NAME] [NVARCHAR] (60) NULL ,	            /* ログイン担当者名 */")
            sb.AppendLine("	[PROCESS_DIV] [NVARCHAR] (2) NULL ,	                /* 処理区分 */")
            sb.AppendLine("	[CONT_MATCH_DIV] [NVARCHAR] (1) NULL ,              /* 照合結果 */")
            sb.AppendLine("	[CONT_MATCH_NUM] [NVARCHAR] (10) NULL ,              /* 照合数 */")
            sb.AppendLine("	[CONST_NO] [NVARCHAR] (20) NULL ,                   /* 工事番号 */")
            sb.AppendLine("	[CONST_START_DT] [CHAR] (8) NULL ,                  /* 工事開始日 */")
            sb.AppendLine("	[CONST_END_DT] [CHAR] (8) NULL ,                    /* 工事終了日 */")
            sb.AppendLine("	[LEASE_EMPLOYEE_CD] [NVARCHAR] (15) NULL ,          /* 貸出先担当者コード */")
            sb.AppendLine("	[LEASE_DUE_DT] [CHAR] (8)  NULL ,                   /* 貸出返却予定日 */")
            sb.AppendLine("	[CONST_LENT_START_DT] [CHAR] (8) NULL ,             /* 工事貸出開始日 */")
            sb.AppendLine("	[CONST_LENT_END_DT] [CHAR] (8) NULL ,               /* 工事貸出終了日 */")
            sb.AppendLine("	[CONST_EMPLOYEE_CD] [NVARCHAR] (15) NULL ,          /* 工事責任者コード */")
            sb.AppendLine("	[MNG_LOC_L_CD] [NVARCHAR] (10) NULL ,               /* 管理ロケ大区分コード */")
            sb.AppendLine("	[LOC_L_CD] [NVARCHAR] (10) NULL ,                   /* ロケ大区分コード */")
            sb.AppendLine("	[LOC_M_CD] [NVARCHAR] (10) NULL ,                   /* ロケ中区分コード */")
            sb.AppendLine("	[LOC_S_CD] [NVARCHAR] (10) NULL ,                   /* ロケ小区分コード */")
            sb.AppendLine("	[LOC_L_NAME] [NVARCHAR] (60) NULL ,                 /* ロケ大区分名称 */")
            sb.AppendLine("	[LOC_M_NAME] [NVARCHAR] (60) NULL ,                 /* ロケ中区分名称 */")
            sb.AppendLine("	[LOC_S_NAME] [NVARCHAR] (60) NULL ,                 /* ロケ小区分名称 */")
            sb.AppendLine("	[RESERVE_ID] [BIGINT] NULL,                         /* 予約 */")


            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 貸出リストデータ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>処理データ更新用一時表DDL</returns>
        Private Function CreateLentTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #LENT_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] [BIGINT] NOT NULL ,        /* 処理順 */")
            sb.AppendLine("	[CONST_NO] [NVARCHAR] (20) NULL ,           /* 工事番号 */")

            sb.AppendLine("	[ASSET_NO] [NVARCHAR] (24) NULL ,           /* 資産番号 */")
            sb.AppendLine("	[ASSET_STATUS] [TINYINT]  NULL ,            /* 資産状態 */")
            sb.AppendLine("	[ASSET_NAME] [NVARCHAR] (60) NULL ,         /* 資産名称 */")
            sb.AppendLine("	[MAKER] [NVARCHAR] (60) NULL ,              /* メーカー */")
            sb.AppendLine("	[MODEL_NO] [NVARCHAR] (30) NULL ,           /* 型番 */")
            sb.AppendLine("	[PRODUCT_NO] [NVARCHAR] (30) NULL ,         /* 品番 */")
            sb.AppendLine("	[LEASE_STATUS] [TINYINT]  NULL ,            /* 貸出状態 */")
            sb.AppendLine("	[TASK_TS] [BIGINT]  NULL ,                  /* 処理日時 */")
            sb.AppendLine("	[ASSET_MNG_LOC_L_CD] [NVARCHAR] (10) NULL , /* 資産マスタ管理ロケ大区分コード */")
            sb.AppendLine("	[REMARK] [NVARCHAR] (255) NULL ,            /* 備考 */")
            sb.AppendLine("	[LEASE_DT] [CHAR] (8)  NULL ,               /* 払出日 */")
            sb.AppendLine("	[MAINTE_DAY_WORK_FLAG] [CHAR] (1)  NULL     /* メンテ日数稼働日フラグ */")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 返却リストデータ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>処理データ更新用一時表DDL</returns>
        Private Function CreateRetTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #RET_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] [BIGINT] NOT NULL ,        /* 処理順 */")
            sb.AppendLine("	[CONST_NO] [NVARCHAR] (20) NULL ,           /* 工事番号 */")
            sb.AppendLine("	[ASSET_NO] [NVARCHAR] (24) NULL ,           /* 資産番号 */")
            sb.AppendLine("	[ASSET_STATUS] [TINYINT]  NULL ,            /* 資産状態 */")
            sb.AppendLine("	[ASSET_NAME] [NVARCHAR] (60) NULL ,         /* 資産名称 */")
            sb.AppendLine("	[MAKER] [NVARCHAR] (60) NULL ,              /* メーカー */")
            sb.AppendLine("	[MODEL_NO] [NVARCHAR] (30) NULL ,           /* 型番 */")
            sb.AppendLine("	[PRODUCT_NO] [NVARCHAR] (30) NULL ,         /* 品番 */")
            sb.AppendLine("	[INSPE_FLG] [NVARCHAR] (1) NULL ,           /* 点検可否フラグ */")
            sb.AppendLine("	[LEASE_STATUS] [TINYINT]  NULL ,            /* 貸出状態 */")
            sb.AppendLine("	[TASK_TS] [BIGINT]  NULL ,                  /* 処理日時 */")
            sb.AppendLine("	[ASSET_MNG_LOC_L_CD] [NVARCHAR] (10) NULL , /* 資産マスタ管理ロケ大区分コード */")
            sb.AppendLine("	[CONT_MATCH_DIV] [NVARCHAR] (1) NULL ,      /* 照合結果 */")
            sb.AppendLine("	[MAINTE_DAY_WORK_FLAG] [CHAR] (1)  NULL     /* メンテ日数稼働日フラグ */")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 照合資産リストデータ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>処理データ更新用一時表DDL</returns>
        Private Function CreateMatchTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #MATCH_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] [BIGINT] NOT NULL ,        /* 処理順 */")
            sb.AppendLine("	[CONST_NO] [NVARCHAR] (20) NULL ,           /* 工事番号 */")
            sb.AppendLine("	[ASSET_NO] [NVARCHAR] (24) NULL ,           /* 資産番号 */")
            sb.AppendLine("	[ASSET_STATUS] [TINYINT]  NULL ,            /* 資産状態 */")
            sb.AppendLine("	[ASSET_NAME] [NVARCHAR] (60) NULL ,         /* 資産名称 */")
            sb.AppendLine("	[MAKER] [NVARCHAR] (60) NULL ,              /* メーカー */")
            sb.AppendLine("	[MODEL_NO] [NVARCHAR] (30) NULL ,           /* 型番 */")
            sb.AppendLine("	[PRODUCT_NO] [NVARCHAR] (30) NULL ,         /* 品番 */")
            sb.AppendLine("	[LEASE_STATUS] [TINYINT]  NULL ,            /* 貸出状態 */")
            sb.AppendLine("	[TASK_TS] [BIGINT]  NULL ,                  /* 処理日時 */")
            sb.AppendLine("	[ASSET_MNG_LOC_L_CD] [NVARCHAR] (10) NULL , /* 資産マスタ管理ロケ大区分コード */")
            sb.AppendLine("	[CONT_MATCH_DIV] [NVARCHAR] (1) NULL ,      /* 照合結果 */")
            sb.AppendLine("	[MAINTE_DAY_WORK_FLAG] [CHAR] (1)  NULL     /* メンテ日数稼働日フラグ */")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置きリストデータ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>処理データ更新用一時表DDL</returns>
        Private Function CreateTempTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #TEMP_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] [BIGINT] NOT NULL ,        /* 処理順 */")
            sb.AppendLine("	[CONST_NO] [NVARCHAR] (20) NULL ,           /* 工事番号 */")
            sb.AppendLine("	[TEMP_ID] [BIGINT] NULL ,                   /* 仮置きID */")
            sb.AppendLine("	[BEFORE_TEMP_ID] [NVARCHAR] (20) NULL ,     /* 以前仮置きID */")
            sb.AppendLine("	[TEMP_DT] [CHAR] (8) NULL ,                 /* 仮置き日 */")
            'sb.AppendLine("	[TEMP_WORK_EMPLOYEE_CD] [NVARCHAR] (15) NULL ,      /* 作業責任者コード */")
            'sb.AppendLine("	[TEMP_WORK_EMPLOYEE_NAME] [NVARCHAR] (60) NULL ,    /* 作業責任者氏名 */")
            sb.AppendLine("	[TEMP_INFO1] [NVARCHAR] (255) NULL ,	    /* 仮置き情報1 */")
            sb.AppendLine("	[TEMP_INFO2] [NVARCHAR] (255) NULL ,	    /* 仮置き情報2 */")
            sb.AppendLine("	[TEMP_INFO3] [NVARCHAR] (255) NULL ,	    /* 仮置き情報3 */")
            sb.AppendLine("	[TEMP_INFO4] [NVARCHAR] (255) NULL ,	    /* 仮置き情報4 */")
            sb.AppendLine("	[TEMP_INFO5] [NVARCHAR] (255) NULL ,	    /* 仮置き情報5 */")
            sb.AppendLine("	[TEMP_INFO6] [NVARCHAR] (255) NULL ,	    /* 仮置き情報6 */")
            sb.AppendLine("	[TEMP_INFO7] [NVARCHAR] (255) NULL ,	    /* 仮置き情報7 */")
            sb.AppendLine("	[TEMP_REMARK] [NVARCHAR] (255)  NULL        /* 備考 */")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置き資産リストデータ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>処理データ更新用一時表DDL</returns>
        Private Function CreateTempAssetTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #TEMP_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] [BIGINT] NOT NULL ,        /* 処理順 */")
            sb.AppendLine("	[CONST_NO] [NVARCHAR] (20) NULL ,           /* 工事番号 */")
            sb.AppendLine("	[TEMP_ID] [BIGINT] NULL ,                   /* 仮置きID */")
            sb.AppendLine("	[ASSET_NO] [NVARCHAR] (24) NULL ,           /* 資産番号 */")
            sb.AppendLine("	[ASSET_STATUS] [TINYINT]  NULL ,            /* 資産状態 */")
            sb.AppendLine("	[ASSET_NAME] [NVARCHAR] (60) NULL ,         /* 資産名称 */")
            sb.AppendLine("	[MAKER] [NVARCHAR] (60) NULL ,              /* メーカー */")
            sb.AppendLine("	[MODEL_NO] [NVARCHAR] (30) NULL ,           /* 型番 */")
            sb.AppendLine("	[PRODUCT_NO] [NVARCHAR] (30) NULL ,         /* 品番 */")
            sb.AppendLine("	[LEASE_STATUS] [TINYINT]  NULL ,            /* 貸出状態 */")
            sb.AppendLine("	[TASK_TS] [BIGINT]  NULL ,                  /* 処理日時 */")
            sb.AppendLine("	[ASSET_MNG_LOC_L_CD] [NVARCHAR] (10) NULL , /* 資産マスタ管理ロケ大区分コード */")
            sb.AppendLine("	[MAINTE_DAY_WORK_FLAG] [CHAR] (1)  NULL     /* メンテ日数稼働日フラグ */")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
#End Region

#Region "一時データにデータ追加"
        ''' <summary>
        ''' 貸出リスト情報を使い、照合処理データ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">処理データ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function LentInfoInsertTempTable(ByVal item As EntryMatchProcessDataDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #MATCH_PROCESS_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] ,          /* 処理順 */")
            sb.AppendLine("	[PROCESS_DT] ,              /* 処理日 */")
            sb.AppendLine("	[PROCESS_TM] ,              /* 処理時刻 */")
            sb.AppendLine("	[EMPLOYEE_CD] ,	            /* ログイン担当者コード */")
            sb.AppendLine("	[PROCESS_DIV] ,	            /* 処理区分 */")

            sb.AppendLine("	[CONST_NO] ,                /* 工事番号 */")
            sb.AppendLine("	[LEASE_EMPLOYEE_CD] ,       /* 貸出先社員コード */")
            sb.AppendLine("	[LEASE_DUE_DT] ,            /* 貸出返却予定日 */")
            sb.AppendLine("	[CONT_MATCH_DIV] ,          /* 照合結果 */")
            sb.AppendLine("	[CONT_MATCH_NUM]            /* 照合数 */")
            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")
            sb.AppendLine("	@ProcessDt,")
            sb.AppendLine("	@ProcessTm,")
            sb.AppendLine("	@EmployeeCd,")
            sb.AppendLine("	@ProcessDiv,")

            sb.AppendLine("	@ConstNo,")
            sb.AppendLine("	@LeaseEmployeeCd,")
            sb.AppendLine("	@LeaseDueDt,")
            sb.AppendLine("	@ContMatchDiv,")
            sb.AppendLine("	@ContMatchNum ")

            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.processLogId))
            param.Add(New SqlParameter("@ProcessDt", IIf(item.processDt Is Nothing, DBNull.Value, item.processDt)))
            param.Add(New SqlParameter("@ProcessTm", IIf(item.processTm Is Nothing, DBNull.Value, item.processTm)))
            param.Add(New SqlParameter("@EmployeeCd", IIf(item.employeeCd Is Nothing, DBNull.Value, item.employeeCd)))
            param.Add(New SqlParameter("@ProcessDiv", IIf(item.processDiv Is Nothing, DBNull.Value, item.processDiv)))

            param.Add(New SqlParameter("@ConstNo", IIf(item.constNo Is Nothing, DBNull.Value, item.constNo)))
            param.Add(New SqlParameter("@LeaseEmployeeCd", IIf(item.employeeCd Is Nothing, DBNull.Value, item.employeeCd)))
            param.Add(New SqlParameter("@LeaseDueDt", IIf(item.leaseDueDt Is Nothing, DBNull.Value, item.leaseDueDt)))

            param.Add(New SqlParameter("@ContMatchDiv", IIf(item.contMatchDiv Is Nothing, DBNull.Value, item.contMatchDiv)))
            param.Add(New SqlParameter("@ContMatchNum", IIf(item.contMatchNum Is Nothing, DBNull.Value, item.contMatchNum)))

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 貸出リスト情報を使い、照合処理データ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">処理データ情報</param>
        ''' <param name="subItem">貸出リストデータ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function LentInsertTempTable(ByVal item As EntryMatchProcessDataDetail, ByVal subItem As EntryLentAssetListDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #LENT_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] ,          /* 処理順 */")

            sb.AppendLine("	[ASSET_NO] ,                /* 資産番号 */")
            sb.AppendLine("	[REMARK] ,                  /* 備考 */")

            sb.AppendLine("	[LEASE_DT] ,                /* 払出日 */")

            sb.AppendLine("	[CONST_NO]                 /* 工事番号 */")

            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")


            sb.AppendLine("	@AssetNo,")
            sb.AppendLine("	@remark,")

            sb.AppendLine("	@LeaseDt,")


            sb.AppendLine("	@ConstNo ")

            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.processLogId))


            param.Add(New SqlParameter("@AssetNo", IIf(subItem.assetNo Is Nothing, DBNull.Value, subItem.assetNo)))
            param.Add(New SqlParameter("@remark", IIf(subItem.remark Is Nothing, DBNull.Value, subItem.remark)))

            param.Add(New SqlParameter("@LeaseDt", IIf(subItem.leaseDt Is Nothing, DBNull.Value, subItem.leaseDt)))

            param.Add(New SqlParameter("@ConstNo", IIf(item.constNo Is Nothing, DBNull.Value, item.constNo)))


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 返却リスト情報を使い、照合処理データ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">処理データ情報</param>
        ''' <param name="subItem">返却リストデータ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function RetInfoInsertTempTable(ByVal item As EntryMatchProcessDataDetail, ByVal subItem As EntryRetAssetListDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #RET_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] ,          /* 処理順 */")

            sb.AppendLine("	[ASSET_NO] ,                /* 資産番号 */")
            sb.AppendLine("	[CONT_MATCH_DIV] ,    /* 資産ごとの照合結果 */")
            sb.AppendLine("	[INSPE_FLG] ,         /* 点検可否フラグ */")
            sb.AppendLine("	[CONST_NO]                 /* 工事番号 */")
            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")

            sb.AppendLine("	@AssetNo,")
            sb.AppendLine("	@ContMatchDiv,")
            sb.AppendLine("	@InspeFlg,")
            sb.AppendLine("	@ConstNo")

            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.processLogId))
            param.Add(New SqlParameter("@AssetNo", IIf(subItem.assetNo Is Nothing, DBNull.Value, subItem.assetNo)))
            param.Add(New SqlParameter("@ContMatchDiv", IIf(subItem.contMatchDiv Is Nothing, DBNull.Value, subItem.contMatchDiv)))
            param.Add(New SqlParameter("@InspeFlg", IIf(subItem.inspeFlag Is Nothing, DBNull.Value, subItem.inspeFlag)))
            param.Add(New SqlParameter("@ConstNo", IIf(item.constNo Is Nothing, DBNull.Value, item.constNo)))


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 照合資産リスト情報を使い、照合処理データ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">処理データ情報</param>
        ''' <param name="subItem">照合資産リストデータ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function MatchAssetInfoInsertTempTable(ByVal item As EntryMatchProcessDataDetail, ByVal subItem As EntryMatchAssetListDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #MATCH_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] ,          /* 処理順 */")
            sb.AppendLine("	[ASSET_NO] ,                /* 資産番号 */")
            sb.AppendLine("	[CONT_MATCH_DIV] ,    /* 資産ごとの照合結果 */")
            sb.AppendLine("	[CONST_NO]                 /* 工事番号 */")
            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")
            sb.AppendLine("	@AssetNo,")
            sb.AppendLine("	@ContMatchDiv,")
            sb.AppendLine("	@ConstNo")

            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.processLogId))
            param.Add(New SqlParameter("@AssetNo", IIf(subItem.assetNo Is Nothing, DBNull.Value, subItem.assetNo)))
            param.Add(New SqlParameter("@ContMatchDiv", IIf(subItem.contMatchDiv Is Nothing, DBNull.Value, subItem.contMatchDiv)))
            param.Add(New SqlParameter("@ConstNo", IIf(item.constNo Is Nothing, DBNull.Value, item.constNo)))

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置きリスト情報を使い、照合処理データ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">処理データ情報</param>
        ''' <param name="subItem">仮置きリストデータ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function TempInfoInsertTempTable(ByVal item As EntryMatchProcessDataDetail, ByVal subItem As EntryTempListDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #TEMP_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] ,          /* 処理順 */")
            sb.AppendLine("	[TEMP_REMARK] ,                  /* 備考 */")

            sb.AppendLine("	[TEMP_ID] ,                 /* 仮置きID */")
            sb.AppendLine("	[BEFORE_TEMP_ID] ,          /* 以前仮置きID */")
            sb.AppendLine("	[TEMP_DT] ,                 /* 持出日 */")
            'sb.AppendLine("	[TEMP_WORK_EMPLOYEE_CD] ,   /* 作業責任者コード */")
            'sb.AppendLine("	[TEMP_WORK_EMPLOYEE_NAME] , /* 作業責任者氏名 */")
            sb.AppendLine("	[TEMP_INFO1] ,              /* 仮置き情報1 */")
            sb.AppendLine("	[TEMP_INFO2] ,              /* 仮置き情報2 */")
            sb.AppendLine("	[TEMP_INFO3] ,              /* 仮置き情報3 */")
            sb.AppendLine("	[TEMP_INFO4] ,              /* 仮置き情報4 */")
            sb.AppendLine("	[TEMP_INFO5] ,              /* 仮置き情報5 */")
            sb.AppendLine("	[TEMP_INFO6] ,              /* 仮置き情報6 */")
            sb.AppendLine("	[TEMP_INFO7] ,              /* 仮置き情報7 */")

            sb.AppendLine("	[CONST_NO]                  /* 工事番号 */")

            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")

            sb.AppendLine("	@TempRemark,")

            sb.AppendLine("	@TempId,")
            sb.AppendLine("	@BeforeTempId,")
            sb.AppendLine("	@TempDt,")
            'sb.AppendLine("	@TempWorkEmployeeCd,")
            'sb.AppendLine("	@TempWorkEmployeeName,")
            sb.AppendLine("	@TempInfo01,")
            sb.AppendLine("	@TempInfo02,")
            sb.AppendLine("	@TempInfo03,")
            sb.AppendLine("	@TempInfo04,")
            sb.AppendLine("	@TempInfo05,")
            sb.AppendLine("	@TempInfo06,")
            sb.AppendLine("	@TempInfo07,")

            sb.AppendLine("	@ConstNo ")


            sb.AppendLine(")")

            Dim beforeTempId As String = ""

            'TempIdにzが入っている場合　空白に変更
            If subItem.tempId.Contains("z") Then
                ' 対象文字列が含まれている場合の処理
                '元仮置きID 保存
                beforeTempId = subItem.tempId
                subItem.tempId = ""
            End If

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.processLogId))

            param.Add(New SqlParameter("@TempRemark", IIf(subItem.tempRemark Is Nothing, DBNull.Value, subItem.tempRemark)))
            'TempIDが入っている場合
            If String.IsNullOrEmpty(subItem.tempId) Then
                '新規登録の場合NULLを登録
                param.Add(New SqlParameter("@TempId", DBNull.Value))
            Else
                '変更の場合はTempIdを登録
                param.Add(New SqlParameter("@TempId", subItem.tempId))
            End If

            'BeforeTempIDが入っている場合
            param.Add(New SqlParameter("@BeforeTempId", IIf(beforeTempId Is Nothing, DBNull.Value, beforeTempId)))

            param.Add(New SqlParameter("@TempDt", IIf(subItem.tempDt Is Nothing, DBNull.Value, subItem.tempDt)))
            'param.Add(New SqlParameter("@TempWorkEmployeeCd", IIf(subItem.tempEmpCd Is Nothing, DBNull.Value, subItem.tempEmpCd)))
            'param.Add(New SqlParameter("@TempWorkEmployeeName", IIf(subItem.tempEmpName Is Nothing, DBNull.Value, subItem.tempEmpName)))
            param.Add(New SqlParameter("@TempInfo01", IIf(subItem.tempInfo1 Is Nothing, DBNull.Value, subItem.tempInfo1)))
            param.Add(New SqlParameter("@TempInfo02", IIf(subItem.tempInfo2 Is Nothing, DBNull.Value, subItem.tempInfo2)))
            param.Add(New SqlParameter("@TempInfo03", IIf(subItem.tempInfo3 Is Nothing, DBNull.Value, subItem.tempInfo3)))
            param.Add(New SqlParameter("@TempInfo04", IIf(subItem.tempInfo4 Is Nothing, DBNull.Value, subItem.tempInfo4)))
            param.Add(New SqlParameter("@TempInfo05", IIf(subItem.tempInfo5 Is Nothing, DBNull.Value, subItem.tempInfo5)))
            param.Add(New SqlParameter("@TempInfo06", IIf(subItem.tempInfo6 Is Nothing, DBNull.Value, subItem.tempInfo6)))
            param.Add(New SqlParameter("@TempInfo07", IIf(subItem.tempInfo7 Is Nothing, DBNull.Value, subItem.tempInfo7)))

            param.Add(New SqlParameter("@ConstNo", IIf(item.constNo Is Nothing, DBNull.Value, item.constNo)))


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 照合資産リスト情報を使い、照合処理データ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">処理データ情報</param>
        ''' <param name="subItem">照合資産リストデータ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function TempAssetInfoInsertTempTable(ByVal item As EntryMatchProcessDataDetail, ByVal subItem As EntryTempListDetail, ByVal subAssetItem As EntryTempAssetListDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #TEMP_ASSET_LIST_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] ,          /* 処理順 */")
            sb.AppendLine("	[TEMP_ID] ,                 /* 仮置きID */")
            sb.AppendLine("	[ASSET_NO] ,                /* 資産番号 */")
            sb.AppendLine("	[CONST_NO]                 /* 工事番号 */")
            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")
            sb.AppendLine("	@TempId,")
            sb.AppendLine("	@AssetNo,")
            sb.AppendLine("	@ConstNo")

            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.processLogId))
            'TempIDが入っている場合
            If String.IsNullOrEmpty(subItem.tempId) Then
                '新規登録の場合NULLを登録
                param.Add(New SqlParameter("@TempId", DBNull.Value))
            Else
                '変更の場合はTempIdを登録
                param.Add(New SqlParameter("@TempId", subItem.tempId))
            End If
            param.Add(New SqlParameter("@AssetNo", IIf(subAssetItem.no Is Nothing, DBNull.Value, subAssetItem.no)))
            param.Add(New SqlParameter("@ConstNo", IIf(item.constNo Is Nothing, DBNull.Value, item.constNo)))


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

#End Region

#Region "一時データの必要データ更新"
        ''' <summary>
        ''' 貸出先名称を更新する
        ''' </summary>
        Private Function UpdateLeasePlaceName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine($" LENT_ASSET_LIST_TEMP")
            sb.AppendLine(" SET")
            sb.AppendLine("	LEASE_PLACE_NAME = PLA.LEASE_PLACE_NAME")
            sb.AppendLine(" FROM")
            sb.AppendLine($" #LENT_ASSET_LIST_TEMP AS LENT_ASSET_LIST_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LEASE_PLACE AS PLA")
            sb.AppendLine("		ON")
            sb.AppendLine("			LENT_ASSET_LIST_TEMP.LEASE_PLACE_CD = PLA.LEASE_PLACE_CD ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ログイン担当者名を更新する
        ''' </summary>
        Private Function UpdateEmployeeName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	MATCH_PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	 EMPLOYEE_NAME = EMP.EMPLOYEE_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#MATCH_PROCESS_TEMP AS MATCH_PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_EMPLOYEE AS EMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			MATCH_PROCESS_TEMP.EMPLOYEE_CD = EMP.EMPLOYEE_CD")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケ大区分名称を更新する
        ''' </summary>
        Private Function UpdateLargeLocName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	MATCH_PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	LOC_L_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#MATCH_PROCESS_TEMP AS MATCH_PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			MATCH_PROCESS_TEMP.LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine($"		And LOC_DIV = '{CommonConst.LOC_L_DIV}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケ中区分名称を更新する
        ''' </summary>
        Private Function UpdateMiddleLocName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	MATCH_PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	LOC_M_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#MATCH_PROCESS_TEMP AS MATCH_PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			MATCH_PROCESS_TEMP.LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine("		AND MATCH_PROCESS_TEMP.LOC_M_CD = LOC.LOC_M_CD")
            sb.AppendLine($"	AND LOC_DIV = '{CommonConst.LOC_M_DIV}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケ小区分名称を更新する
        ''' </summary>
        Private Function UpdateSmallLocName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	MATCH_PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	LOC_S_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#MATCH_PROCESS_TEMP AS MATCH_PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			MATCH_PROCESS_TEMP.LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine("		AND MATCH_PROCESS_TEMP.LOC_M_CD = LOC.LOC_M_CD")
            sb.AppendLine("		AND MATCH_PROCESS_TEMP.LOC_S_CD = LOC.LOC_S_CD")
            sb.AppendLine($"	AND LOC_DIV = '{CommonConst.LOC_S_DIV}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function


        ''' <summary>
        ''' 貸出先担当者名を更新する
        ''' </summary>
        Private Function UpdateLeaseEmployeeName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	LENT_ASSET_LIST_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	 LEASE_EMPLOYEE_NAME = EMP.EMPLOYEE_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#LENT_ASSET_LIST_TEMP AS LENT_ASSET_LIST_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_EMPLOYEE AS EMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			LENT_ASSET_LIST_TEMP.LEASE_EMPLOYEE_CD = EMP.EMPLOYEE_CD")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産情報を更新する
        ''' </summary>
        Private Function UpdateAssetInfomation(ByVal tableName As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine($" {tableName}")
            sb.AppendLine("SET")
            sb.AppendLine("  ASSET_STATUS = ASSET.ASSET_STATUS")
            sb.AppendLine(" ,ASSET_NAME = ASSET.ASSET_NAME")
            sb.AppendLine(" ,ASSET_MNG_LOC_L_CD = ASSET.MNG_LOC_L_CD")
            sb.AppendLine(" ,MAKER = ASSET.MAKER")
            sb.AppendLine(" ,PRODUCT_NO = ASSET.PRODUCT_NO")
            sb.AppendLine(" ,MODEL_NO = ASSET.MODEL_NO")
            sb.AppendLine(" ,LEASE_STATUS = ASSET.LEASE_STATUS")
            sb.AppendLine(" ,MAINTE_DAY_WORK_FLAG = ASSET.MAINTE_DAY_WORK_FLAG")
            sb.AppendLine(" ,TASK_TS = ASSET.TASK_TS")
            sb.AppendLine("FROM")
            sb.AppendLine($" #{tableName} AS {tableName} ")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_ASSET AS ASSET")
            sb.AppendLine("		ON")
            sb.AppendLine($"		{tableName}.ASSET_NO = ASSET.ASSET_NO")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事情報を更新する
        ''' </summary>
        Private Function UpdateConstRuctionInfomation() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine(" MATCH_PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("  CONST_START_DT = CONST.CONST_START_DT")
            sb.AppendLine(" ,CONST_END_DT = CONST.CONST_END_DT")
            sb.AppendLine(" ,CONST_LENT_START_DT = CONST.CONST_LENT_START_DT")
            sb.AppendLine(" ,CONST_LENT_END_DT = CONST.CONST_LENT_END_DT")
            sb.AppendLine(" ,CONST_EMPLOYEE_CD = CONST.CONST_EMPLOYEE_CD")
            sb.AppendLine(" ,MNG_LOC_L_CD = CONST.MNG_LOC_L_CD")
            sb.AppendLine("FROM")
            sb.AppendLine("	#MATCH_PROCESS_TEMP AS MATCH_PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_CONSTRUCTION AS CONST")
            sb.AppendLine("		ON")
            sb.AppendLine("			MATCH_PROCESS_TEMP.CONST_NO = CONST.CONST_NO")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約情報を更新する
        ''' </summary>
        Private Function UpdateReserveInfomation() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine(" MATCH_PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("  RESERVE_ID = RESERVE.RESERVE_ID")
            sb.AppendLine("FROM")
            sb.AppendLine("	#MATCH_PROCESS_TEMP AS MATCH_PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		T_RESERVE AS RESERVE")
            sb.AppendLine("		ON")
            sb.AppendLine("			MATCH_PROCESS_TEMP.CONST_NO = RESERVE.REMARK")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
#End Region

#Region "一時全データ取得"
        ''' <summary>
        ''' 一時表の処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllProcessData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_LOG_ID             AS ProcessLogId ")
            sb.AppendLine(" ,PROCESS_DT                 AS ProcessDt ")
            sb.AppendLine(" ,PROCESS_TM                 AS ProcessTm ")
            sb.AppendLine(" ,EMPLOYEE_CD                AS employeeCd ")
            sb.AppendLine(" ,EMPLOYEE_NAME              AS employeeName ")
            sb.AppendLine(" ,PROCESS_DIV                AS processDiv ")
            sb.AppendLine(" ,CONT_MATCH_DIV             AS contMatchDiv ")
            sb.AppendLine(" ,CONT_MATCH_NUM             AS contMatchNum ")
            sb.AppendLine(" ,CONST_NO                   AS constNo ")
            sb.AppendLine(" ,CONST_START_DT             AS constStartDt ")
            sb.AppendLine(" ,CONST_END_DT               AS constEndDt ")

            sb.AppendLine(" ,LEASE_EMPLOYEE_CD          AS leaseEmployeeCd ")
            sb.AppendLine(" ,LEASE_DUE_DT               AS leaseDueDt ")

            sb.AppendLine(" ,CONST_LENT_START_DT        AS constLentStartDt ")
            sb.AppendLine(" ,CONST_LENT_END_DT          AS constLentEndDt ")
            sb.AppendLine(" ,CONST_EMPLOYEE_CD          AS constEmployeeCd ")
            sb.AppendLine(" ,MNG_LOC_L_CD               AS mngLocLCd ")
            sb.AppendLine(" ,RESERVE_ID                 AS reserveId ")


            sb.AppendLine("FROM")
            sb.AppendLine(" #MATCH_PROCESS_TEMP")
            sb.AppendLine("ORDER BY")
            sb.AppendLine(" processLogId")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 貸出リスト一時表の処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllLengData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_LOG_ID             AS ProcessLogId ")
            sb.AppendLine(" ,ASSET_NO                   AS assetNo ")
            sb.AppendLine(" ,REMARK                     AS remark ")
            sb.AppendLine(" ,ASSET_STATUS               AS assetStart ")
            sb.AppendLine(" ,ASSET_NAME                 AS assetName ")
            sb.AppendLine(" ,MAKER                      AS maker ")
            sb.AppendLine(" ,MODEL_NO                   AS modelNo ")
            sb.AppendLine(" ,PRODUCT_NO                 AS productNo ")
            sb.AppendLine(" ,LEASE_STATUS               AS leaseStatus ")
            sb.AppendLine(" ,ASSET_MNG_LOC_L_CD         AS assetMngLocLCd ")
            sb.AppendLine(" ,TASK_TS                    AS taskTs ")
            sb.AppendLine(" ,LEASE_DT                   AS leaseDt ")
            sb.AppendLine(" ,MAINTE_DAY_WORK_FLAG       AS mainteDayWorkFlag ")
            sb.AppendLine(" ,CONST_NO                   AS constNo ")

            sb.AppendLine("FROM")
            sb.AppendLine(" #LENT_ASSET_LIST_TEMP")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 返却リスト一時表の処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllRetData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_LOG_ID             AS ProcessLogId ")
            sb.AppendLine(" ,ASSET_NO                   AS assetNo ")
            sb.AppendLine(" ,ASSET_STATUS               AS assetStart ")
            sb.AppendLine(" ,ASSET_NAME                 AS assetName ")
            sb.AppendLine(" ,MAKER                      AS maker ")
            sb.AppendLine(" ,MODEL_NO                   AS modelNo ")
            sb.AppendLine(" ,PRODUCT_NO                 AS productNo ")
            sb.AppendLine(" ,INSPE_FLG                  AS inspeFlg ")
            sb.AppendLine(" ,LEASE_STATUS               AS leaseStatus ")
            sb.AppendLine(" ,ASSET_MNG_LOC_L_CD         AS assetMngLocLCd ")
            sb.AppendLine(" ,TASK_TS                    AS taskTs ")
            sb.AppendLine(" ,CONST_NO                   AS constNo ")
            sb.AppendLine(" ,CONT_MATCH_DIV             AS contMatchDiv ")


            sb.AppendLine("FROM")
            sb.AppendLine(" #RET_ASSET_LIST_TEMP")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 照合資産リスト一時表の処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllMatchAssetData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_LOG_ID             AS ProcessLogId ")
            sb.AppendLine(" ,ASSET_NO                   AS assetNo ")
            sb.AppendLine(" ,ASSET_STATUS               AS assetStart ")
            sb.AppendLine(" ,ASSET_NAME                 AS assetName ")
            sb.AppendLine(" ,MAKER                      AS maker ")
            sb.AppendLine(" ,MODEL_NO                   AS modelNo ")
            sb.AppendLine(" ,PRODUCT_NO                 AS productNo ")
            sb.AppendLine(" ,LEASE_STATUS               AS leaseStatus ")
            sb.AppendLine(" ,ASSET_MNG_LOC_L_CD         AS assetMngLocLCd ")
            sb.AppendLine(" ,TASK_TS                    AS taskTs ")
            sb.AppendLine(" ,CONST_NO                   AS constNo ")
            sb.AppendLine(" ,CONT_MATCH_DIV             AS contMatchDiv ")


            sb.AppendLine("FROM")
            sb.AppendLine(" #MATCH_ASSET_LIST_TEMP")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置きリスト一時表の処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllTempData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_LOG_ID             AS ProcessLogId ")

            sb.AppendLine(" ,CAST(TEMP_ID AS VARCHAR)   AS tempId ")
            sb.AppendLine(" ,BEFORE_TEMP_ID             AS beforeTempId ")
            sb.AppendLine(" ,TEMP_DT                    AS tempDt ")
            'sb.AppendLine(" ,TEMP_WORK_EMPLOYEE_CD      AS tempWorkEmployeeCd ")
            'sb.AppendLine(" ,TEMP_WORK_EMPLOYEE_NAME    AS tempWorkEmployeeName ")
            sb.AppendLine(" ,TEMP_INFO1                 AS tempInfo1 ")
            sb.AppendLine(" ,TEMP_INFO2                 AS tempInfo2 ")
            sb.AppendLine(" ,TEMP_INFO3                 AS tempInfo3 ")
            sb.AppendLine(" ,TEMP_INFO4                 AS tempInfo4 ")
            sb.AppendLine(" ,TEMP_INFO5                 AS tempInfo5 ")
            sb.AppendLine(" ,TEMP_INFO6                 AS tempInfo6 ")
            sb.AppendLine(" ,TEMP_INFO7                 AS tempInfo7 ")

            sb.AppendLine(" ,CONST_NO                   AS constNo ")
            sb.AppendLine(" ,TEMP_REMARK                AS remark ")


            sb.AppendLine("FROM")
            sb.AppendLine(" #TEMP_LIST_TEMP")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 照合資産リスト一時表の処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllTempAssetData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_LOG_ID             AS ProcessLogId ")
            sb.AppendLine(" ,CAST(TEMP_ID AS VARCHAR)   AS tempId ")
            sb.AppendLine(" ,ASSET_NO                   AS assetNo ")
            sb.AppendLine(" ,ASSET_STATUS               AS assetStart ")
            sb.AppendLine(" ,ASSET_NAME                 AS assetName ")
            sb.AppendLine(" ,MAKER                      AS maker ")
            sb.AppendLine(" ,MODEL_NO                   AS modelNo ")
            sb.AppendLine(" ,PRODUCT_NO                 AS productNo ")
            sb.AppendLine(" ,LEASE_STATUS               AS leaseStatus ")
            sb.AppendLine(" ,ASSET_MNG_LOC_L_CD         AS assetMngLocLCd ")
            sb.AppendLine(" ,TASK_TS                    AS taskTs ")
            sb.AppendLine(" ,CONST_NO                   AS constNo ")

            sb.AppendLine("FROM")
            sb.AppendLine(" #TEMP_ASSET_LIST_TEMP")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
#End Region

#Region "共通"
        ''' <summary>
        ''' 前回の工事履歴情報を取得する
        ''' </summary>
        Private Function GetConstLog(ByVal constNo As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  CONST_NO               AS constNo ")
            sb.AppendLine(" ,LENT_NUM               AS lentNum ")
            sb.AppendLine(" ,LEASE_NUM              AS leaseNum ")
            sb.AppendLine(" ,TEMP_REG_NUM           AS tempRegNum ")
            sb.AppendLine(" ,CONST_IN_MATCH_NUM     AS constInMatchNum ")
            sb.AppendLine(" ,CONST_OUT_MATCH_NUM    AS constOutMatchNum ")
            sb.AppendLine(" ,CONST_IN_NUM           AS constInNum ")
            sb.AppendLine(" ,TEMP_NUM               AS tempNum ")
            sb.AppendLine(" ,CONST_OUT_NUM          AS constOutNum ")

            sb.AppendLine(" FROM ")
            sb.AppendLine(" T_CONST_LOG")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{constNo}' ")
            sb.AppendLine(" ORDER BY ")
            sb.AppendLine(" CONST_WORK_TS DESC ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事履歴情報の存在確認する。
        ''' </summary>
        Private Function GetConstLog(ByVal constNo As String, ByVal taskTs As String, ByVal workDiv As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  COUNT(1) ")
            sb.AppendLine(" FROM ")
            sb.AppendLine(" T_CONST_LOG")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($"    CONST_NO = '{constNo}' ")
            sb.AppendLine($"AND CONST_WORK_TS = CONVERT(BIGINT,LEFT({taskTs}, 14)) ")
            sb.AppendLine($"AND CONST_WORK_DIV = '{workDiv}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 前回の工事資産履歴情報を取得する
        ''' </summary>
        Private Function GetConstAssetLog(ByVal constNo As String, ByVal assetNo As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  CONST_NO               AS constNo ")
            sb.AppendLine(" ,ASSET_NO               AS assetNo ")
            sb.AppendLine(" ,CONST_MATCH_STATUS     AS constMatchStatus ")
            sb.AppendLine(" ,TEMP_FLAG              AS tempFlag ")
            sb.AppendLine(" ,TEMP_ID                AS tempId ")

            sb.AppendLine(" FROM ")
            sb.AppendLine(" T_CONST_ASSET_LOG")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{constNo}' ")
            sb.AppendLine($"AND ASSET_NO = '{assetNo}' ")
            sb.AppendLine(" ORDER BY ")
            sb.AppendLine(" CONST_WORK_TS DESC ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を取得する
        ''' </summary>
        Private Function GetConstAsset(ByVal constNo As String, ByVal assetNo As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT ")
            sb.AppendLine("  COUNT( * ) ")

            sb.AppendLine(" FROM ")
            sb.AppendLine(" T_CONST_ASSET ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{constNo}' ")
            sb.AppendLine($" AND ASSET_NO = '{assetNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

#End Region

#Region "貸出で使用"
        ''' <summary>
        ''' 一時表の品番ごとの処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllProductNoData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine(" ASSET.PRODUCT_NO                        AS productNo ")
            sb.AppendLine(" ,PROCESS.LEASE_DUE_DT          AS assetLeaseDueDt ")
            sb.AppendLine(" ,ASSET.MAINTE_DAY_WORK_FLAG  AS mainteDayWorkFlag ")
            sb.AppendLine(" ,PROCESS.PROCESS_DT          AS processDt ")
            sb.AppendLine(" ,PROCESS.CONST_NO            AS constNo ")
            sb.AppendLine(" ,COUNT(*)                    AS recordCount ")

            sb.AppendLine("FROM ")
            sb.AppendLine(" #MATCH_PROCESS_TEMP AS PROCESS")
            sb.AppendLine(" INNER JOIN")
            sb.AppendLine(" #LENT_ASSET_LIST_TEMP AS ASSET")
            sb.AppendLine(" ON PROCESS.PROCESS_LOG_ID = ASSET.PROCESS_LOG_ID")
            sb.AppendLine(" GROUP BY ")
            sb.AppendLine(" ASSET.PRODUCT_NO,PROCESS.LEASE_DUE_DT,ASSET.MAINTE_DAY_WORK_FLAG,PROCESS.PROCESS_DT,PROCESS.CONST_NO ")
            sb.AppendLine(" ORDER BY ")
            sb.AppendLine(" ASSET.PRODUCT_NO ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約IDのシーケンス取得する
        ''' </summary>
        Private Function GetReserveIdSequence() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("select next value for RESERVE_ID_SEQUENCE")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約テーブルを登録する（処理区分：貸出）
        ''' </summary>
        Private Function InsertReserve(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, reserveId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("INSERT INTO T_RESERVE")
            sb.AppendLine("(")
            sb.AppendLine("  RESERVE_ID")
            sb.AppendLine(" ,REQ_EMP_CD")
            sb.AppendLine(" ,CHARGE_EMP_CD")
            sb.AppendLine(" ,REQUEST_DT")
            sb.AppendLine(" ,REAL_START_DT")
            sb.AppendLine(" ,REAL_END_DT")
            sb.AppendLine(" ,USER_START_DT")
            sb.AppendLine(" ,USER_END_DT")
            sb.AppendLine(" ,LEASE_PLACE_CD")
            sb.AppendLine(" ,LEASE_USE")
            sb.AppendLine(" ,MNG_LOC_L_CD")
            sb.AppendLine(" ,DEST_NAME1")
            sb.AppendLine(" ,LEASE_CHG_TIMES")
            sb.AppendLine(" ,RESERVE_STATUS")
            sb.AppendLine(" ,REMARK")
            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{reserveId}'")
            sb.AppendLine($",'{matchProcessTmpList.employeeCd}'")
            sb.AppendLine($",'{matchProcessTmpList.employeeCd}'")
            sb.AppendLine($",'{matchProcessTmpList.processDt}'")
            sb.AppendLine($",'{matchProcessTmpList.processDt}'")
            If String.IsNullOrEmpty(matchProcessTmpList.leaseDueDt) = False Then
                '値がある場合
                sb.AppendLine($",'{matchProcessTmpList.leaseDueDt}'")
            Else
                '値がない場合
                sb.AppendLine($",'{matchProcessTmpList.processDt}'")
            End If
            sb.AppendLine($",'{matchProcessTmpList.processDt}'")

            If String.IsNullOrEmpty(matchProcessTmpList.leaseDueDt) = False Then
                '値がある場合
                sb.AppendLine($",'{matchProcessTmpList.leaseDueDt}'")
            Else
                '値がない場合
                sb.AppendLine($",'{matchProcessTmpList.processDt}'")
            End If
            sb.AppendLine($",' '")
            sb.AppendLine($",' '")
            sb.AppendLine($",'{matchProcessTmpList.mngLocLCd}'")
            sb.AppendLine($",' '")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'{matchProcessTmpList.constNo}'")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{CommonConst.TASK_LENT_REGIST}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事履歴を登録する（処理区分：貸出）
        ''' </summary>
        Private Function InsertConstLogLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                        ByVal constLog As EntryConstLogDetail,
                                        ByVal lentCnt As Long,
                                        ByVal constWorkDiv As String,
                                        ByVal constMatchDiv As String,
                                        ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,CONST_WORK_DIV")
            sb.AppendLine(" ,CONST_MATCH_DIV")

            sb.AppendLine(" ,LENT_NUM")
            sb.AppendLine(" ,LEASE_NUM")
            sb.AppendLine(" ,TEMP_REG_NUM")
            sb.AppendLine(" ,CONST_IN_MATCH_NUM")
            sb.AppendLine(" ,CONST_OUT_MATCH_NUM")
            sb.AppendLine(" ,CONST_IN_NUM")
            sb.AppendLine(" ,TEMP_NUM")
            sb.AppendLine(" ,CONST_OUT_NUM")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{constWorkDiv}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'{0 + lentCnt}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND CONST_ASSET_STATUS = '1' )")                                                      '持込数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                              '現在仮置数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NULL AND  CONST_ASSET_STATUS = '1')")                                 '持帰数

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function


        ''' <summary>
        ''' 工事資産履歴を登録する（処理区分：貸出）
        ''' </summary>
        Private Function InsertConstAssetLogLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchLentList As EntryMatchLentTmpDataDetail,
                                             ByVal constMatchDiv As String,
                                             ByVal constMatchStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,CONST_MATCH_STATUS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{matchLentList.assetNo}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'{constMatchStatus}'")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を更新する（処理区分：貸出）
        ''' </summary>
        Private Function UpdateConstAssetLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchLentList As EntryMatchLentTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_CONST_ASSET")
            sb.AppendLine(" SET")
            sb.AppendLine($"  CONST_ASSET_STATUS = '{constAssetStatus}' ")
            sb.AppendLine($"	 ,TEMP_ID = NULL ")
            sb.AppendLine($" ,TASK_TS =   CONVERT(BIGINT,LEFT({taskIs}, 14)) ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")
            sb.AppendLine($" AND ASSET_NO = '{matchLentList.assetNo}' ")
            sb.AppendLine($" AND TASK_TS <=   CONVERT(BIGINT,LEFT({taskIs}, 14)) ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を更新する（処理区分：仮置き）
        ''' </summary>
        Private Function UpdateConstAssetTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal tmpList As EntryTempAssetTmpDataDetail,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_CONST_ASSET")
            sb.AppendLine(" SET")
            sb.AppendLine($"  TEMP_ID = {tmpList.tempId} ")
            sb.AppendLine($" ,TASK_TS =   CONVERT(BIGINT,LEFT({taskIs}, 14)) ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")
            sb.AppendLine($" AND ASSET_NO = '{tmpList.assetNo}' ")
            sb.AppendLine($" AND TASK_TS <=   CONVERT(BIGINT,LEFT({taskIs}, 14)) ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を登録する（処理区分：貸出）
        ''' </summary>
        Private Function InsertConstAssetLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchLentList As EntryMatchLentTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal prevTempFlag As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_ASSET_STATUS")
            sb.AppendLine(" ,TEMP_ID")
            sb.AppendLine(" ,PREV_TEMP_FLAG")
            sb.AppendLine(" ,TASK_TS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($",'{matchLentList.assetNo}'")
            sb.AppendLine($",'{constAssetStatus}'")
            sb.AppendLine($",NULL")
            sb.AppendLine($",'{prevTempFlag}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を登録する（処理区分：仮置き）
        ''' </summary>
        Private Function InsertConstAssetTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal tmpList As EntryTempAssetTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal prevTempFlag As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_ASSET_STATUS")
            sb.AppendLine(" ,TEMP_ID")
            sb.AppendLine(" ,PREV_TEMP_FLAG")
            sb.AppendLine(" ,TASK_TS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($",'{tmpList.assetNo}'")
            sb.AppendLine($",'{constAssetStatus}'")
            sb.AppendLine($",'{tmpList.tempId }'")
            sb.AppendLine($",'{prevTempFlag}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事マスタを更新する（処理区分：貸出）
        ''' </summary>
        Private Function UpdateConstRuctionLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	M_CONSTRUCTION")
            sb.AppendLine(" SET")
            sb.AppendLine($"  CONST_IN_NUM = ISNULL(CONST_IN_NUM + {matchProcessTmpList.matchLentList.Count},0) ")
            sb.AppendLine($" ,CUR_LENT_NUM = ISNULL(CUR_LENT_NUM + {matchProcessTmpList.matchLentList.Count},0) ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：貸出）
        ''' </summary>
        Private Function UpdateProcessLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal matchLentList As EntryMatchLentTmpDataDetail, reserveFlg As Boolean) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	M_ASSET ")
            sb.AppendLine(" SET")
            sb.AppendLine("	 LEASE_CNT = LEASE_CNT + 1 ")
            sb.AppendLine($",ASSET_STATUS = {CommonConst.ASSET_STATUS_LEASE} ")
            'sb.AppendLine($" ,LEASE_PLACE_CD = '{matchLentList.leasePlaceCd}' ")
            'sb.AppendLine($" ,LEASE_PLACE_NAME = '{matchLentList.leasePlaceName}' ")
            'sb.AppendLine($" ,LEASE_EMPLOYEE_CD = '{matchLentList.leaseEmployeeCd}' ")
            'sb.AppendLine($" ,LEASE_EMPLOYEE_NAME = '{matchLentList.leaseEmployeeName}' ")
            sb.AppendLine($" ,LEASE_DT = '{matchLentList.leaseDt}' ")
            If String.IsNullOrEmpty(matchProcessTmpList.leaseDueDt) = False Then
                '値がある場合
                sb.AppendLine($" ,LEASE_DUE_DT = '{matchProcessTmpList.leaseDueDt}' ")
            Else
                '値がない場合
                sb.AppendLine($" ,LEASE_DUE_DT = NULL ")
            End If

            sb.AppendLine($" ,TASK_TS =  CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{CommonConst.TASK_LENT_REGIST}' ")
            sb.AppendLine("FROM")
            sb.AppendLine("	M_ASSET ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" ASSET_NO = '{matchLentList.assetNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約明細テーブルを更新する（処理区分：貸出）
        ''' </summary>
        ''' <param name="reserveId"></param>
        ''' <returns></returns>
        Private Function UpdateReservationDetailLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal matchLentList As EntryMatchLentTmpDataDetail, ByVal reserveId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine(" UPDATE ")
            sb.AppendLine($"  T_RESERVE_DETAIL ")
            sb.AppendLine(" SET ")
            sb.AppendLine($"   LENDING_DT =  '{matchProcessTmpList.processDt}'")
            sb.AppendLine($"  ,PR_ST_DT   =  '{matchProcessTmpList.processDt}'")
            sb.AppendLine($" ,RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine("  ,UPD_DATETIME = GETDATE() ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{CommonConst.TASK_LENT_REGIST}'")
            sb.AppendLine(" FROM")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} ")

            sb.AppendLine(" WHERE")
            sb.AppendLine($"  RESERVE_ID = '{reserveId}' ")
            sb.AppendLine($"  AND ASSET_NO = '{matchLentList.assetNo}' ")
            sb.AppendLine($"  AND REAL_START_DT = '{matchProcessTmpList.processDt}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約テーブルを更新する（処理区分：貸出）
        ''' </summary>
        ''' <param name="reserveId"></param>
        ''' <returns></returns>
        Private Function UpdateReservationLease(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal reserveId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("   T_RESERVE")
            sb.AppendLine("SET")
            sb.AppendLine($"  RESERVE_STATUS = CASE (SELECT COUNT(1) FROM {CommonConst.TABLE_RESERVE_DETAIL} AS DTL WHERE DTL.RESERVE_ID = '{reserveId}' AND RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_UNRENTED}') WHEN 0 THEN '{CommonConst.RESERVE_STATUS_RENTED}' ELSE '{CommonConst.RESERVE_STATUS_UNRENTED}' END ")
            sb.AppendLine("  ,UPD_DATETIME = GETDATE() ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{CommonConst.TASK_LENT_REGIST}'")
            sb.AppendLine(" FROM")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE} ")
            sb.AppendLine(" WHERE")
            sb.AppendLine($"  RESERVE_ID = '{reserveId}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

#End Region

#Region "返却で使用"
        ''' <summary>
        ''' 予約明細から工事番号を抽出する(処理区分：返却)
        ''' </summary>
        Private Function GetReserveDetail(ByVal item As EntryMatchProcessTmpDataDetail, ByVal retItem As EntryMatchReturnTmpDataDetail) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  RESERVE.REMARK               AS constNo ")
            sb.AppendLine(" FROM ")
            sb.AppendLine(" T_RESERVE AS RESERVE")
            sb.AppendLine(" INNER JOIN ")
            sb.AppendLine(" T_RESERVE_DETAIL AS RESERVED")
            sb.AppendLine(" ON ")
            sb.AppendLine(" RESERVE.RESERVE_ID = RESERVED.RESERVE_ID ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" RESERVED.ASSET_NO = '{retItem.assetNo}' ")
            sb.AppendLine($" AND RESERVED.RESERVE_STATUS = 1 ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事履歴を登録する(処理区分：返却)
        ''' </summary>
        Private Function InsertConstLogRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                        ByVal constWorkDiv As String,
                                        ByVal constMatchDiv As String,
                                        ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,CONST_WORK_DIV")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,LENT_NUM")
            sb.AppendLine(" ,LEASE_NUM")
            sb.AppendLine(" ,TEMP_REG_NUM")
            sb.AppendLine(" ,CONST_IN_MATCH_NUM")
            sb.AppendLine(" ,CONST_OUT_MATCH_NUM")
            sb.AppendLine(" ,CONST_IN_NUM")
            sb.AppendLine(" ,TEMP_NUM")
            sb.AppendLine(" ,CONST_OUT_NUM")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{constWorkDiv}'")
            sb.AppendLine($",'{constMatchDiv}'")

            sb.AppendLine($",'0'")
            sb.AppendLine($",'{0 + 1}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND CONST_ASSET_STATUS = '1' )")                                                      '持込数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                              '現在仮置数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NULL AND  CONST_ASSET_STATUS = '1')")                                 '持帰数

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事履歴を更新する(処理区分：返却)
        ''' </summary>
        Private Function UpdateConstLogRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                        ByVal constLog As EntryConstLogDetail,
                                        ByVal constWorkDiv As String,
                                        ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE T_CONST_LOG ")
            sb.AppendLine("SET ")
            sb.AppendLine($"   LEASE_NUM  = LEASE_NUM + 1")                                                                                                                                                 '返却数
            sb.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND CONST_ASSET_STATUS = '1' )")                                                      '持込数
            sb.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = '1')")                               '持帰数
            sb.AppendLine($"  ,TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                                 '現在仮置数
            sb.AppendLine($"  ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($"  ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" FROM ")
            sb.AppendLine($" T_CONST_LOG CON ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")
            sb.AppendLine($"AND CONST_WORK_TS = CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($"AND CONST_WORK_DIV = '{constWorkDiv}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産履歴を登録する（処理区分：返却）
        ''' </summary>
        Private Function InsertConstAssetLogRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchRetList As EntryMatchReturnTmpDataDetail,
                                             ByVal constMatchDiv As String,
                                             ByVal constMatchStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,CONST_MATCH_STATUS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{matchRetList.assetNo}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'{constMatchStatus}'")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事マスタを更新する（処理区分：返却）
        ''' </summary>
        Private Function UpdateConstRuctionRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	M_CONSTRUCTION")
            sb.AppendLine(" SET")
            sb.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")                                                        '前回仮置き
            sb.AppendLine($"  ,CONST_IN_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1')")                                                    '持込数
            sb.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND PREV_TEMP_FLAG = 'N' AND TEMP_ID IS NOT NULL )")   '今回仮置き数
            sb.AppendLine($"  ,CONST_OUT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NULL AND CONST_ASSET_STATUS = '1')")                               '持帰数
            sb.AppendLine($"  ,CUR_LENT_NUM  = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND CONST_ASSET_STATUS = '1' AND TEMP_ID IS NULL )")                               '現在貸出数
            sb.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                             '現在仮置数


            '前回仮置き数 + 持込数 = 今回仮置き数-持帰数の場合
            sb.AppendLine($" ,SITE_EMPLOYEE_CD = '{matchProcessTmpList.employeeName}' ")
            sb.AppendLine($" ,SITE_EMPLOYEE_NAME = '{matchProcessTmpList.employeeName}' ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" FROM ")
            sb.AppendLine($" M_CONSTRUCTION CON ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事マスタを完了する。
        ''' </summary>
        Private Function UpdateConstFinish(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	M_CONSTRUCTION")
            sb.AppendLine(" SET")
            sb.AppendLine($"  CONST_CONFIRM_TS = FORMAT(GETDATE(), 'yyyyMMddHHmmss') ")
            sb.AppendLine($" ,CONST_CONFIRM_EMPLOYEE_CD = '{matchProcessTmpList.employeeName}' ")
            sb.AppendLine($" ,CONST_CONFIRM_EMPLOYEE_NAME = '{matchProcessTmpList.employeeName}' ")
            sb.AppendLine($" ,CONST_STATUS = '1' ")                                                     '完了

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" FROM ")
            sb.AppendLine($" M_CONSTRUCTION CON ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")
            sb.AppendLine($" AND  CONST_OUT_NUM = 0 ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を更新する（処理区分：照合返却）
        ''' </summary>
        Private Function UpdateConstAssetRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchRetList As EntryMatchReturnTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_CONST_ASSET")
            sb.AppendLine(" SET")
            sb.AppendLine($"  CONST_ASSET_STATUS = '{constAssetStatus}' ")
            sb.AppendLine($"	 ,TEMP_ID = NULL ")
            sb.AppendLine($" ,TASK_TS =  CONVERT(BIGINT,LEFT({taskIs}, 14)) ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")
            sb.AppendLine($" AND ASSET_NO = '{matchRetList.assetNo}' ")
            sb.AppendLine($" AND TASK_TS <= CONVERT(BIGINT,LEFT({taskIs}, 14)) ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を登録する（処理区分：照合返却）
        ''' </summary>
        Private Function InsertConstAssetRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchRetList As EntryMatchReturnTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal prevTempFlag As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_ASSET_STATUS")
            sb.AppendLine(" ,TEMP_ID")
            sb.AppendLine(" ,PREV_TEMP_FLAG")
            sb.AppendLine(" ,TASK_TS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($",'{matchRetList.assetNo}'")
            sb.AppendLine($",'{constAssetStatus}'")
            sb.AppendLine($",NULL")
            sb.AppendLine($",'{prevTempFlag}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：返却）
        ''' </summary>
        Private Function UpdateProcessRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal matchRetList As EntryMatchReturnTmpDataDetail, Optional ByVal locUpdFlg As Boolean = False) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine(" Set")
            sb.AppendLine($" ASSET_STATUS = {CommonConst.ASSET_STATUS_INV} ")
            If matchRetList.inspeFlg = "9" Then
                sb.AppendLine($", LEASE_STATUS = 0 ")
            End If
            sb.AppendLine(" , LEASE_PLACE_CD = NULL")
            sb.AppendLine(" , LEASE_PLACE_NAME = NULL")
            sb.AppendLine(" , LEASE_EMPLOYEE_CD = NULL")
            sb.AppendLine(" , LEASE_EMPLOYEE_NAME = NULL")
            sb.AppendLine(" , LEASE_DT = NULL")
            sb.AppendLine(" , LEASE_DUE_DT = NULL")
            sb.AppendLine(" , REMARK = NULL")
            sb.AppendLine($" ,TASK_TS = CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine("	, UPD_DATETIME = GETDATE()")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{TASK_NAME}' ")

            'If locUpdFlg Then
            '    'ロケ大区分更新
            '    sb.AppendLine($",LOC_L_CD = CASE ISNULL('{matchProcessTmpList.mngLocLCd}', '') WHEN '' THEN ' ' ELSE '{matchProcessTmpList.mngLocLCd}' END ")
            '    sb.AppendLine("	,LOC_M_CD = CASE ISNULL(PROCESS_TEMP.LOC_M_CD, '') WHEN '' THEN ' ' END ")
            '    sb.AppendLine("	,LOC_S_CD = CASE ISNULL(PROCESS_TEMP.LOC_S_CD, '') WHEN '' THEN ' ' END ")
            'End If

            sb.AppendLine("FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("WHERE")
            sb.AppendLine($" ASSET_NO = '{matchRetList.assetNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 紐付情報をセットし、仮置きを完了状態へ更新する
        ''' </summary>
        Private Function UpdateTemporaryFinish(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_TEMPORARY ")
            sb.AppendLine(" SET")
            sb.AppendLine($"  REL_CONST_NO = ' ' ")
            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" FROM ")
            sb.AppendLine($" T_TEMPORARY TMP ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine(" REL_CONST_NO IS NULL ")
            sb.AppendLine(" AND NOT EXISTS( SELECT 1 FROM T_TEMP_ASSET AST INNER JOIN T_CONST_ASSET CON ON CON.CONST_NO = TMP.REG_CONST_NO AND TMP.TEMP_ID = AST.TEMP_ID AND CON.ASSET_NO = AST.ASSET_NO WHERE CON.CONST_ASSET_STATUS <> '3') ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約明細テーブルを更新する（処理区分：返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function UpdateReservationDetailRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal matchRetList As EntryMatchReturnTmpDataDetail) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL}")
            sb.AppendLine("SET")
            sb.AppendLine($"   RETURN_DT =  '{matchProcessTmpList.processDt}'")
            sb.AppendLine($"  ,PR_END_DT   =  '{matchProcessTmpList.processDt}'")
            sb.AppendLine($" ,RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RERUENED}' ")
            sb.AppendLine("  ,UPD_DATETIME = GETDATE() ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{CommonConst.TASK_RETURN_REGIST}'")
            sb.AppendLine(" OUTPUT inserted.RESERVE_ID ")

            sb.AppendLine(" FROM")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} ")

            sb.AppendLine(" WHERE")
            sb.AppendLine($"  RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine($"  AND ASSET_NO = '{matchRetList.assetNo}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約テーブルを更新する（処理区分：返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function UpdateReservationRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal reserveId As Long) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("   RESERVE")
            sb.AppendLine("SET")
            sb.AppendLine($"   RESERVE_STATUS =  '{CommonConst.RESERVE_STATUS_RERUENED}'")
            sb.AppendLine("  ,UPD_DATETIME = GETDATE() ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{CommonConst.TASK_RETURN_REGIST}'")

            sb.AppendLine($" FROM   {CommonConst.TABLE_RESERVE} RESERVE")

            sb.AppendLine($"	INNER JOIN {CommonConst.TABLE_RESERVE_DETAIL} DETAIL")
            sb.AppendLine("		ON")
            sb.AppendLine("			RESERVE.RESERVE_ID = DETAIL.RESERVE_ID")

            sb.AppendLine("	WHERE")
            sb.AppendLine($" RESERVE.RESERVE_ID = '{reserveId}'")
            sb.AppendLine($" AND RESERVE.RESERVE_STATUS =  '{CommonConst.RESERVE_STATUS_RENTED}'")


            logger.Info(vbCrLf & sb.ToString)


            Return sb.ToString
        End Function

        ''' <summary>
        ''' 更新する予約IDを取得する（処理区分：返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function GetReserveIdReturn(AseetNo As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("   DETAIL.RESERVE_ID     AS reserveId ")
            sb.AppendLine("  ,DETAIL.ASSET_NO       AS assetNo ")
            sb.AppendLine("  ,DETAIL.REAL_START_DT  AS realStartDt ")
            sb.AppendLine("  ,CONVERT(VARCHAR, MAINTE.MAINTE_ID)      AS mainteId ")
            sb.AppendLine("  ,CONVERT(VARCHAR, ISNULL(RANK() OVER (PARTITION BY MAINTE.MAINTE_ID ORDER BY DETAIL.REAL_END_DT DESC),0)) AS mainteOrder ")
            sb.AppendLine(" FROM ")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} AS DETAIL ")
            sb.AppendLine($"    LEFT OUTER JOIN {CommonConst.TABLE_RESERVE_MAINTE} AS MAINTE ")
            sb.AppendLine("        ON  MAINTE.RESERVE_ID = DETAIL.RESERVE_ID ")
            sb.AppendLine("        AND MAINTE.ASSET_NO   = DETAIL.ASSET_NO ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($"     DETAIL.ASSET_NO       = '{AseetNo}' ")
            sb.AppendLine($" AND DETAIL.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine(" ORDER BY ")
            sb.AppendLine("   DETAIL.REAL_START_DT ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約機能の有無を取得する
        ''' </summary>
        ''' <returns></returns>
        Private Function GetReservationFunction() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine($"  ISNULL(MT_1.DATA1, '{CommonConst.RESERVATION_FUNCTION_NO}')  AS reservationFunction")
            sb.AppendLine($", ISNULL(MT_1.DATA2, '{CommonConst.RESERVATION_FUNCTION_NO}')  AS reservationDataDiv")
            sb.AppendLine($", ISNULL(MT_2.DATA1, '{CommonConst.DEFAULT_LEAD_TIME}')        AS leadTime")
            sb.AppendLine($", ISNULL(MT_2.DATA2, '{CommonConst.RETURN_ACTUAL_DAYS_DIV}')   AS dateDiv")
            sb.AppendLine("FROM")
            sb.AppendLine($" {CommonConst.MASTER_SETTING} MT_1")
            sb.AppendLine($" LEFT OUTER JOIN {CommonConst.MASTER_SETTING} MT_2")
            sb.AppendLine($"   ON MT_2.KEY_DIV = '{CommonConst.LENDING_LT_RESERVE_DATA}'")
            sb.AppendLine("WHERE")
            sb.AppendLine($"  MT_1.KEY_DIV = '{CommonConst.RESERVATION_FUNCTION}'")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 次の最小予約日を取得する（処理区分：返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function GetNextReserveInfoReturn(logId As Long, ByVal mainteId As String, dateDiv As String) As String
            Dim sb As StringBuilder = New StringBuilder()
            sb.AppendLine($" WITH [V_NEXT] AS ( ")
            sb.AppendLine($" SELECT MIN(DETAIL_NEXT.REAL_START_DT) AS START_DT ")
            sb.AppendLine($" FROM   {CommonConst.TABLE_RESERVE_MAINTE} MAINT ")
            sb.AppendLine($"   INNER JOIN {CommonConst.TABLE_RESERVE_DETAIL} DETAIL_MY ")
            sb.AppendLine($"     ON  DETAIL_MY.RESERVE_ID = MAINT.RESERVE_ID ")
            sb.AppendLine($"     AND DETAIL_MY.ASSET_NO   = MAINT.ASSET_NO ")
            sb.AppendLine($"   INNER JOIN {CommonConst.TABLE_RESERVE_DETAIL} DETAIL_NEXT ")
            sb.AppendLine($"     ON  DETAIL_NEXT.ASSET_NO      = MAINT.ASSET_NO ")
            sb.AppendLine($"     AND DETAIL_NEXT.REAL_START_DT > DETAIL_MY.REAL_START_DT ")
            sb.AppendLine($" WHERE  MAINT.MAINTE_ID = '{mainteId}' ")
            sb.AppendLine($"   AND  DETAIL_NEXT.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_UNRENTED}' ")
            sb.AppendLine($" ) ")
            sb.AppendLine($" , [V_MAINT] AS ( ")
            sb.AppendLine($" SELECT MAINT.MAINTE_ID ")
            sb.AppendLine($"      , CASE WHEN DETAIL.PR_END_DT < DETAIL.REAL_END_DT ")
            sb.AppendLine($"             THEN dbo.TARGET_DATE_SF(MAINT.MAINTE_START_DT, ISNULL(dbo.RESERVE_DATE_COUNT_SF(DETAIL.PR_END_DT, DETAIL.REAL_END_DT, '{dateDiv}') - 1,0) * -1, '{dateDiv}') ")
            sb.AppendLine($"             ELSE dbo.TARGET_DATE_SF(MAINT.MAINTE_START_DT, ISNULL(dbo.RESERVE_DATE_COUNT_SF(DETAIL.REAL_END_DT, DETAIL.PR_END_DT, '{dateDiv}') - 1,0), '{dateDiv}') ")
            sb.AppendLine($"             END AS START_DT ")
            sb.AppendLine($"      , CASE WHEN DETAIL.PR_END_DT < DETAIL.REAL_END_DT ")
            sb.AppendLine($"             THEN dbo.TARGET_DATE_SF(MAINT.MAINTE_END_DT, ISNULL(dbo.RESERVE_DATE_COUNT_SF(DETAIL.PR_END_DT, DETAIL.REAL_END_DT, '{dateDiv}') - 1,0) * -1, '{dateDiv}') ")
            sb.AppendLine($"             ELSE dbo.TARGET_DATE_SF(MAINT.MAINTE_END_DT, ISNULL(dbo.RESERVE_DATE_COUNT_SF(DETAIL.REAL_END_DT, DETAIL.PR_END_DT, '{dateDiv}') - 1,0), '{dateDiv}') ")
            sb.AppendLine($"             END AS END_DT ")
            sb.AppendLine($" FROM   {CommonConst.TABLE_RESERVE_MAINTE} MAINT ")
            sb.AppendLine($"   INNER JOIN {CommonConst.TABLE_RESERVE_DETAIL} DETAIL ")
            sb.AppendLine($"     ON  DETAIL.RESERVE_ID = MAINT.RESERVE_ID ")
            sb.AppendLine($"     AND DETAIL.ASSET_NO   = MAINT.ASSET_NO ")
            sb.AppendLine($" WHERE  MAINT.MAINTE_ID = '{mainteId}' ")
            sb.AppendLine($" ) ")
            sb.AppendLine($" SELECT V1.MAINTE_ID AS mainteId")
            sb.AppendLine($"      , CASE WHEN V2.START_DT IS NULL THEN 9 ")
            sb.AppendLine($"             ELSE dbo.RESERVE_DATE_COUNT_SF(V1.START_DT, V2.START_DT, '{dateDiv}') - 1 ")
            sb.AppendLine($"          END AS mainteCnt ")
            sb.AppendLine($"      , V1.START_DT AS startDt ")
            sb.AppendLine($"      , CASE WHEN V2.START_DT IS NULL THEN V1.END_DT ")
            sb.AppendLine($"             WHEN V1.END_DT < V2.START_DT THEN V1.END_DT ")
            sb.AppendLine($"             WHEN V1.END_DT = V2.START_DT AND V1.START_DT < V1.END_DT THEN dbo.TARGET_DATE_SF(V1.END_DT, -1, '{dateDiv}') ")
            sb.AppendLine($"             ELSE dbo.TARGET_DATE_SF(V2.START_DT, -1, '{dateDiv}') ")
            sb.AppendLine($"         END AS endDt ")
            sb.AppendLine($"      , V2.START_DT AS NEXT_DT ")
            sb.AppendLine($" FROM   [V_MAINT] V1 ")
            sb.AppendLine($"   LEFT OUTER JOIN [V_NEXT] V2 ")
            sb.AppendLine($"     ON  1=1 ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約メンテテーブルを更新する（処理区分：返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function DeleteReturnMeinte(ByVal nextDt As NextMenteUpdData) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine(" DELETE ")
            sb.AppendLine($" FROM   {CommonConst.TABLE_RESERVE_MAINTE} ")
            sb.AppendLine($" WHERE MAINTE_ID = '{nextDt.mainteId}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

#End Region

#Region "照合返却で使用"
        ''' <summary>
        ''' 工事履歴を登録する（処理区分：照合返却）
        ''' </summary>
        Private Function InsertConstLogMatRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                        ByVal constLog As EntryConstLogDetail,
                                        ByVal RetCnt As Long,
                                        ByVal MatchCnt As Long,
                                        ByVal constWorkDiv As String,
                                        ByVal constMatchDiv As String,
                                        ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,CONST_WORK_DIV")
            sb.AppendLine(" ,CONST_MATCH_DIV")

            sb.AppendLine(" ,LENT_NUM")
            sb.AppendLine(" ,LEASE_NUM")
            sb.AppendLine(" ,TEMP_REG_NUM")

            sb.AppendLine(" ,CONST_MATCH_TTLNUM")
            sb.AppendLine(" ,CONST_IN_MATCH_NUM")
            sb.AppendLine(" ,CONST_OUT_MATCH_NUM")
            sb.AppendLine(" ,CONST_IN_NUM")
            sb.AppendLine(" ,TEMP_NUM")
            sb.AppendLine(" ,CONST_OUT_NUM")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{constWorkDiv}'")
            sb.AppendLine($",'{constMatchDiv}'")

            sb.AppendLine($",'0'")
            sb.AppendLine($",'{0 + RetCnt}'")
            sb.AppendLine($",'0'")

            sb.AppendLine($",'{0 + MatchCnt}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'{0 + RetCnt}'")

            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND CONST_ASSET_STATUS = '1' )")                                                      '持込数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                              '現在仮置数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NULL AND  CONST_ASSET_STATUS = '1')")                                 '持帰数

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産履歴を登録する（処理区分：照合返却）
        ''' </summary>
        Private Function InsertConstAssetLogMatRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchRetList As EntryMatchReturnTmpDataDetail,
                                             ByVal constMatchDiv As String,
                                             ByVal constMatchStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,CONST_MATCH_STATUS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{matchRetList.assetNo}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'{constMatchStatus}'")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事マスタを更新する（処理区分：照合返却）
        ''' </summary>
        Private Function UpdateConstRuctionMatRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	M_CONSTRUCTION")
            sb.AppendLine(" SET")
            sb.AppendLine($"  CONST_OUT_NUM = CONST_IN_NUM + 1 ")
            sb.AppendLine($" ,CUR_LENT_NUM = CUR_LENT_NUM - 1 ")

            '前回仮置き数 + 持込数 = 今回仮置き数-持帰数の場合
            sb.AppendLine($" ,SITE_EMPLOYEE_CD = CASE WHEN PREV_TEMP_NUM + CONST_IN_NUM = NOW_TEMP_NUM - CONST_OUT_NUM THEN ISNULL('{matchProcessTmpList.employeeCd}', ' ') ELSE SITE_EMPLOYEE_CD END ")
            sb.AppendLine($" ,SITE_EMPLOYEE_NAME = CASE WHEN PREV_TEMP_NUM + CONST_IN_NUM = NOW_TEMP_NUM - CONST_OUT_NUM THEN ISNULL('{matchProcessTmpList.employeeName}', ' ') ELSE SITE_EMPLOYEE_NAME END ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を更新する（処理区分：照合返却）
        ''' </summary>
        Private Function UpdateConstAssetMatRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchRetList As EntryMatchReturnTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_CONST_ASSET")
            sb.AppendLine(" SET")
            sb.AppendLine($"  CONST_ASSET_STATUS = '{constAssetStatus}' ")
            sb.AppendLine($" ,TASK_TS =  CONVERT(BIGINT,LEFT({taskIs}, 14)) ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")
            sb.AppendLine($" AND ASSET_NO = '{matchRetList.assetNo}' ")
            sb.AppendLine($" AND TASK_TS <= CONVERT(BIGINT,LEFT({taskIs}, 14)) ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を登録する（処理区分：照合返却）
        ''' </summary>
        Private Function InsertConstAssetMatRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchRetList As EntryMatchReturnTmpDataDetail,
                                             ByVal constAssetStatus As String,
                                             ByVal prevTempFlag As String,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_ASSET_STATUS")
            sb.AppendLine(" ,TEMP_ID")
            sb.AppendLine(" ,PREV_TEMP_FLAG")
            sb.AppendLine(" ,TASK_TS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($",'{matchRetList.assetNo}'")
            sb.AppendLine($",'{constAssetStatus}'")
            sb.AppendLine($",NULL")
            sb.AppendLine($",'{prevTempFlag}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：照合返却）
        ''' </summary>
        Private Function UpdateProcessMatRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal matchRetList As EntryMatchReturnTmpDataDetail, Optional ByVal locUpdFlg As Boolean = False) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine(" Set")
            sb.AppendLine($" ASSET_STATUS = {CommonConst.ASSET_STATUS_INV} ")
            sb.AppendLine(" , LEASE_PLACE_CD = NULL")
            sb.AppendLine(" , LEASE_PLACE_NAME = NULL")
            sb.AppendLine(" , LEASE_EMPLOYEE_CD = NULL")
            sb.AppendLine(" , LEASE_EMPLOYEE_NAME = NULL")
            sb.AppendLine(" , LEASE_DT = NULL")
            sb.AppendLine(" , LEASE_DUE_DT = NULL")
            sb.AppendLine(" , REMARK = NULL")
            sb.AppendLine($" ,TASK_TS = CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine("	, UPD_DATETIME = GETDATE()")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{CommonConst.TASK_MATCH_RETURN_REGIST}' ")

            'If locUpdFlg Then
            '    'ロケ大区分更新
            '    sb.AppendLine($",LOC_L_CD = CASE ISNULL('{matchProcessTmpList.mngLocLCd}', '') WHEN '' THEN ' ' ELSE '{matchProcessTmpList.mngLocLCd}' END ")
            '    sb.AppendLine("	,LOC_M_CD = CASE ISNULL(PROCESS_TEMP.LOC_M_CD, '') WHEN '' THEN ' ' END ")
            '    sb.AppendLine("	,LOC_S_CD = CASE ISNULL(PROCESS_TEMP.LOC_S_CD, '') WHEN '' THEN ' ' END ")
            'End If

            sb.AppendLine("FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("WHERE")
            sb.AppendLine($" ASSET_NO = '{matchRetList.assetNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約明細テーブルを更新する（処理区分：照合返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function UpdateReservationDetailMatRet(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, ByVal matchRetList As EntryMatchReturnTmpDataDetail) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL}")
            sb.AppendLine("SET")
            sb.AppendLine($"   RETURN_DT =  '{matchProcessTmpList.processDt}'")
            sb.AppendLine($"  ,PR_END_DT   =  '{matchProcessTmpList.processDt}'")
            sb.AppendLine($" ,RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RERUENED}' ")
            sb.AppendLine("  ,UPD_DATETIME = GETDATE() ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($"  ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{CommonConst.TASK_MATCH_RETURN_REGIST}'")
            sb.AppendLine(" OUTPUT inserted.RESERVE_ID ")

            sb.AppendLine(" FROM")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} ")

            sb.AppendLine(" WHERE")
            sb.AppendLine($"  RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine($"  AND ASSET_NO = '{matchRetList.assetNo}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約テーブルを更新する（処理区分：照合返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function UpdateReservationMatRet(ByVal employeeCd As String, ByVal employeeName As String, ByVal reserveId As Long) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("   RESERVE")
            sb.AppendLine("SET")
            sb.AppendLine($"  RESERVE.RESERVE_STATUS = CASE ")
            sb.AppendLine($"     WHEN STS_ALL = STS_02 THEN '{CommonConst.RESERVE_STATUS_RERUENED}' ")
            sb.AppendLine($"     WHEN STS_01 > 0 THEN '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine($"     ELSE'{CommonConst.RESERVE_STATUS_UNRENTED}' END ")
            sb.AppendLine("  ,RESERVE.UPD_DATETIME = GETDATE() ")
            sb.AppendLine($"  ,RESERVE.UPD_EMPLOYEE_CD = ISNULL('{employeeCd}', ' ') ")
            sb.AppendLine($"  ,RESERVE.UPD_EMPLOYEE_NAME= ISNULL('{employeeName}', ' ') ")
            sb.AppendLine($" ,RESERVE.UPD_TASK = '{TASK_NAME}'")

            sb.AppendLine($" FROM   {CommonConst.TABLE_RESERVE} RESERVE ")
            sb.AppendLine("  INNER JOIN ")
            sb.AppendLine("     (SELECT DTL.RESERVE_ID")
            sb.AppendLine($"          , COUNT(DTL.RESERVE_ID) AS STS_ALL")
            sb.AppendLine($"          , SUM(CASE WHEN DTL.RESERVE_STATUS =  {CommonConst.RESERVE_STATUS_UNRENTED} THEN 1 ELSE 0 END) AS STS_00")
            sb.AppendLine($"          , SUM(CASE WHEN DTL.RESERVE_STATUS =  {CommonConst.RESERVE_STATUS_RENTED}   THEN 1 ELSE 0 END) AS STS_01")
            sb.AppendLine($"          , SUM(CASE WHEN DTL.RESERVE_STATUS =  {CommonConst.RESERVE_STATUS_RERUENED} THEN 1 ELSE 0 END) AS STS_02")
            sb.AppendLine($"     FROM {CommonConst.TABLE_RESERVE_DETAIL} DTL")
            sb.AppendLine($"     WHERE DTL.RESERVE_ID = {reserveId}")
            sb.AppendLine("     GROUP BY DTL.RESERVE_ID) WK")
            sb.AppendLine("  ON  RESERVE.RESERVE_ID = WK.RESERVE_ID")

            logger.Info(vbCrLf & sb.ToString)


            Return sb.ToString
        End Function

#End Region

#Region "持込照合/持帰照合で使用"

        ''' <summary>
        ''' 工事履歴を登録する（処理区分：持込照合）
        ''' </summary>
        Private Function InsertConstLogMatIn(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                        ByVal constLog As EntryConstLogDetail,
                                        ByVal MatCnt As Long,
                                        ByVal RetCnt As Long,
                                        ByVal constWorkDiv As String,
                                        ByVal constMatchDiv As String,
                                        ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,CONST_WORK_DIV")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,LENT_NUM")
            sb.AppendLine(" ,LEASE_NUM")

            sb.AppendLine(" ,CONST_MATCH_TTLNUM")
            sb.AppendLine(" ,TEMP_REG_NUM")
            sb.AppendLine(" ,CONST_IN_MATCH_NUM")
            sb.AppendLine(" ,CONST_OUT_MATCH_NUM")
            sb.AppendLine(" ,CONST_IN_NUM")
            sb.AppendLine(" ,TEMP_NUM")
            sb.AppendLine(" ,CONST_OUT_NUM")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{constWorkDiv}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")


            sb.AppendLine($",'{0 + RetCnt}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'{0 + MatCnt}'")
            sb.AppendLine($",'0'")

            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND CONST_ASSET_STATUS = '1' )")                                                      '持込数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                              '現在仮置数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NULL AND  CONST_ASSET_STATUS = '1')")                                 '持帰数

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事履歴を登録する（処理区分：持帰照合）
        ''' </summary>
        Private Function InsertConstLogMatOut(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                        ByVal constLog As EntryConstLogDetail,
                                        ByVal MatCnt As Long,
                                        ByVal RetCnt As Long,
                                        ByVal constWorkDiv As String,
                                        ByVal constMatchDiv As String,
                                        ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,CONST_WORK_DIV")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,LENT_NUM")
            sb.AppendLine(" ,LEASE_NUM")

            sb.AppendLine(" ,CONST_MATCH_TTLNUM")
            sb.AppendLine(" ,TEMP_REG_NUM")
            sb.AppendLine(" ,CONST_IN_MATCH_NUM")
            sb.AppendLine(" ,CONST_OUT_MATCH_NUM")
            sb.AppendLine(" ,CONST_IN_NUM")
            sb.AppendLine(" ,TEMP_NUM")
            sb.AppendLine(" ,CONST_OUT_NUM")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{constWorkDiv}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")

            sb.AppendLine($",'{0 + RetCnt}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'{0 + MatCnt}'")

            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND CONST_ASSET_STATUS = '1' )")                                                      '持込数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                              '現在仮置数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NULL AND  CONST_ASSET_STATUS = '1')")                                 '持帰数

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産履歴を登録する（処理区分：持帰照合）
        ''' </summary>
        Private Function InsertConstAssetLogMat(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal matchList As EntryMatchAssetTmpDataDetail,
                                             ByVal constAssetLog As EntryConstAssetLogDetail,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            If constAssetLog IsNot Nothing Then
                sb.AppendLine(" ,CONST_MATCH_STATUS")
                sb.AppendLine(" ,TEMP_FLAG")
                sb.AppendLine(" ,TEMP_ID")
            End If

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($", CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{matchList.assetNo}'")
            sb.AppendLine($",'{matchList.contMatchDiv}'")
            If constAssetLog IsNot Nothing Then
                sb.AppendLine($",'{constAssetLog.constMatchStatus}'")
                sb.AppendLine($",'{constAssetLog.tempFlag}'")
                If constAssetLog.tempId Is Nothing Then
                    sb.AppendLine($",NULL")
                Else
                    sb.AppendLine($",'{constAssetLog.tempId}'")
                End If
            End If

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
#End Region

#Region "仮置きで使用"

        ''' <summary>
        ''' 工事資産情報を取得する
        ''' </summary>
        Private Function GetTempAssetCNT(ByVal tempId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT ")
            sb.AppendLine("  COUNT( * ) ")

            sb.AppendLine(" FROM ")
            sb.AppendLine(" T_TEMP_ASSET ")

            sb.AppendLine(" WHERE ")
            sb.AppendLine($" TEMP_ID = '{tempId}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産情報を更新する（処理区分：仮置き）
        ''' </summary>
        Private Function UpdateConstAssetTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_CONST_ASSET")
            sb.AppendLine(" SET")
            sb.AppendLine($"  TEMP_ID = NULL ")

            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置き資産を削除する（処理区分：仮置き）
        ''' </summary>
        Private Function DeleteTempAssetTemp(ByVal tempList As EntryTempTmpDataDetail) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine(" DELETE ")
            sb.AppendLine($" FROM   {CommonConst.TABLE_TEMP_ASSET} ")
            sb.AppendLine($" WHERE TEMP_ID = '{tempList.tempId}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        '''' <summary>
        '''' 工事マスタを更新する（処理区分：仮置き）
        '''' </summary>
        'Private Function UpdateConstRuctionTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
        '                                     ByVal updTask As String) As String
        '    Dim sb As StringBuilder = New StringBuilder()

        '    sb.AppendLine("UPDATE")
        '    sb.AppendLine("	M_CONSTRUCTION ")
        '    sb.AppendLine(" SET")
        '    sb.AppendLine($"   PREV_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'Y')")
        '    sb.AppendLine($"  ,NOW_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND PREV_TEMP_FLAG = 'N')")
        '    sb.AppendLine($"  ,CUR_TEMP_NUM = (SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CON.CONST_NO = AST.CONST_NO AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")

        '    '前回仮置き数 + 持込数 = 今回仮置き数-持帰数の場合
        '    sb.AppendLine($" ,SITE_EMPLOYEE_CD = CASE WHEN PREV_TEMP_NUM + CONST_IN_NUM = NOW_TEMP_NUM - CONST_OUT_NUM THEN ISNULL('{matchProcessTmpList.employeeCd}', ' ') ELSE SITE_EMPLOYEE_CD END ")
        '    sb.AppendLine($" ,SITE_EMPLOYEE_NAME = CASE WHEN PREV_TEMP_NUM + CONST_IN_NUM = NOW_TEMP_NUM - CONST_OUT_NUM THEN ISNULL('{matchProcessTmpList.employeeName}', ' ') ELSE SITE_EMPLOYEE_NAME END ")

        '    sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
        '    sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
        '    sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
        '    sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
        '    sb.AppendLine(" FROM ")
        '    sb.AppendLine("	M_CONSTRUCTION CON")
        '    sb.AppendLine(" WHERE ")
        '    sb.AppendLine($" CONST_NO = '{matchProcessTmpList.constNo}' ")


        '    logger.Info(vbCrLf & sb.ToString)

        '    Return sb.ToString
        'End Function

        ''' <summary>
        ''' 工事履歴を登録する（処理区分：仮置き）
        ''' </summary>
        Private Function InsertConstLogTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                        ByVal constLog As EntryConstLogDetail,
                                        ByVal MatCnt As Long,
                                        ByVal cntBeforeTepmData As Integer,
                                        ByVal constWorkDiv As String,
                                        ByVal constMatchDiv As String,
                                        ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,CONST_WORK_DIV")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,LENT_NUM")
            sb.AppendLine(" ,LEASE_NUM")
            sb.AppendLine(" ,TEMP_REG_NUM")
            sb.AppendLine(" ,CONST_IN_MATCH_NUM")
            sb.AppendLine(" ,CONST_OUT_MATCH_NUM")
            sb.AppendLine(" ,CONST_IN_NUM")
            sb.AppendLine(" ,TEMP_NUM")
            sb.AppendLine(" ,CONST_OUT_NUM")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.constNo}'")
            sb.AppendLine($",  CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{constWorkDiv}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",ISNULL('{MatCnt - cntBeforeTepmData} ','0')")
            sb.AppendLine($",'0'")
            sb.AppendLine($",'0'")
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND CONST_ASSET_STATUS = '1' )")                                                      '持込数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NOT NULL AND CONST_ASSET_STATUS = '1')")                              '現在仮置数
            sb.AppendLine($",(SELECT COUNT(1) FROM T_CONST_ASSET AST WHERE CONST_NO = '{matchProcessTmpList.constNo}' AND TEMP_ID IS NULL AND  CONST_ASSET_STATUS = '1')")                                 '持帰数

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置きを更新する（処理区分：仮置き）
        ''' </summary>
        Private Function UpdateTemporaryTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal tempList As EntryTempTmpDataDetail,
                                             ByVal updTask As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_TEMPORARY")
            sb.AppendLine(" SET")
            sb.AppendLine($" TEMP_DT = '{tempList.tempDt}' ")
            sb.AppendLine($" ,WORK_EMPLOYEE_CD = '{matchProcessTmpList.employeeCd}' ")
            sb.AppendLine($" ,WORK_EMPLOYEE_NAME = '{matchProcessTmpList.employeeName}' ")
            sb.AppendLine($" ,TEMP_INFO1 = '{tempList.tempInfo1}' ")
            sb.AppendLine($" ,TEMP_INFO2 = '{tempList.tempInfo2}' ")
            sb.AppendLine($" ,TEMP_INFO3 = '{tempList.tempInfo3}' ")
            sb.AppendLine($" ,TEMP_INFO4 = '{tempList.tempInfo4}' ")
            sb.AppendLine($" ,TEMP_INFO5 = '{tempList.tempInfo5}' ")
            sb.AppendLine($" ,TEMP_INFO6 = '{tempList.tempInfo6}' ")
            sb.AppendLine($" ,TEMP_INFO7 = '{tempList.tempInfo7}' ")
            sb.AppendLine($" ,TEMP_REMARK = '{tempList.remark}' ")

            sb.AppendLine($" ,TASK_TS =  CONVERT(BIGINT,LEFT({taskIs}, 14)) ")
            sb.AppendLine($" ,UPD_DATETIME = GetDate() ")
            sb.AppendLine($" ,UPD_EMPLOYEE_CD = ISNULL('{matchProcessTmpList.employeeCd}', ' ') ")
            sb.AppendLine($" ,UPD_EMPLOYEE_NAME = ISNULL('{matchProcessTmpList.employeeName}', ' ') ")
            sb.AppendLine($" ,UPD_TASK = '{updTask}' ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($" TEMP_ID = '{tempList.tempId}' ")
            sb.AppendLine($" AND TASK_TS <= CONVERT(BIGINT,LEFT({taskIs}, 14)) ")


            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function


        ''' <summary>
        ''' 仮置きを登録する（処理区分：仮置き）
        ''' </summary>
        Private Function InsertTemporaryTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal tempList As EntryTempTmpDataDetail,
                                             ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_TEMPORARY")
            sb.AppendLine(" (")
            sb.AppendLine("  TEMP_DT")
            sb.AppendLine(" ,REG_CONST_NO")
            sb.AppendLine(" ,WORK_EMPLOYEE_CD")
            sb.AppendLine(" ,WORK_EMPLOYEE_NAME")
            sb.AppendLine(" ,TEMP_INFO1")
            sb.AppendLine(" ,TEMP_INFO2")
            sb.AppendLine(" ,TEMP_INFO3")
            sb.AppendLine(" ,TEMP_INFO4")
            sb.AppendLine(" ,TEMP_INFO5")
            sb.AppendLine(" ,TEMP_INFO6")
            sb.AppendLine(" ,TEMP_INFO7")
            sb.AppendLine(" ,TEMP_REMARK")

            sb.AppendLine(" ,TASK_TS")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($" '{matchProcessTmpList.processDt}'")
            sb.AppendLine($",'{matchProcessTmpList.constNo}'")
            sb.AppendLine($",'{matchProcessTmpList.employeeCd}'")
            sb.AppendLine($",'{matchProcessTmpList.employeeName}'")
            sb.AppendLine($",'{tempList.tempInfo1}'")
            sb.AppendLine($",'{tempList.tempInfo2}'")
            sb.AppendLine($",'{tempList.tempInfo3}'")
            sb.AppendLine($",'{tempList.tempInfo4}'")
            sb.AppendLine($",'{tempList.tempInfo5}'")
            sb.AppendLine($",'{tempList.tempInfo6}'")
            sb.AppendLine($",'{tempList.tempInfo7}'")
            sb.AppendLine($",'{tempList.remark}'")
            sb.AppendLine($",CONVERT(BIGINT,LEFT({taskIs}, 14))")


            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 仮置きIDの連番取得する
        ''' </summary>
        Private Function GetTempIdIdentity() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT SCOPE_IDENTITY() AS TEMP_ID")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function


        ''' <summary>
        ''' 仮置き資産を登録する（処理区分：仮置き）
        ''' </summary>
        Private Function InsertTempAssetTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal tempList As EntryTempTmpDataDetail,
                                             ByVal tempAssetList As EntryTempAssetTmpDataDetail,
                                             ByVal tempId As Long,
                                             ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("INSERT INTO T_TEMP_ASSET")
            sb.AppendLine(" (")
            sb.AppendLine("  TEMP_ID")
            sb.AppendLine(" ,ASSET_NO")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($"'{tempId}'")
            sb.AppendLine($",'{tempAssetList.assetNo}'")

            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 工事資産履歴を登録する（処理区分：仮置き）
        ''' </summary>
        Private Function InsertConstAssetLogTemp(ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail,
                                             ByVal tempList As EntryTempTmpDataDetail,
                                             ByVal tempAssetList As EntryTempAssetTmpDataDetail,
                                             ByVal tempId As Long,
                                             ByVal constMatchDiv As String,
                                             ByVal assetStatus As String,
                                             ByVal tempFlag As String,
                                             ByVal updTask As String
                                        ) As String
            Dim sb As StringBuilder = New StringBuilder()

            Dim taskIs As String = matchProcessTmpList.processDt + matchProcessTmpList.processTm

            sb.AppendLine("INSERT INTO T_CONST_ASSET_LOG")
            sb.AppendLine(" (")
            sb.AppendLine("  CONST_NO")
            sb.AppendLine(" ,CONST_WORK_TS")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,CONST_MATCH_DIV")
            sb.AppendLine(" ,CONST_MATCH_STATUS")
            sb.AppendLine(" ,TEMP_FLAG")
            sb.AppendLine(" ,TEMP_ID")

            sb.AppendLine(" ,INS_DATETIME")
            sb.AppendLine(" ,INS_EMPLOYEE_CD")
            sb.AppendLine(" ,INS_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")

            sb.AppendLine(") VALUES (")

            sb.AppendLine($"'{tempAssetList.constNo}'")
            sb.AppendLine($",CONVERT(BIGINT,LEFT({taskIs}, 14))")
            sb.AppendLine($",'{tempAssetList.assetNo}'")
            sb.AppendLine($",'{constMatchDiv}'")
            sb.AppendLine($",'{assetStatus}'")
            sb.AppendLine($",'{tempFlag}'")
            sb.AppendLine($",'{tempId}'")


            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeCd}', ' ')")
            sb.AppendLine($",ISNULL('{matchProcessTmpList.employeeName}', ' ')")
            sb.AppendLine($",'{updTask}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

#End Region

        ''' <summary>
        ''' 予約メンテテーブルを更新する（処理区分：返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function UpdateReturnMeinte(logId As Long, ByVal nextDt As NextMenteUpdData) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine(" UPDATE ")
            sb.AppendLine("   MAINTE ")
            sb.AppendLine(" SET ")
            sb.AppendLine($"  MAINTE.MAINTE_START_DT   = {nextDt.startDt} ")
            sb.AppendLine($" ,MAINTE.MAINTE_END_DT     = {nextDt.endDt} ")
            sb.AppendLine("  ,MAINTE.UPD_DATETIME      = GETDATE() ")
            sb.AppendLine("  ,MAINTE.UPD_EMPLOYEE_CD   = ISNULL(TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("  ,MAINTE.UPD_EMPLOYEE_NAME = ISNULL(TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($" ,MAINTE.UPD_TASK = '{TASK_NAME}' ")

            sb.AppendLine($" FROM   {CommonConst.TABLE_RESERVE_MAINTE} MAINTE ")
            sb.AppendLine($"    INNER JOIN #MATCH_PROCESS_TEMP AS TEMP ")
            sb.AppendLine($"        ON  TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine($" WHERE MAINTE.MAINTE_ID = '{nextDt.mainteId}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function



        ''' <summary>
        ''' 資産マスタから実棚した件数を反映する
        ''' </summary>
        Private Function UpdateInventoryNum(ByVal emp As M_EMPLOYEE, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	T_INV_DETAIL")
            sb.AppendLine("SET")
            sb.AppendLine($" LOGIC_INV_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS <> '{CommonConst.PHYINV_STATUS_EXCLUDED}' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD) ")
            sb.AppendLine($",PHYINV_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_NORMAL}' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD)")
            sb.AppendLine($",LOC_DIFF_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_WARN}' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD)")
            sb.AppendLine($",NO_ITEM_NUM = (SELECT COUNT(1) FROM M_ASSET WHERE PHYINV_STATUS = '{CommonConst.PHYINV_STATUS_ERROR }' AND M_ASSET.LOC_L_CD = T_INV_DETAIL.INV_LOC_L_CD)")
            sb.AppendLine("	,UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,UPD_EMPLOYEE_CD = @UpdEmployeeCd ")
            sb.AppendLine("	,UPD_EMPLOYEE_NAME = @UpdEmployeeName ")
            sb.AppendLine($",UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine("FROM")
            sb.AppendLine("	T_INV_DETAIL INNER JOIN T_INVENTORY ON T_INV_DETAIL.INV_ID = T_INVENTORY.INV_ID ")
            sb.AppendLine("WHERE")
            sb.AppendLine("	INV_CONFIRM_TS IS NULL")

            'パラメータ設定
            If emp IsNot Nothing Then
                param.Add(New SqlParameter("@UpdEmployeeCd", emp.EMPLOYEE_CD))
                param.Add(New SqlParameter("@UpdEmployeeName", emp.EMPLOYEE_NAME))
            Else
                param.Add(New SqlParameter("@UpdEmployeeCd", " "))
                param.Add(New SqlParameter("@UpdEmployeeName", " "))
            End If

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function


        '2020/02/13 戸祭 v1.2 貸出上限機能対応 START
        ''' <summary>
        ''' 貸出上限情報を取得する
        ''' </summary>
        ''' <returns></returns>
        Private Function GetLeaseUpperInfo(ByVal assetNo As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 CASE WHEN CS.CAT_DIV IS NOT NULL THEN CASE WHEN CS.LEASE_ALERT_DIV = 'Y' THEN CS.LEASE_UPPER_ERR_DIV ELSE NULL END ELSE CASE WHEN CM.CAT_DIV IS NOT NULL THEN CASE WHEN CM.LEASE_ALERT_DIV = 'Y' THEN CM.LEASE_UPPER_ERR_DIV ELSE NULL END  ELSE CL.LEASE_UPPER_ERR_DIV END END AS leastUpperErrDiv")
            sb.AppendLine("	,CASE WHEN CS.CAT_DIV IS NOT NULL THEN CASE WHEN CS.LEASE_ALERT_DIV = 'Y' THEN CS.LEASE_UPPER_LIMIT ELSE NULL END ELSE CASE WHEN CM.CAT_DIV IS NOT NULL THEN CASE WHEN CM.LEASE_ALERT_DIV = 'Y' THEN CM.LEASE_UPPER_LIMIT ELSE NULL END  ELSE CL.LEASE_UPPER_LIMIT END END AS leaseUpperLimit")
            sb.AppendLine("	,LEASE_CNT AS leaseCnt")
            sb.AppendLine("FROM")
            sb.AppendLine($" {CommonConst.MASTER_ASSET} AS ASSET")
            sb.AppendLine($"LEFT JOIN {CommonConst.MASTER_CATEGORY} AS CL")
            sb.AppendLine("	ON")
            sb.AppendLine($"	    CL.CAT_DIV = '{CommonConst.CAT_L_DIV }'")
            sb.AppendLine("	    AND CL.CAT_L_CD = ASSET.CAT_L_CD")
            sb.AppendLine($"    AND CL.LEASE_ALERT_DIV = '{CommonConst.LEASE_ALERT_DIV_Y}'")    'アラート区分=Y
            sb.AppendLine("	LEFT JOIN M_CATEGORY AS CM")
            sb.AppendLine("	ON")
            sb.AppendLine($"	    CM.CAT_DIV = '{CommonConst.CAT_M_DIV }'")
            sb.AppendLine("	    AND CM.CAT_L_CD = ASSET.CAT_L_CD")
            sb.AppendLine("	    AND CM.CAT_M_CD = ASSET.CAT_M_CD")
            sb.AppendLine($"    AND CM.LEASE_ALERT_DIV <> '{CommonConst.LEASE_ALERT_DIV_F}'")   'アラート区分<>F
            sb.AppendLine("	LEFT JOIN M_CATEGORY AS CS")
            sb.AppendLine("	ON")
            sb.AppendLine($"	    CS.CAT_DIV = '{CommonConst.CAT_S_DIV }'")
            sb.AppendLine("	    AND CS.CAT_L_CD = ASSET.CAT_L_CD")
            sb.AppendLine("	    AND CS.CAT_M_CD = ASSET.CAT_M_CD")
            sb.AppendLine("	    AND CS.CAT_S_CD = ASSET.CAT_S_CD")
            sb.AppendLine($"	AND CS.LEASE_ALERT_DIV <> '{CommonConst.LEASE_ALERT_DIV_F}'")   'アラート区分<>F
            sb.AppendLine(" WHERE")
            sb.AppendLine($"	        ASSET_NO = '{assetNo}'")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
        '2020/02/13 戸祭 v1.2 貸出上限機能対応 END


        ''' <summary>
        ''' 指定品番予約可能数のストアドドファンクション呼出
        ''' </summary>
        ''' <returns></returns>
        Private Function GetModelReservePossibleSf(db As SystemDBContext, productNo As String, fromDt As String, toDt As String, operationKbn As String, constId As String) As Integer
            Dim targetDate As Integer? = Nothing

            Dim sb As StringBuilder = New StringBuilder()

            '貸出返却予定日がNULLの場合
            If String.IsNullOrEmpty(toDt.Trim) Then
                '処理日を入れる
                toDt = fromDt
            End If

            '実稼働日がYの場合
            If operationKbn = CommonConst.WORK_FLAG.YES Then
                '稼働日
                operationKbn = CommonConst.DATA_DIV_K
            Else
                '実日
                operationKbn = CommonConst.DATA_DIV_J
            End If

            sb.AppendLine("SELECT")
            sb.AppendLine($"  dbo.ASSET_MODEL_RESERVE_POSSIBLE_SF('{productNo}', {fromDt}, {toDt},'{operationKbn}',NULL)")

            logger.Info(vbCrLf & sb.ToString)

            Dim rsvDt As Integer?() = db.Database.SqlQuery(Of Integer?)(sb.ToString).ToArray()
            targetDate = rsvDt(0)

            'nullが返却された場合はシステム日付を返す
            If targetDate Is Nothing Then
                targetDate = Date.Today.ToShortDateString.Replace("/", "")
            End If

            Return targetDate

        End Function

        ''' <summary>
        ''' 代替品番予約更新ストアドファンクション呼出
        ''' </summary>
        ''' <returns></returns>
        Private Function GetReplaceReservePossibleSf(db As SystemDBContext, ByVal matchProcessTmpList As EntryMatchProcessTmpDataDetail, productNo As String, assetNo As String, fromDt As String, toDt As String, operationKbn As String, constId As String, reservId As String) As Integer

            Dim sb As StringBuilder = New StringBuilder()

            '貸出返却予定日がNULLの場合
            If String.IsNullOrEmpty(toDt.Trim) Then
                '処理日を入れる
                toDt = fromDt
            End If

            '実稼働日がYの場合
            If operationKbn = CommonConst.WORK_FLAG.YES Then
                '稼働日
                operationKbn = CommonConst.DATA_DIV_K
            Else
                '実日
                operationKbn = CommonConst.DATA_DIV_J
            End If

            Dim params As List(Of SqlParameter)
            params = New List(Of SqlParameter)
            Dim result As Integer = 0

            sb.AppendLine("DECLARE @nRet INT;")
            sb.AppendLine("EXEC @nRet =dbo.[ASSET_REPLACE_RESERVE_POSSIBLE_SP]")
            sb.AppendLine("            @IN_PRODUCT_NO")
            sb.AppendLine("            ,@ASSET_NO")
            sb.AppendLine("            ,@IN_FROM_DT")
            sb.AppendLine("            ,@IN_TO_DT")
            sb.AppendLine("            ,@IN_KADO_JITU")
            sb.AppendLine("            ,@IN_CONST_NO")
            sb.AppendLine("            ,@IN_CONST_RESERVE_ID")
            sb.AppendLine("            ,@IN_REG_EMP_CD")
            sb.AppendLine("            ,@IN_REG_EMP_NAME")
            sb.AppendLine("            ,@IN_REG_TASK")
            sb.AppendLine(";")
            sb.AppendLine("SELECT @RETURN_VALUE = @nRet")

            params.Add(New SqlParameter($"@IN_PRODUCT_NO", If(productNo Is Nothing, " ", productNo)))
            params.Add(New SqlParameter($"@ASSET_NO", assetNo))
            params.Add(New SqlParameter($"@IN_FROM_DT", fromDt))
            params.Add(New SqlParameter($"@IN_TO_DT", toDt))
            params.Add(New SqlParameter($"@IN_KADO_JITU", operationKbn))
            params.Add(New SqlParameter($"@IN_CONST_NO", constId))
            params.Add(New SqlParameter($"@IN_CONST_RESERVE_ID", reservId))
            params.Add(New SqlParameter($"@IN_REG_EMP_CD", matchProcessTmpList.employeeCd))
            params.Add(New SqlParameter($"@IN_REG_EMP_NAME", If(matchProcessTmpList.employeeName Is Nothing, "''", matchProcessTmpList.employeeName)))
            params.Add(New SqlParameter($"@IN_REG_TASK", CommonConst.TASK_LENT_REGIST))

            Dim returnValueParam = New SqlParameter()
            returnValueParam.ParameterName = $"@RETURN_VALUE"
            returnValueParam.SqlDbType = SqlDbType.Int
            returnValueParam.Direction = ParameterDirection.Output
            params.Add(returnValueParam)

            logger.Info(vbCrLf & sb.ToString)
            logger.Info(vbCrLf & params.ToString)

            'SQL実行
            db.Database.ExecuteSqlCommand(sb.ToString(), params.ToArray)

            '戻り値
            result = params.Where(Function(x) x.ParameterName = "@RETURN_VALUE").FirstOrDefault.Value

            Return result

        End Function


        ''' <summary>
        ''' 拠点ロケ更新有無チェック
        ''' </summary>
        ''' <param name="prmDb"></param>
        ''' <returns></returns>
        Private Function GetLocationUpdInfo(ByRef prmDb As SystemDBContext) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine($"  ISNULL(DATA1, '{CommonConst.RETURN_LOC_UPDATE_NO}')  AS locUpdDiv")
            sb.AppendLine("FROM")
            sb.AppendLine($" {CommonConst.MASTER_SETTING}")
            sb.AppendLine("WHERE")
            sb.AppendLine($"  KEY_DIV = '{CommonConst.RETURN_UPDATE_LOCATION}'")

            Dim locUpdDivList As String() = CType(prmDb.Database.SqlQuery(Of String)(sb.ToString).ToArray(), String())
            Dim locUpdDiv As String = String.Empty

            If IsNothing(locUpdDivList) OrElse locUpdDivList.Length <= 0 Then
                'レコードが存在しない場合は拠点ロケ更新なし
                locUpdDiv = CommonConst.RETURN_LOC_UPDATE_NO
            Else
                locUpdDiv = locUpdDivList(0)
            End If

            Return locUpdDiv
        End Function

        ''' <summary>
        ''' 資産処理履歴を登録する(貸出)
        ''' </summary>
        Private Function InsertProcessLog(ByVal base As EntryMatchProcessTmpDataDetail, ByVal detail As EntryMatchLentTmpDataDetail, ByVal taskResult As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("INSERT INTO T_PROCESS_LOG")
            sb.AppendLine("(")
            sb.AppendLine("  PROCESS_DT")
            sb.AppendLine(" ,PROCESS_TM")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,ASSET_ID")
            sb.AppendLine(" ,PROCESS_TOOL_DIV")
            sb.AppendLine(" ,PROCESS_DIV")
            sb.AppendLine(" ,LEASE_PLACE_CD")
            sb.AppendLine(" ,LEASE_PLACE_NAME")
            sb.AppendLine(" ,REMARK")
            sb.AppendLine(" ,LEASE_EMPLOYEE_CD")
            sb.AppendLine(" ,LEASE_EMPLOYEE_NAME")
            sb.AppendLine(" ,LEASE_DUE_DT")
            sb.AppendLine(" ,MNG_LOC_L_CD")
            'sb.AppendLine(" ,LOC_L_CD")
            'sb.AppendLine(" ,LOC_M_CD")
            'sb.AppendLine(" ,LOC_S_CD")
            'sb.AppendLine(" ,LOC_L_NAME")
            'sb.AppendLine(" ,LOC_M_NAME")
            'sb.AppendLine(" ,LOC_S_NAME")
            sb.AppendLine(" ,ASSET_NAME")
            sb.AppendLine(" ,MAKER")
            sb.AppendLine(" ,PRODUCT_NO")
            sb.AppendLine(" ,MODEL_NO")
            sb.AppendLine(" ,TASK_RESULT")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")
            sb.AppendLine(")")
            sb.AppendLine("VALUES (")
            sb.AppendLine($"  {base.processDt } ")
            sb.AppendLine($" ,{base.processTm } ")
            sb.AppendLine($" ,'{detail.assetNo }' ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,'H' ")
            sb.AppendLine($" ,'{base.processDiv }' ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,'[{base.constNo}]{detail.remark}'")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,NULL ")
            If Not String.IsNullOrEmpty(base.constLentEndDt) Then
                sb.AppendLine($" ,CASE ISNULL('{base.constLentEndDt}', '') WHEN '' THEN NULL ELSE '{base.constLentEndDt}' END ")
            Else
                sb.AppendLine($" ,CASE ISNULL('{base.leaseDueDt}', '') WHEN '' THEN NULL ELSE '{base.leaseDueDt}' END ")
            End If
            sb.AppendLine($" ,CASE ISNULL('{detail.assetMngLocLCd}', '') WHEN '' THEN ' ' ELSE '{detail.assetMngLocLCd}' END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocLCd}', '') WHEN '' THEN ' ' ELSE {detail.assetLocLCd} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocMCd}', '') WHEN '' THEN ' ' ELSE {detail.assetLocMCd} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocSCd}', '') WHEN '' THEN ' ' ELSE {detail.assetLocSCd} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocLName}', '') WHEN '' THEN ' ' ELSE {detail.assetLocLName} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocMName}', '') WHEN '' THEN ' ' ELSE {detail.assetLocMName} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocSName}', '') WHEN '' THEN ' ' ELSE {detail.assetLocSName} END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.assetName}', '') WHEN '' THEN ' ' ELSE '{detail.assetName}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.maker}', '') WHEN '' THEN ' ' ELSE '{detail.maker}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.productNo }', '') WHEN '' THEN ' ' ELSE '{detail.productNo}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.modelNo }', '') WHEN '' THEN ' ' ELSE '{detail.modelNo}' END ")
            sb.AppendLine($" ,'{taskResult}'")
            sb.AppendLine($" ,GETDATE()")
            sb.AppendLine($" ,ISNULL('{base.employeeCd }', ' ') ")
            sb.AppendLine($" ,ISNULL('{base.employeeName }', ' ') ")
            sb.AppendLine($" ,'{TASK_NAME}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産処理履歴を登録する(返却)
        ''' </summary>
        Private Function InsertProcessLog(ByVal base As EntryMatchProcessTmpDataDetail, ByVal detail As EntryMatchReturnTmpDataDetail, ByVal taskResult As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("INSERT INTO T_PROCESS_LOG")
            sb.AppendLine("(")
            sb.AppendLine("  PROCESS_DT")
            sb.AppendLine(" ,PROCESS_TM")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,ASSET_ID")
            sb.AppendLine(" ,PROCESS_TOOL_DIV")
            sb.AppendLine(" ,PROCESS_DIV")
            sb.AppendLine(" ,LEASE_PLACE_CD")
            sb.AppendLine(" ,LEASE_PLACE_NAME")
            sb.AppendLine(" ,REMARK")
            sb.AppendLine(" ,LEASE_EMPLOYEE_CD")
            sb.AppendLine(" ,LEASE_EMPLOYEE_NAME")
            sb.AppendLine(" ,LEASE_DUE_DT")
            sb.AppendLine(" ,MNG_LOC_L_CD")
            'sb.AppendLine(" ,LOC_L_CD")
            'sb.AppendLine(" ,LOC_M_CD")
            'sb.AppendLine(" ,LOC_S_CD")
            'sb.AppendLine(" ,LOC_L_NAME")
            'sb.AppendLine(" ,LOC_M_NAME")
            'sb.AppendLine(" ,LOC_S_NAME")
            sb.AppendLine(" ,ASSET_NAME")
            sb.AppendLine(" ,MAKER")
            sb.AppendLine(" ,PRODUCT_NO")
            sb.AppendLine(" ,MODEL_NO")
            sb.AppendLine(" ,TASK_RESULT")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")
            sb.AppendLine(")")
            sb.AppendLine("VALUES (")
            sb.AppendLine($"  {base.processDt } ")
            sb.AppendLine($" ,{base.processTm } ")
            sb.AppendLine($" ,'{detail.assetNo }' ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,'H' ")
            sb.AppendLine($" ,'BI' ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,NULL ")
            If String.IsNullOrWhiteSpace(base.constNo) Then
                sb.AppendLine($" ,NULL ")
            Else
                sb.AppendLine($" ,'[{base.constNo}]'")
            End If
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,CASE ISNULL('{base.constLentEndDt}', '') WHEN '' THEN NULL ELSE '{base.constLentEndDt}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.assetMngLocLCd}', '') WHEN '' THEN ' ' ELSE '{detail.assetMngLocLCd}' END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocLCd}', '') WHEN '' THEN ' ' ELSE {detail.assetLocLCd} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocMCd}', '') WHEN '' THEN ' ' ELSE {detail.assetLocMCd} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocSCd}', '') WHEN '' THEN ' ' ELSE {detail.assetLocSCd} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocLName}', '') WHEN '' THEN ' ' ELSE {detail.assetLocLName} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocMName}', '') WHEN '' THEN ' ' ELSE {detail.assetLocMName} END ")
            'sb.AppendLine($" ,CASE ISNULL('{detail.assetLocSName}', '') WHEN '' THEN ' ' ELSE {detail.assetLocSName} END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.assetName}', '') WHEN '' THEN ' ' ELSE '{detail.assetName}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.maker}', '') WHEN '' THEN ' ' ELSE '{detail.maker}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.productNo }', '') WHEN '' THEN ' ' ELSE '{detail.productNo}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.modelNo }', '') WHEN '' THEN ' ' ELSE '{detail.modelNo}' END ")
            sb.AppendLine($" ,'{taskResult}'")
            sb.AppendLine($" ,GETDATE()")
            sb.AppendLine($" ,ISNULL('{base.employeeCd }', ' ') ")
            sb.AppendLine($" ,ISNULL('{base.employeeName }', ' ') ")
            sb.AppendLine($" ,'{TASK_NAME}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産処理履歴を登録する(返却時の異常登録)
        ''' </summary>
        Private Function InsertInspectionProcessLog(ByVal base As EntryMatchProcessTmpDataDetail, ByVal detail As EntryMatchReturnTmpDataDetail, ByVal taskResult As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("INSERT INTO T_PROCESS_LOG")
            sb.AppendLine("(")
            sb.AppendLine("  PROCESS_DT")
            sb.AppendLine(" ,PROCESS_TM")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,ASSET_ID")
            sb.AppendLine(" ,PROCESS_TOOL_DIV")
            sb.AppendLine(" ,PROCESS_DIV")
            sb.AppendLine(" ,LEASE_PLACE_CD")
            sb.AppendLine(" ,LEASE_PLACE_NAME")
            sb.AppendLine(" ,REMARK")
            sb.AppendLine(" ,LEASE_EMPLOYEE_CD")
            sb.AppendLine(" ,LEASE_EMPLOYEE_NAME")
            sb.AppendLine(" ,LEASE_DUE_DT")
            sb.AppendLine(" ,MNG_LOC_L_CD")
            sb.AppendLine(" ,ASSET_NAME")
            sb.AppendLine(" ,MAKER")
            sb.AppendLine(" ,PRODUCT_NO")
            sb.AppendLine(" ,MODEL_NO")
            sb.AppendLine(" ,TASK_RESULT")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")
            sb.AppendLine(")")
            sb.AppendLine("VALUES (")
            sb.AppendLine($"  {base.processDt } ")
            sb.AppendLine($" ,{base.processTm } ")
            sb.AppendLine($" ,'{detail.assetNo }' ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,'H' ")
            sb.AppendLine($" ,'MU' ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,(SELECT WORDING FROM M_WORD ")
            sb.AppendLine("    WHERE")
            sb.AppendLine($"     LANG_DIV = '{CommonConst.LANG_DIV_LU}'")
            sb.AppendLine($" AND TOOL_DIV = 'H'")
            sb.AppendLine($" AND WORD_DIV = '{CommonConst.WORD_DIV_M}'")
            sb.AppendLine($" AND WORD_CD =  '990066'")
            sb.AppendLine($" )")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,NULL ")
            sb.AppendLine($" ,CASE ISNULL('{base.constLentEndDt}', '') WHEN '' THEN NULL ELSE '{base.constLentEndDt}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.assetMngLocLCd}', '') WHEN '' THEN ' ' ELSE '{detail.assetMngLocLCd}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.assetName}', '') WHEN '' THEN ' ' ELSE '{detail.assetName}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.maker}', '') WHEN '' THEN ' ' ELSE '{detail.maker}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.productNo }', '') WHEN '' THEN ' ' ELSE '{detail.productNo}' END ")
            sb.AppendLine($" ,CASE ISNULL('{detail.modelNo }', '') WHEN '' THEN ' ' ELSE '{detail.modelNo}' END ")
            sb.AppendLine($" ,'{taskResult}'")
            sb.AppendLine($" ,GETDATE()")
            sb.AppendLine($" ,ISNULL('{base.employeeCd }', ' ') ")
            sb.AppendLine($" ,ISNULL('{base.employeeName }', ' ') ")
            sb.AppendLine($" ,'{TASK_NAME}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function


#End Region

    End Class

    ''' <summary>
    ''' 照合処理データ一時表情報
    ''' </summary>
    Public Class EntryMatchProcessTmpDataDetail
        ''' <summary>
        ''' 処理履歴ID
        ''' </summary>
        Property processLogId As Long
        ''' <summary>
        ''' 処理日
        ''' </summary>
        Property processDt As String
        ''' <summary>
        ''' 処理時刻
        ''' </summary>
        Property processTm As String
        ''' <summary>
        ''' ログイン担当者コード
        ''' </summary>
        Property employeeCd As String
        ''' <summary>
        ''' ログイン担当者名
        ''' </summary>
        Property employeeName As String
        ''' <summary>
        ''' 処理区分
        ''' </summary>
        Property processDiv As String
        ''' <summary>
        ''' 照合結果
        ''' </summary>
        Property contMatchDiv As String
        ''' <summary>
        ''' 照合数
        ''' </summary>
        Property contMatchNum As String
        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String
        ''' <summary>
        ''' 工事開始日
        ''' </summary>
        Property constStartDt As String
        ''' <summary>
        ''' 工事終了日
        ''' </summary>
        Property constEndDt As String

        ''' <summary>
        ''' 貸出先担当者コード
        ''' </summary>
        Property leaseEmployeeCd As String

        ''' <summary>
        ''' 貸出返却予定日
        ''' </summary>
        Property leaseDueDt As String

        ''' <summary>
        ''' 工事貸出開始日
        ''' </summary>
        Property constLentStartDt As String
        ''' <summary>
        ''' 工事貸出終了日
        ''' </summary>
        Property constLentEndDt As String
        ''' <summary>
        ''' 工事責任者コード
        ''' </summary>
        Property constEmployeeCd As String
        ''' <summary>
        ''' 管理ロケ大区分コード
        ''' </summary>
        Property mngLocLCd As String
        ''' <summary>
        ''' 予約
        ''' </summary>
        Property reserveId As Long?

        ''' <summary>
        ''' 貸出リスト件数
        ''' </summary>
        Property matchLentList As EntryMatchLentTmpDataDetail()

        ''' <summary>
        ''' 返却リスト件数
        ''' </summary>
        Property matchRetList As EntryMatchReturnTmpDataDetail()

        ''' <summary>
        ''' 照合資産リスト件数
        ''' </summary>
        Property matchAssetList As EntryMatchAssetTmpDataDetail()

        ''' <summary>
        ''' 仮置きリスト件数
        ''' </summary>
        Property matchTempList As EntryTempTmpDataDetail()


    End Class

    ''' <summary>
    ''' 照合貸出データ一時表情報
    ''' </summary>
    Public Class EntryMatchLentTmpDataDetail
        ''' <summary>
        ''' 処理履歴ID
        ''' </summary>
        Property processLogId As Long

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        Property assetNo As String
        ''' <summary>
        ''' 資産状態
        ''' </summary>
        Property assetStatus As Byte?
        ''' <summary>
        ''' 資産名称
        ''' </summary>
        Property assetName As String
        ''' <summary>
        ''' メーカー
        ''' </summary>
        Property maker As String
        ''' <summary>
        ''' 型番
        ''' </summary>
        Property modelNo As String
        ''' <summary>
        ''' 品番
        ''' </summary>
        Property productNo As String
        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        Property leaseStatus As Byte?
        ''' <summary>
        ''' 処理日時
        ''' </summary>
        Property taskTs As Long?
        ''' <summary>
        ''' 資産マスタ管理ロケコード
        ''' </summary>
        Property assetMngLocLCd As String
        ''' <summary>
        ''' 大ロケコード
        ''' </summary>
        Property assetLocLCd As String
        ''' <summary>
        ''' 中ロケコード
        ''' </summary>
        Property assetLocMCd As String
        ''' <summary>
        ''' 小ロケコード
        ''' </summary>
        Property assetLocSCd As String
        ''' <summary>
        ''' 大ロケ名
        ''' </summary>
        Property assetLocLName As String
        ''' <summary>
        ''' 中ロケ名
        ''' </summary>
        Property assetLocMName As String
        ''' <summary>
        ''' 小ロケ名
        ''' </summary>
        Property assetLocSName As String
        ''' <summary>
        ''' 払出日
        ''' </summary>
        Property leaseDt As String
        ''' <summary>
        ''' メンテ日数稼働フラグ
        ''' </summary>
        Property mainteDayWorkFlag As String
        ''' <summary>
        ''' 備考
        ''' </summary>
        Property remark As String

    End Class

    ''' <summary>
    ''' 照合返却データ一時表情報
    ''' </summary>
    Public Class EntryMatchReturnTmpDataDetail
        ''' <summary>
        ''' 処理履歴ID
        ''' </summary>
        Property processLogId As Long

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        Property assetNo As String
        ''' <summary>
        ''' 資産状態
        ''' </summary>
        Property assetStatus As Byte?
        ''' <summary>
        ''' 資産名称
        ''' </summary>
        Property assetName As String
        ''' <summary>
        ''' メーカー
        ''' </summary>
        Property maker As String
        ''' <summary>
        ''' 型番
        ''' </summary>
        Property modelNo As String
        ''' <summary>
        ''' 品番
        ''' </summary>
        Property productNo As String
        ''' <summary>
        ''' 点検可否フラグ
        ''' </summary>
        Property inspeFlg As String

        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        Property leaseStatus As Byte?
        ''' <summary>
        ''' 処理日時
        ''' </summary>
        Property taskTs As Long?
        ''' <summary>
        ''' 資産マスタ管理ロケコード
        ''' </summary>
        Property assetMngLocLCd As String
        ''' <summary>
        ''' 照合結果
        ''' </summary>
        Property contMatchDiv As String

    End Class

    ''' <summary>
    ''' 照合資産データ一時表情報
    ''' </summary>
    Public Class EntryMatchAssetTmpDataDetail
        ''' <summary>
        ''' 処理履歴ID
        ''' </summary>
        Property processLogId As Long

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        Property assetNo As String
        ''' <summary>
        ''' 資産状態
        ''' </summary>
        Property assetStatus As Byte?
        ''' <summary>
        ''' 資産名称
        ''' </summary>
        Property assetName As String
        ''' <summary>
        ''' メーカー
        ''' </summary>
        Property maker As String
        ''' <summary>
        ''' 型番
        ''' </summary>
        Property modelNo As String
        ''' <summary>
        ''' 品番
        ''' </summary>
        Property productNo As String
        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        Property leaseStatus As Byte?
        ''' <summary>
        ''' 処理日時
        ''' </summary>
        Property taskTs As Long?
        ''' <summary>
        ''' 資産マスタ管理ロケコード
        ''' </summary>
        Property assetMngLocLCd As String
        ''' <summary>
        ''' 照合結果
        ''' </summary>
        Property contMatchDiv As String

    End Class

    ''' <summary>
    ''' 仮置きデータ一時表情報
    ''' </summary>
    Public Class EntryTempTmpDataDetail
        ''' <summary>
        ''' 処理履歴ID
        ''' </summary>
        Property processLogId As Long

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 仮置きID
        ''' </summary>
        Property tempId As String

        ''' <summary>
        ''' 以前仮置きID
        ''' </summary>
        Property beforeTempId As String

        ''' <summary>
        ''' 仮置き日
        ''' </summary>
        Property tempDt As String
        '''' <summary>
        '''' 作業責任者コード
        '''' </summary>
        'Property tempWorkEmployeeCd As String
        '''' <summary>
        '''' 作業責任者氏名
        '''' </summary>
        'Property tempWorkEmployeeName As String
        ''' <summary>
        ''' 仮置き情報1
        ''' </summary>
        Property tempInfo1 As String
        ''' <summary>
        ''' 仮置き情報2
        ''' </summary>
        Property tempInfo2 As String
        ''' <summary>
        ''' 仮置き情報3
        ''' </summary>
        Property tempInfo3 As String
        ''' <summary>
        ''' 仮置き情報4
        ''' </summary>
        Property tempInfo4 As String
        ''' <summary>
        ''' 仮置き情報5
        ''' </summary>
        Property tempInfo5 As String
        ''' <summary>
        ''' 仮置き情報6
        ''' </summary>
        Property tempInfo6 As String
        ''' <summary>
        ''' 仮置き情報7
        ''' </summary>
        Property tempInfo7 As String
        ''' <summary>
        ''' 備考
        ''' </summary>
        Property remark As String
        ''' <summary>
        ''' 仮置き資産リスト
        ''' </summary>
        Property matchTempAssetList As EntryTempAssetTmpDataDetail()

    End Class

    ''' <summary>
    ''' 仮置き資産データ一時表情報
    ''' </summary>
    Public Class EntryTempAssetTmpDataDetail
        ''' <summary>
        ''' 処理履歴ID
        ''' </summary>
        Property processLogId As Long

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 仮置きID
        ''' </summary>
        Property tempId As String

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        Property assetNo As String
        ''' <summary>
        ''' 資産状態
        ''' </summary>
        Property assetStatus As Byte?
        ''' <summary>
        ''' 資産名称
        ''' </summary>
        Property assetName As String
        ''' <summary>
        ''' メーカー
        ''' </summary>
        Property maker As String
        ''' <summary>
        ''' 型番
        ''' </summary>
        Property modelNo As String
        ''' <summary>
        ''' 品番
        ''' </summary>
        Property productNo As String
        ''' <summary>
        ''' 貸出状態
        ''' </summary>
        Property leaseStatus As Byte?
        ''' <summary>
        ''' 処理日時
        ''' </summary>
        Property taskTs As Long?
        ''' <summary>
        ''' 資産マスタ管理ロケコード
        ''' </summary>
        Property assetMngLocLCd As String
    End Class

    ''' <summary>
    ''' 工事履歴情報
    ''' </summary>
    Public Class EntryConstLogDetail
        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 貸出数
        ''' </summary>
        Property lentNum As Integer

        ''' <summary>
        ''' 返却数
        ''' </summary>
        Property leaseNum As Integer

        ''' <summary>
        ''' 仮置登録数
        ''' </summary>
        Property tempRegNum As Integer

        ''' <summary>
        ''' 持込照合数
        ''' </summary>
        Property constInMatchNum As Integer

        ''' <summary>
        ''' 持帰照合数
        ''' </summary>
        Property constOutMatchNum As Integer

        ''' <summary>
        ''' 持込数
        ''' </summary>
        Property constInNum As Integer

        ''' <summary>
        ''' 仮置数
        ''' </summary>
        Property tempNum As Integer

        ''' <summary>
        ''' 持帰数
        ''' </summary>
        Property constOutNum As Integer

    End Class

    ''' <summary>
    ''' 工事資産履歴情報
    ''' </summary>
    Public Class EntryConstAssetLogDetail
        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 資産番号
        ''' </summary>
        Property assetNo As String

        ''' <summary>
        ''' 資産状態
        ''' </summary>
        Property constMatchStatus As Byte

        ''' <summary>
        ''' 仮置きフラグ
        ''' </summary>
        Property tempFlag As String

        ''' <summary>
        ''' 仮置きID
        ''' </summary>
        Property tempId As Long?
    End Class

    ''' <summary>
    ''' ストアド用予約情報
    ''' </summary>
    Public Class EntryReservDetail
        ''' <summary>
        ''' 品番
        ''' </summary>
        Property productNo As String

        ''' <summary>
        ''' 貸出返却予定日
        ''' </summary>
        Property assetLeaseDueDt As String

        ''' <summary>
        ''' メンテ日数稼働フラグ
        ''' </summary>
        Property mainteDayWorkFlag As String

        ''' <summary>
        ''' 処理日
        ''' </summary>
        Property processDt As String

        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

        ''' <summary>
        ''' 件数
        ''' </summary>
        Property recordCount As Integer

    End Class

    ''' <summary>
    ''' 予約明細.工事番号
    ''' </summary>
    Public Class EntryReserveDetail
        ''' <summary>
        ''' 工事番号
        ''' </summary>
        Property constNo As String

    End Class

    ''' <summary>
    ''' 更新用予約データ
    ''' </summary>
    Public Class MatReservationData
        ''' <summary>
        ''' 予約ID
        ''' </summary>
        ''' <returns></returns>
        Property reserveId As Long
        ''' <summary>
        ''' 備考
        ''' </summary>
        ''' <returns></returns>
        Property remark As String
        ''' <summary>
        ''' 資材番号
        ''' </summary>
        ''' <returns></returns>
        Property assetNo As String
        ''' <summary>
        ''' 予約開始日
        ''' </summary>
        ''' <returns></returns>
        Property realStartDt As Integer
        ''' <summary>
        ''' 貸出先場所コード
        ''' </summary>
        ''' <returns></returns>
        Property leasePlaceCd As String
        ''' <summary>
        ''' 貸出先場所コード名
        ''' </summary>
        ''' <returns></returns>
        Property leasePlaceNm As String
        ''' <summary>
        '''　貸出先担当者社員コード
        ''' </summary>
        ''' <returns></returns>
        Property changeEmpCd As String
        ''' <summary>
        '''　貸出先担当者社員コード名
        ''' </summary>
        ''' <returns></returns>
        Property changeEmpNm As String
        ''' <summary>
        ''' 貸出返却予定日
        ''' </summary>
        ''' <returns></returns>
        Property realEndDt As Integer

        ''' <summary>
        '''　メンテナンスID
        ''' </summary>
        ''' <returns></returns>
        Property mainteId As String
        ''' <summary>
        '''　メンテナンスIDの処理順（1のみ処理）
        ''' </summary>
        ''' <returns></returns>
        Property mainteOrder As String
    End Class

    '''' <summary>
    '''' 予約機能情報
    '''' </summary>
    'Public Class MatchReservationFunctionInfo
    '    ''' <summary>
    '    ''' 予約機能有無
    '    ''' </summary>
    '    ''' <returns></returns>
    '    Private _reservationFunction As String
    '    Property reservationFunction() As String
    '        Get
    '            Return _reservationFunction
    '        End Get
    '        Set(ByVal value As String)
    '            If String.IsNullOrEmpty(value) Then
    '                _reservationFunction = CommonConst.RESERVATION_FUNCTION_NO
    '            Else
    '                _reservationFunction = value
    '            End If
    '        End Set
    '    End Property
    '    ''' <summary>
    '    ''' 予約機能の稼働日有無
    '    ''' </summary>
    '    ''' <returns></returns>
    '    Private _reservationDataDiv As String
    '    Property reservationDataDiv() As String
    '        Get
    '            Return _reservationDataDiv
    '        End Get
    '        Set(ByVal value As String)
    '            If String.IsNullOrEmpty(value) Then
    '                _reservationDataDiv = CommonConst.DATA_DIV_J
    '            Else
    '                If value.Equals(CommonConst.RESERVATION_FUNCTION_YES) Then
    '                    _reservationDataDiv = CommonConst.DATA_DIV_K
    '                Else
    '                    _reservationDataDiv = CommonConst.DATA_DIV_J
    '                End If
    '            End If
    '        End Set
    '    End Property


    '    ''' <summary>
    '    '''　返却日予定日から拠点へ返却するまでのリードタイム日数
    '    ''' </summary>
    '    Private _leadTime As String
    '    Property leadTime() As String
    '        Get
    '            Return _leadTime
    '        End Get
    '        Set(ByVal value As String)
    '            If String.IsNullOrEmpty(value) Then
    '                _leadTime = CommonConst.DEFAULT_LEAD_TIME
    '            Else
    '                _leadTime = value
    '            End If
    '        End Set
    '    End Property

    '    ''' <summary>
    '    ''' 稼働実数か実日数か
    '    ''' </summary>
    '    Private _dateDiv As String
    '    Property dateDiv() As String
    '        Get
    '            Return _dateDiv
    '        End Get
    '        Set(ByVal value As String)
    '            If String.IsNullOrEmpty(value) Then
    '                _dateDiv = CommonConst.RETURN_ACTUAL_DAYS_DIV
    '            Else
    '                _dateDiv = value
    '            End If
    '        End Set
    '    End Property
    'End Class

End Namespace