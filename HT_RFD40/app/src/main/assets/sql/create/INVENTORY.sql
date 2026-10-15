CREATE TABLE INVENTORY (
	 ASSET_NO  TEXT NOT NULL ,			/* 資産番号 */
	 ASSET_ID  TEXT NULL ,			/* 資産ID */
	 EPC_SIZE  INT NULL ,			/* EPC領域サイズ */
	 GEN_ASSET_ID  TEXT NULL ,			/* 生成資産ID */
	 ASSET_DIV  TEXT NULL ,			/* 資産区分 */
	 ASSET_NAME  TEXT NULL ,			/* 資産名称 */
	 LOC_L_CD  TEXT NULL ,			/* ロケ大区分コード */
	 LOC_M_CD  TEXT NULL ,			/* ロケ中区分コード */
	 LOC_S_CD  TEXT NULL ,			/* ロケ小区分コード */
	 CAT_L_CD  TEXT NULL ,			/* カテゴリ大分類コード */
	 CAT_M_CD  TEXT NULL ,			/* カテゴリ中分類コード */
	 CAT_S_CD  TEXT NULL ,			/* カテゴリ小分類コード */
	 MAKER  TEXT NULL ,			/* メーカ */
	 PRODUCT_NO  TEXT NULL ,			/* 品番 */
	 MODEL_NO  TEXT NULL ,			/* 型番 */
	 LEASE_NO  TEXT NULL ,			/* リース管理番号 */
	 LEASE_LIMIT  INT NULL ,			/* リース期限 */
	 GENERIC_LIMIT  INT NULL ,			/* □□期限 */
	 FREE_ITEM1  TEXT NULL ,			/* フリー項目1 */
	 FREE_ITEM2  TEXT NULL ,			/* フリー項目2 */
	 FREE_ITEM3  TEXT NULL ,			/* フリー項目3 */
	 FREE_ITEM4  TEXT NULL ,			/* フリー項目4 */
	 FREE_ITEM5  TEXT NULL ,			/* フリー項目5 */
	 FREE_ITEM6  TEXT NULL ,			/* フリー項目6 */
	 FREE_ITEM7  TEXT NULL ,			/* フリー項目7 */
	 FREE_ITEM8  TEXT NULL ,			/* フリー項目8 */
	 FREE_ITEM9  TEXT NULL ,			/* フリー項目9 */
	 FREE_ITEM10  TEXT NULL ,			/* フリー項目10 */
	 ASSET_PIC_FILENAME  TEXT NULL ,			/* 資産画像ファイル名 */
	 LEASE_CNT  INT NOT NULL  DEFAULT(0) ,			/* 貸出回数 */
	 ASSET_STATUS  INTEGER NOT NULL  DEFAULT(0) ,			/* 資産状態 */
	 EPC_ENC_STATUS  INTEGER NOT NULL  DEFAULT(0) ,			/* EPCエンコード状態 */
	 TAG_LINK_STATUS  INTEGER NOT NULL  DEFAULT(0) ,			/* タグ紐付状態 */
	 RETIRE_TS  BIGINT NULL ,			/* 除却日時 */
	 RETIRE_EMPLOYEE_CD  TEXT NULL ,			/* 除却者社員コード */
	 RETIRE_EMPLOYEE_NAME  TEXT NULL ,			/* 除却者氏名 */
	 RETIRE_ASSET_ID  TEXT NULL ,			/* 除却資産ID */
	 PHYINV_TS  BIGINT NULL ,			/* 実棚日時 */
	 PHYINV_STATUS  CHAR NULL ,			/* 実棚ステータス */
	 PHYINV_LOC_L_CD  TEXT NULL ,			/* 実棚ロケ大区分コード */
	 PHYINV_LOC_M_CD  TEXT NULL ,			/* 実棚ロケ中区分コード */
	 PHYINV_LOC_S_CD  TEXT NULL ,			/* 実棚ロケ小区分コード */
	 PHYINV_LOC_L_NAME  TEXT NULL ,			/* 実棚ロケ大区分名称 */
	 PHYINV_LOC_M_NAME  TEXT NULL ,			/* 実棚ロケ中区分名称 */
	 PHYINV_LOC_S_NAME  TEXT NULL ,			/* 実棚ロケ小区分名称 */
	 PHYINV_EMPLOYEE_CD  TEXT NULL ,			/* 実棚実施者社員コード */
	 PHYINV_EMPLOYEE_NAME  TEXT NULL ,			/* 実棚実施者氏名 */
	 INS_DATETIME  NUMERIC NOT NULL ,			/* 登録日時 */
	 INS_EMPLOYEE_CD  TEXT NOT NULL ,			/* 登録者社員コード */
	 INS_EMPLOYEE_NAME  TEXT NOT NULL ,			/* 登録者氏名 */
	 UPD_DATETIME  NUMERIC NOT NULL ,			/* 最終登録更新日時 */
	 UPD_EMPLOYEE_CD  TEXT NOT NULL ,			/* 最終登録更新者社員コード */
	 UPD_EMPLOYEE_NAME  TEXT NOT NULL ,			/* 最終登録更新者氏名 */
	 UPD_TASK  TEXT NOT NULL ,			/* 最終登録更新処理 */
	PRIMARY KEY (ASSET_NO)
)

