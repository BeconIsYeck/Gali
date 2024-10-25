namespace Gali.Drawables;

class Factors : IDrawable {
	public Factor[,] factors;
	public Rect showRect;
	public (double W, double H) scale;
	public Planet planet;
	Dictionary<string, bool> factorSToggles;

	public Factors(Factor[,] factors, Rect showRect, (double W, double H) scale, Planet planet, Dictionary<string, bool> factorSToggles) {
		this.factors = factors;
		this.showRect = showRect;
		this.scale = scale;
		this.planet = planet;
		this.factorSToggles = factorSToggles;
	}

	public void Draw(ICanvas canvas, RectF rect) {

		for (var i = 0; i < factors.GetLength(0); i++) {
			for (var j = 0; j < factors.GetLength(1); j++) {
				var factor = factors[i, j];

				var colors = new List<Color>();

				var r = 0.0;
				var g = 0.0;
				var b = 0.0;
				var a = 0.5;

				if (factorSToggles["Populations"]) {
					foreach (var p in factor.Populations) {
						var cStr = p.Clade.ToString();

						if (factorSToggles.ContainsKey(cStr)) {
							if (factorSToggles[cStr]) {
								colors.Add(p.Color);
							}
						}
					}
				}

				if (factorSToggles["Temperature"]) {
					var t = factor.Temperature;

					if (t >= 0 && t < 5) {
						colors.Add(Color.FromRgb(25, 50, 150)); 
					}
					else if (t >= 5 && t < 10) {
						colors.Add(Color.FromRgb(45, 160, 220)); // Blue
					}
					else if (t >= 10 && t < 15) {
						colors.Add(Color.FromRgb(40, 200, 150));
					}
					else if (t >= 15 && t < 20) {
						colors.Add(Color.FromRgb(60, 255, 95)); // Green
					}
					else if (t >= 20 && t < 25) {
						colors.Add(Color.FromRgb(255, 255, 0));
					}
					else if (t >= 25 && t < 27.5) {
						colors.Add(Color.FromRgb(225, 230, 0)); // Yellow
					}
					else if (t >= 27.5 && t < 30) {
						colors.Add(Color.FromRgb(255, 160, 0)); 
					}
					else if (t >= 30 && t < 35) {
						colors.Add(Color.FromRgb(255, 60, 0)); //Red
					}
					else if (t >= 35 && t < 40) {
						colors.Add(Color.FromRgb(255, 10, 220)); // Magenta
					}
					else if (t >= 40 && t <= 45) {
						colors.Add(Color.FromRgb(255, 255, 255)); // White
					}
					else {
						colors.Add(Color.FromRgb(0, 0, 0));
					}
				}

				if (factorSToggles["Depth"]) {
					colors.Add(Color.FromRgba(
						MathX.PercentDifference(planet.Ocean.AverageDepth, factor.Depth) / 100,
						MathX.PercentDifference(planet.Ocean.AverageDepth, factor.Depth) / 100,
						MathX.PercentDifference(planet.Ocean.AverageDepth, factor.Depth) / 100,
						1 - MathX.PercentDifference(planet.Ocean.AverageDepth, factor.Depth) / 100
					));
				}

				if (factorSToggles["Salinity"]) {
					colors.Add(Color.FromRgba(
						0,
						MathX.PercentDifference(planet.Ocean.AverageSalinity, factor.Salinity) / 100,
						0,
						1
					));
				}

				if (factorSToggles["Oxygen"]) {
					colors.Add(Color.FromRgba(
						0,
						MathX.PercentDifference(planet.Ocean.AverageSurfaceOxygen, factor.Oxygen) / 100,
						MathX.PercentDifference(planet.Ocean.AverageSurfaceOxygen, factor.Oxygen) / 100,
						1
					));
				}

				if (factorSToggles["Calcium"]) {
					colors.Add(Color.FromRgba(
						MathX.PercentDifference(planet.Ocean.AverageCalcium, factor.Calcium) / 100,
						MathX.PercentDifference(planet.Ocean.AverageCalcium, factor.Calcium) / 100,
						MathX.PercentDifference(planet.Ocean.AverageCalcium, factor.Calcium) / 100,
						1
					));
				}

				if (factorSToggles["Nitrogen"]) {
					colors.Add(Color.FromRgba(
						MathX.PercentDifference(planet.Ocean.AverageNitrogen, factor.Nitrogen) / 100,
						0,
						0,
						1
					));
				}

				if (factorSToggles["Acidity"]) {
					colors.Add(Color.FromRgba(
						MathX.PercentDifference(planet.Ocean.AverageAcidity, factor.Acidity) / 100,
						0,
						MathX.PercentDifference(planet.Ocean.AverageAcidity, factor.Acidity) / 100,
						1
					));
				}

				r = MathX.AverageColor(colors).Red;
				g = MathX.AverageColor(colors).Green;
				b = MathX.AverageColor(colors).Blue;

				a = factorSToggles["Full"] ? 1 : 0.5;

				var color = Color.FromRgba(
					r,
					g,
					b,
					a
				);

				var x = (float)(showRect.X + i * scale.W);
				var y = (float)(showRect.Y + j * scale.H);

				var w = (float)(scale.W > 0 ? scale.W : 1);
				var h = (float)(scale.H > 0 ? scale.H : 1);

				canvas.FillColor = color;
				canvas.FillRectangle(x, y, w, h);
			}
		}
	}
}

class Lines : IDrawable {
	public List<(double X1, double Y1, double X2, double Y2)> lines;
	public Rect showRect;
	public (double W, double H) scale;
	public Planet planet;

	public Lines(List<(double X1, double Y1, double X2, double Y2)> lines, Rect showRect, (double W, double H) scale, Planet planet) {
		this.lines = new List<(double X1, double Y1, double X2, double Y2)>(lines);
		this.showRect = showRect;
		this.scale = scale;
		this.planet = planet;
	}

	public void Draw(ICanvas canvas, RectF rect) {
		canvas.StrokeColor = Colors.Teal;
		canvas.StrokeSize = 3;

		foreach (var line in lines) {
			(var x1, var y1, var x2, var y2) = line;
			canvas.DrawLine((float)x1, (float)y1, (float)x2, (float)y2);
		}
	}
}