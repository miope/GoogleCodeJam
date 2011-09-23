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

                        var _locations = new List<Location>();
                        var _home = new Location("home", 0, 0);
                        _locations.Add(_home);

                        Enumerable.Range(1, _numOfShops).ToList().ForEach(x => {
                            _parts = _input.ReadLine().Split(' ');
                            var _loc = new Location(string.Format("{0}.{1}", _case, x.ToString()), int.Parse(_parts[0]), int.Parse(_parts[1]));
                            _loc.Products = _parts.Skip(2).Select(s => {
                                var _a = s.Split(':');
                                return _products.GetByName(_a[0]).Clone(double.Parse(_a[1]));
                            });
                            _locations.Add(_loc);
                        });

                        var _temp = _locations[0].PossiblePurchases(_locations, _gasPrice);
                        var a = _temp.First();
                        var b = a.NextPurchases(_locations, _gasPrice);

                        var _minAmount = (double)_temp.Count();
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
        public IEnumerable<Product> Products { get; set; }

        private IEnumerable<Purchase> _purchaseCache;

        public  Location(string name, int x, int y)
        {
            Name = name;
            X = x;
            Y = y;
            Products = new List<Product>();
        }

        public bool IsHome
        {
            get { return this.X == 0 && this.Y == 0; }
        }

        public double DistanceTo(Location otherLocation)
        {
            return Math.Sqrt((this.X - otherLocation.X).Squared() + (this.Y - otherLocation.Y).Squared());
        }

        public double DistanceToVia(Location otherLocation, Location via)
        {
            return this.DistanceTo(via) + via.DistanceTo(otherLocation);
        }

        public IEnumerable<Purchase> LocalPurchases(Location comingFrom, double gasPrice)
        {
            return this.Products.Select(p => new Purchase() { 
                Location = this,
                Product = p,
                IsPersihable = p.IsPerishable,
                Cost = p.Price + (comingFrom.DistanceTo(this) * gasPrice)
            });
        }

        public IEnumerable<Purchase> PossiblePurchases(IEnumerable<Location> locations, double gasPrice)
        {
            if (_purchaseCache == null)
            {
                _purchaseCache = locations.SelectMany(l => l.LocalPurchases(this, gasPrice));
            }

            return _purchaseCache;
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
        private IEnumerable<Purchase> _nextPurchases;

        public Location Location { get; set; }
        public Product Product { get; set; }
        public double Cost { get; set; }
        public bool IsPersihable { get; set; }

        public IEnumerable<Purchase> NextPurchases(IEnumerable<Location> locations, double gasPrice)
        {
            if(_nextPurchases == null)
            {
                _nextPurchases = Location.PossiblePurchases(locations, gasPrice)
                                         .Where(p => p.Product.Name != Product.Name);

                _nextPurchases.ToList().ForEach(p => {
                    p.IsPersihable = this.IsPersihable && p.Location.Name == this.Location.Name;
                    if(this.IsPersihable && p.Location.Name != this.Location.Name)
                    {
                        p.Cost = p.Product.Price + this.Location.DistanceToVia(p.Location, locations.GetHome()); ;
                    }
                });
            }

            return _nextPurchases;
        }

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

        public static Location GetHome(this IEnumerable<Location> locations)
        {
            return locations.Where(l => l.Name == "home").SingleOrDefault();
        }
    }
}