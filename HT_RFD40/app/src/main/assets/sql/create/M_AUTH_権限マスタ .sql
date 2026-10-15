/****** テーブル名 : M_AUTH (権限マスタ)  from : HTDBﾚｲｱｳﾄ_20210729.xls ******/
CREATE TABLE M_AUTH (
      FUNCTION_ID TEXT NOT NULL ,            /* 機能ID */
      AUTH_DIV CHAR NOT NULL ,               /* 権限区分 */
      AUTH_TYPE CHAR NOT NULL ,              /* 権限タイプ */
      HEAD_OFFICE_FLG CHAR NULL,             /* 本部フラグ */
      READ_AUTH_FLG CHAR NULL,               /* 参照権限 */
      UPD_AUTH_FLG CHAR NULL,                /* 更新権限 */
      READ_HUB_AUTH_FLG CHAR NULL,           /* 参照権限(他拠点) */
      UPD_HUB_AUTH_FLG CHAR NULL,            /* 更新権限(他拠点) */
      AUTH_OPTION CHAR NULL,                 /* 特殊権限 */
      PRIMARY KEY (FUNCTION_ID, AUTH_DIV, AUTH_TYPE, HEAD_OFFICE_FLG)
)
