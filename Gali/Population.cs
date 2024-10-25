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

	//public int Size { get; set; } = 0;
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
	//public Range Lifespan  { get; set; }

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

	//public Population(string genus, string species, int size, double mass, double changeRate, Range temp, Range depth, Range salinity, Range oxy, Range calcium, Range nitrogen, double def, double pow, double mob, double sth, Range offspring, Range childhood, Range lifespan, List<Coordinate> range, double rangeRad) {
	//	//Genus = genus;
	//	//Species = species;
	//	Size = size;
	//	MassGrams = mass;
	//	ChangeRate = changeRate;
	//	Temperature = temp;
	//	Depth = depth;
	//	Salinity = salinity;
	//	Oxygen = oxy;
	//	Calcium = calcium;
	//	Nitrogen = nitrogen;
	//	Defense = def;
	//	Power = pow;
	//	Mobility = mob;
	//	Stealth = sth;
	//	Offspring = offspring;
	//	//Childhood = childhood;
	//	//Lifespan = lifespan;
	//}

	public Population(string genus, string species, Planet planet, int size, Coordinate center, int radius) {
		//Genus = genus;
		//Species = species;
		//Size = size;
		Center = center;
		Planet = planet;
		Radius = radius;

		Temperature = new Range(0, 100_000);
		Depth = new Range(4_000, 5_000);
		Salinity = new Range(100, 100_000);
		Oxygen = new Range(8, 10);
		Calcium = new Range(200, 600);
		Nitrogen = new Range(0, 6);

	}

	string compare(Factor factor) {
		var okay = "";

		okay += Temperature.InRange(factor.Temperature) ? "" : "t";
		//okay += Depth.InRange(factor.Depth)             ? "" : "d";
		okay += Salinity.InRange(factor.Salinity) ? "" : "s";
		okay += Oxygen.InRange(factor.Oxygen) ? "" : "o";
		okay += Calcium.InRange(factor.Calcium) ? "" : "c";
		okay += Nitrogen.InRange(factor.Nitrogen) ? "" : "n";

		if (BottomFeeder) {
			okay += Depth.InRange(factor.Depth) ? "" : "d";
		}

		//throw new Exception($"{okay}");

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

	//int populationIncrease(int steps) => // po^s
	//	Size * (int)Math.Pow( Math.Round(MathX.RandomDouble(Offspring.Min, Offspring.Max)), steps);

	double FoodNeededPerStep(int size, int steps) => // 365imps
		365 * PercentWeightIntake * MassGrams * size * steps;

	/// <summary>
	/// Don't forget to divide by Size, this is just the CAP
	/// </summary>
	/// <returns></returns>
	double GetSizeCap() => GlobalSizeCap / MassGrams;

	public static Population? CreateProducter(Planet planet, Coordinate center, ProducerType type) {
		var producer = new Population();

		var cF = Factor.FindFactorFromCoordinate(center, planet.Factors);

		producer.Planet = planet;
		producer.Center = center;

		producer.Clade = (Taxonomy.Clade)type;
		//producer.Diet = (TrophicSrategy)producer.Clade;

		var offspO = 0.0;
		var offspB = 0.0;

		var massO = 0.0;
		var massB = 0.0;

		var o = 0.0;

		if (type == ProducerType.Plant) {
			producer.Diet = TrophicSrategy.Phototrophism;
			//producer.Size = MathX.RandomInt(10_000, 50_000);

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
			//producer.Size = MathX.RandomInt(500_000_000, 1_000_000_000);

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
			//producer.Size = MathX.RandomInt(100_000, 1_000_000);


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
			//producer.Size = MathX.RandomInt(750, 15_000);

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
				////if (producer.Depth.InRange(factor.Depth)) {
				//factor.Populations.Add(producer);

				//alive = true;
				//}
			}
		}

		if (validFactors != 0) {
			foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
				var comp = producer.compare(factor);

				if (comp == "") {
					//factor.Populations.Add((producer, 0));//  (int)Math.Ceiling(producer.Size / (double)validFactors) ));
					factor.Populations.Add(producer);//  (int)Math.Ceiling(producer.Size / (double)validFactors) ));

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

	public int Fight(Population contender, Factor factor) {
		var win = 0; // -1 lost, 0 nothing, 1 win

		var thisTS = (int)Diet;
		var contTS = (int)contender.Diet;

		if (thisTS == 0) {
		
		}

		return win;
	}

	public bool Draw(Planet planet) {
		var factorsInRadius = Planet.GetFactorsWithinRadius(Center, Radius);

		var cF = Factor.FindFactorFromCoordinate(Center, Planet.Factors);

		var alive = false;
		var validFactors = 0;

		foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
			if (compare(factor) == "") {
				validFactors++;
			}
		}

		if (validFactors != 0) {
			foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
				if (compare(factor) == "") {
					//factor.Populations.Add((this, 10));// (int)Math.Ceiling(Size / (double)validFactors)));
					factor.Populations.Add(this);// (int)Math.Ceiling(Size / (double)validFactors)));

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
				//factor.Populations.Add(this);

				Factors.Add(factor);

				//alive = true;
			}
		}

		if (validFactors != 0) {
			foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
				if (compare(factor) == "") {
					//factor.Populations.Add((this, 0)); //(int)Math.Ceiling(Size / (double)validFactors)));
					factor.Populations.Add(this); //(int)Math.Ceiling(Size / (double)validFactors)));

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

	static double Aggregation(List<(double mu, double sigma)> prey, List<(double mu, double sigma)> predator) {
		var totalIA = 0.0;

		return totalIA;
	}



	//public void Procede(int years) {
	//	var sizeCap = GetSizeCap();

	//	var normalizedStat = (Power * MassGrams, Defense * MassGrams, Mobility * MassGrams, Stealth * MassGrams, Sight * MassGrams);

	//	var nP = normalizedStat.Item1;
	//	var nD = normalizedStat.Item2;
	//	var nM = normalizedStat.Item3;
	//	var nS = normalizedStat.Item4;
	//	var nI = normalizedStat.Item5;

	//	foreach (var factor in Factors) {
	//		var comp = compare(factor);

	//		if (comp == "") {
	//			var competition = factor.Populations;

	//			#region Remove If No Resources

	//			var cI = 0;
	//			if (competition.Count == 0) {
	//				foreach ((var pop, var _) in factor.Populations) {
	//					if (pop.ID == ID)
	//						factor.Populations.RemoveAt(cI);


	//					cI++;
	//				}
	//			}
	//			#endregion

	//			var prey = new List<(Population Population, int Ammount, int Index)>();
	//			var predators = new List<(Population Population, int Ammount, int Index)>();

	//			var thisAmmInFactor = 0;
	//			var thisInd = 0;

	//			var pI = 0;

	//			foreach ((var pop, var amm) in factor.Populations) {
	//				if (pop.ID == ID) {
	//					thisAmmInFactor = amm;
	//					thisInd = pI;
	//				}

	//				if (Diet == TrophicSrategy.Microbivore) {
	//					if (pop.MassGrams <= 0.03) {
	//						prey.Add((pop, amm, pI));
	//					}
	//				}
	//				else if (Diet == TrophicSrategy.Herbivory) {
	//					if (pop.Diet == TrophicSrategy.Phototrophism && pop.MassGrams > 0.03) {
	//						prey.Add((pop, amm, pI));
	//					}
	//				}

	//				pI++;
	//			}

	//			var foodNeeded = FoodNeededPerStep(thisAmmInFactor, years);
	//			var foodLeft = foodNeeded;

	//			foreach ((var pop, var amm, var ind) in prey) {
	//				var popNormalizedStat = (pop.Power * pop.MassGrams, pop.Defense * pop.MassGrams, pop.Mobility * pop.MassGrams, pop.Stealth * pop.MassGrams, pop.Sight * pop.MassGrams);


	//				var pNP = popNormalizedStat.Item1;
	//				var pND = popNormalizedStat.Item2;
	//				var pNM = popNormalizedStat.Item3;
	//				var pNS = popNormalizedStat.Item4;
	//				var pNI = popNormalizedStat.Item5;

	//				var threshold = MathX.FindFirstIntersection(nP, PowerVariation, pND, pop.DefenseVariation);
	//				var area = MathX.CDF(threshold, nP, PowerVariation);

	//				var overlapPowDef = MathX.FindIntersectionArea(pNP, pop.PowerVariation, nP, PowerVariation);




	//				//var foodValuePer = (pop.MassGrams * amm * years) / prey.Count;
	//				//var foodNeededPer = foodNeeded / prey.Count;

	//				//var leftover = foodValuePer - foodNeededPer;
	//				//foodLeft -= foodValuePer;


	//			}
	//		}
	//	}
	//}

	//
	//public void Procede(int years) {
	//	if (Diet != TrophicSrategy.Phototrophism && Diet != TrophicSrategy.Chemotrophism) {
	//		var cA = Center.ToAbsoluteSpace();

	//		foreach (var factor in Factors) { // Eat or die
	//			var comp = compare(factor);

	//			if (comp == "") {
	//				#region Remove If No Resources
	//				var competition = factor.Populations;

	//				var i = 0;

	//				if (competition.Count == 0) {
	//					foreach ((var pop, var _) in factor.Populations) {
	//						if (pop.ID == ID) { // If no resources, kill it
	//							factor.Populations.RemoveAt(i);
	//						}

	//						i++;
	//					}
	//				}
	//				#endregion

	//				var thisInd = 0;
	//				var thisAmmInFactor = 0;

	//				#region Find Food and Predators

	//				var food = new List<(Population, int, int)>();
	//				var predators = new List<(Population, int, int)>();

	//				var pI = 0;

	//				foreach ((var pop, var amm) in factor.Populations) {
	//					if (pop.ID == ID) {
	//						thisAmmInFactor = amm;
	//						thisInd = pI;
	//					}

	//					if (Diet == TrophicSrategy.Microbivore) {
	//						if (pop.MassGrams <= 0.03 && pop.MassGrams <= 0.03) {
	//							food.Add((pop, amm, pI));
	//						}
	//					}

	//					pI++;
	//				}
	//				#endregion

	//				var foodNeeded = annualIntake(thisAmmInFactor, years);
	//				var fNDif = foodNeeded;

	//				var indicesToRemove = new List<int>();

	//				foreach ((var pop, var amm, var ind) in food) {
	//					var foodValuePer = (pop.MassGrams * amm * years) / food.Count;
	//					var foodNeededPer = foodNeeded / food.Count;

	//					var leftover = foodValuePer - foodNeededPer;
	//					fNDif -= foodValuePer;

	//					if (leftover >= 0) {
	//						indicesToRemove.Add(ind);
	//					}
	//				}

	//				if (fNDif >= 0) {
	//					indicesToRemove.Add(thisInd);

	//					Size -= thisAmmInFactor;
	//				}

	//				foreach (var ind in indicesToRemove.OrderByDescending(x => x)) {
	//					factor.Populations.RemoveAt(ind);
	//				}

	//				if (Size <= 0) {
	//					break;
	//				}

	//				#region Waste

	//				//var eaten = foodValue - foodNeeded;

	//				//if (eaten >= 0) {
	//				//	// EATING GOOD TONIGHT


	//				//}
	//				//else {
	//				//	// BAD BAD BAD BAD BAD
	//				//}

	//				//#region fit
	//				//var tryFit = fit(factor);

	//				//if (tryFit > bestFit.Item1)
	//				//	bestFit = (tryFit, factor, rX, rY);

	//				//if (Math.Abs(0.5 - tryFit) > worstFit.Item1)
	//				//	worstFit = (tryFit, factor, rX, rY);

	//				//#endregion

	//				//bestFit.Item2.Temperature = 1000000;
	//				//worstFit.Item2.Temperature = 39;

	//				//bestCoord = Coordinate.AbsoluteSpaceToCoordinateSpace(bestFit.X, bestFit.Y);
	//				//worstCoord = Coordinate.AbsoluteSpaceToCoordinateSpace(worstFit.X, worstFit.Y);

	//				//var cToBRot = Math.Atan2(Center.Latitude - bestCoord.Item1, Center.Longitude - bestCoord.Item2);
	//				//var cToBDis = MathX.DistanceXY(Center.Longitude, bestCoord.Item1, Center.Latitude, bestCoord.Item2);

	//				//var rndMove = MathX.Lerp2(Center.Longitude, bestCoord.Item1, Center.Latitude, bestCoord.Item2, MathX.RandomDouble(0, 0.4));
	//				//var movDIs = MathX.DistanceXY(Center.Longitude, Center.Latitude, rndMove.X, rndMove.Y);

	//				//var newCenter = MathX.ApplyRotationDEG(Center.Longitude, Center.Latitude, cToBDis, movDIs);

	//				//var longD = Center.Longitude - newCenter.X;
	//				//var latD = Center.Latitude - newCenter.Y;

	//				//Center.MoveRight(-longD);
	//				//Center.MoveUp(-latD);
	//				#endregion
	//			}

	//			if (Size <= 0) {

	//			}
	//		}

	//		var sizeCap = GetSizeCap() / Size;


	//		//var ldr = lived / died;

	//		Radius += (int)(sizeCap / 2);
	//		//Size = (int)Math.Clamp(Size + Size, Size, GetSizeCap());
	//		Size = (int)Math.Clamp(populationIncrease(years), 0, GetSizeCap());

	//		var factorsInRadius = Planet.GetFactorsWithinRadius(Center, Radius);

	//		var validFactors = 0;

	//		Factors.Clear();

	//		foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
	//			if (compare(factor) == "") {
	//				validFactors++;
	//				//factor.Populations.Add(this);

	//				Factors.Add(factor);

	//				//alive = true;
	//			}
	//		}

	//		//var rB = false;

	//		if (validFactors != 0) {
	//			foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
	//				if (compare(factor) == "") {
	//					var i = 0;

	//					var rB = false;

	//					foreach ((var pop, var _) in factor.Populations) {
	//						if (pop.ID == ID) {
	//							factor.Populations[i] = (pop, (int)Math.Ceiling(Size / (double)validFactors));

	//							rB = true;
	//							break;
	//						}

	//						i++;
	//					}

	//					if (rB)
	//						continue;

	//					factor.Populations.Add((this, (int)Math.Ceiling(Size / (double)validFactors)));
	//				}
	//			}
	//		}
	//	}
	//	else {

	//	}

	//	//bestFit.Item2.Temperature = 1000000;
	//	//worstFit.Item2.Temperature = 39;

	//	//bestCoord = Coordinate.AbsoluteSpaceToCoordinateSpace(bestFit.X, bestFit.Y);
	//	//worstCoord = Coordinate.AbsoluteSpaceToCoordinateSpace(worstFit.X, worstFit.Y);

	//	//var cToBRot = Math.Atan2(Center.Latitude - bestCoord.Item1, Center.Longitude - bestCoord.Item2);
	//	//var cToBDis = MathX.DistanceXY(Center.Longitude, bestCoord.Item1, Center.Latitude, bestCoord.Item2);

	//	//var rndMove = MathX.Lerp2(Center.Longitude, bestCoord.Item1, Center.Latitude, bestCoord.Item2, MathX.RandomDouble(0, 0.4));
	//	//var movDIs = MathX.DistanceXY(Center.Longitude, Center.Latitude, rndMove.X, rndMove.Y);

	//	//var newCenter = MathX.ApplyRotationDEG(Center.Longitude, Center.Latitude, cToBDis, movDIs);

	//	//var longD = Center.Longitude - newCenter.X;
	//	//var latD = Center.Latitude - newCenter.Y;

	//	//Center.MoveRight(-longD);
	//	//Center.MoveUp(-latD);

	//	//var alive = false;
	//	//var validFactors = 0;
	//}

	public void Procede(int years) {
		var factorsInRadius = Planet.GetFactorsWithinRadius(Center, Radius);

		var cA = Center.ToAbsoluteSpace();

		(double, Factor, int X, int Y) bestFit = (0.0, new(), 0, 0);
		(double, Factor, int X, int Y) worstFit = (0.0, new(), 0, 0);

		var bestCoord = (0.0, 0.0);
		var worstCoord = (0.0, 0.0);

		foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
			var comp = compare(factor);

			if (comp == "") {
				#region fit
				var tryFit = fit(factor);

				if (tryFit > bestFit.Item1)
					bestFit = (tryFit, factor, rX, rY);

				if (Math.Abs(0.5 - tryFit) > worstFit.Item1)
					worstFit = (tryFit, factor, rX, rY);

				#endregion

				for (var i = 0; i < factor.Populations.Count; i++) {
					var p = factor.Populations[i];

					if (p.ID == ID) {
						factor.Populations.RemoveAt(i);
					}
				}

				//var competition = factor.Populations;

				//foreach (var pop in competition) {
				//	if (Diet == TrophicSrategy.Microbivore) {
				//		if (pop.MassGrams <= 0.03) {

				//		}
				//	}
				//}
			}
		}

		//bestFit.Item2.Temperature = 1000000;
		//worstFit.Item2.Temperature = 39;

		bestCoord = Coordinate.AbsoluteSpaceToCoordinateSpace(bestFit.X, bestFit.Y);
		worstCoord = Coordinate.AbsoluteSpaceToCoordinateSpace(worstFit.X, worstFit.Y);

		var cToBRot = Math.Atan2(Center.Latitude - bestCoord.Item1, Center.Longitude - bestCoord.Item2);
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
			//Populations++;
			Alive = true;
		}

		//foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
		//	if (compare(factor) == "") {
		//		factor.Populations.Add((this, (int)Math.Ceiling(Size / (double)100)));
		//	}
		//}
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

	public Dictionary<TrophicSrategy, object?> TrophicWeb = new() {
		{ TrophicSrategy.Phototrophism, null },
		{ TrophicSrategy.Chemotrophism, null },
		{ TrophicSrategy.Microbivore, null },
		{ TrophicSrategy.Herbivory, (0, 1) },
		{ TrophicSrategy.Mesoherbivory, (0, 1, 2, 3, 4, 5) },
		{ TrophicSrategy.Omnivory, (0, 1, 1, 2, 3, 4, 5, 6, 7) },
		{ TrophicSrategy.Mesocarnivory, (0, 1, 1, 2, 3, 4, 5, 6, 7) },
		{ TrophicSrategy.Carnivory, (1, 2, 3, 4, 5, 6, 7) },
	};

	public enum ProducerType {
		Plankton = Taxonomy.Clade.Plankton,
		Bacteria = Taxonomy.Clade.Bacteria,
		Plant = Taxonomy.Clade.Plant,
		Charnia = Taxonomy.Clade.Charnia,
	}

	public static int CompareTrophisms(TrophicSrategy trophism1, TrophicSrategy trophism2) {
		var res = 0;

		var f1 = true;

		var t1 = (int)trophism1;
		var t2 = (int)trophism2;

		return res;
	}
}

class Food {
	enum FoodType {
		Plankton = 0,
		Vent = 1,
		Pant = 2,
	}	 
}