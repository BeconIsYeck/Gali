﻿namespace Gali;

internal class Plate {
	public List<Coordinate> Points { get; set; }
	public double Direction { get; set; }
	public double Drift { get; set; }
	public double Stability { get; set; }

	public Plate(List<Coordinate> points, double dir, double drift, double stability) {
		Points = points;
		Direction = dir;
		Drift = drift;
		Stability = stability;
	}
}