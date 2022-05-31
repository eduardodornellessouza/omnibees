using OB.Log;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common
{
    public static class Configuration
    {
        static ILogger _logger;
        static ILogger Logger => _logger ?? (_logger = LogsManager.CreateLogger("Configuration"));

        /// <summary>
        /// ES.API.Services endpoint
        /// </summary>

        private static string esAPIServiceEndpoint;
        public static string ESAPIServiceEndpoint
        {
            get
            {
                if (esAPIServiceEndpoint == null)
                    esAPIServiceEndpoint = ConfigurationManager.AppSettings["ES.API.Services.Endpoint"];

                return esAPIServiceEndpoint;
            }
        }
        

        /// <summary>
        /// OB.REST.Services endpoint
        /// </summary>

        private static string obRestServiceEndpoint;
        public static string OBRestServiceEndpoint
        {
            get
            {
                if (obRestServiceEndpoint == null)
                    obRestServiceEndpoint = ConfigurationManager.AppSettings["OB.REST.Services.Endpoint"];

                return obRestServiceEndpoint;
            }
        }


        /// <summary>
        /// OB.REST.Services endpoint
        /// </summary>

        private static string poRestServiceEndpoint;
        public static string PORestServiceEndpoint
        {
            get
            {
                if (poRestServiceEndpoint == null)
                    poRestServiceEndpoint = ConfigurationManager.AppSettings["PO.REST.Services.Endpoint"];

                return poRestServiceEndpoint;
            }
        }

        private static string _pullReservationErrorMargin;
        public static string PullReservationErrorMargin
        {
            get
            {
                if (_pullReservationErrorMargin == null)
                    _pullReservationErrorMargin = ConfigurationManager.AppSettings["PullReservationErrorMargin"];

                return _pullReservationErrorMargin;
            }
        }

        /// <summary>
        /// The default RefreshTime (TTL) for cached repositories.
        /// </summary>
        public static TimeSpan DefaultCacheRefresh => _defaultCacheRefreshMinutes ??
            (_defaultCacheRefreshMinutes = TimeSpan.FromMinutes(GetAndTryConvertTo("DefaultCacheRefresh_min", 1440))).Value; // 1 day by default
        static TimeSpan? _defaultCacheRefreshMinutes;

        /// <summary>
        /// The Application Name.
        /// </summary>
        static string _appName;
        public static string AppName => _appName ?? (_appName = GetAndLogMissingKey("AppName", "Reservations.Api"));

        static string _environment;
        public static string Environment => _environment ?? (_environment = GetAndLogMissingKey("Environment", ""));

        public static int CouchbaseVersion => _couchbaseVersion ?? (_couchbaseVersion = GetAndTryConvertTo<int>("CouchbaseVersion")).Value;
        static int? _couchbaseVersion;

        public static string CouchbaseUsername => _couchbaseUsername ?? (_couchbaseUsername = GetAndLogMissingKey("CouchbaseUsername", "userapp"));
        static string _couchbaseUsername;

        public static string CouchbasePassword => _couchbasePassword ?? (_couchbasePassword = GetAndLogMissingKey("CouchbasePassword", "123456"));
        static string _couchbasePassword;

        static bool? _enableNewOffers;
        public static bool EnableNewOffers => _enableNewOffers ?? (_enableNewOffers = GetAndTryConvertTo<bool>("EnableNewOffers")).Value;

        public static int CloseSalesRrdRetriesDelayMin_ms => _closeSalesRrdRetriesDelayMin_ms ?? (_closeSalesRrdRetriesDelayMin_ms = GetAndTryConvertTo("CloseSalesRrdRetriesDelayMin_ms", 500)).Value;
        static int? _closeSalesRrdRetriesDelayMin_ms;

        public static int CloseSalesRrdRetriesDelayMax_ms => _closeSalesRrdRetriesDelayMax_ms ?? (_closeSalesRrdRetriesDelayMax_ms = GetAndTryConvertTo("CloseSalesRrdRetriesDelayMax_ms", 1000)).Value;
        static int? _closeSalesRrdRetriesDelayMax_ms;

        public static int CloseSalesRetriesRrdCount => _closeSalesRetriesRrdCount ?? (_closeSalesRetriesRrdCount = GetAndTryConvertTo("CloseSalesRetriesRrdCount", 2)).Value;
        static int? _closeSalesRetriesRrdCount;

        static bool? _enableFeatureStates128077;
        public static bool EnableFeatureStates128077 => _enableFeatureStates128077 ?? (_enableFeatureStates128077 = GetAndTryConvertTo<bool>("EnableFeatureStates128077")).Value;

        #region Helpers

        static T GetAndTryConvertTo<T>(string key, T defaultValue = default(T))
        {
            string str = GetAndLogMissingKey(key, defaultValue?.ToString());

            if (!str.GenericTryParse(out T value))
            {
                Logger.Warn($"The '{key}' key has an invalid format in Web.config or App.config. The DefaultValue was applied.");
                return defaultValue;
            }

            return value;
        }

        static string GetAndLogMissingKey(string key, string defaultValue = null)
        {
            string value = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrEmpty(value))
            {
                Logger.Warn($"The '{key}' key is missing in Web.config or App.Config.");
                return defaultValue;
            }

            return value;
        }

        public static bool GenericTryParse<T>(this string input, out T value)
        {
            var converter = System.ComponentModel.TypeDescriptor.GetConverter(typeof(T));

            if (converter != null && converter.IsValid(input))
            {
                value = (T)converter.ConvertFromString(input);
                return true;
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Helper to split string into a list of strings.
        /// </summary>
        /// <param name="str"></param>
        /// <param name="splitChar"></param>
        /// <param name="removeEmptyEntries"></param>
        /// <returns></returns>
        static List<string> SplitStr(string str, string splitChar, bool removeEmptyEntries = true)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new List<string>();

            var splitOption = removeEmptyEntries ? System.StringSplitOptions.RemoveEmptyEntries : System.StringSplitOptions.None;
            return str.Split(new string[] { splitChar }, splitOption).Select(x => x.Trim()).ToList();
        }

        #endregion Helpers
    }
}
