
///**
//* Copyright (c) 2006-2007, Yahoo! Inc. All rights reserved.
//* Code licensed under the BSD License:
//* http://developer.yahoo.com/yui/license.txt
//* version: 0.10.0
//*/
//using System;
//using System.IO;
//using System.Collections;
//using System.Text;
//using System.Net;
//using System.Collections.Generic;
//using System.Globalization;

//namespace YahooEWSClient
//{
//    class YahooEWSClient
//    {

//        private static bool DEBUG = true;

//        private static string EWS_ACCESS_HTTP_PROTOCOL = "https";
//        private static string EWS_LOCATION_SERVICE_ENDPOINT = "sandbox.marketing.ews.yahooapis.com";

//        private static string MARKET = "US";
//        private static string LOCALE = "en_US";
//        private static Encoding Charset = Encoding.UTF8;
//        private static masterAccountID _masterAccountID;
//        private static accountID _AccountID;
//        private static string _accountID;
//        private static Security _securityValue;
//        private static license _license;
//        private static Hashtable _locationCache = new Hashtable();
//        private static Hashtable _localizedHash = new Hashtable();
//        private static string _locationCacheFilename;
//        private static DateTime _starttime;
//        private static LocationServiceService _locationService = null;
//        private static CampaignServiceService _campaignService = null;
//        private static AdGroupServiceService _adGroupService = null;
//        private static AdServiceService _adService = null;
//        private static KeywordServiceService _keywordService = null;
//        private static ExcludedWordsServiceService _excludedWordsService = null;
//        //enumerations for use in printing rejection reasons
//        private const int AD_TYPE = 1;
//        private const int KEYWORD_TYPE = 2;

//        // DUNE
//        //Constructor
//        public YahooEWSClient()
//        {

//            debug("creating sample objects for " + MARKET + " market");

//            try
//            {
//                _locationService = new LocationServiceService();
//                _campaignService = new CampaignServiceService();
//                _adGroupService = new AdGroupServiceService();
//                _adService = new AdServiceService();
//                _keywordService = new KeywordServiceService();
//                _excludedWordsService = new ExcludedWordsServiceService();
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//            loadLocalizedPropertiesFile();
//            loadLocationCache();
//            _starttime = DateTime.Now;
//            _masterAccountID = new masterAccountID();
//            _masterAccountID.Text = new String[] { "1141842001366" };
//            _AccountID = new accountID();
//            _AccountID.Text = new String[] { "1141842001367" };
//            _accountID = "1141842001367";
//            _securityValue = new Security();
//            _securityValue.UsernameToken = new UsernameToken();
//            _securityValue.UsernameToken.Username = "myusername";
//            _securityValue.UsernameToken.Password = "mypassword";
//            _license = new license();
//            _license.Text = new String[] { "B3C5D81911F3FB68" };
//            string endPointLocation = getEndPointFromLocationService(_masterAccountID.Text[0]);

//            createClientService(endPointLocation);
//        }

//        private void createClientService(string endPointLocation)
//        {
//            // setup client service information
//            _campaignService.Url = endPointLocation + "/V4/CampaignService";
//            _campaignService.masterAccountIDValue = _masterAccountID;
//            _campaignService.SecurityValue = _securityValue;
//            _campaignService.accountIDValue = _AccountID;
//            _campaignService.licenseValue = _license;
//            _adGroupService.Url = endPointLocation + "/V4/AdGroupService";
//            _adGroupService.masterAccountIDValue = _masterAccountID;
//            _adGroupService.SecurityValue = _securityValue;
//            _adGroupService.accountIDValue = _AccountID;
//            _adGroupService.licenseValue = _license;
//            _adService.Url = endPointLocation + "/V4/AdService";
//            _adService.masterAccountIDValue = _masterAccountID;
//            _adService.SecurityValue = _securityValue;
//            _adService.accountIDValue = _AccountID;
//            _adService.licenseValue = _license;
//            _keywordService.Url = endPointLocation + "/V4/KeywordService";
//            _keywordService.masterAccountIDValue = _masterAccountID;
//            _keywordService.SecurityValue = _securityValue;
//            _keywordService.accountIDValue = _AccountID;
//            _keywordService.licenseValue = _license;
//            _excludedWordsService.Url = endPointLocation + "/V4/ExcludedWordsService";
//            _excludedWordsService.masterAccountIDValue = _masterAccountID;
//            _excludedWordsService.SecurityValue = _securityValue;
//            _excludedWordsService.accountIDValue = _AccountID;
//            _excludedWordsService.licenseValue = _license;
//        }
//        /// <summary>
//        /// The main entry point for the application.
//        /// </summary>
//        [STAThread]
//        static void Main(string[] args)
//        {

