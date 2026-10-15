/****** テーブル名 : T_OPERATION_LOG (操作ログ)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE T_OPERATION_LOG (
	 OPE_LOG_ID  INTEGER NOT NULL PRIMARY KEY autoincrement ,			/* 操作ログID */
	 PROCESS_DT  TEXT NOT NULL ,			/* 処理日 */
	 PROCESS_TM  TEXT NOT NULL ,			/* 処理時刻 */
	 PROCESS_CAT  CHAR NOT NULL ,			/* 処理種別 */
     PROCESS_TOOL_DIV  CHAR NOT NULL ,		/* 処理ツール区分 */
	 PROCESS_CONTENTS  TEXT NULL ,			/* 処理内容 */
	 OPE_EMPLOYEE_CD  TEXT NULL, 			/* 操作者社員コード */
	 LOG_KIND  TEXT NULL         			/* ログ種別 */
)
