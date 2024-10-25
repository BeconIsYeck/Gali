using Microsoft.Maui.Controls.Shapes;
using Gali.Drawables;


namespace Gali;

class Planet {
	public string Name { get; set; } = "Earth";
	public ContentPage ContentPage { get; set; }

	public double MoonDistance { get; set; } = 10000000000000;
	public double TidalStrength { get; set; } = 1_000;

	public double TimeRate { get; set; } = 1;
	public double ChangeRate { get; set; } = 0.000002;
	public Ocean Ocean { get; set; } = new Ocean();
	public List<Population?> Populations { get; set; } = new List<Population?>();
	public int PlatePrecision { get; set; } = 3;
	public int NumberOfPlates { get; set; } = 11;
	public (int, int) PlateAmount { get; set; } = (20, 40);
	public double PlateDrift { get; set; } = 0.00000135;
	public List<Coordinate> PlatePoints { get; set; } = new List<Coordinate>();
	public List<Plate> Plates { get; set; }
	public Factor[,] Factors { get; set; }

	public int Year { get; set; }
	public int Seed { get; set; } = 0;
	public double Greenhouse { get; set; } = 1;
	public double GreenhouseMin { get; set; } = -0.04;
	public double GreenhouseMax { get; set; } = 0.04;
	public bool Warm { get; set; } = false;
	public double SwapChance { get; set; } = 0.1;

	public static LinearGradientBrush GeneralGradientBrush = new LinearGradientBrush {
		StartPoint = new Point(0, 0),
		EndPoint = new Point(1, 1),
		GradientStops = [
			new GradientStop { Color = Colors.SkyBlue, Offset = 1.0f },
			new GradientStop { Color = Colors.Orchid, Offset = 0.5f },
			new GradientStop { Color = Colors.Aquamarine, Offset = 0.0f },

		],
	};

	static List<(double X1, double Y1, double X2, double Y2)>? Lines = null;
	List<List<Line>> LineGroups = new List< List<Line> >();

	#region Extra
	public static void DrawPoint(Coordinate coord, int width, AbsoluteLayout layout, Rect showRect, (double, double) scale, Color color1, Color color2, int zIndex = 1) {
		var ellipse = new Ellipse() {
			WidthRequest = width,
			HeightRequest = width,
			Fill = new RadialGradientBrush {
				Center = new Point(0.5, 0.5),
				Radius = 0.5,
				GradientStops = [
						new GradientStop { Color = color1, Offset = 0 },
						new GradientStop { Color = color1, Offset = 0.25f },
						new GradientStop { Color = color2, Offset = 1 }
					]
			},
			ZIndex = zIndex
		};

		var pf = coord.ToPointF(showRect, scale);

		layout.SetLayoutBounds(ellipse, new Rect(pf.X - width / 2, pf.Y - width / 2, width, width));
		layout.Children.Add(ellipse);
	}

	public static (List<Coordinate>, List<Coordinate>, List<Coordinate>, List<Coordinate>) DivideIntoQuadrants(List<Coordinate> coordinates) {
		var midX = 0;
		var midY = 0;

		var q1 = new List<Coordinate>();
		var q2 = new List<Coordinate>();
		var q3 = new List<Coordinate>();
		var q4 = new List<Coordinate>();

		foreach (var coordinate in coordinates) {
			var x = coordinate.Longitude;
			var y = coordinate.Latitude;

			if (x < midX && y < midY)
				q1.Add(coordinate);
			else if (x >= midX && y < midY)
				q2.Add(coordinate);
			else if (x < midX && y >= midY)
				q3.Add(coordinate);
			else if (x >= midX && y >= midY)
				q4.Add(coordinate);
		}

		return (q1, q2, q3, q4);
	}

