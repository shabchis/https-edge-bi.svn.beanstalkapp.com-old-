set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

ALTER PROCEDURE GkManager_GetKeywordGK
	-- Identity columns
	@Account_ID		Int,			-- 1
	@Keyword		NVarChar(MAX)	-- 2
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
		@Keyword is null
	)
		return;


	/*******************************************/
	begin transaction

	-- Lookup
	select top 1
		@returnValue = Keyword_GK
	from
		UserProcess_GUI_Keyword with(SERIALIZABLE, XLOCK)
	where
		isnull(Account_ID,-1) = isnull(@Account_ID,-1) and
		isnull(Keyword,'') = isnull(@Keyword,'');

	-- insert
	if @returnValue is null
	begin
		insert into UserProcess_GUI_Keyword
		(
			Account_ID,
			Keyword,
			LastUpdated
		)
		select
			@Account_ID,
			@Keyword,
			getdate();

		set @returnValue = scope_identity();
	end

	/*
	-- update
	else
	begin
		if
		begin
			update UserProcess_GUI_Keyword
			set
				LastUpdated = getdate()
			where
				Keyword_GK = @returnValue;
		end
	end
	*/
	
	commit

	select @returnValue;

END

