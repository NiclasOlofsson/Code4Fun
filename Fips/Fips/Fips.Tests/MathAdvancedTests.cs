using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;

namespace Fips.Tests
{
	/// <summary>
	/// Tests from the NIST standard: https://nvlpubs.nist.gov/nistpubs/legacy/sp/nistspecialpublication800-22r1a.pdf
	/// </summary>
	[TestFixture]
	public class MathAdvancedTests
	{
		[Test]
		public void frequency_test_should_match_standard()
		{
			byte[] bytes = GetBytes("1100100100001111110110101010001000100001011010001100001000110100110001001100011001100010100010111000");
			byte[][] values = new byte[bytes.Length][];
			for (int i = 0; i < bytes.Length; i++)
			{
				values[i] = new []{bytes[i]};
			}

			var result = MathAdvanced.FrequencyTest(values);

			Assert.AreEqual(100, bytes.Length);
			Assert.AreEqual(0.109599, Math.Round(result[0], 6));
		}

		public static byte[] GetBytes(string bitString)
		{
			return Enumerable.Range(0, bitString.Length).
				Select(pos => Convert.ToByte(
					bitString.Substring(pos, 1),
					2)
				).ToArray();
		}

		[Test]
		public void frequency_with_block_test_should_match_standard()
		{
			byte[] bytes = GetBytes("1100100100001111110110101010001000100001011010001100001000110100110001001100011001100010100010111000");
			byte[][] values = new byte[bytes.Length][];
			for (int i = 0; i < bytes.Length; i++)
			{
				values[i] = new[] { bytes[i] };
			}

			var result = MathAdvanced.BlockTest(values, 10);

			Assert.AreEqual(100, bytes.Length);
			Assert.AreEqual(0.706438, Math.Round(result[0], 6));
		}

		[Test]
		public void runs_test_should_match_standard()
		{
			byte[] bytes = GetBytes("1100100100001111110110101010001000100001011010001100001000110100110001001100011001100010100010111000");
			byte[][] values = new byte[bytes.Length][];
			for (int i = 0; i < bytes.Length; i++)
			{
				values[i] = new[] { bytes[i] };
			}

			var result = MathAdvanced.RunsTest(values);

			Assert.AreEqual(100, bytes.Length);
			Assert.AreEqual(0.500798, Math.Round(result[0], 6));
		}

	}
}