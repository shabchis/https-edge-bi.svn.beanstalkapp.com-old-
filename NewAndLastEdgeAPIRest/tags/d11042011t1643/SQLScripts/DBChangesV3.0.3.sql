-- Update DBs for version XXXX, new schedular with current services & API3

--****************************************************************************************************
USE [Seperia]
GO
BEGIN TRANSACTION;
GO

ALTER PROCEDURE [dbo].[Session_ValidateSession] 
  @SessionID Int
AS 
BEGIN 
SET NOCOUNT ON;

if DATEDIFF(MI,(SELECT TimeModified FROM User_GUI_Session WHERE SessionID=@SessionID),GETDATE()) >30 
BEGIN
	SELECT 0 as 'Valid',-1 as 'UserID'
end
ELSE 
BEGIN 
	UPDATE User_GUI_Session 
	SET TimeModified=GETDATE()
	WHERE SessionID=@SessionID
	SELECT 1 as 'Valid',UserID 
	FROM User_GUI_Session 
	WHERE SessionID=@SessionID
END;

END

GO

--****************************************************************************************************

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER Procedure [dbo].[Permissions_Operations]
@AccountID as Int,
@TargetID as int,
@TargetIsGroup as bit,
@PermissionType as NvarChar(50),
@Value as bit=null

AS
BEGIN
INSERT INTO User_GUI_AccountPermission 
(AccountID,TargetID,TargetIsGroup,PermissionType,Value)
VALUES(@AccountID,@TargetID,@TargetIsGroup,@PermissionType,@Value);SELECT @@IDENTITY;

declare @clientID as int;
declare @scopeID as int;

SELECT @clientID= Parent_ID
from V_User_GUI_Accounts
where ID=@AccountID
if @clientID is not null
SELECT @scopeID= Parent_ID
from V_User_GUI_Accounts
where ID=@clientID

if @scopeID is not null
INSERT INTO User_GUI_AccountPermission 
(AccountID,TargetID,TargetIsGroup,PermissionType,Value)
VALUES(@scopeID,@TargetID,@TargetIsGroup,'FictivePermission',@Value)
if @clientID is not null
INSERT INTO User_GUI_AccountPermission 
(AccountID,TargetID,TargetIsGroup,PermissionType,Value)
VALUES(@clientID,@TargetID,@TargetIsGroup,'FictivePermission',@Value)
END

--****************************************************************************************************
alter table dbo.Constant_PermissionType
add [Name] [nvarchar](50) NULL

GO
--****************************************************************************************************
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[Menu_GetMenuByUserPermission]
	@userID int,
	@menuPath Nvarchar(max) 
	
AS
BEGIN

declare @permissionTable as table
(
AccountID  INT,
PermissionType  Nvarchar(max)

)

insert into @permissionTable
exec User_CalculatePermissions @userID

select [ID],'Name'=
CASE 
WHEN [Name] IS null then REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
WHEN [Name]='' then  REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
else [Name]
END
,[Path],[MetaData],[Ordinal],[IsOpen] 
FROM [Constant_Menu] 
WHERE [Path]  in (select PermissionType from @permissionTable) and [Path] LIKE @menuPath
ORDER BY [Path]

END
GO
--****************************************************************************************************

CREATE PROCEDURE [dbo].[Permmissions_GetAllPermissions]
AS
SELECT Name=REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END)),[Path] 
FROM Constant_PermissionType
UNION ALL
SELECT 'Name'=
CASE 
WHEN [Name] IS null THEN REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
WHEN [Name]='' THEN  REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
ELSE [Name] 
END, [Path] 
FROM Constant_Menu 
ORDER BY Path

GO
--****************************************************************************************************

CREATE PROCEDURE [dbo].[Group_AssignedPermission]
 @GroupID int
