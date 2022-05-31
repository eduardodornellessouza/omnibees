using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OB.Reservation.BL
{
    public class Constants
    {
        public Constants()
        {
        }

        public static long BookingEngineChannelId = 1;

        public static string OmnibeesConnectionString = "OmnibeesConnectionString";
        public static string ReservationsConnectionString = "ReservationsConnectionString";

        /// <summary>
        /// Reservation Action
        /// </summary>
        public enum ReservationAction
        {
            Insert = 1,
            Update = 2,
            Modify = 3,
            Cancel = 4
        }

        #region RESERVATION TRANSACTIONS

        /// <summary>
        /// Reservation Transaction Type
        /// </summary>
        public enum ReservationTransactionType
        {
            A = 1,
            B = 2
        }

        /// <summary>
        /// Reservation Transaction Action
        /// </summary>
        public enum ReservationTransactionAction
        {
            Initiate = 1,
            Modify = 2,
            Commit = 3,
            Ignore = 4
        }

        /// <summary>
        /// Reservation Transaction Status
        /// </summary>
        public enum ReservationTransactionStatus
        {
            None = 0,
            Pending = 1,
            Commited = 2,
            Ignored = 3,
            Cancelled = 4,
            CommitedOnRequest = 5,
            CancelledOnRequest = 6,
            RefusedOnRequest = 7,
            OnRequestAccepted = 8
        }

        #endregion

        /// <summary>
        /// Business Rules Group Types
        /// </summary>
        public enum RuleType
        {
            Push = 0,
            GDS = 1,
            Pull = 2,
            PMS = 3,
            BE = 4,
            Omnibees = 5,
            PortalOperadoras = 6,
            BEAPI = 7
        }

        [Flags]
        public enum BusinessRuleFlags
        {
            /// <summary>
            /// Represents all rules
            /// </summary>
            All = 0,

            // Validations
            ValidateCancelationCosts = 1 << 0,
            ValidateGuarantee = 1 << 1,
            ValidateRestrictions = 1 << 2,
            ValidateAllotment = 1 << 4,
            ValidateBookingWindow = 1 << 30,
            ValidateReservation = 1 << 31,
            ValidateRateChannelPartialPayments = 1 << 32,

            // Behavior
            HandleCancelationPolicy = 1 << 5,
            ForceDefaultCancellationPolicy = 1 << 6,
            HandleDepositPolicy = 1 << 7,
            HandlePaymentGateway = 1 << 8,
            UseReservationTransactions = 1 << 9,
            ValidateDepositCosts = 1 << 10,
            LoyaltyDiscount = 1 << 11,
            CalculatePriceModel = 1 << 12,
            GenerateReservationNumber = 1 << 13,
            EncryptCreditCard = 1 << 14,
            ConvertValuesToPropertyCurrency = 1 << 15,
            PullTpiReservationCalculation = 1 << 16,
            ReturnSellingPrices = 1 << 17,
            ApplyNewReservationFilter = 1 << 18,
            ConvertValuesToRateCurrency = 1 << 19,
            BEReservationCalculation = 1 << 20,
            GDSBuyerGroup = 1 << 21,
            PriceCalculationAbsoluteTolerance = 1 << 22,
            CalculateExtraBedPrice = 1 << 23,
            IsToPreCheckAvailability = 1 << 24,
            IgnoreAvailability = 1 << 25,
            /// <summary>
            /// Ignores the concatenation of DepositPolicy.Name with DepositPolicy.Description.
            /// </summary>
            IgnoreDepositPolicyConcat = 1 << 26,
            /// <summary>
            /// Ignores the validations of PaymentMethodTypes. If this flag is set than all payments are accepted.
            /// </summary>
            IgnorePaymentMethodTypeValidations = 1 << 27,
            ConvertValuesFromClientToRates = 1 << 28,
            CalculateStayWindowWeekDays = 1 << 29
        }

        /// <summary>
        /// Dsc :: Enum to identify the reservation status
        /// </summary>
        public enum ReservationStatus
        {
            Booked = 1,
            Cancelled = 2,
            Pending = 3,
            Modified = 4,
            BookingOnRequest = 5,
            RefusedOnRequest = 6,
            CancelledOnRequest = 7,
            CancelledPending = 8,
            OnRequestAccepted = 9,
            OnRequestChannelCancel = 10
        }

        /// <summary>
        /// Extra or Tax BillingType_UID
        /// </summary>
        public enum BillingType
        {
            FixedAmount = 1,
            PerPerson = 2,
            PerRoom = 3,
            PerNight = 4,
            PerStay = 5,
            Percentage = 6,
            PerItem = 7,
            PerReservationPercentage = 8
        }

        /// <summary>
        /// Approval Billing for Billed Payment Type Method
        /// </summary>
        public enum ApprovalBilling
        {
            ApprovedOnlyDaily = 1,
            ApprovedDailyAndExtras = 2,
            Unapproved = 3,
            InAnalysis = 4,
            Blocked = 5
        }

        public enum ChannelStatu
        {
            Active = 0,
            InActive = 1,
            Pending = 2
        }

        public enum ExternalSystemsCodes
        {
            Opera = 1,
            Perot = 2
        }

        /// <summary>
        /// Create User Status
        /// </summary>
        public enum CreateUserStatus
        {
            Success = 0,
            RepeatedUsername = 1,
            RepeatedEmail = 2
        }

        public enum UserStatus
        {
            LoggedIn = 1,
            Islocked = 2,
            PasswordExpire = 3,
            InActive = 4,
            DummyUser = 5,
            TwoFactorRequired = 6,
            TwoFactorFailed = 7,
            RecoverPassword = 8
        }

        public enum RoleCategory
        {
            Users = 1,
            AccountManagers = 2
        }

        public enum RoleType
        {
            Users = 1,
            AccountManagers = 2,
            Admin = 3,
            BillingUser = 4
        }

        public enum ReviewStatus
        {
            Publish = 1,
            UnPublish = 2
        }

        /// <summary>
        /// Created By :: Ronak Shah
        /// Created Date :: 16 Dec 2010
        /// Desc :: Enum for System Events Conditions
        /// </summary>
        public enum SystemEvents
        {
            NewBooking = 1,
            BookingChanged = 2,
            PreStay = 3,
            PostStay = 4,
            BookingCancelled = 5,
            NewAttraction = 6,
            NewCampaign = 7,
            NewGuestClient = 8,
            NewIDSChannel = 10,
            NewExtra = 11,
            GeneralPolicyChanged = 12,
            NewPackage = 13,
            NewHotelProperty = 14,
            NewRate = 15,
            RateChanged = 16,
            NewReview = 17,
            NewSurvey = 18,
            SurveyEnded = 19,
            NewVideo = 20,
            NewPropertyPhoto = 21,

            //NewRoomPhoto = 30,
            Availability = 23,

            DayRate = 24,
            IDSChannelNotUpdating = 25,
            IDSChannelUpdated = 26,
            TweetWithKeyword = 28,
            HotelClientBirthday = 29,
            GuestBirthday = 30,
            OnRequestBooking = 31,
            ExtraNotificationEmail = 32,
            RefusedOnRequestBooking = 33,
            CancelOnRequest = 41
        }

        /// <summary>
        /// Enum for Marketing List Category
        /// </summary>
        public enum MarketingListErrorCategory
        {
            NonExistingAddress = 1,
            Blocked = 2,
            MailBoxFull = 3,
            VacationAutoReply = 4,
            Undeliverables = 5,
            Others = 6
        }

        /// <summary>
        /// Calendar eventCategory Color Code
        /// </summary>
        public enum EventColorCode
        {
            Gold = 1,
            Silver = 2,
            Bronze = 3,
            Red = 4,
            Blue = 5,
            Green = 6,
            Pink = 7,
            Orange = 8,
            Black = 9,
            White = 10
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 30 Dec 2010
        /// Desc :: To Set ChangeLogEntityID
        /// </summary>
        public enum ChangeLogEntityID
        {
            Property = 1,
            Activity = 2,
            Amenities = 3,
            Attractions = 4,
            CalenderEvents = 5,
            Campaigns = 6,
            Guests = 7,
            Users = 8,
            MarketingLists = 9,
            ThirdPartyIntermediaries = 10,
            Rates = 11,
            Reservations = 12,
            Surveys = 13,
            UpdateRatesAvailability = 14,
            UsersLoginHistory = 15,
            TourOperatorsSetCommission = 18,
            RoomTypes = 19,
            OccupancyLevelsUserChange = 20,
            OccupancyLevelsSystemUpdates = 21,
            Inventory = 22,
        }

        public enum LogTransactionType
        {
            LogTransaction = 1,
            LogTransactionDetails = 2,
            LogTransactionDetailsByPaypalAutoGeneratedId = 3
        }

        /// <summary>
        /// Created By :: Ronak Shah
        /// Created Date :: 16 Dec 2010
        /// Desc :: Class for Hotel Property Status
        /// </summary>
        public class HotelStatusReport
        {
            public bool HasExtra { get; set; }

            public bool HasAttraction { get; set; }

            public bool HasRoomType { get; set; }

            public bool HasDepositPolicy { get; set; }

            public bool HasCancellationPolicy { get; set; }

            public bool HasOtherPolicy { get; set; }

            public bool HasTaxPolicy { get; set; }

            public bool HasRate { get; set; }
        }

        /// <summary>
        /// Created By :: Tabrez Shaikh
        /// Created Date :: 28 Jan 2011
        /// Desc :: To identify system action which is define in SystemActions table in Database.
        /// </summary>
        public enum SystemAction
        {
            SendMail = 1,
            SendTweet = 2,
            SendFaceBook = 6
        }

        /// <summary>
        /// Set BEThemeCode
        /// </summary>
        public enum BEThemeCode
        {
            Blue = 1,
            Yellow = 2,
            Green = 3,
            Red = 4,
            Customize = 5
        }

        /// <summary>
        /// BE Landing pages type - US 2546
        /// </summary>
        public enum BELandingPageType
        {
            Room = 1,
            Rate = 2,
            Package = 3,
            Extra = 4,
            GroupCode = 5,
            PromotionalCode = 6
        }

        public enum SystemEventsCode
        {
            NewBookingArrived = 1,
            BookingChanged = 2,
            PreStay = 3,
            PostStay = 4,
            Bookingcancelled = 5,
            NewAttractionAdded = 6,
            NewCampaignCreated = 7,
            NewPackageCreated = 12,
            NewRateCreated = 14,
            NewReviewAdded = 16,
            NewSurveyCreated = 17,
            Availability = 22,
            GuestBirthday = 23,
            NewReview = 24,
            NextCalendarEvents = 25,
            PromotionCodes = 26,
            Package = 29,
            Survey = 30,
            ReservationSumary = 31,
            GuestInformation = 32,
            RoomRatePolicies = 33,
            HotelPolicies = 34,
            ReservationSummaryHeadar = 35,
            ReservationSummaryFooter = 36,
            GuestInformationHeadar = 37,
            RoomRatePoliciesHeadar = 38,
            HotelPoliciesHeadar = 39,
            RoomExtraHeadar = 40,
            RoomExtra = 41,
            RoomExtraFootar = 42,
            NextCalendarEventsHeadar = 43,
            PromotionCodesHeadar = 44,
            PackageHeadar = 45,
            AttractionHeadar = 46,
            Attraction = 47,
            ActivityHeadar = 48,
            Activity = 49,

            //ForgotPassword = 50,
            TravelDirections = 51,

            PostStayIntroductoryText = 56,
            PreStayIntroductoryText = 57,
            NewBookingIntroductoryText = 58,
            BookingCancelledIntroductoryText = 59,
            BookingChangedIntroductoryText = 60,
            BillingAddres = 61,
            OnRequestBooking = 62,
            ExtraNotificationEmail = 63,
            HotelInfo = 70,
            RefusedOnRequestBooking = 64,
            CancelOnRequest = 65,
            NewGuest = 1000,
            MailtoFriendForRoomDetail = 1001,
            MailtoFriendForRateDetail = 1002,
            MailtoUserforReservationDetail = 1003,
            MailtoUserForgotPassword = 1004,
            MailReservationDetailsToFriends = 1005,
            MailOnRequestBookingGuest = 1006,
            MailOnRequestBookingHotel = 1007,
            MailForgotPasswordOmnibees = 1008,
            RateUpdate = 66,
            ChannelActivityErrors = 67,
            CreditLimit = 68
        }

        /// <summary>
        /// Created By :: Tabrez Shaikh
        /// Created Date :: 28 Jan 2011
        /// Desc :: To identify condition which is define in PropertyEventConditions table in Database for room availability event.
        /// </summary>
        public enum RoomQtyEventCondition
        {
            LessThan = 1,
            LessThanEqualTo = 2,
            GreaterThan = 3,
            GreaterThanEqualTo = 4,
            EqualTo = 5
        }

        /// <summary>
        /// Created By :: Ronak Shah
        /// Created Date :: 19 Nov 2010
        /// Desc :: To set Rate Restrictions Types
        /// </summary>
        public enum RateRestrictionTypes
        {
            MinDays = 1,
            StayThrough = 2,
            ClosedOnArr = 3,
            ClosedOnDep = 4,
            ReleaseDays = 5,
            Allocation = 6,
            Blocked = 7,
            MaxDays = 8
        }

        /// <summary>
        /// Created By :: Tabrez Shaikh
        /// Created Date :: 29 Dec 2010
        /// Desc :: To identify status of sales document.
        /// </summary>
        public enum SalesDocStatus
        {
            Paid = 1,
            Unpaid = 2
        }

        /// <summary>
        /// Created By :: Ronak Shah
        /// Created Date :: 04 Jan 2011
        /// Desc :: To identify Property Template Category
        /// </summary>
        public enum PropertyTemplateCategory
        {
            Reservation = 1,
            Campaign = 2,
            NewUser = 3,
            CancelReservation = 4
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 10 Jan 2011
        /// Desc :: List Of Languages
        /// </summary>
        public enum Languages
        {
            English = 1,
            French = 2,
            Spanish = 3,
            Portugal = 4,
            Denmark = 5,
            Italy = 6,
            Germany = 7,
            PortugalBrazil = 8
        }

        /// <summary>
        /// List of Themes
        /// </summary>
        public enum Themes
        {
            Orange = 2,
            Yellow = 3,
            Green = 4
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 15 Feb 2012
        /// Desc :: To get the ReservationOption
        /// </summary>
        public enum Reservationoption
        {
            AllReservation = 1,
            Bookingengine = 2,
            Channels = 3
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 01 May 2011
        /// Desc :: List of BEStyle
        /// </summary>
        public enum BEStyle
        {
            Header = 1,
            Subtitle = 2,
            Text = 3,
            TermsConditions = 4,
            BackgroungImage = 5,
            Banner = 6,
            BackgroungColor = 7,
            BackgroungFramColor = 8,
            DefaultRate = 9,
            MaxRooms = 10,
            MaxDefaultVisibleRates = 1,
            Logo = 11,
            BEPropertyLogo = 12,
            BEMobileMainPhoto = 13
        }

        /// <summary>
        /// List of MailType
        /// </summary>
        public enum MailType
        {
            ReservationEmail = 1,
            CampaignEmail = 2,
            SurveyEmail = 3,
            GeneralEmail = 4
        }

        /// <summary>
        /// Created By :: Hitesh Patel
        /// Created Date :: 18 Oct 2011
        /// Desc :: Enum to be used in Campaign's Send Tab in CRM.
        /// </summary>

        public enum CampaignSendOptions
        {
            ConfigureSendOptionsLater = 1,
            SendNow = 2,
            ScheduleSend = 3
        }

        /// <summary>
        /// Send State of campaign
        /// </summary>
        public enum CampaignSendState
        {
            ScheduleSend = 1,
            Sent = 2,
            Error = 3,
            Preparing = 4,
            Draft = 5,
            UnderReview = 6
        }

        public enum CampaignTemplateType
        {
            LoadTemplate = 1,
            SystemTemplate = 2,
            NewTemplate = 3,
            ImportFromFile = 4,
            ImportFromURL = 5
        }

        /// <summary>
        /// ConnectorMessageStatus
        /// </summary>
        public enum ConnectorMessageStatus
        {
            //Notprocessed = 0,
            //Fail = 1,
            //Processed = 2,
            //Expired = 3,
            //Generatednotsent = 4,
            //Skipped = 5,
            //SuccesswithWarnings = 6,
            //Internal = 10
            Notprocessed = 0,

            Fail = 1,
            Success = 2,
            Expired = 3,
            Pending = 4,
            Skipped = 5,
            SuccesswithWarnings = 6,
            Internal = 10
        }

        public enum SettingGroup
        {
            Private,
            Component,
            Public
        }

        public enum SettingCategory
        {
            All,
            Omnibees
        }

        //Variables to be used for subject line while sending new Campaign mail.
        public static string NewCampaignMailSubject = "New Campaign";

        public static string NewCampaignFromName = "Protur";
        public static string NewCampaignUnSubscribe = "<br/><a href='http://www.google.com'>Click Here</a> to Unsubscribe from this notification.";

        //Variables to be used for subject line while sending new Guest Deatil
        public static string NewReservationMailSubject = "Reservation Detail";

        public static string NewGuestDetailMailSubject = "Guest Detail";

        //CancelReservation
        public static string CancelReservationSubject = "Cancel Reservation";

        public static string ExtraSceduledSubject = "Extras Scheduled";
        public static string RoomTypeDetailSubject = "Room Details";
        public static string RateDetailSubject = "Rate Details";
        public static string ForgotPasswordSubject = "Omnibees - Password Recovery";

        public static Dictionary<string, string> dicForgotPasswordSubject = new Dictionary<string, string>() { { "pt-PT", "Omnibees - Recuperação de Password" }, { "en-US", "Omnibees - Password Recovery" }, { "fr-FR", "Omnibees - Mot-clé de récupération" }, { "es-ES", "Omnibees - Recuperación de la contraseña" }, { "it-IT", "Omnibees - Parola chiave di recupero" }, { "de-DE", "Omnibees - Wieder Stichwort" }, { "pt-BR", "Omnibees - Recuperação de Password" } };

        public static string Date = "&lt;Date&gt;";
        public static string Name = "&lt;Name&gt;";
        public static string ReservationNo = "&lt;ReservationNo&gt;";
        public static string UserName = "&lt;UserName&gt;";
        public static string Password = "&lt;Password&gt;";
        public static string ExtrasName = "&lt;ExtrasName&gt;";
        public static string Day = "&lt;Day&gt;";
        public static string RoomNo = "&lt;RoomNo&gt;";
        public static string FilePath = "~/MailTemplates/";
        public static string DateFormate = "dd MMMM,yyyy";
        public static string DateFormatWithDayOfWeek = "dddd, dd MMMM yyyy";
        public static string WoeID = "Woeid";
        public static string WoeidPassword = "****";
        public static string Twitter = "Twitter";
        public static string Facebook = "Facebook";
        public static int ImageWidth = 150;
        public static int ImageHeight = 100;
        public static string DateFormatForIPhone = "dd/MM/yyyy hh:mm tt";
        public static string DateDisplayForIPhone = "dd/MM/yyyy";

        public static string SalesDocumentsSerieFormate = "{0:000000}";

        //Buyer Group Code
        public const string BGEveryoneCode = "001";

        public const string BGAllTPIsCode = "002";
        public const string BGAllCompaniesCode = "003";
        public const string BGPublicCode = "004";

        /// <summary>
        /// Created By :: Ronak Shah
        /// Created Date :: 18 Apr 2011
        /// Updated By :: Angelo Cavaco at 20 July 2016
        /// Desc :: To identify credit card types
        /// </summary>
        public enum CCTypes
        {
            Visa = 1,
            MasterCard = 2,
            AmericanExpress = 3,
            Discover = 4,
            Hipercard = 5,
            Diners = 6,
            JCB = 7,
            Electron = 8,
            Eurocard = 9,
            enRouteCard = 10,
            Laser = 11,
            Maestro = 12,
            Solo = 13,
            PayPal = 14,
            Aura = 15,
            CreditCard_Liberate = 16,
            Elo = 17
        }

        /// <summary>
        /// Created By :: Angelo Cavaco
        /// Created Date :: 20 July 2016
        /// Desc :: To get Name for credit card types
        /// </summary>
        public static string GetCCTypeName(CCTypes type)
        {
            switch(type)
            {
                case CCTypes.Visa: return "Visa";  // 1
                case CCTypes.MasterCard: return "Master Card";  // 2
                case CCTypes.AmericanExpress: return "American Express";  // 3
                case CCTypes.Discover: return "Discover";  // 4
                case CCTypes.Hipercard: return "Hipercard";  // 5
                case CCTypes.Diners: return "Dinners Club";  // 6
                case CCTypes.JCB: return "JCB";  // 7
                case CCTypes.Electron: return "Electron";  // 8
                case CCTypes.Eurocard: return "Eurocard";  // 9
                case CCTypes.enRouteCard: return "enRouteCard";  // 10
                case CCTypes.Laser: return "Laser";  // 11
                case CCTypes.Maestro: return "Maestro";  // 12
                case CCTypes.Solo: return "Solo";  // 13
                case CCTypes.PayPal: return "PayPal";  // 14
                case CCTypes.Aura: return "Aura";  // 15
                case CCTypes.CreditCard_Liberate: return "Credit Card / Liberate";  // 16
                case CCTypes.Elo: return "Elo";  // 17
            }

            return "Unknow Credit Card Type";
        }

        /// <summary>
        ///  Enum to be useed in Iphone quary
        /// </summary>
        public enum ImageType
        {
            Property = 1,
            Extra = 2,
            Attraction = 3
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 19 Apr 2011
        /// Desc :: To identify BookingEngineUserType
        /// </summary>
        public enum BookingEngineUserType
        {
            Guest = 1,
            TravelAgent = 2,
            Corporate = 3,
            Employee = 4
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 08 sep 2011
        /// Desc :: To identify Marketing contect persone type
        /// </summary>
        public enum MarketinglistType
        {
            TPI = 0,
            Corporate = 1,
            Guest = 2
        }

        /// <summary>
        /// Created By :: Gaurang Pathak
        /// Created Date :: 20 Apr 2011
        /// Desc :: To identify System Template Category
        /// </summary>
        public enum SystemTemplatesCategory
        {
            NewReservation = 1,
            Campaign = 2,
            NewUser = 3,
            CancelReservation = 4,
            NewGuest = 5,
            Extrascheduled = 6,
            RoomType = 7,
            RateDetails = 8,
            BookingChangedEmail = 9,
            Campaign_Template2 = 49,
            Campaign_Template3 = 50,
            Campaign_Template4 = 51
        }

        /// <summary>
        /// Set TPICommissionStatus
        /// </summary>
        public enum TPICommissionStatus
        {
            Booked = 1,
            Cancelled = 2,
            Pending = 3
        }

        /// <summary>
        /// set SalesDocumentsTypes
        /// </summary>
        public enum SalesDocumentsTypes
        {
            Invoice = 1,
            CreditNotes = 2,
            Receipt = 3
        }

        /// <summary>
        /// Created By :: Tabrez Shaikh
        /// Created Date :: 20 Nov 2010
        /// Desc :: To get the Incentive Type
        /// </summary>
        public enum IncentiveType
        {
            [Display(Description = "Early Booking")]
            EarlyBooking = 1,
            [Display(Description = "Last Minute Booking")]
            LastMinuteBooking = 2,
            [Display(Description = "Free Nights")]
            FreeNights = 3,
            [Display(Description = "Discount")]
            Discount = 4,
            [Display(Description = "Stay Discount")]
            StayDiscount = 5
        }

        /// <summary>
        /// Created by :: Pathak Gaurang A
        /// Created Date :: 08/12/2011
        /// Desc :: to define action of service
        /// </summary>
        public enum ServiceAction
        {
            CampaignSchedule = 1,
            ScheduledTweet = 2,
            NewAttractionAdded = 3,
            NewCampaignCreated = 4,
            NewPackageCreated = 5,
            NewRateCreated = 6,
            NewReviewAdded = 7,
            NewSurveyCreated = 8,
            GuestBirthday = 9,
            NewBookingArrived = 10,
            PostStay = 11,
            PreStay = 12,
            SendTweet = 13,
            GenerateSalesDocumentsByService = 14,
            ReadCampaignFailureMails = 15,
            GetUnFollowerFromFollowerList = 16
        }

        public enum DaysOfWeek
        {
            Monday = 0,
            Tuesday = 1,
            Wednesday = 2,
            Thursday = 3,
            Friday = 4,
            Saturday = 5,
            Sunday = 6
        }

        public enum ImageUploadType
        {
            RoomType = 1,
            MediaCategory = 2,
            Package = 3,
            Attraction = 4,
            Extras = 5,
            Client = 6,
            PropertyLogo = 7,
            CalenderEvents = 8,
            BEPropertyLogo = 9,
            BEMobileMainPhoto = 10
        }

        /// <summary>
        /// Created By :: Hitesh Patel
        /// Created Date :: 04 May 2012
        /// Desc :: To identify System Templates by thier ID
        /// </summary>
        public enum SystemTemplatesIDs
        {
            //OnRequestBooking = 1,
            //HotelInfo = 2,
            NewBookingEmail = 81,

            NewBookingEmailHotel = 180,
            BookingChanged = 67,
            BookingCanceled = 68,
            NewGuestTemplate = 69,
            ForgotPassword = 164,
            RoomTypeTemplate = 165,
            RateDetailsTemplate = 166,
            ReservationSummaryHeadar = 78,
            ReservationSummaryFooter = 79,
            ReservationSumary = 75,
            GuestInformation = 76,
            RoomRatePolicies = 77,
            RoomRatePoliciesHeadar = 80,
            RoomExtraHeadar = 82,
            RoomExtra = 83,
            RoomExtraFootar = 84,
            BillingAddress = 179,
            TravelDirections = 171,
            HotelPoliciesHeadar = 172,
            HotelPolicies = 173,
            ExtraNotificationEmail = 181,
            CancelExtraNotificationEmail = 182,
            HotelInfo = 183,
            OnRequestBooking = 184,
            OnRequestRefuseBooking = 185,
            OnRequestBookingHotel = 186,
            RoomAvailability = 187
        }

        /// <summary>
        /// Created By :: Hitesh Patel
        /// Created Date :: 05 Septmber 2012
        /// Desc :: To identify System Templates by thier Code
        /// </summary>
        public enum SystemTemplatesCodes
        {
            NewBookingEmail = 81,
            PreStay = 88,
            PostStay = 59,
            NewBookingEmailHotel = 180,
            BookingChanged = 67,
            BookingChangedHotel = 197,
            BookingCanceled = 68,
            BookingCanceledHotel = 198,
            NewGuestTemplate = 69,
            ForgotPassword = 164,
            RoomTypeTemplate = 165,
            RateDetailsTemplate = 166,
            ReservationSummaryHeadar = 78,
            ReservationSummaryFooter = 79,
            ReservationSumary = 75,
            GuestInformation = 76,
            RoomRatePolicies = 77,
            RoomRatePoliciesHeadar = 80,
            RoomExtraHeadar = 82,
            RoomExtra = 83,
            RoomExtraFootar = 84,
            BillingAddress = 179,
            TravelDirections = 171,
            HotelPoliciesHeadar = 172,
            HotelPolicies = 173,
            ExtraNotificationEmail = 181,
            CancelExtraNotificationEmail = 182,
            HotelInfo = 184,
            OnRequestBooking = 183,
            OnRequestRefuseBooking = 186,
            OnRequestBookingHotel = 185,
            RoomAvailability = 187,
            CancelOnRequest = 189,
            ForgotPasswordOmnibees = 191,
            RateUpdate = 190,

            // campaigns
            Campaign_Template1 = 167,

            Campaign_Template2 = 168,
            Campaign_Template3 = 169,
            Campaign_Template4 = 170,
            Campaign_Template5 = 192,
            Campaign_Template6 = 193,
            Campaign_Template7 = 194,
            CreditLimit = 199
        }

        public static class CampaignGuestReplaceTokens
        {
            public static string Prefix = "_Prefix_";
            public static string FirstName = "_FirstName_";
            public static string LastName = "_LastName_";
            public static string Email = "_Email_";
            public static string FacebookUser = "_FBUser_";
            public static string TwitterUser = "_TwitterUser_";
        }

        public enum BETemplateTypeCodes
        {
            BookingEngineTemplate1 = 1,
            BookingEngineTemplate2 = 2,
            BookingEngineTemplate3 = 3
        }

        public enum RateCategories
        {
            General = 1,
            Promotional = 2,
            Package = 3,
            Group = 4,
            Corporate = 5,
            Weekend = 6,
            Fair = 7
        }

        /// <summary>
        /// Synchronize with PaymentMethodTypes in DB (code field)
        /// </summary>
        public enum PaymentMethodTypesCode
        {
            Other = 0, // not in db
            CreditCard = 1,
            DirectPaymentAtHotel = 2,
            BankDeposit = 3,
            Invoicing = 4,
            DailyBilled = 5,
            DailyBilledExtras = 6,
            PrePayment = 7,
            Paypal = 8
        }

        /// <summary>
        /// Synchronize with PaymentMethodTypes in DB (code field)
        /// </summary>
        public enum PaymentTypes
        {
            BE = 0, // not in db
            HoteisNet = 1,
            Both = 2
        }

        public enum PaymentGateway
        {
            MaxiPago = 100,
            Paypal = 200,
            BrasPag = 300,
            Adyen = 400,
            BPag = 500,
            PayU = 600
        }

        public enum PaymentGatewayAuthorizationTypes
        {
            Authorization = 1,
            AuthorizationWithCaptureDelay = 2,
            AuthorizationAndCapture = 3
        }

        /// <summary>
        /// Synchronize with ImageSizeCategories in DB (UID field)
        /// </summary>
        public enum ImageSizeCategories
        {
            LargerSizeImage = 1,
            BookingEngine = 2,
            BOImage = 4
        }

        /// <summary>
        /// Created By :: Jorge Guerreiro
        /// Created Date :: 31 January 2013
        /// Desc :: To define de Payment Model of Cancellation Policies
        /// </summary>
        public enum CancellationPoliciesPaymentModel
        {
            FixedValue = 1,
            Percentage = 2,
            Nights = 3
        }

        public enum SalesmanStatus
        {
            Active = 1,
            Inactive = 2,
            ActiveAndBlocked = 3,
            InactiveAndBlocked = 4
        }

        public static class Channels
        {
            public static string Sabre = "Sabre";
            public static string BookingEngine = "Booking Engine";
        }

        public enum ChannelType
        {
            Push = 0,
            GDS = 1,
            Pull = 2,
            Mixed = 3,
        }

        public enum BEMandatoryFields
        {
            BI = 1
        }

        public enum OperatorsType
        {
            None = 0,
            Operators = 1,
            OperatorsHoteisNet = 2
        }

        public enum Permissions
        {
            Read,
            Add,
            Update,
            Delete
        }

        public enum InventoryUpdateType
        {
            Allocated = 0,
            Used = 1,
            AllocatedAdjustment = 2
        }

        public enum TPIType
        {
            TravelAgent = 1,
            Company = 2,
            TravelAgencyOffice = 3,
            FilialCVC = 4
        }

        /// <summary>
        /// Enum for TripAdvisorReview
        /// </summary>
        public enum TripAdvisorReviewAction
        {
            Create = 1,
            Update = 2,
            Delete = 3
        }

        public enum GuaranteeType
        {
            CreditCard = 5,
            AgencyNameAdress = 18,
            CompanyNameAdress = 29,
            GuestNameAdress = 24,
            IATANumber = 19,
            CorporateID = 30
        }

        public enum BrasilStateCodes
        {
            AC = 3101859,
            AL = 5592975,
            AP = 3486536,
            AM = 6004326,
            BA = 7705856,
            CE = 2028838,
            ES = 6739233,
            GO = 4027598,
            MA = 5971644,
            MT = 6431043,
            MS = 6001439,
            MG = 888721,
            PA = 2734094,
            PB = 6897926,
            PR = 910322,
            PE = 921402,
            PI = 7481546,
            RJ = 5906543,
            RN = 2247291,
            RS = 7316529,
            RO = 1386303,
            RR = 4500268,
            SC = 587082,
            SP = 6362053,
            SE = 103928,
            TO = 2325281,
            DF = 6557263
        }

        public enum PO_KeeperType
        {
            TPI_PO = 1,
            Channel = 2,
            TPI_OB = 3,
            Representative = 7
        }

        public enum CreditCardVisualizationAction 
        {
            [Display(Description = "Unknow")]
            Unknow = 0,
            [Display(Description = "Authorized")]
            Authorized = 1,
            [Display(Description = "Invalid: User not found")]
            Invalid_UserNotFound = 2,
            [Display(Description = "Invalid: Reservation does not exists")]
            Invalid_ReservationNotFound = 3,
            [Display(Description = "Invalid: Does not have reservation access")]
            Invalid_ReservationAccessDenied = 4,
            [Display(Description = "Not allowed to see credit card on booking print")]
            Invalid_NotAllowedToPrint = 5,
            [Display(Description = "Invalid: Credit Card info not found")]
            Invalid_CreditCardInfoNotFound = 6,
            [Display(Description = "Get Credit Card info failed")]
            Invalid_GettingCardInfoFailed = 7
        }

        // Admin User on table Users
        public const long AdminUserUID = 65;
        public const string AdminUserName = "Admin";

        public enum RateModels
        {
            Commissionable = 1,
            Markup = 2,
            Margin = 4,
            Package = 5
        }

        public enum PayPalAction
        {
            [Display(Description = "UNKNOW")]
            UNKNOW = 0,
            [Display(Description = "CANCEL")]
            CANCEL = 1,
            [Display(Description = "SUSPEND")]
            SUSPEND = 2,
            [Display(Description = "REACTIVATE")]
            REACTIVATE = 3
        }

        public enum PaypalRefundType
        {
            [Display(Description = "OTHER")]
            OTHER = 0,
            [Display(Description = "FULL")]
            FULL = 1,
            [Display(Description = "PARTIAL")]
            PARTIAL = 2,
            [Display(Description = "EXTERNALDISPUTE")]
            EXTERNALDISPUTE = 3,
        }

    }
}