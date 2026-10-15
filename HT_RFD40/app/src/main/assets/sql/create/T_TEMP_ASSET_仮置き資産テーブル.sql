CREATE TABLE T_TEMP_ASSET (
    ASSET_NO Text NOT NULL,
    CONST_NO  TEXT NOT NULL ,			                    /* 工事番号 */
    TEMP_ID  Long NOT NULL ,			                    /* 仮置きID */
    PRIMARY KEY (ASSET_NO,TEMP_ID)
)