AS
BEGIN
SELECT PermissionName=REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END)),T1.AccountID, T1.[PermissionType],T1.Value 
FROM Constant_PermissionType T0
INNER JOIN User_GUI_AccountPermission T1 ON T0.Path=T1.PermissionType
WHERE T1.TargetID=@GroupID AND T1.TargetIsGroup=1
UNION
SELECT 'PermissionName'=
CASE 
WHEN [Name] IS null THEN REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
WHEN [Name]='' THEN  REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
ELSE [Name] 
END,T3.AccountID, [Path] AS 'PermissionType' ,T3.Value
FROM Constant_Menu T2
INNER JOIN User_GUI_AccountPermission T3 ON  REPLACE(T3.PermissionType,'menu:','')=T2.Path
WHERE T3.TargetID=@GroupID AND T3.TargetIsGroup=1
ORDER BY PermissionType
END

GO

--****************************************************************************************************
CREATE PROCEDURE [dbo].[User_AssignedPermission]
 @UserID int
AS
BEGIN
SELECT PermissionName=REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END)),T1.AccountID, T1.[PermissionType],T1.Value 
FROM Constant_PermissionType T0
INNER JOIN User_GUI_AccountPermission T1 ON T0.Path=T1.PermissionType
WHERE T1.TargetID=@UserID AND T1.TargetIsGroup=0
UNION
SELECT 'PermissionName'=
CASE 
WHEN [Name] IS null THEN REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
WHEN [Name]='' THEN  REVERSE(SUBSTRING(REVERSE([Path]), 0, CASE CHARINDEX('/', REVERSE([Path])) WHEN 0 THEN LEN([Path]) + 1 ELSE CHARINDEX('/', REVERSE([Path])) END))
ELSE [Name] 
END,T3.AccountID, [Path] AS 'PermissionType' ,T3.Value
FROM Constant_Menu T2
INNER JOIN User_GUI_AccountPermission T3 ON  REPLACE(T3.PermissionType,'menu:','')=T2.Path
WHERE T3.TargetID=@UserID AND T3.TargetIsGroup=0
ORDER BY PermissionType
END

GO
--****************************************************************************************************

ALTER Procedure [dbo].[User_Operations]
@Action as Int,
@Name as nvarchar(50)=null,
@AccountAdmin as bit=null,
@IsActive as bit=null,
@Email as nvarchar(255)=null,
@Password as nvarchar(20)=null,
@UserID as int=null

AS
BEGIN

if  @Action=1 /*INSERT*/
BEGIN
INSERT INTO User_GUI_User
(Name,IsActive,AccountAdmin,Email,Password)
VALUES 
(@Name,1,@AccountAdmin,@Email,@Password);SELECT @UserID=@@IDENTITY
END

if  @Action=2 /*UPDATE*/

BEGIN
UPDATE User_GUI_User
SET Name=ISNULL(@Name,Name),
IsActive=ISNULL(@IsActive,IsActive),
AccountAdmin=ISNULL(@AccountAdmin,AccountAdmin),
Email=ISNULL(@Email,Email),
Password=ISNULL(@Password,Password)
WHERE UserID=@UserID
END
if  @Action=3 /*DELETE*/
BEGIN
DELETE FROM User_GUI_User
WHERE UserID =@UserID
END
/* in any case!!! ( delete/update/insert) we clear assigned permission and assigned groups
in case of update and insert we are inserting it in the code again*/

/*AssignedPermissions*/
DELETE FROM User_GUI_AccountPermission
WHERE TargetID=@UserID AND TargetIsGroup=0

/*AssignedGroups*/
DELETE FROM User_GUI_UserGroupUser
WHERE UserID=@UserID

SELECT @UserID
END

GO
--****************************************************************************************************
alter table UserProcess_GUI_PaidCampaign
add [ScheduleEnabled] [bit] NOT NULL DEFAULT (0)
GO
--****************************************************************************************************
CREATE TABLE [ServicesConfigIDS](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ServiceConfigurationName] [nvarchar](255) NOT NULL
) ON [PRIMARY]

GO
--****************************************************************************************************

