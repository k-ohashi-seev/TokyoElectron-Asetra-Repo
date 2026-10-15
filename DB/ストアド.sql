IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AUTH_LOCATION_ULOC]'))
	DROP FUNCTION [dbo].[AUTH_LOCATION_ULOC]
GO

CREATE FUNCTION [dbo].[AUTH_LOCATION_ULOC]
--	*************************************************************************
--	システム名		:	Asetra
--	プログラム名	:	拠点情報取得(特殊権限)
--	プログラムID	:	AUTH_LOCATION_ULOC
--	概要			:	
--	*************************************************************************
--パラメータ
--				：	@IN_EMPLOYEE_CD			NVARCHAR(15)
--				：	@IN_READ_AUTH_FLG		CHAR(1)
--				：	@IN_UPD_AUTH_FLG		CHAR(1)
--				：	@IN_READ_HUB_AUTH_FLG	CHAR(1)
--				：	@IN_UPD_HUB_AUTH_FLG	CHAR(1)
--				：	@IN_AUTH_OPTION			NVARCHAR(15)
--	戻り値		：	TABLE
--	*************************************************************************
(
				 @IN_EMPLOYEE_CD		NVARCHAR(15)
				,@IN_READ_AUTH_FLG		CHAR(1)
				,@IN_UPD_AUTH_FLG		CHAR(1)
				,@IN_READ_HUB_AUTH_FLG	CHAR(1)
				,@IN_UPD_HUB_AUTH_FLG	CHAR(1)
				,@IN_AUTH_OPTION		NVARCHAR(15) = '' 
)
RETURNS @locList TABLE
(
	 LOC_L_CD			NVARCHAR(10)
	,LOC_L_NAME			NVARCHAR(60)
	,LOC_M_CD			NVARCHAR(10)
	,LOC_M_NAME			NVARCHAR(60)
	,UPD_FLG			CHAR(1)
	,LOC_M_FLG			CHAR(1)
)
BEGIN
	--定数
	DECLARE	@s定数_ロケコード大		CHAR(1)		= 'L';
	DECLARE	@s定数_ロケコード中		CHAR(1)		= 'M';
	DECLARE	@s定数_YES				CHAR(1)		= 'Y';
	DECLARE	@s定数_NO				CHAR(1)		= 'N';
	DECLARE	@s定数_中ロケ取得権限	NVARCHAR(15)	= 'U_LOC_M';
	DECLARE	@s定数_スーパー管理者	CHAR(1)		= 'S';
	
	--変数
	DECLARE	@s参照権限フラグ		CHAR(1)		= ISNULL(@IN_READ_AUTH_FLG, @s定数_NO);
	DECLARE	@s他参照権限フラグ		CHAR(1)		= ISNULL(@IN_READ_HUB_AUTH_FLG, @s定数_NO);
	DECLARE	@s更新権限フラグ		CHAR(1)		= ISNULL(@IN_UPD_AUTH_FLG, @s定数_NO);
	DECLARE	@s他更新権限フラグ		CHAR(1)		= ISNULL(@IN_UPD_HUB_AUTH_FLG, @s定数_NO);


	INSERT INTO @locList
	
	--自拠点の大ロケ情報を抽出
	SELECT
		 LOC_L_CD
		,LOC_NAME			LOC_L_NAME
		,''					LOC_M_CD
		,''					LOC_M_NAME
		,@s更新権限フラグ	UPD_FLG
		,@s定数_NO			LOC_M_FLG
	FROM
		M_LOCATION	LOC
	WHERE
			@s参照権限フラグ	= @s定数_YES				--自拠点の参照権限=Y
		AND
		(
				EXISTS(											--自拠点判定
					SELECT 1 FROM M_EMPLOYEE_HUB
					WHERE
						EMPLOYEE_CD		= @IN_EMPLOYEE_CD
					AND	HUB_LOC_L_CD	= LOC.LOC_L_CD
					AND	LOC_DIV			= @s定数_ロケコード大
				)
			OR
				EXISTS(
					SELECT 1 FROM M_EMPLOYEE
					WHERE
						EMPLOYEE_CD		= @IN_EMPLOYEE_CD
					AND AUTH_DIV		= @s定数_スーパー管理者
				)
		)
		AND	LOC_DIV			= @s定数_ロケコード大
		AND	CHARINDEX(@s定数_中ロケ取得権限, ISNULL(@IN_AUTH_OPTION,'')) = 0

	UNION ALL
	--自拠点の中ロケ情報を抽出
	SELECT
		 LOCL.LOC_L_CD
		,LOCL.LOC_NAME		LOC_L_NAME
		,LOCM.LOC_M_CD		LOC_M_CD
		,LOCM.LOC_NAME		LOC_M_NAME
		,@s更新権限フラグ	UPD_FLG
		,@s定数_YES			LOC_M_FLG
	FROM
		M_LOCATION	LOCL
		INNER JOIN M_LOCATION	LOCM
		ON
			LOCM.LOC_DIV	= @s定数_ロケコード中
		AND	LOCL.LOC_L_CD	= LOCM.LOC_L_CD
	WHERE
			@s参照権限フラグ	= @s定数_YES				--自拠点の参照権限=Y
		AND
		(
				EXISTS(											--自拠点判定
					SELECT 1 FROM M_EMPLOYEE_HUB
					WHERE
						EMPLOYEE_CD		= @IN_EMPLOYEE_CD
					AND	HUB_LOC_L_CD	= LOCM.LOC_L_CD
					AND	HUB_LOC_M_CD	= LOCM.LOC_M_CD
				)
			OR
				EXISTS(
					SELECT 1 FROM M_EMPLOYEE
					WHERE
						EMPLOYEE_CD		= @IN_EMPLOYEE_CD
					AND AUTH_DIV		= @s定数_スーパー管理者
				)
		)
		AND	LOCL.LOC_DIV	= @s定数_ロケコード大
		AND	CHARINDEX(@s定数_中ロケ取得権限, ISNULL(@IN_AUTH_OPTION,'')) > 0

	UNION ALL
	--他拠点の大ロケ情報を抽出
	SELECT
		 LOC_L_CD
		,LOC_NAME			LOC_L_NAME
		,''					LOC_M_CD
		,''					LOC_M_NAME
		,@s他更新権限フラグ	UPD_FLG
		,@s定数_NO			LOC_M_FLG
	FROM
		M_LOCATION	LOC
	WHERE
			@s他参照権限フラグ	= @s定数_YES				--他拠点の参照権限=Y
		AND NOT EXISTS(										--他拠点判定
				SELECT 1 FROM M_EMPLOYEE_HUB
				WHERE
					EMPLOYEE_CD		= @IN_EMPLOYEE_CD
				AND	HUB_LOC_L_CD	= LOC.LOC_L_CD
				AND	LOC_DIV			= @s定数_ロケコード大
			)
		AND NOT EXISTS(
				SELECT 1 FROM M_EMPLOYEE
				WHERE
					EMPLOYEE_CD		= @IN_EMPLOYEE_CD
				AND AUTH_DIV		= @s定数_スーパー管理者
			)
		AND	LOC_DIV			= @s定数_ロケコード大
	UNION ALL
	--他拠点の中ロケ情報を抽出
	SELECT
		 LOCL.LOC_L_CD
		,LOCL.LOC_NAME		LOC_L_NAME
		,LOCM.LOC_M_CD		LOC_M_CD
		,LOCM.LOC_NAME		LOC_M_NAME
		,@s他更新権限フラグ	UPD_FLG
		,@s定数_YES			LOC_M_FLG
	FROM
		M_LOCATION	LOCL
		INNER JOIN M_LOCATION	LOCM
		ON
			LOCM.LOC_DIV	= @s定数_ロケコード中
		AND	LOCL.LOC_L_CD	= LOCM.LOC_L_CD
	WHERE
			@s他参照権限フラグ	= @s定数_YES				--他拠点の参照権限=Y
		AND NOT EXISTS(										--他拠点判定
				SELECT 1 FROM M_EMPLOYEE_HUB
				WHERE
					EMPLOYEE_CD		= @IN_EMPLOYEE_CD
				AND	HUB_LOC_L_CD	= LOCM.LOC_L_CD
				AND	HUB_LOC_M_CD	= LOCM.LOC_M_CD
			)
		AND NOT EXISTS(
				SELECT 1 FROM M_EMPLOYEE
				WHERE
					EMPLOYEE_CD		= @IN_EMPLOYEE_CD
				AND AUTH_DIV		= @s定数_スーパー管理者
		)
		AND	LOCL.LOC_DIV	= @s定数_ロケコード大
		AND	CHARINDEX(@s定数_中ロケ取得権限, ISNULL(@IN_AUTH_OPTION,'')) > 0

	;

	
	RETURN;
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AUTH_LOCATION_SF]'))
	DROP FUNCTION [dbo].[AUTH_LOCATION_SF]
