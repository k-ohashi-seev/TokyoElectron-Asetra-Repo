Public Class frmDspProductKey
    Private Sub frmDspProductKey_Load(sender As Object, e As EventArgs) Handles MyBase.Load


    End Sub

    Private Sub cmdDsp_Click(sender As Object, e As EventArgs) Handles cmdDsp.Click
        Try
            txtProductNo.Text = AsetraLibRegist.LibRegist.GetProductNo(txtLisenceKey.Text.Trim)
            txtProductKey.Text = AsetraLibRegist.LibRegist.GetProductkey(txtProductNo.Text)
        Catch ex As Exception
            MessageBox.Show("DBのLICENSE_KEY の値が不正です。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub
End Class
