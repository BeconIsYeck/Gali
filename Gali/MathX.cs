using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Gali;

static class MathX {
	private static Random RND = new Random();

	public static void SetSeed(int seed) {
		RND = new Random(seed);
	}

	public static double Radians = Math.PI / 180;
	public static double Degrees = 180 / Math.PI;

	public static double Lerp1(double a, double b, double t) => (1 - t) * a + t * b;

	public static (double X, double Y) Lerp2(double x0, double y0, double x1, double y1, double t) => (Lerp1(x0, x1, t), Lerp1(y0, y1, t));

	public static (double, double, double) Lerp3(double a0, double a1, double a2, double b0, double b1, double b2, double t) =>
		(Lerp1(a0, b0, t), Lerp1(a1, b1, t), Lerp1(a2, b2, t));

	public static double ExponentialLerp1(double a, double b, double t, double s) {
		double t2 = Math.Pow(t, s);
		return (a * (1 - t2) + b * t2);
	}

	public static double SigmoidLerp1(double a, double b, double t, double sharpness, double factor) {
		double midpoint = 0.5;
		double sigmoid = 1 / (1 + Math.Exp(-sharpness * (t - midpoint)));

		double poleFactor = Math.Pow(t, factor); 

		return (a * (1 - poleFactor) + b * sigmoid);
	}

	public static Coordinate Lerp2C(Coordinate a, Coordinate b, double t) =>
		new Coordinate( Lerp1(a.Longitude, b.Longitude, t), Lerp1(a.Latitude, b.Latitude, t) );

	//public static Coordinate3D Lerp3C(Coordinate3D a, Coordinate3D b, double t) =>
	//	new Coordinate3D( Lerp1(a.Longitude, b.Longitude, t), Lerp1(a.Latitude, a.Latitude, t), Lerp1(a.Depth, b.Depth, t) );

	public static double CalculateAlpha(double a, double b, double c) => (c -a) / (b - a);

	public static Coordinate MidPoint(Coordinate a, Coordinate b) =>
		new Coordinate((a.Longitude + b.Longitude) / 2, (a.Latitude + b.Latitude) / 2);

	public static double Slope(Coordinate a, Coordinate b) =>
		(b.Latitude - a.Latitude) / (b.Longitude - a.Longitude);

	public static double Distance(Coordinate a, Coordinate b) =>
		Math.Sqrt(Math.Pow(a.Longitude - b.Longitude, 2) + Math.Pow(a.Latitude - b.Latitude, 2));

	public static double DistanceXY(double x1, double y1, double x2, double y2) =>
		Math.Sqrt(Math.Pow(x1 - x2, 2) + Math.Pow(y1 - y2, 2));


	public static double BeerLambert(double x, double i, double k) => i * Math.Pow(Math.E, -k * (x - 1));

	public static double RandomDouble(double min, double max) => RND.NextDouble() * (max - min) + min;

	public static int RandomInt(int min, int max) => RND.Next(min, max);

	public static T RandomItem<T>(List<T> list) {
		return list[RND.Next(list.Count)];
	}
	
	public static double PercentDifference(double benchmark, double variance) =>
		((variance - benchmark) / benchmark) * 100;

	public static Color AverageColor(List<Color> colors) {
		if (colors.Count == 0) return Colors.Transparent;

		double totalR = 0, totalG = 0, totalB = 0, totalA = 0;

		foreach (var color in colors) {
			totalR += color.Red;
			totalG += color.Green;
			totalB += color.Blue;
			totalA += color.Alpha;
		}

		int count = colors.Count;
		return new Color(
			(float)(totalR / count),
			(float)(totalG / count),
			(float)(totalB / count),
			(float)(totalA / count)
		);
	}

	public static Color RandomColor(bool red = false, bool green = false, bool blue = false, bool alpha = false, double min = 0, double max = 1, bool same = false) {
		if (same) {
			var all = RandomDouble(min, max);

			return Color.FromRgba(
				red   ? all : 0,
				green ? all : 0,
				blue  ? all : 0,
				alpha ? all : 1
			);
		}

		return Color.FromRgba(
			red   ? RandomDouble(min, max) : 0,
			green ? RandomDouble(min, max) : 0,
			blue  ? RandomDouble(min, max) : 0,
			alpha ? RandomDouble(min, max) : 1
		);
	}

