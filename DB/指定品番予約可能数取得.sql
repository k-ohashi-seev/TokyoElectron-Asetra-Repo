IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ASSET_MODEL_RESERVE_POSSIBLE_SF]'))
	DROP FUNCTION [dbo].[ASSET_MODEL_RESERVE_POSSIBLE_SF]
GO

CREATE FUNCTION [dbo].[ASSET_MODEL_RESERVE_POSSIBLE_SF]
--	*************************************************************************
--	システム名		:	ASETRA
--	プログラム名	:	指定品番予約可能数取得
--	プログラムID	:	ASSET_MODEL_RESERVE_POSSIBLE_SF
--	概要			:	
--	*************************************************************************
--パラメータ
--				：	@IN_PRODUCT_NO			NVARCHAR(30)
--				：	@IN_FROM_DT				INT
--				：	@IN_TO_DT				INT
--				：	@IN_KADO_JITU			CHAR(1)
--				：	@IN_CONST_RESERVE_ID	BIGINT
--	戻り値		：	TABLE
--	*************************************************************************
(
				 @IN_PRODUCT_NO			NVARCHAR(30)
				,@IN_FROM_DT			INT
				,@IN_TO_DT				INT
				,@IN_KADO_JITU			CHAR(1)
				,@IN_CONST_RESERVE_ID	BIGINT
)
RETURNS NVARCHAR(30)

BEGIN
	--変数
	DECLARE @iCnt				INT		= 0;
	DECLARE	@sAssetNo			NVARCHAR(24);
	DECLARE	@dLimitDt			INT;
    DECLARE	@iWarnMainteSumCount	INT = 0;  --警告件数の合計
    DECLARE	@iBanCount	INT = 0;              --重複禁止データ件数
    
    DECLARE	@sMainteDiv		    CHAR(1);		--メンテナンス区分
    DECLARE	@sMainteExecFlg		CHAR(1);        --メンテナンス実施フラグ
    DECLARE	@nWarnMainteKbn	    CHAR(1);        --警告区分
    
    DECLARE	@s定数_YES				CHAR(1)		= 'Y';
    DECLARE	@s定数_稼働日			CHAR(1)		= 'K';
	DECLARE	@s定数_実日				CHAR(1)		= 'J';
    DECLARE	@s定数_ロケ大区分		CHAR(1)		= 'L';
    
    DECLARE	@s定数_メンテ区分_通常	CHAR(1)		= 'N';
	
	DECLARE CUR_HINBAN CURSOR FOR 
		SELECT
			ASSET_NO
		FROM
			M_ASSET
		WHERE
			ISNULL(PRODUCT_NO, ' ')		= ISNULL(@IN_PRODUCT_NO, ' ')
		AND ASSET_STATUS	IN (1, 2)				--保管または貸出中
		AND LEASE_STATUS = '1'						--貸出可能
		;
	BEGIN

		OPEN CUR_HINBAN;

		FETCH NEXT FROM CUR_HINBAN INTO @sAssetNo;

		WHILE (@@FETCH_STATUS <> -1)
		BEGIN
		
			SELECT
				@dLimitDt = 
					dbo.RESERVE_LIMIT_SF
					(
						 @sAssetNo
						,@IN_CONST_RESERVE_ID
						,@IN_FROM_DT
						,@IN_TO_DT
						,@IN_KADO_JITU
					)
			;
    
                SELECT
    				 @sMainteExecFlg = MAINTE_EXEC_FLG
    				,@sMainteDiv = MAINTE_DIV
                    ,@nWarnMainteKbn = WARN_MAINTE_KBN
    			FROM
    				dbo.WARN_MAINTE_GET_SF
    				(
    					@sAssetNo
    					,@IN_CONST_RESERVE_ID
    					,@IN_FROM_DT
    					,@IN_TO_DT
    					,@IN_KADO_JITU
    				)
    			;
                
                --メンテナンスフラグがあるか
                IF @sMainteExecFlg = @s定数_YES
                    BEGIN
                        IF @sMainteDiv IS NOT NULL AND @sMainteDiv = @s定数_メンテ区分_通常
            			BEGIN
            				SET @iCnt = @iCnt + 1;
            		    END;  

                END
                ELSE
                    BEGIN
            			IF @dLimitDt IS NOT NULL AND @dLimitDt >= @IN_TO_DT
            			BEGIN
            				SET @iCnt = @iCnt + 1;
            			END;
                END

                    --警告区分が存在している場合
                    IF @nWarnMainteKbn IS NOT NULL
                    BEGIN
                        --取得した警告データをカウント
                        SET @iWarnMainteSumCount = @iWarnMainteSumCount + 1;
                    END
                    
    
    			FETCH NEXT FROM CUR_HINBAN INTO @sAssetNo;
        END;
		CLOSE		CUR_HINBAN;
		DEALLOCATE	CUR_HINBAN;

	END;
			
	--値を返す
	RETURN CAST(@iCnt AS NVARCHAR) + ',' + CAST(@iWarnMainteSumCount AS NVARCHAR);

END
GO
