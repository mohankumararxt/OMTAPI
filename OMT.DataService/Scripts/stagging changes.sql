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


--INSERT INTO Keywordstable (KeywordName, isactive)
--VALUES 
--('ABORT', 1),
--('ABSOLUTE', 1),
--('ACTION', 1),
--('ADD', 1),
--('ADMIN', 1),
--('AFTER', 1),
--('AGGREGATE', 1),
--('ALIAS', 1),
--('ALL', 1),
--('ALLOCATE', 1),
--('ALTER', 1),
--('AND', 1),
--('ANY', 1),
--('ARE', 1),
--('ARRAY', 1),
--('AS', 1),
--('ASC', 1),
--('ASSERTION', 1),
--('AT', 1),
--('AUTHORIZATION', 1),
--('AVG', 1),
--('BACKUP', 1),
--('BEFORE', 1),
--('BEGIN', 1),
--('BETWEEN', 1),
--('BINARY', 1),
--('BIT', 1),
--('BLOB', 1),
--('BOOLEAN', 1),
--('BOTH', 1),
--('BREAK', 1),
--('BROWSE', 1),
--('BULK', 1),
--('BY', 1),
--('CALL', 1),
--('CASCADE', 1),
--('CASCADED', 1),
--('CASE', 1),
--('CAST', 1),
--('CATALOG', 1),
--('CHAR', 1),
--('CHARACTER', 1),
--('CHECK', 1),
--('CHECKPOINT', 1),
--('CLOSE', 1),
--('CLUSTERED', 1),
--('COALESCE', 1),
--('COLLATE', 1),
--('COLLATION', 1),
--('COLUMN', 1),
--('COMMIT', 1),
--('COMPLETION', 1),
--('COMPUTE', 1),
--('CONNECT', 1),
--('CONNECTION', 1),
--('CONSTRAINT', 1),
--('CONTAINS', 1),
--('CONTAINSTABLE', 1),
--('CONTINUE', 1),
--('CONVERT', 1),
--('CORRESPONDING', 1),
--('COUNT', 1),
--('CREATE', 1),
--('CROSS', 1),
--('CUBE', 1),
--('CURRENT', 1),
--('CURRENT_DATE', 1),
--('CURRENT_PATH', 1),
--('CURRENT_ROLE', 1),
--('CURRENT_TIME', 1),
--('CURRENT_TIMESTAMP', 1),
--('CURRENT_USER', 1),
--('CURSOR', 1),
--('CYCLE', 1),
--('DATA', 1),
--('DATABASE', 1),
--('DAY', 1),
--('DBCC', 1),
--('DEALLOCATE', 1),
--('DECLARE', 1),
--('DEFAULT', 1),
--('DEFERRABLE', 1),
--('DEFERRED', 1),
--('DELETE', 1),
--('DENY', 1),
--('DEPTH', 1),
--('DEREF', 1),
--('DESC', 1),
--('DESCRIBE', 1),
--('DESCRIPTOR', 1),
--('DETERMINISTIC', 1),
--('DIAGNOSTICS', 1),
--('DISCONNECT', 1),
--('DISK', 1),
--('DISTINCT', 1),
--('DISTRIBUTED', 1),
--('DO', 1),
--('DOMAIN', 1),
--('DOUBLE', 1),
--('DROP', 1),
--('DUMMY', 1),
--('DUMP', 1),
--('DYNAMIC', 1),
--('EACH', 1),
--('ELSE', 1),
--('END', 1),
--('END-EXEC', 1),
--('EQUALS', 1),
--('ESCAPE', 1),
--('EVERY', 1),
--('EXCEPT', 1),
--('EXCEPTION', 1),
--('EXEC', 1),
--('EXECUTE', 1),
--('EXISTS', 1),
--('EXIT', 1),
--('EXTERNAL', 1),
--('EXTRACT', 1),
--('FALSE', 1),
--('FETCH', 1),
--('FILE', 1),
--('FILLFACTOR', 1),
--('FIRST', 1),
--('FLOAT', 1),
--('FOR', 1),
--('FOREIGN', 1),
--('FOUND', 1),
--('FREE', 1),
--('FREETEXT', 1),
--('FREETEXTTABLE', 1),
--('FROM', 1),
--('FULL', 1),
--('FUNCTION', 1),
--('GENERAL', 1),
--('GET', 1),
--('GLOBAL', 1),
--('GO', 1),
--('GOTO', 1),
--('GRANT', 1),
--('GROUP', 1),
--('GROUPING', 1),
--('HAVING', 1),
--('HOLD', 1),
--('HOLDLOCK', 1),
--('HOST', 1),
--('HOUR', 1),
--('IDENTITY', 1),
--('IDENTITY_INSERT', 1),
--('IDENTITYCOL', 1),
--('IF', 1),
--('IMMEDIATE', 1),
--('IN', 1),
--('INDICATOR', 1),
--('INITIALIZE', 1),
--('INITIALLY', 1),
--('INNER', 1),
--('INOUT', 1),
--('INPUT', 1),
--('INSERT', 1),
--('INT', 1),
--('INTEGER', 1),
--('INTERSECT', 1),
--('INTERVAL', 1),
--('INTO', 1),
--('IS', 1),
--('ISOLATION', 1),
--('JOIN', 1),
--('KEY', 1),
--('KILL', 1),
--('LANGUAGE', 1),
--('LAST', 1),
--('LEADING', 1),
--('LEFT', 1),
--('LEVEL', 1),
--('LIKE', 1),
--('LIMIT', 1),
--('LINENO', 1),
--('LOAD', 1),
--('LOCAL', 1),
--('LOCALTIME', 1),
--('LOCALTIMESTAMP', 1),
--('LOCATOR', 1),
--('MAP', 1),
--('MATCH', 1),
--('MINUTE', 1),
--('MODIFIES', 1),
--('MODIFY', 1),
--('MODULE', 1),
--('MONTH', 1),
--('NAMES', 1),
--('NATIONAL', 1),
--('NATURAL', 1),
--('NCHAR', 1),
--('NCLOB', 1),
--('NEW', 1),
--('NEXT', 1),
--('NO', 1),
--('NOCHECK', 1),
--('NONCLUSTERED', 1),
--('NONE', 1),
--('NOT', 1),
--('NULL', 1),
--('NULLIF', 1),
--('NUMERIC', 1),
--('OBJECT', 1),
--('OCCURRENCES', 1),
--('OF', 1),
--('OFF', 1),
--('OFFSETS', 1),
--('ON', 1),
--('ONLY', 1),
--('OPEN', 1),
--('OPENDATASOURCE', 1),
--('OPENQUERY', 1),
--('OPENROWSET', 1),
--('OPENXML', 1),
--('OPERATION', 1),
--('OPTION', 1),
--('OR', 1),
--('ORDER', 1),
--('ORDINALITY', 1),
--('OUT', 1),
--('OUTER', 1),
--('OUTPUT', 1),
--('OVER', 1),
--('OVERLAPS', 1),
--('PAD', 1),
--('PARAMETER', 1),
--('PARAMETERS', 1),
--('PARTIAL', 1),
--('PARTITION', 1),
--('PATH', 1),
--('PERCENT', 1),
--('PLAN', 1),
--('POSITION', 1),
--('POSTFIX', 1),
--('PRECISION', 1),
--('PREORDER', 1),
--('PREPARE', 1),
--('PRESERVE', 1),
--('PRIMARY', 1),
--('PRINT', 1),
--('PRIOR', 1),
--('PRIVILEGES', 1),
--('PROC', 1),
--('PROCEDURE', 1),
--('PUBLIC', 1),
--('RAISE', 1),
--('RAISERROR', 1),
--('READ', 1),
--('READS', 1),
--('READTEXT', 1),
--('REAL', 1),
--('RECONFIGURE', 1),
--('REFERENCES', 1),
--('REFERENCING', 1),
--('RELATIVE', 1),
--('REPLICATION', 1),
--('RESTORE', 1),
--('RESTRICT', 1),
--('RESULT', 1),
--('RETURN', 1),
--('RETURNS', 1),
--('REVOKE', 1),
--('RIGHT', 1),
--('ROLE', 1),
--('ROLLBACK', 1),
--('ROLLUP', 1),
--('ROUTINE', 1),
--('ROW', 1),
--('ROWS', 1),
--('RULE', 1),
--('SAVE', 1),
--('SAVEPOINT', 1),
--('SCHEMA', 1),
--('SCROLL', 1),
--('SEARCH', 1),
--('SECOND', 1),
--('SECTION', 1),
--('SELECT', 1),
--('SEQUENCE', 1),
--('SESSION', 1),
--('SESSION_USER', 1),
--('SET', 1),
--('SETS', 1),
--('SHUTDOWN', 1),
--('SIZE', 1),
--('SOME', 1),
--('SPACE', 1),
--('SPECIFIC', 1),
--('SPECIFICTYPE', 1),
--('SQL', 1),
--('SQLCODE', 1),
--('SQLERROR', 1),
--('SQLEXCEPTION', 1),
--('SQLSTATE', 1),
--('SQLWARNING', 1),
--('START', 1),
--('STATE', 1),
--('STATEMENT', 1),
--('STATIC', 1),
--('STATISTICS', 1),
--('STRUCTURE', 1),
--('SUBSTRING', 1),
--('SUM', 1),
--('SYSTEM_USER', 1),
--('TABLE', 1),
--('TABLESAMPLE', 1),
--('TEMPORARY', 1),
--('TERMINATE', 1),
--('TEXTSIZE', 1),
--('THAN', 1),
--('THEN', 1),
--('TIME', 1),
--('TIMESTAMP', 1),
--('TIMEZONE_HOUR', 1),
--('TIMEZONE_MINUTE', 1),
--('TO', 1),
--('TOP', 1),
--('TRAILING', 1),
--('TRAN', 1),
--('TRANSACTION', 1),
--('TRANSLATE', 1),
--('TRANSLATION', 1),
--('TRIGGER', 1),
--('TRIM', 1),
--('TRUE', 1),
--('TRUNCATE', 1),
--('TSEQUAL', 1),
--('UNDER', 1),
--('UNDO', 1),
--('UNION', 1),
--('UNIQUE', 1),
--('UNKNOWN', 1),
--('UNNEST', 1),
--('UPDATE', 1),
--('UPDATETEXT', 1),
--('USAGE', 1),
--('USE', 1),
--('USER', 1),
--('USING', 1),
--('VALUE', 1),
--('VALUES', 1),
--('VARBINARY', 1),
--('VARCHAR', 1),
--('VARIABLE', 1),
--('VARYING', 1),
--('VIEW', 1),
--('WAITFOR', 1),
--('WHEN', 1),
--('WHENEVER', 1),
--('WHERE', 1),
--('WHILE', 1),
--('WITH', 1),
--('WITHIN', 1),
--('WITHOUT', 1),
--('WORK', 1),
--('WRITE', 1),
--('WRITETEXT', 1),
--('YEAR', 1),
--('ZONE', 1);


