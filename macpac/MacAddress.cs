using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Macpac
{
	class MacAddress
	{
		public static string Generate(bool FixAddr = false)
		{
			string Addr = Convert.ToUInt64(new Random().NextDouble().ToString().Substring(2)).ToString("X2").PadLeft(12, '0').Substring(0, 12);
			if(FixAddr) return Correct(Addr);
			else return Addr;
		}
		public static string Correct(string Addr) //windows 7 is limited to locally administered unicast addresses
		{
			string[] HexDigits = { "2", "6", "A", "E" };
			if(HexDigits.Contains(Addr.Substring(1, 1))) return Addr.Substring(0, 1) + HexDigits[new Random().Next(0, 3)] + Addr.Substring(2);
			else return Addr;
		}
		public static bool Validate(string Addr)
		{
			return new Regex(@"[\da-fA-F]{12}").IsMatch(Addr);
		}
	}
}