	public static (List<(double X1, double Y1, double X2, double Y2)>, List<(double X1, double Y1, double X2, double Y2)>, List<(double X1, double Y1, double X2, double Y2)>, List<(double X1, double Y1, double X2, double Y2)>) DivideLinesIntoQuadrants(List<(double X1, double Y1, double X2, double Y2)> lines, Rect showRect, (double w, double h) scale) {
		var width = showRect.Width * scale.w;
		var height = showRect.Height * scale.h;

		var midX = (width / 2) + showRect.X;
		var midY = (height/ 2) + showRect.Y;

		var q1 = new List<(double X1, double Y1, double X2, double Y2)>();
		var q2 = new List<(double X1, double Y1, double X2, double Y2)>();
		var q3 = new List<(double X1, double Y1, double X2, double Y2)>();
		var q4 = new List<(double X1, double Y1, double X2, double Y2)>();

		foreach (var line in lines) {
			(var x, var y) = MathX.Lerp2(line.X1, line.Y1, line.X2, line.Y2, 0.5);

			if (x < midX && y < midY)
				q1.Add(line);
			else if (x >= midX && y < midY)
				q2.Add(line);
			else if (x < midX && y >= midY)
				q3.Add(line);
			else if (x >= midX && y >= midY)
				q4.Add(line);
		}

		return (q1, q2, q3, q4);
	}

	public static List<Coordinate> MergeQuadrants(List<Coordinate> p1, List<Coordinate> p2, List<Coordinate> p3, List<Coordinate> p4) {
		var merged = new List<Coordinate>();

		merged.AddRange(p1);
		merged.AddRange(p2);
		merged.AddRange(p3);
		merged.AddRange(p4);

		return merged;
	}

	public static List<(Factor, double)> GetFactorsInPolygon(Coordinate center, List<Coordinate> coords, Factor[,] factors) {
		var factorsList = new List<(Factor factor, double distance)>();

		foreach (var coord in coords) {
			var (x, y) = coord.ToAbsoluteSpace();

			if (x >= 0 && x < factors.GetLength(0) && y >= 0 && y < factors.GetLength(1)) {
				var factor = factors[x, y];
				var distance = MathX.Distance(center, coord);
				factorsList.Add((factor, distance));
			}
		}

		factorsList.Sort((a, b) => a.distance.CompareTo(b.distance));

		return factorsList;
	}

	public static bool IsPointInPolygon(Coordinate[] polygon, Coordinate testPoint) {
		bool result = false;
		int j = polygon.Length - 1;

		for (int i = 0; i < polygon.Length; i++) {
			if (polygon[i].Latitude < testPoint.Latitude && polygon[j].Latitude >= testPoint.Latitude ||
				polygon[j].Latitude < testPoint.Latitude && polygon[i].Latitude >= testPoint.Latitude) {
				if (polygon[i].Longitude + (testPoint.Latitude - polygon[i].Latitude) /
				   (polygon[j].Latitude - polygon[i].Latitude) *
				   (polygon[j].Longitude - polygon[i].Longitude) < testPoint.Longitude) {
					result = !result;
				}
			}

			j = i;
		}

		return result;
	}

	public static void DisposeList<T>(ref List<T>? list) {
		list?.Clear();
		list = null;
	}

	static List<((double X1, double Y1, double X2, double Y2) A, (double X1, double Y1, double X2, double Y2) B)> GetLineInteractions(List<(double X1, double Y1, double X2, double Y2)> lines) {
		var li = new List<((double X1, double Y1, double X2, double Y2) A, (double X1, double Y1, double X2, double Y2) B)>();

		bool ccw((double, double) A, (double, double) B, (double, double) C) =>
			(C.Item2 - A.Item2) * (B.Item1 - A.Item1) > (B.Item2 - A.Item2) * (C.Item1 - A.Item1);

		bool getIntersect((double X1, double Y1, double X2, double Y2) l1, (double X1, double Y1, double X2, double Y2) l2) {
			var A = (l1.X1, l1.Y1);
			var B = (l1.X2, l1.Y2);
			var C = (l2.X1, l2.Y1);
			var D = (l2.X2, l2.Y2);

			return ccw(A, C, D) != ccw(B, C, D) && ccw(A, B, C) != ccw(A, B, D);
		}

		for (var i = 0; i < lines.Count; i++) {
			for (var j = i + 1; j < lines.Count; j++) {
				//li = (getIntersect(lines[i], lines[j])) ? li + 1 : li;
				if (getIntersect(lines[i], lines[j])) {
					li.Add((lines[i], lines[j]));
				}
			}
		}

		return li;
	}