//            YahooEWSClient client = new YahooEWSClient();
//            try
//            {
//                //create and add campaign
//                Campaign myCampaign = client.createCampaign();
//                myCampaign = client.addCampaign(myCampaign);
//                if (myCampaign == null) throw new Exception("campaign object is null");

//                //create and add ad group
//                AdGroup myAdGroup = client.createAdGroup((long)myCampaign.ID);
//                myAdGroup = client.addAdGroup(myAdGroup);
//                if (myAdGroup == null) throw new Exception("Ad Group object is null");

//                //create and add ads
//                Ad[] myAds = client.createAds((long)myAdGroup.ID);
//                myAds = client.addAds(myAds);
//                if (myAds == null) throw new Exception("Ads object is null");

//                //create and add keywords
//                Keyword[] myKeywords = client.createKeywords((long)myAdGroup.ID);
//                myKeywords = client.addKeywords(myKeywords);
//                if (myKeywords == null) throw new Exception("Keyword object is null");

//                //create and add negative keywords
//                ExcludedWord[] myExcludedWords = client.createExcludedWordsForAdGroup((long)myAdGroup.ID);
//                myExcludedWords = client.addExcludedWordsToAdGroup(myExcludedWords);
//                if (myExcludedWords == null) throw new Exception("Excluded Word object is null");
//            }
//            catch (Exception e)
//            {
//                client.debug(e.Message);
//            }
//            finally
//            {
//                Console.Write("Please press [ENTER] to exit....");
//                Console.Read();
//            }
//        }

//        /**
//         * Creates an individual Campaign data object.
//         *
//         * @return The created Campaign
//         */
//        private Campaign createCampaign()
//        {
//            try
//            {
//                Campaign campaign = new Campaign();
//                campaign.name = getLocalizedString("CAMPAIGN_NAME");
//                campaign.description = getLocalizedString("CAMPAIGN_DESCRIPTION");
//                campaign.accountID = _accountID;
//                campaign.sponsoredSearchON = true;
//                campaign.advancedMatchON = true;
//                campaign.campaignOptimizationON = true;
//                campaign.contentMatchON = true;
//                // NOTE: .NET only allows the date to be in the timezone of the machine running the application.
//                // If your master account timezone is different from the machine timezone make sure to adjust the date to the local time.
//                // For more information see http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dndotnet/html/datetimecode.asp
//                DateTime startDate = DateTime.Now.Date;
//                startDate = startDate.AddDays(1);
//                campaign.startDate = startDate;
//                campaign.startDateSpecified = true;
//                DateTime endDate = startDate.AddYears(1);
//                campaign.endDate = endDate;
//                campaign.endDateSpecified = true;
//                campaign.status = CampaignStatus.On;
//                campaign.statusSpecified = true;
//                campaign.watchON = false;
//                return campaign;
//            }
//            catch (Exception e)
//            {
//                debug(e.Message);
//                throw new Exception(e.Message);
//            }
//        }
//        /**
//         * Adds a campaign to the account.
//         *
//         * @param campaign
//         * @return Campaign that was added, including the assigned campaign ID.
//         * @throws RemoteException
//         */
//        private Campaign addCampaign(Campaign campaign)
//        {

