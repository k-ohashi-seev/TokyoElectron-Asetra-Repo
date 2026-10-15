Imports System
Imports System.Collections.Generic
Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Spatial
Imports Asetra.Misc

Namespace Models
    ''' <summary>
    ''' ���Y�ꗗ�p���f��
    ''' </summary>
    <Table("M_ASSET")>
    Partial Public Class AssetListModel
        <Key>
        <Required(ErrorMessage:=CommonLabel.MSG_ERR_NOT_ENTERD)>
        <StringLength(24, ErrorMessage:=CommonErrorMsgMember.MAXLENGTH_ID)>
        Public Property ASSET_NO As String

        <StringLength(128)>
        Public Property ASSET_ID As String

        <StringLength(24, ErrorMessage:=CommonErrorMsgMember.MAXLENGTH_ID)>
        Public Property SUB_ASSET_NO As String

        Public Property EPC_SIZE As Integer?

        <StringLength(64)>
        Public Property GEN_ASSET_ID As String

        <StringLength(60)>
        Public Property ASSET_DIV As String

        <StringLength(60)>
        Public Property ASSET_NAME As String

        <StringLength(10)>
        Public Property MNG_LOC_L_CD As String '2021/09/16 nv miyaji add

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property LOC_S_CD As String

        <StringLength(8)>
        Public Property CAT_L_CD As String

        <StringLength(8)>
        Public Property CAT_M_CD As String

        <StringLength(8)>
        Public Property CAT_S_CD As String

        <StringLength(60)>
        Public Property MAKER As String

        <StringLength(30)>
        Public Property PRODUCT_NO As String

        <StringLength(30)>
        Public Property MODEL_NO As String

        <StringLength(30)>
        Public Property LEASE_NO As String

        Public Property LEASE_LIMIT As Integer?

        Public Property GENERIC_LIMIT As Integer?

        <StringLength(255)>
        Public Property FREE_ITEM1 As String

        <StringLength(255)>
        Public Property FREE_ITEM2 As String

        <StringLength(255)>
        Public Property FREE_ITEM3 As String

        <StringLength(255)>
        Public Property FREE_ITEM4 As String

        <StringLength(255)>
        Public Property FREE_ITEM5 As String

        <StringLength(255)>
        Public Property FREE_ITEM6 As String

        <StringLength(255)>
        Public Property FREE_ITEM7 As String

        <StringLength(255)>
        Public Property FREE_ITEM8 As String

        <StringLength(255)>
        Public Property FREE_ITEM9 As String

        <StringLength(255)>
        Public Property FREE_ITEM10 As String

        '2023/02/08 Kawamoto ADD START
        <StringLength(255)>
        Public Property FREE_ITEM11 As String

        <StringLength(255)>
        Public Property FREE_ITEM12 As String

        <StringLength(255)>
        Public Property FREE_ITEM13 As String

        <StringLength(255)>
        Public Property FREE_ITEM14 As String

        <StringLength(255)>
        Public Property FREE_ITEM15 As String

        <StringLength(255)>
        Public Property FREE_ITEM16 As String

        <StringLength(255)>
        Public Property FREE_ITEM17 As String

        <StringLength(255)>
        Public Property FREE_ITEM18 As String

        <StringLength(255)>
        Public Property FREE_ITEM19 As String
        '2023/02/08 Kawamoto ADD END

        <StringLength(255)>
        Public Property FREE_ITEM20 As String

        <StringLength(255)>
        Public Property FREE_ITEM21 As String

        <StringLength(255)>
        Public Property FREE_ITEM22 As String

        <StringLength(255)>
        Public Property FREE_ITEM23 As String

        <StringLength(255)>
        Public Property FREE_ITEM24 As String

        <StringLength(255)>
        Public Property FREE_ITEM25 As String

        <StringLength(255)>
        Public Property FREE_ITEM26 As String

        <StringLength(255)>
        Public Property FREE_ITEM27 As String

        <StringLength(255)>
        Public Property FREE_ITEM28 As String

        <StringLength(255)>
        Public Property FREE_ITEM29 As String

        <StringLength(255)>
        Public Property FREE_ITEM30 As String

        <StringLength(255)>
        Public Property FREE_ITEM31 As String

        <StringLength(255)>
        Public Property FREE_ITEM32 As String

        <StringLength(255)>
        Public Property FREE_ITEM33 As String

        <StringLength(255)>
        Public Property FREE_ITEM34 As String

        <StringLength(255)>
        Public Property FREE_ITEM35 As String

        <StringLength(255)>
        Public Property FREE_ITEM36 As String

        <StringLength(255)>
        Public Property FREE_ITEM37 As String

        <StringLength(255)>
        Public Property FREE_ITEM38 As String

        <StringLength(255)>
        Public Property FREE_ITEM39 As String

        <StringLength(255)>
        Public Property FREE_ITEM40 As String

        <StringLength(255)>
        Public Property FREE_ITEM41 As String

        <StringLength(255)>
        Public Property FREE_ITEM42 As String

        <StringLength(255)>
        Public Property FREE_ITEM43 As String

        <StringLength(255)>
        Public Property FREE_ITEM44 As String

        <StringLength(255)>
        Public Property FREE_ITEM45 As String

        <StringLength(255)>
        Public Property FREE_ITEM46 As String

        <StringLength(255)>
        Public Property FREE_ITEM47 As String

        <StringLength(255)>
        Public Property FREE_ITEM48 As String

        <StringLength(255)>
        Public Property FREE_ITEM49 As String

        <StringLength(255)>
        Public Property FREE_ITEM50 As String

        <StringLength(255)>
        Public Property FREE_ITEM51 As String

        <StringLength(255)>
        Public Property FREE_ITEM52 As String

        <StringLength(255)>
        Public Property FREE_ITEM53 As String

        <StringLength(255)>
        Public Property FREE_ITEM54 As String

        <StringLength(255)>
        Public Property FREE_ITEM55 As String

        <StringLength(255)>
        Public Property FREE_ITEM56 As String

        <StringLength(255)>
        Public Property FREE_ITEM57 As String

        <StringLength(255)>
        Public Property FREE_ITEM58 As String

        <StringLength(255)>
        Public Property FREE_ITEM59 As String

        <StringLength(255)>
        Public Property FREE_ITEM60 As String

        <StringLength(255)>
        Public Property REMARK1 As String

        <StringLength(255)>
        Public Property REMARK2 As String

        <StringLength(255)>
        Public Property REMARK3 As String

        <StringLength(255)>
        Public Property REMARK4 As String

        <StringLength(255)>
        Public Property REMARK5 As String

        <StringLength(255)>
        Public Property REMARK6 As String

        <StringLength(255)>
        Public Property REMARK7 As String

        <StringLength(255)>
        Public Property REMARK8 As String

        <StringLength(255)>
        Public Property REMARK9 As String

        <StringLength(255)>
        Public Property REMARK10 As String

        <StringLength(255)>
        Public Property ASSET_PIC_FILENAME As String

        Public Property LEASE_CNT As Integer = 0

        Public Property ASSET_STATUS As Byte = 0

        Public Property EPC_ENC_STATUS As Byte = 0

        Public Property TAG_LINK_STATUS As Byte = 0

        Public Property LEASE_STATUS As Byte = 0 '2021/09/16 nv miyaji add

        <StringLength(15)>
        Public Property LEASE_PLACE_CD As String

        <StringLength(60)>
        Public Property LEASE_PLACE_NAME As String

        <StringLength(255)>
        Public Property REMARK As String

        <StringLength(15)>
        Public Property LEASE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property LEASE_EMPLOYEE_NAME As String

        Public Property LEASE_DT As Integer?

        Public Property LEASE_DUE_DT As Integer?

        '--2021/09/16 nv miyaji add start
        Public Property MAINTE_LEASE_NUM As Integer?

        Public Property MAINTE_LEASE_DAY As Integer?

        <StringLength(1)>
        Public Property MAINTE_LEASE_DAY_WORK_FLAG As String

        Public Property MAINTE_DAY As Integer?

        <StringLength(1)>
        Public Property MAINTE_DAY_WORK_FLAG As String
        '--2021/09/16 nv miyaji add end

        Public Property RETIRE_TS As Long?

        <StringLength(15)>
        Public Property RETIRE_EMPLOYEE_CD As String

        <StringLength(60)>
        Public Property RETIRE_EMPLOYEE_NAME As String

        <StringLength(128)>
        Public Property RETIRE_ASSET_ID As String

        Public Property PHYINV_TS As Long?

        <StringLength(1)>
        Public Property PHYINV_STATUS As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_L_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_M_CD As String

        <StringLength(10)>      '20210510 UPD Suzuki v1.3
        Public Property PHYINV_LOC_S_CD As String

        <StringLength(60)>
        Public Property PHYINV_LOC_L_NAME As String

        <StringLength(60)>
        Public Property PHYINV_LOC_M_NAME As String

        <StringLength(60)>
        Public Property PHYINV_LOC_S_NAME As String

        <StringLength(15)>
        Public Property PHYINV_EMPLOYEE_CD As String

        Public Property TASK_TS As Long?

        Public Property PHYINV_EMPLOYEE_NAME As String

        ''' <summary>
        ''' �L�`�E���`�敪
        ''' </summary>
        ''' <returns>�L�`�E���`�敪</returns>
        <StringLength(1)>
        Public Property IS_TANGIBLE As String

        ''' <summary>
        ''' �o�^����
        ''' </summary>
        ''' <returns>�o�^����</returns>
        <DataType(DataType.Date)>
        Public Property INS_DATETIME As Date? = Nothing

        ''' <summary>
        ''' �o�^�ҎЈ��R�[�h
        ''' </summary>
        ''' <returns>�o�^�ҎЈ��R�[�h</returns>
        <StringLength(15)>
        Public Property INS_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' �o�^�Ҏ���
        ''' </summary>
        ''' <returns>�o�^�Ҏ���</returns>
        <StringLength(60)>
        Public Property INS_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' �ŏI�o�^�X�V����
        ''' </summary>
        ''' <returns>�ŏI�o�^�X�V����</returns>
        <DataType(DataType.Date)>
        Public Property UPD_DATETIME As Date? = Nothing

        ''' <summary>
        ''' �ŏI�o�^�X�V�ҎЈ��R�[�h
        ''' </summary>
        ''' <returns>�ŏI�o�^�X�V�ҎЈ��R�[�h</returns>
        <StringLength(15)>
        Public Property UPD_EMPLOYEE_CD As String = String.Empty

        ''' <summary>
        ''' �ŏI�o�^�X�V�Ҏ���
        ''' </summary>
        ''' <returns>�ŏI�o�^�X�V�Ҏ���</returns>
        <StringLength(60)>
        Public Property UPD_EMPLOYEE_NAME As String = String.Empty

        ''' <summary>
        ''' �ŏI�o�^�X�V����
        ''' </summary>
        ''' <returns>�ŏI�o�^�X�V����</returns>
        <StringLength(255)>
        Public Property UPD_TASK As String = String.Empty

        ''' <summary>
        ''' �o�^���� ������
        ''' </summary>
        ''' <returns>�o�^���� ������</returns>
        <NotMapped>
        Public Property InsDatetimeString As String = String.Empty

        ''' <summary>
        ''' �ŏI�o�^�X�V���� ������
        ''' </summary>
        ''' <returns>�ŏI�o�^�X�V���� ������</returns>
        <NotMapped>
        Public Property UpdDatetimeString As String = String.Empty

        ''' <summary>
        '''  �Ǘ����P�[�V�����h���b�v�_�E�����X�g�f�[�^  '2021/10/01 nv miyaji add
        ''' </summary>
        ''' <returns>�Ǘ����P�[�V�����h���b�v�_�E�����X�g�f�[�^</returns>
        <NotMapped>
        Public Property MnglocLDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  ��敪�h���b�v�_�E�����X�g�f�[�^
        ''' </summary>
        ''' <returns>��敪�h���b�v�_�E�����X�g�f�[�^</returns>
        <NotMapped>
        Public Property locLDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  ���敪�h���b�v�_�E�����X�g�f�[�^
        ''' </summary>
        ''' <returns>���敪�h���b�v�_�E�����X�g�f�[�^</returns>
        <NotMapped>
        Public Property locMDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  ���敪�h���b�v�_�E�����X�g�f�[�^
        ''' </summary>
        ''' <returns>���敪�h���b�v�_�E�����X�g�f�[�^</returns>
        <NotMapped>
        Public Property locSDropdownData As List(Of LocationDropdownModel)

        ''' <summary>
        '''  �J�e�S���[�@�啪�ރh���b�v�_�E�����X�g�f�[�^
        ''' </summary>
        ''' <returns>�啪�ރh���b�v�_�E�����X�g�f�[�^</returns>
        <NotMapped>
        Public Property catLDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        '''  �J�e�S���[�@�����ރh���b�v�_�E�����X�g�f�[�^
        ''' </summary>
        ''' <returns>�����ރh���b�v�_�E�����X�g�f�[�^</returns>
        <NotMapped>
        Public Property catMDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        '''  �J�e�S���[�@�����ރh���b�v�_�E�����X�g�f�[�^
        ''' </summary>
        ''' <returns>�����ރh���b�v�_�E�����X�g�f�[�^</returns>
        <NotMapped>
        Public Property catSDropdownData As List(Of CategoryDropdownModel)

        ''' <summary>
        ''' ���YID�\����\��
        ''' </summary>
        ''' <returns>���YID�\����\��</returns>
        <NotMapped>
        Public Property AssetIdDivReg As Boolean = True

        ''' <summary>
        ''' ���p�����\��������
        ''' </summary>
        ''' <returns>���p�����\��������</returns>
        <NotMapped>
        Public Property RetireTsString As String = String.Empty

        ''' <summary>
        ''' ���[�X�����\��������
        ''' </summary>
        ''' <returns>���[�X�����\��������</returns>
        <NotMapped>
        Public Property LeaseLimitString As String = String.Empty

        ''' <summary>
        ''' ���������\��������
        ''' </summary>
        ''' <returns>�����\��������</returns>
        <NotMapped>
        Public Property GenLimitString As String = String.Empty

        ''' <summary>
        ''' ���Y�w�����\��������
        ''' </summary>
        ''' <returns>���Y�w�����\��������</returns>
        <NotMapped>
        Public Property AssetKounyuDateString As String = String.Empty

        ''' <summary>
        ''' �ύX�t���O
        ''' </summary>
        ''' <returns>�ύX�t���O</returns>
        <NotMapped>
        Public Property UpdateFlg As Boolean = False

        '2023/02/08 Kawamoto ADD START
        ''' <summary>
        ''' ��i�@�\�iTrue�FON, False:OFF�j
        ''' </summary>
        ''' <returns>��i�@�\�iTrue�FON, False:OFF�j</returns>
        <NotMapped>
        Public Property DrugsFunc As Boolean = False
        '2023/02/08 Kawamoto ADD END

        '2023/03/22 Kawamoto ADD START
        ''' <summary>
        ''' �Y�t�t�@�C������
        ''' </summary>
        ''' <returns>�Y�t�t�@�C������</returns>
        <NotMapped>
        Public Property ATTACHMENT_NUM As Integer?
        '2023/03/22 Kawamoto ADD END

        '2023/05/10 Kawamoto ADD START
        ''' <summary>
        ''' FreeItem3(���h�@�̕���)�̊댯���̎w�萔�ʂ̒P��
        ''' </summary>
        ''' <returns>FreeItem3(���h�@�̕���)�̊댯���̎w�萔�ʂ̒P��</returns>
        <NotMapped>
        Public Property Unit As String

        ''' <summary>
        ''' �v����A�g����̌v����
        ''' </summary>
        ''' <returns>�v����A�g����̌v����</returns>
        <NotMapped>
        Public Property WeighingMachineLinkList As List(Of String)
        '2023/05/10 Kawamoto ADD END
    End Class

End Namespace
