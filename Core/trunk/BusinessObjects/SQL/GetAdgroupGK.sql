set ANSI_NULLS ON
set QUOTED_IDENTIFIER ON
go

ALTER PROCEDURE GkManager_GetAdgroupGK
	-- Identity columns
	@Account_ID		Int,			-- 1
	@Channel_ID		Int,			-- 2 
	@Campaign_GK	BigInt,			-- 3
	@adgroup		NVarChar(MAX),	-- 4

	-- Additional columns
	@adgroupID		BigInt			-- 5
AS
BEGIN
	SET NOCOUNT ON;

	-- Comparison values
	declare @current_adgroupID		BigInt;			-- 5

	-- Return value
	declare @returnValue BigInt;

	if
	(
		@Account_ID is null or
		@Channel_ID is null or
		@Campaign_GK is null or
		@adgroup is null
	)
		return;

	/*******************************************/
	begin transaction

	-- Lookup
	select top 1
		@returnValue			= Adgroup_GK,
		@current_adgroupID		= adgroupID
	from
		UserProcess_GUI_PaidAdGroup with(SERIALIZABLE, XLOCK)
	where
		Account_ID = @Account_ID and
		Channel_ID = @Channel_ID and
		Campaign_GK = @Campaign_GK and
		adgroup = @adgroup;

	-- Insert
	if @returnValue is null
	begin
		insert into UserProcess_GUI_PaidAdGroup
		(
			Account_ID,
			Channel_ID,
			Campaign_GK,
			adgroup,
			adgroupID,
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
			@adgroup,
			@adgroupID,
			getdate(),
			(select Segment1 from UserProcess_GUI_PaidCampaign where Campaign_GK = @Campaign_GK),
			(select Segment2 from UserProcess_GUI_PaidCampaign where Campaign_GK = @Campaign_GK),
			(select Segment3 from UserProcess_GUI_PaidCampaign where Campaign_GK = @Campaign_GK),
			(select Segment4 from UserProcess_GUI_PaidCampaign where Campaign_GK = @Campaign_GK),
			(select Segment5 from UserProcess_GUI_PaidCampaign where Campaign_GK = @Campaign_GK);

		set @returnValue = scope_identity();
	end

	-- Update
	else
	begin
		if
			isnull(@current_adgroupID,-1) != isnull(@adgroupID,-1)
		begin
			update UserProcess_GUI_PaidAdGroup
			set
				adgroupID		= @adgroupID,
				LastUpdated		= getdate()
			where
				Account_ID = @Account_ID and
				Channel_ID = @Channel_ID and
				Campaign_GK = @Campaign_GK and
				adgroup = @adgroup;

				--Adgroup_GK = @returnValue;
		end
	end

	commit

	select @returnValue;

END