	public void GenerateFactors(ref Factor[,] factors) {
		var data = new double[factors.GetLength(0), factors.GetLength(1)];

		var noise = new FastNoiseLite(Seed);
		noise.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2S);
		noise.SetFractalType(FastNoiseLite.FractalType.FBm);

		for (var x = 0; x < factors.GetLength(0); x++) {
			for (var y = 0; y < factors.GetLength(1); y++) {
				var factor = new Factor();

				var lerpT = Math.Abs(y - 90) / 90.0;

				//factor.Temperature = MathX.SigmoidLerp1(Ocean.PolarTemperature, Ocean.EquatorialTemperature, lerpT, 100, 3);

				factor.Temperature = Math.Pow(MathX.ExponentialLerp1(Ocean.PolarTemperature, Ocean.EquatorialTemperature, lerpT, 1.5), 1.025);

				factor.Salinity = Ocean.Salinity;
				factor.Oxygen = Ocean.SurfaceOxygen;
				factor.Calcium = Ocean.Calcium;
				factor.Nitrogen = Ocean.Nitrogen;
				factor.Acidity = Ocean.Acidity;

				factor.Depth = Ocean.AverageDepth * Math.Pow((noise.GetNoise(x, y) + 1) / 1.25, 6);

				factors[x, y] = factor;
			}
		}
	}

	public List<(Factor, double, int X, int Y)> GetFactorsWithinRadius(Coordinate coord, double radius) {
		List<(Factor factor, double distance, int, int)> withinRad = new List<(Factor factor, double distance, int, int)>();

		var center = coord.ToAbsoluteSpace();

		var centerX = center.X;
		var centerY = center.Y;
		var rad = (int)radius;

		for (var y = Math.Max(0, centerY - rad); y < Math.Min(Factors.GetLength(1), centerY + rad); y++) {
			for (var x = Math.Max(0, centerX - rad); x < Math.Min(Factors.GetLength(0), centerX + rad); x++) {
				var distance = Math.Sqrt((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY));

				if (distance <= rad) {
					withinRad.Add((Factors[x, y], distance, x, y));
				}
			}
		}

		withinRad.Sort((a, b) => a.distance.CompareTo(b.distance));

		return withinRad;
	}
	#endregion

	public Planet(string name, int seed, ContentPage contentPage, int populations, double eqTemp, double poleTemp, int numOfPlt, (int, int) pltAm, Rect showRect, double scaleW, double scaleH) {
		Name = name;
		ContentPage = contentPage;

		Year = 0;
		Seed = seed;

		NumberOfPlates = numOfPlt;
		PlateAmount = pltAm;
		Plates = new List<Plate>();

		var factors = new Factor[360, 180];

		GenerateFactors(ref factors);

		Factors = factors;

		MathX.SetSeed(Seed);

		/* Plates */

		var pPAM = MathX.RandomInt(PlateAmount.Item1, PlateAmount.Item2);

		for (var i = 0; i < pPAM; i++) {
			var point = new Coordinate(MathX.RandomInt(-180, 180), MathX.RandomInt(-90, 90));
			PlatePoints.Add(point);
		}

		(var q1, var q2, var q3, var q4) = DivideIntoQuadrants(PlatePoints);

		Plates.Add(new Plate(q1, MathX.RandomInt(0, 360), MathX.RandomDouble(0, PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		Plates.Add(new Plate(q2, MathX.RandomInt(0, 360), MathX.RandomDouble(0, PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		Plates.Add(new Plate(q3, MathX.RandomInt(0, 360), MathX.RandomDouble(0, PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		Plates.Add(new Plate(q4, MathX.RandomInt(0, 360), MathX.RandomDouble(0, PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));

		/* Factors */

		/* Activities */

		/* Populations */

		var producersNeeded = (int)Math.Ceiling((double)populations / 2) + 1;

		for (var i = 0; i < producersNeeded; i++) {
			var center = new Coordinate(MathX.RandomDouble(-180, 180), MathX.RandomDouble(-90, 90));

			var cF = Factor.FindFactorFromCoordinate(center, Factors);

			center.SetDepth(MathX.RandomDouble(0, Math.Clamp(Ocean.LastLight, 0, cF.Depth)), cF);
			var plant = Population.CreateProducter(this, center, Population.ProducerType.Plant);

			center.SetDepth(MathX.RandomDouble(0, cF.Depth), cF);
			var bacteria = Population.CreateProducter(this, center, Population.ProducerType.Bacteria);

			center.SetDepth(MathX.RandomDouble(0, Math.Clamp(Ocean.LastLight + 50, 0, cF.Depth)), cF);
			var plankton = Population.CreateProducter(this, center, Population.ProducerType.Plankton);

			center.SetDepth(MathX.RandomDouble(0, Math.Clamp(Ocean.AverageDepth, 0, cF.Depth)), cF);
			var charnia = Population.CreateProducter(this, center, Population.ProducerType.Charnia);

			Populations.Add(plant);
			Populations.Add(plankton);
			Populations.Add(bacteria);
			Populations.Add(charnia);
		}


		for (var i = 0; i < populations; i++) {
			var center = new Coordinate(MathX.RandomDouble(-180, 180), MathX.RandomDouble(-90, 90));
			var cF = Factor.FindFactorFromCoordinate(center, Factors);

			center.SetDepth(MathX.RandomDouble(0, cF.Depth), cF);

			var pop = new Population();
			pop.Planet = this;
			pop.Center = center;
			pop.Radius = MathX.RandomInt(15, 50);

			pop.Power = Math.Floor(MathX.RandomDouble(1, 90));
			pop.Defense = Math.Floor(MathX.RandomDouble(1, 90));
			pop.Mobility = Math.Floor(MathX.RandomDouble(1, 90));
			pop.Stealth = Math.Floor(MathX.RandomDouble(1, 90));
			pop.Sight = Math.Floor(MathX.RandomDouble(1, 90));

			pop.Clade = (Taxonomy.Clade)MathX.RandomInt(0, 9);
			pop.Diet = (Population.TrophicSrategy)MathX.RandomInt(2, 7);

			pop.MassGrams = MathX.RandomDouble(1, 10_000);
			pop.Offspring = new Range(5, 10);

			pop.Color = MathX.RandomColor(true, true, true, false, 0.3, 1);
			var alive = pop.Form(MathX.RandomDouble(0.1, 0.7));

			if (alive)
				Populations.Add(pop);
		}
	}

	public void Show(AbsoluteLayout layout, Rect showRect, (double W, double H) scale, Toggles toggles, Dictionary<string, bool> factorSToggles) {
		var scaleW = scale.Item1;
		var scaleH = scale.Item2;

		layout.Children.Clear();

		/* Labels */

		var lblBorder = new Border {
			WidthRequest = 560,
			HeightRequest = 35,

			Background = GeneralGradientBrush,
			StrokeShape = new RoundRectangle {
				CornerRadius = 1024,
			},
		};

		var lbl = new Label {
			Text = "arg",
			FontSize = 15,

			TextColor = Colors.Black,

			Background = Colors.Transparent,
		};

		/* Background */

		layout.SetLayoutBounds(lblBorder, new Rect( ((showRect.Width * scaleW) / 2 - 25), 49, lblBorder.WidthRequest, lblBorder.HeightRequest));
		layout.SetLayoutBounds(lbl, new Rect((showRect.Width * scaleW) / 2 , 55, lblBorder.WidthRequest, lblBorder.HeightRequest));

		var bgST = 10;

		var bgBorder = new Border {
			WidthRequest = ((showRect.Width * scaleW) + (bgST * 2)) + (showRect.X - bgST),
			HeightRequest = ((showRect.Height * scaleH) + (bgST * 2)),

			Background = GeneralGradientBrush,

			Stroke = new SolidColorBrush(Colors.Black),
			StrokeThickness = 0,

			StrokeShape = new RoundRectangle {
				CornerRadius = new CornerRadius(0, 8, 0, 8),
			}
		};

		var background = new Border {
			WidthRequest  = showRect.Width  * scaleW,
			HeightRequest = showRect.Height * scaleH,

			StrokeThickness = 0,

			StrokeShape = new RoundRectangle {
				CornerRadius = 8
			}
		};

		background.Background = new LinearGradientBrush {
			StartPoint = new Point(0.5, 0),
			EndPoint = new Point(0.5, 1),
			GradientStops = [
				new GradientStop { Color = Colors.RoyalBlue, Offset = 0.0f },
				new GradientStop { Color = Colors.MediumBlue, Offset = 0.1f },
				new GradientStop { Color = Colors.DarkBlue, Offset = 0.4f },
				new GradientStop { Color = Colors.DarkBlue, Offset = 0.6f },
				new GradientStop { Color = Colors.MediumBlue, Offset = 0.9f },
				new GradientStop { Color = Colors.RoyalBlue, Offset = 1.0f }
			],
		};

		layout.SetLayoutBounds(background, new Rect(showRect.X, showRect.Y, background.WidthRequest, background.HeightRequest));
		layout.SetLayoutBounds(bgBorder, new Rect(0, showRect.Y - bgST, bgBorder.WidthRequest, bgBorder.HeightRequest));

		layout.Children.Add(bgBorder);
		layout.Children.Add(background);

		/* Factors */

		if (toggles.Factors) {
			var factorsDraw = new Factors(Factors, showRect, scale, this, factorSToggles);

			var fGView = new GraphicsView {
				WidthRequest = 2000,
				HeightRequest = 2000,
				BackgroundColor = Colors.Transparent,
			};

			fGView.Drawable = factorsDraw;

			layout.Children.Add(fGView);
		}

		/* Tectonic Plates */

		var lines = new List<(double X1, double Y1, double X2, double Y2)>();

		for (var i = 0; i < PlatePoints.Count; i++) {
			var p0 = PlatePoints[i];

			lbl.Text += $" {p0.Longitude}, {p0.Latitude} ";

			var disl = new List<(Coordinate, double)>();
			var threshold = 100_000_000_000;

			for (var j = 0; j < PlatePoints.Count; j++) { // SORT N NEAREST
				var p1 = PlatePoints[j];

				if (p1.Longitude == p0.Longitude && p1.Latitude == p0.Latitude)
					continue;

				var dis = MathX.Distance(p0, p1);

				if (dis <= threshold)
					disl.Add((p1, dis));
			}

			var nearest = disl.OrderBy(t => t.Item2).Take(PlatePrecision).ToList();

			var pf0 = p0.ToPointF(showRect, scale);

			foreach (var n in nearest) {
				var near = n.Item1;

				var nearpf = near.ToPointF(showRect, scale);

				var newLine = (pf0.X, pf0.Y, nearpf.X, nearpf.Y);

				lines.Add(newLine);
			}
		}

		var uniqueLines = new List<(double X1, double Y1, double X2, double Y2)>();

		var uniqueLinesSet = new HashSet<string>();

		foreach (var line in lines) {
			var normalizedLine = line.X1 < line.X2 || line.Y1 < line.Y2
				? $"{line.X1},{line.Y1},{line.X2},{line.Y2}"
				: $"{line.X2},{line.Y2},{line.X1},{line.Y1}";

			if (uniqueLinesSet.Add(normalizedLine)) {
				uniqueLines.Add(line);
			} 
		}

		if (toggles.PlateLines) {
			var linesDraw = new Lines(uniqueLines, showRect, scale, this);

			var lGView = new GraphicsView {
				WidthRequest = 2000,
				HeightRequest = 2000,
				BackgroundColor = Colors.Transparent,
			};

			lGView.Drawable = linesDraw;
			layout.Children.Add(lGView);
		}

		lbl.Text = $"PlatePoints: {PlatePoints.Count} / Calculated Boundaries: {lines.Count} / Rendered Boundaries: {uniqueLines.Count}";

		layout.Children.Add(lblBorder);
		layout.Children.Add(lbl);

		if (toggles.PlatePoints) {

			for (var i = 0; i < Plates.Count; i++) {
				var plate = Plates[i];

				var color1 = Colors.Transparent;
				var color2 = Colors.Transparent;

				switch (i) {
					case 0:
						color1 = Colors.Brown;
						color2 = Colors.Red;

						break;
					case 1:
						color1 = Colors.Lime;
						color2 = Colors.Green;

						break;
					case 2:
						color1 = Colors.MediumOrchid;
						color2 = Colors.Purple;

						break;
					case 3:
						color1 = Colors.DarkGray;
						color2 = Colors.Black;

						break;
				}

				foreach (var p in plate.Points) {
					DrawPoint(p, 15, layout, showRect, scale, color1, color2);

				}
			}
		}

		/* Factors */

		/* Populations */

		/* Cleanup! */

		DisposeList(ref Lines);

		Lines = new List<(double X1, double Y1, double X2, double Y2)>(lines);

		DisposeList(ref lines);
		DisposeList(ref uniqueLines);
	}

	public void Step(int steps, AbsoluteLayout layout, Rect showRect, (double, double) scale, Toggles toggles, Dictionary<string, bool> factorSToggles, bool nothing = false) {
		var lbl = new Label() {
			Text = "",
			FontSize = 20,
		};

		var lbl2 = new Label() {
			Text = "",
			FontSize = 14,
		};

		var scaleW = scale.Item1;
		var scaleH = scale.Item2;

		layout.SetLayoutBounds(lbl, new Rect(((showRect.Width * scaleW) / 2) - 10, showRect.Height * scaleH + showRect.Y + 20, 1800, 1800));
		layout.SetLayoutBounds(lbl2, new Rect( 6 , showRect.Y, 600, 1800));

		if (!nothing) {
			layout.Children.Clear();

			var yearsPassed = (int)TimeRate * steps;

			Year += yearsPassed;

			/* Tectonic Plate Movement */

			for (var i = 0; i < Plates.Count; i++) {
				var plate = Plates[i];

				var px = Math.Cos(plate.Direction * (Math.PI / 180));
				var py = Math.Sin(plate.Direction * (Math.PI / 180));

				var change = (TimeRate * steps) * plate.Drift;

				foreach (var p in plate.Points) {
					p.MoveUp(py * change);
					p.MoveRight(px * change);
				}

				plate.Direction += MathX.RandomDouble(-change, change);
			}

			Show(layout, showRect, scale, toggles, factorSToggles);
			Greenhouse = Warm ? 1.001 : 0.999;

			if (MathX.RandomDouble(0, 1) <= SwapChance) {
				Warm = !Warm;
			}

			/* Activities */
			#region Activities
			if (Lines != null) {
				(var q1, var q2, var q3, var q4) = DivideLinesIntoQuadrants(Lines, showRect, scale);

				var l1 = GetLineInteractions(q1);
				var l2 = GetLineInteractions(q2);
				var l3 = GetLineInteractions(q3);
				var l4 = GetLineInteractions(q4);

				var avg = (l1.Count + l2.Count + l3.Count + l4.Count) / 4;

				var l1Options = Activity.ChooseBestOptions(l1, avg, 0, showRect, scale, yearsPassed);
				var l2Options = Activity.ChooseBestOptions(l2, avg, 1, showRect, scale, yearsPassed);
				var l3Options = Activity.ChooseBestOptions(l3, avg, 2, showRect, scale, yearsPassed);
				var l4Options = Activity.ChooseBestOptions(l4, avg, 3, showRect, scale, yearsPassed);

				foreach ((var act, var plateIndex, var intensity) in l1Options) {
					lbl.Text += $"Top-Left: {act.GetType().Name}, {Math.Round(intensity, 3)}";

					if (MathX.RandomDouble(0, 1) <= act.Chance) {
						act.Function(this, plateIndex, intensity);

						if (toggles.Activities)
							act.Draw(layout, showRect, scale);

						lbl.Text += " !";
					}

					lbl.Text += "\n";
				}

				foreach ((var act, var plateIndex, var intensity) in l2Options) {
					lbl.Text += $"Top-Right: {act.GetType().Name}, {Math.Round(intensity, 3)}";

					if (MathX.RandomDouble(0, 1) <= act.Chance) {
						act.Function(this, plateIndex, intensity);

						if (toggles.Activities)
							act.Draw(layout, showRect, scale);

						lbl.Text += " !";
					}

					lbl.Text += "\n";
				}

				foreach ((var act, var plateIndex, var intensity) in l3Options) {
					lbl.Text += $"Bottom-Left Plate: {act.GetType().Name}, {Math.Round(intensity, 3)}";

					if (MathX.RandomDouble(0, 1) <= act.Chance) {
						act.Function(this, plateIndex, intensity);

						if (toggles.Activities)
							act.Draw(layout, showRect, scale);

						lbl.Text += " !";
					}

					lbl.Text += "\n";
				}

				foreach ((var act, var plateIndex, var intensity) in l4Options) {
					lbl.Text += $"Bottom-Right: {act.GetType().Name}, {Math.Round(intensity, 3)}";

					if (MathX.RandomDouble(0, 1) <= act.Chance) {
						act.Function(this, plateIndex, intensity);

						if (toggles.Activities)
							act.Draw(layout, showRect, scale);

						lbl.Text += " !";
					}

					lbl.Text += "\n";
				}
			}
			#endregion

			/* Factors */

			for (var x = 0; x < Factors.GetLength(0); x++) {
				for (var y = 0; y < Factors.GetLength(1); y++) {
					var factor = Factors[x, y];

					factor.Temperature = Math.Pow(factor.Temperature, Greenhouse);
				}
			}

			foreach (var pop in Populations) {
				pop?.Procede(yearsPassed);

				if (factorSToggles["Populations"]) {
					var cStr = pop?.Clade.ToString();

					if (factorSToggles.ContainsKey(cStr)) {
						if (factorSToggles[cStr]) {
							lbl2.Text += $"{pop?.Clade.ToString()} ({(pop?.ID)})\n";
						}
					}
				}
			}
		}
		else {
			foreach (var pop in Populations) {
				if (factorSToggles["Populations"]) {
					var cStr = pop?.Clade.ToString();

					if (factorSToggles.ContainsKey(cStr)) {
						if (factorSToggles[cStr]) {
							lbl2.Text += $"{pop?.Clade.ToString()} ({(pop?.ID)})\n";
						}
					}
				}
			}
		}

		layout.Children.Add(lbl);
		layout.Children.Add(lbl2);
	}

	public int Return() {
		return 0;
	}

	public void Exit() {

	}
}

#region maybe useful comments for later

//	var tree = new KDTree(2);
//	Node? root = null;

//	foreach (var p1 in PlatePoints) {
//		if (p0.Longitude == p1.Longitude && p0.Longitude == p1.Latitude)
//			continue;

//		root = tree.Insert(root, p1);
//	}

//	var nearest = tree.FindNearest(root, p0, PlatePrecision);


//var boundaries = new Microsoft.Maui.Controls.Shapes.Path {
//	Stroke = new SolidColorBrush(Colors.Yellow),
//	StrokeThickness = 2,
//};

//var bGeo = new PathGeometry();
//PathFigure? bFig = null;


//var sd0xy = sdisl[0].Item1.ToXY(showRect, scaleW, scaleH);
//var sd1xy = sdisl[1].Item1.ToXY(showRect, scaleW, scaleH);

//var l0 = new Line() {
//	X1 = p0xy.Item1.X, Y1 = p0xy.Item1.Y,
//	X2 = sd0xy.Item1.X, Y2 = sd0xy.Item1.Y,
//	Stroke = new SolidColorBrush(Colors.Yellow),
//	StrokeThickness = 2,
//};

//var l1 = new Line() {
//	X1 = p0xy.Item1.X, Y1 = p0xy.Item1.Y,
//	X2 = sd1xy.Item1.X, Y2 = sd1xy.Item1.Y,
//	Stroke = new SolidColorBrush(Colors.Yellow),
//	StrokeThickness = 2,
//};

//var morty = new Ellipse() {
//	WidthRequest = 10,
//	HeightRequest = 10,
//	Fill = new SolidColorBrush(Colors.Red),
//};
//var mortyC = new Coordinate(00, 00);

//void a() {
//	while (true) {
//		MainThread.BeginInvokeOnMainThread(() => {
//			mortyC.Latitude -= 10;

//			var x = 10;

//			var pfa = mortyC.ToXY(showRect, scaleW, scaleH);
//			var pf = pfa.Item1;

//			//nicelbl.Text = $"({pf.X}, {pf.Y}) | ({mortyC.Longitude}, {mortyC.Latitude} / {pfa.Item2} {pfa.Item2}";

//			layout.SetLayoutBounds(morty, new Rect(pf.X - (x / 2), pf.Y - (x / 2), morty.Width, morty.Height));
//		});

//		Thread.Sleep((int)Math.Ceiling(1000 / (double)30));
//	}
//}

//var b = new Thread(new ThreadStart(a));

//b.Start();

//layout.Children.Add(morty);

//if (bFig != null) {
//	bGeo.Figures.Add(bFig);
//}

//boundaries.Data = bGeo;

//layout.Children.Add(boundaries);

//layout.Children.Add(l0);
//layout.Children.Add(l1);

//var pfa = sdisl[0].Item1.ToXY(showRect, scaleW, scaleH);
//var pf = pfa.Item1;

//var pfa2 = sdisl[1].Item1.ToXY(showRect, scaleW, scaleH);
//var pf2 = pfa2.Item1;


//if (bFig == null) {
//	bFig = new PathFigure {
//		StartPoint = new Microsoft.Maui.Graphics.Point(pf.X, pf.Y),
//	};
//}
//else {
//	bFig.Segments.Add(new LineSegment {
//		Point = new Microsoft.Maui.Graphics.Point(pf.X, pf.Y),
//	});
//}

//if (bFig == null) {
//	bFig = new PathFigure {
//		StartPoint = new Microsoft.Maui.Graphics.Point(pf2.X, pf2.Y),
//	};
//}
//else {
//	bFig.Segments.Add(new LineSegment {
//		Point = new Microsoft.Maui.Graphics.Point(pf2.X, pf2.Y),
//	});
//}

//	var morty = new Ellipse() {
//		WidthRequest = 10,
//		HeightRequest = 10,
//		Fill = new SolidColorBrush(Colors.Red),
//	};
//var mortyC = new Coordinate(00, 00);

//void a() {
//	while (true) {
//		MainThread.BeginInvokeOnMainThread(() => {
//			mortyC.Latitude -= 10;

//			var x = 10;

//			var pfa = mortyC.ToXY(showRect, scaleW, scaleH);
//			var pf = pfa.Item1;

//			//nicelbl.Text = $"({pf.X}, {pf.Y}) | ({mortyC.Longitude}, {mortyC.Latitude} / {pfa.Item2} {pfa.Item2}";

//			layout.SetLayoutBounds(morty, new Rect(pf.X - (x / 2), pf.Y - (x / 2), morty.Width, morty.Height));
//		});

//		Thread.Sleep((int)Math.Ceiling(1000 / (double)30));
//	}
//}

//var b = new Thread(new ThreadStart(a));

//	b.Start();

//	layout.Children.Add(morty);
#endregion 
// end
