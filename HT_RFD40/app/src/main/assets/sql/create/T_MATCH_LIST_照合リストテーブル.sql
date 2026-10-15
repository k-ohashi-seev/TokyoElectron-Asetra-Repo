/****** テーブル名 : T_MATCH_LIST (照合リストテーブル)  ******/
CREATE TABLE T_MATCH_LIST (
	 PROCESS_LOG_ID  INTEGER NOT NULL,		/* 処理履歴ID */
	 PROCESS_DT  TEXT NOT NULL ,			/* 処理日 */
	 PROCESS_TM  TEXT NOT NULL ,			/* 処理時刻 */
	 PROCESS_DIV  CHAR NOT NULL ,			/* 処理区分 */
     CONST_NO  TEXT NOT NULL ,			    /* 工事番号 */
     LEASE_EMPLOYEE_CD  TEXT NOT NULL ,		/* 貸出先担当者社員コード */
     LEASE_DUE_DT  TEXT NOT NULL ,			/* 貸出返却予定日 */
     CONT_MATCH_DIV  TEXT NOT NULL ,		/* 照合結果 */
     CONT_MATCH_NUM  TEXT NOT NULL ,		/* 照合数 */
	 EMPLOYEE_CD  TEXT NULL ,			/* ログイン担当者コード */
	 PRIMARY KEY (PROCESS_LOG_ID)
)

