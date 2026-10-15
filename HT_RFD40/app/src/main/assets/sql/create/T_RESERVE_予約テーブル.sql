CREATE TABLE T_RESERVE (
     RESERVE_ID  TEXT NOT NULL ,			                /* 予約番号 */
	 CONST_NO  TEXT NOT NULL ,			                    /* 工事番号 */
	 CONST_CONTENTS  TEXT NULL ,			                /* 工事内容 */
	 EMP  TEXT NULL ,			                            /* 担当者社員コード */
	 RES_ST_DT  TEXT NULL ,			                        /* 実質貸出開始日 */
     RES_END_DT  TEXT NULL ,			                    /* 実質貸出終了日 */
     ASEET_NO  TEXT NULL ,			                        /* 資産番号 */
	PRIMARY KEY (RESERVE_ID,CONST_NO,ASEET_NO,RES_ST_DT)
)

