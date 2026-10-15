/****** テーブル名 : M_SETTING (設定マスタ)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE M_SETTING (
	 KEY_DIV  TEXT NOT NULL ,			/* KEY種別 */
	 KEY1  TEXT NOT NULL ,			/* KEY1 */
	 KEY2  TEXT NOT NULL ,			/* KEY2 */
	 KEY3  TEXT NOT NULL ,			/* KEY3 */
	 REMARK  TEXT NULL ,			/* 備考 */
	 DATA1  TEXT NULL ,			/* DATA1 */
	 DATA2  TEXT NULL ,			/* DATA2 */
	 DATA3  TEXT NULL ,			/* DATA3 */
	 DATA4  TEXT NULL ,			/* DATA4       */
	 DATA5  TEXT NULL ,			/* DATA5       */
	 DATA6  TEXT NULL ,			/* DATA6       */
	 DATA7  TEXT NULL ,			/* DATA7       */
	 DATA8  TEXT NULL ,			/* DATA8     */
	 DATA9  TEXT NULL ,			/* DATA9      */
	 DATA10  TEXT NULL ,			/* DATA10   */
	 SPECIAL_DATA_DIV  TEXT NULL ,			/* 特定データ区分 */
	PRIMARY KEY (KEY_DIV,KEY1,KEY2,KEY3)
)

