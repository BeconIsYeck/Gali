using Gali.Drawables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Gali.Population;


namespace Gali;

class Factor {
	public double Temperature { get; set; } = 0;
	public double Depth       { get; set; } = 0;      
	public double Salinity    { get; set; } = 0;
	public double Oxygen      { get; set; } = 0;
	public double Calcium     { get; set; } = 0;
	public double Nitrogen    { get; set; } = 0;
	public double Acidity    { get; set; } = 0;
	public List<Population> Populations { get; set; } = new List<Population>();
	//public List<(Population, int)> Populations { get; set; } = new List<(Population, int)>();

	public Factor() { }

	public Factor(double temp, double depth, double salinity, double oxygen, double calcium, double nitrogen, double acidity) {
		Temperature = temp;
		Depth = depth;
		Salinity = salinity;
		Oxygen = oxygen;
		Calcium = calcium;
		Nitrogen = nitrogen;
		Acidity = acidity;
	}


	public static Factor FindFactorFromCoordinate(Coordinate coord, Factor[,] factors) {
		var cA = coord.ToAbsoluteSpace();

		var cF = factors[cA.X, cA.Y];

		return cF;
	}

    public static List<(int, int)> Get4Neighbors(int x, int y, int maxX, int maxY) {
		List<(int, int)> neighbors = new List<(int, int)>();

		if (y > 0) neighbors.Add((x, y - 1)); // Up
		if (y < maxY - 1) neighbors.Add((x, y + 1)); // Down
		if (x > 0) neighbors.Add((x - 1, y)); // Left
		if (x < maxX - 1) neighbors.Add((x + 1, y)); // Right

		return neighbors;
	}

	public static List<(int, int)> Get8Neighbors(int x, int y, int maxX, int maxY) {
		List<(int, int)> neighbors = Get4Neighbors(x, y, maxX, maxY);

		if (x > 0 && y > 0) neighbors.Add((x - 1, y - 1)); // Top-Left
		if (x < maxX - 1 && y > 0) neighbors.Add((x + 1, y - 1)); // Top-Right
		if (x > 0 && y < maxY - 1) neighbors.Add((x - 1, y + 1)); // Bottom-Left
		if (x < maxX - 1 && y < maxY - 1) neighbors.Add((x + 1, y + 1)); // Bottom-Right

		return neighbors;
	}

	//public List<(int X, int Y, Population pop)> SimulatePopulations(int years, Factor[,] factors, int x, int y) {
	//	var updatedPops = new List<(int X, int Y, Population pop)>();

	//	//foreach (var pop in Populations) {
	//	Parallel.For(0, Populations.Count, i => {
	//	var pop = Populations[i];
	//		//updatedPops.Add(pop);

	//		//foreach ((var pop, var size) in Populations) {
	//		var normalizedStat = (pop.Power * pop.MassGrams, pop.Defense * pop.MassGrams, pop.Mobility * pop.MassGrams, pop.Stealth * pop.MassGrams, pop.Sight * pop.MassGrams);

	//		var nP = normalizedStat.Item1;
	//		var nD = normalizedStat.Item2;
	//		var nM = normalizedStat.Item3;
	//		var nS = normalizedStat.Item4;
	//		var nI = normalizedStat.Item5;

	//		var prey = new List<(Population Population, int Index)>();
	//		//var prey = new List<(Population Population, int Ammount, int Index)>();
	//		var predators = new List<(Population Population, int Index)>();
	//		//var predators = new List<(Population Population, int Ammount, int Index)>();

	//		//var thisAmmInFactor = 0;
	//		var thisInd = 0;
	//		var pI = 0;

	//		//foreach ((var pop2, var amm2) in Populations) {
	//		foreach (var pop2 in Populations) {
	//			if (pop2.ID == pop.ID) {
	//				//thisAmmInFactor = amm2;
	//				thisInd = pI;
	//			}

	//			if (pop.Diet == TrophicSrategy.Microbivore) {
	//				if (pop2.MassGrams <= 0.03) {
	//					prey.Add((pop2, pI));
	//				}
	//			}
	//			else if (pop.Diet == TrophicSrategy.Herbivory) {
	//				if (pop2.Diet == TrophicSrategy.Phototrophism && pop2.MassGrams > 0.03) {
	//					prey.Add((pop2, pI));
	//				}
	//			}

