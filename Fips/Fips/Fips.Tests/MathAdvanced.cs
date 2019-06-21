using System;

namespace Fips.Tests
{
	public class MathAdvanced
	{
		public static double ChiSquarePval(double x, int df)
		{
			// x = a computed chi-square value.
			// df = degrees of freedom.
			// output = prob. x value occurred by chance.
			// ACM 299.
			if (x <= 0.0 || df < 1)
				throw new Exception("Bad arg in ChiSquarePval()");
			double a = 0.0; // 299 variable names
			double y = 0.0;
			double s = 0.0;
			double z = 0.0;
			double ee = 0.0; // change from e
			double c;
			bool even; // Is df even?
			a = 0.5 * x;
			if (df % 2 == 0) even = true;
			else even = false;
			if (df > 1) y = Exp(-a); // ACM update remark (4)
			if (even == true) s = y;
			else s = 2.0 * Gauss(-Math.Sqrt(x));
			if (df > 2)
			{
				x = 0.5 * (df - 1.0);
				if (even == true) z = 1.0;
				else z = 0.5;
				if (a > 40.0) // ACM remark (5)
				{
					if (even == true) ee = 0.0;
					else ee = 0.5723649429247000870717135;
					c = Math.Log(a); // log base e
					while (z <= x)
					{
						ee = Math.Log(z) + ee;
						s = s + Exp(c * z - a - ee); // ACM update remark (6)
						z = z + 1.0;
					}
					return s;
				} // a > 40.0
				else
				{
					if (even == true) ee = 1.0;
					else
						ee = 0.5641895835477562869480795 / Math.Sqrt(a);
					c = 0.0;
					while (z <= x)
					{
						ee = ee * (a / z); // ACM update remark (7)
						c = c + ee;
						z = z + 1.0;
					}
					return c * y + s;
				}
			} // df > 2
			else
			{
				return s;
			}
		} // ChiSquarePval()

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
				p = 0.0;
			else
			{
				y = Math.Abs(z) / 2;
				if (y >= 3.0)
				{
					p = 1.0;
				}
				else if (y < 1.0)
				{
					w = y * y;
					p = ((((((((0.000124818987 * w
								- 0.001075204047) * w + 0.005198775019) * w
							- 0.019198292004) * w + 0.059054035642) * w
							- 0.151968751364) * w + 0.319152932694) * w
						- 0.531923007300) * w + 0.797884560593) * y
																* 2.0;
				}
				else
				{
					y = y - 2.0;
					p = (((((((((((((-0.000045255659 * y
									+ 0.000152529290) * y - 0.000019538132) * y
									- 0.000676904986) * y + 0.001390604284) * y
								- 0.000794620820) * y - 0.002034254874) * y
								+ 0.006549791214) * y - 0.010557625006) * y
							+ 0.011630447319) * y - 0.009279453341) * y
							+ 0.005353579108) * y - 0.002141268741) * y
						+ 0.000535310849) * y + 0.999936657524;
				}
			}
			if (z > 0.0)
				return (p + 1.0) / 2;
			else
				return (1.0 - p) / 2;
		} // Gauss()

		public static double Erfc(double x)
		{
			return 1 - Erf(x);
		}

		public static double Erf(double x)
		{
			// constants
			double a1 = 0.254829592;
			double a2 = -0.284496736;
			double a3 = 1.421413741;
			double a4 = -1.453152027;
			double a5 = 1.061405429;
			double p = 0.3275911;

			// Save the sign of x
			int sign = 1;
			if (x < 0)
				sign = -1;
			x = Math.Abs(x);

			// A&S formula 7.1.26
			double t = 1.0 / (1.0 + p * x);
			double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

			return sign * y;
		}
	}
}