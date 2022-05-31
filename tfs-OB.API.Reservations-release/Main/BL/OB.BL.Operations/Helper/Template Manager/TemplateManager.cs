using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.BL.Operations.Helper
{
    public class TemplateManager : ITemplateManager
    {
        public ITemplateSource Resolve(ITemplateKey key)
        {
            var template = GetTemplate(key.Name);
            return new LoadedTemplateSource(template, null);
        }

        public ITemplateKey GetKey(string name, ResolveType resolveType, ITemplateKey context)
        {
            // If you can have different templates with the same name depending on the 
            // context or the resolveType you need your own implementation here!
            // Otherwise you can just use NameOnlyTemplateKey.
            return new NameOnlyTemplateKey(name, resolveType, context);
        }

        public void AddDynamic(ITemplateKey key, ITemplateSource source)
        {
            // You can disable dynamic templates completely, but 
            // then all convenience methods (Compile and RunCompile) with
            // a TemplateSource will no longer work (they are not really needed anyway).
            throw new NotImplementedException("dynamic templates are not supported!");
        }

        private string GetTemplate(string templateKey)
        {
            switch (templateKey)
            {
                case "Perot":
                    return PEROT_TEMPLATE;
                default:
                    return string.Empty;
            }

            //OB.Reservation.BL.Contracts.Data.Reservations.Reservation res = new Contracts.Data.Reservations.Reservation()
            //res.Property_UID
        }

        private const string PEROT_TEMPLATE = "G|A|@Model.Property_UID||@Model.GuestLastName|@Model.GuestFirstName|@Model.Number|@Model.DateFrom|@Model.DateTo|@Model.NumberOfDays|@Model.NumberOfRooms||@Model.CreatedDate"
            + "|@Model.TransactionCode||@Model.NumberOfAdults|@Model.NumberOfChildren||||||@Model.CurrencyCode|@Model.CommissionableRevenue|@Model.CommissionRate|||@Model.CommissionAmount|||||||||||||||IATA|"
            + "@Model.PayeeId|@Model.PayeeLegalName|@Model.PayeeName|@Model.PayeeAddress||@Model.PayeeCity|@Model.PayeeStateCode|@Model.PayeePostalCode|@Model.PayeeCountryCode||||||||HS|";

    }
}
