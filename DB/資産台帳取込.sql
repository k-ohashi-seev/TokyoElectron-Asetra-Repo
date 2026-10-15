IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[IMPORT_ASSET_FILE]'))
	DROP PROCEDURE [dbo].IMPORT_ASSET_FILE
GO
CREATE PROCEDURE [dbo].IMPORT_ASSET_FILE
--	*************************************************************************
--	システム名	:	Asetra
--	プログラム名	:	資産台帳取込
--	プログラムID	:	IMPORT_ASSET_FILE
--	概要		:	
--	*************************************************************************
--パラメータ
--			：	@IN_TMPTBL_NAME			NVARCHAR(30)
--			：	@IN_LOC_L_CD			NVARCHAR(10)
--			：	@OUT_RETURN				INT
--	*************************************************************************
(
				 @IN_TMPTBL_NAME		NVARCHAR(30)
				,@IN_LOC_L_CD			NVARCHAR(10)
				,@OUT_RETURN			INT OUTPUT
				,@OUT_MESSAGE			NVARCHAR(1000) OUTPUT
)
AS
BEGIN
	    
    SET NOCOUNT ON;

    --定数
    DECLARE	@CONST_ASSET_EXISTS		CHAR(1)		= 'A';		--更新フラグ　資産マスタあり
    DECLARE	@CONST_ASSET_UPDATE		CHAR(1)		= 'B';		--更新フラグ　資産マスタ更新
    DECLARE	@CONST_ASSET_INSERT		CHAR(1)		= 'C';		--更新フラグ　資産マスタ追加
    DECLARE	@CONST_IN_EXISTS		CHAR(1)		= 'D';		--更新フラグ　入庫予定テーブルあり
    DECLARE	@CONST_IN_UPDATE		CHAR(1)		= 'E';		--更新フラグ　入庫予定テーブル更新
    DECLARE	@CONST_IN_INSERT		CHAR(1)		= 'F';		--更新フラグ　入庫予定テーブル追加
	
    --変数
    DECLARE @RfidFlg CHAR(1);				--RFIDタグ拠点
    DECLARE @EmployeeCD NVARCHAR(15);		--登録担当者コード
    DECLARE @EmployeeNm NVARCHAR(60);		--登録担当者名
    DECLARE @UpdTask NVARCHAR(255);			--登録担当者名
    DECLARE @sql NVARCHAR(MAX);				--SQL
    DECLARE @Condition NVARCHAR(MAX);		--更新有無確認条件
    DECLARE @InsColList NVARCHAR(MAX);		--追加列SQL
    DECLARE @InsValList NVARCHAR(MAX);		--追加値SQL
    DECLARE @UpdColList NVARCHAR(MAX);		--更新SQL
    DECLARE @InsProcess NVARCHAR(MAX);		--資産処理履歴追加SQL
    DECLARE @InsOpeLog NVARCHAR(MAX);		--ログ追加SQL
    DECLARE @Count INT = 0;					--更新件数
    DECLARE @i INT = 1;						--カウンタ
    DECLARE @Contents NVARCHAR(1024);		--ログメッセージ

    -- ==================================================
    -- ログ追加SQL作成
    -- ==================================================
    --ログ追加SQL
    SET @InsOpeLog= N'
        INSERT INTO T_OPERATION_LOG (
        	PROCESS_DT,PROCESS_TM,PROCESS_TOOL_DIV,PROCESS_CAT,LOC_L_CD,PROCESS_CONTENTS
			,OPE_EMPLOYEE_CD,OPE_EMPLOYEE_NAME
			,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
        ) VALUES (
        	CONVERT(int, FORMAT(GetDate(), ''yyyyMMdd'')),FORMAT(GetDate(), ''HHmmssfff'')
        	,''W'',@ProcessCat,@LocLCd,@Contents,@EmployeeCD,@EmployeeNM
        	,GETDATE(), @EmployeeCD, @EmployeeNM,@UpdTask
        )
    ';
        
    BEGIN TRY
        -- ==================================================
        -- 引数確認
        -- ==================================================
        --引数がNULLかどうかを確認
        IF TRIM(ISNULL(@IN_TMPTBL_NAME, '')) = ''
        BEGIN
            PRINT '引数.テーブル名なし';
            SET @OUT_RETURN = 8;
        	RETURN;
        END
        IF TRIM(ISNULL(@IN_LOC_L_CD, '')) = ''
        BEGIN
            PRINT '引数.ロケ大区分コードなし';
            SET @OUT_RETURN = 8;
        	RETURN;
        END
        
        -- ==================================================
        -- RFIDタグ拠点取得
        -- ==================================================
        SELECT @RfidFlg = LOC_RFID_FLG
        FROM M_LOCATION
        WHERE LOC_DIV = 'L' AND LOC_L_CD = @IN_LOC_L_CD;
        IF @RfidFlg IS NULL
        BEGIN
        	--ロケマスタ大区分なし、RFIDタグ拠点設定なしの場合、エラーとする
        	PRINT 'ロケマスタ大区分なし';
        	SET @OUT_RETURN = 7;
        	RETURN;
        END
        
         -- ==================================================
        -- 登録担当者取得
        -- ==================================================
        SET @sql = N'
        	SELECT TOP 1
        	@EmployeeCD = IP_EMPLOYEE_CD,
        	@EmployeeNm = IP_EMPLOYEE_NAME,
        	@UpdTask    = UPD_TASK
    		FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ';
		';
        EXEC sp_executesql 
            @sql,
            N'@EmployeeCD  NVARCHAR(15) OUTPUT,@EmployeeNm  NVARCHAR(60) OUTPUT,@UpdTask  NVARCHAR(255) OUTPUT',
            @EmployeeCD = @EmployeeCD OUTPUT,
            @EmployeeNm = @EmployeeNm OUTPUT,
            @UpdTask = @UpdTask OUTPUT
            ;
            
        IF @EmployeeCD IS NULL
        BEGIN
        	--登録担当者なしの場合、エラーとする
        	PRINT '登録データなし';
        	SET @OUT_RETURN = 6;
        	RETURN;
        END

		-- ==================================================
        -- ロケマスタ大区分
        -- ==================================================
        -- UPDATE
        SET @sql = N'
            UPDATE LOC
            SET LOC.LOC_NAME = W.LOC_L_NAME
            ,LOC.UPD_DATETIME = GETDATE(),LOC.UPD_EMPLOYEE_CD = @EmployeeCD,LOC.UPD_EMPLOYEE_NAME =  @EmployeeNM,LOC.UPD_TASK = @UpdTask
            FROM  M_LOCATION AS LOC
            INNER JOIN (
            	SELECT LOC_L_CD,MIN(LOC_L_NAME) LOC_L_NAME
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' 
            	GROUP BY LOC_L_CD
            ) W
            ON LOC.LOC_DIV =''L'' AND W.LOC_L_CD=LOC.LOC_L_CD 
            WHERE ISNULL(W.LOC_L_NAME, '''') <> ISNULL(LOC.LOC_NAME, '''');
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT 'ロケマスタ大区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = 'ロケマスタ大区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        -- ==================================================
        -- ロケマスタ中区分
        -- ==================================================
        -- UPDATE
        SET @sql = N'
            UPDATE LOC
            SET LOC.LOC_NAME = W.LOC_M_NAME
            ,LOC.UPD_DATETIME = GETDATE(),LOC.UPD_EMPLOYEE_CD = @EmployeeCD,LOC.UPD_EMPLOYEE_NAME =  @EmployeeNM,LOC.UPD_TASK = @UpdTask
            FROM  M_LOCATION AS LOC
            INNER JOIN (
            	SELECT LOC_L_CD,LOC_M_CD,MIN(LOC_M_NAME) LOC_M_NAME
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' 
            	GROUP BY LOC_L_CD,LOC_M_CD
            ) W
            ON LOC.LOC_DIV =''M'' AND W.LOC_L_CD=LOC.LOC_L_CD AND W.LOC_M_CD=LOC.LOC_M_CD 
            WHERE ISNULL(W.LOC_M_NAME, '''') <> ISNULL(LOC.LOC_NAME, '''')
            AND TRIM(ISNULL(W.LOC_M_CD, '''')) <> ''''
            ;
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT 'ロケマスタ中区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = 'ロケマスタ中区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;

        -- INSERT
        SET @sql = N'
            INSERT INTO M_LOCATION (
            	LOC_DIV,LOC_L_CD,LOC_M_CD,LOC_S_CD,LOC_NAME,LOC_SHARE_FLG
            	,INS_DATETIME,INS_EMPLOYEE_CD,INS_EMPLOYEE_NAME,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            )
            SELECT ''M'',W.LOC_L_CD,W.LOC_M_CD,'' '',MIN(W.LOC_M_NAME),''N''
            ,GETDATE(),@EmployeeCD,@EmployeeNM,GETDATE(),@EmployeeCD,@EmployeeNM,@UpdTask
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            LEFT JOIN M_LOCATION AS LOC
            ON LOC.LOC_DIV =''M'' AND W.LOC_L_CD=LOC.LOC_L_CD AND W.LOC_M_CD=LOC.LOC_M_CD 
            WHERE LOC.LOC_L_CD IS NULL
            AND TRIM(ISNULL(W.LOC_M_CD, '''')) <> ''''
            GROUP BY W.LOC_L_CD,W.LOC_M_CD
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT 'ロケマスタ中区分挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = 'ロケマスタ中区分挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        -- ==================================================
        -- ロケマスタ小区分
        -- ==================================================
        -- UPDATE
        SET @sql = N'
            UPDATE LOC
            SET LOC.LOC_NAME = W.LOC_S_NAME
            ,LOC.UPD_DATETIME = GETDATE(),LOC.UPD_EMPLOYEE_CD = @EmployeeCD,LOC.UPD_EMPLOYEE_NAME =  @EmployeeNM,LOC.UPD_TASK = @UpdTask
            FROM  M_LOCATION AS LOC
            INNER JOIN (
            	SELECT LOC_L_CD,LOC_M_CD,LOC_S_CD,MIN(LOC_S_NAME) LOC_S_NAME
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' 
            	GROUP BY LOC_L_CD,LOC_M_CD,LOC_S_CD
            ) W
            ON LOC.LOC_DIV =''S'' AND W.LOC_L_CD=LOC.LOC_L_CD AND W.LOC_M_CD=LOC.LOC_M_CD AND W.LOC_S_CD=LOC.LOC_S_CD
            WHERE ISNULL(W.LOC_S_NAME, '''') <> ISNULL(LOC.LOC_NAME, '''')
            AND TRIM(ISNULL(W.LOC_M_CD, '''')) <> '''' AND TRIM(ISNULL(W.LOC_S_CD, '''')) <> ''''
            ;
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT 'ロケマスタ小区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = 'ロケマスタ小区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;

        -- INSERT
        SET @sql = N'
            INSERT INTO M_LOCATION (
            	LOC_DIV,LOC_L_CD,LOC_M_CD,LOC_S_CD,LOC_NAME,LOC_SHARE_FLG
            	,INS_DATETIME,INS_EMPLOYEE_CD,INS_EMPLOYEE_NAME,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            )
            SELECT ''S'',W.LOC_L_CD,W.LOC_M_CD,W.LOC_S_CD,MIN(W.LOC_S_NAME),''N''
            ,GETDATE(),@EmployeeCD,@EmployeeNM,GETDATE(),@EmployeeCD,@EmployeeNM,@UpdTask
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' AS W
            LEFT JOIN M_LOCATION AS LOC
            ON LOC.LOC_DIV =''S'' AND W.LOC_L_CD=LOC.LOC_L_CD AND W.LOC_M_CD=LOC.LOC_M_CD AND W.LOC_S_CD=LOC.LOC_S_CD
            WHERE LOC.LOC_L_CD IS NULL
            AND TRIM(ISNULL(W.LOC_M_CD, '''')) <> '''' AND TRIM(ISNULL(W.LOC_S_CD, '''')) <> ''''
            GROUP BY W.LOC_L_CD,W.LOC_M_CD,W.LOC_S_CD
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT 'ロケマスタ小区分挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = 'ロケマスタ小区分挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        -- ==================================================
        -- カテゴリマスタ大区分
        -- ==================================================
        -- UPDATE
        SET @sql = N'
            UPDATE CAT
            SET CAT.CAT_NAME = W.CAT_L_NAME
            ,CAT.UPD_DATETIME = GETDATE(),CAT.UPD_EMPLOYEE_CD = @EmployeeCD,CAT.UPD_EMPLOYEE_NAME = @EmployeeNM,CAT.UPD_TASK = @UpdTask
            FROM  M_CATEGORY AS CAT
            INNER JOIN (
            	SELECT CAT_L_CD,MIN(CAT_L_NAME) CAT_L_NAME
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' 
            	GROUP BY CAT_L_CD
            ) W
            ON CAT.CAT_DIV =''L'' AND W.CAT_L_CD=CAT.CAT_L_CD 
            WHERE ISNULL(W.CAT_L_NAME, '''') <> ISNULL(CAT.CAT_NAME, '''')
            AND TRIM(ISNULL(W.CAT_L_CD, '''')) <> ''''
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT 'カテゴリマスタ大区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = 'カテゴリマスタ大区分更新件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;

        -- INSERT
        SET @sql = N'
            INSERT INTO M_CATEGORY (
            	CAT_DIV,CAT_L_CD,CAT_M_CD,CAT_S_CD,CAT_NAME,ASSET_ID_REG_DIV,EPC_SIZE
            	,LEASE_LIMIT_ALERT_DIV,GENERIC_LIMIT_ALERT_DIV,LEASE_ALERT_DIV,LEASE_UPPER_ERR_DIV
            	,INS_DATETIME,INS_EMPLOYEE_CD,INS_EMPLOYEE_NAME,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            )
            SELECT ''L'',W.CAT_L_CD,'' '','' '',MIN(W.CAT_L_NAME),''3'',''096'',''N'',''N'',''N'',''N''
            ,GETDATE(),@EmployeeCD,@EmployeeNM,GETDATE(),@EmployeeCD,@EmployeeNM,@UpdTask
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            LEFT JOIN M_CATEGORY AS CAT
            ON CAT.CAT_DIV =''L'' AND W.CAT_L_CD=CAT.CAT_L_CD
            WHERE CAT.CAT_L_CD IS NULL
            AND TRIM(ISNULL(W.CAT_L_CD, '''')) <> ''''
            GROUP BY W.CAT_L_CD
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT 'カテゴリマスタ大区分挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = 'カテゴリマスタ大区分挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        -- ==================================================
        -- 社員マスタ(INSERTのみ)
        -- ==================================================
            SET @sql = N'
            INSERT INTO M_EMPLOYEE (
            	EMPLOYEE_CD,EMPLOYEE_NAME,DEPARTMENT_CD,AUTH_DIV,RETIRE_FLG
            	,INS_DATETIME,INS_EMPLOYEE_CD,INS_EMPLOYEE_NAME,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            )
            SELECT W.FREE_ITEM6,W.FREE_ITEM6,''002'',''N'',''N''
            ,GETDATE(),@EmployeeCD,@EmployeeNM,GETDATE(),@EmployeeCD,@EmployeeNM,@UpdTask
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            LEFT JOIN M_EMPLOYEE AS EMP
            ON W.FREE_ITEM6=EMP.EMPLOYEE_CD
            WHERE EMP.EMPLOYEE_CD IS NULL
            AND TRIM(ISNULL(W.FREE_ITEM6, '''')) <> ''''
            GROUP BY W.FREE_ITEM6
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT '社員マスタ挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '社員マスタ挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        -- ==================================================
        -- 社員所属拠点マスタ(INSERTのみ)
        -- ==================================================
            SET @sql = N'
            INSERT INTO M_EMPLOYEE_HUB (
            	EMPLOYEE_CD,HUB_LOC_L_CD,HUB_LOC_M_CD
            	,INS_DATETIME,INS_EMPLOYEE_CD,INS_EMPLOYEE_NAME,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            )
            SELECT W.FREE_ITEM6,W.LOC_L_CD,W.LOC_M_CD
            ,GETDATE(),@EmployeeCD,@EmployeeNM,GETDATE(),@EmployeeCD,@EmployeeNM,@UpdTask
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            LEFT JOIN M_EMPLOYEE_HUB AS HUB
            ON W.FREE_ITEM6=HUB.EMPLOYEE_CD AND W.LOC_L_CD=HUB.HUB_LOC_L_CD AND W.LOC_M_CD=HUB.HUB_LOC_M_CD
            WHERE HUB.EMPLOYEE_CD IS NULL
            AND TRIM(ISNULL(W.LOC_L_CD, '''')) <> ''''
            AND TRIM(ISNULL(W.FREE_ITEM6, '''')) <> ''''
            GROUP BY W.FREE_ITEM6,W.LOC_L_CD,W.LOC_M_CD
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@Count_OUT INT OUTPUT,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT '社員所属拠点マスタ挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '社員所属拠点マスタ挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;

        -- ==================================================
        -- 比較条件、列名リスト、Set列名リスト作成
        -- ==================================================
        SET @Condition = ' ISNULL(W.SUB_ASSET_NO, '''') <> ISNULL(A.SUB_ASSET_NO, '''')';
        SET @Condition += ' OR ISNULL(W.ASSET_NAME, '''') <> ISNULL(A.ASSET_NAME, '''')';
        SET @Condition += ' OR ISNULL(W.MNG_LOC_L_CD, '''') <> ISNULL(A.MNG_LOC_L_CD, '''')';
        SET @Condition += ' OR ISNULL(W.LOC_L_CD, '''') <> ISNULL(A.LOC_L_CD, '''')';
        SET @Condition += ' OR ISNULL(W.LOC_M_CD, '''') <> ISNULL(A.LOC_M_CD, '''')';
        SET @Condition += ' OR ISNULL(W.LOC_S_CD, '''') <> ISNULL(A.LOC_S_CD, '''')';
        SET @Condition += ' OR ISNULL(W.CAT_L_CD, '''') <> ISNULL(A.CAT_L_CD, '''')';
        SET @InsColList = ' ASSET_NO,SUB_ASSET_NO,ASSET_NAME,MNG_LOC_L_CD,LOC_L_CD,LOC_M_CD,LOC_S_CD,CAT_L_CD,ASSET_STATUS';
        SET @InsValList = ' W.ASSET_NO,W.SUB_ASSET_NO,W.ASSET_NAME,W.MNG_LOC_L_CD,W.LOC_L_CD,W.LOC_M_CD,W.LOC_S_CD,W.CAT_L_CD,W.ASSET_STATUS';
        SET @UpdColList = ' A.SUB_ASSET_NO = W.SUB_ASSET_NO';
        SET @UpdColList += ' ,A.ASSET_NAME = W.ASSET_NAME';
        SET @UpdColList += ' ,A.MNG_LOC_L_CD = W.MNG_LOC_L_CD';
        SET @UpdColList += ' ,A.LOC_L_CD = W.LOC_L_CD';
        SET @UpdColList += ' ,A.LOC_M_CD = W.LOC_M_CD';
        SET @UpdColList += ' ,A.LOC_S_CD = W.LOC_S_CD';
        SET @UpdColList += ' ,A.CAT_L_CD = W.CAT_L_CD';
        
        WHILE @i <= 60
        BEGIN
            SET @Condition += ' OR ISNULL(W.FREE_ITEM' + CAST(@i AS NVARCHAR(2)) + ', '''') <> ISNULL(A.FREE_ITEM' + CAST(@i AS NVARCHAR(2)) + ', '''')';
            SET @InsColList += ' ,FREE_ITEM' + CAST(@i AS NVARCHAR(2));
            SET @InsValList += ' ,W.FREE_ITEM' + CAST(@i AS NVARCHAR(2));
            SET @UpdColList += ' ,A.FREE_ITEM' + CAST(@i AS NVARCHAR(2)) + ' = W.FREE_ITEM' + CAST(@i AS NVARCHAR(2));
            SET @i += 1;
        END
        
        SET @InsColList += ',IS_TANGIBLE'
        SET @InsColList += ',INS_DATETIME,INS_EMPLOYEE_CD,INS_EMPLOYEE_NAME,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK';
        SET @InsValList += ', W.IS_TANGIBLE'
        SET @InsValList += ',GETDATE(),W.IP_EMPLOYEE_CD,W.IP_EMPLOYEE_NAME,GETDATE(),W.IP_EMPLOYEE_CD,W.IP_EMPLOYEE_NAME,W.UPD_TASK';
        SET @UpdColList += ' ,A.IS_TANGIBLE = W.IS_TANGIBLE'
        SET @UpdColList += ' ,A.UPD_DATETIME = GETDATE()';
        SET @UpdColList += ' ,A.UPD_EMPLOYEE_CD = W.IP_EMPLOYEE_CD';
        SET @UpdColList += ' ,A.UPD_EMPLOYEE_NAME = W.IP_EMPLOYEE_NAME';
        SET @UpdColList += ' ,A.UPD_TASK = W.UPD_TASK';
        
        --資産処理履歴追加SQL
        SET @InsProcess= N'
            INSERT INTO T_PROCESS_LOG (
            	PROCESS_DT,PROCESS_TM,ASSET_NO,ASSET_ID,PROCESS_TOOL_DIV,PROCESS_DIV
            	,LEASE_PLACE_CD,LEASE_PLACE_NAME
            	,REMARK,LEASE_EMPLOYEE_CD,LEASE_EMPLOYEE_NAME,LEASE_DT,LEASE_DUE_DT
            	,LOC_L_CD,LOC_M_CD,LOC_S_CD,LOC_L_NAME,LOC_M_NAME,LOC_S_NAME
            	,ASSET_NAME,MAKER,PRODUCT_NO,MODEL_NO,MNG_LOC_L_CD,LEASE_STATUS,TASK_RESULT
            	,REMARK1,REMARK2,REMARK3,REMARK4,REMARK5,REMARK6,REMARK7,REMARK8,REMARK9,REMARK10
            	,SUB_ASSET_NO,CAT_L_CD,CAT_NAME
            	,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            )
            SELECT CONVERT(int, FORMAT(GetDate(), ''yyyyMMdd'')),FORMAT(GetDate(), ''HHmmssfff'')
            ,A.ASSET_NO,A.ASSET_ID,''W'',@ToolDiv_OUT
            ,A.LEASE_PLACE_CD,A.LEASE_PLACE_NAME
            ,A.REMARK,A.LEASE_EMPLOYEE_CD,A.LEASE_EMPLOYEE_NAME,A.LEASE_DT,A.LEASE_DUE_DT
            ,A.LOC_L_CD,A.LOC_M_CD,A.LOC_S_CD,W.LOC_L_NAME,W.LOC_M_NAME,W.LOC_S_NAME
            ,A.ASSET_NAME,A.MAKER,PRODUCT_NO,A.MODEL_NO,A.MNG_LOC_L_CD,A.LEASE_STATUS,''N''
            ,A.REMARK1,A.REMARK2,A.REMARK3,A.REMARK4,A.REMARK5,A.REMARK6,A.REMARK7,A.REMARK8,A.REMARK9,A.REMARK10
            ,A.SUB_ASSET_NO,A.CAT_L_CD,W.CAT_L_NAME
            ,GETDATE(),W.IP_EMPLOYEE_CD,W.IP_EMPLOYEE_NAME,W.UPD_TASK
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            INNER JOIN M_ASSET AS A
            ON W.ASSET_NO = A.ASSET_NO
            WHERE W.UPD_FLG = @UpdFlg_OUT
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        -- ==================================================
        -- 更新フラグ設定(資産マスタ)
        -- ==================================================
        --資産マスタあり
        SET @sql = N'
            UPDATE W
            SET W.UPD_FLG = @UpdFlg_OUT
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' AS W
            INNER JOIN M_ASSET AS A
            ON W.ASSET_NO = A.ASSET_NO
                
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            @UpdFlg_OUT = @CONST_ASSET_EXISTS,
            @Count_OUT = @Count OUTPUT;

        PRINT '更新フラグ資産マスタあり件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '更新フラグ資産マスタあり件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;

        --資産マスタ変更あり
        SET @sql = N'
            UPDATE W
            SET W.UPD_FLG = @UpdFlg_OUT
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' AS W
            INNER JOIN M_ASSET AS A
            ON W.ASSET_NO = A.ASSET_NO
            WHERE ' + @Condition + '
        
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            @UpdFlg_OUT = @CONST_ASSET_UPDATE,
            @Count_OUT = @Count OUTPUT;

        PRINT '更新フラグ資産マスタ更新件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '更新フラグ資産マスタ更新件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;

        -- ==================================================
        -- 資産マスタ更新
        -- ==================================================
        --UPDATE　更新フラグ=Bのもの
        SET @sql = N'
            UPDATE A
            SET ' + @UpdColList + N'
            ,TASK_TS = FORMAT(GetDate(), ''yyyyMMddHHmmss'')
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' AS W
            INNER JOIN M_ASSET AS A
            ON W.ASSET_NO = A.ASSET_NO
            WHERE W.UPD_FLG = @UpdFlg_OUT
        
            SELECT @Count_OUT = @@ROWCOUNT;
    	';

        EXEC sp_executesql 
            @sql,
            N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            @UpdFlg_OUT = @CONST_ASSET_UPDATE,
            @Count_OUT = @Count OUTPUT;

        PRINT '資産マスタ更新件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '資産マスタ更新件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        --資産処理履歴INSERT　更新フラグ=Bのもの
        EXEC sp_executesql 
            @InsProcess,
            N'@UpdFlg_OUT CHAR(1), @ToolDiv_OUT CHAR(2), @Count_OUT INT OUTPUT',
            @UpdFlg_OUT = @CONST_ASSET_UPDATE,
            @ToolDiv_OUT ='CU',
            @Count_OUT = @Count OUTPUT;

        PRINT '資産処理履歴(CU)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '資産処理履歴(CU)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        
        IF @RfidFlg = 'N'
        BEGIN
        	--バーコード運用拠点の場合、資産マスタに追加
        	-- ==================================================
        	-- 資産マスタ追加
        	-- ==================================================
        	--更新フラグ=Cに更新
        	SET @sql = N'
        		UPDATE  ' + QUOTENAME(@IN_TMPTBL_NAME) + '
        		SET UPD_FLG = @UpdFlg_OUT
        		WHERE TRIM(ISNULL(UPD_FLG, '''')) = ''''
        	';
        	
        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1)',
            	@UpdFlg_OUT = @CONST_ASSET_INSERT;
            
            --INSERT　更新フラグ=Bのもの
            SET @sql = N'
            	INSERT INTO M_ASSET ( ' + @InsColList + N' 
            	,CAT_M_CD,CAT_S_CD,TASK_TS,TAG_LINK_STATUS
            	)
            	SELECT' + @InsValList + N'
            	,'' '','' '',FORMAT(GetDate(), ''yyyyMMddHHmmss''),''0''
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            	WHERE W.UPD_FLG = @UpdFlg_OUT
            	
            	SELECT @Count_OUT = @@ROWCOUNT;
        	';

        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_ASSET_INSERT,
            	@Count_OUT = @Count OUTPUT;

        	PRINT '資産マスタ挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        	SET @Contents = '資産マスタ挿入件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;
        	
        	 --資産処理履歴INSERT　更新フラグ='C'のもの
        	EXEC sp_executesql 
            	@InsProcess,
            	N'@UpdFlg_OUT CHAR(1), @ToolDiv_OUT CHAR(2), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_ASSET_INSERT,
            	@ToolDiv_OUT ='UI',
            	@Count_OUT = @Count OUTPUT;

        	PRINT '資産処理履歴(UI)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        	SET @Contents = '資産処理履歴(UI)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;
        END
        ELSE
        BEGIN
        	 --RFID拠点の場合、入庫予定テーブルに追加
        	-- ==================================================
        	-- 更新フラグ設定(入庫予定)
        	-- ==================================================
        	--入庫予定あり
        	SET @sql = N'
            	UPDATE W
            	SET W.UPD_FLG = @UpdFlg_OUT
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' AS W
            	INNER JOIN T_IN_ASSET AS A
            	ON W.ASSET_NO = A.ASSET_NO
            	WHERE TRIM(ISNULL(UPD_FLG, '''')) = ''''
                
            	SELECT @Count_OUT = @@ROWCOUNT;
        	';

        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_IN_EXISTS,
            	@Count_OUT = @Count OUTPUT;

        	PRINT '更新フラグ入庫予定あり件数: ' + CAST(@Count AS NVARCHAR(10));
        	SET @Contents = '更新フラグ入庫予定あり件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;

        	--入庫予定変更あり
        	SET @sql = N'
            	UPDATE W
            	SET W.UPD_FLG = @UpdFlg_OUT
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' AS W
            	INNER JOIN T_IN_ASSET AS A
            	ON W.ASSET_NO = A.ASSET_NO
            	WHERE (' + @Condition + ')
            	AND W.UPD_FLG = @UpdFlgW_OUT
        
            	SELECT @Count_OUT = @@ROWCOUNT;
        	';

        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1), @UpdFlgW_OUT CHAR(1), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_IN_UPDATE,
            	@UpdFlgW_OUT = @CONST_IN_EXISTS,
            	@Count_OUT = @Count OUTPUT;

        	PRINT '更新フラグ入庫予定更新件数: ' + CAST(@Count AS NVARCHAR(10));
        	SET @Contents = '更新フラグ入庫予定更新件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;

			--更新フラグ=Fに更新
        	SET @sql = N'
        		UPDATE  ' + QUOTENAME(@IN_TMPTBL_NAME) + '
        		SET UPD_FLG = @UpdFlg_OUT
        		WHERE TRIM(ISNULL(UPD_FLG, '''')) = ''''
        		
            	SELECT @Count_OUT = @@ROWCOUNT;
        	';
        	
        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_IN_INSERT,
            	@Count_OUT = @Count OUTPUT;
            
            PRINT '更新フラグ入庫予定追加件数: ' + CAST(@Count AS NVARCHAR(10));
            SET @Contents = '更新フラグ入庫予定追加件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;

        	-- ==================================================
        	-- 入庫予定テーブル更新
        	-- ==================================================
        	--UPDATE　更新フラグ=Eのもの
        	SET @sql = N'
            	UPDATE A
            	SET ' + @UpdColList + N'
            	,IP_DATETIME = W.IP_DATETIME
				,IP_EMPLOYEE_CD = W.IP_EMPLOYEE_CD
				,IP_FILE_NAME = W.IP_FILE_NAME
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + ' AS W
            	INNER JOIN T_IN_ASSET AS A
            	ON W.ASSET_NO = A.ASSET_NO
            	WHERE W.UPD_FLG = @UpdFlg_OUT
        
            	SELECT @Count_OUT = @@ROWCOUNT;
    		';

        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_IN_UPDATE,
            	@Count_OUT = @Count OUTPUT;
            
            PRINT '入庫予定テーブル更新件数: ' + CAST(@Count AS NVARCHAR(10));
            SET @Contents = '入庫予定テーブル更新件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;
            
            -- ==================================================
        	-- 入庫予定テーブル追加
        	-- ==================================================
        	--INSERT　更新フラグ=Fのもの
            SET @sql = N'
            	INSERT INTO T_IN_ASSET ( ' + @InsColList + N' 
            	,IP_DATETIME,IP_EMPLOYEE_CD,IP_FILE_NAME
            	)
            	SELECT' + @InsValList + N'
            	,W.IP_DATETIME,W.IP_EMPLOYEE_CD,W.IP_FILE_NAME
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            	WHERE W.UPD_FLG = @UpdFlg_OUT
            	
            	SELECT @Count_OUT = @@ROWCOUNT;
        	';

        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_IN_INSERT,
            	@Count_OUT = @Count OUTPUT;

        	PRINT '入庫予定テーブル挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        	SET @Contents = '入庫予定テーブル挿入件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;
        	
        	--資産処理履歴追加SQL
        	SET @sql= N'
            	INSERT INTO T_PROCESS_LOG (
            		PROCESS_DT,PROCESS_TM,ASSET_NO,PROCESS_TOOL_DIV,PROCESS_DIV
            		,MNG_LOC_L_CD,LOC_L_CD,LOC_M_CD,LOC_S_CD,LOC_L_NAME,LOC_M_NAME,LOC_S_NAME
            		,ASSET_NAME,TASK_RESULT
            		,SUB_ASSET_NO,CAT_L_CD,CAT_NAME
            		,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            	)
            	SELECT CONVERT(int, FORMAT(GetDate(), ''yyyyMMdd'')),FORMAT(GetDate(), ''HHmmssfff'')
            	,W.ASSET_NO,''W'',@ToolDiv_OUT
            	,W.LOC_L_CD,W.LOC_L_CD,W.LOC_M_CD,W.LOC_S_CD,W.LOC_L_NAME,W.LOC_M_NAME,W.LOC_S_NAME
            	,W.ASSET_NAME,''N''
            	,W.SUB_ASSET_NO,W.CAT_L_CD,W.CAT_L_NAME
            	,GETDATE(),W.IP_EMPLOYEE_CD,W.IP_EMPLOYEE_NAME,W.UPD_TASK
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            	WHERE W.UPD_FLG = @UpdFlg_OUT
            	
            	SELECT @Count_OUT = @@ROWCOUNT;
        	';
        
        	 --資産処理履歴INSERT　更新フラグ='F'のもの
        	EXEC sp_executesql 
            	@sql,
            	N'@UpdFlg_OUT CHAR(1), @ToolDiv_OUT CHAR(2), @Count_OUT INT OUTPUT',
            	@UpdFlg_OUT = @CONST_IN_INSERT,
            	@ToolDiv_OUT ='LP',
            	@Count_OUT = @Count OUTPUT;

        	PRINT '資産処理履歴(LP)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        	SET @Contents = '資産処理履歴(LP)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;
        	
        	 -- ==================================================
        	-- 入庫予定テーブル削除(引数.ロケ大区分コードのデータ)
        	-- ==================================================
            SET @sql = N'
            	DELETE FROM T_IN_ASSET 
            	WHERE MNG_LOC_L_CD = @IN_LOC_L_CD
            	AND NOT EXISTS (
            		SELECT ''X''
            		FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            		WHERE W.ASSET_NO = T_IN_ASSET.ASSET_NO 
            	)
            	
            	SELECT @Count_OUT = @@ROWCOUNT;
        	';

        	EXEC sp_executesql 
            	@sql,
            	N'@IN_LOC_L_CD NVARCHAR(10),@Count_OUT INT OUTPUT',
            	@IN_LOC_L_CD = @IN_LOC_L_CD,
            	@Count_OUT = @Count OUTPUT;

        	PRINT '入庫予定テーブル削除件数: ' + CAST(@Count AS NVARCHAR(10));
        	SET @Contents = '入庫予定テーブル削除件数: ' + CAST(@Count AS NVARCHAR(10));
	        EXEC sp_executesql 
	            @InsOpeLog,
	            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
	            @ProcessCat = 'EV',
	            @LocLCd = @IN_LOC_L_CD,
	            @Contents = @Contents,
	            @EmployeeCD = @EmployeeCD,
	            @EmployeeNM = @EmployeeNM,
	            @UpdTask = @UpdTask
	            ;
        END

        -- ==================================================
        -- 資産マスタ削除(引数.ロケ大区分コードのデータ)
        -- ==================================================
        --資産処理履歴追加
        SET @sql= N'
            INSERT INTO T_PROCESS_LOG (
            	PROCESS_DT,PROCESS_TM,ASSET_NO,ASSET_ID,PROCESS_TOOL_DIV,PROCESS_DIV
            	,LEASE_PLACE_CD,LEASE_PLACE_NAME
            	,REMARK,LEASE_EMPLOYEE_CD,LEASE_EMPLOYEE_NAME,LEASE_DT,LEASE_DUE_DT
            	,LOC_L_CD,LOC_M_CD,LOC_S_CD,LOC_L_NAME,LOC_M_NAME,LOC_S_NAME
            	,ASSET_NAME,MAKER,PRODUCT_NO,MODEL_NO,MNG_LOC_L_CD,LEASE_STATUS,TASK_RESULT
            	,REMARK1,REMARK2,REMARK3,REMARK4,REMARK5,REMARK6,REMARK7,REMARK8,REMARK9,REMARK10
            	,SUB_ASSET_NO,CAT_L_CD,CAT_NAME
            	,UPD_DATETIME,UPD_EMPLOYEE_CD,UPD_EMPLOYEE_NAME,UPD_TASK
            )
            SELECT CONVERT(int, FORMAT(GetDate(), ''yyyyMMdd'')),FORMAT(GetDate(), ''HHmmssfff'')
            ,A.ASSET_NO,A.ASSET_ID,''W'',@ToolDiv_OUT
            ,A.LEASE_PLACE_CD,A.LEASE_PLACE_NAME
            ,A.REMARK,A.LEASE_EMPLOYEE_CD,A.LEASE_EMPLOYEE_NAME,A.LEASE_DT,A.LEASE_DUE_DT
            ,A.LOC_L_CD,A.LOC_M_CD,A.LOC_S_CD,LOCL.LOC_NAME,LOCM.LOC_NAME,LOCS.LOC_NAME
            ,A.ASSET_NAME,A.MAKER,PRODUCT_NO,A.MODEL_NO,A.MNG_LOC_L_CD,A.LEASE_STATUS,''N''
            ,A.REMARK1,A.REMARK2,A.REMARK3,A.REMARK4,A.REMARK5,A.REMARK6,A.REMARK7,A.REMARK8,A.REMARK9,A.REMARK10
            ,A.SUB_ASSET_NO,A.CAT_L_CD,CAT.CAT_NAME
            ,GETDATE(),@EmployeeCD,@EmployeeNM,@UpdTask
            FROM M_ASSET AS A
            LEFT JOIN M_LOCATION LOCL
            ON LOCL.LOC_DIV = ''L'' AND LOCL.LOC_L_CD = A.LOC_L_CD
            LEFT JOIN M_LOCATION LOCM
            ON LOCM.LOC_DIV = ''M'' AND LOCM.LOC_L_CD = A.LOC_L_CD AND LOCM.LOC_M_CD = A.LOC_M_CD
            LEFT JOIN M_LOCATION LOCS
            ON LOCS.LOC_DIV = ''S'' AND LOCS.LOC_L_CD = A.LOC_L_CD AND LOCS.LOC_M_CD = A.LOC_M_CD AND LOCS.LOC_S_CD = A.LOC_S_CD
            LEFT JOIN M_CATEGORY CAT
            ON CAT.CAT_DIV = ''L'' AND CAT.CAT_L_CD = A.CAT_L_CD
            WHERE A.MNG_LOC_L_CD = @IN_LOC_L_CD
            AND NOT EXISTS (
            	SELECT ''X''
            	FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            	WHERE W.ASSET_NO = A.ASSET_NO 
            )
            
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';
        
        EXEC sp_executesql 
            @sql,
            N'@IN_LOC_L_CD NVARCHAR(10),@UpdFlg_OUT CHAR(1), @ToolDiv_OUT CHAR(2) 
            ,@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)
            ,@Count_OUT INT OUTPUT',
            @IN_LOC_L_CD = @IN_LOC_L_CD,
            @UpdFlg_OUT = @CONST_IN_INSERT,
            @ToolDiv_OUT ='CD',
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask,
            @Count_OUT = @Count OUTPUT;

        PRINT '資産処理履歴(CD)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '資産処理履歴(CD)挿入件数: ' + CAST(@Count AS NVARCHAR(10));
        EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;
        
        --資産マスタ削除
        SET @sql = N'
            DELETE FROM M_ASSET 
            WHERE MNG_LOC_L_CD = @IN_LOC_L_CD
            AND NOT EXISTS (
            SELECT ''X''
            FROM ' + QUOTENAME(@IN_TMPTBL_NAME) + '  AS W
            	WHERE W.ASSET_NO = M_ASSET.ASSET_NO 
            )
            
            SELECT @Count_OUT = @@ROWCOUNT;
        ';

        EXEC sp_executesql 
            @sql,
            N'@IN_LOC_L_CD NVARCHAR(10),@Count_OUT INT OUTPUT',
            @IN_LOC_L_CD = @IN_LOC_L_CD,
            @Count_OUT = @Count OUTPUT;

        PRINT '資産マスタ削除件数: ' + CAST(@Count AS NVARCHAR(10));
        SET @Contents = '資産マスタ削除件数: ' + CAST(@Count AS NVARCHAR(10));
	    EXEC sp_executesql 
            @InsOpeLog,
            N'@ProcessCat CHAR(2),@LocLCd  NVARCHAR(10),@Contents NVARCHAR(1024),@EmployeeCD NVARCHAR(15), @EmployeeNM NVARCHAR(60), @UpdTask NVARCHAR(255)',
            @ProcessCat = 'EV',
            @LocLCd = @IN_LOC_L_CD,
            @Contents = @Contents,
            @EmployeeCD = @EmployeeCD,
            @EmployeeNM = @EmployeeNM,
            @UpdTask = @UpdTask
            ;

        -- 正常終了
        SET @OUT_RETURN = 1;
    END TRY

	-- 例外処理
    BEGIN CATCH
        PRINT 'エラーが発生しました: ' + ERROR_MESSAGE();
        SET @OUT_MESSAGE = 
            N'エラー発生: ' + ERROR_MESSAGE() + 
            N'（行: ' + CAST(ERROR_LINE() AS NVARCHAR(10)) + N'）';
	    SET @OUT_RETURN = 9;
    END CATCH
END;
