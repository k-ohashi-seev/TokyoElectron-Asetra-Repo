/****** テーブル名 : M_ASSET (資産マスタ)  from : HTDBﾚｲｱｳﾄ_20190713.xls ******/
CREATE TABLE M_ASSET (
	 ASSET_NO  TEXT NOT NULL ,		/* 資産番号 */
	 ASSET_ID  TEXT NULL ,			/* 資産ID */
	 ASSET_NAME  TEXT NULL ,		/* 資産名称 */
	 MODEL_NO  TEXT NULL ,          /* 型番 */
	 LOC_L_CD  TEXT NULL ,			/* ロケ大区分コード */
	 LOC_M_CD  TEXT NULL ,			/* ロケ中区分コード */
	 LOC_S_CD  TEXT NULL ,			/* ロケ小区分コード */
	 ASSET_STATUS  INTEGER NOT NULL  DEFAULT(0) ,	/* 資産状態 */
	 ASSETID_REG_DIB TEXT NULL ,   	/* 資産ID書込み方法 */
	 EPC_SIZE TEXT NULL,             /* EPC_SIZE */
	 PHYINV_FLAG TEXT NULL,          /* 実棚済みフラグ */
	 PHYINV_STATUS TEXT NULL,        /* 実棚ステータス */
	 LEASE_CNT INTEGER NOT NULL DEFAULT(0), /* 貸出回数 20200225ADD V1.2*/
	 LEASE_UPPER_ERR_DIV TEXT NULL, /* 貸出上限エラー区分 20200225ADD V1.2 */
	 LEASE_UPPER_LIMIT TEXT NULL,   /* 貸出上限数 20200225ADD V1.2 */
	 MNG_LOC_L_CD TEXT NOT NULL,    /* 管理ロケ大区分コード */
	 LEASE_STATUS INTEGER NOT NULL, /* 貸出状態 */
     LEASE_DUE_DT TEXT NULL,        /*貸出返却予定日 20230906 ADD v2.4.1*/
     LEASE_LIMIT TEXT NULL,         /*リース期限 20230906 ADD v2.4.1*/
     GENERIC_LIMIT TEXT NULL,       /*○○期限 20230906 ADD v2.4.1*/
     LEASE_UPPER_ALERT_LIMIT NULL,  /*貸出上限警告数  20230906 ADD v2.4.1*/
     SUB_ASSET_NO TEXT NULL,        /*サブ資産番号 */
	 IS_TANGIBLE TEXT NOT NULL,     /*有形/無形区分 */
	 INV_FLG TEXT NOT NULL,         /*棚卸対象区分 */
	 PRIMARY KEY (ASSET_NO)
)

