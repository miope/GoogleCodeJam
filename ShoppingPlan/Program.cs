using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ShoppingPlan
{
    class Program
    {
        static void Main(string[] args)
        {
            string _inputFile = args[0];
            string _outputFile = Path.ChangeExtension(_inputFile, "out");

            using (var _file = new StreamReader(_inputFile))
            {
                var _home = new Location("home", 0, 0);
                var _testCases = _file.ReadLine();
                string _line;
                int _case = 0;

                while((_line = _file.ReadLine()) != null)
                {
                    _case++;
                    var _parts = _line.Split(' ');
                    var _numOfProducts = int.Parse(_parts[0]);
                    var _numOfShops = int.Parse(_parts[1]);
                    var _gasPrice = double.Parse(_parts[2]);

                    IEnumerable<Product> _products = _file.ReadLine().Split(' ').Select(s => new Product(s));
                    var _shops = new List<Shop>();
                    Enumerable.Range(1, _numOfShops).ToList().ForEach(x => {
                        _parts = _file.ReadLine().Split(' ');
                        var _aShop = new Shop(x.ToString(), int.Parse(_parts[0]), int.Parse(_parts[1]));
                        _aShop.Products = _parts.Skip(2).Select(s => {
                            var _a = s.Split(':');
                            return new Product(_a[0], double.Parse(_a[1]));
                        });
                        _shops.Add(_aShop);
                    });

                    _shops.SelectMany(s => s.PossiblePurchases(_home, _gasPrice))
                                            .Take(5)
                                            .ToList()
                                            .ForEach(p => {
                                                Console.WriteLine(p.Shop.Name);
                                                Console.WriteLine(string.Format("{0} - {1}", p.Product.Name, p.Cost));
                                                Console.WriteLine();
                                            });
                }
            }

            Console.ReadLine();
        }
    }

    public class Location
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public  Location(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
        }

        public double DistanceTo(Location otherLocation)
        {
            return Math.Sqrt((this.X - otherLocation.X).Squared() + (this.Y - otherLocation.Y).Squared());
        }
    }

    public class Shop : Location
    {
        public IEnumerable<Product> Products { get; set; }

        public Shop(string name, int x, int y)
            :base(name, x, y)
        { 
        }

        public IEnumerable<Purchase> PossiblePurchases(Location comingFrom, double gasPrice)
        {
            return this.Products.Select(p => new Purchase() { 
                Shop = this,
                Product = p,
                Cost = p.Price + (comingFrom.DistanceTo(this) * gasPrice)
            });
        }
    }

    public class Product
    {
        public string Name { get; set; }
        public double Price { get; set; }

        public bool IsPerishable
        {
            get
            {
                return Name.EndsWith("!");
            }
        }

        public Product(string name)
        {
            Name = name;
        }

        public Product(string name, double price)
            : this(name)
        {
            Price = price;
        }
    }

    public class Purchase
    {
        public Shop Shop { get; set; }
        public Product Product { get; set; }
        public double Cost { get; set; }
    }

    public static class Extensions
    {
        public static double Squared(this double num)
        {
            return Math.Pow(num, 2d);
        }

        public static double Squared(this int num)
        {
            return ((double)num).Squared();
        }

        public static Product GetByName(this IEnumerable<Product> products, string name)
        {
            return products.Where(p => p.Name == name || p.Name == string.Format("{0}!", name)).SingleOrDefault();
        }

        public static Product Clone(this Product product, double newPrice)
        {
            return new Product(product.Name, newPrice);
        }
    }
}