//            try
//            {
//                debug("adding campaign...");
//                CampaignResponse campaignResponse = _campaignService.addCampaign(campaign);
//                printHeaders(_campaignService.remainingQuotaValue.Text[0], _campaignService.commandGroupValue.Text[0], _campaignService.timeTakenMillisValue.Text[0]);
//                if ((bool)campaignResponse.operationSucceeded)
//                {
//                    return campaignResponse.campaign;
//                }
//                else
//                {
//                    printErrors(campaignResponse.errors);
//                }
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//            return null;
//        }
//        /**
//         * Creates an individual Ad Group data object.
//         *
//         * @param campaignID
//         * @return The created Ad Group
//         */
//        private AdGroup createAdGroup(long campaignID)
//        {
//            try
//            {
//                AdGroup adGroup = new AdGroup();
//                adGroup.name = getLocalizedString("ADGROUP_NAME");
//                adGroup.advancedMatchON = false;
//                adGroup.advancedMatchONSpecified = true;
//                adGroup.contentMatchON = true;
//                adGroup.contentMatchONSpecified = true;
//                adGroup.contentMatchMaxBid = Convert.ToDouble(getLocalizedString("ADGROUP_CM_MAX_BID"));
//                adGroup.contentMatchMaxBidSpecified = true;
//                adGroup.adAutoOptimizationON = false;
//                adGroup.adAutoOptimizationONSpecified = true;
//                adGroup.campaignID = campaignID;
//                adGroup.campaignIDSpecified = true;
//                adGroup.sponsoredSearchON = true;
//                adGroup.sponsoredSearchONSpecified = true;
//                adGroup.sponsoredSearchMaxBid = Convert.ToDouble(getLocalizedString("ADGROUP_SS_MAX_BID"));
//                adGroup.sponsoredSearchMaxBidSpecified = true;
//                adGroup.status = AdGroupStatus.On;
//                adGroup.statusSpecified = true;
//                adGroup.watchON = false;
//                adGroup.watchONSpecified = true;
//                return adGroup;
//            }
//            catch (Exception e)
//            {
//                debug(e.Message);
//                throw new Exception(e.Message);
//            }
//        }
//        /**
//         * Adds an ad group to a campaign.
//         *
//         * @param adGroup
//         * @return Ad Group that was added, including the assigned Ad Group ID.
//         * @throws RemoteException
//         */
//        private AdGroup addAdGroup(AdGroup adGroup)
//        {
//            try
//            {
//                debug("adding ad group...");
//                AdGroupResponse adGroupResponse = _adGroupService.addAdGroup(adGroup);
//                printHeaders(_adGroupService.remainingQuotaValue.Text[0], _adGroupService.commandGroupValue.Text[0], _adGroupService.timeTakenMillisValue.Text[0]);
//                if ((bool)adGroupResponse.operationSucceeded)
//                {
//                    return adGroupResponse.adGroup;
//                }
//                else
//                {
//                    printErrors(adGroupResponse.errors);
//                }
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//            return null;
//        }
//        /**
//        * Creates two Ad data objects and adds them to an array.
//        *
//        * @param adGroupID
//        * @return Array of created Ads
//        */
//        private Ad[] createAds(long adGroupID)
//        {
//            try
//            {
//                Ad[] ads = new Ad[2];
//                Ad ad1 = new Ad();
//                ad1.name = getLocalizedString("AD1_NAME");
//                ad1.title = getLocalizedString("AD1_TITLE");
//                ad1.status = AdStatus.On;
//                ad1.statusSpecified = true;
//                ad1.adGroupID = adGroupID;
//                ad1.adGroupIDSpecified = true;
//                ad1.displayUrl = getLocalizedString("AD1_DISPLAY_URL");
//                ad1.shortDescription = getLocalizedString("AD1_SHORT_DESC");
//                ad1.description = getLocalizedString("AD1_DESC");
//                ad1.url = getLocalizedString("AD1_URL");
//                ads[0] = ad1;
//                Ad ad2 = new Ad();
//                ad2.name = getLocalizedString("AD2_NAME");
//                ad2.title = getLocalizedString("AD2_TITLE");
//                ad2.status = AdStatus.On;
//                ad2.statusSpecified = true;
//                ad2.adGroupID = adGroupID;
//                ad2.adGroupIDSpecified = true;
//                ad2.displayUrl = getLocalizedString("AD2_DISPLAY_URL");
//                ad2.shortDescription = getLocalizedString("AD2_SHORT_DESC");
//                ad2.description = getLocalizedString("AD2_DESC");
//                ad2.url = getLocalizedString("AD2_URL");
//                ads[1] = ad2;
//                return ads;
//            }
//            catch (Exception e)
//            {
//                debug(e.Message);
//                throw new Exception(e.Message);
//            }
//        }
//        /**
//        * Adds multiple ads to an ad group.
//        *
//        * @param ads
//        * @return Ads that were added, including the assigned Ad IDs.
//        * @throws RemoteException
//        */
//        private Ad[] addAds(Ad[] ads)
//        {
//            try
//            {
//                debug("adding ads...");
//                AdResponse[] adResponses = _adService.addAds(ads);
//                printHeaders(_adService.remainingQuotaValue.Text[0], _adService.commandGroupValue.Text[0], _adService.timeTakenMillisValue.Text[0]);
//                ArrayList addedAdsList = new ArrayList();
//                for (int i = 0; i < adResponses.Length; i++)
//                {
//                    AdResponse adResponse = adResponses[i];
//                    if ((bool)adResponse.operationSucceeded)
//                    {
//                        addedAdsList.Add(adResponse.ad);
//                    }
//                    else
//                    {
//                        debug("Error adding ad index " + i + " in list...");
//                        printErrors(adResponse.errors);
//                        //print rejection reasons if any of the ads were rejected already
//                        printRejectionReasonsForAd(adResponse.editorialReasons, i);
//                    }
//                }
//                Ad[] addedAds = new Ad[addedAdsList.Count];
//                addedAds = (Ad[])addedAdsList.ToArray(typeof(Ad));
//                return addedAds;
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//            return null;
//        }
//        /**
//         * Creates four Keyword data objects.
//         *
//         * @param adGroupID
//         * @return Array of created Keywords
//         */
//        private Keyword[] createKeywords(long adGroupID)
//        {
//            try
//            {
//                Keyword[] keywords = new Keyword[4];
//                //first keyword
//                Keyword keyword1 = new Keyword();
//                keyword1.advancedMatchON = true;
//                keyword1.advancedMatchONSpecified = true;
//                keyword1.text = getLocalizedString("KEYWORD1_TEXT");
//                keyword1.status = KeywordStatus.On;
//                keyword1.statusSpecified = true;
//                keyword1.adGroupID = adGroupID;
//                keyword1.adGroupIDSpecified = true;
//                keyword1.editorialStatus = EditorialStatus.Pending;
//                keyword1.editorialStatusSpecified = true;
//                keyword1.advancedMatchON = true;
//                keyword1.advancedMatchONSpecified = true;
//                keyword1.watchON = true;
//                keyword1.watchONSpecified = true;
//                keywords[0] = keyword1;
//                //second keyword
//                Keyword keyword2 = new Keyword();
//                keyword2.advancedMatchON = true;
//                keyword2.advancedMatchONSpecified = true;
//                keyword2.text = getLocalizedString("KEYWORD2_TEXT");
//                keyword2.status = KeywordStatus.On;
//                keyword2.statusSpecified = true;
//                keyword2.adGroupID = adGroupID;
//                keyword2.adGroupIDSpecified = true;
//                keyword2.sponsoredSearchMaxBid = Convert.ToDouble(getLocalizedString("KEYWORD2_SS_MAX_BID"));
//                keyword2.sponsoredSearchMaxBidSpecified = true;
//                keyword2.editorialStatus = EditorialStatus.Pending;
//                keyword2.editorialStatusSpecified = true;
//                keyword2.advancedMatchON = true;
//                keyword2.advancedMatchONSpecified = true;
//                keyword2.watchON = true;
//                keyword2.watchONSpecified = true;
//                keywords[1] = keyword2;
//                //third keyword
//                Keyword keyword3 = new Keyword();
//                keyword3.advancedMatchON = true;
//                keyword3.advancedMatchONSpecified = true;
//                keyword3.text = getLocalizedString("KEYWORD3_TEXT");
//                keyword3.status = KeywordStatus.On;
//                keyword3.statusSpecified = true;
//                keyword3.adGroupID = adGroupID;
//                keyword3.adGroupIDSpecified = true;
//                keyword3.sponsoredSearchMaxBid = Convert.ToDouble(getLocalizedString("KEYWORD3_SS_MAX_BID"));
//                keyword3.sponsoredSearchMaxBidSpecified = true;
//                keyword3.editorialStatus = EditorialStatus.Pending;
//                keyword3.editorialStatusSpecified = true;
//                keyword3.advancedMatchON = true;
//                keyword3.advancedMatchONSpecified = true;
//                keyword3.watchON = true;
//                keyword3.watchONSpecified = true;
//                keywords[2] = keyword3;
//                //fourth keyword
//                Keyword keyword4 = new Keyword();
//                keyword4.advancedMatchON = true;
//                keyword4.advancedMatchONSpecified = true;
//                keyword4.text = getLocalizedString("KEYWORD4_TEXT");
//                keyword4.status = KeywordStatus.On;
//                keyword4.statusSpecified = true;
//                keyword4.adGroupID = adGroupID;
//                keyword4.adGroupIDSpecified = true;
//                keyword4.sponsoredSearchMaxBid = Convert.ToDouble(getLocalizedString("KEYWORD4_SS_MAX_BID"));
//                keyword4.sponsoredSearchMaxBidSpecified = true;
//                keyword4.editorialStatus = EditorialStatus.Pending;
//                keyword4.editorialStatusSpecified = true;
//                keyword4.advancedMatchON = true;
//                keyword4.advancedMatchONSpecified = true;
//                keyword4.watchON = true;
//                keyword4.watchONSpecified = true;
//                keywords[3] = keyword4;
//                return keywords;
//            }
//            catch (Exception e)
//            {
//                debug(e.Message);
//                throw new Exception(e.Message);
//            }
//        }
//        /**
//         * Adds multiple keywords to an ad group.
//         *
//         * @param keywords
//         * @return Keywords that were added, including the assigned Keyword IDs.
//         * @throws RemoteException
//         */
//        private Keyword[] addKeywords(Keyword[] keywords)
//        {
//            try
//            {
//                debug("adding keywords...");
//                KeywordResponse[] keywordResponses = _keywordService.addKeywords(keywords);
//                printHeaders(_keywordService.remainingQuotaValue.Text[0], _keywordService.commandGroupValue.Text[0], _keywordService.timeTakenMillisValue.Text[0]);
//                ArrayList addedKeywordsList = new ArrayList();
//                for (int i = 0; i < keywordResponses.Length; i++)
//                {
//                    KeywordResponse keywordResponse = keywordResponses[i];
//                    if ((bool)keywordResponse.operationSucceeded)
//                    {
//                        addedKeywordsList.Add(keywordResponse.keyword);
//                    }
//                    else
//                    {
//                        debug("Error adding keyword index " + i + " in list...");
//                        printErrors(keywordResponse.errors);
//                        //print rejection reasons if any of the ads were rejected already
//                        printRejectionReasonsForKeyword(keywordResponse.editorialReasons, i);
//                    }
//                }
//                Keyword[] addedKeywords = new Keyword[addedKeywordsList.Count];
//                addedKeywords = (Keyword[])addedKeywordsList.ToArray(typeof(Keyword));
//                //returns the succesfully added keywords
//                return addedKeywords;


