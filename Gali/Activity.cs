using System.Reflection;

namespace Gali;

abstract class Activity {
	public virtual double Diameter { get; set; } = 5;
	public abstract Range LineInteraction { get; }
	public virtual double Chance { get; } = 1;
	public virtual int PerYears { get; } = 1_000_000;
	public virtual Color Color { get; } = Colors.LightGrey;

	public Coordinate Location { get; set; } = new Coordinate(0, 0);

	public static List<(Activity Activity, int PlateIndex, double Intensity)> ChooseBestOptions(List<((double X1, double Y1, double X2, double Y2) A, (double X1, double Y1, double X2, double Y2) B)> lineInteractions, int averageLI, int plateIndex, Rect showRect, (double W, double h) scale, int yearsPassed) {

		var li = lineInteractions.Count;

		var options = new List<(Activity Activity, int PlateIndex, double Intensity)>();

		List<Type>? subclasses = Assembly.GetAssembly(typeof(Activity))
								 .GetTypes()
								 .Where(t => t.IsSubclassOf(typeof(Activity)) && !t.IsAbstract)
								 .ToList();

		var perDif  = MathX.PercentDifference(averageLI, li);

		foreach (Type c in subclasses) {
			if (c.GetConstructor(Type.EmptyTypes) == null)
				continue;
			
			var inst = Activator.CreateInstance(c);
			var act = inst as Activity;

			var lir = act.LineInteraction;
			var chance = act.Chance;

			if (lir != null && lir.InRange(perDif)) {
				var grossAmount = Math.Abs(MathX.PercentDifference(act.PerYears, yearsPassed) / 100);
				int precAmount;

				if (grossAmount < 1) {
					precAmount = MathX.RandomDouble(0, 1) >= grossAmount ? (int)Math.Ceiling(grossAmount) : (int)Math.Floor(grossAmount);
				}
				else {
					precAmount = MathX.RandomDouble(0, 1) >= 0.5 ? (int)Math.Ceiling(grossAmount) : (int)Math.Floor(grossAmount);
				}

				var lirIntensity = MathX.CalculateAlpha(lir.Min, lir.Max, perDif);
				(var rA, var rB) = MathX.RandomItem(lineInteractions);

				var randL = MathX.RandomDouble(0, 1) >= 0.5 ? rA : rB;

				(var x, var y) = MathX.Lerp2(randL.X1, randL.Y1, randL.X2, randL.Y2, MathX.RandomDouble(0, 1));

				act.Location = Coordinate.XYToCoordinate(x, y, showRect, scale);

				for (var i = 0; i < precAmount; i++) {
					options.Add((act, plateIndex, lirIntensity));
				}
			}
		}

		return options;
	}

	public virtual void Draw(AbsoluteLayout layout, Rect showRect, (double W, double H) scale) {
		Planet.DrawPoint(Location, (int)(Diameter * ((scale.W + scale.H) / 2)), layout, showRect, scale, Color, Colors.Transparent, 2);
	}

	public abstract int Function(Planet planet, int plate, double intensity);

	double intensityToMultiplier(double intensity) => (Math.Sqrt(intensity) / 5) + 1;

	public static double scaleWithDis(double start, double dis, double rad) => (-(Math.Sqrt(dis / rad) / 5) + start);
}

/* /// -- Positive -- Positive -- Positive -- Positive -- Positive -- /// */

class SeafloorSpreading: Activity { // SeafloorSpreading
	public override Range LineInteraction { get; } = new Range(0, 5);
	public override double Chance { get; } = 0.5;
	public override Color Color { get; } = Colors.DodgerBlue;

	public SeafloorSpreading() {
		Diameter = MathX.RandomDouble(15, 25);
	}

	public override int Function(Planet planet, int plateIndex, double intensity) { // Increase Nitrgon and Calcium Lower Depth
		var factors = planet.GetFactorsWithinRadius(Location, Diameter / 2);

		foreach ((var factor, var dis, var rX, var rY) in factors) {
			factor.Depth *= scaleWithDis(0.98, dis, Diameter / 2);
			factor.Acidity *= scaleWithDis(0.99, dis, Diameter / 2);
		}

		return 0;
	}
}

class Hotspot : Activity { // Hotspot
	public override Range LineInteraction { get; } = new Range(20, 95);
	public override double Chance { get; } = 0.55;
	public override int PerYears { get; } = 500_000;
	public override Color Color { get; } = Colors.Orange;

