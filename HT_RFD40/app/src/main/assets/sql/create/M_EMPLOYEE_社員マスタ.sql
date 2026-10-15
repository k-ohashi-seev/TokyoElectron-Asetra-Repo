/****** テーブル名 : M_EMPLOYEE (社員マスタ)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE M_EMPLOYEE (
	 EMPLOYEE_CD  TEXT NOT NULL ,			/* 社員コード */
	 EMPLOYEE_NAME  TEXT NOT NULL ,			/* 社員名 */
	 AUTH_DIV CHAR NOT NULL,                /* 権限区分 */
	 HEAD_OFFICE_FLG TEXT NULL ,			/* 本部フラグ */
	 PRIMARY KEY (EMPLOYEE_CD)
)