//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//            return keywords;
//        }
//        /**
//        * Creates three Excluded Word data objects.
//        *
//        * @param adGroupID
//        * @return Array of created Excluded Words
//        */
//        private ExcludedWord[] createExcludedWordsForAdGroup(long adGroupID)
//        {
//            try
//            {
//                ExcludedWord[] excludedWords = new ExcludedWord[3];
//                //first keyword to exclude
//                ExcludedWord excludedWord1 = new ExcludedWord();
//                excludedWord1.adGroupID = adGroupID;
//                excludedWord1.adGroupIDSpecified = true;
//                excludedWord1.text = getLocalizedString("EXCLUDEDWORD1_TEXT");
//                excludedWords[0] = excludedWord1;
//                //second keyword to exclude
//                ExcludedWord excludedWord2 = new ExcludedWord();
//                excludedWord2.adGroupID = adGroupID;
//                excludedWord2.adGroupIDSpecified = true;
//                excludedWord2.text = getLocalizedString("EXCLUDEDWORD2_TEXT");
//                excludedWords[1] = excludedWord2;
//                //third keyword to exclude
//                ExcludedWord excludedWord3 = new ExcludedWord();
//                excludedWord3.adGroupID = adGroupID;
//                excludedWord3.adGroupIDSpecified = true;
//                excludedWord3.text = getLocalizedString("EXCLUDEDWORD3_TEXT");
//                excludedWords[2] = excludedWord3;
//                return excludedWords;
//            }
//            catch (Exception e)
//            {
//                debug(e.Message);
//                throw new Exception(e.Message);
//            }
//        }