CREATE TABLE [ServiceConfigExecutionTimes](
	[ConfigName] [nvarchar](255) NOT NULL,
	[ProfileID] [int] NOT NULL,
	[Percentile] [int] NOT NULL,
	[Value] [bigint] NOT NULL,
 CONSTRAINT [PK_ServiceConfigExecutionTimes_1] PRIMARY KEY CLUSTERED 
(
	[ConfigName] ASC,
	[ProfileID] ASC,
	[Percentile] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
--****************************************************************************************************

CREATE TABLE [Campaigns_Scheduling](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Account_ID] [int] NOT NULL,
	[Campaign_GK] [int] NOT NULL,
	[Channel_ID] [int] NOT NULL,
	[Day] [int] NOT NULL,
	[Hour00] [int] NULL,
	[Hour01] [int] NULL,
	[Hour02] [int] NULL,
	[Hour03] [int] NULL,
	[Hour04] [int] NULL,
	[Hour05] [int] NULL,
	[Hour06] [int] NULL,
	[Hour07] [int] NULL,
	[Hour08] [int] NULL,
	[Hour09] [int] NULL,
	[Hour10] [int] NULL,
	[Hour11] [int] NULL,
	[Hour12] [int] NULL,
	[Hour13] [int] NULL,
	[Hour14] [int] NULL,
	[Hour15] [int] NULL,
	[Hour16] [int] NULL,
	[Hour17] [int] NULL,
	[Hour18] [int] NULL,
	[Hour19] [int] NULL,
	[Hour20] [int] NULL,
	[Hour21] [int] NULL,
	[Hour22] [int] NULL,
	[Hour23] [int] NULL,
 CONSTRAINT [PK_Facebook_Campaign_StatusByTime] PRIMARY KEY CLUSTERED 
(
	[Campaign_GK] ASC,
	[Day] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO
--****************************************************************************************************
CREATE TABLE [ExchangeRates](
	[RateDateTime] [datetime] NOT NULL,
	[CurrencyID] [int] NOT NULL,
	[Rate] [decimal](18, 4) NULL,
	[DayCode] [int] NULL,
 CONSTRAINT [PK_ExchangeRates] PRIMARY KEY CLUSTERED 
(
	[RateDateTime] ASC,
	[CurrencyID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

--****************************************************************************************************
ALTER PROCEDURE [dbo].[User_GetByID]
		@userID int
AS
BEGIN
	SET NOCOUNT ON;

	select
		usr.UserID,
		usr.Name,
		usr.Email,
		usr.IsActive,
		usr.AccountAdmin,		
		cast(
				(case when max(cast(usr.AccountAdmin as int)) > 0 or max(cast(grp.AccountAdmin as int)) > 0 then 1 else 0 end)
				as bit
			)
			as AccountAdmin
	from
		User_GUI_User usr
		left outer join User_GUI_UserGroupUser usrGroups on
			usrGroups.UserID = @userID
		left outer join User_GUI_UserGroup grp on
			grp.GroupID = usrGroups.GroupID and 
			grp.IsActive = 1
	where
		usr.UserID = @userID and
		usr.IsActive = 1
	group by
		usr.UserID,
		usr.Name,
		usr.Email,
		usr.IsActive,
		usr.AccountAdmin
END

GO
--****************************************************************************************************

CREATE procedure [dbo].[SP_UpdateService_Times]
@AccountID int,
@ServiceName NvarChar(255)
AS
Begin
Insert into Source.dbo.Service_Account_Times
(StartEndDateTime,AccountID,ServiceName)
values
(GETDATE(),@AccountID,@ServiceName)
END

GO

--****************************************************************************************************
CREATE PROCEDURE [dbo].[ServiceConfiguration_GetExecutionTime]
@ConfigName NvarChar(255),
@Percentile INT,
@ProfileID INT
AS
BEGIN
declare @Value as decimal(18,2)
SELECT @Value= Value
FROM ServiceConfigExecutionTimes
WHERE ConfigName=@ConfigName AND Percentile=@Percentile  AND ProfileID=@ProfileID

if @Value is not null
select @Value
else
insert into ServiceConfigExecutionTimes
values (@ConfigName,@ProfileID,@Percentile,5)
select 5
END

GO


--****************************************************************************************************
CREATE PROCEDURE [dbo].[CampaignStatusSchedules_GetByCampaignGK]
@Campaign_GK AS INT
AS
BEGIN
SELECT  T0.[Account_ID]
      , T0.[Campaign_GK]
      , T0.[Channel_ID]
      , T0.[Day]
      ,T1.ScheduleEnabled
      , T0.[Hour00]
      , T0.[Hour01]
      , T0.[Hour02]
      , T0.[Hour03]
      , T0.[Hour04]
      , T0.[Hour05]
      ,T0.[Hour06]
      ,T0.[Hour07]
      ,T0.[Hour08]
      ,T0.[Hour09]
      ,T0.[Hour10]
      ,T0.[Hour11]
      ,T0.[Hour12]
      ,T0.[Hour13]
      ,T0.[Hour14]
      ,T0.[Hour15]
      ,T0.[Hour16]
      ,T0.[Hour17]
      ,T0.[Hour18]
      ,T0.[Hour19]
      ,T0.[Hour20]
      ,T0.[Hour21]
      ,T0.[Hour22]
      ,T0.[Hour23]
  FROM [testdb].[dbo].Campaigns_Scheduling T0
  INNER JOIN UserProcess_GUI_PaidCampaign T1 ON T0.Campaign_GK=T1.Campaign_GK
  WHERE T0.Campaign_GK=@Campaign_GK 
  ORDER BY [Day]
END

GO


--****************************************************************************************************
CREATE PROCEDURE  [dbo].[CampaignStatusSchedule_Insert]
@Action AS Int,
@Account_ID AS int,
@Campaign_GK AS int,
@Channel_ID AS int,
@Day AS int,
@Hour00 AS int,
@Hour01 AS int,
@Hour02 AS int,
@Hour03 AS int,
@Hour04 AS int,
@Hour05 AS int,
@Hour06 AS int,
@Hour07 AS int,
@Hour08 AS int,
@Hour09 AS int,
@Hour10 AS int,
@Hour11 AS int,
@Hour12 AS int,
@Hour13 AS int,
@Hour14 AS int,
@Hour15 AS int,
@Hour16 AS int,
@Hour17 AS int,
@Hour18 AS int,
@Hour19 AS int,
@Hour20 AS int,
@Hour21 AS int,
@Hour22 AS int,
@Hour23 AS int
AS
BEGIN
DELETE FROM Campaigns_Scheduling
WHERE Campaign_GK=@Campaign_GK AND [Day]=@Day

INSERT INTO Campaigns_Scheduling
           ([Account_ID] ,[Campaign_GK]
           ,[Channel_ID]
           ,[Day]
           ,[Hour00]
           ,[Hour01]
           ,[Hour02]
           ,[Hour03]
           ,[Hour04]
           ,[Hour05]
           ,[Hour06]
           ,[Hour07]
           ,[Hour08]
           ,[Hour09]
           ,[Hour10]
           ,[Hour11]
           ,[Hour12]
           ,[Hour13]
           ,[Hour14]
           ,[Hour15]
           ,[Hour16]
           ,[Hour17]
           ,[Hour18]
           ,[Hour19]
           ,[Hour20]
           ,[Hour21]
           ,[Hour22]
           ,[Hour23])
     VALUES
           (@Account_ID ,
           @Campaign_GK ,
           @Channel_ID ,
           @Day ,
           @Hour00 ,
           @Hour01 ,
           @Hour02 ,
           @Hour03 ,
           @Hour04 ,
           @Hour05 ,
           @Hour06 ,
           @Hour07 ,
           @Hour08 ,
           @Hour09 ,
           @Hour10 ,
           @Hour11 ,
           @Hour12 ,
           @Hour13 ,
           @Hour14 ,
           @Hour15 ,
           @Hour16 ,
           @Hour17 ,
           @Hour18 ,
           @Hour19 ,
           @Hour20 ,
           @Hour21 ,
           @Hour22 ,
           @Hour23 );SELECT @@IDENTITY; 
END
GO


--****************************************************************************************************
ALTER PROCEDURE [dbo].[CampaignByAccountAndChannel]
@Channel_ID AS INT,
@Account_ID AS INT

AS
BEGIN
SELECT Campaign_GK,campaignid,campaign,Account_ID,Channel_ID,campStatus ,ScheduleEnabled
FROM UserProcess_GUI_PaidCampaign 
WHERE Account_ID=@Account_ID AND  Channel_ID=@Channel_ID AND campStatus<>3 ORDER BY Campaign_GK
END

GO




--****************************************************************************************************
IF @@ERROR<>0
BEGIN
ROLLBACK TRAN
END
ELSE
BEGIN 
COMMIT TRANSACTION
END
GO