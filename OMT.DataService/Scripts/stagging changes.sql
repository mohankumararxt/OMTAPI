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
--insert into timeline values(6,'',10,0,1)

--insert into timeline values(8,'',4,0,1)
--insert into timeline values(9,'',10,0,1)
--insert into timeline values(13,'',10,0,1)
--insert into timeline values(14,'',5,0,1)
--insert into timeline values(16,'',5,0,1)
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

--- add column IsHardState to timeline table
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

--check file
