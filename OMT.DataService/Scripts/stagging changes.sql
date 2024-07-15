--create table Timeline
--(
--TimelineId int IDENTITY(1,1) primary key,
--SkillSetId int not null,
--Hardstatename nvarchar(20) null,
--ExceedTime int not null,
--IsHardState bit not null,
--IsActive bit not null DEFAULT 1
--)

--ALTER TABLE Timeline
--ADD CONSTRAINT fk_Timeline
--FOREIGN KEY (SkillSetId)
--REFERENCES SkillSet(SkillSetId);

-----------add 5 skillsets for resware(Equity_Automation_Typing,Equity_Automation_TIQ,Automation_Review_Typing,AVR_Automation_Review,DR_Automation_Review)
---change in hardstate and timeline table remove 7 and add 78


--------------------check timeline for skillsets
--insert into timeline values(1,'PA',15,1,1)
--insert into timeline values(1,'MI',15,1,1)
--insert into timeline values(1,'CO',15,1,1)
--insert into timeline values(1,'NY',15,1,1)
--insert into timeline values(1,'',5,0,1)

--insert into timeline values(2,'',5,0,1)

--insert into timeline values(3,'PA',10,1,1)
--insert into timeline values(3,'MI',10,1,1)
--insert into timeline values(3,'CO',10,1,1)
--insert into timeline values(3,'NY',10,1,1)
--insert into timeline values(3,'',3,0,1)


--insert into timeline values(4,'',3,0,1)
--insert into timeline values(5,'',12,0,1)


--insert into timeline values(8,'',4,0,1)

--insert into timeline values(13,'',10,0,1)


--insert into timeline values(77,'',4,0,1)

--insert into timeline values(78,'PA',4,1,1)
--insert into timeline values(78,'MI',5,1,1)
--insert into timeline values(78,'CO',5,1,1)
--insert into timeline values(78,'NY',4,1,1)
--insert into timeline values(78,'',4,0,1)

--insert into timeline values(79,'',3,0,1)
--insert into timeline values(80,'',12,0,1)
--insert into timeline values(81,'',4,0,1)
--insert into timeline values(82,'',2,0,1)
--insert into timeline values(83,'',3,0,1)
--insert into timeline values(84,'',12,0,1)
--insert into timeline values(85,'',4,0,1)

--insert into timeline values(86,'',3,0,1)

--insert into timeline values(87,'',2,0,1)
--insert into timeline values(88,'',12,0,1)
--insert into timeline values(89,'',5,0,1)
--insert into timeline values(90,'',10,0,1)

--update defaulttemplatecolumns set defaultcolumnname = 'PropertyState' where id = 19        -- instead of state 


---update timeline set ishardstate = 0 where timelineid between 19 and 38
--update timeline set ishardstate = 0 where timelineid between 5 and 6
--update timeline set ishardstate = 0 where timelineid between 11 and 14


--in getinvoice stored procedure change state to propertystate in  INSERT INTO InvoiceDump ' + @ResColumns + '
    --SELECT
    --    ir.CustomerId,
    --    ir.OrderId,
    --    ir.PropertyState,  this one alone


--	exec sp_rename 'InvoiceDump.State','PropertyState','COLUMN'


--delete from timeline where SkillSetId = 9 or skillsetid = 14 or skillsetid = 16 or SkillSetId = 6 
--delete from skillset where skillsetid = 9 or skillsetid = 14 or skillsetid = 16 or SkillSetId = 6 

--update skillset set skillsetname = 'Equity_Automation_TIQ' where skillsetid = 90

--insert into processtype values('Automation Review',1)
--insert into processtype values('Verify-PP',1)
--insert into processtype values('Verify-PPO',1)

--insert into InvoiceJointResware values(2,13,4,5,2,4,3)

--update InvoiceJointResware set processtypeid = 5 where skillsetid = 8 or skillsetid = 83 

--update InvoiceJointResware set processtypeid = 7 where skillsetid = 82 or skillsetid = 87

-----------------------------------------------------------------------------
--create table InvoiceSkillSet
--(
--InvoiceSkillSetId int IDENTITY(1,1) primary key,
--SkillSetName nvarchar(100) null,
--MergeSkillSets nvarchar(20) null,
--CompareSkillSets nvarchar(20) null,
--OperationType int null,
--ShowInInvoice bit not null DEFAULT 1,
--Description nvarchar(100) null,
--IsActive bit not null DEFAULT 1,
--SystemofRecordId int null
--)