GO

CREATE FUNCTION [dbo].[AUTH_LOCATION_SF]
--	*************************************************************************
--	システム名		:	Asetra
--	プログラム名	:	ロケーション権限情報取得
--	プログラムID	:	AUTH_LOCATION_SF
--	概要			:	
--	*************************************************************************
--パラメータ
--				：	@IN_EMPLOYEE_CD			NVARCHAR(15)
--				：	@IN_READ_AUTH_FLG		CHAR(1)
--				：	@IN_UPD_AUTH_FLG		CHAR(1)
--				：	@IN_READ_HUB_AUTH_FLG	CHAR(1)
--				：	@IN_UPD_HUB_AUTH_FLG	CHAR(1)
--				：	@IN_UPD_AUTH_TYPE		NVARCHAR(15)
--	戻り値		：	TABLE
--	*************************************************************************
(
				 @IN_EMPLOYEE_CD		NVARCHAR(15)
				,@IN_READ_AUTH_FLG		CHAR(1)
				,@IN_UPD_AUTH_FLG		CHAR(1)
				,@IN_READ_HUB_AUTH_FLG	CHAR(1)
				,@IN_UPD_HUB_AUTH_FLG	CHAR(1)
				,@IN_UPD_AUTH_TYPE		NVARCHAR(15) = 'UPD_AUTH' 
)
RETURNS @locList TABLE
(
	 LOC_L_CD			NVARCHAR(10)
	,LOC_NAME			NVARCHAR(60)
	,SHARED_FLG			CHAR(1)
	,UPD_FLG			CHAR(1)
)
BEGIN
	--定数
	DECLARE	@s定数_ロケコード大		CHAR(1)		= 'L';
	DECLARE	@s定数_YES				CHAR(1)		= 'Y';
	DECLARE	@s定数_NO				CHAR(1)		= 'N';
	DECLARE	@s定数_他拠点			CHAR(1)		= 'A';
	DECLARE	@s定数_共有拠点更新権限対象_自				NVARCHAR(15)		= 'UPD_AUTH_HUB';
	DECLARE	@s定数_共有拠点更新権限対象_他				NVARCHAR(15)		= 'UPD_AUTH';
	DECLARE	@s定数_スーパー管理者	CHAR(1)		= 'S';
	
	--変数
	DECLARE	@s参照権限フラグ		CHAR(1)		= ISNULL(@IN_READ_AUTH_FLG, @s定数_NO);
	DECLARE	@s他参照権限フラグ		CHAR(1)		= ISNULL(@IN_READ_HUB_AUTH_FLG, @s定数_NO);
	DECLARE	@s更新権限フラグ		CHAR(1)		= ISNULL(@IN_UPD_AUTH_FLG, @s定数_NO);
	DECLARE	@s他更新権限フラグ		CHAR(1)		= ISNULL(@IN_UPD_HUB_AUTH_FLG, @s定数_NO);


	INSERT INTO @locList
	
	--自拠点の情報を抽出
	SELECT
		 LOC_L_CD
		,LOC_NAME
		,LOC_SHARE_FLG
		,@s更新権限フラグ	UPD_FLG
	FROM
		M_LOCATION	LOC
	WHERE
			@s参照権限フラグ	= @s定数_YES				--自拠点の参照権限=Y
		AND
		(
				EXISTS(											--自拠点判定
					SELECT 1 FROM M_EMPLOYEE_HUB
					WHERE
						EMPLOYEE_CD		= @IN_EMPLOYEE_CD
					AND	HUB_LOC_L_CD	= LOC.LOC_L_CD
					AND	LOC_DIV			= @s定数_ロケコード大
				)
			OR
				EXISTS(
					SELECT 1 FROM M_EMPLOYEE
					WHERE
						EMPLOYEE_CD		= @IN_EMPLOYEE_CD
					AND AUTH_DIV		= @s定数_スーパー管理者
				)
		)
		AND	LOC_DIV			= @s定数_ロケコード大

	UNION ALL
	--共通拠点でかつ自拠点でない拠点を抽出
	SELECT
		 LOC_L_CD
		,LOC_NAME
		,LOC_SHARE_FLG
		,CASE WHEN @IN_UPD_AUTH_TYPE = @s定数_共有拠点更新権限対象_自
		 THEN	@s更新権限フラグ
		 ELSE	@s他更新権限フラグ
		 END	UPD_FLG
	FROM
		M_LOCATION	LOC
	WHERE
			@s参照権限フラグ	= @s定数_YES				--自拠点の参照権限=Y
		AND NOT EXISTS(										--他拠点判定
			SELECT 1 FROM M_EMPLOYEE_HUB
			WHERE
				EMPLOYEE_CD		= @IN_EMPLOYEE_CD
			AND	HUB_LOC_L_CD	= LOC.LOC_L_CD
			AND	LOC_DIV			= @s定数_ロケコード大
		)
		AND NOT EXISTS(
				SELECT 1 FROM M_EMPLOYEE
				WHERE
					EMPLOYEE_CD		= @IN_EMPLOYEE_CD
				AND AUTH_DIV		= @s定数_スーパー管理者
			)
		AND	LOC_SHARE_FLG	= @s定数_YES					--共通拠点
		AND	LOC_DIV			= @s定数_ロケコード大

	UNION ALL
	--他拠点の情報を抽出
	SELECT
		 LOC_L_CD
		,LOC_NAME
		,CASE WHEN LOC_SHARE_FLG = @s定数_NO THEN @s定数_他拠点 ELSE @s定数_YES END		--共有拠点でない場合は他拠点(O)を返却
		,@s他更新権限フラグ		UPD_FLG
	FROM
		M_LOCATION	LOC
	WHERE
			@s他参照権限フラグ		= @s定数_YES			--他拠点の参照権限=Y
		AND NOT EXISTS(										--他拠点判定
				SELECT 1 FROM M_EMPLOYEE_HUB
				WHERE
					EMPLOYEE_CD		= @IN_EMPLOYEE_CD
				AND	HUB_LOC_L_CD	= LOC.LOC_L_CD
				AND	LOC_DIV			= @s定数_ロケコード大
			)
		AND NOT EXISTS(
				SELECT 1 FROM M_EMPLOYEE
				WHERE
					EMPLOYEE_CD		= @IN_EMPLOYEE_CD
				AND AUTH_DIV		= @s定数_スーパー管理者
			)
		AND	LOC_SHARE_FLG	= @s定数_NO						--共通拠点以外
		AND	LOC_DIV			= @s定数_ロケコード大
	;

	
	RETURN;
END
GO

