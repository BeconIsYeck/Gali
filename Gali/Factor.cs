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
}
