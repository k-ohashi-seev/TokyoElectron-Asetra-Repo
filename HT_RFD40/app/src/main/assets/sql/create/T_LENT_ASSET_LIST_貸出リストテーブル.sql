CREATE TABLE T_LENT_ASSET_LIST (
	 PROCESS_LOG_ID  INTEGER NOT NULL,		/* 処理履歴ID */
    ASSET_NO Text NOT NULL,                 /* 資産番号 */
    CONST_NO TEXT NOT NULL ,			    /* 工事番号 */
    LEASE_DT TEXT ,                         /* 持出日 */
    REMARK TEXT,                            /* 備考 */
	 PRIMARY KEY (PROCESS_LOG_ID,ASSET_NO,CONST_NO)
)

