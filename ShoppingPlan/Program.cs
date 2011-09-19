using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace ShoppingPlan
{
    class Program
    {
        static void Main(string[] args)
        {
            string _inputFile = args[0];
            string _outputFile = Path.ChangeExtension(_inputFile, "out");

            using (var _input = new StreamReader(_inputFile))
            {
                using (var _output = new StreamWriter(_outputFile))
                { 
                    var _home = new Location("home", 0, 0);
                    var _testCases = _input.ReadLine();
                    string _line;
                    int _case = 0;

                    while((_line = _input.ReadLine()) != null)
                    {
                        _case++;

                        var _parts = _line.Split(' ');
                        var _numOfProducts = int.Parse(_parts[0]);
                        var _numOfShops = int.Parse(_parts[1]);
                        var _gasPrice = double.Parse(_parts[2]);

                        IEnumerable<Product> _products = _input.ReadLine().Split(' ').Select(s => new Product(s));

                        var _shops = new List<Shop>();
                        Enumerable.Range(1, _numOfShops).ToList().ForEach(x => {
                            _parts = _input.ReadLine().Split(' ');
                            var _aShop = new Shop(string.Format("{0}.{1}", _case, x.ToString()), int.Parse(_parts[0]), int.Parse(_parts[1]));
                            _aShop.Products = _parts.Skip(2).Select(s => {
                                var _a = s.Split(':');
                                return _products.GetByName(_a[0]).Clone(double.Parse(_a[1]));
                            });
                            _shops.Add(_aShop);
                        });

                        var _shoppingPlan = _products.GetShoppingPlan(_shops, _home, _gasPrice);
                        var _minAmount = _shoppingPlan.Sum(p => p.Cost) + _shoppingPlan.Last().Shop.DistanceTo(_home) * _gasPrice;

                        _output.WriteLine(string.Format("Case #{0}: {1}", _case, Math.Round(_minAmount, 7).ToString("F7", new CultureInfo("en-US"))));

                    }
                }
            }
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

        public bool IsHome
        {
            get
            {
                return this.X == 0 && this.Y == 0;
            }
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
            get { return Name.EndsWith("!"); }
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
        public static double Squared(this int num)
        {
            return Math.Pow((double)num, 2.0);
        }

        public static Product GetByName(this IEnumerable<Product> products, string name)
        {
            return products.Where(p => p.Name == name || p.Name == string.Format("{0}!", name)).SingleOrDefault();
        }

        public static Product Clone(this Product product, double newPrice)
        {
            return new Product(product.Name, newPrice);
        }

        public static IEnumerable<Purchase> GetShoppingPlan(this IEnumerable<Product> products, IEnumerable<Shop> shops, 
            Location home, double gasPrice)
        {

            var _purchases = new List<Purchase>();
            var _products = new List<Product>(products);
            var _destination = home;

            while (_products.Any())
            {
                var _purchase = shops.SelectMany(s => s.PossiblePurchases(_destination, gasPrice))
                                     .Where(p => _products.Select(o => o.Name).Contains(p.Product.Name))
                                     .OrderBy(p => p.Cost)
                                     .First();

                _purchases.Add(_purchase);
                _destination = _purchase.Shop;
                _products.Remove(_products.GetByName(_purchase.Product.Name));
            }

            return _purchases;
        }
    }
}