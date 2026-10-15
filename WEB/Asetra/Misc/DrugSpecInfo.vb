Namespace Misc
    ''' <summary>
    ''' 指定数量管理表情報クラス
    ''' </summary>
    Public Class DrugSpecInfo
        '-----インスタンス変数-----
        ''' <summary>資産番号</summary>
        Private _sAssetNo As String
        ''' <summary>資産名</summary>
        Private _sAssetNm As String
        ''' <summary>CAS.NO</summary>
        Private _sCasNo As String
        ''' <summary>消防法の分類</summary>
        Private _sFireServiceAct As String
        ''' <summary>有規則の該当有無</summary>
        Private _sOrganicRule As String
        ''' <summary>毒物及び劇物取締法の該当</summary>
        Private _sPoisonuosRule As String
        ''' <summary>特化則の該当有無</summary>
        Private _sSepcificRule As String
        ''' <summary>PRTR制度の該当有無</summary>
        Private _sPRTR As String
        ''' <summary>化学物質ｱｾｽﾒﾝﾄ実施義務の有無</summary>
        Private _sChemicalSubstance As String
        ''' <summary>購入者</summary>
        Private _sPurchaser As String
        ''' <summary>内容量(kg)</summary>
        Private _sInternalCapKg As Decimal?
        ''' <summary>内容量(L)</summary>
        Private _sInternalCapL As Decimal?
        ''' <summary>比重</summary>
        Private _sSpecificGravity As Decimal?
        ''' <summary>指定数量(L)</summary>
        Private _sSpecifiedQtyL As Decimal?
        ''' <summary>納入日</summary>
        Private _sDueDate As String
        ''' <summary>受入れ時の容器を含む重量(kg)</summary>
        Private _sInitWeight As Decimal?
        ''' <summary>保管場所</summary>
        Private _sStorageLoc As String
        ''' <summary>指定数量（ロケマスタ）</summary>
        Private _dLocSpecifiedQty As Decimal?
        ''' <summary>重量履歴情報</summary>
        Private _lstDueHist As New List(Of DueHist)

        '-----プロパティ-----
        ''' <summary>資産番号</summary>
        Public Property AssetNo() As String
            Get
                Return _sAssetNo
            End Get
            Set(ByVal value As String)
                _sAssetNo = value
            End Set
        End Property
        ''' <summary>資産名</summary>
        Public Property AssetNm() As String
            Get
                Return _sAssetNm
            End Get
            Set(ByVal value As String)
                _sAssetNm = value
            End Set
        End Property
        ''' <summary>CAS.NO</summary>
        Public Property CasNo() As String
            Get
                Return _sCasNo
            End Get
            Set(ByVal value As String)
                _sCasNo = value
            End Set
        End Property
        ''' <summary>消防法の分類</summary>
        Public Property FireServiceAct() As String
            Get
                Return _sFireServiceAct
            End Get
            Set(ByVal value As String)
                _sFireServiceAct = value
            End Set
        End Property
        ''' <summary>有規則の該当有無</summary>
        Public Property OrganicRule() As String
            Get
                Return _sOrganicRule
            End Get
            Set(ByVal value As String)
                _sOrganicRule = value
            End Set
        End Property
        ''' <summary>毒物及び劇物取締法の該当</summary>
        Public Property PoisonuosRule() As String
            Get
                Return _sPoisonuosRule
            End Get
            Set(ByVal value As String)
                _sPoisonuosRule = value
            End Set
        End Property
        ''' <summary>特化則の該当有無</summary>
        Public Property SpecificRule() As String
            Get
                Return _sSepcificRule
            End Get
            Set(ByVal value As String)
                _sSepcificRule = value
            End Set
        End Property
        ''' <summary>PRTR制度の該当有無</summary>
        Public Property PRTR() As String
            Get
                Return _sPRTR
            End Get
            Set(ByVal value As String)
                _sPRTR = value
            End Set
        End Property
        ''' <summary>化学物質ｱｾｽﾒﾝﾄ実施義務の有無</summary>
        Public Property ChemicalSubstance() As String
            Get
                Return _sChemicalSubstance
            End Get
            Set(ByVal value As String)
                _sChemicalSubstance = value
            End Set
        End Property
        ''' <summary>購入者</summary>
        Public Property Purchaser() As String
            Get
                Return _sPurchaser
            End Get
            Set(ByVal value As String)
                _sPurchaser = value
            End Set
        End Property
        ''' <summary>内容量(kg)</summary>
        Public Property InternalCapKg() As Decimal?
            Get
                Return _sInternalCapKg
            End Get
            Set(ByVal value As Decimal?)
                _sInternalCapKg = value
            End Set
        End Property
        ''' <summary>内容量(L)</summary>
        Public Property InternalCapL() As Decimal?
            Get
                Return _sInternalCapL
            End Get
            Set(ByVal value As Decimal?)
                _sInternalCapL = value
            End Set
        End Property
        ''' <summary>比重</summary>
        Public Property SpecificGravity() As Decimal?
            Get
                Return _sSpecificGravity
            End Get
            Set(ByVal value As Decimal?)
                _sSpecificGravity = value
            End Set
        End Property
        ''' <summary>指定数量(L)</summary>
        Public Property SpecifiedQtyL() As Decimal?
            Get
                Return _sSpecifiedQtyL
            End Get
            Set(ByVal value As Decimal?)
                _sSpecifiedQtyL = value
            End Set
        End Property
        ''' <summary>納入日</summary>
        Public Property DueDate() As String
            Get
                Return _sDueDate
            End Get
            Set(ByVal value As String)
                _sDueDate = value
            End Set
        End Property
        ''' <summary>受入れ時の容器を含む重量(kg)</summary>
        Public Property InitWeight() As Decimal?
            Get
                Return _sInitWeight
            End Get
            Set(ByVal value As Decimal?)
                _sInitWeight = value
            End Set
        End Property
        ''' <summary>保管場所</summary>
        Public Property StorageLoc() As String
            Get
                Return _sStorageLoc
            End Get
            Set(ByVal value As String)
                _sStorageLoc = value
            End Set
        End Property
        ''' <summary>指定数量（ロケマスタ）</summary>
        Public Property LocSpecifiedQty() As Decimal
            Get
                Return _dLocSpecifiedQty
            End Get
            Set(ByVal value As Decimal)
                _dLocSpecifiedQty = value
            End Set
        End Property
        ''' <summary>重量履歴情報</summary>
        Public Property LstDueHist() As List(Of DueHist)
            Get
                Return _lstDueHist
            End Get
            Set(ByVal value As List(Of DueHist))
                _lstDueHist = value
            End Set
        End Property

#Region "重量履歴クラス定義"
        Public Class DueHist
            '-----インスタンス変数-----
            ''' <summary>確認日</summary>
            Private _sProcessDt As String
            ''' <summary>重量(kg)</summary>
            Private _sDueQty As Decimal?
            ''' <summary>残量(kg)</summary>
            Private _sRemainQty As Decimal?
            ''' <summary>指定数量</summary>
            Private _sSpecifiedQty As Decimal

            '-----プロパティ-----
            ''' <summary>確認日</summary>
            Public Property ProcessDt() As String
                Get
                    Return _sProcessDt
                End Get
                Set(ByVal value As String)
                    _sProcessDt = value
                End Set
            End Property
            ''' <summary>重量(kg)</summary>
            Public Property DueQty() As Decimal?
                Get
                    Return _sDueQty
                End Get
                Set(ByVal value As Decimal?)
                    _sDueQty = value
                End Set
            End Property
            ''' <summary>残量(kg)</summary>
            Public Property RemainQty() As Decimal?
                Get
                    Return _sRemainQty
                End Get
                Set(ByVal value As Decimal?)
                    _sRemainQty = value
                End Set
            End Property
            ''' <summary>指定数量</summary>
            Public Property SpecifiedQty() As Decimal
                Get
                    Return _sSpecifiedQty
                End Get
                Set(ByVal value As Decimal)
                    _sSpecifiedQty = value
                End Set
            End Property
        End Class
#End Region

#Region "重量履歴リスト日付存在チェック"
        ''' <summary>
        ''' 重量履歴リスト日付存在チェック
        ''' </summary>
        ''' <param name="sProcessDt">確認日</param>
        ''' <returns>True:存在する False:存在しない</returns>
        Public Function ExistsDueHist(ByVal sProcessDt As String) As Boolean
            Return _lstDueHist.Exists(Function(x) x.ProcessDt = sProcessDt)
        End Function