	public Hotspot() {
		Diameter = MathX.RandomDouble(9, 25);
	}


	public override int Function(Planet planet, int plateIndex, double intensity) { // Increase Temperature and lower Depth
		var factors = planet.GetFactorsWithinRadius(Location, Diameter / 2);

		foreach ((var factor, var dis, var rX, var rY) in factors) {
			factor.Temperature *= scaleWithDis(1.125, dis, Diameter / 2);
			factor.Acidity *= scaleWithDis(1.075, dis, Diameter / 2);
		}

		return 0;
	}
}

class Tremor : Activity { // Tremor INCREASE DEPTH
	public override Range LineInteraction { get; } = new Range(80, 140);
	public override double Chance { get; } = 0.9;
	public override Color Color { get; } = Colors.DarkOliveGreen;

	public Tremor() {
		Diameter = MathX.RandomDouble(10, 35);
	}

	public override int Function(Planet planet, int plateIndex, double intensity) {
		var rnd = new Random();

		var factors = planet.GetFactorsWithinRadius(Location, Diameter / 2);

		foreach ((var factor, var dis, var rX, var rY) in factors) {
			factor.Oxygen *= scaleWithDis(0.995, dis, Diameter / 2);
			factor.Nitrogen *= scaleWithDis(1.125, dis, Diameter / 2);
			factor.Calcium *= scaleWithDis(0.99, dis, Diameter / 2);
			factor.Depth *= scaleWithDis(1.2, dis, Diameter / 2);

		}

		var pointChange = Math.Round((intensity) * 10, 0) * (MathX.RandomDouble(0, 1) >= 0.5 ? 1 : -1); // Get how many points will be removed / added (50 50 chance to add or remove)
		var sign = Math.Sign(pointChange);
		sign = sign == 0 ? 1 : sign;

		for (var i = 0; sign >= 0 ? i <= pointChange : i > pointChange; i += sign) {
			if (sign >= 0) { // If we have to add plates
				var px = 0;
				var py = 0;

				switch (plateIndex) { // Generate new point inside each possible quadrant
					case 0: px = rnd.Next(-180, 0); py = rnd.Next(0, 90);  break;
					case 1: px = rnd.Next(0, 180); py = rnd.Next(0, 90);   break;
					case 2: px = rnd.Next(-180, 0); py = rnd.Next(-90, 0); break;
					case 3: px = rnd.Next(0, 180); py = rnd.Next(-90, 0);  break;
				}

				planet.Plates[plateIndex].Points.Add(new Coordinate(px, py));
			}
			else { // If we have to remove plates
				var randInd = rnd.Next(0, planet.Plates[plateIndex].Points.Count);
				planet.Plates[plateIndex].Points.RemoveAt(randInd);
			}
		}

		planet.PlatePoints.Clear(); // Clear global points and then merge all the plates so the added / removed points update globally
		planet.PlatePoints = Planet.MergeQuadrants(planet.Plates[0].Points, planet.Plates[1].Points, planet.Plates[2].Points, planet.Plates[3].Points);

		planet.Plates.Clear(); // Now that the global points are updated, redraw plate boundaries

		(var q1, var q2, var q3, var q4) = Planet.DivideIntoQuadrants(planet.PlatePoints);

		planet.Plates.Add(new Plate(q1, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		planet.Plates.Add(new Plate(q2, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		planet.Plates.Add(new Plate(q3, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		planet.Plates.Add(new Plate(q4, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));

		return 0;
	}
}

class MantlePlume : Activity { // MantlePlume
	public override Range LineInteraction { get; } = new Range(110, 135);
	public override double Chance { get; } = 0.8;
	public override Color Color { get; } = Colors.OrangeRed;

	public MantlePlume() {
		Diameter = MathX.RandomDouble(15, 40);
	}

	public override int Function(Planet planet, int plateIndex, double intensity) {
		var rnd = new Random();

		var factors = planet.GetFactorsWithinRadius(Location, Diameter / 2);

		foreach ((var factor, var dis, var rX, var rY) in factors) {
			factor.Temperature *= scaleWithDis(1.25, dis, Diameter / 2);
			factor.Nitrogen *= scaleWithDis(0.99, dis, Diameter / 2);
			factor.Calcium *= scaleWithDis(1.05, dis, Diameter / 2);

		}

		var pointChange = Math.Round((intensity) * 10, 0) * (MathX.RandomDouble(0, 1) >= 0.5 ? 1 : -1); // Get how many points will be removed / added (50 50 chance to add or remove)
		var sign = Math.Sign(pointChange);
		sign = sign == 0 ? 1 : sign;

		for (var i = 0; sign >= 0 ? i <= pointChange : i > pointChange; i += sign) {
			if (sign >= 0) { // If we have to add plates
				var px = 0;
				var py = 0;

				switch (plateIndex) { // Generate new point inside each possible quadrant
					case 0: px = rnd.Next(-180, 0); py = rnd.Next(0, 90); break;
					case 1: px = rnd.Next(0, 180); py = rnd.Next(0, 90); break;
					case 2: px = rnd.Next(-180, 0); py = rnd.Next(-90, 0); break;
					case 3: px = rnd.Next(0, 180); py = rnd.Next(-90, 0); break;
				}

				planet.Plates[plateIndex].Points.Add(new Coordinate(px, py));
			}
			else { // If we have to remove plates
				var randInd = rnd.Next(0, planet.Plates[plateIndex].Points.Count);

				planet.Plates[plateIndex].Points.RemoveAt(randInd);
			}
		}

		planet.PlatePoints.Clear(); // Clear global points and then merge all the plates so the added / removed points update globally
		planet.PlatePoints = Planet.MergeQuadrants(planet.Plates[0].Points, planet.Plates[1].Points, planet.Plates[2].Points, planet.Plates[3].Points);

		planet.Plates.Clear(); // Now that the global points are updated, redraw plate boundaries

		(var q1, var q2, var q3, var q4) = Planet.DivideIntoQuadrants(planet.PlatePoints);

		planet.Plates.Add(new Plate(q1, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		planet.Plates.Add(new Plate(q2, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		planet.Plates.Add(new Plate(q3, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));
		planet.Plates.Add(new Plate(q4, rnd.Next(0, 360), MathX.RandomDouble(0, planet.PlateDrift * 2), MathX.RandomDouble(0.1, 0.5)));

		return 0;
	}
}

/* /// -- Negative -- Negative -- Negative -- Negative -- Negative -- /// */

class SaltDeposit : Activity { // SaltDeposit
	public override Range LineInteraction { get; } = new Range(-10, -1);
	public override Color Color { get; } = Colors.PaleTurquoise;
	public override double Chance { get; } = 0.25;

	public SaltDeposit() {
		Diameter = MathX.RandomDouble(40, 55);
	}

	public override int Function(Planet planet, int plateIndex, double intensity) { // Salt+++++
		var factors = planet.GetFactorsWithinRadius(Location, Diameter / 2);

		foreach ((var factor, var dis, var rX, var rY) in factors) {
			factor.Salinity *= scaleWithDis(1.25, dis, Diameter / 2);
			factor.Calcium *= scaleWithDis(1.25, dis, Diameter / 2);

		}

		return 0;
	}
}

class Rift : Activity { // Rift
	public override Range LineInteraction { get; } = new Range(-55, -12.5);
	public override double Chance { get; } = 0.75;
	public override Color Color { get; } = Colors.YellowGreen;

	public Rift() {
		Diameter = MathX.RandomDouble(10, 30);
	}

	public override int Function(Planet planet, int plateIndex, double intensity) { // Increase Oxygen and Depth
		var factors = planet.GetFactorsWithinRadius(Location, Diameter / 2);

		foreach ((var factor, var dis, var rX, var rY) in factors) {
			factor.Oxygen *= scaleWithDis(1.075, dis, Diameter / 2);
			factor.Depth *= scaleWithDis(1.1, dis, Diameter / 2);

		}

		return 0;
	}
}

class Building : Activity { // Building
	public override Range LineInteraction { get; } = new Range(-250, -25);
	public override double Chance { get; } = 0.075;
	public override Color Color { get; } = Colors.Purple;

	public Building() {
		Diameter = MathX.RandomDouble(5, 15);
	}

	public override int Function(Planet planet, int plateIndex, double intensity) {
		var factors = planet.GetFactorsWithinRadius(Location, Diameter / 2);

		foreach ((var factor, var dis, var rX, var rY) in factors) {
			factor.Depth *= scaleWithDis(1.075, dis, Diameter / 2);
			factor.Acidity *= scaleWithDis(1.075, dis, Diameter / 2);
			factor.Nitrogen *= scaleWithDis(1.075, dis, Diameter / 2);

		}

		return 0;
	}
}