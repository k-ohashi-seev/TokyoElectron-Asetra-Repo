IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ASSET_REPLACE_RESERVE_POSSIBLE_SP]'))
	DROP PROCEDURE [dbo].[ASSET_REPLACE_RESERVE_POSSIBLE_SP]
GO

CREATE PROCEDURE [dbo].[ASSET_REPLACE_RESERVE_POSSIBLE_SP]
--	*************************************************************************
--	システム名		:	ASETRA
--	プログラム名	:	代替品番予約更新
--	プログラムID	:	ASSET_REPLACE_RESERVE_POSSIBLE_SP
--	概要			:	
--	*************************************************************************
--パラメータ
--				：	@IN_PRODUCT_NO			NVARCHAR(30)
--				：	@IN_ASSET_NO			NVARCHAR(24)
--				：	@IN_FROM_DT				INT
--				：	@IN_TO_DT				INT
--				：	@IN_KADO_JITU			CHAR(1)
--				：	@IN_CONST_NO			NVARCHAR(20)
--				：	@IN_CONST_RESERVE_ID	BIGINT
--				：	@IN_REG_EMP_CD			NVARCHAR(15)
--				：	@IN_REG_EMP_NAME		NVARCHAR(60)
--				：	@IN_REG_TASK			NVARCHAR(255)
--				：	戻り値					INT
--	*************************************************************************
(
				 @IN_PRODUCT_NO			NVARCHAR(30)
				,@IN_ASSET_NO			NVARCHAR(24)
				,@IN_FROM_DT			INT
				,@IN_TO_DT				INT
				,@IN_KADO_JITU			CHAR(1)
				,@IN_CONST_NO			NVARCHAR(20)
				,@IN_CONST_RESERVE_ID	BIGINT
				,@IN_REG_EMP_CD			NVARCHAR(15)
				,@IN_REG_EMP_NAME		NVARCHAR(60)
				,@IN_REG_TASK			NVARCHAR(255)
)
AS
BEGIN
	--定数
	DECLARE	@s定数_予約ステータス_未貸出		CHAR(1)		= '0';
	DECLARE	@s定数_予約ステータス_貸出中		CHAR(1)		= '1';
	DECLARE	@s定数_メンテナンス編集_OK			CHAR(1)		= 'N';
	DECLARE	@s定数_貸出状態_可能				INT			= 1;
	DECLARE	@n定数_一次表_工事予約				INT			= 1;
	DECLARE	@n定数_一次表_未予約				INT			= 2;
	DECLARE	@s定数_戻り値_OK					INT			= 1;
	DECLARE	@s定数_戻り値_予約変更NG			INT			= 8;
	DECLARE	@s定数_戻り値_予約期間NG			INT			= 9;
	DECLARE	@n定数_戻り値_予約明細更新			INT			= 1;
	DECLARE	@n定数_戻り値_予約明細登録			INT			= 2;

	--変数
	DECLARE @sConstNo			NVARCHAR(255);
	DECLARE @sChgAssetNo		NVARCHAR(24);
	DECLARE	@nRealStartDt		INT;
	DECLARE	@nRealEndDt			INT;
	DECLARE @nReserveId			BIGINT;
	DECLARE @sReserveStatus		CHAR(1);
	DECLARE @sMainteExecFlg		CHAR(1);
	DECLARE @sMainteDiv			CHAR(1);
	DECLARE	@nStDt				INT;
	DECLARE	@nEndDt				INT;
	DECLARE	@nAnotherStDt		INT;
	DECLARE	@nAnotherEndDt		INT;
	DECLARE	@nLimitDt			INT;
	DECLARE @nDataCount			INT;
	DECLARE @nRank				INT;
	DECLARE @nRet				INT;
	DECLARE	@nWarnMainteKbn	    CHAR(1);
    DECLARE	@nBanMainteKbn	    CHAR(1);

	--警告データ用の変数を宣言
    DECLARE	@wReserveId			BIGINT;
    DECLARE	@wStDt				INT;
	DECLARE	@wEndDt				INT;
	DECLARE	@wRet				INT;
	DECLARE	@wChgAssetNo		NVARCHAR(24);
	DECLARE	@wRealStartDt		INT;
	DECLARE	@wRealEndDt			INT;
	DECLARE	@wAnotherStDt		INT;
	DECLARE	@wAnotherEndDt		INT;

	--※※※※※※※※※
	PRINT @IN_ASSET_NO;
	--※※※※※※※※※

	--差し替え対象の品番のリストと対象工事で予約している同一品番を一次表に登録する。
	IF NOT EXISTS(SELECT 1 FROM sys.objects WHERE name = '#HINBAN_ENTRY' AND type = 'U')
		BEGIN
			-- 未予約の同一品番のリストを作成
			CREATE TABLE #HINBAN_ENTRY (
				 ASSET_NO	NVARCHAR(24)
			);
			INSERT INTO #HINBAN_ENTRY(ASSET_NO)
				SELECT
					ASSET_NO
				FROM
					M_ASSET AST
				WHERE
					PRODUCT_NO = @IN_PRODUCT_NO
				AND ASSET_NO <> ISNULL(@IN_ASSET_NO, ' ')
				AND LEASE_STATUS = @s定数_貸出状態_可能
				AND NOT EXISTS(
						SELECT 1 FROM T_RESERVE_DETAIL DTL
						WHERE
							DTL.ASSET_NO = AST.ASSET_NO
						AND DTL.RESERVE_STATUS IN (@s定数_予約ステータス_未貸出, @s定数_予約ステータス_貸出中)
				)
			;

			-- 同一工事で同一品番で作成された予約のうち、未貸出のリストを作成
			CREATE TABLE #CONST_ENTRY (
				 ASSET_NO	NVARCHAR(24)
			);
			INSERT INTO #CONST_ENTRY(ASSET_NO)
				SELECT
					DTL.ASSET_NO
				FROM
					T_RESERVE_DETAIL DTL
					INNER JOIN T_RESERVE RES
						ON DTL.RESERVE_ID = RES.RESERVE_ID
					INNER JOIN M_ASSET AST
						ON DTL.ASSET_NO = AST.ASSET_NO
				WHERE
					DTL.ASSET_NO <> ISNULL(@IN_ASSET_NO, ' ')
				AND DTL.RESERVE_ID = @IN_CONST_RESERVE_ID
				AND DTL.RESERVE_STATUS = @s定数_予約ステータス_未貸出
				AND AST.PRODUCT_NO = @IN_PRODUCT_NO
			;

		END;

	--※※※※※※※※※
	SELECT @nRet = COUNT(1) FROM #HINBAN_ENTRY;
	PRINT CONCAT('#HINBAN_ENTRY',@nRet);
	SELECT @nRet = COUNT(1) FROM #CONST_ENTRY;
	PRINT CONCAT('##CONST_ENTRY',@nRet);
	--※※※※※※※※※

	-- 未貸出リストのカーソル
	DECLARE CUR_CONST_ENTRY CURSOR FOR 
		SELECT
			*
		FROM
		(
			SELECT
				 ASSET_NO
				,@n定数_一次表_工事予約 AS RANK_PERIOD
			FROM
				#CONST_ENTRY
			UNION
			SELECT
				 ASSET_NO
				,@n定数_一次表_未予約 AS RANK_PERIOD
			FROM
				#HINBAN_ENTRY
		) AS A
		ORDER BY
			 RANK_PERIOD
			,ASSET_NO
		;

	--引数の資産番号の設定有無で処理を分岐する
	IF @IN_ASSET_NO IS NOT NULL
		BEGIN
			-- 指定した資産番号の予約を抽出する。
			SELECT TOP 1
				 @sConstNo = RES.REMARK
				,@nReserveId = RES.RESERVE_ID
				,@nRealStartDt = DTL.REAL_START_DT
				,@nRealEndDt = DTL.REAL_END_DT
				,@sReserveStatus = DTL.RESERVE_STATUS
			FROM
				T_RESERVE_DETAIL DTL
				INNER JOIN T_RESERVE RES
					ON DTL.RESERVE_ID = RES.RESERVE_ID
			WHERE
				DTL.ASSET_NO = @IN_ASSET_NO
			AND DTL.RESERVE_STATUS IN (@s定数_予約ステータス_未貸出, @s定数_予約ステータス_貸出中)
			ORDER BY
				DTL.REAL_START_DT
			;

			SET @nDataCount = @@ROWCOUNT;
	--※※※※※※※※※
	PRINT CONCAT('予約件数確認 ',@nDataCount);
	--※※※※※※※※※

			IF @nDataCount = 0
				BEGIN
					--データ未抽出時は予約IDをNULLにする
					SET @nReserveId = NULL;
				END;