--insert into InvoiceSkillSet values('AVR_Verify & QC','82','81',1,1,'Get common from both tables',1,2)
--insert into InvoiceSkillSet values('AVR_QC','82','81',2,0,'Compare skillsets and show non duplicate from first skillset',1,2)
--insert into InvoiceSkillSet values('DR_Verify/Keyed Verify & QC','87','86,85',1,1,'Get common from both tables',1,2)
--insert into InvoiceSkillSet values('DR_QC','87','86,85',2,0,'Compare skillsets and show non duplicate from first skillset',1,2)
--insert into InvoiceSkillSet values('AVR_Verify','81','82',2,0,'Compare skillsets and show non duplicate from first skillset',1,2)
--insert into InvoiceSkillSet values('DR_Verify','86','87',2,0,'Compare skillsets and show non duplicate from first skillset',1,2)
--insert into InvoiceSkillSet values('DR_Keyed_Verify','85','87',2,0,'Compare skillsets and show non duplicate from first skillset',1,2)


--ALTER TABLE invoiceskillset
--ADD CONSTRAINT fk_invoiceskillset
--FOREIGN KEY (SystemofRecordId)
--REFERENCES SystemofRecord(SystemofRecordId);


--ALTER TABLE INVOICEDUMP
--ADD OrderFees NVARCHAR(50)
--ALTER TABLE INVOICEDUMP
--ADD AOLFees NVARCHAR(50)
--ALTER TABLE INVOICEDUMP
--ADD CopyFees NVARCHAR(50)
--ALTER TABLE INVOICEDUMP
--ADD CertifiedCopyFees NVARCHAR(50)



--execute invoice sp in stagging -- done check if update with ordrfees colmns

--insert into ResWareProductDescriptions values('Assignment Verification Report',1)
--insert into ResWareProductDescriptionMap values(68,8)

--update InvoiceSkillSet set MergeSkillSets = '82', CompareSkillSets = '81' where invoiceskillsetid = 1

--update InvoiceSkillSet set MergeSkillSets = '87', CompareSkillSets = '86,85' where invoiceskillsetid = 3


--truncate table userskillset
--truncate table templatecolumns
--delete from skillset where skillsetid >90
--truncate table teamassociation
--delete from userprofile where userid >126

--create table Keywordstable
--(
--KeywordsId int IDENTITY(1,1) primary key,
--Keywordname nvarchar(100) null,
--IsActive bit not null DEFAULT 1
--)

