set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

ALTER PROCEDURE GkManager_GetAdgroupKeywordGK
	-- Identity columns
	@Account_ID		Int,			-- 1
	@Channel_ID		Int,			-- 2 
	@Campaign_GK	BigInt,			-- 3
	@AdGroup_GK		BigInt,			-- 4
	@Keyword_GK		BigInt,			-- 5
	@MatchType		Int,			-- 6

	-- Additional columns
	@kwDestUrl		NVarChar(MAX),	-- 7
	@Gateway_GK		BigInt			-- 8
AS
BEGIN
	SET NOCOUNT ON;

	-- Comparison values
	declare @current_kwDestUrl		NVarChar(MAX);	-- 7
	declare @current_Gateway_GK		BigInt;			-- 8

	-- Return value
	declare @returnValue BigInt;

	if
	(
		@Account_ID is null or
		@Channel_ID is null or
		@Campaign_GK is null or
		@AdGroup_GK is null or
		@Keyword_GK is null or
		@MatchType is null
	)
		return;

	/*******************************************/
	begin transaction

	-- Lookup
	select top 1
		@returnValue			= PPC_Keyword_GK,
		@current_kwDestUrl		= kwDestUrl,
		@current_Gateway_GK		= Gateway_GK
	from
		UserProcess_GUI_PaidAdgroupKeyword with(SERIALIZABLE, XLOCK)
	where
		Account_ID = @Account_ID and
		Channel_ID = @Channel_ID and
		Campaign_GK = @Campaign_GK and
		AdGroup_GK = @AdGroup_GK and
		Keyword_GK = @Keyword_GK and
		MatchType = @MatchType;

	-- Insert
	if @returnValue is null
	begin
		insert into UserProcess_GUI_PaidAdgroupKeyword
		(
			Account_ID,
			Channel_ID,
			Campaign_GK,
			AdGroup_GK,
			Keyword_GK,
			MatchType,
			kwDestUrl,
			Gateway_GK,
			LastUpdated,
			Segment1,
			Segment2,
			Segment3,
			Segment4,
			Segment5
		)
		select
			@Account_ID,
			@Channel_ID,
			@Campaign_GK,
			@AdGroup_GK,
			@Keyword_GK,
			@MatchType,
			@kwDestUrl,
			@Gateway_GK,
			getdate(),
			(select Segment1 from UserProcess_GUI_PaidAdgroup where Adgroup_GK = @AdGroup_GK),
			(select Segment2 from UserProcess_GUI_PaidAdgroup where Adgroup_GK = @AdGroup_GK),
			(select Segment3 from UserProcess_GUI_PaidAdgroup where Adgroup_GK = @AdGroup_GK),
			(select Segment4 from UserProcess_GUI_PaidAdgroup where Adgroup_GK = @AdGroup_GK),
			(select Segment5 from UserProcess_GUI_PaidAdgroup where Adgroup_GK = @AdGroup_GK);

		set @returnValue = scope_identity();
	end

	-- Update
	else
	begin
		if
			isnull(@current_kwDestUrl, '')	 != isnull(@kwDestUrl, '') or
			isnull(@current_Gateway_GK, -1)	 != isnull(@Gateway_GK, -1)
		begin
			update UserProcess_GUI_PaidAdgroupKeyword
			set
				kwDestUrl		= @kwDestUrl,
				Gateway_GK		= @Gateway_GK,
				LastUpdated		= getdate()
			where
				Account_ID = @Account_ID and
				Channel_ID = @Channel_ID and
				Campaign_GK = @Campaign_GK and
				AdGroup_GK = @AdGroup_GK and
				Keyword_GK = @Keyword_GK and
				MatchType = @MatchType;

				--PPC_Keyword_GK = @returnValue;
		end
	end

	commit

	select @returnValue;

END

