set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

ALTER PROCEDURE GkManager_GetCreativeGK
	-- Identity columns
	@Account_ID		Int,	-- 1
	@Creative_Title	NVarChar(MAX),	-- 2
	@Creative_Desc1	NVarChar(MAX),	-- 3
	@Creative_Desc2	NVarChar(MAX)	-- 4
AS
BEGIN
	SET NOCOUNT ON;

	-- Comparison values
	-- <NONE>

	-- Return value
	declare @returnValue BigInt;

	if
	(
		@Account_ID is null or
		@Creative_Title is null or
		@Creative_Desc1 is null or
		@Creative_Desc2 is null
	)
		return;

	/*******************************************/
	begin transaction

	-- Lookup
	select top 1
		@returnValue = Creative_GK
	from
		UserProcess_GUI_Creative with(SERIALIZABLE, XLOCK)
	where
		Account_ID = @Account_ID and
		Creative_Title = @Creative_Title and 
		Creative_Desc1 = @Creative_Desc1 and
		Creative_Desc2 = @Creative_Desc2;

	-- insert
	if @returnValue is null
	begin
		insert into UserProcess_GUI_Creative
		(
			Account_ID,
			Creative_Title,
			Creative_Desc1,
			Creative_Desc2,
			LastUpdated
		)
		select
			@Account_ID,
			@Creative_Title,
			@Creative_Desc1,
			@Creative_Desc2,
			getdate();

		set @returnValue = scope_identity();
	end

	/*
	-- update
	else
	begin
		if
		begin
			update UserProcess_GUI_Creative
			set
				LastUpdated = getdate()
			where
				Creative_GK = @returnValue;
		end
	end
	*/
	
	commit

	select @returnValue;

END