--INSERT INTO Keywordstable (Keywordname, IsActive)
--VALUES
--    ('ADD', 1),
--    ('EXCEPT', 1),
--    ('PERCENT', 1),
--    ('ALL', 1),
--    ('EXEC', 1),
--    ('PLAN', 1),
--    ('ALTER', 1),
--    ('EXECUTE', 1),
--    ('PRECISION', 1),
--    ('AND', 1),
--    ('EXISTS', 1),
--    ('PRIMARY', 1),
--    ('ANY', 1),
--    ('EXIT', 1),
--    ('PRINT', 1),
--    ('AS', 1),
--    ('FETCH', 1),
--    ('PROC', 1),
--    ('ASC', 1),
--    ('FILE', 1),
--    ('PROCEDURE', 1),
--    ('AUTHORIZATION', 1),
--    ('FILLFACTOR', 1),
--    ('PUBLIC', 1),
--    ('BACKUP', 1),
--    ('FOR', 1),
--    ('RAISERROR', 1),
--    ('BEGIN', 1),
--    ('FOREIGN', 1),
--    ('READ', 1),
--    ('BETWEEN', 1),
--    ('FREETEXT', 1),
--    ('READTEXT', 1),
--    ('BREAK', 1),
--    ('FREETEXTTABLE', 1),
--    ('RECONFIGURE', 1),
--    ('BROWSE', 1),
--    ('FROM', 1),
--    ('REFERENCES', 1),
--    ('BULK', 1),
--    ('FULL', 1),
--    ('REPLICATION', 1),
--    ('BY', 1),
--    ('FUNCTION', 1),
--    ('RESTORE', 1),
--    ('CASCADE', 1),
--    ('GOTO', 1),
--    ('RESTRICT', 1),
--    ('CASE', 1),
--    ('GRANT', 1),
--    ('RETURN', 1),
--    ('CHECK', 1),
--    ('GROUP', 1),
--    ('REVOKE', 1),
--    ('CHECKPOINT', 1),
--    ('HAVING', 1),
--    ('RIGHT', 1),
--    ('CLOSE', 1),
--    ('HOLDLOCK', 1),
--    ('ROLLBACK', 1),
--    ('CLUSTERED', 1),
--    ('IDENTITY', 1),
--    ('ROWCOUNT', 1),
--    ('COALESCE', 1),
--    ('IDENTITY_INSERT', 1),
--    ('ROWGUIDCOL', 1),
--    ('COLLATE', 1),
--    ('IDENTITYCOL', 1),
--    ('RULE', 1),
--    ('COLUMN', 1),
--    ('IF', 1),
--    ('SAVE', 1),
--    ('COMMIT', 1),
--    ('IN', 1),
--    ('SCHEMA', 1),
--    ('COMPUTE', 1),
--    ('INDEX', 1),
--    ('SELECT', 1),
--    ('CONSTRAINT', 1),
--    ('INNER', 1),
--    ('SESSION_USER', 1),
--    ('CONTAINS', 1),
--    ('INSERT', 1),
--    ('SET', 1),
--    ('CONTAINSTABLE', 1),
--    ('INTERSECT', 1),
--    ('SETUSER', 1),
--    ('CONTINUE', 1),
--    ('INTO', 1),
--    ('SHUTDOWN', 1),
--    ('CONVERT', 1),
--    ('IS', 1),
--    ('SOME', 1),
--    ('CREATE', 1),
--    ('JOIN', 1),
--    ('STATISTICS', 1),
--    ('CROSS', 1),
--    ('KEY', 1),
--    ('SYSTEM_USER', 1),
--    ('CURRENT', 1),
--    ('KILL', 1),
--    ('TABLE', 1),
--    ('CURRENT_DATE', 1),
--    ('LEFT', 1),
--    ('TEXTSIZE', 1),
--    ('CURRENT_TIME', 1),
--    ('LIKE', 1),
--    ('THEN', 1),
--    ('CURRENT_TIMESTAMP', 1),
--    ('LINENO', 1),
--    ('TO', 1),
--    ('CURRENT_USER', 1),
--    ('LOAD', 1),
--    ('TOP', 1),
--    ('CURSOR', 1),
--    ('NATIONAL', 1),
--    ('TRAN', 1),
--    ('DATABASE', 1),
--    ('NOCHECK', 1),
--    ('TRANSACTION', 1),
--    ('DBCC', 1),
--    ('NONCLUSTERED', 1),
--    ('TRIGGER', 1),
--    ('DEALLOCATE', 1),
--    ('NOT', 1),
--    ('TRUNCATE', 1),
--    ('DECLARE', 1),
--    ('NULL', 1),
--    ('TSEQUAL', 1),
--    ('DEFAULT', 1),
--    ('NULLIF', 1),
--    ('UNION', 1),
--    ('DELETE', 1),
--    ('OF', 1),
--    ('UNIQUE', 1),
--    ('DENY', 1),
--    ('OFF', 1),
--    ('UPDATE', 1),
--    ('DESC', 1),
--    ('OFFSETS', 1),
--    ('UPDATETEXT', 1),
--    ('DISK', 1),
--    ('ON', 1),
--    ('USE', 1),
--    ('DISTINCT', 1),
--    ('OPEN', 1),
--    ('USER', 1),
--    ('DISTRIBUTED', 1),
--    ('OPENDATASOURCE', 1),
--    ('VALUES', 1),
--    ('DOUBLE', 1),
--    ('OPENQUERY', 1),
--    ('VARYING', 1),
--    ('DROP', 1),
--    ('OPENROWSET', 1),
--    ('VIEW', 1),
--    ('DUMP', 1),
--    ('OPENXML', 1),
--    ('WAITFOR', 1),
--    ('ELSE', 1),
--    ('OPTION', 1),
--    ('WHEN', 1),
--    ('END', 1),
--    ('OR', 1),
--    ('WHERE', 1),
--    ('ERRLVL', 1),
--    ('ORDER', 1),
--    ('WHILE', 1),
--    ('ESCAPE', 1),
--    ('OUTER', 1),
--    ('WITH', 1),
--    ('EXCEPT', 1),
--    ('OVER', 1),
--    ('WRITETEXT', 1);


