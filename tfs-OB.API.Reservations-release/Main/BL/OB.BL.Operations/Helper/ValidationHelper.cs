using System.Text;

namespace OB.BL.Operations.Helper
{
    public static class ValidationHelper
    {
        public static bool CardCheckDigit(string CreditCardNumber)
        {
            StringBuilder digitsOnlyBuffer = new StringBuilder();
            foreach (char c in CreditCardNumber.Replace("-", "").Replace(" ", "").ToCharArray())
            {
                if (char.IsDigit(c))
                {
                    digitsOnlyBuffer.Append(c);
                }
            }


            string digitsOnly = digitsOnlyBuffer.ToString();
            int sum = 0;
            int digit = 0;
            int addend = 0;
            bool timesTwo = false;

            for (int i = digitsOnly.Length - 1; i >= 0; i--)
            {
                digit = int.Parse(digitsOnly.Substring(i, 1));
                if (timesTwo)
                {
                    addend = digit * 2;
                    if (addend > 9)
                    {
                        addend -= 9;
                    }
                }
                else
                {
                    addend = digit;
                }
                sum += addend;
                timesTwo = !timesTwo;
            }

            int modulus = sum % 10;
            return modulus == 0;
        }

        public static bool CardCheck(string ccn, long? cardType)
        {
            if (!cardType.HasValue)
                cardType = 0;

            var ic = System.StringComparison.InvariantCulture;
            string CreditCardNumber = ccn.Trim().Replace("-", "").Replace(" ", "");
            if (CardCheckDigit(ccn))
            {
                switch (cardType)
                {
                    case (int)Constants.CCTypes.AmericanExpress:
                        if ((CreditCardNumber.StartsWith("34", ic) || CreditCardNumber.StartsWith("37", ic))
                            &&
                            CreditCardNumber.Length == 15)
                            return true;
                        return false;


                    case (int)Constants.CCTypes.Discover:
                        if (CreditCardNumber.StartsWith("6011", ic) &&
                            CreditCardNumber.Length == 16)
                            return true;
                        return false;


                    case (int)Constants.CCTypes.MasterCard:
                        if ((CreditCardNumber.StartsWith("51", ic) ||
                            CreditCardNumber.StartsWith("52", ic) ||
                            CreditCardNumber.StartsWith("53", ic) ||
                            CreditCardNumber.StartsWith("54", ic) ||
                            CreditCardNumber.StartsWith("55", ic))
                            &&
                            CreditCardNumber.Length == 16)
                            return true;
                        return false;


                    case (int)Constants.CCTypes.Visa:
                        if (CreditCardNumber.StartsWith("4", ic) &&
                            (CreditCardNumber.Length == 13 ||
                            CreditCardNumber.Length == 16))
                            return true;
                        return false;

                    case (int)Constants.CCTypes.Diners:
                        if ((CreditCardNumber.StartsWith("300", ic) ||
                            CreditCardNumber.StartsWith("301", ic) ||
                            CreditCardNumber.StartsWith("302", ic) ||
                            CreditCardNumber.StartsWith("303", ic) ||
                            CreditCardNumber.StartsWith("304", ic) ||
                            CreditCardNumber.StartsWith("305", ic) ||
                            CreditCardNumber.StartsWith("36", ic) ||
                            CreditCardNumber.StartsWith("38", ic)) &&
                            CreditCardNumber.Length == 14)
                            return true;
                        return false;


                    default:
                        return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
