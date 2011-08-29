using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AlienNumbers
{
    static class Program
    {
        static void Main(string[] args)
        {
            string _inPath = args[0];
            string _outPath = Path.ChangeExtension(_inPath, "out");

            var _result = File.ReadAllLines(_inPath)
                              .Skip(1)
                              .Select((line, idx) => {
                                  var _parts = line.Split(' ');
                                  var _res = _parts[0].ToDecimal(_parts[1]).ToTargetLanguage(_parts[2]);
                                  return string.Format("Case #{0}: {1}", idx+1, _res);
                              });

            File.WriteAllLines(_outPath, _result);
        }

        public static int ToDecimal(this string number, string sourceAlphabet)
        {
            var _base = sourceAlphabet.Length;
            var _lookup = sourceAlphabet.Select((c, i) => new { Key = c, Value = i })
                                        .ToDictionary(o => o.Key);

            return number.Reverse()
                         .Select((c, i) => _lookup[c].Value * (int)Math.Pow(_base, i))
                         .Sum();
        }

        public static string ToTargetLanguage(this int number, string targetAlphabet)
        {
            var _base = targetAlphabet.Length;
            var _num = number;
            var _rem = 0;
            IList<char> _chars = new List<char>();

            do
            {
                _rem = _num % _base;
                _chars.Add(targetAlphabet[_rem]);
                _num = _num / _base;
            }
            while (_num > 0);

            return string.Join("", _chars.Reverse());
        }
    }
}