//        /**
//        * Adds multiple excluded words to an ad Group.
//        *
//        * @param excludedWords
//        * @return Excluded Words that were added, including the assigned Excluded Word IDs.
//        * @throws RemoteException
//        */
//        private ExcludedWord[] addExcludedWordsToAdGroup(ExcludedWord[] excludedWords)
//        {
//            try
//            {
//                debug("adding excluded keywords...");
//                ExcludedWordResponse[] excludedWordResponses = _excludedWordsService.addExcludedWordsToAdGroup(excludedWords);
//                printHeaders(_excludedWordsService.remainingQuotaValue.Text[0], _excludedWordsService.commandGroupValue.Text[0], _excludedWordsService.timeTakenMillisValue.Text[0]);
//                ArrayList addedExcludedWordsList = new ArrayList();
//                for (int i = 0; i < excludedWordResponses.Length; i++)
//                {
//                    ExcludedWordResponse excludedWordResponse = excludedWordResponses[i];
//                    if ((bool)excludedWordResponse.operationSucceeded)
//                    {
//                        addedExcludedWordsList.Add(excludedWordResponse.excludedWord);
//                    }
//                    else
//                    {
//                        debug("Error adding keyword " + i + " in list...");
//                        printErrors(excludedWordResponse.errors);
//                    }
//                }
//                ExcludedWord[] addedExcludedWords = new ExcludedWord[addedExcludedWordsList.Count];
//                addedExcludedWords = (ExcludedWord[])addedExcludedWordsList.ToArray(typeof(ExcludedWord));
//                return addedExcludedWords;

