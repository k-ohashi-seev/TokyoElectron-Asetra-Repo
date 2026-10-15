/****** テーブル名 : M_WORD (文言マスタ)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE M_WORD (
	 TOOL_DIV  CHAR NOT NULL ,			/* 利用ツール区分 */
	 WORD_DIV  CHAR NOT NULL ,			/* 文言区分 */
	 WORD_CD  TEXT NOT NULL ,			/* 文言コード */
	 WORDING  TEXT NOT NULL ,			/* 文言 */
	PRIMARY KEY (TOOL_DIV,WORD_DIV,WORD_CD)
)