--insert into DefaultTemplateColumns values(1,'AllocationDate','Date',1,0,1,1)
--insert into DefaultTemplateColumns values(2,'AllocationDate','Date',1,0,1,1)

--update DefaultTemplateColumns set DefaultColumnName = 'AllocationDate',datatype = 'Date' where id = 31

--create table LiveReportTiming
--(
--LiveReportTimingId int IDENTITY(1,1) primary key,
--SystemOfRecordId int not null,
--StartTime time not null,
--EndTime time not null,
--IsActive bit not null DEFAULT 1
--)

--ALTER TABLE LiveReportTiming
--ADD CONSTRAINT fk_LiveReportTiming
--FOREIGN KEY (SystemOfRecordId)
--REFERENCES SystemOfRecord(SystemOfRecordId);

--insert into LiveReportTiming (SystemOfRecordId,StartTime,EndTime,isactive)
--values (1,'02:00:00','02:00:00',1),
-- (2,'02:00:00','02:00:00',1),
-- (3,'02:00:00','02:00:00',1)

 --insert into ProcessStatus values(1,'Not Keyed',1)

 --update LiveReportTiming set StartTime = '19:30:00',EndTime ='19:30:00'

-- create table ReportColumns
--(
--ReportColumnsId int IDENTITY(1,1) primary key,
--SystemOfRecordId int not null,
--ReportColumnName varchar(100) not null,
--IsActive bit not null DEFAULT 1
--)

