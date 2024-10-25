namespace Gali;

internal class Range {
	public double Min {  get; set; }
	public double Max { get; set; }

	public Range(double min, double max) {
		Min = min; Max = max;
	}

	public static bool InRange(Range range, double val) => range.Min <= val && val <= range.Max;
	public bool InRange(double val) => InRange(this, val);
}