//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//            return excludedWords;
//        }
//        /**
//         * Called at initialization time to load previously
//         * cached location entries.  File contents are in the typical Properties file format:
//         * <masterAccountID> = <location>
//         *
//         * @throws IOException
//         */
//        private void loadLocationCache()
//        {
//            String line;
//            StreamReader sr = null;
//            try
//            {
//                String filename = getLocationCacheFileName();
//                if (File.Exists(filename))
//                {
//                    sr = new StreamReader(new FileStream(filename, FileMode.Open));
//                    while ((line = sr.ReadLine()) != null)
//                    {
//                        if (line.StartsWith("#")) continue;
//                        if (line.Trim().Length == 0) continue;
//                        int delimIndex = line.IndexOf("=");
//                        if (delimIndex != -1)
//                        {
//                            string masterAccountID = line.Substring(0, delimIndex).Trim();
//                            string location = line.Substring(delimIndex + 1).Trim();
//                            debug("read masterAccountID " + masterAccountID + " from file.");
//                            debug("read location " + location + " from file.");
//                            _locationCache.Add(masterAccountID, location);
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                debug("Exception: " + e.Message);
//            }
//            finally
//            {
//                if (sr != null) sr.Close();
//                debug("Executing finally block.");
//            }
//        }
//        /**
//         * Simple algorithm to generate a unique cache file name unique to
//         * store location information gotten from the specified LocationService
//         *
//         * @return filename of the persisted Location Service cache
//         */
//        private string getLocationCacheFileName()
//        {
//            if (_locationCacheFilename == null)
//            {
//                _locationCacheFilename = "ews_cache_";
//                string locationServiceAddress = EWS_LOCATION_SERVICE_ENDPOINT;
//                StringBuilder suffix = new StringBuilder();
//                for (int i = 0; i < locationServiceAddress.Length; i++)
//                {
//                    char c = Convert.ToChar(locationServiceAddress.Substring(i, 1));
//                    if (c >= 'A' && c <= 'z')
//                    {
//                        suffix.Append(c);
//                    }
//                }
//                _locationCacheFilename = _locationCacheFilename + suffix.ToString();
//            }
//            debug("locationCacheFilename: " + _locationCacheFilename);
//            return _locationCacheFilename;
//        }
//        /**
//         * This address can be found in the LocationService wsdl
//         *
//         * @return Endpoint for LocationService call
//         */
//        private string getLocationServiceAddress()
//        {
//            //send LocationService request to this address prefix
//            return EWS_ACCESS_HTTP_PROTOCOL + "://" + EWS_LOCATION_SERVICE_ENDPOINT + "/services";
//        }
//        /**
//         * Retrieves the service endpoint address prefix for the specific masterAccountID using the
//         * EWS LocationService.
//         * The steps to obtain a location are:
//         * i)  If location is present in the locationCache, return it.
//         * ii) If not present in locationCache, call LocationService to
//         * fetch the location. Store the location in the cache file
//         * and locationCache before returning it.
//         *
//         * @param masterAccountID masterAccountID for which location is sought.
//         * @return endpoint to use for all services except for LocationService
//         * @throws Exception
//         */
//        private string getEndPointFromLocationService(string masterAccountID)
//        {
//            string endPointLocation = null;
//            try
//            {
//                endPointLocation = (string)_locationCache[masterAccountID];
//                if (endPointLocation == null)
//                {
//                    _locationService.Url = (string)(getLocationServiceAddress() + "/V4/LocationService");
//                    _locationService.masterAccountIDValue = _masterAccountID;
//                    _locationService.SecurityValue = _securityValue;
//                    _locationService.licenseValue = _license;
//                    //use LocationService to get the address prefix for the rest of the services
//                    endPointLocation = _locationService.getMasterAccountLocation();
//                    debug("endPointPrefix is " + endPointLocation);
//                    //persist location address for the master account
//                    persistAccountLocationCache(masterAccountID, endPointLocation);
//                }
//            }
//            catch (Exception ex)
//            {
//                debug("Exeption: " + ex.Message);
//            }
//            return endPointLocation;
//        }
//        /**
//        * Persist location gotten from LocationService in memory cache and on disk for future use
//        *
//        * @param masterAccountID
//        * @param serviceLocationAddress
//        * @throws IOException
//        */
//        private void persistAccountLocationCache(string masterAccountID, string serviceLocationAddress)
//        {
//            try
//            {
//                //store it in the internal data structure
//                _locationCache.Add(masterAccountID, serviceLocationAddress);
//                //store it in a persistent store (local file)
//                String filename = getLocationCacheFileName();
//                StreamWriter writer = new StreamWriter(filename);
//                //write master account ID and serviceLocationAddress for the master account into the file
//                writer.Write("\n" + masterAccountID + " = " + serviceLocationAddress);
//                writer.Flush();
//                writer.Close();
//                debug("Successfully wrote masterAccountID and serviceLocationAddress " + serviceLocationAddress + " to file " + filename);
//            }
//            catch (IOException ex)
//            {
//                debug("IOException: " + ex.Message);
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//        }
//        /**
//        * Print debug message
//        *
//        * @param msg
//        */
//        private void debug(string msg)
//        {
//            if (DEBUG) Console.WriteLine((DateTime.Now - _starttime) + " [debug] " + msg);
//        }
//        /**
//        * Prints SOAP response header
//        */
//        private void printHeaders(string remainingQuota, string commandGroup, string timeTakenMillis)
//        {
//            debug("remaingQuota: " + remainingQuota);
//            debug("commandGroup: " + commandGroup);
//            debug("timeTakenMillis: " + timeTakenMillis);
//        }

