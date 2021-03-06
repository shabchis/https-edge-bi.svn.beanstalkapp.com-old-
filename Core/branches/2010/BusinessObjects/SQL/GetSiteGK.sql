set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

ALTER PROCEDURE GkManager_GetSiteGK
	-- Identity columns
	@Account_ID		Int,			-- 1
	@Site			NVarChar(MAX)	-- 2
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
		@Site is null
	)
		return;

	/*******************************************/

	begin transaction

	-- Lookup
	select top 1
		@returnValue = Site_GK
	from
		UserProcess_GUI_Site with(SERIALIZABLE, XLOCK)
	where
		Account_ID = @Account_ID and
		Site = @Site;

	-- insert
	if @returnValue is null
	begin
		insert into UserProcess_GUI_Site
		(
			Account_ID,
			Site,
			LastUpdated
		)
		select
			@Account_ID,
			@Site,
			getdate();

		set @returnValue = scope_identity();
	end

	/*
	-- update
	else
	begin
		if
		begin
			update UserProcess_GUI_Site
			set
				LastUpdated = getdate()
			where
				Site_GK = @returnValue;
		end
	end
	*/
	
	commit

	select @returnValue;

END

