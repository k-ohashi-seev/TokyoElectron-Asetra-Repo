Imports System
Imports System.Data.Entity
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Linq

Partial Public Class SystemDBContext
    Inherits DbContext

    ''' <summary>データベース接続フラグ</summary>
    Private _DataBaseOpenFlag As Boolean = False
    ''' <summary>データベース接続フラグ</summary>
    Public ReadOnly Property p_DataBaseOpenFlag As Boolean
        Get
            Return _DataBaseOpenFlag
        End Get
    End Property

    Public Sub New()
        MyBase.New("name=RfidAm")
        Try
            'データベース接続
            Database.Connection.Open()
            _DataBaseOpenFlag = True    '接続完了
        Catch ex As Exception
            '接続失敗 エラーログを記述
            _DataBaseOpenFlag = False
        End Try

    End Sub

    ''' <summary>
    ''' 終了処理
    ''' </summary>
    ''' <remarks>関数の最後にコールする</remarks>
    Public Sub subTerminate()
        On Error Resume Next
        Database.Connection.Close()
    End Sub

    Public Overridable Property M_ASSET As DbSet(Of M_ASSET)
    Public Overridable Property M_CATEGORY As DbSet(Of M_CATEGORY)
    Public Overridable Property M_DEPARTMENT As DbSet(Of M_DEPARTMENT)
    Public Overridable Property M_EMPLOYEE As DbSet(Of M_EMPLOYEE)
    Public Overridable Property M_EMPLOYEE_HUB As DbSet(Of M_EMPLOYEE_HUB)
    Public Overridable Property M_LEASE_PLACE As DbSet(Of M_LEASE_PLACE)
    Public Overridable Property M_LOCATION As DbSet(Of M_LOCATION)
    Public Overridable Property M_SETTING As DbSet(Of M_SETTING)
    Public Overridable Property M_WORD As DbSet(Of M_WORD)
    Public Overridable Property MV_M_WORK As DbSet(Of MV_M_WORK)
    Public Overridable Property T_INV_RESULT As DbSet(Of T_INV_RESULT)
    Public Overridable Property T_INVENTORY As DbSet(Of T_INVENTORY)
    Public Overridable Property T_OPERATION_LOG As DbSet(Of T_OPERATION_LOG)
    Public Overridable Property T_PROCESS_LOG As DbSet(Of T_PROCESS_LOG)
    Public Overridable Property WK_T_INVENTORY As DbSet(Of WK_T_INVENTORY)

    Protected Overrides Sub OnModelCreating(ByVal modelBuilder As DbModelBuilder)
        modelBuilder.Entity(Of M_ASSET)() _
            .Property(Function(e) e.PHYINV_STATUS) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_CATEGORY)() _
            .Property(Function(e) e.CAT_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_CATEGORY)() _
            .Property(Function(e) e.LEASE_LIMIT_ALERT_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_CATEGORY)() _
            .Property(Function(e) e.GENERIC_LIMIT_ALERT_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_EMPLOYEE)() _
            .Property(Function(e) e.AUTH_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_LOCATION)() _
            .Property(Function(e) e.LOC_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_WORD)() _
            .Property(Function(e) e.LANG_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_WORD)() _
            .Property(Function(e) e.TOOL_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of M_WORD)() _
            .Property(Function(e) e.WORD_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of MV_M_WORK)() _
            .Property(Function(e) e.LANG_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of MV_M_WORK)() _
            .Property(Function(e) e.TOOL_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of MV_M_WORK)() _
            .Property(Function(e) e.WORD_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of T_INV_RESULT)() _
            .Property(Function(e) e.PHYINV_STATUS) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of T_OPERATION_LOG)() _
            .Property(Function(e) e.PROCESS_TM) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of T_OPERATION_LOG)() _
            .Property(Function(e) e.PROCESS_TOOL_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of T_OPERATION_LOG)() _
            .Property(Function(e) e.PROCESS_CAT) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of T_PROCESS_LOG)() _
            .Property(Function(e) e.PROCESS_TM) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of T_PROCESS_LOG)() _
            .Property(Function(e) e.PROCESS_TOOL_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

        modelBuilder.Entity(Of T_PROCESS_LOG)() _
            .Property(Function(e) e.PROCESS_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

    End Sub
End Class