	//			pI++;
	//		}

	//		var overlap = 0.0;

	//		foreach ((var p, var ind) in prey) {
	//			//foreach ((var p, var amm, var ind) in prey) {
	//			var pNormalizedStat = (p.Power * p.MassGrams, p.Defense * p.MassGrams, p.Mobility * p.MassGrams, p.Stealth * p.MassGrams, p.Sight * p.MassGrams);

	//			var pNP = pNormalizedStat.Item1;
	//			var pND = pNormalizedStat.Item2;
	//			var pNM = pNormalizedStat.Item3;
	//			var pNS = pNormalizedStat.Item4;
	//			var pNI = pNormalizedStat.Item5;

	//			var threshold = MathX.FindFirstIntersection(nP, pop.PowerVariation, pND, pop.DefenseVariation);
	//			var area = MathX.CDF(threshold, nP, pop.PowerVariation);

	//			var overlapPowDef = MathX.FindIntersectionArea(pNP, pop.PowerVariation, nP, pop.PowerVariation);

	//			if (pop.ID == 44)
	//				Console.WriteLine();

	//			var popRI = (int)(2 * Math.Abs(1 - overlapPowDef));
	//			var pRI = (int)(2 * Math.Abs(overlapPowDef));

	//			pop.Radius *= popRI;
	//			//p.Radius *= pRI;

	//			overlap += overlapPowDef;

	//			if (overlapPowDef < 0.5) {

	//				// Predator won
	//			}
	//			else {
	//				// Prey won
	//			}

	//			//pop.Size *= (int)(pop.Size * (1 - overlapPowDef));
	//			//p.Size *= (int)Math.Ceiling(overlapPowDef);

	//		}

	//		if (prey.Count != 0) {
	//			overlap /= prey.Count;

	//			if (overlap < 0.5) {
	//				// Predator won

	//				var neighbors = Get8Neighbors(x, y, factors.GetLength(0), factors.GetLength(1));

	//				var k = 0;

	//				foreach ((var nX, var nY) in neighbors) {
	//					var n = factors[nX, nY];

	//					//lock (factors[nX, nY].Populations) {  // Added lock statement
	//					//	factors[nX, nY].Populations.Add(pop);  // Changed updatedPops.Add(pop) to directly modify the factors array
	//					//}
	//					//n.Populations.Add(pop);// this line is causing it to crash
	//					updatedPops.Add((nX, nY, pop));


	//					k++;
	//				}

	//				Console.WriteLine($"{k}");
	//			}
	//			else {
	//				// Prey won
	//			}
	//		}
	//	});


	//	return updatedPops;
	//	//factors[x, y].Populations = updatedPops;
	//}

		//foreach (var pop in Populations) {
		//	//foreach ((var pop, var size) in Populations) {
		//	var normalizedStat = (pop.Power * pop.MassGrams, pop.Defense * pop.MassGrams, pop.Mobility * pop.MassGrams, pop.Stealth * pop.MassGrams, pop.Sight * pop.MassGrams);

		//	var nP = normalizedStat.Item1;
		//	var nD = normalizedStat.Item2;
		//	var nM = normalizedStat.Item3;
		//	var nS = normalizedStat.Item4;
		//	var nI = normalizedStat.Item5;

		//	var prey = new List<(Population Population, int Index)>();
		//	//var prey = new List<(Population Population, int Ammount, int Index)>();
		//	var predators = new List<(Population Population, int Index)>();
		//	//var predators = new List<(Population Population, int Ammount, int Index)>();

		//	//var thisAmmInFactor = 0;
		//	var thisInd = 0;
		//	var pI = 0;

		//	//foreach ((var pop2, var amm2) in Populations) {
		//	foreach (var pop2 in Populations) {
		//		if (pop2.ID == pop.ID) {
		//			//thisAmmInFactor = amm2;
		//			thisInd = pI;
		//		}

