using System;
using System.Collections.Generic;
using System.Text;

namespace invoice
{
    public interface IProgram
    {
        string GetCountryFromAddress(string country);
        string ConvertCountryToCountryCode(string memberCountry, List<Country> countries);
        int CalculateVAT(int VAT, string customerVATCode, string vendorVATCode, string CustomerCountryCode, string VendorCountryCode, List<VAT> vats);
    }
}