	public static (double X, double Y) ApplyRotationDEG(double x, double y, double deg, double dis) =>
		(x + dis * Math.Cos(dis * Radians), y + dis * Math.Sin(dis * Radians));

	//public static double Gaussian(double x, double median, double sDeviation) {
	//	double exp = -Math.Pow(x - median, 2) / (2 * Math.Pow(sDeviation, 2));

	//	return (1 / (sDeviation * Math.Sqrt(2 * Math.PI))) * Math.Exp(exp);
	//}

	public static double Gaussian(double x, double median, double sDeviation) {
		double exponent = -Math.Pow(x - median, 2) / (2 * Math.Pow(sDeviation, 2));
		return (1 / (sDeviation * Math.Sqrt(2 * Math.PI))) * Math.Exp(exponent);
	}

	// Cumulative Distribution Function (CDF)
	public static double CDF(double x, double median, double sDeviation) {
		return 0.5 * (1 + Erf((x - median) / (Math.Sqrt(2) * sDeviation)));
	}

	// Error Function (Erf)
	public static double Erf(double x) {
		double a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741, a4 = -1.453152027, a5 = 1.061405429;
		double p = 0.3275911;
		double sign = Math.Sign(x);
		x = Math.Abs(x);

		double t = 1.0 / (1.0 + p * x);
		double y = 1.0 - (((((a5 * t + a4) * t) + a3) * t + a2) * t + a1) * t * Math.Exp(-x * x);

		return sign * y;
	}

	// Modified Gaussian function to find surviving population
	public static double SizePastThreshold(double threshold, double median, double sDeviation, double size) {
		double cdfThreshold = CDF(threshold, median, sDeviation);
		return (1 - cdfThreshold) * size;
	}

	public static double Integrate(Func<double, double> function, double start, double end, int steps = 1000) {
		double stepSize = (end - start) / steps;
		double area = 0.0;

		for (int i = 0; i < steps; i++) {
			double x1 = start + i * stepSize;
			double x2 = start + (i + 1) * stepSize;
			double y1 = function(x1);
			double y2 = function(x2);
			area += (y1 + y2) * stepSize / 2;
		}

		return area;
	}

	public static double FindFirstIntersection(double mu1, double sigma1, double mu2, double sigma2, double tolerance = 1e-6, double minX = -10, double maxX = 10) {
		Func<double, double> d1 = x => MathX.Gaussian(x, mu1, sigma1);
		Func<double, double> d2 = x => MathX.Gaussian(x, mu2, sigma2);

		Func<double, double> difference = x => d1(x) - d2(x);

		double a = minX, b = maxX;

		while (Math.Abs(b - a) > tolerance) {
			double mid = (a + b) / 2;
			if (difference(a) * difference(mid) < 0) {
				b = mid;
			}
			else {
				a = mid;
			}
		}

		return (a + b) / 2;
	}

	public static double FindIntersectionArea(double muPrey, double sigmaPrey, double muPredator, double sigmaPredator) {
		Func<double, double> preyDistribution = x => Gaussian(x, muPrey, sigmaPrey);
		Func<double, double> predatorDistribution = x => Gaussian(x, muPredator, sigmaPredator);

		// Approximate range for intersection (tweak this as necessary)
		double start = Math.Min(muPrey, muPredator) - 3 * Math.Max(sigmaPrey, sigmaPredator);
		double end = Math.Max(muPrey, muPredator) + 3 * Math.Max(sigmaPrey, sigmaPredator);

		// Integrate the overlapping area
		double intersectArea = Integrate(x => Math.Min(preyDistribution(x), predatorDistribution(x)), start, end);

		return intersectArea;
	}

	public static double AggregateIntersectionAreas(List<(double mu, double sigma)> distributions1, List<(double mu, double sigma)> distributions2) {
		double totalIntersectionArea = 0.0;

		foreach (var dist1 in distributions1) {
			foreach (var dist2 in distributions2) {
				double intersectArea = FindIntersectionArea(dist1.mu, dist1.sigma, dist2.mu, dist2.sigma);
				totalIntersectionArea += intersectArea;
			}
		}

		return totalIntersectionArea;
	}
}