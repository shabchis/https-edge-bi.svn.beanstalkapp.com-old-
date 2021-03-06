set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

ALTER PROCEDURE GkManager_GetCampaignGK
	-- Identity columns
	@Account_ID		Int,			-- 1
	@Channel_ID		Int,			-- 2
	@campaign		NVarChar(MAX),	-- 3

	-- Additional columns
	@campaignid		BigInt			-- 4
AS
BEGIN
	SET NOCOUNT ON;

	-- Comparison values
	declare @current_campaignid		BigInt;			-- 4

	-- Return value
	declare @returnValue BigInt;

	if
	(
		@Account_ID is null or
		@Channel_ID is null or
		@campaign is null
	)
		return;

	/*******************************************/
	begin transaction

	-- Lookup
	select top 1
		@returnValue			= Campaign_GK,
		@current_campaignid		= campaignid
	from
		UserProcess_GUI_PaidCampaign with(SERIALIZABLE, XLOCK)
	where
		Account_ID = @Account_ID and
		Channel_ID = @Channel_ID and
		campaign = @campaign;

	-- Insert
	if @returnValue is null
	begin
		insert into UserProcess_GUI_PaidCampaign
		(
			Account_ID,
			Channel_ID,
			campaign,
			campaignid,
			LastUpdated
		)
		select
			@Account_ID,
			@Channel_ID,
			@campaign,
			@campaignid,
			getdate();

		set @returnValue = scope_identity();
	end

	-- Update
	else
	begin
		if
			isnull(@current_campaignid,-1)	 != isnull(@campaignid,-1)
		begin
			update UserProcess_GUI_PaidCampaign
			set
				campaignid		= @campaignid,
				LastUpdated		= getdate()
			where
				Account_ID = @Account_ID and
				Channel_ID = @Channel_ID and
				campaign = @campaign;
				--Campaign_GK = @returnValue;
		end
	end

	commit

	select @returnValue;

END

