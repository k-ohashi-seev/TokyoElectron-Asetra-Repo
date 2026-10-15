Imports System.IO
Imports System.Text
Imports System.Security.Cryptography
Imports System.Net.NetworkInformation

''' <summary>
''' システム名    ：ＲＦＩＤ資産管理パッケージ
''' サブシステム名：ライセンス・トライアルDLL
''' クラス名      ：AsetraLibRegist LibRegist クラス
''' 作成日        ：2020/01/02  Inoue
''' 修正日        ：0000/00/00
''' </summary>
''' <remarks></remarks>
Friend Class SubRegist

#Region "定数宣言"

    ''' <summary>
    ''' AES暗号化パラメータ関連 定数クラス
    ''' </summary>
    Private Class AESPARAM
        Public Shared BlockSize As Integer = 128
        Public Shared KeySize As Integer = 128
        Public Shared IV As String = "K6Jdw9B40M7BeGbq"
        Public Shared Key As String = "SAT0ASETRAWEBKEY"
        Public Shared Mode As CipherMode = CipherMode.CBC
        Public Shared Padding As PaddingMode = PaddingMode.PKCS7
    End Class

    ''' <summary>
    ''' BASE64文字列暗号化処理関連 定数クラス
    ''' </summary>
    Private Class BASE64CNV
        ''' <summary>BASE64変換後の文字数</summary>
        Public Shared UseLength As Integer = 22
        ''' <summary>23文字目以降に付与する任意文字列の1桁目を取得する文字位置</summary>
        Public Shared Add1Pos As Integer = 3
        ''' <summary>23文字目以降に付与する任意文字列の2桁目を取得する文字位置</summary>
        Public Shared Add2Pos As Integer = 8
        ''' <summary>23文字目以降に付与する任意文字列の3桁目を取得する文字位置</summary>
        Public Shared Add3Pos As Integer = 13
    End Class

    'プロダクト番号、プロダクトキー 区切り文字
    Private Const C_Dmt As Char = "-"c


    ''' <summary>
    ''' レジストリ関連 定数クラス
    ''' </summary>
    Private Class REGIST
        ''' <summary>レジストリ登録フォルダ</summary>
        Public Shared FolderNm As String = "Software\STSV"
        ''' <summary>レジストリ旧登録フォルダ</summary>
        Public Shared OldFolderNm As String = "Software\Asetra"

        ''' <summary>レジストリエラーっメッセージ</summary>
        Public Shared ErrMsg As String = "registry value error [{0}]"

        ''' <summary>レジストリ名　プロダクトキー</summary>
        Public Shared ProductKey As String = "ProductKey"
        ''' <summary>レジストリ　トライアル開始日</summary>
        Public Shared TrialStartDate As String = "TrialStartDate"
    End Class

    ''' <summary>試用可能日数</summary>
    Private Const C_TrialDays As Integer = 30

#End Region

    '----- パブリック関数 -----

    ''' <summary>
    ''' DB作成日より、LICENSE_KEY文字列　を作成する
    ''' （12桁のランダム文字列を生成し、AES暗号化する）
    ''' </summary>
    ''' <param name="dtDbMakeDate">DB作成日(ASETRAバージョンMのシリアルNoを複合化した日付)</param>
    ''' <returns>
    ''' LICENSE_KEY文字列 エラー時: String.Empty
    ''' </returns>
    ''' ''' <remarks>作成日 2021/11/14  Inoue</remarks>
    Friend Shared Function GetLicenseKey(dtDbMakeDate As Date) As String

        '12桁のランダム文字列を生成（簡単な方法がいいということなので、ddEMMCyy0Fdd）
        'Dim sSeiseiKey As String = dtDbMakeDate.ToString("dd") & "E" &
        '                            dtDbMakeDate.ToString("MM") & "C" &
        '                            dtDbMakeDate.ToString("yy") & "0F" &
        '                            dtDbMakeDate.ToString("dd")
        Dim dd As String = dtDbMakeDate.Day().ToString("00")
        Dim mm As String = dtDbMakeDate.Month().ToString("00")
        Dim yy As String = (dtDbMakeDate.Year() Mod 100).ToString("00")
        Dim sSeiseiKey As String = dd + "E" + mm + "C" + yy + "0F" + dd
        '取得した文字列をAES暗号化する。
        Dim byteAes As Byte() = AesEncrypt(sSeiseiKey)
        Dim sLicenseKey As String = BitConverter.ToString(byteAes)
        Return sLicenseKey

    End Function

    '2021/11/14 ランセンスキーより、プロダクトキーを求めるよう、変更 nv Inoue
    '''' <summary>
    '''' WebサーバーMACアドレスを元に、プロダクト番号を取得する。
    '''' </summary>
    '''' <returns>
    '''' プロダクト番号
    '''' </returns>
    'Friend Shared Function SubGetProductNo() As String

    '    'サーバー自身の先頭MACアドレスを取得する。
    '    Dim sMacAddr As String = GetFirstMacAddr()

    '    '取得したMACアドレスをAES暗号化する。
    '    Dim byteAes As Byte() = AesEncrypt(sMacAddr)

    '    'AES暗号化データをさらに、25桁のBASE64文字列に変換する。
    '    Dim sBase24 As String = Base64Encrypt(byteAes)

    '    Dim sProductNo As String = sBase24.Substring(0, 5) & C_Dmt &
    '                                sBase24.Substring(5, 5) & C_Dmt &
    '                                sBase24.Substring(10, 5) & C_Dmt &
    '                                sBase24.Substring(15, 5) & C_Dmt &
    '                                sBase24.Substring(20, 5)

    '    Return sProductNo

    'End Function
    ''' <summary>
    ''' WebサーバーMACアドレスを元に、プロダクト番号を取得する。
    ''' </summary>
    ''' <param name="sLicenseKey">LICENSE_KEY</param>
    ''' <returns>
    ''' プロダクト番号
    ''' </returns>
    Friend Shared Function SubGetProductNo(ByVal sLicenseKey As String) As String

        'バイト配列に変換
        Dim byteAes As Byte() = FromStringToByteArray(sLicenseKey)

        'AES暗号化データをさらに、25桁のBASE64文字列に変換する。
        Dim sBase24 As String = Base64Encrypt(byteAes)

        Dim sProductNo As String = sBase24.Substring(0, 5) & C_Dmt &
                                    sBase24.Substring(5, 5) & C_Dmt &
                                    sBase24.Substring(10, 5) & C_Dmt &
                                    sBase24.Substring(15, 5) & C_Dmt &
                                    sBase24.Substring(20, 5)

        Return sProductNo

    End Function

    ''' <summary>
    ''' プロダクト番号を元に、プロダクトキーを取得する
    ''' </summary>
    ''' <param name="sProductNo">プロダクト番号</param>
    ''' <returns>
    ''' プロダクトキー
    ''' </returns>
    Friend Shared Function SubGetProductkey(sProductNo As String) As String

        'プロダクト番号より、ハイフンを除く。
        Dim arryBase64() As String
        arryBase64 = sProductNo.Split(C_Dmt)

        'その文字列をBASE64の左から有効桁数（22文字）で、切り取り、"=="を付与する。
        Dim sBase64 As String = String.Join(String.Empty, arryBase64)
        sBase64 = sBase64.Substring(0, BASE64CNV.UseLength) & "=="

        '付与後の文字列をBASE64複合化する。
        Dim byteBase64 As Byte() = Base64Decrypt(sBase64)

        'さらにAES暗号化を解除し、MACアドレス文字列に戻す。
        Dim sMacAddr As String = AesDecrypt(byteBase64)

        '求めたMACアドレスをブロックごとに逆に並べる。(Key用文字列)
        Dim arryStr(5) As String
        arryStr(0) = sMacAddr.Substring(10, 2)
        arryStr(1) = sMacAddr.Substring(8, 2)
        arryStr(2) = sMacAddr.Substring(6, 2)
        arryStr(3) = sMacAddr.Substring(4, 2)
        arryStr(4) = sMacAddr.Substring(2, 2)
        arryStr(5) = sMacAddr.Substring(0, 2)
        Dim sKeyAddr As String = String.Join(String.Empty, arryStr)

        'Key用文字列をAES暗号化する。
        Dim byteAes As Byte() = AesEncrypt(sKeyAddr)

        'AES暗号化データをさらに、25桁のBASE64文字列に変換する。
        Dim sBase24 As String = Base64Encrypt(byteAes)

        Dim sProductKey As String = sBase24.Substring(0, 5) & C_Dmt &
                                    sBase24.Substring(5, 5) & C_Dmt &
                                    sBase24.Substring(10, 5) & C_Dmt &
                                    sBase24.Substring(15, 5) & C_Dmt &
                                    sBase24.Substring(20, 5)

        Return sProductKey

    End Function

    ''' <summary>
    ''' ライセンス確認処理
    ''' </summary>
    ''' <param name="sProductkey">プロダクトキー</param>
    ''' <returns>
    ''' チェック結果(integer)　0:プロダクトキー一致 -1:試用期限切れ 1以上:試用可能日数
    ''' </returns>
    Friend Shared Function SubCheckLicense(sProductkey As String) As Integer

        'レジストリのプロダクトキー値の取得を試みる
        Dim sRegProductkey As String = GetRegValue(REGIST.ProductKey)

        If String.IsNullOrEmpty(sRegProductkey) Then
            '-- レジストリのプロダクトキーが空文字列の場合

            Dim sTrialStartDate As String = GetRegValue(REGIST.TrialStartDate)

            If String.IsNullOrEmpty(sTrialStartDate) Then
                '--値が取得できなかった場合
                'システム日をトライアル開始日とし、レジストリ登録する。(AES暗号化後、BASE64文字列に変換)

                'AES暗号化後、BASE64文字列に変換
                Dim byteAes As Byte() = AesEncrypt(Now.Date.ToString("yyyyMMdd"))
                Dim sBase64Date As String = System.Convert.ToBase64String(byteAes)
                sBase64Date = sBase64Date.Substring(0, sBase64Date.Length - 2)      '後ろ2文字の"=="を取る

                Call SetRegValue(REGIST.TrialStartDate, sBase64Date)

                '戻り値に 試用可能日数 をセット
                Return C_TrialDays

            Else
                '--値が取得できた場合

                'BASE64、AES復号化
                sTrialStartDate = sTrialStartDate & "=="
                Dim byteBase64 As Byte() = System.Convert.FromBase64String(sTrialStartDate)
                sTrialStartDate = AesDecrypt(byteBase64)

                'トライアル開始日＋試用可能日数≦システム日　の場合、試用期限切れ。戻り値に -1 をセットし、終了する。
                Dim dtTrialStartDate As Date
                Try
                    dtTrialStartDate = New Date(CInt(sTrialStartDate.Substring(0, 4)), CInt(sTrialStartDate.Substring(4, 2)), CInt(sTrialStartDate.Substring(6, 2)))
                Catch ex As Exception
                    Throw New Exception(String.Format(REGIST.ErrMsg, REGIST.TrialStartDate))
                End Try

                If dtTrialStartDate.AddDays(C_TrialDays) <= Now.Date Then
                    Return -1
                Else
                    '以外の場合、(トライアル開始日＋試用可能日数)－システム日　を戻り値にセット
                    Dim tm As TimeSpan = (dtTrialStartDate.AddDays(C_TrialDays) - Now.Date)
                    Return CInt(tm.TotalDays)
                End If

            End If
        Else
            '-- レジストリのプロダクトキーが空文字列でない場合
            If sRegProductkey = sProductkey Then
                Return 0
            Else
                Return -1
            End If
        End If

    End Function

    ''' <summary>
    ''' 指定のプロダクトキーをレジストリに登録する。
    ''' </summary>
    ''' <param name="sProductkey">プロダクトキー</param>
    Friend Shared Sub SubRegistLicense(sProductkey As String)

        '引数のプロダクトキーをレジストリのプロダクトキー値とし、レジストリ登録する。
        Call SetRegValue(REGIST.ProductKey, sProductkey)

    End Sub

    ''' <summary>
    ''' WebサーバーMACアドレスをAES暗号化し、文字列化にした値を取得する。
    ''' </summary>
    ''' <returns>
    ''' プロダクト番号
    ''' </returns>
    Friend Shared Function SubGetMacAddrAesStr() As String

        'サーバー自身の先頭MACアドレスを取得する。
        Dim sMacAddr As String = GetFirstMacAddr()

        '取得したMACアドレスをAES暗号化する。
        Dim byteAes As Byte() = AesEncrypt(sMacAddr)

        Dim sLicenseKey As String = BitConverter.ToString(byteAes)
        Return sLicenseKey

    End Function

    ''' <summary>
    ''' 旧レジストリの内容を新レジストリに移行する
    ''' </summary>
    ''' <returns>
    ''' 0:正常終了 -1:旧レジストリキーが存在しない -2:新レジストリが存在する
    ''' </returns>
    Friend Shared Function CnvRegistry() As Integer

        Dim regkeyOld As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGIST.OldFolderNm)
        'キーが存在しない時
        If regkeyOld Is Nothing Then
            Return -1
        End If
        Dim regkeyNew As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGIST.FolderNm, False)
        'キーが存在する時
        If Not regkeyNew Is Nothing Then
            Return -2
        End If

        '値取得
        Dim sValProductKey As String = DirectCast(regkeyOld.GetValue(REGIST.ProductKey, String.Empty), String)
        Dim sValTrialStartDate As String = DirectCast(regkeyOld.GetValue(REGIST.TrialStartDate, String.Empty), String)

        If Not String.IsNullOrEmpty(sValProductKey) Then
            Call SetRegValue(REGIST.ProductKey, sValProductKey)
        End If
        If Not String.IsNullOrEmpty(sValTrialStartDate) Then
            Call SetRegValue(REGIST.TrialStartDate, sValTrialStartDate)
        End If

        Call DelOldRegAllValue()

        Return 0

    End Function


