CREATE TABLE T_TEMP_LIST (
	 PROCESS_LOG_ID  INTEGER NOT NULL,             /* 処理履歴ID */
	 TEMP_ID  TEXT ,			                   /* 仮置きID */
	 TEMP_DT INT,                                  /* 仮置き日 */
	 TEMP_INFO1 TEXT ,                             /* 仮置き情報1 */
	 TEMP_INFO2 TEXT ,                             /* 仮置き情報2 */
	 TEMP_INFO3 TEXT ,                             /* 仮置き情報3 */
	 TEMP_INFO4 TEXT ,                             /* 仮置き情報4 */
	 TEMP_INFO5 TEXT ,                             /* 仮置き情報5 */
     TEMP_INFO6 TEXT ,                             /* 仮置き情報6 */
     TEMP_INFO7 TEXT ,                             /* 仮置き情報7 */
	 TEMP_REMARK TEXT ,                            /* 備考 */
	 CONST_NO  TEXT NOT NULL ,			           /* 工事番号 */
    PRIMARY KEY (PROCESS_LOG_ID)
)

