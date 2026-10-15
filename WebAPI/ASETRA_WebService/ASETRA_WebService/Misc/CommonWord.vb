''' <summary>
''' 文言定義クラス
''' </summary>
Public Class CommonWord
    ''' <summary>
    ''' メッセージ：予期せぬエラーが発生しました。
    ''' </summary>
    Public Const MSG_EXCEPTION As String = "HM809999"

    ''' <summary>
    ''' メッセージ：DB接続エラーです。
    ''' </summary>
    Public Const MSG_ERR_DB As String = "HM809998"

    ''' <summary>
    ''' メッセージ：棚卸処理が終了しています。棚卸データを削除しますか？
    ''' </summary>
    Public Const MSG_CONFIRM_INVENTORY As String = "HM800002"

    ''' <summary>
    ''' メッセージ：マスタが最新でない恐れがあります。マスタを再取り込みしてください。
    ''' </summary>
    Public Const MSG_OLD_MST As String = "HM800001"

    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
    ''' <summary>
    ''' メッセージ：貸出上限数を超えたため貸出できなかった資産があります。
    ''' </summary>
    Public Const MSG_LEASE_UPPER_ERR As String = "HM800003"

    ''' <summary>
    ''' メッセージ：貸出上限数を超えた資産がありました。
    ''' </summary>
    Public Const MSG_LEASE_UPPER_WARN As String = "HM800004"
    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --

    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 文言CD(予約されていないため貸出できなかった資産があります。)
    ''' </summary>
    Public Const MSG_RESERVE_ERR As String = "HM800005"
    '2021/09/15 高瀬 v2.0 予約機能 END

    ''' <summary>
    ''' 文言CD(マスタに拠点ロケ情報が存在しません。)    2022/07/25 佐藤 v2.2 拠点ロケ更新
    ''' </summary>
    Public Const MSG_RETURN_LOC_L_CD_ERR As String = "HM800006"

    ''' <summary>
    ''' 文言CD(返却または保管処理でロケ毎の登録上限数を超えたため登録できません。)    2023/01/31 川元 v2.3 Asetra 資産１００万件対応
    ''' </summary>
    Public Const MSG_RETURN_UPPER_LIMIT_OVER_ERR As String = "HM800007"

    ''' <summary>
    ''' 文言CD(照合機能が利用できない設定になっているため、データは処理できません。) 
    ''' </summary>
    Public Const MSG_RETURN_NOTMATCHING_FUNCTION_ERR As String = "HM800008"

    ''' <summary>
    ''' 文言CD(品番{1}は持出できる件数を超えています。(可能数：{2}）) 
    ''' </summary>
    Public Const MSG_RETURN_TAKEOUT_OVER_ERR As String = "HM800009"

    ''' <summary>
    ''' 文言CD(資産番号{1}は他の工事で予約があり、他の品番の資産へ変更できませんでした。) 
    ''' </summary>
    Public Const MSG_RETURN_RESERV_ERR As String = "HM800018"

    ''' <summary>
    ''' 文言CD(資産番号{1}は他の予約があり、期間変更できませんでした。) 
    ''' </summary>
    Public Const MSG_RETURN_LIMIT_OVER_ERR As String = "HM800019"

    ''' <summary>
    ''' 社員が未登録です
    ''' </summary>
    Public Const MSG_ERR_EMPLOYEENOT_REGIST As String = "HM800015"

    ''' <summary>
    ''' 退職しています
    ''' </summary>
    Public Const MSG_ERR_RETIRED As String = "HM800016"

    ''' <summary>
    ''' 退職しています
    ''' </summary>
    Public Const MSG_ERR_PASSWORD As String = "HM800020"

    ''' <summary>
    ''' 作業者以外がロックした資産があります。
    ''' </summary>
    Public Const MSG_ERR_LOCK As String = "HM800021"

    ''' <summary>
    ''' 社員の所属拠点が未登録です。
    ''' </summary>
    Public Const MSG_ERR_NO_ASSIGNED_BASE As String = "HM800022"

    ''' <summary>
    ''' 実棚登録件数:
    ''' </summary>
    Public Const MSG_ERR_REGISTRATION_COUNTS As String = "HM800023"

    ''' <summary>
    ''' 文言CD（マスタが最新でない恐れがあります。マスタを再取り込みしてください。）
    ''' </summary>
    Public Const WORD_CD_OLD_MST As String = "800001"

    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 START --
    ''' <summary>
    ''' 文言CD(貸出上限数を超えたため貸出できなかった資産があります。)
    ''' </summary>
    Public Const WORD_CD_LEASE_UPPER_ERR As String = "800003"

    ''' <summary>
    ''' 文言CD(貸出上限数を超えた資産がありました。)
    ''' </summary>
    Public Const WORD_CD_LEASE_UPPER_WARN As String = "800004"
    '-- 2020/02/12 v1.2 戸祭 貸出上限機能 END --

    '2021/09/15 高瀬 v2.0 予約機能 START
    ''' <summary>
    ''' 文言CD(予約されていないため貸出できなかった資産があります。)
    ''' </summary>
    Public Const WORD_CD_RESERVE_ERR As String = "800005"
    '2021/09/15 高瀬 v2.0 予約機能 END

    ''' <summary>
    ''' 文言CD(マスタに拠点ロケ情報が存在しません。)    2022/07/25 佐藤 v2.2 拠点ロケ更新
    ''' </summary>
    Public Const WORD_CD_RETURN_LOC_L_CD_ERR As String = "800006"

    ''' <summary>
    ''' 文言CD（蓄積データの受信が完了しました。）
    ''' </summary>
    Public Const WORD_CD_RECEIVE_SUCCSE As String = "800010"

    ''' <summary>
    ''' 文言CD（マスタ送信が完了しました。）
    ''' </summary>
    Public Const WORD_CD_SEND_SUCCSE As String = "800011"

End Class
