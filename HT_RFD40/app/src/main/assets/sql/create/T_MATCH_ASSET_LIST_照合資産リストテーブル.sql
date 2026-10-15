CREATE TABLE T_MATCH_ASSET_LIST (
    PROCESS_LOG_ID INTEGER NOT NULL,                        /* 処理履歴ID */
    ASSET_NO Text NOT NULL,                                 /* 資産番号 */
    CONST_NO  TEXT NOT NULL ,			                    /* 工事番号 */
    CONT_MATCH_DIV  TEXT ,			                        /* 照合結果 */
    PRIMARY KEY (PROCESS_LOG_ID,ASSET_NO,CONST_NO)
)