#End Region

#Region "重量履歴データ追加"
        ''' <summary>
        ''' 重量履歴データ追加
        ''' </summary>
        ''' <param name="sProcessDt">確認日</param>
        ''' <param name="sDueQty">重量</param>
        ''' <param name="sRemainQty">残量</param>
        ''' <param name="sSpecificGravity">比重</param>
        ''' <param name="sSpecificQtyL">指定数量(L)</param>
        Public Sub AddDueHist(ByVal sProcessDt As String, ByVal sDueQty As String, ByVal sRemainQty As String, ByVal sSpecificGravity As String, ByVal sSpecificQtyL As String)
            '存在しなければ追加
            If _lstDueHist.Exists(Function(x) x.ProcessDt = sProcessDt) = False Then
                Dim dSpecGravity As Decimal
                Dim dSpecQtyL As Decimal
                Dim dConvertChk As Decimal

                Dim dueHist As New DueHist
                With dueHist
                    .ProcessDt = sProcessDt

                    '重量が数値であれば変換
                    If Decimal.TryParse(sDueQty, dConvertChk) Then
                        .DueQty = dConvertChk
                    End If
                    '残量が数値であれば変換
                    If Decimal.TryParse(sRemainQty, dConvertChk) Then
                        .RemainQty = dConvertChk
                    End If

                    '比重と指定数量(L)が数値の場合のみ
                    If Decimal.TryParse(sSpecificGravity, dSpecGravity) AndAlso Decimal.TryParse(sSpecificQtyL, dSpecQtyL) Then
                        '残量 / (比重 * 指定数量(L))
                        .SpecifiedQty = sRemainQty / (dSpecGravity * dSpecQtyL)
                    End If
                End With

                _lstDueHist.Add(dueHist)
            End If
        End Sub
#End Region

    End Class

End Namespace
