﻿namespace Gali; 

class Ocean {
	public double Acidity { get; set; } = 8.1;
	public double AverageAcidity { get; set; } = 35000;

	public double Salinity { get; set; } = 35000;
	public double AverageSalinity { get; set; } = 35000;

	public double Calcium { get; set; } = 400;
	public double AverageCalcium { get; set; } = 400;

	public double Nitrogen { get; set; } = 0.6;
	public double AverageNitrogen { get; set; } = 0.6;

	public double SurfaceOxygen { get; set; } = 9.5;
	public double AverageSurfaceOxygen { get; set; } = 9.5;

	public double LastLight { get; set; } = 350;

	public double AverageGlobalTemperature { get; set; } = MathX.RandomDouble(17 - 2, 17 + 2);
	public double PolarTemperature { get; set; } = 26;
	public double EquatorialTemperature { get; set; } = 1;

	public double AverageDepth { get; set; } = 2000; // old 3682
}