//        /*
//         *  Print the errors
//         */
//        private void printErrors(Error[] errors)
//        {
//            if (errors == null || errors.Length == 0)
//            {
//                debug("Error list is empty");
//                return;
//            }

//            for (int i = 0; i < errors.Length; i++)
//            {
//                Error error = errors[i];
//                debug("ERROR " + error.code + ": " + error.message);
//            }
//        }

//        /**
//        * Prints the reasons when an ad is rejected.
//        */
//        private void printRejectionReasonsForAd(AdEditorialReasons rejectionReasons, int index)
//        {
//            try
//            {
//                if (rejectionReasons != null)
//                {
//                    debug("Rejection reasons for ad index " + index + ":");
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.adEditorialReasons);
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.descriptionEditorialReasons);
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.displayUrlEditorialReasons);
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.shortDescriptionEditorialReason);
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.titleEditorialReasons);
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.urlContentEditorialReasons);
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.urlEditorialReasons);
//                    printReasonsFromCodes(AD_TYPE, rejectionReasons.urlStringEditorialReasons);
//                }
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//        }

//        /**
//        * Prints the reasons when a keyword is rejected.
//        */
//        private void printRejectionReasonsForKeyword(KeywordEditorialReasons rejectionReasons, int index)
//        {
//            try
//            {
//                if (rejectionReasons != null)
//                {
//                    debug("Rejection reasons for keyword index " + index + ":");
//                    printReasonsFromCodes(KEYWORD_TYPE, rejectionReasons.keywordEditorialReasons);
//                    printReasonsFromCodes(KEYWORD_TYPE, rejectionReasons.alternateTextEditorialReasons);
//                    printReasonsFromCodes(KEYWORD_TYPE, rejectionReasons.phraseSearchTextEditorialReasons);
//                    printReasonsFromCodes(KEYWORD_TYPE, rejectionReasons.textEditorialReasons);
//                    printReasonsFromCodes(KEYWORD_TYPE, rejectionReasons.urlContentEditorialReasons);
//                    printReasonsFromCodes(KEYWORD_TYPE, rejectionReasons.urlEditorialReasons);
//                    printReasonsFromCodes(KEYWORD_TYPE, rejectionReasons.urlStringEditorialReasons);
//                }
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//        }

