Imports System.Data.Entity
Imports Asetra.Models
Imports Asetra.Misc

Namespace Models
    Partial Public Class SystemDBContext
        Inherits Entity.DbContext

        Public Sub New()
            MyBase.New($"name={CommonConst.KEY_DB_CONNECTION}")
            Me.Database.CommandTimeout = 600
        End Sub

        Public Overridable Property Auth As DbSet(Of AuthModel)
        Public Overridable Property M_WORD As DbSet(Of M_WORDModel)
        Public Overridable Property EmployeeView As DbSet(Of EmployeeViewModel)
        Public Overridable Property Employee As DbSet(Of EmployeeModel)
        Public Overridable Property EmployeeCsv As DbSet(Of EmployeeCsvModel)
        Public Overridable Property SettingView As DbSet(Of SettingViewModel)
        Public Overridable Property Setting As DbSet(Of SettingModel)

        Public Overridable Property LocationDropdown As DbSet(Of LocationDropdownModel)
        Public Overridable Property InventoryInquiryView As DbSet(Of InventoryInquiryViewModel)
        Public Overridable Property InventoryInquiry As DbSet(Of InventoryInquiryModel)

        Public Overridable Property InventoryView As DbSet(Of InventoryViewModel)
        Public Overridable Property Inventory As DbSet(Of InventoryModel)
        Public Overridable Property DepartmentView As DbSet(Of DepartmentViewModel)
        Public Overridable Property Department As DbSet(Of DepartmentModel)
        Public Overridable Property LeasePlaceView As DbSet(Of LeasePlaceViewModel)
        Public Overridable Property LeasePlace As DbSet(Of LeasePlaceModel)
        Public Overridable Property CategoryDropdown As DbSet(Of CategoryDropdownModel)
        Public Overridable Property CategoryView As DbSet(Of CategoryViewModel)
        Public Overridable Property Category As DbSet(Of CategoryModel)
        Public Overridable Property CategoryCsv As DbSet(Of CategoryCsvModel)
        Public Overridable Property LocationView As DbSet(Of LocationViewModel)
        Public Overridable Property Location As DbSet(Of LocationModel)
        Public Overridable Property LocationCsv As DbSet(Of LocationCsvModel)
        Public Overridable Property LocationName As DbSet(Of LocationNameModel)
        Public Overridable Property PrinterDropdown As DbSet(Of PrinterDropdownModel)
        Public Overridable Property LocationUsage As DbSet(Of LocationUsageModel)
        Public Overridable Property StockListView As DbSet(Of StockListViewModel)
        Public Overridable Property StockList As DbSet(Of StockListModel)
        Public Overridable Property AssetListView As DbSet(Of AssetListViewModel)
        Public Overridable Property AssetList As DbSet(Of AssetListModel)
        Public Overridable Property AssetListLabel As DbSet(Of AssetListLabelModel)
        Public Overridable Property AssetLabelPrintView As DbSet(Of AssetLabelPrintViewModel)
        Public Overridable Property AssetLabelPrint As DbSet(Of AssetLabelPrintModel)
        Public Overridable Property AssetHistoryView As DbSet(Of AssetHistoryViewModel)
        Public Overridable Property AssetHistory As DbSet(Of AssetHistoryModel)
        Public Overridable Property AssetDetailView As DbSet(Of AssetDetailViewModel)
        Public Overridable Property AssetDetail As DbSet(Of AssetDetailModel)
        Public Overridable Property AssetListWork As DbSet(Of AssetListWorkModel)
        Public Overridable Property Word As DbSet(Of WordModel)
        ' 棚卸概況用
        Public Overridable Property ISInventoryView As DbSet(Of ISInventoryViewModel)
        Public Overridable Property InventorySummaryView As DbSet(Of InventorySummaryViewModel)
        Public Overridable Property InventorySummaryHedView As DbSet(Of InventorySummaryHedViewModel) '2019/10/04 nv miyaji add
        Public Overridable Property InventoryHistoryView As DbSet(Of InventoryHistoryViewModel)
        Public Overridable Property InventoryLocHistoryView As DbSet(Of InventoryLocHistoryViewModel) '20231023 ADD Start Shirakawa
        Public Overridable Property InventoryAssetHistoryView As DbSet(Of InventoryAssetHistoryViewModel)
        Public Overridable Property IASInventoryView As DbSet(Of IASInventoryViewModel)
        Public Overridable Property AssetStatusCountView As DbSet(Of AssetStatusCountViewModel)
        Public Overridable Property PhysicalInventoryCountView As DbSet(Of PhysicalInventoryCountViewModel)
        Public Overridable Property NoItemNumberView As DbSet(Of NoItemNumberViewModel)
        Public Overridable Property AssetView As DbSet(Of AssetViewModel)
        Public Overridable Property OperationLogView As DbSet(Of OperationLogViewModel)
        Public Overridable Property WordView As DbSet(Of WordViewModel)
        Public Overridable Property OperationLog As DbSet(Of OperationLogModel)
        Public Overridable Property CsvMapDetail As DbSet(Of CsvMapDetailModel)  '2019/11/29 nv miyaji add
        Public Overridable Property CsvMapDetailView As DbSet(Of CsvMapDetailViewModel)  '2019/11/29 nv miyaji add
        Public Overridable Property CsvMap As DbSet(Of CsvMapModel) '2019/12/03 nv miyaji add
        Public Overridable Property AssetLeasePlaceListView As DbSet(Of AssetLeasePlaceListViewModel) '2020/01/30 nv miyaji add
        Public Overridable Property AssetLeasePlaceDetailView As DbSet(Of AssetLeasePlaceDetailViewModel) '2020/01/30 nv miyaji add
        Public Overridable Property Gate As DbSet(Of GateModel) '2020/02/18 nv miyaji add
        Public Overridable Property GateAntenna As DbSet(Of GateAntennaModel) '2020/02/18 nv miyaji add
        Public Overridable Property GateView As DbSet(Of GateViewModel) '2020/02/18 nv miyaji add
        Public Overridable Property GatePassLog As DbSet(Of GatePassLogModel) '2020/02/25 nv miyaji add
        Public Overridable Property GatePassLogView As DbSet(Of GatePassLogViewModel) '2020/02/25 nv miyaji add
        Public Overridable Property CalendarView As DbSet(Of CalendarViewModel)             'Add 2021.08.30 NV Iwamoto
        Public Overridable Property Calendar As DbSet(Of CalendarModel)                     'Add 2021.08.30 NV Iwamoto
        Public Overridable Property LendingStatusView As DbSet(Of LendingStatusViewModel)   'Add 2021.09.13 NV Iwamoto
        Public Overridable Property EmployeeHub As DbSet(Of EmployeeHubModel)  '2021/09/16 nv miyaji add
        Public Overridable Property LendingEntryView As DbSet(Of LendingEntryViewModel)  '2021/09/27 nv miyaji add
        Public Overridable Property LendingEntry As DbSet(Of LendingEntryModel)             'Add 2021.09.13 NV Iwamoto
        Public Overridable Property ReserveDetail As DbSet(Of ReserveDetailModel)  '2021/10/11 nv miyaji add
        Public Overridable Property ReserveDetailView As DbSet(Of ReserveDetailViewModel)  '2021/10/11 nv miyaji add
        Public Overridable Property LendingListView As DbSet(Of LendingListViewModel)       'Add 2021.10.14 NV Iwamoto
        Public Overridable Property LendingMainteEntry As DbSet(Of LendingMainteEntryModel)  '2021/10/15 nv miyaji add
        Public Overridable Property LendingMainteEntryView As DbSet(Of LendingMainteEntryViewModel)  '2021/10/15 nv miyaji add
        Public Overridable Property Mail As DbSet(Of MailModel)                                 '2021/10/16 nv Inoue add
        Public Overridable Property MailTo As DbSet(Of MailToModel)                             '2021/10/16 nv Inoue add
        Public Overridable Property GoodsView As DbSet(Of GoodsViewModel)       '2021/10/07 sv suzuki add
        Public Overridable Property Goods As DbSet(Of GoodsModel)               '2021/10/07 sv suzuki add
        Public Overridable Property AssetLeaseHitoryView As DbSet(Of AssetLeaseHitoryViewModel) 'Add 2021.10.26 NV Iwamoto
        Public Overridable Property LendingHistoryView As DbSet(Of LendingHistoryViewModel)     'Add 2021.10.29 NV Iwamoto
        Public Overridable Property LendingRefListCsv As DbSet(Of LendingRefListCsvModel)       'Add 2021.11.01 NV Iwamoto
        Public Overridable Property LendingRefMainteCsv As DbSet(Of LendingRefMainteCsvModel)   'Add 2021.11.01 NV Iwamoto
        Public Overridable Property LendingHistory As DbSet(Of LendingHistoryModel)     '2021/11/04 nv miyaji add
        Public Overridable Property SearchMainteView As DbSet(Of SearchMainteViewModel)     '2021/11/10 nv Inoue add
        Public Overridable Property CheckReserveSF As DbSet(Of CheckReserveSF)                  '2022/07/25 sv sato add
        Public Overridable Property ReserveMainteInfo As DbSet(Of ReserveMainteInfoSF)        '2022/07/25 sv sato add

        Public Overridable Property AssetListLOC_L_CDModel As DbSet(Of AssetListLOC_L_CDModel)        '2023/02/10 ADD Shirakawa
        Public Overridable Property AssetListLOC_L_CDCNTModel As DbSet(Of AssetListLOC_L_CDCNTModel)        '2023/02/10 ADD Shirakawa
        Public Overridable Property AssetListMNG_LOC_L_CDCNTModel As DbSet(Of AssetListMNG_LOC_L_CDCNTModel)        '2023/02/10 ADD Shirakawa

        Public Overridable Property DrugManageListView As DbSet(Of DrugManageListViewModel)                  '2023/04/30 Sato v2.4 ADD
        Public Overridable Property AuthLocationSF As DbSet(Of AuthLocationSFModel)                          '2023/04/30 Sato v2.4 ADD
        Public Overridable Property ColSet As DbSet(Of ColSetModel)
        Public Overridable Property ColSetDetail As DbSet(Of ColSetDetailModel)
        Public Overridable Property ColSetDetailView As DbSet(Of ColSetDetailViewModel)
        Public Overridable Property ColSetDropdown As DbSet(Of ColSetDropdownModel)
        Public Overridable Property ConstructionList As DbSet(Of ConstructionListModel)
        Public Overridable Property ConstructionListView As DbSet(Of ConstructionListViewModel)
        Public Overridable Property ConstructionListTempListView As DbSet(Of ConstructionListTempListViewModel)
        Public Overridable Property ConstructionListReserveListView As DbSet(Of ConstructionListReserveListViewModel)
        Public Overridable Property ConstructionTermList As DbSet(Of ConstructionListTermModel)
        Public Overridable Property ReserveList As DbSet(Of ReserveListModel)
        Public Overridable Property ConstAssetList As DbSet(Of ConstAssetListModel)
        Public Overridable Property ConstAssetListView As DbSet(Of ConstAssetListViewModel)
        Public Overridable Property ConstructionHist As DbSet(Of ConstructionHistModel)
        Public Overridable Property ConstructionHistView As DbSet(Of ConstructionHistViewModel)
        Public Overridable Property ConstAssetHistView As DbSet(Of ConstAssetHistViewModel)

        Public Overridable Property TemporaryList As DbSet(Of TemporaryListModel)
        Public Overridable Property TemporaryListView As DbSet(Of TemporaryListViewModel)
        Public Overridable Property TempAssetList As DbSet(Of TempAssetListModel)
        Public Overridable Property TempAssetListView As DbSet(Of TempAssetListViewModel)
        Public Overridable Property ConstructionListCsv As DbSet(Of ConstructionListCsvModel)

        Protected Overrides Sub OnModelCreating(ByVal modelBuilder As DbModelBuilder)

            '--権限マスタ     'Add 2021.09.25 NV Inoue
            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.PROCESS_TOOL_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.AUTH_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.AUTH_TYPE) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.HEAD_OFFICE_FLG) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.READ_AUTH_FLG) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.UPD_AUTH_FLG) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.READ_HUB_AUTH_FLG) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of AuthModel)() _
            .Property(Function(e) e.UPD_HUB_AUTH_FLG) _
            .IsFixedLength() _
            .IsUnicode(False)

            '--文言マスタ
            modelBuilder.Entity(Of M_WORDModel)() _
                .Property(Function(e) e.MESSAGE_KEY) _
                .IsFixedLength()

            '--社員マスタ
            modelBuilder.Entity(Of EmployeeModel)() _
            .Property(Function(e) e.AUTH_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

            '--設定マスタ
            modelBuilder.Entity(Of SettingModel)() _
            .HasKey(Function(e) New With {e.KEY_DIV, e.KEY1, e.KEY2, e.KEY3})

            '--資産マスタ
            modelBuilder.Entity(Of InventoryInquiryModel)() _
            .Property(Function(e) e.ASSET_NO) _
            .IsFixedLength() _
            .IsUnicode(False)

            '--カテゴリーマスタ
            modelBuilder.Entity(Of CategoryModel)() _
            .Property(Function(e) e.CAT_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

            '--資産マスタ(作業用ワークテーブル)
            modelBuilder.Entity(Of AssetListWorkModel)() _
            .Property(Function(e) e.PHYINV_STATUS) _
            .IsFixedLength() _
            .IsUnicode(False)

            '--操作ログ
            modelBuilder.Entity(Of OperationLogModel)() _
            .Property(Function(e) e.PROCESS_TM) _
            .IsFixedLength() _
            .IsUnicode(False)
            modelBuilder.Entity(Of OperationLogModel)() _
                .Property(Function(e) e.PROCESS_TOOL_DIV) _
                .IsFixedLength() _
                .IsUnicode(False)
            modelBuilder.Entity(Of OperationLogModel)() _
                .Property(Function(e) e.PROCESS_CAT) _
                .IsFixedLength() _
                .IsUnicode(False)

            ''--在庫照会
            'modelBuilder.Entity(Of StockListModel)() _
            '.Property(Function(e) e.PHYINV_STATUS) _
            '.IsFixedLength() _
            '.IsUnicode(False)

            'modelBuilder.Entity(Of AssetListModel)() _
            '.Property(Function(e) e.PHYINV_STATUS) _
            '.IsFixedLength() _
            '.IsUnicode(False)

            'modelBuilder.Entity(Of InventoryModel)() _
            '.Property(Function(e) e.START_DEC_TS) _
            '.IsFixedLength() _
            '.IsUnicode(False)

            '--CSVマッピング明細マスタ
            modelBuilder.Entity(Of CsvMapDetailModel)() _
                .Property(Function(e) e.TOOL_DIV) _
                .IsFixedLength() _
                .IsUnicode(False)

            modelBuilder.Entity(Of CsvMapDetailModel)() _
                .Property(Function(e) e.WORD_DIV) _
                .IsFixedLength() _
                .IsUnicode(False)

            modelBuilder.Entity(Of CsvMapDetailModel)() _
                .Property(Function(e) e.REQUIRED_ITEM_FLAG) _
                .IsFixedLength() _
                .IsUnicode(False)

            modelBuilder.Entity(Of CsvMapDetailModel)() _
                .Property(Function(e) e.SPECIFIED_DIV) _
                .IsFixedLength() _
                .IsUnicode(False)

            '--ゲートマスタ   '2020/02/18 nv miyaji add
            modelBuilder.Entity(Of GateModel)() _
                .Property(Function(e) e.PATLITE_VALID_FLAG) _
                .IsFixedLength() _
                .IsUnicode(False)

            modelBuilder.Entity(Of GateModel)() _
                .Property(Function(e) e.GATE_VALID_FLAG) _
                .IsFixedLength() _
                .IsUnicode(False)

            '周波数のスケールを定義
            modelBuilder.Entity(Of GateModel)() _
                .Property(Function(e) e.FREQUENCY1) _
                .HasPrecision(8, 3)
            modelBuilder.Entity(Of GateModel)() _
                .Property(Function(e) e.FREQUENCY2) _
                .HasPrecision(8, 3)
            modelBuilder.Entity(Of GateModel)() _
                .Property(Function(e) e.FREQUENCY3) _
                .HasPrecision(8, 3)
            modelBuilder.Entity(Of GateModel)() _
                .Property(Function(e) e.FREQUENCY4) _
                .HasPrecision(8, 3)

            '--ゲートアンテナマスタ   '2020/02/18 nv miyaji add
            modelBuilder.Entity(Of GateAntennaModel)() _
                .Property(Function(e) e.ANTENNA_VALID_FLAG) _
                .IsFixedLength() _
                .IsUnicode(False)

            '--ゲート通過履歴   '2020/02/25 nv miyaji add
            modelBuilder.Entity(Of GatePassLogModel)() _
            .Property(Function(e) e.GATE_PASS_TM) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of GatePassLogModel)() _
            .Property(Function(e) e.GATE_PASS_DIV) _
            .IsFixedLength() _
            .IsUnicode(False)

            '--カレンダーマスタ     'Add 2021.09.06 NV Iwamoto
            modelBuilder.Entity(Of CalendarModel)() _
            .Property(Function(e) e.NOT_WORK_FLG) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of LendingEntryModel)() _
            .Property(Function(e) e.DEST_PHONE_NUMBER) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of LendingEntryModel)() _
            .Property(Function(e) e.DEST_ZIP_CODE) _
            .IsFixedLength() _
            .IsUnicode(False)

            modelBuilder.Entity(Of LendingEntryModel)() _
            .Property(Function(e) e.RESERVE_STATUS) _
            .IsFixedLength() _
            .IsUnicode(False)

            '--メール          'Add 2021/10/16 NV Inoue
            modelBuilder.Entity(Of MailModel)() _
            .Property(Function(e) e.MAIL_STATUS) _
            .IsFixedLength() _
            .IsUnicode(False)

            'ロケ－ション
            modelBuilder.Entity(Of LocationModel)() _
            .Property(Function(e) e.SPECIFIED_QTY) _
            .HasPrecision(8, 3)

            '2023/03/22 Kawamoto ADD START
            '履歴照会(資産)
            modelBuilder.Entity(Of AssetDetailModel)() _
            .Property(Function(e) e.DUE_QTY) _
            .HasPrecision(8, 3)

            '資産履歴照会
            modelBuilder.Entity(Of AssetHistoryModel)() _
            .Property(Function(e) e.DUE_QTY) _
            .HasPrecision(8, 3)
            '2023/03/22 Kawamoto ADD END

        End Sub
    End Class
End Namespace