--ALTER TABLE ReportColumns
--ADD CONSTRAINT fk_ReportColumns
--FOREIGN KEY (SystemOfRecordId)
--REFERENCES SystemOfRecord(SystemOfRecordId);

--insert into ReportColumns (SystemOfRecordId,ReportColumnName,IsActive) values(2,'CustomerId',1),(2,'ResWareProductDescriptions',1)

--update DefaultTemplateColumns set IsMandatoryColumn = 1 where id = 21 or id = 5 or id = 31 or id = 32
--update DefaultTemplateColumns set IsMandatoryColumn = 1 where id between 16 and 19
--update DefaultTemplateColumns set IsMandatoryColumn = 1 where id between 1 and 3


--update LiveReportTiming set StartTime = '17:30:00',EndTime ='17:30:00' where LiveReportTimingid = 3

---------------------------------TRD CHANGES--------------------

--insert into ProcessStatus (SystemofRecordId,Status,IsActive) values(3,'Completed',1),(3,'Pending',1)

-- create table DocType
--(
--DocTypeId int IDENTITY(1,1) primary key,
--DocumentName varchar(500) not null,
--IsActive bit not null DEFAULT 1
--)

--insert into DocType (documentname,isactive) values('Recorded Mortgage',1),('Mortgage',1),('Recorded Release',1),('TitlePolicy',1),('Note',1),('Final Title Policy',1),('Miscellaneous',1),('Modification',1),('Power of Attorney',1),('Unrecorded Mortgage',1),('ASSIGNMENT',1),('CEMA',1),('Collateral File',1),('Non-Critical Documents - Collateral File',1),('Original Recorded Documents - Note and RMORT',1),('Rejection-Lien Release',1),('Accommodation Modification',1),('Colorado Deed of Trust',1),('POA',1),('Recorded Electronic Deed of Trust',1),('Legal Description',1),('Miscellaneous Documents',1),('Recorded Security Instrument',1),('Rejection',1),('Lien Release',1),('Lien Release Only',1),('Recorded Release Only',1)

