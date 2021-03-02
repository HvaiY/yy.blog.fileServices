-- 同步校验存储过程
-- 同步校验存储过程
alter proc [dbo].[sp_PPSBindDRMSyncTemplate] @batchId uniqueidentifier, --批次
                                              @loadAdd nvarchar(500), -- 阿里云文件地址
                                              @state BIT = 0 OUTPUT, --导入成功的状态，1表示成功，0表示失败
                                              @succeedCount INT = 0 OUTPUT, -- 成功的条数
                                              @failedCount INT = 0 OUTPUT -- 失败的条数
AS
BEGIN
    BEGIN TRY
        CREATE TABLE #Error
        (
            ID    uniqueidentifier,
            Error nvarchar(max)
        )
        select * into #temp from PPSBindDRMSyncTemplate where BatchId = @batchId;
        if EXISTS(select * from #temp where PPSCode is null or PPSCode = '')
            begin
                insert into #Error select ID, N'销售主管编号不应为空 ' from #temp where PPSCode is null or PPSCode = '';
            end
        if EXISTS(select * from #temp where StoreCode is null or StoreCode = '')
            begin
                insert into #Error select ID, N'门店编码不应为空 ' from #temp where StoreCode is null or StoreCode = '';
            end
        if EXISTS(select * from #temp where OperationType is null)
            begin
                insert into #Error select ID, N'操作类型不应为空 ' from #temp where OperationType is null;
            end
        if EXISTS(select *
                  from #temp t
                  where t.PPSCode is not null
                    and t.PPSCode != ''
                    and t.PPSCode not in (select CustomerID from SystemUser))
            begin
                insert into #Error
                select ID, N'编号为' + t.PPSCode + N'人员信息未找到'
                from #temp t
                where t.PPSCode is not null
                  and t.PPSCode != ''
                  and t.PPSCode not in (select CustomerID from SystemUser);
            end
        if EXISTS(
                select *
                from #temp t
                         left join SystemUser u on t.PPSCode = u.CustomerID
                         left join SystemUserRoleRelation ur on ur.FK_SystemUser_ID = u.ID
                         left join SystemUserRole r on r.ID = ur.FK_SystemUserRole_ID
                where r.Code != '1004')
            begin
                insert into #error
                select t.ID, N'销售主管:' + t.PPSName + N'该人员非销售主管'
                from #temp t
                         left join SystemUser u on t.PPSCode = u.CustomerID
                         left join SystemUserRoleRelation ur on ur.FK_SystemUser_ID = u.ID
                         left join SystemUserRole r on r.ID = ur.FK_SystemUserRole_ID
                where r.Code != '1004'
            end
        if EXISTS(select *
                  from #temp t
                  where t.StoreCode is not null
                    and t.StoreCode != ''
                    and t.StoreCode not in (select Code from TBStore))
            begin
                insert into #Error
                select ID, N'编号为' + t.StoreCode + N'门店信息未找到'
                from #temp t
                where t.StoreCode is not null
                  and t.StoreCode != ''
                  and t.StoreCode not in (select Code from TBStore);
            end
        --合并后的错误信息
        CREATE TABLE #ErrorNew
        (
            ID    uniqueidentifier,
            Error nvarchar(max)
        )
        --合并相同ID的错误信息
        insert into #ErrorNew
        select ID,
               (
                   STUFF((select ';' + Error
                          FROM #Error
                          WHERE ID = b.ID
                          ORDER BY ID
                          FOR XML PATH('')), 1, 1, '')
                   )
        from #Error b
        group by ID

        if ((select COUNT(ID) from #ErrorNew) > 0)
            begin
                insert into PPSBindDRMSyncErrorMessage
                (ID,
                 PPSCode,
                 PPSName,
                 StoreCode,
                 OperationType,
                 OperationTime,
                 BatchId,
                 ErrorMessage)
                select NEWID(),
                       t.PPSCode,
                       t.PPSName,
                       t.StoreCode,
                       t.OperationType,
                       t.OperationTime,
                       t.BatchId,
                       e.Error
                from #ErrorNew as e
                         left join #temp as t on e.ID = t.ID;
            end
        -- 正确数据
        if ((select count(Id) from #temp where Id not in (select ID from #ErrorNew)) > 0)
            begin
                -------start 备份数据------
                if not exists(select top 1 * from SystemUserStoreDRMBak where DateDiff(dd, BakDate, getdate()) = 0)
                    begin
                        delete SystemUserStoreDRMBak where DATEDIFF(day, [BakDate], GETDATE()) > 7;
                        insert into SystemUserStoreDRMBak
                        select *, GETDATE()
                        from SystemUserStore;
                    end;
                -------end 备份数据--------

                select t.*,
                       u.ID FK_SystemUser_ID,
                       s.ID FK_Store_ID,
                       s.Latitude,
                       s.Longitude
                into #needBindData
                from #temp t
                         inner join SystemUser u on t.PPSCode = u.CustomerID
                         inner join TBStore s on t.StoreCode = s.Code
                where t.Id not in (select ID from #ErrorNew)
                  and OperationType = 1
                -- #temp.OperationType=1 绑定 -- 全量绑定
                -- 先解绑原有的PPS 人店关系
                select us.Id, us.FK_SystemUser_ID, us.FK_Store_ID
                into #unbindUS
                from V_SystemUserStore us
                         left join TBStore s on us.FK_Store_ID = s.ID
                         left join SystemUser u on us.FK_SystemUser_ID = u.ID
                         left join V_SystemUserRoleName ur on ur.FK_SystemUser_ID = u.ID
                where us.IsInStore = 1
                  and s.IsVirtual = 0
                  and s.IsDelete = 0
                  and u.IsDelete = 0
                  and ur.Code = '1004';

                update us
                set us.LeaveDate=getdate(),
                    us.ModifyTime=getdate(),
                    us.FK_SystemUserDepartment_Create_ID='00000000-0000-0000-0000-000000000001'
                from SystemUserStore as us
                where us.FK_Store_ID in (select FK_Store_ID from #unbindUS)
                 or us.FK_SystemUser_ID in (select distinct FK_SystemUser_ID from #needBindData)
                -- 直接绑定
                INSERT
                INTO SystemUserStore
                (ID,
                 FK_SystemUser_ID,
                 FK_Store_ID,
                 EntryDate,
                 Long,
                 Lat,
                 FK_SystemUserDepartment_Create_ID)
                SELECT NEWID(),
                       t.FK_SystemUser_ID,
                       t.FK_Store_ID,
                       t.OperationTime,
                       t.Longitude,
                       t.Latitude,
                       '00000000-0000-0000-0000-000000000001'
                FROM #needBindData as t;
                --                 where not exists(select 1
--                                  from SystemUserStore d
--                                  where t.FK_SystemUser_ID = d.FK_SystemUser_ID
--                                    and t.FK_Store_ID = d.FK_Store_ID
--                                    and d.IsDelete = 0
--                     );
                --  绑定时 存在的人店关系   则更新入店时间
--                 update SystemUserStore
--                 set EntryDate=t.OperationTime,
--                     LeaveDate=NULL,
--                     FK_SystemUserDepartment_Create_ID='00000000-0000-0000-0000-000000000001'
--                 FROM #needBindData as t
--                          inner join SystemUserStore d on t.FK_SystemUser_ID = d.FK_SystemUser_ID
--                     and t.FK_Store_ID = d.FK_Store_ID and d.IsDelete = 0;
                -- 解绑人店关系 暂保留
                with succeedData as (select *
                                     from #temp
                                     where Id not in (select ID from #ErrorNew)
                                       and OperationType = 0)
                update SystemUserStore
                set LeaveDate=t.OperationTime,
                    FK_SystemUserDepartment_Create_ID='00000000-0000-0000-0000-000000000001'
                FROM succeedData as t
                         inner join SystemUser u on t.PPSCode = u.CustomerID
                         inner join TBStore s on t.StoreCode = s.Code
                         inner join SystemUserStore d on u.Id = d.FK_SystemUser_ID
                    and s.ID = d.FK_Store_ID and d.IsDelete = 0;
            END
        -- 错误数据
-- 		  	 if((select count(Id) from #temp  where Id  in (select ID from #ErrorNew))>0)
-- 		  	     begin
--
--                  end
        --统计失败条数
        select @failedCount = COUNT(ID) from #ErrorNew;

        --统计成功条数
        select @succeedCount = COUNT(ID) - @failedCount from #temp;
        -- 备份数据
        if ((select count(1) from #temp) > 0)
            begin
                declare @backUpId uniqueidentifier=newid()
                insert into SftpSynLogInfo(id, batchnumber, syncount, synstate, loadadd, createtime, succeedCount,
                                           failedCount, SynType)
                values (@backUpId, @batchId, @failedCount + @succeedCount,
                        IIF(@failedCount = 0, 1, IIF(@succeedCount = 0, 0, 2)), @loadAdd, dbo.GetLocalDate(default),
                        @succeedCount, @failedCount, 8)

                update PPSBindDRMSyncTemplate
                set IsSucceed=IIF((select count(1) from #ErrorNew where id = t.ID) > 0, 0, 1),
                    FK_SftpSyncLpgInfo_ID=@backUpId
                from #temp t
                where PPSBindDRMSyncTemplate.ID = t.ID
            end
        drop table #temp;
        drop table #Error;
        drop table #ErrorNew;

    END TRY
    BEGIN CATCH
        INSERT INTO ScriptExecErrorInfo([ScriptName], ErrorNumber, ErrorSeverity, ErrorState, ErrorProcedure, ErrorLine,
                                        ErrorMessage, IsTran)
        VALUES ('sp_PPSBindDRMSyncTemplate',
                Error_number(), --错误代码
                Error_severity(), --错误严重级别，级别小于10 try catch 捕获不到
                Error_state(), --错误状态码
                Error_Procedure(), --出现错误的存储过程或触发器的名称。
                Error_line(), --发生错误的行号
                Error_message(), --错误的具体信息
                0);
        SELECT error_message();
        SET @failedCount = 1;
    END CATCH
    IF (@failedCount > 0)
        BEGIN
            set @state = 0;
        END
    ELSE
        BEGIN
            SET @state = 1;
        END
END
go

