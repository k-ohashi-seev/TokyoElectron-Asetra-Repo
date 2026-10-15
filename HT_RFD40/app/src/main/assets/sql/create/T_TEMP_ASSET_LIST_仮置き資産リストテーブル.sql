CREATE TABLE T_TEMP_ASSET_LIST (
    PROCESS_LOG_ID Text NOT NULL,
    ASSET_NO Text NOT NULL,
    CONST_NO  TEXT NOT NULL ,			                    /* 工事番号 */
    TEMP_ID  TEXT ,			                    /* 仮置きID */
    PRIMARY KEY (PROCESS_LOG_ID,CONST_NO,ASSET_NO)
)