-- 			-- 引数の資産が引数の期間に変更できるかを確認する。
-- 			SELECT
-- 				 @sMainteExecFlg = MAINTE_EXEC_FLG
-- 				,@sMainteDiv = MAINTE_DIV
-- 				,@nStDt = ST_DT
-- 				,@nEndDt = END_DT
-- 				,@nLimitDt = LIMIT_DT
-- 			FROM
-- 				dbo.RESERVE_MAINTE_INFO_SF
-- 				(
-- 						@IN_ASSET_NO
-- 					,@nReserveId
-- 					,@IN_FROM_DT
-- 					,@IN_TO_DT
-- 					,@IN_KADO_JITU
-- 				)
-- 			;

							--代替品番が別の工事の期間に予約変更できるか確認する。
							SELECT
								 @sMainteExecFlg = MAINTE_EXEC_FLG
								,@sMainteDiv = MAINTE_DIV
								,@nAnotherStDt = ST_DT
								,@nAnotherEndDt = END_DT
								,@nLimitDt = LIMIT_DT
								,@nWarnMainteKbn = WARN_MAINTE_KBN
                                ,@nBanMainteKbn = BAN_MAINTE
							FROM
								dbo.WARN_MAINTE_GET_SF
								(
									 @IN_ASSET_NO
									,@nReserveId
									,@IN_FROM_DT
									,@IN_TO_DT
									,@IN_KADO_JITU
								)
							;
	--※※※※※※※※※
	PRINT CONCAT('期間変更結果 ',@sMainteDiv);
	--※※※※※※※※※

			IF @sMainteDiv <> @s定数_メンテナンス編集_OK OR @nBanMainteKbn IS NOT NULL
				BEGIN
					-- 持出対象が期間変更できない場合はエラーにする。
					RETURN @s定数_戻り値_予約期間NG;
				END;

	--※※※※※※※※※
	PRINT CONCAT('予約の工事番号 ',@sConstNo);
	--※※※※※※※※※

			IF (@nDataCount > 0 AND @sConstNo = @IN_CONST_NO) OR @nDataCount = 0
				BEGIN
					--予約明細と予約メンテナンスを登録/または更新する
					--新規に予約明細を登録した場合は
					SELECT @sChgAssetNo = MAX(ASSET_NO) FROM #CONST_ENTRY;
					EXEC dbo.RESERVE_DTL_UPDATE_SP
						 @IN_CONST_RESERVE_ID
						,@IN_ASSET_NO
						,@IN_FROM_DT
						,@IN_TO_DT
						,@nStDt
						,@nEndDt
						,@sChgAssetNo
						,@IN_REG_EMP_CD
						,@IN_REG_EMP_NAME
						,@IN_REG_TASK
						,@nRet
					;

					-- 引数の資産の期間変更が正常終了
					RETURN @s定数_戻り値_OK;
				END;

			ELSE
				BEGIN
					--引数の工事で登録していない予約の場合
					IF @sReserveStatus = @s定数_予約ステータス_貸出中
						BEGIN
							-- 既に貸出中だった場合は予約変更エラーで終わる
							RETURN @s定数_戻り値_予約変更NG;
						END;

	--※※※※※※※※※
	PRINT CONCAT('別の工事の予約を別品番にするSTART', ' ');
	--※※※※※※※※※
					--この資産の予約を別の品番に差し替える
					--別の工事の予約を当該工事で予約した同一品番で割当できないか確認する。
					OPEN CUR_CONST_ENTRY;
					FETCH NEXT FROM CUR_CONST_ENTRY INTO @sChgAssetNo, @nRank;

					WHILE (@@FETCH_STATUS <> -1)
						BEGIN
	--※※※※※※※※※
	PRINT CONCAT('代替品番 ', @sChgAssetNo);
	--※※※※※※※※※
							--代替品番が別の工事の期間に予約変更できるか確認する。
							SELECT
								 @sMainteExecFlg = MAINTE_EXEC_FLG
								,@sMainteDiv = MAINTE_DIV
								,@nAnotherStDt = ST_DT
								,@nAnotherEndDt = END_DT
								,@nLimitDt = LIMIT_DT
								,@nWarnMainteKbn = WARN_MAINTE_KBN
                                ,@nBanMainteKbn = BAN_MAINTE
							FROM
								dbo.WARN_MAINTE_GET_SF
								(
									 @sChgAssetNo
									,@IN_CONST_RESERVE_ID
									,@nRealStartDt
									,@nRealEndDt
									,@IN_KADO_JITU
								)
							;
	--※※※※※※※※※
	PRINT CONCAT('メンテナンス区分 ', @sMainteDiv);
	--※※※※※※※※※
							IF (@sMainteDiv = @s定数_メンテナンス編集_OK AND @nWarnMainteKbn IS NULL AND @nBanMainteKbn IS NULL) --重複禁止期間でない場合条件追加
								BEGIN
									--代替品番が見つかった場合は、別工事に紐づいていた予約明細を
									--引数に合わせた期間に更新する
									EXEC dbo.RESERVE_DTL_UPDATE_SP
										 @nReserveID
										,@IN_ASSET_NO
										,@IN_FROM_DT
										,@IN_TO_DT
										,@nStDt
										,@nEndDt
										,NULL
										,@IN_REG_EMP_CD
										,@IN_REG_EMP_NAME
										,@IN_REG_TASK
										,@nRet
									;

									--代替品番の期間を更新/または登録する
									EXEC dbo.RESERVE_DTL_UPDATE_SP
										 @IN_CONST_RESERVE_ID
										,@sChgAssetNo
										,@nRealStartDt
										,@nRealEndDt
										,@nAnotherStDt
										,@nAnotherEndDt
										,NULL
										,@IN_REG_EMP_CD
										,@IN_REG_EMP_NAME
										,@IN_REG_TASK
										,@nRet
									;


	--※※※※※※※※※
	PRINT CONCAT('代替更新 ', @nReserveID, ' ', @IN_ASSET_NO, ' ', @IN_CONST_RESERVE_ID);
	--※※※※※※※※※

									--それぞれの予約IDを更新する
									--引数の資産
									UPDATE
										T_RESERVE_DETAIL
									SET
										RESERVE_ID = @IN_CONST_RESERVE_ID
									WHERE
										RESERVE_ID = @nReserveID
									AND ASSET_NO = @IN_ASSET_NO
									;
									UPDATE
										T_RESERVE_MAINTE
									SET
										RESERVE_ID = @IN_CONST_RESERVE_ID
									WHERE
										RESERVE_ID = @nReserveID
									AND ASSET_NO = @IN_ASSET_NO
									;
									--代替資産
									UPDATE
										T_RESERVE_DETAIL
									SET
										RESERVE_ID = @nReserveId
									WHERE
										RESERVE_ID = @IN_CONST_RESERVE_ID
									AND ASSET_NO = @sChgAssetNo
									;
									UPDATE
										T_RESERVE_MAINTE
									SET
										RESERVE_ID = @nReserveId
									WHERE
										RESERVE_ID = @IN_CONST_RESERVE_ID
									AND ASSET_NO = @sChgAssetNo
									;

									-- 引数の資産の別資産への差し替えが完了
									CLOSE		CUR_CONST_ENTRY;
									DEALLOCATE	CUR_CONST_ENTRY;
									RETURN @s定数_戻り値_OK;
								END;


							--警告のメンテナンスデータを退避
						    IF (@sMainteDiv = @s定数_メンテナンス編集_OK AND @nWarnMainteKbn IS NOT NULL AND @nBanMainteKbn IS NULL) --重複禁止期間でない場合条件追加
							BEGIN
								SET @wReserveId    = @nReserveID;
								SET @wStDt         = @nStDt;
								SET @wEndDt        = @nEndDt;
								SET @wRet          = @nRet;
								SET @wChgAssetNo   = @sChgAssetNo;
								SET @wRealStartDt  = @nRealStartDt;
								SET @wRealEndDt    = @nRealEndDt;
								SET @wAnotherStDt  = @nAnotherStDt;
								SET @wAnotherEndDt = @nAnotherEndDt;
							END



							--次の品番を取得する
							FETCH NEXT FROM CUR_CONST_ENTRY INTO @sChgAssetNo, @nRank;
						END;
						--CLOSE		CUR_CONST_ENTRY;
						--DEALLOCATE	CUR_CONST_ENTRY;

						--警告なしのデータが取得できない場合、退避していた警告データで更新
						IF @wReserveId IS NOT NULL
						BEGIN
								BEGIN
									--代替品番が見つかった場合は、別工事に紐づいていた予約明細を
									--引数に合わせた期間に更新する
									EXEC dbo.RESERVE_DTL_UPDATE_SP
										 @wReserveId
										,@IN_ASSET_NO
										,@IN_FROM_DT
										,@IN_TO_DT
										,@wStDt
										,@wEndDt
										,NULL
										,@IN_REG_EMP_CD
										,@IN_REG_EMP_NAME
										,@IN_REG_TASK
										,@wRet
									;

									--代替品番の期間を更新/または登録する
									EXEC dbo.RESERVE_DTL_UPDATE_SP
										 @IN_CONST_RESERVE_ID
										,@wChgAssetNo
										,@wRealStartDt
										,@wRealEndDt
										,@wAnotherStDt
										,@wAnotherEndDt
										,NULL
										,@IN_REG_EMP_CD
										,@IN_REG_EMP_NAME
										,@IN_REG_TASK
										,@wRet
									;


									--それぞれの予約IDを更新する
									--引数の資産
									UPDATE
										T_RESERVE_DETAIL
									SET
										RESERVE_ID = @IN_CONST_RESERVE_ID
									WHERE
										RESERVE_ID = @wReserveId
									AND ASSET_NO = @IN_ASSET_NO
									;
									UPDATE
										T_RESERVE_MAINTE
									SET
										RESERVE_ID = @IN_CONST_RESERVE_ID
									WHERE
										RESERVE_ID = @wReserveId
									AND ASSET_NO = @IN_ASSET_NO
									;
									--代替資産
									UPDATE
										T_RESERVE_DETAIL
									SET
										RESERVE_ID = @wReserveId
									WHERE
										RESERVE_ID = @IN_CONST_RESERVE_ID
									AND ASSET_NO = @wChgAssetNo
									;
									UPDATE
										T_RESERVE_MAINTE
									SET
										RESERVE_ID = @wReserveId
									WHERE
										RESERVE_ID = @IN_CONST_RESERVE_ID
									AND ASSET_NO = @wChgAssetNo
									;

									-- 引数の資産の別資産への差し替えが完了
									CLOSE		CUR_CONST_ENTRY;
									DEALLOCATE	CUR_CONST_ENTRY;
									RETURN @s定数_戻り値_OK;
								END;
						END

						-- 引数の資産を別の資産に割当できなかった
						CLOSE		CUR_CONST_ENTRY;
						DEALLOCATE	CUR_CONST_ENTRY;
						RETURN @s定数_戻り値_予約変更NG;
				END;
		END;

	ELSE
		BEGIN
			--品番の指定がない場合は、予約のない品番から予約する。

			OPEN CUR_CONST_ENTRY;
			FETCH NEXT FROM CUR_CONST_ENTRY INTO @sChgAssetNo, @nRank;

	--※※※※※※※※※
	PRINT CONCAT('変更品番 ', @sChgAssetNo);
	--※※※※※※※※※
			WHILE (@@FETCH_STATUS <> -1)
				BEGIN
					IF @nRank = @n定数_一次表_工事予約
						BEGIN
	--※※※※※※※※※
	PRINT '工事予約用なのでSKIP';
	--※※※※※※※※※

							--既に当該工事で予約している資産の場合はSKIP
							--次の品番を取得する
							FETCH NEXT FROM CUR_CONST_ENTRY INTO @sChgAssetNo, @nRank;
	--※※※※※※※※※
	PRINT CONCAT('変更品番2 ', @sChgAssetNo);
	--※※※※※※※※※
							CONTINUE;
						END;
	--※※※※※※※※※
	PRINT @sChgAssetNo;
	PRINT @IN_CONST_RESERVE_ID
	PRINT @IN_FROM_DT
	PRINT @IN_TO_DT
	PRINT @IN_KADO_JITU
	--※※※※※※※※※
					--代替品番が別の工事の期間に予約変更できるか確認する。
					SELECT
						 @sMainteExecFlg = MAINTE_EXEC_FLG
						,@sMainteDiv = MAINTE_DIV
						,@nStDt = ST_DT
						,@nEndDt = END_DT
						,@nLimitDt = LIMIT_DT
						,@nWarnMainteKbn = WARN_MAINTE_KBN
                        ,@nBanMainteKbn = BAN_MAINTE
					FROM
						dbo.WARN_MAINTE_GET_SF
						(
							 @sChgAssetNo
							,@IN_CONST_RESERVE_ID
							,@IN_FROM_DT
							,@IN_TO_DT
							,@IN_KADO_JITU
						)		
					;

					IF (@sMainteDiv = @s定数_メンテナンス編集_OK AND @nWarnMainteKbn IS NULL AND @nBanMainteKbn IS NULL) --重複禁止期間でない場合条件追加
						BEGIN
							--予約明細とメンテナンスデータを登録する
							EXEC dbo.RESERVE_DTL_UPDATE_SP
								 @IN_CONST_RESERVE_ID
								,@sChgAssetNo
								,@IN_FROM_DT
								,@IN_TO_DT
								,@nStDt
								,@nEndDt
								,NULL
								,@IN_REG_EMP_CD
								,@IN_REG_EMP_NAME
								,@IN_REG_TASK
								,@nRet
							;

							-- 引数の資産の別資産への差し替えが完了
							CLOSE		CUR_CONST_ENTRY;
							DEALLOCATE	CUR_CONST_ENTRY;
							RETURN @s定数_戻り値_OK;
						END;


				    --警告のメンテナンスデータを退避
					IF (@sMainteDiv = @s定数_メンテナンス編集_OK AND @nWarnMainteKbn IS NOT NULL AND @nBanMainteKbn IS NULL) --重複禁止期間でない場合条件追加
					BEGIN
						SET @wStDt         = @nStDt;
						SET @wEndDt        = @nEndDt;
						SET @wRet          = @nRet;
						SET @wChgAssetNo   = @sChgAssetNo;
					END

					--次の品番を取得する
					FETCH NEXT FROM CUR_CONST_ENTRY INTO @sChgAssetNo, @nRank;
				END;

			--CLOSE		CUR_CONST_ENTRY;
			--DEALLOCATE	CUR_CONST_ENTRY;

						--警告なしのデータが取得できない場合、退避していた警告データで更新
						IF @wChgAssetNo IS NOT NULL
						BEGIN
							--予約明細とメンテナンスデータを登録する
							EXEC dbo.RESERVE_DTL_UPDATE_SP
								 @IN_CONST_RESERVE_ID
								,@wChgAssetNo
								,@IN_FROM_DT
								,@IN_TO_DT
								,@wStDt
								,@wEndDt
								,NULL
								,@IN_REG_EMP_CD
								,@IN_REG_EMP_NAME
								,@IN_REG_TASK
								,@wRet
							;

							-- 引数の資産の別資産への差し替えが完了
							CLOSE		CUR_CONST_ENTRY;
							DEALLOCATE	CUR_CONST_ENTRY;
							RETURN @s定数_戻り値_OK;
						END


			-- 引数の資産を別の資産に割当できなかった
			CLOSE		CUR_CONST_ENTRY;
			DEALLOCATE	CUR_CONST_ENTRY;
			RETURN  @s定数_戻り値_予約変更NG;
		END;
END;
GO


