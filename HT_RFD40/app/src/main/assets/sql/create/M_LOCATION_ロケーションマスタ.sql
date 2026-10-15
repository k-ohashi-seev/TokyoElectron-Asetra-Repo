/****** テーブル名 : M_LOCATION (ロケーションマスタ)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE M_LOCATION (
	 LOC_DIV  CHAR NOT NULL ,			/* ロケ区分 */
	 LOC_L_CD  TEXT NOT NULL ,			/* ロケ大区分コード */
	 LOC_M_CD  TEXT NOT NULL ,			/* ロケ中区分コード */
	 LOC_S_CD  TEXT NOT NULL ,			/* ロケ小区分コード */
	 LOC_NAME  TEXT NOT NULL ,			/* ロケ名称 */
	 LOC_RFID_FLG CHAR NOT NULL,        /*RFIDタグ拠点*/
	PRIMARY KEY (LOC_DIV,LOC_L_CD,LOC_M_CD,LOC_S_CD)
)

