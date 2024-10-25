namespace Gali;

class Population {
	public static double GlobalSizeCap = 25_000_000;

	public static int ExtantPopulations = 0;
	public static int TotalPopulations = 0;
	static int IDs { get; set; } = 0;

	public int ID { get; private set; }
	public string Genus { get; set; } = "";
	public string Species { get; set; } = "";
	public Taxonomy.Clade Clade { get; set; } = Taxonomy.Clade.Null;

	public double MassGrams { get; set; } = 10;
	public double ChangeRate { get; set; } = 0.00002;
	public bool BottomFeeder { get; set; } = false;

	public Range Temperature { get; set; }
	public Range Depth { get; set; }
	public Range Salinity { get; set; }
	public Range Oxygen { get; set; }
	public Range Calcium { get; set; }
	public Range Nitrogen { get; set; }

	public double Defense { get; set; }
	public double Power { get; set; }
	public double Mobility { get; set; }
	public double Stealth { get; set; }
	public double Sight { get; set; }

	public double DefenseVariation { get; set; } = 0.5;
	public double PowerVariation { get; set; } = 0.5;
	public double MobilityVariation { get; set; } = 0.5;
	public double StealthVariation { get; set; } = 0.5;
	public double SightVariation { get; set; } = 0.5;

	public TrophicSrategy Diet { get; set; }
	public double PercentWeightIntake { get; set; } = 0.03;

	public bool SexualReproduction { get; set; } = false;
	public Range Offspring { get; set; }

	public int Childhood { get; set; } = 1;

	public int Radius { get; set; }
	public Coordinate Center { get; set; }
	public Planet Planet { get; set; }
	public List<Factor> Factors { get; set; } = new List<Factor>();

	public Color Color { get; set; } = Colors.White;
	public bool Alive { get; set; } = false;

	public Population() {
		ID = IDs;
		IDs++;
	}

	string compare(Factor factor) {
		var okay = "";

		okay += Temperature.InRange(factor.Temperature) ? "" : "t";
		okay += Salinity.InRange(factor.Salinity) ? "" : "s";
		okay += Oxygen.InRange(factor.Oxygen) ? "" : "o";
		okay += Calcium.InRange(factor.Calcium) ? "" : "c";
		okay += Nitrogen.InRange(factor.Nitrogen) ? "" : "n";

		if (BottomFeeder) {
			okay += Depth.InRange(factor.Depth) ? "" : "d";
		}

		return okay;
	}

	double fit(Factor factor) {
		var f = 0.0;

		var tempFit = MathX.CalculateAlpha(Temperature.Min, Temperature.Max, factor.Temperature);
		var depthFit = MathX.CalculateAlpha(Depth.Min, Depth.Max, factor.Depth);
		var salinityFit = MathX.CalculateAlpha(Salinity.Min, Salinity.Max, factor.Salinity);
		var oxygenFit = MathX.CalculateAlpha(Oxygen.Min, Oxygen.Max, factor.Oxygen);
		var calciumFit = MathX.CalculateAlpha(Calcium.Min, Calcium.Max, factor.Calcium);
		var nitrogenFit = MathX.CalculateAlpha(Nitrogen.Min, Nitrogen.Max, factor.Nitrogen);

		f = (tempFit + depthFit + salinityFit + oxygenFit + calciumFit + nitrogenFit) / 6;

		return f;
	}

