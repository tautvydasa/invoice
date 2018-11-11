using System;
using Xunit;
using NSubstitute;
using System.Collections.Generic;

namespace invoice
{
    public class TestCase
    {
        [Fact]
        public void TestGetCountryFromAddress()
        {
            var substitude = Substitute.For<IProgram>();
            Assert.Equal("Lithuania", substitude.GetCountryFromAddress("Stoties 200, Kaunas, Lithuania"));
        }

        [Fact]
        public void TestConvertCountryToCountryCode()
        {
            var substitude = Substitute.For<IProgram>();
            List<Country> list = new List<Country>();
            list.Add(new Country { CountryName = "Austria", CountryCode = "AT" });
            list.Add(new Country { CountryName = "Lithuania", CountryCode = "LT" });
            list.Add(new Country { CountryName = "Slovakia", CountryCode = "SK" });
            Assert.Equal("LT", substitude.ConvertCountryToCountryCode("Lithuania", list));
        }

        [Fact]
        public void TestCalculateVAT()
        {
            var substitude = Substitute.For<IProgram>();
            List<VAT> list = new List<VAT>();
            list.Add(new VAT { Country = "AT", VATSize = 20 });
            list.Add(new VAT { Country = "LT", VATSize = 21 });
            list.Add(new VAT { Country = "SK", VATSize = 20 });
            // Kai paslaugų tiekėjas nėra PVM mokėtojas - PVM mokestis nuo užsakymo sumos nėra skaičiuojamas
            Assert.Equal(0, substitude.CalculateVAT(0, "LT16456466465", null, "LT", "LT", list));
            // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas gyvena už EU(Europos sąjungos) ribų - PVM taikomas 0 %
            Assert.Equal(0, substitude.CalculateVAT(0, "LT16456466465", "AF31321231321313", "LT", "AF", list));
            // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas gyvena EU, yra ne PVM mokėtojas, bet gyvena skirtingoje šalyse nei paslaugų tiekėjas. Taikomas PVM x%, kur x - toje šalyje taikomas PVM procentas, pvz.: Lietuva 21 % PVM
            Assert.Equal(20, substitude.CalculateVAT(0, null, "AT31321231321313", "LT", "AT", list));
            // Kai paslaugų tiekėjas yra PVM mokėtojas, o klientas gyvena EU, yra PVM mokėtojas, bet gyvena skirtingoje šalyse nei paslaugų tiekėjas. Taikomas 0% pagal atvirkštinį apmokestinimą
            Assert.Equal(0, substitude.CalculateVAT(0, "LT16456466465", "AT31321231321313", "LT", "AT", list));
            // Kai paslaugų tiekėjas yra PVM mokėtojas bei paslaugų tiekėjas ir klientas gyvena toje pačioje šalyje - visada taikomas PVM
            Assert.Equal(21, substitude.CalculateVAT(0, "LT16456466465", "LT31321231321313", "LT", "LT", list)); 
        }
    }
}
