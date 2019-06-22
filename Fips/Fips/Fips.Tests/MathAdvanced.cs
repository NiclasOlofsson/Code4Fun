using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fips.Tests
{
	/// <summary>
	/// This code is based on
	/// https://msdn.microsoft.com/en-us/magazine/dn520240.aspx?f=255&MSPPError=-2147217396
	/// </summary>

	public static class MathAdvanced
	{

		public static Dictionary<int, double> FrequencyTest(byte[][] values)
		{
			var bitList = GetBitList(values);

			var result = new Dictionary<int, double>();
			foreach (KeyValuePair<int, BitArray> kvp in bitList)
			{
				result.Add(kvp.Key, FrequencyTest(kvp.Value));
			}

			return result;
		}

		private static double FrequencyTest(BitArray bitArray)
		{
			double sum = 0;
			for (int i = 0; i < bitArray.Length; ++i)
			{
				if (bitArray[i] == false) sum = sum - 1;
				else sum = sum + 1;
			}
			double testStat = Math.Abs(sum) / Math.Sqrt(bitArray.Length);
			double rootTwo = 1.414213562373095;
			double pValue = Erfc(testStat / rootTwo);
			return pValue;
		}

		public static Dictionary<int, double> BlockTest(byte[][] values, int blockLength = 4)
		{
			var bitList = GetBitList(values);

			var result = new Dictionary<int, double>();
			foreach (KeyValuePair<int, BitArray> kvp in bitList)
			{
				result.Add(kvp.Key, BlockTest(kvp.Value, blockLength));
			}

			return result;
		}

		private static double BlockTest(BitArray bitArray, int blockLength)
		{
			int numBlocks = bitArray.Length / blockLength; // 'N'
			double[] proportions = new double[numBlocks];
			int k = 0; // ptr into bitArray
			for (int block = 0; block < numBlocks; ++block)
			{
				int countOnes = 0;
				for (int i = 0; i < blockLength; ++i)
				{
					if (bitArray[k++] == true)
						++countOnes;
				}
				proportions[block] = (countOnes * 1.0) / blockLength;
			}
			double summ = 0.0;
			for (int block = 0; block < numBlocks; ++block)
				summ = summ + (proportions[block] - 0.5) *
						(proportions[block] - 0.5);
			double chiSquared = 4 * blockLength * summ; // magic
			double a = numBlocks / 2.0;
			double x = chiSquared / 2.0;
			double pValue = GammaFunctions.GammaUpper(a, x);
			return pValue;
		}

		public static Dictionary<int, double> RunsTest(byte[][] values)
		{
			var bitList = GetBitList(values);

			var result = new Dictionary<int, double>();
			foreach (KeyValuePair<int, BitArray> kvp in bitList)
			{
				result.Add(kvp.Key, RunsTest(kvp.Value));
			}

			return result;
		}

		private static double RunsTest(BitArray bitArray)
		{
			double numOnes = 0.0;
			for (int i = 0; i < bitArray.Length; ++i)
				if (bitArray[i] == true)
					++numOnes;
			double prop = (numOnes * 1.0) / bitArray.Length;
			// Double tau = 2.0 / Math.Sqrt(bitArray.Length * 1.0);
			// If (Math.Abs(prop - 0.5) >= tau)
			// Return 0.0; // Not-random short-circuit
			int runs = 1;
			for (int i = 0; i < bitArray.Length - 1; ++i)
				if (bitArray[i] != bitArray[i + 1])
					++runs;
			double num = Math.Abs(
				runs - (2 * bitArray.Length * prop * (1 - prop)));
			double denom = 2 * Math.Sqrt(2.0 * bitArray.Length) *
							prop * (1 - prop);
			double pValue = Erfc(num / denom);
			return pValue;
		}

		private static Dictionary<int, BitArray> GetBitList(byte[][] values)
		{
			var bitList = new Dictionary<int, BitArray>();
			var numberOfBits = values[0].Length * 8;

			for (int i = 0; i < numberOfBits; i++)
			{
				bitList.Add(i, new BitArray(values.Length));
			}

			for (int i = 0; i < values.Length; i++)
			{
				var bytes = values[i];
				var valueBits = new BitArray(bytes);

				for (int j = 0; j < numberOfBits; j++)
				{
					bitList[j][i] = valueBits[j];
				}
			}
			return bitList;
		}

		private static double Exp(double x)
		{
			if (x < -40.0) // ACM update remark (8)
				return 0.0;
			else
				return Math.Exp(x);
		}

		public static double Gauss(double z)
		{
			// input = z-value (-inf to +inf)
			// output = p under Normal curve from -inf to z
			// ACM Algorithm #209
			double y; // 209 scratch variable
			double p; // result. called ‘z’ in 209
			double w; // 209 scratch variable
			if (z == 0.0)
			{
				p = 0.0;
			}
			else
			{
				y = Math.Abs(z)/2;
				if (y >= 3.0)
				{
					p = 1.0;
				}
				else if (y < 1.0)
				{
					w = y*y;
					p = ((((((((0.000124818987*w
								- 0.001075204047)*w + 0.005198775019)*w
							- 0.019198292004)*w + 0.059054035642)*w
							- 0.151968751364)*w + 0.319152932694)*w
						- 0.531923007300)*w + 0.797884560593)*y
															*2.0;
				}
				else
				{
					y = y - 2.0;
					p = (((((((((((((-0.000045255659*y
									+ 0.000152529290)*y - 0.000019538132)*y
									- 0.000676904986)*y + 0.001390604284)*y
								- 0.000794620820)*y - 0.002034254874)*y
								+ 0.006549791214)*y - 0.010557625006)*y
							+ 0.011630447319)*y - 0.009279453341)*y
							+ 0.005353579108)*y - 0.002141268741)*y
						+ 0.000535310849)*y + 0.999936657524;
				}
			}
			if (z > 0.0)
				return (p + 1.0)/2;
			else
				return (1.0 - p)/2;
		} // Gauss()

		public static double Erfc(double x)
		{
			return 1 - Erf(x);
		}

		public static double Erf(double x)
		{
			// constants
			var a1 = 0.254829592;
			var a2 = -0.284496736;
			var a3 = 1.421413741;
			var a4 = -1.453152027;
			var a5 = 1.061405429;
			var p = 0.3275911;

			// Save the sign of x
			var sign = 1;
			if (x < 0)
				sign = -1;
			x = Math.Abs(x);

			// A&S formula 7.1.26
			var t = 1.0/(1.0 + p*x);
			var y = 1.0 - ((((a5*t + a4)*t + a3)*t + a2)*t + a1)*t*Math.Exp(-x*x);

			return sign*y;
		}



		public static void Shuffle<T>(this IList<T> list, Random random)
		{
			var rng = random?? new Random();
			int n = list.Count;
			while (n > 1)
			{
				n--;
				int k = rng.Next(n + 1);
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}
		}
	}

	/// <summary>
	/// https://msdn.microsoft.com/en-us/magazine/dn520240.aspx?f=255&MSPPError=-2147217396
	/// </summary>
	public class GammaFunctions
	{
		private static double GammaLowerSer(double a, double x)
		{
			// Incomplete GammaLower (computed by series expansion)
			if (x < 0.0)
				throw new Exception("x param less than 0.0 in GammaLowerSer");
			double gln = LogGamma(a);
			double ap = a;
			double del = 1.0/a;
			double sum = del;
			for (int n = 1; n <= 1000; ++n)
			{
				++ap;
				del *= x/ap;
				sum += del;
				if (Math.Abs(del) < Math.Abs(sum)*3.0E-7) // Close enough?
					return sum*Math.Exp(-x + a*Math.Log(x) - gln);
			}
			throw new Exception("Unable to compute GammaLowerSer " +
								"to desired accuracy");
		}

		private static double GammaUpperCont(double a, double x)
		{
			// Incomplete GammaUpper computed by continuing fraction
			if (x < 0.0)
				throw new Exception("x param less than 0.0 in GammaUpperCont");
			double gln = LogGamma(a);
			double b = x + 1.0 - a;
			double c = 1.0/1.0E-30; // Div by close to double.MinValue
			double d = 1.0/b;
			double h = d;
			for (int i = 1; i <= 1000; ++i)
			{
				double an = -i*(i - a);
				b += 2.0;
				d = an*d + b;
				if (Math.Abs(d) < 1.0E-30) d = 1.0E-30; // Got too small?
				c = b + an/c;
				if (Math.Abs(c) < 1.0E-30) c = 1.0E-30;
				d = 1.0/d;
				double del = d*c;
				h *= del;
				if (Math.Abs(del - 1.0) < 3.0E-7)
					return Math.Exp(-x + a*Math.Log(x) - gln)*h; // Close enough?
			}
			throw new Exception("Unable to compute GammaUpperCont " +
								"to desired accuracy");
		}

		public static double LogGamma(double x)
		{
			double[] coef = new double[6]
			{
				76.18009172947146, -86.50532032941677,
				24.01409824083091, -1.231739572450155,
				0.1208650973866179E-2, -0.5395239384953E-5
			};
			double LogSqrtTwoPi = 0.91893853320467274178;
			double denom = x + 1;
			double y = x + 5.5;
			double series = 1.000000000190015;
			for (int i = 0; i < 6; ++i)
			{
				series += coef[i]/denom;
				denom += 1.0;
			}
			return (LogSqrtTwoPi + (x + 0.5)*Math.Log(y) -
					y + Math.Log(series/x));
		}

		public static double GammaUpper(double a, double x)
		{
			// Incomplete Gamma 'Q' (upper)
			if (x < 0.0 || a <= 0.0)
				throw new Exception("Bad args in GammaUpper");
			if (x < a + 1)
				return 1.0 - GammaLowerSer(a, x); // Indirect is faster
			else
				return GammaUpperCont(a, x);
		}

		public static double GammaLower(double a, double x)
		{
			// Incomplete Gamma 'P' (lower) aka 'igam'
			if (x < 0.0 || a <= 0.0)
				throw new Exception("Bad args in GammaLower");
			if (x < a + 1)
				return GammaLowerSer(a, x); // No surprise
			else
				return 1.0 - GammaUpperCont(a, x); // Indirectly is faster
		}
	}
}