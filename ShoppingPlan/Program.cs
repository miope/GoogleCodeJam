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
            IDictionary<string, double> _cache;
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

                        var _firstOne = new Purchase() { Location = _locations.GetHome(), Cost=0.0, IsPerishable=false, Product=null};
                        _cache = new Dictionary<string, double>();
                        var _temp = GoShopping(0.0, _firstOne, _locations, _gasPrice, _products.OrderBy(p=> p.Name), _cache);
                        var _minAmount = _temp.Min();
                        _output.WriteLine(string.Format("Case #{0}: {1}", _case, Math.Round(_minAmount, 7).ToString("F7", new CultureInfo("en-US"))));
                        //if (_case > 6) break;
                    }
                }
            }
            Console.WriteLine(string.Format("Duration: {0}", DateTime.Now.Subtract(_start).ToString()));
            Console.ReadLine();
        }

        static IEnumerable<double> GoShopping(double spentSoFar, Purchase p, IEnumerable<Location> locations, double gasPrice, IEnumerable<Product> stillToBuy, IDictionary<string, double> memoizer)
        {
            if (p.Product != null)
            { 
                Console.WriteLine(string.Format("Purchased {0} in {1} for {2} {3}", p.Product.Name, p.Location.Name, p.Cost, p.IsPerishable));
            }

            spentSoFar += p.Cost;

            if (!stillToBuy.Any())
            {
                double _res = spentSoFar + p.Location.DistanceTo(locations.GetHome()) * gasPrice;
                Console.WriteLine(string.Format("Total: {0}", _res));
                Enumerable.Range(1, 20).ToList().ForEach(i => Console.Write("-"));
                Console.WriteLine();
                yield return _res;
            }
            else
            { 
                string _errand = string.Format("{0}:{1}", p.Location.Name, stillToBuy.Select(pro => pro.Name).Aggregate((c, n) => c + ";" + n));
                if (memoizer.ContainsKey(_errand))
                {
                    yield return memoizer[_errand];
                }
                else
                { 
                    foreach(Purchase np in p.NextPurchases(locations, gasPrice, stillToBuy))
                    {
                        var _stillOnTheList = stillToBuy.Where(pro => pro.Name != np.Product.Name).OrderBy(pro => pro.Name);
                        foreach (double x in GoShopping(spentSoFar, np, locations, gasPrice, _stillOnTheList, memoizer))
                        {
                            memoizer[_errand] = x;
                            yield return x;
                        }
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

        public IEnumerable<Purchase> NextPurchases(IEnumerable<Location> locations, double gasPrice, IEnumerable<Product> stillToBuy)
        {
            return AllPossiblePurchases(locations, gasPrice).Where(p => stillToBuy.Select(pr => pr.Name).Contains(p.Product.Name));
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
            return locations.Where(loc => loc.IsHome).SingleOrDefault();
        }
    }
}