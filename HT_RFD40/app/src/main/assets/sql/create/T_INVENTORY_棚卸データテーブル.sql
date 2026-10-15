/****** テーブル名 : T_INVENTORY (棚卸データテーブル)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE T_INVENTORY (
	 ASSET_NO  TEXT NOT NULL ,			/* 資産番号 */
	 PHYINV_TS  TEXT NULL ,			    /* 実棚日時 */
	 PHYINV_STATUS  CHAR NULL ,		/* 実棚ステータス */
	 LOC_L_CD  TEXT NULL ,			/* ロケ大区分コード */
	 LOC_M_CD  TEXT NULL ,			/* ロケ中区分コード */
	 LOC_S_CD  TEXT NULL ,			/* ロケ小区分コード */
	 PHYINV_EMPLOYEE_CD  TEXT NULL ,			/* 実棚実施者社員コード */
	PRIMARY KEY (ASSET_NO)
)

