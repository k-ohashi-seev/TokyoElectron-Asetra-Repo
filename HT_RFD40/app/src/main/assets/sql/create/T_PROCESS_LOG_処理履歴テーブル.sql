/****** テーブル名 : T_PROCESS_LOG (処理履歴テーブル)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE T_PROCESS_LOG (
	 PROCESS_LOG_ID  INTEGER NOT NULL primary key autoincrement,			/* 処理履歴ID */
	 PROCESS_DT  TEXT NOT NULL ,			/* 処理日 */
	 PROCESS_TM  TEXT NOT NULL ,			/* 処理時刻 */
	 PROCESS_TOOL_DIV  CHAR NOT NULL ,			/* 処理ツール区分 */
	 ASSET_NO  TEXT NULL ,			/* 資産番号 */
	 ASSET_ID  TEXT NULL ,			/* 資産ID */
	 PROCESS_DIV  CHAR NOT NULL ,			/* 処理区分 */
	 LEASE_PLACE_CD  TEXT NULL ,			/* 貸出先場所コード */
	 LEASE_EMPLOYEE_CD  TEXT NULL ,			/* 貸出先担当者社員コード */
	 LEASE_DUE_DT  INT NULL ,			/* 貸出返却予定日 */
     REMARK TEXT NULL ,			    /* 備考 */
	 LOC_L_CD  TEXT NULL ,			/* ロケ大区分コード */
	 LOC_M_CD  TEXT NULL ,			/* ロケ中区分コード */
	 LOC_S_CD  TEXT NULL ,			/* ロケ小区分コード */
	 EMPLOYEE_CD  TEXT NULL 			/* ログイン担当者コード */
)

