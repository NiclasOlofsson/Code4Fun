using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Fips.Tests
{
	[TestFixture]
	public class EntropyTests
	{
		private Dictionary<int, double> CalculateFipsMonobitTest(byte[][] values)
		{
			// Count the number of ones in the 20,000 bit stream. Denote this quantity by X.
			//
			// FIPS 140-1: The test is passed if 9,654 < X < 10,346.

			Dictionary<int, int> fips = new Dictionary<int, int>();
			for (int i = 0; i < values.Length; i++)
			{
				byte[] bytes = values[i];
				BitArray bits = new BitArray(bytes);
				for (int j = 0; j < bits.Length; j++)
				{
					fips.TryAdd(j, 0);
					bool b = bits[j];
					fips[j] = fips[j] + (b ? 1 : -1);
				}
			}

			Dictionary<int, double> result = new Dictionary<int, double>();

			foreach (var fip in fips)
			{
				// FIPS 140-1: The test is passed if 9,654 < X < 10,346.
				//bool passed = 9654 < fip.Value && fip.Value < 10346;
				//bool passed = 9900 < fip.Value && fip.Value < 10100;

				double sObs = Math.Abs(fip.Value) / Math.Sqrt(values.Length);
				double pVal = MathAdvanced.Erfc(sObs / Math.Sqrt(2));

				bool pass0_10 = pVal > 0.1;
				bool pass0_01 = pVal > 0.01;
				bool pass0_001 = pVal > 0.001;
				bool pass0_0001 = pVal > 0.0001;

				//var distribution = (fip.Value/(double) values.Length);
				//if (!pass0_10)
				//{
				//	Console.WriteLine($"Bit: {fip.Key:D2}; pVal={pVal:F3}; S={fip.Value};" +
				//					$"\n" +
				//					$"\t10% {pass0_10}" +
				//					$"\t1% {pass0_01}" +
				//					$"\t0.1% {pass0_001}" +
				//					$"\t0.01% {pass0_0001}");
				//}
				result.Add(fip.Key, pVal);
			}

			return result;
		}

		private Dictionary<int, double> CalculateFipsPokerTest(byte[][] values)
		{
			// Divide the 20,000 bit stream into 5,000 consecutive 4 bit segments. Count and store the number of occurrences
			// of the 16 possible 4 bit values. Denote f(i)as the number of each 4 bit value i, where 0 = i = 15.
			// Evaluate the following: X = (16/5000) * (SUM i=0 -> i=15 [f(i)]^2) - 5000
			// 
			// FIPS 140-1: The test is passed if 1.03 < X < 57.4.

			if (values.Length % 4 != 0) throw new ArgumentException("length of values must be integer div by 4");

			Dictionary<int, Dictionary<int, int>> fips = new Dictionary<int, Dictionary<int, int>>();
			int k = 0;
			for (int i = 0; i < values.Length; k++)
			{
				BitArray bits0 = new BitArray(values[i++]);
				BitArray bits1 = new BitArray(values[i++]);
				BitArray bits2 = new BitArray(values[i++]);
				BitArray bits3 = new BitArray(values[i++]);
				for (int j = 0; j < bits0.Length; j++)
				{
					int b0 = bits0[j] ? 1 : 0;
					int b1 = bits1[j] ? 1 : 0;
					int b2 = bits2[j] ? 1 : 0;
					int b3 = bits3[j] ? 1 : 0;
					int num = (b0 << 3) | (b1 << 2) | (b2 << 1) | b3;
					fips.TryAdd(j, new Dictionary<int, int>());
					fips[j][k] = num;
				}
			}

			double probability = 1 / 16d;

			Dictionary<int, double> result = new Dictionary<int, double>();

			foreach (var fip in fips)
			{
				var group = fip.Value.GroupBy(pair => pair.Value).Select(n => new
				{
					MetricValue = n.Key,
					MetricCount = n.Count(),
					ExpectedCount = values.Length / 4d / 16d,
					ActualProbability = n.Count() * probability,
				}).OrderBy(arg => arg.MetricCount);

				double chiSq = 0.0;
				foreach (var g in group)
				{
					chiSq += ((g.MetricCount - g.ExpectedCount) *
							(g.MetricCount - g.ExpectedCount)) / g.ExpectedCount;
				}

				var pVal = MathAdvanced.ChiSquarePval(chiSq, 16 - 1);

				// FIPS 140-1: The test is passed if 1.03 < X < 57.4
				bool passed = 1.03 < chiSq && chiSq < 57.4;

				//Console.WriteLine($"Bit: {fip.Key:D2}; ChiSq={chiSq:F2}; PVal={pVal:F3},\tPass={passed}");

				result.Add(fip.Key, pVal);

				//foreach (var g in group)
				//{
				//	Console.WriteLine($"\t{g.MetricValue}; {g.MetricCount}; {g.ExpectedCount:F1}; {g.ActualProbability:F2}");
				//}

			}

			return result;
		}

		private const int byteLength = 5;
		private static byte[][] GenerateBaseValues()
		{
			RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			int sampleCount = 20_000;
			byte[][] values = new byte[sampleCount][];

			for (int i = 0; i < sampleCount; i++)
			{
				values[i] = new byte[byteLength];
				rng.GetBytes(values[i]);
			}
			return values;
		}


		[Test]
		public void Random_generator_base_test()
		{
			var seedGenerator = new Random();
			int sampleCount = 20_000;
			int keyLen = byteLength;
			byte[][] values = new byte[sampleCount][];
			for (int i = 0; i < sampleCount; i++)
			{
				values[i] = new byte[keyLen];
				new Random(seedGenerator.Next()).NextBytes(values[i]);
			}

			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");

			CalculateFipsPokerTest(values);
		}

		[Test]
		public void RNGCrypto_generator_base_test()
		{
			var values = GenerateBaseValues();

			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}

		[Test]
		public void Random_generator_used_wrong_test()
		{
			var rnd = new Random();
			int sampleCount = 20_000;
			int keyLen = byteLength;
			byte[][] values = new byte[sampleCount][];
			for (int i = 0; i < sampleCount; i++)
			{
				values[i] = new byte[keyLen];
				rnd.NextBytes(values[i]);
			}

			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}

		[Test]
		public void Embedded_full_sequence_in_order_detection_test()
		{
			var values = GenerateBaseValues();

			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");

			int c = 0;
			int count = 0;
			for (int i = 0; i < 200; c++)
			{
				values[c] = new byte[byteLength];
				Array.Copy(BitConverter.GetBytes(i), values[c], 4);
				i++;
				count++;
			}

			Console.WriteLine($"Inserted {count} values spread over {c} positions");
			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}

		[Test]
		public void Embedded_fragmented_sequence_in_order_detection_test()
		{
			var values = GenerateBaseValues();

			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");

			var random = new Random();
			int c = 0;
			int count = 0;
			for (int i = 0; i < 200; c++)
			{
				if (random.Next(40) < 10) // simulate multiple users or slow attack
				{
					values[c] = new byte[byteLength];
					Array.Copy(BitConverter.GetBytes(i), values[c], 4);
					i++;
					count++;
				}
			}

			Console.WriteLine($"Inserted {count} values spread over {c} positions");
			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}

		[Test]
		public void Humanize_sequence_test()
		{
			// Test entropy by generating perfect random, and then change the order randomly
			// in order to simulate human usage of generated codes. To simulate usage, we only
			// we also only use 1% of the generated values in the test.
			// Markow says this should not degrade the entropy, however the volume of statistics
			// is not close to required. So this is more a "reality check" than anything else.

			var values = GenerateBaseValues();

			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");

			var random = new Random();
			byte[][] humanOrderedValues = new byte[(int)(values.Length * 0.01)][];
			for (int i = 0; i < humanOrderedValues.Length;)
			{
				int j = random.Next(values.Length);
				if (values[j] == null) continue; // we already used this value

				humanOrderedValues[i] = values[j];
				values[j] = null;

				i++;
			}

			Console.WriteLine($"Monobit: Passed {CalculateFipsMonobitTest(humanOrderedValues).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {CalculateFipsPokerTest(humanOrderedValues).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}
	}
}