-- 引数の予約情報で予約明細を登録する。
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RESERVE_DTL_UPDATE_SP]'))
	DROP PROCEDURE [dbo].RESERVE_DTL_UPDATE_SP
GO
CREATE PROCEDURE RESERVE_DTL_UPDATE_SP
--	*************************************************************************
--	システム名		:	Asetra
--	プログラム名	:	予約明細更新
--	プログラムID	:	RESERVE_DTL_UPDATE_SP
--	概要			:	予約明細と紐づくメンテナンス情報を更新する
--	*************************************************************************
--パラメータ
--				：	@IN_RESERVE_ID					BIGINT
--				：	@IN_ASSET_NO					NVARCHAR(24)
--				：	@IN_START_DT					INT
--				：	@IN_END_DT						INT
--				：	@IN_MAINTE_START_DT				INT
--				：	@IN_MAINTE_END_DT				INT
--				：	@IN_CHG_ASSET_NO				NVARCHAR(24)
--				：	@IN_REG_EMP_CD					NVARCHAR(15)
--				：	@IN_REG_EMP_NAME				NVARCHAR(60)
--				：	@IN_REG_TASK					NVARCHAR(255)
--				：	@OUT_RETURN						INT
--	*************************************************************************
(
	 @IN_RESERVE_ID					BIGINT
	,@IN_ASSET_NO					NVARCHAR(24)
	,@IN_START_DT					INT
	,@IN_END_DT						INT
	,@IN_MAINTE_START_DT			INT
	,@IN_MAINTE_END_DT				INT
	,@IN_CHG_ASSET_NO				NVARCHAR(24)
	,@IN_REG_EMP_CD					NVARCHAR(15)
	,@IN_REG_EMP_NAME				NVARCHAR(60)
	,@IN_REG_TASK					NVARCHAR(255)
	,@OUT_RETURN					INT OUTPUT
)
AS
BEGIN
	--定数
	DECLARE	@s定数_予約ステータス_未貸出		CHAR(1)		= '0';
	DECLARE	@s定数_予約ステータス_貸出中		CHAR(1)		= '1';
	DECLARE	@s定数_予約ステータス_返却済		CHAR(1)		= '2';
	DECLARE	@n定数_戻り値_予約明細更新			INT			= 1;
	DECLARE	@n定数_戻り値_予約明細登録			INT			= 2;
    
    DECLARE	@n定数_メンテナンス区分_メンテナンス CHAR(1)   = '1';
	

	--変数
	DECLARE	@nMainteId		BIGINT;
	DECLARE	@n戻り値		INT;

	-- メンテナンス更新処理
	-- メンテナンス情報を抽出する
	SELECT @nMainteId = MAINTE_ID FROM T_RESERVE_MAINTE WHERE RESERVE_ID = @IN_RESERVE_ID AND ASSET_NO = @IN_ASSET_NO;

	IF @IN_MAINTE_START_DT IS NULL OR @IN_MAINTE_END_DT IS NULL
		BEGIN
			-- 新しいメンテナンス期間の指定がない場合は削除する
			DELETE FROM T_RESERVE_MAINTE WHERE MAINTE_ID = @nMainteId;
		END;
	ELSE
		IF @nMainteId IS NULL
			BEGIN
				-- 既存のメンテナンスデータがない場合は登録する
				INSERT INTO T_RESERVE_MAINTE
				(
					 RESERVE_ID
					,ASSET_NO
					,MAINTE_START_DT
					,MAINTE_END_DT
                    ,MAINTE_KBN
					,INS_DATETIME
					,INS_EMPLOYEE_CD
					,INS_EMPLOYEE_NAME
					,UPD_DATETIME
					,UPD_EMPLOYEE_CD
					,UPD_EMPLOYEE_NAME
					,UPD_TASK
				)
				VALUES
				(
					 @IN_RESERVE_ID
					,@IN_ASSET_NO
					,@IN_MAINTE_START_DT
					,@IN_MAINTE_END_DT
                    ,@n定数_メンテナンス区分_メンテナンス
					,GETDATE()
					,@IN_REG_EMP_CD
					,@IN_REG_EMP_NAME
					,GETDATE()
					,@IN_REG_EMP_CD
					,@IN_REG_EMP_NAME
					,@IN_REG_TASK
				);
			END;
		ELSE
			BEGIN
				-- メンテナンスデータの指定した期間で更新する
				UPDATE
					T_RESERVE_MAINTE
				SET
					 MAINTE_START_DT = @IN_MAINTE_START_DT
					,MAINTE_END_DT = @IN_MAINTE_END_DT
					,UPD_DATETIME = GETDATE()		
					,UPD_EMPLOYEE_CD = @IN_REG_EMP_CD
					,UPD_EMPLOYEE_NAME = @IN_REG_EMP_NAME
					,UPD_TASK = @IN_REG_TASK
				WHERE
					MAINTE_ID = @nMainteId
				;
			END;
			
		--運用上考慮不要だが、システム的に問題が発生するため既に予約明細に登録した返却済みの資産は削除する。
		IF EXISTS(SELECT 1 FROM T_RESERVE_DETAIL WHERE RESERVE_ID = @IN_RESERVE_ID AND ASSET_NO = @IN_ASSET_NO AND RESERVE_STATUS IN (@s定数_予約ステータス_返却済))
			BEGIN
				DELETE FROM
					T_RESERVE_DETAIL
				WHERE
					RESERVE_ID = @IN_RESERVE_ID
				AND ASSET_NO = @IN_ASSET_NO
				;
			END;

	IF EXISTS(SELECT 1 FROM T_RESERVE_DETAIL WHERE RESERVE_ID = @IN_RESERVE_ID AND ASSET_NO = @IN_ASSET_NO AND RESERVE_STATUS IN (@s定数_予約ステータス_未貸出, @s定数_予約ステータス_貸出中))
		BEGIN
			-- 予約明細の期間を更新
			UPDATE
				T_RESERVE_DETAIL
			SET
				 REAL_START_DT = @IN_START_DT
				,REAL_END_DT = @IN_END_DT
				,PR_ST_DT = ISNULL(LENDING_DT, @IN_START_DT)
				,PR_END_DT = ISNULL(RETURN_DT, @IN_END_DT)
				,UPD_DATETIME = GETDATE()		
				,UPD_EMPLOYEE_CD = @IN_REG_EMP_CD
				,UPD_EMPLOYEE_NAME = @IN_REG_EMP_NAME
				,UPD_TASK = @IN_REG_TASK
			WHERE
				RESERVE_ID = @IN_RESERVE_ID
			AND ASSET_NO = @IN_ASSET_NO
			;

			SET @n戻り値 = @n定数_戻り値_予約明細更新;
		END;
	ELSE
		BEGIN
			-- 予約明細を登録
			INSERT INTO
				T_RESERVE_DETAIL
			(
				 RESERVE_ID
				,ASSET_NO
				,REAL_START_DT
				,REAL_END_DT
				,CHARGE_EMP_CD
				,PR_ST_DT
				,PR_END_DT
				,RESERVE_STATUS
				,INS_DATETIME
				,INS_EMPLOYEE_CD
				,INS_EMPLOYEE_NAME
				,UPD_DATETIME
				,UPD_EMPLOYEE_CD
				,UPD_EMPLOYEE_NAME
				,UPD_TASK
			)
			VALUES
			(
				 @IN_RESERVE_ID
				,@IN_ASSET_NO
				,@IN_START_DT
				,@IN_END_DT
				,@IN_REG_EMP_CD
				,@IN_START_DT
				,@IN_END_DT
				,@s定数_予約ステータス_未貸出
				,GETDATE()
				,@IN_REG_EMP_CD
				,@IN_REG_EMP_NAME
				,GETDATE()
				,@IN_REG_EMP_CD
				,@IN_REG_EMP_NAME
				,@IN_REG_TASK
			)
			;

			-- 未貸出の予約明細と予約メンテナンスを削除
			IF @IN_CHG_ASSET_NO IS NOT NULL
				BEGIN
					DELETE FROM T_RESERVE_DETAIL WHERE RESERVE_ID = @IN_RESERVE_ID AND ASSET_NO = @IN_CHG_ASSET_NO AND RESERVE_STATUS = @s定数_予約ステータス_未貸出;
					DELETE FROM T_RESERVE_MAINTE WHERE RESERVE_ID = @IN_RESERVE_ID AND ASSET_NO = @IN_CHG_ASSET_NO;
				END;


			SET @n戻り値 = @n定数_戻り値_予約明細登録;
		END;

	SET @OUT_RETURN = @n戻り値;
END;