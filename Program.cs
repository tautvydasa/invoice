using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace invoice
{
    class Program : IProgram
    {
        static void Main(string[] args)
        {
            Program foo = new Program();
            int VAT = 0;

            string customerCountry = File.ReadLines("..//..//..//Customer.txt").Skip(1).Take(1).First();
            customerCountry = foo.GetCountryFromAddress(customerCountry);
            string vendorCountry = File.ReadLines("..//..//..//Vendor.txt").Skip(1).Take(1).First();
            vendorCountry = foo.GetCountryFromAddress(vendorCountry);

            List<Country> countries = ReadCountryToCountryCodeFile();

            string customerCountryCode = "";
            string vendorCountryCode = "";
            customerCountryCode = foo.ConvertCountryToCountryCode(customerCountry, countries);
            vendorCountryCode = foo.ConvertCountryToCountryCode(vendorCountry, countries);

            string customerVATCode = File.ReadLines("..//..//..//Customer.txt").Skip(3).Take(1).FirstOrDefault();
            string vendorVATCode = File.ReadLines("..//..//..//Vendor.txt").Skip(3).Take(1).FirstOrDefault();

            List<VAT> vats = ReadEUCountriesFile();

            VAT = foo.CalculateVAT(VAT, customerVATCode, vendorVATCode, customerCountryCode, vendorCountryCode, vats);

            List<Product> products = ReadProductsFile();

            WriteToConsole(VAT, products);
            WriteToFile(VAT, products);
            Console.ReadKey();
        }

        public string GetCountryFromAddress(string country)
        {
            string[] words = Regex.Split(country, ", ");
            foreach (string word in words)
            {
                country = word;
            }
            return country;
        }

        public string ConvertCountryToCountryCode(string memberCountry, List<Country> countries)
        {
            foreach (Country country in countries)
            {
                if (country.CountryName == memberCountry)
                {
                    memberCountry = country.CountryCode;
                    return memberCountry;
                }
            }
            return null;
        }

        public int CalculateVAT(int VAT, string customerVATCode, string vendorVATCode, string CustomerCountryCode, string VendorCountryCode, List<VAT> vats)
        {
            if (vendorVATCode == null)
                VAT = 0;
            else
            {
                if (!vats.Any(x => x.Country == CustomerCountryCode))
                    VAT = 0;
                else if (vats.Any(x => x.Country == CustomerCountryCode) && customerVATCode == null && CustomerCountryCode != VendorCountryCode)
                    foreach (VAT vat in vats)
                    {
                        if (vat.Country == VendorCountryCode)
                            VAT = vat.VATSize;
                    }
                else if (vats.Any(x => x.Country == CustomerCountryCode) && customerVATCode != null && CustomerCountryCode != VendorCountryCode)
                    VAT = 0;
                else if (CustomerCountryCode == VendorCountryCode)
                    foreach (VAT vat in vats)
                    {
                        if (vat.Country == CustomerCountryCode)
                            VAT = vat.VATSize;
                    }
            }
            return VAT;
        }

        public static List<Country> ReadCountryToCountryCodeFile()
        {
            List<Country> countries = new List<Country>();
            using (StreamReader file = new StreamReader("..//..//..//CountryToCountryCode.txt"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    char[] delimiters = new char[] { ',' };
                    string[] parts = line.Split(delimiters);
                    Country country = new Country();
                    country.CountryName = parts[0];
                    country.CountryCode = parts[1];
                    countries.Add(country);
                }
                file.Close();
            }
            return countries;
        }

        public static List<VAT> ReadEUCountriesFile()
        {
            List<VAT> vats = new List<VAT>();
            using (StreamReader file = new StreamReader("..//..//..//EUCountriesVATRates.txt"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    char[] delimiters = new char[] { ',' };
                    string[] parts = line.Split(delimiters);
                    VAT vat = new VAT();
                    vat.Country = parts[0];
                    vat.VATSize = Convert.ToInt32(parts[1]);
                    vats.Add(vat);
                }
                file.Close();
            }
            return vats;
        }

        public static List<Product> ReadProductsFile()
        {
            List<Product> products = new List<Product>();
            using (StreamReader file = new StreamReader("..//..//..//Products.txt"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    char[] delimiters = new char[] { ',' };
                    string[] parts = line.Split(delimiters);
                    Product product = new Product();
                    product.Name = parts[0];
                    product.Quantity = Convert.ToInt32(parts[1]);
                    product.Price = Convert.ToDecimal(parts[2]);
                    products.Add(product);
                }
                file.Close();
            }
            return products;
        }

        public static void WriteToConsole(int VAT, List<Product> products)
        {
            Console.WriteLine("TIEKĖJAS");
            using (StreamReader file = new StreamReader("..//..//..//Vendor.txt"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
                file.Close();
            }

            Console.WriteLine("\nPIRKĖJAS");
            using (StreamReader file = new StreamReader("..//..//..//Customer.txt"))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                {
                    Console.WriteLine(line);
                }
                file.Close();
            }

            int counter = 0;
            decimal orderSum = 0;
            Console.WriteLine("\nUŽSAKYMAS");
            Console.WriteLine(new String('-', 126));
            Console.WriteLine("| Eil.nr |      Produktas      | Kiekis | Kaina (be PVM) Eur | Suma (be PVM) Eur | PVM tarifas (%) | PVM suma Eur | Suma Eur |");
            Console.WriteLine(new String('-', 126));
            foreach (Product product in products)
            {
                counter = counter + 1;
                decimal productSumWithoutVAT = product.Quantity * product.Price;
                decimal VATSum = productSumWithoutVAT * VAT / 100;
                decimal productSum = product.Price + product.Price * VAT / 100;
                orderSum = orderSum + productSum;
                Console.WriteLine("| {0,-6} | {1,-19} | {2,6} | {3,18} | {4,17} | {5,15} | {6,12} | {7,8} |", counter, product.Name, product.Quantity, product.Price, productSumWithoutVAT, VAT, VATSum, productSum);
            }
            Console.WriteLine(new String('-', 126));
            Console.Write(new String(' ', 105));
            Console.WriteLine("| Iš viso: {0,8} |", orderSum);
            Console.Write(new String(' ', 105));
            Console.Write(new String('-', 21));
        }

        public static void WriteToFile(int VAT, List<Product> products)
        {
            using (StreamWriter write = new StreamWriter("..//..//..//Invoice.txt"))
            {
                write.WriteLine("TIEKĖJAS");
                using (StreamReader file = new StreamReader("..//..//..//Vendor.txt"))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        write.WriteLine(line);
                    }
                    file.Close();
                }

                write.WriteLine(Environment.NewLine + "PIRKĖJAS");
                using (StreamReader file = new StreamReader("..//..//..//Customer.txt"))
                {
                    string line;
                    while ((line = file.ReadLine()) != null)
                    {
                        write.WriteLine(line);
                    }
                    file.Close();
                }

                int counter = 0;
                decimal orderSum = 0;
                write.WriteLine(Environment.NewLine + "UŽSAKYMAS");
                write.WriteLine(new String('-', 126));
                write.WriteLine("| Eil.nr |      Produktas      | Kiekis | Kaina (be PVM) Eur | Suma (be PVM) Eur | PVM tarifas (%) | PVM suma Eur | Suma Eur |");
                write.WriteLine(new String('-', 126));
                foreach (Product product in products)
                {
                    counter = counter + 1;
                    decimal productSumWithoutVAT = product.Quantity * product.Price;
                    decimal VATSum = productSumWithoutVAT * VAT / 100;
                    decimal productSum = product.Price + product.Price * VAT / 100;
                    orderSum = orderSum + productSum;
                    write.WriteLine("| {0,-6} | {1,-19} | {2,6} | {3,18} | {4,17} | {5,15} | {6,12} | {7,8} |", counter, product.Name, product.Quantity, product.Price, productSumWithoutVAT, VAT, VATSum, productSum);
                }
                write.WriteLine(new String('-', 126));
                write.Write(new String(' ', 105));
                write.WriteLine("| Iš viso: {0,8} |", orderSum);
                write.Write(new String(' ', 105));
                write.Write(new String('-', 21));
            }
        }
    }
}