	public static Population? CreateProducter(Planet planet, Coordinate center, ProducerType type) {
		var producer = new Population();

		var cF = Factor.FindFactorFromCoordinate(center, planet.Factors);

		producer.Planet = planet;
		producer.Center = center;

		producer.Clade = (Taxonomy.Clade)type;

		var offspO = 0.0;
		var offspB = 0.0;

		var massO = 0.0;
		var massB = 0.0;

		var o = 0.0;

		if (type == ProducerType.Plant) {
			producer.Diet = TrophicSrategy.Phototrophism;

			producer.BottomFeeder = true;

			producer.Defense = 6;
			producer.Power = 1;
			producer.Mobility = 0;
			producer.Stealth = 8;
			producer.Sight = 0;

			producer.SexualReproduction = false;

			offspO = MathX.RandomDouble(0.4, 0.7);
			offspB = MathX.RandomInt(10, 500);

			producer.Childhood = 5;
			producer.Offspring = new Range(offspB * (1 - offspO), offspB * (1 + offspO));

			massO = MathX.RandomDouble(0.4, 0.7);
			massB = MathX.RandomDouble(10, 250);

			producer.MassGrams = massB * (1 + o);

			producer.Radius = MathX.RandomInt(100, 150);
			producer.Color = MathX.RandomColor(false, true, false, false, 0.3);

			o = 0.2;

			producer.Depth = new Range(0, Math.Clamp(center.Depth * (1 + 0), 0, cF.Depth));
			producer.Temperature = new Range(cF.Temperature * (1 - o), cF.Temperature * (1 + o));
			producer.Salinity = new Range(cF.Salinity * (1 - o), cF.Salinity * (1 + o));
			producer.Oxygen = new Range(cF.Oxygen * (1 - o), cF.Oxygen * (1 + o));
			producer.Calcium = new Range(cF.Calcium * (1 - o), cF.Calcium * (1 + o));
			producer.Nitrogen = new Range(cF.Nitrogen * (1 - o), cF.Nitrogen * (1 + o));
		}
		else if (type == ProducerType.Bacteria) {
			producer.Diet = TrophicSrategy.Chemotrophism;

			producer.Defense = 0;
			producer.Power = 0;
			producer.Mobility = 4;
			producer.Stealth = 15;
			producer.Sight = 0;

			producer.SexualReproduction = false;

			offspO = MathX.RandomDouble(0.4, 0.7);
			offspB = MathX.RandomInt(100_000, 100_000);

			producer.Childhood = 1;
			producer.Offspring = new Range(offspB * (1 - offspO), offspB * (1 + offspO));

			massO = MathX.RandomDouble(0, 0);
			massB = MathX.RandomDouble(0.0175, 0.0175);

			producer.MassGrams = massB * (1 + o);

			producer.Radius = MathX.RandomInt(150, 200);
			producer.Color = MathX.RandomColor(true, false, false, false, 0.7);

			o = 0.98;

			producer.Depth = new Range(Math.Clamp(center.Depth * (1 - o), 0, cF.Depth), Math.Clamp(center.Depth * (1 + 0), 0, cF.Depth));
			producer.Temperature = new Range(cF.Temperature * (1 - o), cF.Temperature * (1 + o));
			producer.Salinity = new Range(cF.Salinity * (1 - o), cF.Salinity * (1 + o));
			producer.Oxygen = new Range(cF.Oxygen * (1 - o), cF.Oxygen * (1 + o));
			producer.Calcium = new Range(cF.Calcium * (1 - o), cF.Calcium * (1 + o));
			producer.Nitrogen = new Range(cF.Nitrogen * (1 - o), cF.Nitrogen * (1 + o));
		}
		else if (type == ProducerType.Plankton) {
			producer.Diet = TrophicSrategy.Phototrophism;

			producer.Defense = 1;
			producer.Power = 0;
			producer.Mobility = 15;
			producer.Stealth = 10;
			producer.Sight = 0;

			producer.SexualReproduction = false;

			offspO = MathX.RandomDouble(0.6, 0.8);
			offspB = MathX.RandomInt(500, 2500);

			producer.Childhood = 1;
			producer.Offspring = new Range(offspB * (1 - offspO), offspB * (1 + offspO));

			massO = MathX.RandomDouble(0, 0);
			massB = MathX.RandomDouble(0.03, 0.03);

			producer.MassGrams = massB * (1 + o);

			producer.Radius = MathX.RandomInt(50, 90);
			producer.Color = MathX.RandomColor(true, true, false, false, 0.6, 1, true);

			o = 0.4;

			producer.Depth = new Range(0, Math.Clamp(center.Depth * (1 + 0), 0, cF.Depth));
			producer.Temperature = new Range(cF.Temperature * (1 - o), cF.Temperature * (1 + o));
			producer.Salinity = new Range(cF.Salinity * (1 - o), cF.Salinity * (1 + o));
			producer.Oxygen = new Range(cF.Oxygen * (1 - o), cF.Oxygen * (1 + o));
			producer.Calcium = new Range(cF.Calcium * (1 - o), cF.Calcium * (1 + o));
			producer.Nitrogen = new Range(cF.Nitrogen * (1 - o), cF.Nitrogen * (1 + o));
		}
		else if (type == ProducerType.Charnia) {
			producer.Diet = TrophicSrategy.Chemotrophism;

			producer.BottomFeeder = true;

			producer.Defense = 20;
			producer.Power = 0;
			producer.Mobility = 0;
			producer.Stealth = 4;
			producer.Sight = 0;

			producer.SexualReproduction = false;

			offspO = MathX.RandomDouble(0.4, 0.7);
			offspB = MathX.RandomInt(5, 20);

			producer.Childhood = 10;
			producer.Offspring = new Range(offspB * (1 - offspO), offspB * (1 + offspO));

			massO = MathX.RandomDouble(0.4, 0.7);
			massB = MathX.RandomDouble(10, 500);

			producer.MassGrams = massB * (1 + o);

			producer.Radius = MathX.RandomInt(20, 70);
			producer.Color = MathX.RandomColor(true, false, true, false, 0.5, 0, true);

			o = 0.6;

			producer.Depth = new Range(0, Math.Clamp(center.Depth * (1 + 0) + 100, 0, cF.Depth));
			producer.Temperature = new Range(cF.Temperature * (1 - o), cF.Temperature * (1 + o));
			producer.Salinity = new Range(cF.Salinity * (1 - o), cF.Salinity * (1 + o));
			producer.Oxygen = new Range(cF.Oxygen * (1 - o), cF.Oxygen * (1 + o));
			producer.Calcium = new Range(cF.Calcium * (1 - o), cF.Calcium * (1 + o));
			producer.Nitrogen = new Range(cF.Nitrogen * (1 - o), cF.Nitrogen * (1 + o));
		}

		var factorsInRadius = planet.GetFactorsWithinRadius(center, producer.Radius);

		var alive = false;

		var validFactors = 0;

		foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
			var comp = producer.compare(factor);

			if (comp == "") {
				validFactors++;
			}
		}

