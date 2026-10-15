/****** テーブル名 : M_LEASE_PLACE (貸出先マスタ)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE M_LEASE_PLACE (
	 LEASE_PLACE_CD  TEXT NOT NULL ,			/* 貸出先コード */
	 LEASE_PLACE_NAME  TEXT NOT NULL ,			/* 貸出先名 */
	PRIMARY KEY (LEASE_PLACE_CD)
)