#Region "プライベート関数"

#Region "先頭MACアドレスを取得する"
    ''' <summary>
    ''' 先頭MACアドレスを取得する
    ''' </summary>
    ''' <returns>
    ''' プロダクト番号(string)
    ''' </returns>
    ''' <remarks>
    ''' 参考サイト　http://hensa40.cutegirl.jp/archives/817
    ''' </remarks>
    Private Shared Function GetFirstMacAddr() As String
        ' アダプタリストを取得する
        Dim adapters As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()

        Dim MaxPhysicalAddress As String = String.Empty     '最大のMACアドレスを対象アドレスとする

        For Each adapter As NetworkInterface In adapters

            ' 物理（MAC）アドレスの取得
            Dim phy As PhysicalAddress = adapter.GetPhysicalAddress()
            'System.Diagnostics.Debug.WriteLine("MACADDR=" & phy.ToString())

            If Not String.IsNullOrEmpty(phy.ToString) Then
                If MaxPhysicalAddress < phy.ToString Then
                    MaxPhysicalAddress = phy.ToString
                End If
            End If

            '' ネットワーク接続状態が UP のアダプタのみ表示 
            ''If (adapter.OperationalStatus = OperationalStatus.Up) Then

            'System.Diagnostics.Debug.WriteLine(adapter.Name)         ' アダプタ名
            'System.Diagnostics.Debug.WriteLine(adapter.Description)  ' アダプタの説明

            'Dim ip_prop As IPInterfaceProperties = adapter.GetIPProperties()

            '' ユニキャスト IP アドレスの取得
            'Dim addrs As UnicastIPAddressInformationCollection = ip_prop.UnicastAddresses()
            'For Each addr As UnicastIPAddressInformation In addrs
            '    System.Diagnostics.Debug.WriteLine(addr.Address.ToString())

            '    ' メモ
            '    '-----------------------------------------------------------------------------
            '    ' IPv4 : addr.Address.AddressFamily = Net.Sockets.AddressFamily.InterNetwork 
            '    ' IPv6 : addr.Address.AddressFamily = Net.Sockets.AddressFamily.InterNetwork6 
            'Next

            '' ゲートウェイ IP アドレスの取得
            'Dim gates As GatewayIPAddressInformationCollection = ip_prop.GatewayAddresses
            'For Each gate As GatewayIPAddressInformation In gates
            '    System.Diagnostics.Debug.WriteLine(gate.Address.ToString())
            'Next

            ''End If
        Next

        'System.Diagnostics.Debug.WriteLine("MAXMACADDR=" & MaxPhysicalAddress)

        Return MaxPhysicalAddress
    End Function
#End Region

#Region "文字列をAESで暗号化"

    ''' <summary>
    ''' 文字列をAESで暗号化
    ''' </summary>
    ''' <param name="text">対象文字列</param>
    ''' <returns>
    ''' AESで暗号化バイト配列(byte())
    ''' </returns>
    ''' <remarks>
    ''' 参考サイト　http://programmers.high-way.info/vb/aes.html
    ''' </remarks>
    Private Shared Function AesEncrypt(text As String) As Byte()

        ' AES暗号化サービスプロバイダ
        Dim aes As New AesCryptoServiceProvider()
        aes.BlockSize = AESPARAM.BlockSize
        aes.KeySize = AESPARAM.KeySize
        aes.IV = Encoding.UTF8.GetBytes(AESPARAM.IV)
        aes.Key = Encoding.UTF8.GetBytes(AESPARAM.Key)
        aes.Mode = AESPARAM.Mode
        aes.Padding = AESPARAM.Padding

        ' 文字列をバイト型配列に変換
        'Dim src As Byte() = Encoding.Unicode.GetBytes(text)
        Dim src As Byte() = Encoding.UTF8.GetBytes(text)

        ' 暗号化する
        Using enc As ICryptoTransform = aes.CreateEncryptor()
            Dim dest As Byte() = enc.TransformFinalBlock(src, 0, src.Length)
            Return dest
        End Using

    End Function
#End Region

#Region "バイト配列をBASE64で暗号化"

    ''' <summary>
    ''' 文字列をAESで暗号化
    ''' </summary>
    ''' <param name="dest">対象バイト配列</param>
    ''' <returns>
    ''' BASE64で暗号化後の文字列
    ''' </returns>
    ''' <remarks>
    ''' 参考サイト　http://programmers.high-way.info/vb/aes.html
    ''' </remarks>
    Private Shared Function Base64Encrypt(dest As Byte()) As String

        'BASE64文字列に変換する。
        Dim sBase64 As String = System.Convert.ToBase64String(dest)

        '後ろ2文字の"=="を取って、指定桁の3文字を付与する
        sBase64 = sBase64.Substring(0, BASE64CNV.UseLength)
        sBase64 = sBase64 & sBase64.Substring(BASE64CNV.Add1Pos - 1, 1) & sBase64.Substring(BASE64CNV.Add2Pos - 1, 1) & sBase64.Substring(BASE64CNV.Add3Pos - 1, 1)

        Return sBase64
    End Function
#End Region

#Region "バイト配列をAESで復号化"

    ''' <summary>
    ''' 文字列をAESで復号化
    ''' </summary>
    ''' <param name="src">対象バイト配列</param>
    ''' <returns>
    ''' AES復号化文字列(string)
    ''' </returns>
    ''' <remarks>
    ''' 参考サイト　http://programmers.high-way.info/vb/aes.html
    ''' </remarks>
    Private Shared Function AesDecrypt(src As Byte()) As String

        ' AES暗号化サービスプロバイダ
        Dim aes As New AesCryptoServiceProvider()
        aes.BlockSize = AESPARAM.BlockSize
        aes.KeySize = AESPARAM.KeySize
        aes.IV = Encoding.UTF8.GetBytes(AESPARAM.IV)
        aes.Key = Encoding.UTF8.GetBytes(AESPARAM.Key)
        aes.Mode = AESPARAM.Mode
        aes.Padding = AESPARAM.Padding

        ' 複号化する
        Using dec As ICryptoTransform = aes.CreateDecryptor()
            Dim dest As Byte() = dec.TransformFinalBlock(src, 0, src.Length)
            'Return Encoding.Unicode.GetString(dest)
            Return Encoding.UTF8.GetString(dest)
        End Using

    End Function
#End Region

#Region "Base64形式の文字列を複合化し、バイト型配列に変換"

    ''' <summary>
    ''' Base64形式の文字列を複合化し、バイト型配列に変換
    ''' </summary>
    ''' <param name="src">対象文字列</param>
    ''' <returns>
    ''' BASE64複合化バイト配列
    ''' </returns>
    ''' <remarks>
    ''' 参考サイト　http://programmers.high-way.info/vb/aes.html
    ''' </remarks>
    Private Shared Function Base64Decrypt(src As String) As Byte()
        Dim sBase64 As String

        '後ろ3文字を取って、"=="を付与する
        sBase64 = src.Substring(0, BASE64CNV.UseLength)
        sBase64 = sBase64 & "=="

        ' Base64形式の文字列からバイト型配列に変換
        Dim byteBase64 As Byte() = System.Convert.FromBase64String(sBase64)

        Return byteBase64
    End Function
#End Region

#Region "BitConverterを使用して文字列へ変換したByte配列を復元します"

    ''' <summary>
    ''' BitConverterを使用して文字列へ変換したByte配列を復元します
    ''' </summary>
    ''' <param name="src">対象文字列</param>
    ''' <returns>
    ''' 複合化バイト配列
    ''' </returns>
    ''' <remarks>
    ''' 参考サイト　https://blog.beaglesoft.net/entry/%3Fp%3D39
    ''' </remarks>
    Private Shared Function FromStringToByteArray(src As String) As Byte()
        Dim aryWk() As String = src.Split("-"c)
        Dim listWk As New List(Of Byte)()

        For ii As Integer = 0 To aryWk.Length - 1
            listWk.Add(Convert.ToByte(aryWk(ii), 16))
        Next

        Return listWk.ToArray
    End Function
#End Region




#Region "レジストリ内容を取得する。(HKEY_LOCAL_MACHINE\Software\STSV)"

    ''' <summary>
    ''' レジストリ内容を取得する。(HKEY_LOCAL_MACHINE\Software\STSV)
    ''' </summary>
    ''' <param name="sNm">名前</param>
    ''' <returns>
    ''' レジストリ内容(string)
    ''' </returns>
    ''' <remarks>
    ''' 参考サイト　https://dobon.net/vb/dotnet/system/registrykey.html
    ''' </remarks>
    Private Shared Function GetRegValue(sNm As String) As String

        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(REGIST.FolderNm, False)
        'キーが存在しない時
        If regkey Is Nothing Then
            Return String.Empty
        End If

        '値取得
        Dim sRet As String = DirectCast(regkey.GetValue(sNm, String.Empty), String)

        Return sRet
    End Function
#End Region

#Region "レジストリに書き込む。(HKEY_LOCAL_MACHINE\Software\STSV)"

    ''' <summary>
    ''' レジストリに書き込む。(HKEY_LOCAL_MACHINE\Software\STSV)
    ''' </summary>
    ''' <param name="sNm">名前</param>
    ''' <param name="sValue">値</param>
    ''' <remarks>
    ''' 参考サイト　https://dobon.net/vb/dotnet/system/registrykey.html
    ''' </remarks>
    Private Shared Sub SetRegValue(sNm As String, sValue As String)
        Dim regkey As Microsoft.Win32.RegistryKey = Microsoft.Win32.Registry.LocalMachine.CreateSubKey(REGIST.FolderNm, Microsoft.Win32.RegistryKeyPermissionCheck.ReadWriteSubTree)

        Call regkey.SetValue(sNm, sValue)

    End Sub
#End Region

#Region "旧レジストリを削除する。(HKEY_LOCAL_MACHINE\Software\Asetra内)"

    ''' <summary>
    ''' 旧レジストリを削除する。(HKEY_LOCAL_MACHINE\Software\Asetra内)
    ''' </summary>
    ''' <remarks>
    ''' 参考サイト　https://dobon.net/vb/dotnet/system/registrykey.html
    ''' </remarks>
    Private Shared Sub DelOldRegAllValue()
        Call Microsoft.Win32.Registry.LocalMachine.DeleteSubKeyTree(REGIST.OldFolderNm, False)

    End Sub
#End Region

#End Region

End Class
