using OB.DL.Common.QueryResultObjects;
using contractsRates = OB.BL.Contracts.Data.Rates;

namespace OB.BL.Operations.Internal.TypeConverters
{
    /// <summary>
    /// The purpose of this class is exclusively to convert classes from the QueryResultObjects repository namespace 
    /// into BusinessObjects/Data Transfer Objects or classes from the Contracts namespace.
    /// </summary>
    public class BusinessObjectTypeToQueryResultObjectConverter
    {
        #region DepositPolicy

        public static DepositPolicyQR1 Convert(OB.BL.Contracts.Data.Rates.DepositPolicy obj)
        {
            var newObj = new DepositPolicyQR1();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(OB.BL.Contracts.Data.Rates.DepositPolicy obj, DepositPolicyQR1 objDestination)
        {
            objDestination.UID                      = obj.UID;
            objDestination.DepositPolicyName                     = obj.Name;
            objDestination.DepositPolicyDescription = obj.Description;
            objDestination.TranslatedDepositPolicyName           = obj.TranslatedName;
            objDestination.TranslatedDepositPolicyDescription    = obj.TranslatedDescription;
            objDestination.DepositPolicyDays                     = obj.Days ?? 0;
            objDestination.IsDepositCostsAllowed    = obj.IsDepositCostsAllowed;
            objDestination.DepositCosts             = obj.DepositCosts;
            objDestination.Value                    = obj.Value;
            objDestination.PaymentModel             = obj.PaymentModel;
            objDestination.NrNights                 = obj.NrNights;
        }

        #endregion

        #region CancelationPolicy

        public static CancellationPolicyQR1 Convert(contractsRates.CancellationPolicy obj)
        {
            var newObj = new CancellationPolicyQR1();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsRates.CancellationPolicy obj, CancellationPolicyQR1 objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.CancelPolicyName = obj.Name;
            objDestination.CancellationPolicy_Description = obj.Description;
            objDestination.TranslatedCancelPolicyName = obj.TranslatedName;
            objDestination.TranslatedCancellationPolicy_Description = obj.TranslatedDescription;
            objDestination.CancellationDays = obj.Days ?? 0;
            objDestination.IsCancellationAllowed = obj.IsCancellationAllowed ?? false;
            objDestination.CancellationCosts = obj.CancellationCosts ?? false;
            objDestination.Value = obj.Value;
            objDestination.PaymentModel = obj.PaymentModel;
            objDestination.NrNights = obj.NrNights;
        }

        #endregion

        #region OTHER POLICY

        public static OtherPolicyQR1 Convert(contractsRates.OtherPolicy obj)
        {
            var newObj = new OtherPolicyQR1();

            Map(obj, newObj);

            return newObj;
        }

        public static void Map(contractsRates.OtherPolicy obj, OtherPolicyQR1 objDestination)
        {
            objDestination.UID = obj.UID;
            objDestination.Name = obj.OtherPolicy_Name;
            objDestination.Description = obj.OtherPolicy_Description;
            objDestination.TranslatedName = obj.TranslatedName;
            objDestination.TranslatedDescription = obj.TranslatedDescription;
        }

        #endregion OTHER POLICY
    }
}