		//		if (pop.Diet == TrophicSrategy.Microbivore) {
		//			if (pop2.MassGrams <= 0.03) {
		//				prey.Add((pop2, pI));
		//			}
		//		}
		//		else if (pop.Diet == TrophicSrategy.Herbivory) {
		//			if (pop2.Diet == TrophicSrategy.Phototrophism && pop2.MassGrams > 0.03) {
		//				prey.Add((pop2, pI));
		//			}
		//		}

		//		pI++;
		//	}

		//	var overlap = 0.0;

		//	foreach ((var p, var ind) in prey) {
		//	//foreach ((var p, var amm, var ind) in prey) {
		//		var pNormalizedStat = (p.Power * p.MassGrams, p.Defense * p.MassGrams, p.Mobility * p.MassGrams, p.Stealth * p.MassGrams, p.Sight * p.MassGrams);

		//		var pNP = pNormalizedStat.Item1;
		//		var pND = pNormalizedStat.Item2;
		//		var pNM = pNormalizedStat.Item3;
		//		var pNS = pNormalizedStat.Item4;
		//		var pNI = pNormalizedStat.Item5;

		//		var threshold = MathX.FindFirstIntersection(nP, pop.PowerVariation, pND, pop.DefenseVariation);
		//		var area = MathX.CDF(threshold, nP, pop.PowerVariation);

		//		var overlapPowDef = MathX.FindIntersectionArea(pNP, pop.PowerVariation, nP, pop.PowerVariation);

		//		if (pop.ID == 44)
		//			Console.WriteLine();

		//		var popRI = (int)(2 * Math.Abs(1 - overlapPowDef));
		//		var pRI = (int)(2 * Math.Abs(overlapPowDef));

		//		pop.Radius *= popRI;
		//		//p.Radius *= pRI;

		//		overlap += overlapPowDef;

		//		if (overlapPowDef < 0.5) {

		//			// Predator won
		//		}
		//		else {
		//			// Prey won
		//		}

		//		//pop.Size *= (int)(pop.Size * (1 - overlapPowDef));
		//		//p.Size *= (int)Math.Ceiling(overlapPowDef);

		//	}

		//	if (prey.Count != 0) {
		//		overlap /= prey.Count;

		//		if (overlap < 0.5) {
		//			// Predator won

		//			var neighbors = Get8Neighbors(x, y, factors.GetLength(0), factors.GetLength(1));

		//			foreach ((var nX, var nY) in neighbors) {
		//				var n = factors[nX, nY];

		//				n.Populations.Add(pop);
		//			}
		//		}
		//		else {
		//			// Prey won
		//		}
		//	}



		//	#region old
		//	//var foodValuePer = (pop.MassGrams * amm * years) / prey.Count;
		//	//var foodNeededPer = foodNeeded / prey.Count;

		//	//var leftover = foodValuePer - foodNeededPer;
		//	//foodLeft -= foodValuePer;

		//	//var test = MathX.CDF()

		//	//var prey =
		//	#endregion
		//}

		////var validFactors = 

		//foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
		//	if (compare(factor) == "") {
		//		validFactors++;
		//		//factor.Populations.Add(this);

		//		Factors.Add(factor);

		//		//alive = true;
		//	}
		//}

		//var rB = false;

		//if (validFactors != 0) {
		//	foreach ((var factor, var dis, var rX, var rY) in factorsInRadius) {
		//		if (compare(factor) == "") {
		//			var i = 0;

		//			var rB = false;

		//			foreach ((var pop, var _) in factor.Populations) {
		//				if (pop.ID == ID) {
		//					factor.Populations[i] = (pop, (int)Math.Ceiling(Size / (double)validFactors));

		//					rB = true;
		//					break;
		//				}

		//				i++;
		//			}

		//			if (rB)
		//				continue;

		//			factor.Populations.Add((this, (int)Math.Ceiling(Size / (double)validFactors)));
		//		}
		//	}
		//}

	public Factor Clone() {
		var clone = new Factor();

		clone.Temperature = Temperature;
		clone.Depth = Depth;
		clone.Salinity = Salinity;
		clone.Oxygen = Oxygen;
		clone.Calcium = Calcium;
		clone.Nitrogen = Nitrogen;

		return clone;
	}
}
