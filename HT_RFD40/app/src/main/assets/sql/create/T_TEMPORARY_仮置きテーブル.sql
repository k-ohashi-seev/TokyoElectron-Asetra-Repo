CREATE TABLE T_TEMPORARY (
	 TEMP_ID  TEXT NOT NULL ,			                    /* 仮置きID */
	 TEMP_DT INT NOT NULL,                                  /* 仮置き日 */
	 TEMP_INFO1 Text NULL,                                  /* 仮置き情報1 */
	 TEMP_INFO2 Text NULL,                                  /* 仮置き情報2 */
	 TEMP_INFO3 Text NULL,                                  /* 仮置き情報3 */
	 TEMP_INFO4 Text NULL,                                  /* 仮置き情報4 */
	 TEMP_INFO5 Text NULL,                                  /* 仮置き情報5 */
     TEMP_INFO6 Text NULL,                                  /* 仮置き情報6 */
     TEMP_INFO7 Text NULL,                                  /* 仮置き情報7 */
	 TEMP_REMARK Text NULL,                                 /* 備考 */
	 CONST_NO  TEXT NOT NULL ,			                    /* 工事番号 */

	 PRIMARY KEY (TEMP_ID,CONST_NO)
)

