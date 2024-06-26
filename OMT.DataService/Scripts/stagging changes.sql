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


