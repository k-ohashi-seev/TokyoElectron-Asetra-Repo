Imports System.Data.SqlClient
Imports System.Net
Imports System.Web.Http
Imports System.Web.Http.Cors

Namespace Controllers
    ''' <summary>
    ''' 処理データを登録するコントローラー
    ''' </summary>
    Public Class HtEntryProcessDataCmdController
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
        Public Function Post(<FromBody> ByVal value As HtEntryProcessDataCmdReqParameter) As HtEntryProcessDataCmdResParameter
            Dim oldMst As Boolean = False
            Dim ret As HtEntryProcessDataCmdResParameter = New HtEntryProcessDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.OK)}
            Dim assetTagErrorList = New List(Of AssetTagError)
            '2020/02/13 戸祭 v1.2 貸出上限機能 START
            Dim assetLimitErrorList = New List(Of AssetLeaseError)
            Dim assetLimitWarnList = New List(Of AssetLeaseError)
            '2020/02/13 戸祭 v1.2 貸出上限機能 END
            Dim reserveErrorList = New List(Of ReserveError) '2021/09/15 高瀬 v2.0 予約機能
            logger.Info($"Input：{Newtonsoft.Json.JsonConvert.SerializeObject(value)}")
            If Not ModelState.IsValid Then
                '送信情報が不正
                Return New HtEntryProcessDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_EXCEPTION}
            End If

            Dim notExistLocLCd As Boolean = False            '2022/07/25 佐藤 v2.2 拠点ロケ更新

            Dim db As New SystemDBContext

            If db.p_DataBaseOpenFlag = False Then
                'DB接続異常
                Return New HtEntryProcessDataCmdResParameter() With {.returnCode = Convert.ToInt32(ReturnCodeEnum.NG), .wordCd = CommonWord.MSG_ERR_DB}
            End If

            logger.Info($"SEND COUNT {value.processDataList.Count}")

            If value.processDataList.Count <= 0 Then
                '送信情報が0件は正常終了
                Return ret
            End If

            Try
                Dim param As List(Of SqlParameter)
                Dim sql As String
                '最終データのログイン担当者の名称を入手する
                Dim emp As M_EMPLOYEE = db.M_EMPLOYEE.Find(value.processDataList.Last.employeeCd)

                Using tx = db.Database.BeginTransaction((System.Data.IsolationLevel.Serializable))
                    logger.Info("TRANSACTION START")

                    '2023/08/25 戸祭 v2.4.1 ADD START
                    '入手したデータを処理日時の降順に取得する。
                    Dim processList = value.processDataList.OrderByDescending(Function(x) x.processLogId).ToList    '後ろから実績データを確認するためのリスト
                    Dim processDataList As List(Of EntryProcessDataDetail) = New List(Of EntryProcessDataDetail)           '実績登録するためのリスト

                    Dim assetMap As Hashtable = New Hashtable()
                    Dim src As EntryProcessDataDetail
                    For i As Integer = 0 To processList.Count - 1
                        Dim p = processList(i)

                        '同じ資産番号が処理対象にいるかを確認する
                        If assetMap.ContainsKey(p.assetNo) Then
                            src = assetMap(p.assetNo)
                            If src.processDiv = p.processDiv Then
                                '実績登録処理しない。次の実績データを処理
                                logger.Info($"DELETE DATA ASSET_NO={p.assetNo}, DIV={p.processDiv}")
                                Continue For
                            Else
                                '現在の資産番号の実績でハッシュテーブルを上書き
                                assetMap(p.assetNo) = p
                            End If
                        Else
                            '資産番号ハッシュテーブルに情報を追加
                            assetMap.Add(p.assetNo, p)
                        End If

                        processDataList.Add(p)
                    Next
                    value.processDataList = processDataList.ToArray                    '送信された実績データから重複した実績を削除したリストで上書き
                    '2023/08/25 戸祭 v2.4.1 ADD START

                    '一時表の作成
                    db.Database.ExecuteSqlCommand(CreateTempTable())

                    '一時表へ処理データ情報を登録する
                    For Each item As EntryProcessDataDetail In value.processDataList
                        param = New List(Of SqlParameter)
                        sql = InsertTempTable(item, param)
                        db.Database.ExecuteSqlCommand(sql, param.ToArray)
                    Next
                    'ロケ大区分名称をセット
                    db.Database.ExecuteSqlCommand(UpdateLargeLocName())

                    'ロケ中区分名称をセット
                    db.Database.ExecuteSqlCommand(UpdateMiddleLocName())

                    'ロケ小区分名称をセット
                    db.Database.ExecuteSqlCommand(UpdateSmallLocName())

                    '貸出先場所名をセット
                    db.Database.ExecuteSqlCommand(UpdateLeasePlaceName())

                    '貸出先担当者をセット
                    db.Database.ExecuteSqlCommand(UpdateLeaseEmployeeName())

                    'ログイン担当者名をセット
                    db.Database.ExecuteSqlCommand(UpdateEmployeeName())

                    '資産情報をセット
                    db.Database.ExecuteSqlCommand(UpdateAssetInfomation())

                    '一時表のデータを全て取得する
                    Dim processTmpList As EntryProcessTmpDataDetail()

                    processTmpList = CType(db.Database.SqlQuery(Of EntryProcessTmpDataDetail)(GetAllProcessData()).ToArray(), EntryProcessTmpDataDetail())

                    '2021/09/15 高瀬 v2.0 予約機能 START
                    Dim rsvFuncInfoList As ReservationFunctionInfo() = CType(db.Database.SqlQuery(Of ReservationFunctionInfo)(GetReservationFunction()).ToArray(), ReservationFunctionInfo())
                    Dim rsvFuncInfo As ReservationFunctionInfo = rsvFuncInfoList(0)
                    '2021/09/15 高瀬 v2.0 予約機能 END

                    '一時表のデータ件数分処理を行う
                    For Each item In processTmpList
                        Dim masterUpdFlg As Boolean = False
                        Dim assetIdExistFlg As Boolean = True
                        Dim taskResult As String = CommonConst.TASK_RESULT_N
                        Dim rsvUpdateFlag As Boolean = False             '2021/09/15 高瀬 v2.0 予約機能
                        Dim trgRevD As ReservationData = Nothing

                        'ログイン担当者名が入っていない場合は、マスタ不整合扱い
                        If String.IsNullOrEmpty(item.employeeCd) = False AndAlso String.IsNullOrEmpty(item.employeeName) Then
                            oldMst = True
                            logger.Info($"NOT FOUND EMPLOYEE_CD={item.employeeCd}")
                        End If

                        '貸出かつ予約機能かつ貸出ステータスが貸出中の場合処理をスキップ
                        '設定マスタの予約機能(KEY種別=Reservation_Function)がY(予約機能利用)の場合
                        If rsvFuncInfo.reservationFunction = CommonConst.RESERVATION_FUNCTION_YES Then
                            '貸出処理の場合
                            If item.processDiv = CommonConst.PROCESS_DIV_BO Then
                                '貸出中の場合
                                If item.assetStatus = CommonConst.ASSET_STATUS_LEASE Then
                                    '処理をスキップする
                                    Continue For
                                End If

                            End If

                        End If


                        '資産状態が「除却済」の場合は、処理結果をエラーとする
                        If item.assetStatus = CommonConst.ASSET_STATUS_RETIRE Then
                            taskResult = CommonConst.TASK_RESULT_E
                            masterUpdFlg = True
                        Else
                            '処理日時が空白または数値以外 デフォルト値
                            If String.IsNullOrEmpty(item.taskTs) = True OrElse IsNumeric(item.taskTs) = False Then
                                item.taskTs = "0"
                            End If

                            'If Convert.ToInt64(item.taskTs) >= Convert.ToInt64(item.processDt & item.processTm & "0") Then
                            If Convert.ToInt64(item.taskTs) >= Convert.ToInt64(item.processDt.Trim & item.processTm.Trim & "0") Then
                                masterUpdFlg = False
                            Else
                                masterUpdFlg = True
                            End If
                        End If

                        '2020/02/13 戸祭 v1.2 貸出上限機能 START
                        If item.processDiv = CommonConst.PROCESS_DIV_BO Then
                            '2021/09/15 高瀬 v2.0 予約機能 START
                            '設定マスタの予約機能(KEY種別=Reservation_Function)がY(予約機能利用)の場合
                            If rsvFuncInfo.reservationFunction = CommonConst.RESERVATION_FUNCTION_YES Then
                                '対象処理日の対象日取得
                                Dim targetDate As String = GetTargetDate(db, item.processDt, rsvFuncInfo).ToString
                                '貸出可能資産が存在する場合
                                If ExistsReservationDate(db, item.processDt, targetDate, item.assetNo) Then
                                    Dim rsvData As ReservationData() = CType(db.Database.SqlQuery(Of ReservationData)(GetReservationDate(item.processDt, targetDate, item.assetNo)).ToArray(), ReservationData())
                                    trgRevD = rsvData(0)
                                    '予約情報（ロケ、担当者、返却予定日、備考）をセット
                                    db.Database.ExecuteSqlCommand(UpdateTempTable(item.processLogId, trgRevD))
                                    rsvUpdateFlag = True
                                Else
                                    reserveErrorList.Add(New ReserveError With {.assetId = item.assetId, .assetNo = item.assetNo})
                                    taskResult = CommonConst.TASK_RESULT_E          '処理履歴をエラーで登録し、貸出処理しない
                                    logger.Info($" RESERVE ERR　ASSET_NO={item.assetNo}")
                                End If

                            End If
                            '2021/09/15 高瀬 v2.0 予約機能 END

                            '貸出の場合、貸出数が上限を超えていないかをチェックする
                            Dim leaUpInfos As LeaseUpperInfo() = CType(db.Database.SqlQuery(Of LeaseUpperInfo)(GetLeaseUpperInfo(item.assetNo)).ToArray(), LeaseUpperInfo())
                            '貸出数が上限を超えていないかをチェックする
                            If leaUpInfos.Count() > 0 Then
                                Dim leaUpInfo As LeaseUpperInfo = leaUpInfos(0)

                                If leaUpInfo.leastUpperErrDiv = CommonConst.LEASE_UPPER_ERR_DIV_ERR OrElse leaUpInfo.leastUpperErrDiv = CommonConst.LEASE_UPPER_ERR_DIV_WARN Then
                                    '貸出上限を超えたときにエラーか警告を発生する場合

                                    Dim assetItem As M_ASSET = db.M_ASSET.Find(item.assetNo)    '最新の資産情報を取得

                                    If assetItem.LEASE_CNT >= leaUpInfo.LeaseUpperLimit Then
                                        Select Case leaUpInfo.leastUpperErrDiv
                                            Case CommonConst.LEASE_UPPER_ERR_DIV_ERR            'エラー
                                                assetLimitErrorList.Add(New AssetLeaseError With {.assetId = item.assetId, .assetNo = item.assetNo, .upperLimitCount = leaUpInfo.LeaseUpperLimit})
                                                taskResult = CommonConst.TASK_RESULT_E          '処理履歴をエラーで登録し、貸出処理しない
                                                logger.Info($"LEASE UPPER ERROR　ASSET_NO={item.assetNo} LEASE_UPPER_CNT={leaUpInfo.LeaseUpperLimit}")

                                            Case CommonConst.LEASE_UPPER_ERR_DIV_WARN           '警告
                                                assetLimitWarnList.Add(New AssetLeaseError With {.assetId = item.assetId, .assetNo = item.assetNo, .upperLimitCount = leaUpInfo.LeaseUpperLimit})
                                                logger.Info($"LEASE UPPER WARN　ASSET_NO={item.assetNo} LEASE_UPPER_CNT={leaUpInfo.LeaseUpperLimit}")

                                        End Select
                                    End If
                                Else
                                    '貸出上限をチェックしない場合は何もしない
                                End If
                            End If
                        End If
                        '2020/02/13 戸祭 v1.2 貸出上限機能 END

                        'タグ紐付(読取時)は資産IDの存在チェックを行う
                        If CommonConst.PROCESS_DIV_T2.Equals(item.processDiv) Then
                            Dim assetIds As String() = CType(db.Database.SqlQuery(Of String)(GetExistAssetId(),
                                                                                                   New SqlParameter("@AssetId", item.assetId),
                                                                                                   New SqlParameter("@AssetNo", item.assetNo)).ToArray(), String())
                            If assetIds.Count > 0 Then
                                '資産IDが他の資産で使われている場合
                                assetTagErrorList.Add(New AssetTagError With {.assetId = item.assetId, .assetNo = item.assetNo, .dupAssetNo = assetIds(0)})
                                assetIdExistFlg = False

                                logger.Info($"TAG ERROR DUP_ASSET_ID={assetIds(0)} ASSET_NO={item.assetNo} PROCESS_DIV={item.processDiv}")
                            End If
                        End If

                        '資産処理履歴を登録する
                        If masterUpdFlg AndAlso assetIdExistFlg Then
                            db.Database.ExecuteSqlCommand(InsertProcessLog(item.processLogId, taskResult))

                            '2020/02/12 v1.2 戸祭 貸出上限機能 START
                            Select Case item.processDiv
                                Case CommonConst.PROCESS_DIV_BI             '処理区分が返却
                                    '登録した返却の履歴に対して貸出情報を設定する
                                    db.Database.ExecuteSqlCommand(UpdateProcessLogReturn(item.assetNo, item.processDt, item.processTm))
                            End Select
                            '2020/02/12 v1.2 戸祭 貸出上限機能 END
                        End If

                        '対象の資産が除却されていない場合に資産マスタを更新する。
                        If CommonConst.TASK_RESULT_N.Equals(taskResult) Then

                            Select Case item.processDiv

                                Case CommonConst.PROCESS_DIV_BO             '処理区分が貸出

                                    '貸出先担当者コードが入っていて、貸出先担当者名が入っていない場合は、マスタ不整合扱い
                                    If String.IsNullOrEmpty(item.leaseEmployeeCd) = False AndAlso
                                   String.IsNullOrEmpty(item.leaseEmployeeName) = True Then
                                        oldMst = True
                                        logger.Info($"NOT FOUND LEASE_EMPLOYEE_CD={item.leaseEmployeeCd}")

                                    End If

                                    '貸出先場所コードが入っていて、貸出先場所名が入っていない場合は、マスタ不整合扱い
                                    If String.IsNullOrEmpty(item.leasePlaceCd) = False AndAlso
                                   String.IsNullOrEmpty(item.leasePlaceName) = True Then
                                        oldMst = True
                                        logger.Info($"NOT FOUND LEASE_PLACE_CD={item.leasePlaceCd}")
                                    End If

                                    If masterUpdFlg Then
                                        '資産マスタを更新する
                                        db.Database.ExecuteSqlCommand(UpdateProcessLease(item.processLogId, (rsvFuncInfo.reservationFunction = CommonConst.RESERVATION_FUNCTION_YES)))
                                    End If

                                    '2021/09/15 高瀬 v2.0 予約機能 START
                                    '予約機能利用の場合
                                    If rsvFuncInfo.reservationFunction = CommonConst.RESERVATION_FUNCTION_YES Then
                                        If rsvUpdateFlag = True Then
                                            '予約明細テーブルを更新する
                                            db.Database.ExecuteSqlCommand(UpdateReservationDetailLease(item.processLogId, trgRevD.reserveId, trgRevD.realStartDt))
                                            '予約テーブルを更新する
                                            db.Database.ExecuteSqlCommand(UpdateReservationLease(item.processLogId, trgRevD.reserveId))
                                        End If
                                    End If
                                    '2021/09/15 高瀬 v2.0 予約機能 END

                                Case CommonConst.PROCESS_DIV_BI             '処理区分が返却

                                    If masterUpdFlg Then

                                        '2022/07/25 佐藤 v2.2 拠点ロケ更新 START
                                        '資産マスタを更新する
                                        If GetLocationUpdInfo(db) = CommonConst.RETURN_LOC_UPDATE_YES AndAlso Not String.IsNullOrEmpty(item.locLCd) Then
                                            '拠点ロケ更新あり
                                            If ExistLocLCd(db, item.locLCd) = False Then
                                                'マスタに存在しないロケ大区分コードの場合
                                                notExistLocLCd = True
                                                Exit For
                                            End If

                                            db.Database.ExecuteSqlCommand(UpdateProcessReturn(item.processLogId, True))
                                        Else
                                            '拠点ロケ更新なし
                                            db.Database.ExecuteSqlCommand(UpdateProcessReturn(item.processLogId))
                                        End If
                                        '2022/07/25 佐藤 v2.2 拠点ロケ更新 END
                                    End If

                                    '2021/09/15 高瀬 v2.0 予約機能 START
                                    '予約機能利用の場合
                                    If rsvFuncInfo.reservationFunction = CommonConst.RESERVATION_FUNCTION_YES Then
                                        Dim dataList As ReservationData() = CType(db.Database.SqlQuery(Of ReservationData)(GetReserveIdReturn(item.processLogId)).ToArray(), ReservationData())
                                        For iCnt As Integer = 0 To (dataList.Length - 1)
                                            '予約明細テーブルを更新する
                                            db.Database.ExecuteSqlCommand(UpdateReservationDetailReturn(item.processLogId, dataList(iCnt).reserveId, dataList(iCnt).realStartDt, rsvFuncInfo.reservationDataDiv _
                                                , (0 = (dataList.Length - 1)) _
                                                , (iCnt = (dataList.Length - 1))))
                                            '予約テーブルを更新する
                                            db.Database.ExecuteSqlCommand(UpdateReservationReturn(item.processLogId, dataList(iCnt).reserveId))

                                            ' メンテ予約があり、メンテ内の最新行の場合、更新
                                            If Not String.IsNullOrEmpty(dataList(iCnt).mainteId) _
                                                AndAlso dataList(iCnt).mainteOrder = "1" Then

                                                Dim nextDt As NextMenteUpdData() = CType(db.Database.SqlQuery(Of NextMenteUpdData)(GetNextReserveInfoReturn(item.processLogId, dataList(iCnt).mainteId, rsvFuncInfo.reservationDataDiv)).ToArray(), NextMenteUpdData())
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
                                        Next iCnt
                                    End If
                                    '2021/09/15 高瀬 v2.0 予約機能 END

                                    '2023/04/11 川元 v2.4 ADD START
                                    '設定マスタ（薬品管理機能）の取得
                                    Dim DrugsFunction As M_SETTING = db.M_SETTING.Find(CommonConst.KEY_DRUGS_FUNCTION, "**","**","**")

                                    '薬品管理機能が"Y"(あり)、かつ処理ツール区分が"C"(C/S画面)の場合
                                    If (DrugsFunction IsNot Nothing AndAlso DrugsFunction.DATA1 = CommonConst.DRUGS_FUNCTION.YES) AndAlso item.processToolDiv = CommonConst.WORD_TOOL_C Then

                                        '資産情報を取得
                                        Dim assetItem As M_ASSET = db.M_ASSET.Find(item.assetNo)

                                        '容器の重量を計算
                                        Dim packageQty As String = CalsPackageQty(assetItem.FREE_ITEM10,
                                                                                  assetItem.FREE_ITEM11,
                                                                                  assetItem.FREE_ITEM12,
                                                                                  assetItem.FREE_ITEM14)

                                        '使用量(容器の重量 ＋　NET残量　ー 返却重量)の設定
                                        Dim useQty As String = Math.Round(Decimal.Parse(assetItem.FREE_ITEM16) + Decimal.Parse(packageQty) - Decimal.Parse(item.DueQty), 2, MidpointRounding.AwayFromZero).ToString
                                        If Decimal.Parse(useQty) <0 Then
                                            useQty="0"
                                        End If

                                        '返却残量(返却重量 ー 容器の残量)の設定
                                        Dim remainQty As String = Math.Round(Decimal.Parse(item.DueQty) - Decimal.Parse(packageQty), 2, MidpointRounding.AwayFromZero).ToString
                                        If Decimal.Parse(remainQty) < 0 Then
                                            remainQty = "0"
                                        End If

                                        '消費割合の設定
                                        Dim consumptionRate As String = CalsConsumptionRate(assetItem.FREE_ITEM10,
                                                                                            assetItem.FREE_ITEM11,
                                                                                            assetItem.FREE_ITEM12,
                                                                                            assetItem.FREE_ITEM14,
                                                                                            remainQty)

                                        '薬品重量履歴を登録する
                                        db.Database.ExecuteSqlCommand(InsertDrugDueLog(item, useQty, remainQty))

                                        '資産マスタを更新する(残量(NET重量)/消費割合)
                                        db.Database.ExecuteSqlCommand(UpdateRemainQtyConsumptionRate(item.processLogId, remainQty, consumptionRate))
                                    End If
                                    '2023/04/11 川元 v2.4 ADD END

                                Case CommonConst.PROCESS_DIV_BS             '処理区分が保管

                                    'ロケ大区分名称が入っていない場合は、マスタ不整合扱い
                                    If String.IsNullOrEmpty(item.locLName) Then
                                        oldMst = True
                                        logger.Info($"NOT FOUND LOC_L_CD={item.locLCd}")
                                    Else
                                        'ロケ中区分名称が入っていない場合は、マスタ不整合扱い
                                        If Not String.IsNullOrEmpty(item.locMCd) AndAlso String.IsNullOrEmpty(item.locMName) Then
                                            oldMst = True
                                            logger.Info($"NOT FOUND LOC_M_CD={item.locMCd}")
                                        Else
                                            'ロケ小区分名称が入っていない場合は、マスタ不整合扱い
                                            If Not String.IsNullOrEmpty(item.locSCd) AndAlso String.IsNullOrEmpty(item.locSName) Then
                                                oldMst = True
                                                logger.Info($"NOT FOUND LOC_S_CD={item.locSCd}")
                                            End If
                                        End If
                                    End If

                                    If masterUpdFlg Then
                                        '資産マスタを更新する
                                        db.Database.ExecuteSqlCommand(UpdateProcessStorage(item.processLogId))
                                    End If

                                Case CommonConst.PROCESS_DIV_BR             '処理区分が除却

                                    If masterUpdFlg Then
                                        '資産マスタを更新する
                                        db.Database.ExecuteSqlCommand(UpdateProcessRetire(item.processLogId))
                                    End If

                                Case CommonConst.PROCESS_DIV_T1, CommonConst.PROCESS_DIV_T2             '処理区分がタグ紐付

                                    If masterUpdFlg AndAlso assetIdExistFlg Then
                                        '資産マスタを更新する
                                        db.Database.ExecuteSqlCommand(UpdateProcessTagLink(item.processLogId))
                                    End If
                            End Select
                        End If
                        logger.Info($"ASSET_NO={item.assetNo} PROCESS_DIV={item.processDiv} TASK_RESULT={taskResult} MASTER UPDATE={masterUpdFlg}")
                    Next

                    '棚卸管理テーブルへ実棚件数を反映する。
                    param = New List(Of SqlParameter)
                    sql = UpdateInventoryNum(emp, param)
                    db.Database.ExecuteSqlCommand(sql, param.ToArray)


                    '2020/02/12 v1.2 戸祭 貸出上限機能 START
                    'If assetTagErrorList.Count > 0 Then
                    '    'タグ紐付エラーあり
                    '    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN_TAG)
                    '    ret.wordCd = ""
                    '    ret.assetTagList = assetTagErrorList.ToArray

                    'ElseIf oldMst Then
                    '    'マスタが古い場合は警告終了
                    '    db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_OLD_MST))
                    '    ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN)
                    '    ret.wordCd = CommonWord.MSG_OLD_MST
                    'Else
                    '    db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_RECEIVE_SUCCSE))
                    'End If

                    '設定マスタ（ロケごとの資産登録上限数）の取得
                    Dim setting As M_SETTING = db.M_SETTING.Find(CommonConst.ASSET_REGISTRATION_LIMIT, "**","**","**")

                    '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 START
                    'ロケごとの資産登録上限数 ＞ 0の場合
                    If setting IsNot Nothing AndAlso Convert.ToInt32(setting.DATA1) > 0 Then
                        '管理ロケ、またはロケ大区分毎の登録件数を取得する
                        Dim locLCodeTotal As LocLCodeTotal() = CType(db.Database.SqlQuery(Of LocLCodeTotal)(GetAssetLocLCodeTotal()).ToArray(), LocLCodeTotal())
                        For i As Integer = 0 To locLCodeTotal.Length - 1
                            If locLCodeTotal(i).Cnt > Convert.ToInt32(setting.DATA1) Then
                                ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN)
                                ret.wordCdReservation = CommonWord.MSG_RETURN_UPPER_LIMIT_OVER_ERR
                                ret.wordCd = CommonWord.MSG_RETURN_UPPER_LIMIT_OVER_ERR

                                tx.Rollback
                                logger.Info("ROLLBACK")
                                db.subTerminate()
                                Return ret
                            End If
                        Next
                    End If
                    '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 END

                    '2021/09/15 高瀬 v2.0 予約機能 START
                    If reserveErrorList.Count > 0 Then
                        '該当する予約なし（予約機能利用時のみ）
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                        ret.wordCdReservation = CommonWord.MSG_RESERVE_ERR
                        ret.wordCdTag = ""
                        ret.wordCdLeaseErr = ""
                        ret.wordCdLeaseWarn = ""
                        ret.wordCd = CommonWord.MSG_RESERVE_ERR
                        ret.reserveList = reserveErrorList.ToArray
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_RESERVE_ERR))
                    End If
                    '2021/09/15 高瀬 v2.0 予約機能 END

                    If assetLimitErrorList.Count > 0 Then
                        '貸出上限警告有(未処理）
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                        ret.wordCdReservation = ""
                        ret.wordCdTag = ""
                        ret.wordCdLeaseErr = CommonWord.MSG_LEASE_UPPER_ERR                     '2021/09/15 高瀬 v2.0 予約機能
                        ret.wordCdLeaseWarn = ""
                        ret.wordCd = CommonWord.MSG_LEASE_UPPER_ERR
                        ret.assetLeaseList = assetLimitErrorList.ToArray
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_LEASE_UPPER_ERR))

                    ElseIf assetTagErrorList.Count > 0 Then
                        'タグ紐付エラーあり
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                        ret.wordCdReservation = ""
                        ret.wordCdTag = ""                                                      '2021/09/15 高瀬 v2.0 予約機能
                        ret.wordCdLeaseErr = ""
                        ret.wordCdLeaseWarn = ""
                        ret.wordCd = ""
                        ret.assetTagList = assetTagErrorList.ToArray
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_RECEIVE_SUCCSE))

                    ElseIf assetLimitWarnList.Count > 0 Then
                        '貸出上限警告有(処理済み）
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN)
                        ret.wordCdReservation = ""
                        ret.wordCdTag = ""
                        ret.wordCdLeaseErr = ""
                        ret.wordCdLeaseWarn = CommonWord.MSG_LEASE_UPPER_WARN                   '2021/09/15 高瀬 v2.0 予約機能                  
                        ret.wordCd = CommonWord.MSG_LEASE_UPPER_WARN
                        ret.assetLeaseList = assetLimitWarnList.ToArray
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_LEASE_UPPER_WARN))

                    ElseIf oldMst Then
                        'マスタが古い場合は警告終了
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_OLD_MST))
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.WARN)
                        ret.wordCdReservation = ""
                        ret.wordCdTag = ""
                        ret.wordCdLeaseErr = ""
                        ret.wordCdLeaseWarn = ""
                        ret.wordCd = CommonWord.MSG_OLD_MST

                    ElseIf notExistLocLCd Then                                                  '2022/07/25 佐藤 v2.2 拠点ロケ更新 
                        '返却時、存在しないロケ大区分コードがセットされている場合はエラー
                        ret.returnCode = Convert.ToInt32(ReturnCodeEnum.NG)
                        ret.wordCdReservation = ""
                        ret.wordCdTag = ""
                        ret.wordCdLeaseErr = ""
                        ret.wordCdLeaseWarn = ""
                        ret.wordCd = CommonWord.MSG_RETURN_LOC_L_CD_ERR
                        ret.notExistLocLCd = True
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_RETURN_LOC_L_CD_ERR))

                    Else
                        db.Database.ExecuteSqlCommand(CommonFunction.InsertOperationLog(TASK_NAME, CommonWord.WORD_CD_RECEIVE_SUCCSE))
                    End If
                    '2020/02/12 v1.2 戸祭 貸出上限機能 END

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

            'DB切断
            db.subTerminate()
            Return ret
        End Function
#End Region

#Region "Private メソッド"
        ''' <summary>
        ''' 処理データ登録用の一時表を作成するDDLを返す
        ''' </summary>
        ''' <returns>処理データ更新用一時表DDL</returns>
        Private Function CreateTempTable() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("CREATE TABLE #PROCESS_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID] [BIGINT] NOT NULL ,        /* 処理順 */")
            sb.AppendLine("	[PROCESS_DT] [CHAR] (8) NULL ,              /* 処理日 */")
            sb.AppendLine("	[PROCESS_TM] [CHAR] (9) NULL ,              /* 処理時刻 */")
            sb.AppendLine("	[ASSET_NO] [NVARCHAR] (24) NOT NULL ,       /* 資産番号 */")
            sb.AppendLine("	[ASSET_ID] [NVARCHAR] (128) NULL ,          /* 資産ID */")
            sb.AppendLine("	[PROCESS_TOOL_DIV] [CHAR] (1) NOT NULL ,    /* 処理ツール区分 */")
            sb.AppendLine("	[PROCESS_DIV] [NVARCHAR] (2) NULL ,         /* 処理区分 */")
            sb.AppendLine("	[LEASE_PLACE_CD] [NVARCHAR] (15) NULL ,     /* 貸出先場所コード */")
            sb.AppendLine("	[LEASE_PLACE_NAME] [NVARCHAR] (60) NULL ,   /* 貸出先場所名 */")
            sb.AppendLine("	[LEASE_DUE_DT] [NVARCHAR] (8) NULL ,        /* 貸出返却予定日 */")
            sb.AppendLine("	[REMARK] [NVARCHAR] (255) NULL ,            /* 備考 */")
            sb.AppendLine("	[MNG_LOC_L_CD] [NVARCHAR] (10) NULL ,       /* 管理ロケLコード */")         '20211117 UPD Takase v2.0
            sb.AppendLine("	[LOC_L_CD] [NVARCHAR] (10) NULL ,           /* ロケ大区分コード */")        '20210510 UPD Suzuki v1.3
            sb.AppendLine("	[LOC_M_CD] [NVARCHAR] (10) NULL ,           /* ロケ中区分コード */")        '20210510 UPD Suzuki v1.3
            sb.AppendLine("	[LOC_S_CD] [NVARCHAR] (10) NULL ,           /* ロケ小区分コード */")        '20210510 UPD Suzuki v1.3
            sb.AppendLine("	[LOC_L_NAME] [NVARCHAR] (60) NULL ,         /* ロケ大区分名称 */")
            sb.AppendLine("	[LOC_M_NAME] [NVARCHAR] (60) NULL ,         /* ロケ中区分名称 */")
            sb.AppendLine("	[LOC_S_NAME] [NVARCHAR] (60) NULL ,         /* ロケ小区分名称 */")
            sb.AppendLine("	[ASSET_STATUS] [TINYINT] NOT NULL DEFAULT 0 ,         /* 資産状態 */")
            sb.AppendLine("	[ASSET_NAME] [NVARCHAR] (60) NULL ,         /* 資産名称 */")
            sb.AppendLine("	[MAKER] [NVARCHAR] (60) NULL ,              /* メーカー */")
            sb.AppendLine("	[PRODUCT_NO] [NVARCHAR] (30) NULL ,         /* 品番 */")
            sb.AppendLine("	[MODEL_NO] [NVARCHAR] (30) NULL ,           /* 型番 */")
            '2023/04/11 川元 v2.4 ADD START
            sb.AppendLine("	[DUE_QTY] [DECIMAL] (8,3) NULL ,            /* 返却重量 */")
            sb.AppendLine("	[WORKER_MINUTE] [SMALLINT] NULL ,           /* 作業時間 */")
            sb.AppendLine("	[WORKER_OVERVIEW] [NVARCHAR] (255) NULL ,   /* 作業概要 */")
            sb.AppendLine("	[WORKER_REMARK1] [NVARCHAR] (255) NULL ,    /* 作業備考1 */")
            sb.AppendLine("	[WORKER_REMARK2] [NVARCHAR] (255) NULL ,    /* 作業備考2 */")
            '2023/04/11 川元 v2.4 ADD END
            sb.AppendLine("	[TASK_TS] [BIGINT]  NULL ,                  /* 処理日時 */")
            sb.AppendLine("	[LEASE_EMPLOYEE_CD] [NVARCHAR] (15) NULL ,  /* 貸出先担当者社員コード */")
            sb.AppendLine("	[LEASE_EMPLOYEE_NAME] [NVARCHAR] (60) NULL, /* 貸出先担当者氏名 */")
            sb.AppendLine("	[EMPLOYEE_CD] [NVARCHAR] (15) NULL ,	    /* ログイン担当者コード */")
            sb.AppendLine("	[EMPLOYEE_NAME] [NVARCHAR] (60) NULL        /* ログイン担当者氏名 */")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 処理データ登録用の一時表にデータを登録するクエリとパラメータを返す
        ''' </summary>
        ''' <param name="item">処理データ情報</param>
        ''' <param name="param">返却用パラメータ</param>
        ''' <returns>登録DML</returns>
        Private Function InsertTempTable(ByVal item As EntryProcessDataDetail, ByRef param As List(Of SqlParameter)) As String
            Dim sb As StringBuilder = New StringBuilder()

            '一時表への登録
            sb.AppendLine("INSERT INTO #PROCESS_TEMP")
            sb.AppendLine("(")
            sb.AppendLine("	[PROCESS_LOG_ID],")
            sb.AppendLine("	[PROCESS_DT],")
            sb.AppendLine("	[PROCESS_TM],")
            sb.AppendLine("	[ASSET_NO],")
            sb.AppendLine("	[ASSET_ID],")
            sb.AppendLine("	[PROCESS_TOOL_DIV],")
            sb.AppendLine("	[PROCESS_DIV],")
            sb.AppendLine("	[LEASE_PLACE_CD],")
            sb.AppendLine("	[LEASE_EMPLOYEE_CD],")
            sb.AppendLine("	[LEASE_DUE_DT],")
            sb.AppendLine("	[REMARK],")
            sb.AppendLine("	[LOC_L_CD],")
            sb.AppendLine("	[LOC_M_CD],")
            sb.AppendLine("	[LOC_S_CD],")
            '2023/04/11 川元 v2.4 ADD START
            sb.AppendLine("	[DUE_QTY],")
            sb.AppendLine("	[WORKER_MINUTE],")
            sb.AppendLine("	[WORKER_OVERVIEW],")
            sb.AppendLine("	[WORKER_REMARK1],")
            sb.AppendLine("	[WORKER_REMARK2],")
            '2023/04/11 川元 v2.4 ADD END
            sb.AppendLine("	[EMPLOYEE_CD]")
            sb.AppendLine(")")
            sb.AppendLine("VALUES")
            sb.AppendLine("(")
            sb.AppendLine("	@ProcessLogId,")
            sb.AppendLine("	@ProcessDt,")
            sb.AppendLine("	@ProcessTm,")
            sb.AppendLine("	@AssetNo,")
            sb.AppendLine("	@AssetId,")
            sb.AppendLine("	@ProcessToolDiv,")
            sb.AppendLine("	@ProcessDiv,")
            sb.AppendLine("	@LeasePlaceCd,")
            sb.AppendLine("	@LeaseEmployeeCd,")
            sb.AppendLine("	@LeaseDueDt,")
            sb.AppendLine("	@Remark,")
            sb.AppendLine("	@LocLCd,")
            sb.AppendLine("	@LocMCd,")
            sb.AppendLine("	@LocSCd,")
            '2023/04/11 川元 v2.4 ADD START
            sb.AppendLine("	@DUE_QTY,")
            sb.AppendLine("	@WORKER_MINUTE,")
            sb.AppendLine("	@WORKER_OVERVIEW,")
            sb.AppendLine("	@WORKER_REMARK1,")
            sb.AppendLine("	@WORKER_REMARK2,")
            '2023/04/11 川元 v2.4 ADD END
            sb.AppendLine("	@EmployeeCd")
            sb.AppendLine(")")

            'パラメータ設定
            param.Add(New SqlParameter("@ProcessLogId", item.processLogId))
            param.Add(New SqlParameter("@ProcessDt", IIf(item.processDt Is Nothing, DBNull.Value, item.processDt)))
            param.Add(New SqlParameter("@ProcessTm", IIf(item.processTm Is Nothing, DBNull.Value, item.processTm)))
            param.Add(New SqlParameter("@AssetNo", IIf(item.assetNo Is Nothing, DBNull.Value, item.assetNo)))
            param.Add(New SqlParameter("@AssetId", IIf(item.assetId Is Nothing, DBNull.Value, item.assetId)))
            param.Add(New SqlParameter("@ProcessToolDiv", IIf(item.processToolDiv Is Nothing, DBNull.Value, item.processToolDiv)))
            param.Add(New SqlParameter("@ProcessDiv", IIf(item.processDiv Is Nothing, DBNull.Value, item.processDiv)))
            param.Add(New SqlParameter("@LeasePlaceCd", IIf(item.leasePlaceCd Is Nothing, DBNull.Value, item.leasePlaceCd)))
            param.Add(New SqlParameter("@LeaseEmployeeCd", IIf(item.leaseEmployeeCd Is Nothing, DBNull.Value, item.leaseEmployeeCd)))
            param.Add(New SqlParameter("@LeaseDueDt", IIf(item.leaseDueDt Is Nothing, DBNull.Value, item.leaseDueDt)))
            param.Add(New SqlParameter("@Remark", IIf(item.remark Is Nothing, DBNull.Value, item.remark)))
            param.Add(New SqlParameter("@LocLCd", IIf(item.locLCd Is Nothing, DBNull.Value, item.locLCd)))
            param.Add(New SqlParameter("@LocMCd", IIf(item.locMCd Is Nothing, DBNull.Value, item.locMCd)))
            param.Add(New SqlParameter("@LocSCd", IIf(item.locSCd Is Nothing, DBNull.Value, item.locSCd)))
            '2023/04/11 川元 v2.4 ADD START
            param.Add(New SqlParameter("@DUE_QTY", IIf(String.IsNullOrEmpty(item.dueQty), DBNull.Value, item.dueQty)))
            param.Add(New SqlParameter("@WORKER_MINUTE", IIf(String.IsNullOrEmpty(item.workTime), DBNull.Value, item.workTime)))
            param.Add(New SqlParameter("@WORKER_OVERVIEW", IIf(String.IsNullOrEmpty(item.workOutline), DBNull.Value, item.workOutline)))
            param.Add(New SqlParameter("@WORKER_REMARK1", IIf(String.IsNullOrEmpty(item.retRemark1), DBNull.Value, item.retRemark1)))
            param.Add(New SqlParameter("@WORKER_REMARK2", IIf(String.IsNullOrEmpty(item.retRemark2), DBNull.Value, item.retRemark2)))
            '2023/04/11 川元 v2.4 ADD END
            param.Add(New SqlParameter("@EmployeeCd", IIf(item.employeeCd Is Nothing, DBNull.Value, item.employeeCd)))

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ロケ大区分名称を更新する
        ''' </summary>
        Private Function UpdateLargeLocName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	LOC_L_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			PROCESS_TEMP.LOC_L_CD = LOC.LOC_L_CD")
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
            sb.AppendLine("	PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	LOC_M_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			PROCESS_TEMP.LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine("		AND PROCESS_TEMP.LOC_M_CD = LOC.LOC_M_CD")
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
            sb.AppendLine("	PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	LOC_S_NAME = LOC.LOC_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LOCATION AS LOC")
            sb.AppendLine("		ON")
            sb.AppendLine("			PROCESS_TEMP.LOC_L_CD = LOC.LOC_L_CD")
            sb.AppendLine("		AND PROCESS_TEMP.LOC_M_CD = LOC.LOC_M_CD")
            sb.AppendLine("		AND PROCESS_TEMP.LOC_S_CD = LOC.LOC_S_CD")
            sb.AppendLine($"	AND LOC_DIV = '{CommonConst.LOC_S_DIV}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 貸出先名称を更新する
        ''' </summary>
        Private Function UpdateLeasePlaceName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	LEASE_PLACE_NAME = PLA.LEASE_PLACE_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_LEASE_PLACE AS PLA")
            sb.AppendLine("		ON")
            sb.AppendLine("			PROCESS_TEMP.LEASE_PLACE_CD = PLA.LEASE_PLACE_CD ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' ログイン担当者名を更新する
        ''' </summary>
        Private Function UpdateEmployeeName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	 EMPLOYEE_NAME = EMP.EMPLOYEE_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_EMPLOYEE AS EMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			PROCESS_TEMP.EMPLOYEE_CD = EMP.EMPLOYEE_CD")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 貸出先担当者名を更新する
        ''' </summary>
        Private Function UpdateLeaseEmployeeName() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("	 LEASE_EMPLOYEE_NAME = EMP.EMPLOYEE_NAME")
            sb.AppendLine("FROM")
            sb.AppendLine("	#PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_EMPLOYEE AS EMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			PROCESS_TEMP.LEASE_EMPLOYEE_CD = EMP.EMPLOYEE_CD")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産情報を更新する
        ''' </summary>
        Private Function UpdateAssetInfomation() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine(" PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine("  ASSET_STATUS = ASSET.ASSET_STATUS")
            sb.AppendLine(" ,ASSET_NAME = ASSET.ASSET_NAME")
            sb.AppendLine(" ,MNG_LOC_L_CD = ASSET.MNG_LOC_L_CD")
            sb.AppendLine(" ,MAKER = ASSET.MAKER")
            sb.AppendLine(" ,PRODUCT_NO = ASSET.PRODUCT_NO")
            sb.AppendLine(" ,MODEL_NO = ASSET.MODEL_NO")
            sb.AppendLine(" ,TASK_TS = ASSET.TASK_TS")
            sb.AppendLine("FROM")
            sb.AppendLine("	#PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("	INNER JOIN")
            sb.AppendLine("		M_ASSET AS ASSET")
            sb.AppendLine("		ON")
            sb.AppendLine("			PROCESS_TEMP.ASSET_NO = ASSET.ASSET_NO")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 一時表の処理データ情報をすべて返す
        ''' </summary>
        Private Function GetAllProcessData() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_LOG_ID      AS ProcessLogId ")
            sb.AppendLine(" ,PROCESS_DT          AS ProcessDt ")
            sb.AppendLine(" ,PROCESS_TM          AS ProcessTm ")
            sb.AppendLine(" ,ASSET_NO            AS AssetNo ")
            sb.AppendLine(" ,ASSET_ID            AS AssetId ")
            '2023/04/11 川元 v2.4 ADD START
            sb.AppendLine(" ,PROCESS_TOOL_DIV    As ProcessToolDiv ")
            '2023/04/11 川元 v2.4 ADD END
            sb.AppendLine(" ,PROCESS_DIV         As ProcessDiv ")
            sb.AppendLine(" ,LEASE_PLACE_CD      AS LeasePlaceCd ")
            sb.AppendLine(" ,LEASE_PLACE_NAME    AS LeasePlaceName ")
            sb.AppendLine(" ,LEASE_DUE_DT        As LeaseDueDt ")
            sb.AppendLine(" ,REMARK              As Remark ")
            sb.AppendLine(" ,LOC_L_CD            As LocLCd ")
            sb.AppendLine(" ,LOC_M_CD            As LocMCd ")
            sb.AppendLine(" ,LOC_S_CD            As LocSCd ")
            sb.AppendLine(" ,LOC_L_NAME          As LocLName ")
            sb.AppendLine(" ,LOC_M_NAME          As LocMName ")
            sb.AppendLine(" ,LOC_S_NAME          As LocSName ")
            sb.AppendLine(" ,CONVERT(VARCHAR,ASSET_STATUS)        As AssetStatus ")
            sb.AppendLine(" ,ASSET_NAME          As AssetName ")
            sb.AppendLine(" ,MAKER               As Maker ")
            sb.AppendLine(" ,PRODUCT_NO          As ProductNo ")
            sb.AppendLine(" ,MODEL_NO            As ModelNo ")
            '2023/04/11 川元 v2.4 ADD START
            sb.AppendLine("	,DUE_QTY             As DueQty ")
            sb.AppendLine("	,WORKER_MINUTE       As WorkerMinute ")
            sb.AppendLine("	,WORKER_OVERVIEW     As WorkerOverview ")
            sb.AppendLine("	,WORKER_REMARK1      As WorkerRemark1 ")
            sb.AppendLine("	,WORKER_REMARK2      As WorkerRemark2 ")
            '2023/04/11 川元 v2.4 ADD END
            sb.AppendLine(" ,CONVERT(VARCHAR,TASK_TS)             As TaskTs ")
            sb.AppendLine(" ,LEASE_EMPLOYEE_CD   As LeaseEmployeeCd ")
            sb.AppendLine(" ,LEASE_EMPLOYEE_NAME As LeaseEmployeeName ")
            sb.AppendLine(" ,EMPLOYEE_CD         As EmployeeCd ")
            sb.AppendLine(" ,EMPLOYEE_NAME       As EmployeeName ")
            sb.AppendLine("FROM")
            sb.AppendLine(" #PROCESS_TEMP")
            sb.AppendLine("ORDER BY")
            sb.AppendLine(" processLogId")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：貸出）
        ''' </summary>
        Private Function UpdateProcessLease(ByVal logId As String, reserveFlg As Boolean) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine("SET")
            sb.AppendLine("	 ASSET.LEASE_CNT = ASSET.LEASE_CNT + 1 ")
            sb.AppendLine($",ASSET.ASSET_STATUS = {CommonConst.ASSET_STATUS_LEASE} ")
            sb.AppendLine(" ,ASSET.LEASE_PLACE_CD = PROCESS_TEMP.LEASE_PLACE_CD")
            sb.AppendLine(" ,ASSET.LEASE_PLACE_NAME = PROCESS_TEMP.LEASE_PLACE_NAME")
            sb.AppendLine(" ,ASSET.LEASE_EMPLOYEE_CD = PROCESS_TEMP.LEASE_EMPLOYEE_CD")
            sb.AppendLine(" ,ASSET.LEASE_EMPLOYEE_NAME = PROCESS_TEMP.LEASE_EMPLOYEE_NAME")
            sb.AppendLine(" ,ASSET.LEASE_DT = PROCESS_TEMP.PROCESS_DT")
            sb.AppendLine(" ,ASSET.LEASE_DUE_DT = PROCESS_TEMP.LEASE_DUE_DT")
            If Not reserveFlg Then
                sb.AppendLine(" ,ASSET.REMARK = PROCESS_TEMP.REMARK")
            End If
            sb.AppendLine("	,ASSET.TASK_TS = CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine("	,ASSET.UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",ASSET.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine("FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("	INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET.ASSET_NO = PROCESS_TEMP.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    PROCESS_TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine(" AND (")
            sb.AppendLine("         ASSET.TASK_TS IS NULL ")
            sb.AppendLine("      OR ")
            sb.AppendLine("         ASSET.TASK_TS <= CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine(" )")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：返却）
        ''' </summary>
        Private Function UpdateProcessReturn(ByVal logId As String, Optional ByVal locUpdFlg As Boolean = False) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine(" Set")
            sb.AppendLine($" ASSET.ASSET_STATUS = {CommonConst.ASSET_STATUS_INV} ")
            sb.AppendLine(" , ASSET.LEASE_PLACE_CD = NULL")
            sb.AppendLine(" , ASSET.LEASE_PLACE_NAME = NULL")
            sb.AppendLine(" , ASSET.LEASE_EMPLOYEE_CD = NULL")
            sb.AppendLine(" , ASSET.LEASE_EMPLOYEE_NAME = NULL")
            sb.AppendLine(" , ASSET.LEASE_DT = NULL")
            sb.AppendLine(" , ASSET.LEASE_DUE_DT = NULL")
            sb.AppendLine(" , ASSET.REMARK = NULL")                         '2020/02/06 戸祭 貸出先備考クリア追加
            sb.AppendLine("	, ASSET.TASK_TS = Convert(BIGINT, PROCESS_TEMP.PROCESS_DT + Left(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine("	, ASSET.UPD_DATETIME = GETDATE()")
            sb.AppendLine("	, ASSET.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",ASSET.UPD_TASK = '{TASK_NAME}' ")
            '2022/07/25 佐藤 v2.2 拠点ロケ更新 START
            If locUpdFlg Then
                'ロケ大区分更新
                sb.AppendLine("	,ASSET.LOC_L_CD = CASE ISNULL(PROCESS_TEMP.LOC_L_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.LOC_L_CD END ")
                sb.AppendLine("	,ASSET.LOC_M_CD = CASE ISNULL(PROCESS_TEMP.LOC_M_CD, '') WHEN '' THEN ' ' END ")
                sb.AppendLine("	,ASSET.LOC_S_CD = CASE ISNULL(PROCESS_TEMP.LOC_S_CD, '') WHEN '' THEN ' ' END ")
            End If
            '2022/07/25 佐藤 v2.2 拠点ロケ更新 END
            sb.AppendLine("FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("	INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET.ASSET_NO = PROCESS_TEMP.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    PROCESS_TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine(" AND (")
            sb.AppendLine("         ASSET.TASK_TS IS NULL ")
            sb.AppendLine("      OR ")
            sb.AppendLine("         ASSET.TASK_TS <= CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine(" )")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：保管）
        ''' </summary>
        Private Function UpdateProcessStorage(ByVal logId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine(" SET")
            sb.AppendLine($" ASSET.ASSET_STATUS = {CommonConst.ASSET_STATUS_INV} ")
            sb.AppendLine("	,ASSET.LOC_L_CD = CASE ISNULL(PROCESS_TEMP.LOC_L_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.LOC_L_CD END ")
            sb.AppendLine("	,ASSET.LOC_M_CD = CASE ISNULL(PROCESS_TEMP.LOC_M_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.LOC_M_CD END ")
            sb.AppendLine("	,ASSET.LOC_S_CD = CASE ISNULL(PROCESS_TEMP.LOC_S_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.LOC_S_CD END ")
            sb.AppendLine(" ,ASSET.LEASE_PLACE_CD = NULL")
            sb.AppendLine(" ,ASSET.LEASE_PLACE_NAME = NULL")
            sb.AppendLine(" ,ASSET.LEASE_EMPLOYEE_CD = NULL")
            sb.AppendLine(" ,ASSET.LEASE_EMPLOYEE_NAME = NULL")
            sb.AppendLine(" ,ASSET.LEASE_DT = NULL")
            sb.AppendLine(" ,ASSET.LEASE_DUE_DT = NULL")
            sb.AppendLine(" ,ASSET.REMARK = NULL")                          '2020/02/06 戸祭 貸出先備考クリア追加
            sb.AppendLine("	,ASSET.TASK_TS = CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine("	,ASSET.UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",ASSET.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine(" FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("	INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET.ASSET_NO = PROCESS_TEMP.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    PROCESS_TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine(" AND (")
            sb.AppendLine("         ASSET.TASK_TS IS NULL ")
            sb.AppendLine("      OR ")
            sb.AppendLine("         ASSET.TASK_TS <= CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine(" )")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：除却）
        ''' </summary>
        Private Function UpdateProcessRetire(ByVal logId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine(" SET")
            sb.AppendLine("  ASSET.ASSET_ID = NULL ")
            sb.AppendLine($",ASSET.ASSET_STATUS = {CommonConst.ASSET_STATUS_RETIRE} ")
            sb.AppendLine("	,ASSET.EPC_ENC_STATUS = 0 ")
            sb.AppendLine("	,ASSET.TAG_LINK_STATUS = 0 ")
            sb.AppendLine("	,ASSET.RETIRE_TS = CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + PROCESS_TEMP.PROCESS_TM) ")
            sb.AppendLine("	,ASSET.RETIRE_EMPLOYEE_CD = PROCESS_TEMP.EMPLOYEE_CD ")
            sb.AppendLine("	,ASSET.RETIRE_EMPLOYEE_NAME = PROCESS_TEMP.EMPLOYEE_NAME ")
            sb.AppendLine("	,ASSET.RETIRE_ASSET_ID = PROCESS_TEMP.ASSET_ID ")
            sb.AppendLine("	,ASSET.TASK_TS = CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine("	,ASSET.UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",ASSET.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine(" FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("	INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET.ASSET_NO = PROCESS_TEMP.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    PROCESS_TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine($"AND ASSET.ASSET_STATUS <> {CommonConst.ASSET_STATUS_RETIRE} ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 資産マスタを更新する（処理区分：タグ紐付）
        ''' </summary>
        Private Function UpdateProcessTagLink(ByVal logId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine(" Set")
            sb.AppendLine("  ASSET.ASSET_ID = PROCESS_TEMP.ASSET_ID ")
            sb.AppendLine($",ASSET.ASSET_STATUS = CASE ASSET.ASSET_STATUS WHEN '{CommonConst.ASSET_STATUS_NOT_LINK}' THEN '{CommonConst.ASSET_STATUS_INV}' ELSE ASSET.ASSET_STATUS END ")         '2020/02/13 戸祭 v1.1 ステータス更新の考慮漏れ対応
            sb.AppendLine(" ,ASSET.EPC_ENC_STATUS = 1 ")
            sb.AppendLine("	,ASSET.TAG_LINK_STATUS = 1 ")
            sb.AppendLine("	,ASSET.TASK_TS = CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine($",ASSET.PHYINV_STATUS = CASE WHEN (SELECT 1 FROM T_INVENTORY INV INNER JOIN T_INV_DETAIL DTL ON INV.INV_ID = DTL.INV_ID WHERE INV_CONFIRM_TS IS NULL AND INV_LOC_L_CD = ASSET.LOC_L_CD) = 1 THEN CASE ASSET.ASSET_STATUS WHEN '{CommonConst.ASSET_STATUS_NOT_LINK}' THEN '{CommonConst.PHYINV_STATUS_ERROR}' ELSE ASSET.PHYINV_STATUS END ELSE ASSET.PHYINV_STATUS END ")    '2020/02/13 戸祭 v1.1 
            sb.AppendLine("	,ASSET.UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",ASSET.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine(" FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("	INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET.ASSET_NO = PROCESS_TEMP.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    PROCESS_TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine(" AND (")
            sb.AppendLine("         ASSET.TASK_TS IS NULL ")
            sb.AppendLine("      OR ")
            sb.AppendLine("         ASSET.TASK_TS <= CONVERT(BIGINT,PROCESS_TEMP.PROCESS_DT + LEFT(PROCESS_TEMP.PROCESS_TM, 6))")
            sb.AppendLine(" )")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        '2023/04/11 川元 v2.4 ADD START
        ''' <summary>
        ''' 資産マスタを更新する（フリー項目16(残量(NET重量))、フリー項目17(消費割合)）
        ''' </summary>
        Private Function UpdateRemainQtyConsumptionRate(ByVal logId As String,
                                                     ByVal remainQty As String,
                                                     ByVal consumptionRate As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	ASSET")
            sb.AppendLine("SET")
            sb.AppendLine($" ASSET.FREE_ITEM16 = {remainQty} ")
            sb.AppendLine($",ASSET.FREE_ITEM17 = {consumptionRate} ")
            sb.AppendLine("	,ASSET.UPD_DATETIME = GETDATE()")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("	,ASSET.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($",ASSET.UPD_TASK = '{CommonConst.TASK_RETURN_REGIST}'")
            sb.AppendLine(" FROM")
            sb.AppendLine("	M_ASSET AS ASSET")
            sb.AppendLine("	INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("		ON")
            sb.AppendLine("			ASSET.ASSET_NO = PROCESS_TEMP.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    PROCESS_TEMP.PROCESS_LOG_ID = {logId} ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
        '2023/04/11 川元 v2.4 ADD END

        '2021/09/15 高瀬 v2.0 予約機能 START
        ''' <summary>
        ''' 一時表の予約データを更新する
        ''' </summary>
        ''' <param name="res"></param>
        ''' <returns></returns>
        Private Function UpdateTempTable(ByVal logId As String, ByVal res As ReservationData) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("	  #PROCESS_TEMP")
            sb.AppendLine("SET")
            sb.AppendLine($"  LEASE_PLACE_CD      = '{res.leasePlaceCd}'")
            sb.AppendLine($" ,LEASE_PLACE_NAME    = '{res.leasePlaceNm}'")
            sb.AppendLine($" ,REMARK              = '{res.remark}'")
            sb.AppendLine($" ,LEASE_EMPLOYEE_CD   = '{res.changeEmpCd}'")
            sb.AppendLine($" ,LEASE_EMPLOYEE_NAME = '{res.changeEmpNm}'")
            sb.AppendLine($" ,LEASE_DUE_DT        = '{res.realEndDt}'")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    PROCESS_LOG_ID = {logId} ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString

        End Function

        ''' <summary>
        ''' 予約明細テーブルを更新する（処理区分：貸出）
        ''' </summary>
        ''' <param name="reserveId"></param>
        ''' <returns></returns>
        Private Function UpdateReservationDetailLease(ByVal logId As String, ByVal reserveId As String, realStartDt As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine($"  RESERVE_DETAIL")
            sb.AppendLine("SET")
            sb.AppendLine("   RESERVE_DETAIL.LENDING_DT =  PROCESS_TEMP.PROCESS_DT")
            sb.AppendLine("  ,RESERVE_DETAIL.PR_ST_DT   = PROCESS_TEMP.PROCESS_DT")
            sb.AppendLine($" ,RESERVE_DETAIL.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine("  ,RESERVE_DETAIL.UPD_DATETIME = GETDATE() ")
            sb.AppendLine("  ,RESERVE_DETAIL.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("  ,RESERVE_DETAIL.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($" ,RESERVE_DETAIL.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine(" FROM")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} AS RESERVE_DETAIL")
            sb.AppendLine("INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine($" ON PROCESS_TEMP.PROCESS_LOG_ID = {logId}")
            sb.AppendLine(" WHERE")
            sb.AppendLine($"  RESERVE_DETAIL.RESERVE_ID = '{reserveId}' ")
            sb.AppendLine($"  AND RESERVE_DETAIL.ASSET_NO = PROCESS_TEMP.ASSET_NO ")
            sb.AppendLine($"  AND RESERVE_DETAIL.REAL_START_DT = '{realStartDt}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約テーブルを更新する（処理区分：貸出）
        ''' </summary>
        ''' <param name="reserveId"></param>
        ''' <returns></returns>
        Private Function UpdateReservationLease(ByVal logId As String, ByVal reserveId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("   RESERVE")
            sb.AppendLine("SET")
            '2024/04/17 Tomatsuri Start
            '予約明細に登録している計測器が全て貸出になった場合のみ"貸出中"に更新する
            'sb.AppendLine($"  RESERVE.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine($"  RESERVE.RESERVE_STATUS = CASE (SELECT COUNT(1) FROM {CommonConst.TABLE_RESERVE_DETAIL} AS DTL WHERE DTL.RESERVE_ID = '{reserveId}' AND RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_UNRENTED}') WHEN 0 THEN '{CommonConst.RESERVE_STATUS_RENTED}' ELSE '{CommonConst.RESERVE_STATUS_UNRENTED}' END ")
            '2024/04/17 Tomatsuri END
            sb.AppendLine("  ,RESERVE.UPD_DATETIME = GETDATE() ")
            sb.AppendLine("  ,RESERVE.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("  ,RESERVE.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($" ,RESERVE.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine(" FROM")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} AS RESERVE_DETAIL")
            sb.AppendLine("INNER JOIN ")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE} AS RESERVE")
            sb.AppendLine("ON ")
            sb.AppendLine("	  RESERVE_DETAIL.RESERVE_ID = RESERVE.RESERVE_ID")
            sb.AppendLine("INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine($" ON PROCESS_TEMP.PROCESS_LOG_ID = {logId}")
            sb.AppendLine(" WHERE")
            sb.AppendLine($"  RESERVE.RESERVE_ID = '{reserveId}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 更新する予約IDを取得する（処理区分：返却）
        ''' </summary>
        ''' <returns></returns>
        Private Function GetReserveIdReturn(logId As Long) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("   DETAIL.RESERVE_ID     AS reserveId ")
            sb.AppendLine("  ,DETAIL.ASSET_NO       AS assetNo ")
            sb.AppendLine("  ,DETAIL.REAL_START_DT  AS realStartDt ")
            sb.AppendLine("  ,CONVERT(VARCHAR, MAINTE.MAINTE_ID)      AS mainteId ")
            sb.AppendLine("  ,CONVERT(VARCHAR, ISNULL(RANK() OVER (PARTITION BY MAINTE.MAINTE_ID ORDER BY DETAIL.REAL_END_DT DESC),0)) AS mainteOrder ")
            sb.AppendLine(" FROM ")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} AS DETAIL ")
            sb.AppendLine($"    INNER JOIN #PROCESS_TEMP AS TEMP ")
            sb.AppendLine($"        ON  TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine($"    LEFT OUTER JOIN {CommonConst.TABLE_RESERVE_MAINTE} AS MAINTE ")
            sb.AppendLine("        ON  MAINTE.RESERVE_ID = DETAIL.RESERVE_ID ")
            sb.AppendLine("        AND MAINTE.ASSET_NO   = DETAIL.ASSET_NO ")
            sb.AppendLine(" WHERE ")
            sb.AppendLine($"     DETAIL.ASSET_NO       = TEMP.ASSET_NO ")
            sb.AppendLine($" AND DETAIL.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine(" ORDER BY ")
            sb.AppendLine("   DETAIL.REAL_START_DT ")

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
        ''' 予約明細テーブルを更新する（処理区分：返却）
        ''' </summary>
        ''' <param name="reserveId"></param>
        ''' <returns></returns>
        Private Function UpdateReservationDetailReturn(ByVal logId As String, ByVal reserveId As String, realStartDt As String, dateDiv As String, isOnly As Boolean, isLast As Boolean) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine($" WITH V_NEXT AS ( ")
            sb.AppendLine($"   SELECT MIN(D1.REAL_START_DT) AS NEXT_DT")
            sb.AppendLine($"   FROM #PROCESS_TEMP AS T1 ")
            sb.AppendLine($"     LEFT OUTER JOIN {CommonConst.TABLE_RESERVE_DETAIL} D1 ")
            sb.AppendLine($"       ON D1.ASSET_NO       = T1.ASSET_NO")
            sb.AppendLine($"      AND D1.REAL_START_DT  > '{realStartDt}' ")
            sb.AppendLine($"      AND D1.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_UNRENTED}' ")
            sb.AppendLine($"   WHERE T1.PROCESS_LOG_ID = {logId}")
            sb.AppendLine(" )")


            sb.AppendLine("UPDATE")
            sb.AppendLine($"  RESERVE_DETAIL")
            sb.AppendLine("SET")

            If isOnly Then
                ' 通常の場合
                sb.AppendLine("   RESERVE_DETAIL.RETURN_DT")
                sb.AppendLine("     = CASE WHEN V_NEXT.NEXT_DT IS NULL                   THEN PROCESS_TEMP.PROCESS_DT ")
                sb.AppendLine("            WHEN V_NEXT.NEXT_DT > PROCESS_TEMP.PROCESS_DT THEN PROCESS_TEMP.PROCESS_DT ")
                sb.AppendLine($"           ELSE dbo.TARGET_DATE_SF(V_NEXT.NEXT_DT, -1, '{dateDiv}') END ")
                sb.AppendLine("  ,RESERVE_DETAIL.PR_END_DT")
                sb.AppendLine("     = CASE WHEN V_NEXT.NEXT_DT IS NULL                   THEN PROCESS_TEMP.PROCESS_DT ")
                sb.AppendLine("            WHEN V_NEXT.NEXT_DT > PROCESS_TEMP.PROCESS_DT THEN PROCESS_TEMP.PROCESS_DT ")
                sb.AppendLine($"           ELSE dbo.TARGET_DATE_SF(V_NEXT.NEXT_DT, -1, '{dateDiv}') END ")
            Else
                ' 引継ぎデータの場合
                If isLast Then
                    ' (最終)引継先の場合

                    ' 開始日が返却処理日より後の場合、  予実予定日を開始/終了日に設定する
                    sb.AppendLine("   RESERVE_DETAIL.RETURN_DT ")
                    sb.AppendLine("     = (CASE WHEN RESERVE_DETAIL.REAL_START_DT >  PROCESS_TEMP.PROCESS_DT THEN RESERVE_DETAIL.REAL_START_DT")
                    sb.AppendLine("             WHEN V_NEXT.NEXT_DT               IS NULL                    THEN PROCESS_TEMP.PROCESS_DT ")
                    sb.AppendLine("             WHEN V_NEXT.NEXT_DT               >  PROCESS_TEMP.PROCESS_DT THEN PROCESS_TEMP.PROCESS_DT ")
                    sb.AppendLine($"            ELSE dbo.TARGET_DATE_SF(V_NEXT.NEXT_DT, -1, '{dateDiv}') END)")
                    sb.AppendLine("  ,RESERVE_DETAIL.PR_END_DT ")
                    sb.AppendLine("     = (CASE WHEN RESERVE_DETAIL.REAL_START_DT >  PROCESS_TEMP.PROCESS_DT THEN RESERVE_DETAIL.REAL_START_DT")
                    sb.AppendLine("             WHEN V_NEXT.NEXT_DT               IS NULL                    THEN PROCESS_TEMP.PROCESS_DT ")
                    sb.AppendLine("             WHEN V_NEXT.NEXT_DT               >  PROCESS_TEMP.PROCESS_DT THEN PROCESS_TEMP.PROCESS_DT ")
                    sb.AppendLine($"            ELSE dbo.TARGET_DATE_SF(V_NEXT.NEXT_DT, -1, '{dateDiv}') END)")
                Else
                    ' 引継元の場合
                    sb.AppendLine("   RESERVE_DETAIL.RETURN_DT = RESERVE_DETAIL.PR_END_DT")
                    sb.AppendLine("  ,RESERVE_DETAIL.PR_END_DT = RESERVE_DETAIL.PR_END_DT")
                End If
            End If
            sb.AppendLine($" ,RESERVE_DETAIL.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_RERUENED}' ")
            sb.AppendLine("  ,RESERVE_DETAIL.UPD_DATETIME = GETDATE() ")
            sb.AppendLine("  ,RESERVE_DETAIL.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("  ,RESERVE_DETAIL.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($" ,RESERVE_DETAIL.UPD_TASK = '{TASK_NAME}'")
            sb.AppendLine(" FROM")
            sb.AppendLine($"  {CommonConst.TABLE_RESERVE_DETAIL} AS RESERVE_DETAIL")
            sb.AppendLine(" INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine($" ON  PROCESS_TEMP.PROCESS_LOG_ID = {logId}")
            sb.AppendLine(" INNER JOIN V_NEXT ON  1=1")
            sb.AppendLine(" WHERE")
            sb.AppendLine($"  RESERVE_DETAIL.RESERVE_ID = '{reserveId}' ")
            sb.AppendLine($"  AND RESERVE_DETAIL.ASSET_NO = PROCESS_TEMP.ASSET_NO ")
            sb.AppendLine($"  AND RESERVE_DETAIL.REAL_START_DT = '{realStartDt}' ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 予約テーブルを更新する（処理区分：返却）
        ''' </summary>
        ''' <param name="reserveId"></param>
        ''' <returns></returns>
        Private Function UpdateReservationReturn(ByVal logId As String, ByVal reserveId As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE")
            sb.AppendLine("   RESERVE")
            sb.AppendLine("SET")
            sb.AppendLine($"  RESERVE.RESERVE_STATUS = CASE ")
            sb.AppendLine($"     WHEN STS_ALL = STS_02 THEN '{CommonConst.RESERVE_STATUS_RERUENED}' ")
            sb.AppendLine($"     WHEN STS_01 > 0 THEN '{CommonConst.RESERVE_STATUS_RENTED}' ")
            sb.AppendLine($"     ELSE'{CommonConst.RESERVE_STATUS_UNRENTED}' END ")
            sb.AppendLine("  ,RESERVE.UPD_DATETIME = GETDATE() ")
            sb.AppendLine("  ,RESERVE.UPD_EMPLOYEE_CD = ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine("  ,RESERVE.UPD_EMPLOYEE_NAME = ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($" ,RESERVE.UPD_TASK = '{TASK_NAME}'")

            sb.AppendLine($" FROM   {CommonConst.TABLE_RESERVE} RESERVE")
            sb.AppendLine("INNER JOIN #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine($" ON  PROCESS_TEMP.PROCESS_LOG_ID = {logId}")
            sb.AppendLine(" INNER JOIN ")
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
            sb.AppendLine($"    INNER JOIN #PROCESS_TEMP AS TEMP ")
            sb.AppendLine($"        ON  TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine($" WHERE MAINTE.MAINTE_ID = '{nextDt.mainteId}' ")

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
        '2021/09/15 高瀬 v2.0 予約機能 END

        ''' <summary>
        ''' 資産処理履歴を登録する
        ''' </summary>
        Private Function InsertProcessLog(ByVal logId As String, ByVal taskResult As String) As String
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
            sb.AppendLine(" ,REMARK")                       '2020/02/12 戸祭 備考を履歴に登録
            sb.AppendLine(" ,LEASE_EMPLOYEE_CD")
            sb.AppendLine(" ,LEASE_EMPLOYEE_NAME")
            sb.AppendLine(" ,LEASE_DUE_DT")
            sb.AppendLine(" ,MNG_LOC_L_CD")
            sb.AppendLine(" ,LOC_L_CD")
            sb.AppendLine(" ,LOC_M_CD")
            sb.AppendLine(" ,LOC_S_CD")
            sb.AppendLine(" ,LOC_L_NAME")
            sb.AppendLine(" ,LOC_M_NAME")
            sb.AppendLine(" ,LOC_S_NAME")
            sb.AppendLine(" ,ASSET_NAME")
            sb.AppendLine(" ,MAKER")
            sb.AppendLine(" ,PRODUCT_NO")
            sb.AppendLine(" ,MODEL_NO")
            '2023/04/11 川元 v2.4 ADD START
            sb.AppendLine(" ,DUE_QTY")
            sb.AppendLine(" ,WORKER_EMPLOYEE_CD")
            sb.AppendLine(" ,WORKER_EMPLOYEE_NAME")
            sb.AppendLine(" ,WORKER_MINUTE")
            sb.AppendLine(" ,WORKER_OVERVIEW")
            sb.AppendLine(" ,WORKER_REMARK1")
            sb.AppendLine(" ,WORKER_REMARK2")
            '2023/04/11 川元 v2.4 ADD END
            sb.AppendLine(" ,TASK_RESULT")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")
            sb.AppendLine(")")
            sb.AppendLine("SELECT")
            sb.AppendLine("  PROCESS_TEMP.PROCESS_DT ")
            sb.AppendLine(" ,PROCESS_TEMP.PROCESS_TM ")
            sb.AppendLine(" ,PROCESS_TEMP.ASSET_NO ")
            sb.AppendLine(" ,PROCESS_TEMP.ASSET_ID ")
            sb.AppendLine(" ,PROCESS_TEMP.PROCESS_TOOL_DIV ")
            sb.AppendLine(" ,PROCESS_TEMP.PROCESS_DIV ")
            sb.AppendLine(" ,PROCESS_TEMP.LEASE_PLACE_CD")
            sb.AppendLine(" ,PROCESS_TEMP.LEASE_PLACE_NAME")
            sb.AppendLine(" ,PROCESS_TEMP.REMARK")                  '2020/02/12 戸祭 備考を履歴に登録
            sb.AppendLine(" ,PROCESS_TEMP.LEASE_EMPLOYEE_CD")
            sb.AppendLine(" ,PROCESS_TEMP.LEASE_EMPLOYEE_NAME")
            sb.AppendLine(" ,CASE ISNULL(PROCESS_TEMP.LEASE_DUE_DT, '') WHEN '' THEN NULL ELSE PROCESS_TEMP.LEASE_DUE_DT END ") '2020/03/23 戸祭 貸出予定日が""の場合に0がセットされる不具合対応
            sb.AppendLine(" ,CASE ISNULL(PROCESS_TEMP.MNG_LOC_L_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.MNG_LOC_L_CD END ")
            sb.AppendLine(" ,CASE ISNULL(PROCESS_TEMP.LOC_L_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.LOC_L_CD END ")
            sb.AppendLine(" ,CASE ISNULL(PROCESS_TEMP.LOC_M_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.LOC_M_CD END ")
            sb.AppendLine(" ,CASE ISNULL(PROCESS_TEMP.LOC_S_CD, '') WHEN '' THEN ' ' ELSE PROCESS_TEMP.LOC_S_CD END ")
            sb.AppendLine(" ,PROCESS_TEMP.LOC_L_NAME ")
            sb.AppendLine(" ,PROCESS_TEMP.LOC_M_NAME ")
            sb.AppendLine(" ,PROCESS_TEMP.LOC_S_NAME ")
            sb.AppendLine(" ,PROCESS_TEMP.ASSET_NAME")
            sb.AppendLine(" ,PROCESS_TEMP.MAKER")
            sb.AppendLine(" ,PROCESS_TEMP.PRODUCT_NO")
            sb.AppendLine(" ,PROCESS_TEMP.MODEL_NO")
            '2023/04/11 川元 v2.4 ADD START
            sb.AppendLine(" ,PROCESS_TEMP.DUE_QTY")
            sb.AppendLine(" ,ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine(" ,ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine(" ,PROCESS_TEMP.WORKER_MINUTE")
            sb.AppendLine(" ,PROCESS_TEMP.WORKER_OVERVIEW")
            sb.AppendLine(" ,PROCESS_TEMP.WORKER_REMARK1")
            sb.AppendLine(" ,PROCESS_TEMP.WORKER_REMARK2")
            '2023/04/11 川元 v2.4 ADD END
            sb.AppendLine($" ,'{taskResult}'")
            sb.AppendLine(" ,GETDATE()")
            sb.AppendLine(" ,ISNULL(PROCESS_TEMP.EMPLOYEE_CD, ' ') ")
            sb.AppendLine(" ,ISNULL(PROCESS_TEMP.EMPLOYEE_NAME, ' ') ")
            sb.AppendLine($" ,'{TASK_NAME}'")
            sb.AppendLine("FROM")
            sb.AppendLine(" #PROCESS_TEMP AS PROCESS_TEMP")
            sb.AppendLine("WHERE")
            sb.AppendLine($" PROCESS_TEMP.PROCESS_LOG_ID = {logId} ")
            sb.AppendLine("ORDER BY")
            sb.AppendLine(" PROCESS_TEMP.PROCESS_LOG_ID")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        '2020/02/12 v1.2 戸祭 貸出上限機能 START
        ''' <summary>
        ''' 返却の処理履歴を更新する
        ''' </summary>
        ''' <param name="assetNo">資産番号</param>
        ''' <param name="dt">処理日</param>
        ''' <param name="tm">処理時間</param>
        Private Function UpdateProcessLogReturn(ByVal assetNo As String, ByVal dt As String, ByVal tm As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("UPDATE ")
            sb.AppendLine("  T_PROCESS_LOG")
            sb.AppendLine("SET")
            sb.AppendLine("    LEASE_PLACE_CD = BEF.LEASE_PLACE_CD")
            sb.AppendLine(",   LEASE_PLACE_NAME = BEF.LEASE_PLACE_NAME")
            sb.AppendLine(",   LEASE_EMPLOYEE_CD = BEF.LEASE_EMPLOYEE_CD")
            sb.AppendLine(",   LEASE_EMPLOYEE_NAME = BEF.LEASE_EMPLOYEE_NAME")
            sb.AppendLine(",   LEASE_DUE_DT = BEF.LEASE_DUE_DT")
            sb.AppendLine(",   REMARK = BEF.REMARK ")
            sb.AppendLine(",   LEASE_DT = BEF.PROCESS_DT ")
            sb.AppendLine("FROM")
            sb.AppendLine("	T_PROCESS_LOG AS LOG")
            sb.AppendLine("	LEFT JOIN ")
            sb.AppendLine("	(")
            sb.AppendLine("	    SELECT")                             '最新の貸出・返却情報を取得
            sb.AppendLine("	         ASSET_NO")
            sb.AppendLine($"        ,CASE PROCESS_DIV WHEN '{CommonConst.PROCESS_DIV_BO}' THEN PROCESS_DT ELSE NULL END AS PROCESS_DT")
            sb.AppendLine($"        ,CASE PROCESS_DIV WHEN '{CommonConst.PROCESS_DIV_BO}' THEN LEASE_PLACE_CD ELSE NULL END AS LEASE_PLACE_CD")
            sb.AppendLine($"        ,CASE PROCESS_DIV WHEN '{CommonConst.PROCESS_DIV_BO}' THEN LEASE_PLACE_NAME ELSE NULL END AS LEASE_PLACE_NAME")
            sb.AppendLine($"        ,CASE PROCESS_DIV WHEN '{CommonConst.PROCESS_DIV_BO}' THEN REMARK ELSE NULL END AS REMARK")
            sb.AppendLine($"        ,CASE PROCESS_DIV WHEN '{CommonConst.PROCESS_DIV_BO}' THEN LEASE_EMPLOYEE_CD ELSE NULL END AS LEASE_EMPLOYEE_CD")
            sb.AppendLine($"        ,CASE PROCESS_DIV WHEN '{CommonConst.PROCESS_DIV_BO}' THEN LEASE_EMPLOYEE_NAME ELSE NULL END AS LEASE_EMPLOYEE_NAME")
            sb.AppendLine($"        ,CASE PROCESS_DIV WHEN '{CommonConst.PROCESS_DIV_BO}' THEN LEASE_DUE_DT ELSE NULL END AS LEASE_DUE_DT")
            sb.AppendLine("	    FROM")
            sb.AppendLine("	    (")
            sb.AppendLine("	    SELECT TOP 1 ")                             '最新の貸出・返却情報を取得
            sb.AppendLine("	         ASSET_NO")
            sb.AppendLine("	        ,PROCESS_DIV")
            sb.AppendLine("         ,PROCESS_DT")
            sb.AppendLine("         ,LEASE_PLACE_CD")
            sb.AppendLine("         ,LEASE_PLACE_NAME")
            sb.AppendLine("         ,REMARK")
            sb.AppendLine("         ,LEASE_EMPLOYEE_CD")
            sb.AppendLine("         ,LEASE_EMPLOYEE_NAME")
            sb.AppendLine("         ,LEASE_DUE_DT")
            sb.AppendLine("	    FROM")
            sb.AppendLine("	         T_PROCESS_LOG")
            sb.AppendLine("	    WHERE")
            sb.AppendLine($"	        ASSET_NO = '{assetNo}'")
            sb.AppendLine($"	    AND PROCESS_DIV IN( '{CommonConst.PROCESS_DIV_BO}', '{CommonConst.PROCESS_DIV_BI}')")
            sb.AppendLine($"	    AND TASK_RESULT = '{CommonConst.TASK_RESULT_N }'")
            sb.AppendLine("	        AND (")
            sb.AppendLine($"	             PROCESS_DT < {dt}")
            sb.AppendLine("	             OR (")
            sb.AppendLine($"	                 PROCESS_DT = {dt}")
            sb.AppendLine($"	             AND PROCESS_TM < '{tm}' ")
            sb.AppendLine("	                 )")
            sb.AppendLine("	            )")
            sb.AppendLine("	    ORDER BY")
            sb.AppendLine("	         PROCESS_DT DESC")
            sb.AppendLine("	        ,PROCESS_TM DESC")
            sb.AppendLine("	    ) TMP")
            sb.AppendLine("	) BEF")
            sb.AppendLine("		ON")
            sb.AppendLine("			BEF.ASSET_NO = LOG.ASSET_NO")
            sb.AppendLine("WHERE")
            sb.AppendLine("    LOG.PROCESS_LOG_ID IN (SELECT IDENT_CURRENT('T_PROCESS_LOG')) ")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function
        '2020/02/12 v1.2 戸祭 貸出上限機能 END

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

        ''' <summary>
        ''' 同じ資産IDを使っている資産番号を入手する。
        ''' </summary>
        ''' <returns></returns>
        Private Function GetExistAssetId() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 ASSET_NO")
            sb.AppendLine("FROM")
            sb.AppendLine("  M_ASSET")
            sb.AppendLine("WHERE")
            sb.AppendLine("    ASSET_ID = @AssetId")
            sb.AppendLine("AND ASSET_NO <> @AssetNo")

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

        '2021/09/15 高瀬 v2.0 予約機能 START
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
        ''' 貸出可能データ存在チェック
        ''' </summary>
        ''' <param name="db"></param>
        ''' <param name="targetDate"></param>
        ''' <param name="assetNo"></param>
        ''' <returns></returns>
        Private Function ExistsReservationDate(ByVal db As SystemDBContext _
                , processDt As String, ByVal targetDate As String, ByVal assetNo As String) As Boolean
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 COUNT(RD.RESERVE_ID)")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.TABLE_RESERVE} AS R")
            sb.AppendLine("INNER JOIN")
            sb.AppendLine($"{CommonConst.TABLE_RESERVE_DETAIL} AS RD")
            sb.AppendLine("ON")
            sb.AppendLine("  R.RESERVE_ID = RD.RESERVE_ID")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    RD.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_UNRENTED}'")
            If (processDt.CompareTo(targetDate) < 0) Then
                sb.AppendLine($"AND {targetDate} >= RD.REAL_START_DT")
                sb.AppendLine($"AND {processDt}  <= RD.REAL_END_DT")
            Else
                sb.AppendLine($"AND {processDt}  >= RD.REAL_START_DT")
                sb.AppendLine($"AND {targetDate} <= RD.REAL_END_DT")
            End If
            sb.AppendLine($"AND ASSET_NO = '{assetNo}'")

            logger.Info(vbCrLf & sb.ToString)

            Dim rsvCount As Integer() = db.Database.SqlQuery(Of Integer)(sb.ToString).ToArray()
            Dim count As Integer = rsvCount(0)

            If count = 0 Then
                Return False
            End If

            Return True

        End Function

        ''' <summary>
        ''' 更新用データを予約テーブル・予約明細テーブルから取得する
        ''' </summary>
        ''' <param name="targetDate"></param>
        ''' <param name="assetNo"></param>
        ''' <returns></returns>
        Private Function GetReservationDate(processDt As String, ByVal targetDate As String, ByVal assetNo As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine("	 R.RESERVE_ID         AS reserveId")
            sb.AppendLine("	,R.LEASE_PLACE_CD     AS leasePlaceCd")
            sb.AppendLine("	,PLA.LEASE_PLACE_NAME AS leasePlaceNm")
            sb.AppendLine("	,R.REMARK_INTERNAL    AS remark")
            sb.AppendLine(" ,RD.ASSET_NO          AS assetNo")
            sb.AppendLine(" ,RD.REAL_START_DT     AS realStartDt")
            sb.AppendLine("	,RD.CHARGE_EMP_CD     AS changeEmpCd")
            sb.AppendLine("	,EMP.EMPLOYEE_NAME    AS changeEmpNm")
            sb.AppendLine("	,RD.REAL_END_DT       AS realEndDt")
            sb.AppendLine("FROM")
            sb.AppendLine($"{CommonConst.TABLE_RESERVE} AS R")
            sb.AppendLine("INNER JOIN")
            sb.AppendLine($"{CommonConst.TABLE_RESERVE_DETAIL} AS RD")
            sb.AppendLine("ON")
            sb.AppendLine("  R.RESERVE_ID = RD.RESERVE_ID")
            sb.AppendLine("	LEFT OUTER JOIN M_LEASE_PLACE AS PLA")
            sb.AppendLine("		ON R.LEASE_PLACE_CD = PLA.LEASE_PLACE_CD ")
            sb.AppendLine("	LEFT OUTER JOIN M_EMPLOYEE AS EMP")
            sb.AppendLine("		ON RD.CHARGE_EMP_CD = EMP.EMPLOYEE_CD")
            sb.AppendLine("WHERE")
            sb.AppendLine($"    RD.RESERVE_STATUS = '{CommonConst.RESERVE_STATUS_UNRENTED}'")
            If (processDt.CompareTo(targetDate) < 0) Then
                sb.AppendLine($"AND {targetDate} >= RD.REAL_START_DT")
                sb.AppendLine($"AND {processDt}  <= RD.REAL_END_DT")
            Else
                sb.AppendLine($"AND {processDt}  >= RD.REAL_START_DT")
                sb.AppendLine($"AND {targetDate} <= RD.REAL_END_DT")
            End If
            sb.AppendLine($"AND ASSET_NO = '{assetNo}'")
            sb.AppendLine("ORDER BY")
            sb.AppendLine(" R.REAL_START_DT")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 対象日取得のストアドファンクション呼出
        ''' </summary>
        ''' <returns></returns>
        Private Function GetTargetDate(db As SystemDBContext, processDt As String, info As ReservationFunctionInfo) As Integer
            Dim targetDate As Integer? = Nothing

            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine($"  dbo.TARGET_DATE_SF({processDt}, {info.leadTime}, '{info.dateDiv}')")

            logger.Info(vbCrLf & sb.ToString)

            Dim rsvDt As Integer?() = db.Database.SqlQuery(Of Integer?)(sb.ToString).ToArray()
            targetDate = rsvDt(0)

            'nullが返却された場合はシステム日付を返す
            If targetDate Is Nothing Then
                targetDate = Date.Today.ToShortDateString.Replace("/", "")
            End If

            Return targetDate

        End Function
        '2021/09/15 高瀬 v2.0 予約機能 END

        '2022/07/25 佐藤 v2.2 拠点ロケ更新 START
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
        ''' ロケ大区分コード存在チェック
        ''' </summary>
        ''' <param name="prmDb"></param>
        ''' <param name="prmLocLCD">ロケ大区分コード</param>
        ''' <returns></returns>
        Private Function ExistLocLCd(ByRef prmDb As SystemDBContext, ByVal prmLocLCD As String) As Boolean
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("SELECT")
            sb.AppendLine($" LOC_L_CD ")
            sb.AppendLine("FROM")
            sb.AppendLine($" {CommonConst.MASTER_LOCATION}")
            sb.AppendLine("WHERE")
            sb.AppendLine($"  LOC_DIV = '{CommonConst.LOC_L_DIV}'")
            sb.AppendLine($"  AND LOC_L_CD = '{prmLocLCD}'")

            Dim locLCodes As String() = prmDb.Database.SqlQuery(Of String)(sb.ToString).ToArray()

            If IsNothing(locLCodes) OrElse locLCodes.Length <= 0 Then
                'レコードが存在しない場合は拠点ロケ更新なし
                Return False
            End If

            Return True
        End Function
        '2022/07/25 佐藤 v2.2 拠点ロケ更新 END

        '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 ADD START
        ''' <summary>
        ''' 資産マスタ(ロケ大区分、または管理ロケ大区分毎の登録件数)を取得する
        ''' </summary>
        ''' <returns>クエリ</returns>
        Private Function GetAssetLocLCodeTotal() As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.Append("SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED ")
            sb.Append("SELECT ")
            sb.Append("  MNG_LOC_L_CD AS LOC_L_CD")
            sb.Append(" ,COUNT(*) AS CNT ")
            sb.Append("FROM ")
            sb.Append("  M_ASSET ")
            sb.Append("WHERE ")
            sb.Append("  ASSET_STATUS <> '" & CommonConst.ASSET_STATUS_RETIRE & "' ")
            sb.Append("GROUP BY ")
            sb.Append("  MNG_LOC_L_CD ")
            sb.Append("UNION ")
            sb.Append("SELECT ")
            sb.Append("  LOC_L_CD")
            sb.Append(" ,COUNT(*) AS CNT ")
            sb.Append("FROM ")
            sb.Append("  M_ASSET ")
            sb.Append("WHERE ")
            sb.Append("  ASSET_STATUS <> '" & CommonConst.ASSET_STATUS_RETIRE & "' ")
            sb.Append("GROUP BY ")
            sb.Append("  LOC_L_CD ")

            Return sb.ToString

        End Function
        '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 ADD END

        '2023/04/11 川元 v2.4 ADD START
        ''' <summary>
        ''' 資産処理履歴を登録する
        ''' </summary>
        Private Function InsertDrugDueLog(ByVal item As EntryProcessTmpDataDetail, 
                                          ByVal useQty As String,
                                          ByVal remainQty As String) As String
            Dim sb As StringBuilder = New StringBuilder()

            sb.AppendLine("INSERT INTO T_DRUG_DUE_LOG")
            sb.AppendLine("(")
            sb.AppendLine("  DRUG_WORK_DIV")
            sb.AppendLine(" ,PROCESS_DT")
            sb.AppendLine(" ,PROCESS_TM")
            sb.AppendLine(" ,ASSET_NO")
            sb.AppendLine(" ,WORKER_EMPLOYEE_CD")
            sb.AppendLine(" ,WORKER_EMPLOYEE_NAME")
            sb.AppendLine(" ,RESPONSE_EMPLOYEE_CD")
            sb.AppendLine(" ,RESPONSE_EMPLOYEE_NAME")
            sb.AppendLine(" ,USE_QTY")
            sb.AppendLine(" ,DUE_QTY")
            sb.AppendLine(" ,REMAIN_QTY")
            sb.AppendLine(" ,WORK_OUTLINE")
            sb.AppendLine(" ,WORK_TIME")
            sb.AppendLine(" ,REMARK1")
            sb.AppendLine(" ,REMARK2")
            sb.AppendLine(" ,UPD_DATETIME")
            sb.AppendLine(" ,UPD_EMPLOYEE_CD")
            sb.AppendLine(" ,UPD_EMPLOYEE_NAME")
            sb.AppendLine(" ,UPD_TASK")
            sb.AppendLine(") VALUES (")
            sb.AppendLine($" '{CommonConst.DRUG_WORK_DIV.RET}'")
            sb.AppendLine($",'{item.processDt}'")
            sb.AppendLine($",'{item.processTm}'")
            sb.AppendLine($",'{item.assetNo}'")
            sb.AppendLine($",'{item.employeeCd}'")
            sb.AppendLine($",'{item.employeeName}'")
            sb.AppendLine(" ,NULL")
            sb.AppendLine(" ,NULL")
            sb.AppendLine($" ,{useQty}")
            sb.AppendLine($",'{item.DueQty}'")
            sb.AppendLine($" ,{remainQty}")
            If item.WorkerOverview Is Nothing Then
                sb.AppendLine(" ,NULL")
            Else
                sb.AppendLine($",'{item.WorkerOverview}'")
            End If
            If item.WorkerMinute Is Nothing Then
                sb.AppendLine(" ,NULL")
            Else
                sb.AppendLine($", {item.WorkerMinute}")
            End If
            If item.WorkerRemark1 Is Nothing Then
                sb.AppendLine(" ,NULL")
            Else
                sb.AppendLine($",'{item.WorkerRemark1}'")
            End If
            If item.WorkerRemark2 Is Nothing Then
                sb.AppendLine(" ,NULL")
            Else
                sb.AppendLine($",'{item.WorkerRemark2}'")
            End If
            sb.AppendLine(" ,GetDate()")
            sb.AppendLine($",'{item.employeeCd}'")
            sb.AppendLine($",'{item.employeeName}'")
            sb.AppendLine($",'{CommonConst.TASK_RETURN_REGIST}'")
            sb.AppendLine(")")

            logger.Info(vbCrLf & sb.ToString)

            Return sb.ToString
        End Function

        ''' <summary>
        ''' 危険物の指定数量の単位(DATA3)の取得
        ''' <param name="prmProperty">性質</param>
        ''' <returns>危険物の指定数量の単位("L" or "KG")</returns>
        ''' </summary>
        Private Function GetDrugsFSActNumberUnit(db As SystemDBContext, ByVal prmProperty As String) As String
            Dim isRet As String = String.Empty

            Dim sbSQL As StringBuilder = New StringBuilder
            sbSQL.AppendLine("SELECT ")
            sbSQL.AppendLine("  DATA3")
            sbSQL.AppendLine("FROM")
            sbSQL.AppendLine("  M_SETTING")
            sbSQL.AppendLine("WHERE")
            sbSQL.AppendLine("  KEY_DIV = '" & CommonConst.KEY_DRUGS_FSACT_NUMBER & "'")
            sbSQL.AppendLine("  AND KEY2 = '**'")
            sbSQL.AppendLine("  AND KEY3 = '**'")
            sbSQL.AppendLine("  AND DATA1 = '" & prmProperty & "'")
            isRet = db.Database.SqlQuery(Of String)(sbSQL.ToString).FirstOrDefault

            Return isRet
        End Function

        ''' <summary>
        ''' 容器重量の算出
        ''' </summary>
        ''' <param name="prmKg">内容量(kg)</param>
        ''' <param name="prmL">内容量(L)</param>
        ''' <param name="prmSpecificGravity">比重</param>
        ''' <param name="prmWeightIncludContainer">受入時の容器を含む重量</param>
        ''' <returns>容器重量</returns>
        Private Function CalsPackageQty(ByVal prmKg As String,
                                       ByVal prmL As String,
                                       ByVal prmSpecificGravity As String,
                                       ByVal prmWeightIncludContainer As String) As String
            CalsPackageQty = String.Empty
            Dim qty As Decimal = 0

            'kgとLのどちらを使用するか判定
            If String.IsNullOrEmpty(prmKg) Then
                '◆L
                '受入時の容器を含む重量 - ( 内容量(L) * 比重 ) 
                If Not String.IsNullOrEmpty(prmL) Then
                    qty = Decimal.Parse(prmWeightIncludContainer) - (Decimal.Parse(prmL) * Decimal.Parse(prmSpecificGravity))
                End If

            Else
                '◆kg
                '受入時の容器を含む重量 - 内容量(kg) 
                qty = Decimal.Parse(prmWeightIncludContainer) - Decimal.Parse(prmKg)
            End If

            CalsPackageQty = Math.Round(qty, 2, MidpointRounding.AwayFromZero).ToString

        End Function


        ''' <summary>
        ''' 消費割合の算出
        ''' </summary>
        ''' <param name="prmKg">内容量(kg)</param>
        ''' <param name="prmL">内容量(L)</param>
        ''' <param name="prmSpecificGravity">比重</param>
        ''' <param name="prmWeightIncludContainer">受入時の容器を含む重量</param>
        ''' <param name="prmWeight">残量</param>
        ''' <returns>消費割合</returns>
        Private Function CalsConsumptionRate(ByVal prmKg As String,
                                             ByVal prmL As String,
                                             ByVal prmSpecificGravity As String,
                                             ByVal prmWeightIncludContainer As String,
                                             ByVal prmWeight As String) As String
            CalsConsumptionRate = String.Empty
            Dim rate As Decimal = 0

            'kgとLのどちらを使用するか判定
            If String.IsNullOrEmpty(prmKg) Then
                '◆L
                '残量 / ( 内容量(L) * 比重 )
                If Not String.IsNullOrEmpty(prmL) Then
                    rate = Decimal.Parse(prmWeight) / (Decimal.Parse(prmL) * Decimal.Parse(prmSpecificGravity))
                End If

            Else
                '◆kg
                '残量 / 内容量(kg)
                rate = Decimal.Parse(prmWeight) / Decimal.Parse(prmKg)
            End If

            '小数点2位以下を切り捨て
            'CalsConsumptionRate = Math.Round(rate, 2, MidpointRounding.AwayFromZero).ToString
            CalsConsumptionRate = (Math.Floor(rate * 10) / 10).ToString

        End Function
        '2023/04/11 川元 v2.4 ADD END

#End Region

    End Class

    ''' <summary>
    ''' 処理データ一時表情報
    ''' </summary>
    Public Class EntryProcessTmpDataDetail
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
        ''' 資産番号
        ''' </summary>
        Property assetNo As String
        ''' <summary>
        ''' 資産ID
        ''' </summary>
        Property assetId As String
        '2023/04/11 川元 v2.4 ADD START
        ''' <summary>
        ''' 処理区分
        ''' </summary>
        Property processToolDiv As String
        '2023/04/11 川元 v2.4 ADD END
        ''' <summary>
        ''' 処理区分
        ''' </summary>
        Property processDiv As String
        ''' <summary>
        ''' 貸出先場所コード
        ''' </summary>
        Property leasePlaceCd As String
        ''' <summary>
        ''' 貸出先場所名
        ''' </summary>
        Property leasePlaceName As String
        ''' <summary>
        ''' 貸出返却予定日
        ''' </summary>
        Property leaseDueDt As String
        ''' <summary>
        ''' ロケーション大区分コード
        ''' </summary>
        Property locLCd As String
        ''' <summary>
        ''' ロケーション中区分コード
        ''' </summary>
        Property locMCd As String
        ''' <summary>
        ''' ロケーション小区分コード
        ''' </summary>
        Property locSCd As String
        ''' <summary>
        ''' ロケーション大区分名称
        ''' </summary>
        Property locLName As String
        ''' <summary>
        ''' ロケーション中区分名称
        ''' </summary>
        Property locMName As String
        ''' <summary>
        ''' ロケーション小区分名称
        ''' </summary>
        Property locSName As String
        ''' <summary>
        ''' 資産状態
        ''' </summary>
        Property assetStatus As String
        ''' <summary>
        ''' 資産名称
        ''' </summary>
        Property assetName As String
        ''' <summary>
        ''' メーカ
        ''' </summary>
        Property maker As String
        ''' <summary>
        ''' 品番
        ''' </summary>
        Property productCd As String
        ''' <summary>
        ''' 型番
        ''' </summary>
        Property modelCd As String
        '2023/04/11 川元 v2.4 ADD START
        ''' <summary>
        ''' 返却重量
        ''' </summary>
        Property DueQty As Decimal?
        ''' <summary>
        ''' 作業時間
        ''' </summary>
        Property WorkerMinute As Int16?
        ''' <summary>
        ''' 作業概要
        ''' </summary>
        Property WorkerOverview As String
        ''' <summary>
        ''' 返却備考1
        ''' </summary>
        Property WorkerRemark1 As String
        ''' <summary>
        ''' 返却備考2
        ''' </summary>
        Property WorkerRemark2 As String
        '2023/04/11 川元 v2.4 ADD START
        ''' <summary>
        ''' 処理日時
        ''' </summary>
        Property taskTs As String
        ''' <summary>
        ''' 貸出先担当者社員コード
        ''' </summary>
        Property leaseEmployeeCd As String
        ''' <summary>
        ''' 貸出先担当者社員名
        ''' </summary>
        Property leaseEmployeeName As String
        ''' <summary>
        ''' ログイン担当者コード
        ''' </summary>
        Property employeeCd As String
        ''' <summary>
        ''' ログイン担当者名
        ''' </summary>
        Property employeeName As String

    End Class

    '2020/02/13 戸祭 v1.2 貸出上限機能 START
    ''' <summary>
    ''' 貸出上限情報
    ''' </summary>
    Public Class LeaseUpperInfo
        ''' <summary>
        ''' 貸出数
        ''' </summary>
        Property leaseCnt As Integer
        ''' <summary>
        ''' 貸出上限数
        ''' </summary>
        Property LeaseUpperLimit As Integer?
        ''' <summary>
        ''' 貸出上限エラー区分
        ''' </summary>
        Property leastUpperErrDiv As String
    End Class
    '2020/02/13 戸祭 v1.2 貸出上限機能 END

    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 予約機能情報
    ''' </summary>
    Public Class ReservationFunctionInfo
        ''' <summary>
        ''' 予約機能有無
        ''' </summary>
        ''' <returns></returns>
        Private _reservationFunction As String
        Property reservationFunction() As String
            Get
                Return _reservationFunction
            End Get
            Set(ByVal value As String)
                If String.IsNullOrEmpty(value) Then
                    _reservationFunction = CommonConst.RESERVATION_FUNCTION_NO
                Else
                    _reservationFunction = value
                End If
            End Set
        End Property
        ''' <summary>
        ''' 予約機能の稼働日有無
        ''' </summary>
        ''' <returns></returns>
        Private _reservationDataDiv As String
        Property reservationDataDiv() As String
            Get
                Return _reservationDataDiv
            End Get
            Set(ByVal value As String)
                If String.IsNullOrEmpty(value) Then
                    _reservationDataDiv = CommonConst.DATA_DIV_J
                Else
                    If value.Equals(CommonConst.RESERVATION_FUNCTION_YES) Then
                        _reservationDataDiv = CommonConst.DATA_DIV_K
                    Else
                        _reservationDataDiv = CommonConst.DATA_DIV_J
                    End If
                End If
            End Set
        End Property


        ''' <summary>
        '''　返却日予定日から拠点へ返却するまでのリードタイム日数
        ''' </summary>
        Private _leadTime As String
        Property leadTime() As String
            Get
                Return _leadTime
            End Get
            Set(ByVal value As String)
                If String.IsNullOrEmpty(value) Then
                    _leadTime = CommonConst.DEFAULT_LEAD_TIME
                Else
                    _leadTime = value
                End If
            End Set
        End Property

        ''' <summary>
        ''' 稼働実数か実日数か
        ''' </summary>
        Private _dateDiv As String
        Property dateDiv() As String
            Get
                Return _dateDiv
            End Get
            Set(ByVal value As String)
                If String.IsNullOrEmpty(value) Then
                    _dateDiv = CommonConst.RETURN_ACTUAL_DAYS_DIV
                Else
                    _dateDiv = value
                End If
            End Set
        End Property
    End Class

    ''' <summary>
    ''' 更新用予約データ
    ''' </summary>
    Public Class ReservationData
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

    ''' <summary>
    ''' 次メンテ更新用データ
    ''' </summary>
    Public Class NextMenteUpdData
        ''' <summary>
        ''' メンテナンスID
        ''' </summary>
        ''' <returns></returns>
        Property mainteId As Long
        ''' <summary>
        ''' メンテナンス日数
        ''' </summary>
        ''' <returns></returns>
        Property mainteCnt As Integer
        ''' <summary>
        ''' 開始日
        ''' </summary>
        ''' <returns></returns>
        Property startDt As Integer
        ''' <summary>
        ''' 終了日
        ''' </summary>
        ''' <returns></returns>
        Property endDt As Integer
    End Class
    '2021/09/15 高瀬 v2.0 予約機能能 END

    '2022/07/25 佐藤 v2.2 拠点ロケ更新 START
    Public Class ReturnLocLUpdInfo
        ''' <summary>
        ''' ロケ大区分更新有無
        ''' </summary>
        ''' <returns></returns>
        Property returnLocLUpdDiv As String
    End Class
    '2022/07/25 佐藤 v2.2 拠点ロケ更新 END

    '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 START
    ''' <summary>
    ''' 管理ロケ、またはロケ大区分毎の登録件数
    ''' </summary>
    Public Class LocLCodeTotal
        ''' <summary>
        ''' 管理ロケ、またはロケ大区分
        ''' </summary>
        Property LOC_L_CD As String
        ''' <summary>
        ''' 登録件数
        ''' </summary>
        Property Cnt As Integer
    End Class
    '2023/01/31 川元 v2.3 Asetra 資産１００万件対応 END
End Namespace