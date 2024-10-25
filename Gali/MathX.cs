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

	public static Coordinate Lerp2C(Coordinate a, Coordinate b, double t) =>
		new Coordinate( Lerp1(a.Longitude, b.Longitude, t), Lerp1(a.Latitude, b.Latitude, t) );

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
}