--insert into skillset (SystemofRecordId,SkillSetName,Threshold,IsActive) values (3,'NA8120114IM_TitlePolicy',100,1),
--(3,'TH8060113IM_Final_Title Policy',100,1),
--(3,'PG8120119PC_TitlePolicy',100,1),
--(3,'AP8090122PC_TitlePolicy',100,1),
--(3,'KL8040121PC_TitlePolicy',100,1),
--(3,'CF8050115IM_TitlePolicy',100,1),
--(3,'CI8060121PC_TitlePolicy',100,1),
--(3,'AF8060121PC_TitlePolicy',100,1),
--(3,'AB8030122IM_TitlePolicy',100,1),
--(3,'CV8040121PC_TitlePolicy',100,1),
--(3,'TM8080115IM_TitlePolicy',100,1),
--(3,'SY8100122PC_TitlePolicy',100,1),
--(3,'LO8040122PC_TitlePolicy',100,1),
--(3,'PS8010122PC_TitlePolicy',100,1),
--(3,'NA8120114IM_Recorded_Mortgage',100,1),
--(3,'CF8050115IM_Recorded_Mortgage',100,1),
--(3,'PG8120119PC_Recorded_Mortgage',100,1),
--(3,'AP8090122PC_Recorded_Mortgage',100,1),
--(3,'SY8100122PC_Recorded_Mortgage',100,1),
--(3,'KL8040121PC_Recorded_Mortgage',100,1),
--(3,'AF8060121PC_Recorded_Mortgage',100,1),
--(3,'AB8030122IM_Recorded_Mortgage',100,1),
--(3,'LO8040122PC_Recorded_Mortgage',100,1),
--(3,'CI8060121PC_Recorded_Mortgage',100,1),
--(3,'CV8040121PC_Recorded_Mortgage',100,1),
--(3,'PS8010122PC_Recorded_Mortgage',100,1),
--(3,'TM8080115IM_Recorded_Mortgage',100,1),
--(3,'TH8060113IM_Recorded_Security_Instrument',100,1),
--(3,'NN8040122PC_Recorded_Security_Instrument',100,1),
--(3,'PL8110117PC_TitlePolicy',100,1),
--(3,'PL8110117PC_Recorded_Mortgage',100,1),
--(3,'NN8040122PC_Final_Title_Policy',100,1),
--(3,'BA8080121PC_Recorded_Mortgage',100,1),
--(3,'BA8080121PC_TitlePolicy',100,1),
--(3,'PR8050118IM_Recorded_Mortgage',100,1),
--(3,'PR8050118IM_TitlePolicy',100,1),
--(3,'HB8100116IM_TitlePolicy',100,1),
--(3,'HB8100116IM_Recorded_Mortgage',100,1),
--(3,'ST8070116IM_TitlePolicy',100,1),
--(3,'ST8070116IM_Recorded_Mortgage',100,1),
--(3,'GM8060105PC_Recorded_Mortgage',100,1),
--(3,'GM8060105PC_TitlePolicy',100,1),
--(3,'DB8010324PC_Recorded_Mortgage',100,1),
--(3,'DB8010324PC_TitlePolicy',100,1),
--(3,'TW8090119IM_TitlePolicy',100,1),
--(3,'TW8090119IM_Recorded_Mortgage',100,1),
--(3,'SL8120112IM_Recorded_Release',100,1),
--(3,'BA8050117IM_Recorded_Release',100,1),
--(3,'FS8090112IM_Recorded_Release',100,1),
--(3,'HN8021121IM_Recorded_Release',100,1),
--(3,'QL8040120IM_Recorded_Release',100,1),
--(3,'RM8080117IM_Recorded_Release',100,1),
--(3,'SH8070117IM_Recorded_Release',100,1),
--(3,'SM8030119IM_Recorded_Release',100,1),
--(3,'SP8100114IM_Recorded_Release',100,1),
--(3,'US8090119IM_Recorded_Release',100,1),
--(3,'CC8020122PC_TitlePolicy',100,1),
--(3,'CC8020122PC_Recorded_Mortgage',100,1)

-- create table ReportColumns
--(
--ReportColumnsId int IDENTITY(1,1) primary key,
--SystemOfRecordId int not null,
--ReportColumnName varchar(100) not null,
--IsActive bit not null DEFAULT 1
--)