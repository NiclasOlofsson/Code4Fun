using System;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;

namespace Fips.Tests
{
	[TestFixture]
	public class EntropyTests
	{
		private const int byteLength = 5;
		private const int globalSeed = 666;

		private static byte[][] GenerateBaseValues()
		{
			//RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
			var rng = new Random(globalSeed); // We keep this constant to be able to repeat tests
			int sampleCount = 20_000;
			byte[][] values = new byte[sampleCount][];

			for (int i = 0; i < sampleCount; i++)
			{
				values[i] = new byte[byteLength];
				rng.NextBytes(values[i]);
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

			Console.WriteLine($"Monobit: Passed {MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength*8}");
			Console.WriteLine($"Poker:   Passed {MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength*8}");
			Console.WriteLine($"Runs:    Passed {MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength*8}");
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

			Console.WriteLine($"Monobit: Passed {MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Runs:    Passed {MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}

		[Test]
		public void RNGCrypto_generator_base_test()
		{
			var values = GenerateBaseValues();

			Console.WriteLine($"Monobit: Passed {MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength*8}");
			Console.WriteLine($"Block:   Passed {MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Runs:    Passed {MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}

		[Test]
		public void Embedded_full_sequence_in_order_detection_test()
		{
			var values = GenerateBaseValues();

			var monobitScore = MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01);
			var blockScore = MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01);
			var runsScore = MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01);

			int c = 0;
			int count = 0;
			for (int i = 0; i < 100; c++)
			{
				values[c] = new byte[byteLength];
				Array.Copy(BitConverter.GetBytes(i), values[c], 4);
				i++;
				count++;
			}

			Console.WriteLine($"Inserted {count} values spread over {c} positions");

			var monobitScoreAfter = MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01);
			var blockScoreAfter = MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01);
			var runsScoreAfter = MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01);

			Console.WriteLine($"Monobit: Passed {monobitScoreAfter} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {blockScoreAfter} bits out of total {byteLength * 8}");
			Console.WriteLine($"Runs:    Passed {runsScoreAfter} bits out of total {byteLength * 8}");

			Assert.Less(monobitScoreAfter, monobitScore);
			Assert.Less(blockScoreAfter, blockScore);
			Assert.Less(runsScoreAfter, runsScore);
		}

		[Test]
		public void Embedded_fragmented_sequence_in_order_detection_test()
		{
			var values = GenerateBaseValues();

			var monobitScore = MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01);
			var blockScore = MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01);
			var runsScore = MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01);

			var random = new Random();
			int c = 0;
			int count = 0;
			for (int i = 0; i < 500; c++)
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
			var monobitScoreAfter = MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01);
			var blockScoreAfter = MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01);
			var runsScoreAfter = MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01);

			Console.WriteLine($"Monobit: Passed {monobitScoreAfter} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {blockScoreAfter} bits out of total {byteLength * 8}");
			Console.WriteLine($"Runs:    Passed {runsScoreAfter} bits out of total {byteLength * 8}");

			Assert.Less(monobitScoreAfter, monobitScore);
			Assert.Less(blockScoreAfter, blockScore);
			Assert.Less(runsScoreAfter, runsScore);
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

			Console.WriteLine($"Monobit: Passed {MathAdvanced.FrequencyTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {MathAdvanced.BlockTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Runs:    Passed {MathAdvanced.RunsTest(values).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");

			var random = new Random(globalSeed);
			byte[][] humanOrderedValues = new byte[(int) (values.Length*0.01)][];
			for (int i = 0; i < humanOrderedValues.Length;)
			{
				int j = random.Next(values.Length);
				if (values[j] == null) continue; // we already used this value

				humanOrderedValues[i] = values[j];
				values[j] = null;

				i++;
			}

			Console.WriteLine($"Monobit: Passed {MathAdvanced.FrequencyTest(humanOrderedValues).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Poker:   Passed {MathAdvanced.BlockTest(humanOrderedValues).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
			Console.WriteLine($"Runs:    Passed {MathAdvanced.RunsTest(humanOrderedValues).Count(pVal => pVal.Value >= 0.01)} bits out of total {byteLength * 8}");
		}
	}
}