		if (validFactors != 0) {
			foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
				var comp = producer.compare(factor);

				if (comp == "") {
					factor.Populations.Add(producer);

					producer.Factors.Add(factor);

					alive = true;
				}
			}
		}

		if (alive) {
			ExtantPopulations++;
			TotalPopulations++;
			producer.Alive = true;
		}

		return producer;
	}

	public bool Form(double offset, bool set = true) {
		var rad = MathX.Radians;

		var factorsInRadius = Planet.GetFactorsWithinRadius(Center, Radius);

		var o = offset;

		var cF = Factor.FindFactorFromCoordinate(Center, Planet.Factors);

		if (set) {
			Temperature = new Range(cF.Temperature * (1 - o), cF.Temperature * (1 + o));
			Depth = new Range(Math.Clamp(Center.Depth * (1 - o), 0, cF.Depth), Math.Clamp(Center.Depth * (1 + 0), 0, cF.Depth));
			Salinity = new Range(cF.Salinity * (1 - o), cF.Salinity * (1 + o));
			Oxygen = new Range(cF.Oxygen * (1 - o), cF.Oxygen * (1 + o));
			Nitrogen = new Range(cF.Nitrogen * (1 - o), cF.Nitrogen * (1 + o));
			Calcium = new Range(cF.Calcium * (1 - o), cF.Calcium * (1 + o));
		}

		var alive = false;

		var validFactors = 0;

		foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
			if (compare(factor) == "") {
				validFactors++;
				Factors.Add(factor);
			}
		}

		if (validFactors != 0) {
			foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
				if (compare(factor) == "") {
					factor.Populations.Add(this);

					alive = true;
				}
			}
		}

		if (alive) {
			ExtantPopulations++;
			TotalPopulations++;
			Alive = true;
		}

		return alive;
	}

	public void Procede(int years) {
		var factorsInRadius = Planet.GetFactorsWithinRadius(Center, Radius);

		(double, Factor, int X, int Y) bestFit = (0.0, new(), 0, 0);

		var bestCoord = (0.0, 0.0);

		foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
			var comp = compare(factor);

			if (comp == "") {
				#region fit
				var tryFit = fit(factor);

				if (tryFit > bestFit.Item1)
					bestFit = (tryFit, factor, rX, rY);


				#endregion

				for (var i = 0; i < factor.Populations.Count; i++) {
					var p = factor.Populations[i];

					if (p.ID == ID) {
						factor.Populations.RemoveAt(i);
					}
				}
			}
		}

		bestCoord = Coordinate.AbsoluteSpaceToCoordinateSpace(bestFit.X, bestFit.Y);

		var cToBDis = MathX.DistanceXY(Center.Longitude, bestCoord.Item1, Center.Latitude, bestCoord.Item2);

		var rndMove = MathX.Lerp2(Center.Longitude, bestCoord.Item1, Center.Latitude, bestCoord.Item2, MathX.RandomDouble(0, 0.4));
		var movDIs = MathX.DistanceXY(Center.Longitude, Center.Latitude, rndMove.X, rndMove.Y);

		var newCenter = MathX.ApplyRotationDEG(Center.Longitude, Center.Latitude, cToBDis, movDIs);

		var longD = Center.Longitude - newCenter.X;
		var latD = Center.Latitude - newCenter.Y;

		Center.MoveRight(-longD);
		Center.MoveUp(-latD);

		var alive = false;
		var validFactors = 0;

		factorsInRadius = Planet.GetFactorsWithinRadius(Center, Radius);

		foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
			if (compare(factor) == "") {
				validFactors++;
			}
		}

		if (validFactors != 0) {
			foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
				if (compare(factor) == "") {
					factor.Populations.Add(this);

					alive = true;
				}
			}
		}

		if (alive) {
			Alive = true;
		}
	}

	public enum TrophicSrategy {
		Null = -1,
		Phototrophism = 0,
		Chemotrophism = 1,
		Microbivore = 2,
		Herbivory = 3,
		Mesoherbivory = 4,
		Omnivory = 5,
		Mesocarnivory = 6,
		Carnivory = 7,
	}

	public enum ProducerType {
		Plankton = Taxonomy.Clade.Plankton,
		Bacteria = Taxonomy.Clade.Bacteria,
		Plant = Taxonomy.Clade.Plant,
		Charnia = Taxonomy.Clade.Charnia,
	}
}