//        /**
//         *  Prints the text reasons from the rejection code.
//         */
//        private void printReasonsFromCodes(int serviceType, int[] codes)
//        {
//            try
//            {
//                if (codes != null && codes.Length != 0)
//                {
//                    for (int i = 0; i < codes.Length; i++)
//                    {
//                        switch (serviceType)
//                        {
//                            case AD_TYPE:
//                                debug(_adService.getEditorialReasonText(codes[i], LOCALE));
//                                break;
//                            case KEYWORD_TYPE:
//                                debug(_keywordService.getEditorialReasonText(codes[i], LOCALE));
//                                break;
//                        }
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                debug("Exception: " + ex.Message);
//            }
//        }

//        /* Load market specific localized properties file 
//         * and store the localized value in HashTable
//         */

//        private void loadLocalizedPropertiesFile()
//        {
//            string line;
//            StreamReader sr = null;
//            try
//            {
//                string filename = "sample_data_" + MARKET + ".properties";
//                if (File.Exists(filename))
//                {
//                    sr = new StreamReader(new FileStream(filename, FileMode.Open), Charset);
//                    while ((line = sr.ReadLine()) != null)
//                    {
//                        if (line.StartsWith("#")) continue;
//                        if (line.Trim().Length == 0) continue;
//                        int delimIndex = line.IndexOf("=");
//                        if (delimIndex != -1)
//                        {
//                            string key = line.Substring(0, delimIndex).Trim();
//                            string localizedString = line.Substring(delimIndex + 1).Trim();
//                            _localizedHash.Add(key, localizedString);
//                        }
//                    }
//                }
//                else
//                {
//                    debug("Sample data file " + filename + " does not exist.");
//                }
//            }
//            catch (Exception e)
//            {
//                debug("Exception: " + e.Message);
//            }
//            finally
//            {
//                if (sr != null) sr.Close();
//                debug("Executing finally block.");
//            }
//        }

//        /**
//        * Get localized string value
//        *
//        * @param key
//        */
//        private string getLocalizedString(string key)
//        {
//            string localizedString = null;
//            if (_localizedHash.ContainsKey(key))
//            {
//                localizedString = (string)_localizedHash[key];
//                debug("returning localized String: " + localizedString + " for key " + key);
//            }
//            else
//            {
//                throw new Exception("Localized string for key " + key + " not found. ");
//            }
//            return localizedString;
//        }
//    }
//}


