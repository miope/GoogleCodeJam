using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;
using System.Diagnostics;

namespace ShoppingPlan
{
    class Program
    {
        static void Main(string[] args)
        {
            string _inputFile = args[0];
            string _outputFile = Path.ChangeExtension(_inputFile, "out");
            var _start = DateTime.Now;

            using (var _input = new StreamReader(_inputFile))
            {
                using (var _output = new StreamWriter(_outputFile))
                { 
                    var _testCases = _input.ReadLine();
                    string _line;
                    int _case = 0;

                    while((_line = _input.ReadLine()) != null)
                    {
                        ++_case;

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

                        var _firstOne = new Purchase() { Location = _locations.GetHome(), Cost=0.0, IsPerishable=false, Product=null};
                        var _memoizer = new Dictionary<string, double>();
                        Console.WriteLine(string.Format("Calculating Case # {0}...", _case));
                        var _minAmount = MinCostForAcquiring(_products, _firstOne, _locations, _gasPrice, _memoizer);
                        Console.WriteLine(string.Format("Done...result={0}", _minAmount));
                        Console.WriteLine();
                        _output.WriteLine(string.Format("Case #{0}: {1}", _case, Math.Round(_minAmount, 7).ToString("F7", new CultureInfo("en-US"))));
                    }
                }
            }
            Console.WriteLine(string.Format("Duration: {0}", DateTime.Now.Subtract(_start).ToString()));
            Console.ReadLine();
        }

        static double MinCostForAcquiring(IEnumerable<Product> productsToBuy, Purchase startingPoint, IEnumerable<Location> locations, double gasPrice, IDictionary<string, double> memoizer)
        {

            if (!productsToBuy.Any())
            {
                return startingPoint.Location.DistanceTo(locations.GetHome()) * gasPrice;
            }

            string _key = string.Format("{0}:{1}:{2}", startingPoint.Location.Name, startingPoint.IsPerishable.ToString(), productsToBuy.Select(p => p.Name).Aggregate((r, n) => r + ";" + n));

            if (memoizer.ContainsKey(_key))
            {
                return memoizer[_key];
            }

            var _result = productsToBuy.SelectMany(p => startingPoint.PossiblePurchasesOf(p, locations, gasPrice)
                                                                     .Select(pur => pur.Cost + MinCostForAcquiring(productsToBuy.Without(p), 
                                                                                                                   pur, locations, gasPrice, memoizer)))
                                       .Min();

            memoizer.Add(_key, _result);
            return _result;
        }
    }

    public class Location
    {
        public string Name { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public IEnumerable<Product> Products { get; set; }

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
            return Math.Sqrt((this.X - otherLocation.X).Pow2() + (this.Y - otherLocation.Y).Pow2());
        }

        public double DistanceToVia(Location otherLocation, Location via)
        {
            return this.DistanceTo(via) + via.DistanceTo(otherLocation);
        }

        public bool IsSameAs(Location otherLocation)
        {
            return this.X == otherLocation.X && this.Y == otherLocation.Y;
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
        public Location Location { get; set; }
        public Product Product { get; set; }
        public double Cost { get; set; }
        public bool IsPerishable { get; set; }

        private IEnumerable<Purchase> _purchaseCache { get; set; }

        private IEnumerable<Purchase> AllPossiblePurchases(IEnumerable<Location> locations, double gasPrice)
        {
            if (_purchaseCache == null)
            {
                _purchaseCache = locations.SelectMany(loc => 
                    loc.Products.Select(p => new Purchase() {
                        Location = loc,
                        Product = p,
                        IsPerishable = this.IsPerishable & this.Location.IsSameAs(loc) ? true : p.IsPerishable, 
                        Cost = this.IsPerishable & loc.Name != this.Location.Name ? p.Price + this.Location.DistanceToVia(loc, locations.GetHome()) * gasPrice :
                                                                                    p.Price + (this.Location.DistanceTo(loc) * gasPrice)
                    })
                );
            }

            return _purchaseCache;
        }

        public IEnumerable<Purchase> PossiblePurchasesOf(Product product, IEnumerable<Location> locations, double gasPrice)
        {
            return AllPossiblePurchases(locations, gasPrice).Where(p => p.Product.Name == product.Name);
        }
    }

    public static class Extensions
    {
        public static double Pow2(this int num)
        {
            return (double)(num * num);
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
            return locations.Where(loc => loc.IsHome).SingleOrDefault();
        }

        public static IEnumerable<Product> Without(this IEnumerable<Product> products, Product product)
        {
            return products.Where(p => p.Name != product.Name);
        }
    }
}