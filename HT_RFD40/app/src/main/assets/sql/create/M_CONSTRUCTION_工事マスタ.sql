CREATE TABLE M_CONSTRUCTION (
	 CONST_NO  TEXT NOT NULL ,			                    /* 工事番号 */
     CONST_START_DT  INT NOT NULL ,			                /* 工事開始日 */
     CONST_END_DT  INT NOT NULL ,			                /* 工事終了日 */
     CONST_LENT_START_DT  INT NOT NULL ,			        /* 工事貸出開始日 */
     CONST_LENT_END_DT  INT NOT NULL ,			            /* 工事貸出終了日 */
	 CONST_CONTENTS  TEXT NULL ,			                /* 工事内容 */
     CONST_EMPLOYEE_CD  TEXT NULL ,			                /* 工事責任者コード */
	 CONST_EMPLOYEE_NAME  TEXT NULL ,			            /* 工事責任者氏名 */
	 MNG_LOC_L_CD  TEXT NOT NULL ,			                /* 管理ロケ大区分コード */
	 CONST_INFO1  TEXT NULL ,			                    /* 工事情報1 */
	 CONST_INFO2  TEXT NULL ,			                    /* 工事情報2 */
     CONST_INFO3  TEXT NULL ,			                    /* 工事情報3 */
     CONST_INFO4  TEXT NULL ,			                    /* 工事情報4 */
     CONST_INFO5  TEXT NULL ,			                    /* 工事情報5 */
     CONST_INFO6  TEXT NULL ,			                    /* 工事情報6 */
     CONST_INFO7  TEXT NULL ,			                    /* 工事情報7 */
     CONST_INFO8  TEXT NULL ,			                    /* 工事情報8 */
     CONST_INFO9  TEXT NULL ,			                    /* 工事情報9 */
     CONST_INFO10  TEXT NULL ,			                    /* 工事情報10 */
     CONST_INFO11  TEXT NULL ,			                    /* 工事情報11 */
     CONST_INFO12  TEXT NULL ,			                    /* 工事情報12 */
     CONST_INFO13  TEXT NULL ,			                    /* 工事情報13 */
     CONST_INFO14  TEXT NULL ,			                    /* 工事情報14 */
     CONST_INFO15  TEXT NULL ,			                    /* 工事情報15 */
     CONST_INFO16  TEXT NULL ,			                    /* 工事情報16 */
     CONST_INFO17  TEXT NULL ,			                    /* 工事情報17 */
     CONST_INFO18  TEXT NULL ,			                    /* 工事情報18 */
     CONST_INFO19  TEXT NULL ,			                    /* 工事情報19 */
     CONST_INFO20  TEXT NULL ,			                    /* 工事情報20 */
     PREV_TEMP_NUM  INT NOT NULL DEFAULT(0) ,			    /* 前回仮置数 */
     CONST_IN_NUM  INT NOT NULL DEFAULT(0) ,			    /* 持込数 */
     NOW_TEMP_NUM  INT NOT NULL DEFAULT(0) ,			    /* 今回仮置き数 */
     CONST_OUT_NUM  INT NOT NULL DEFAULT(0) ,			    /* 持帰数 */
     CUR_LENT_NUM  INT NOT NULL DEFAULT(0) ,			    /* 現在貸出数 */
     CUR_TEMP_NUM  INT NOT NULL DEFAULT(0) ,			    /* 現在仮置き数 */
     SITE_EMPLOYEE_CD  TEXT NULL ,			                /* 現場確認者コード */
     SITE_EMPLOYEE_NAME  TEXT NULL ,			            /* 現場確認者氏名 */

	PRIMARY KEY (CONST_NO)
)

