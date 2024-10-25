namespace Gali;

internal class Coordinate {
	public double Longitude { get; private set; } = 0;
	public double Latitude { get; private set; } = 0;
	public double Depth { get; private set; } = 0;
	public bool Side { get; private set; } = false; // False = Normal, True = Crossed

	public static (double Min, double Max) LongitudeRange = (-180, 180);
	public static (double Min, double Max) LatitudeRange = (-90, 90);


	public Coordinate(double longitude, double latitude) {
		if (longitude > LongitudeRange.Item2 || longitude < LongitudeRange.Item1) {
			throw new ArgumentException($"Longitude: ({longitude}) outside of LongitudeRange: ({LongitudeRange.Min} to {LongitudeRange.Max})");
		}

		if (latitude > LatitudeRange.Item2 || latitude < LatitudeRange.Item1) {
			throw new ArgumentException($"Latitude: ({latitude}) outside of LatitudeRange: ({LatitudeRange.Min} to {LatitudeRange.Max})");
		}

		Longitude = longitude;
		Latitude = latitude;
	}


	double mod(double a, double b) {
		return (a % b + b) % b;
	}

	double wrap(double l) {
		var minln = LongitudeRange.Item1;
		var maxln = LongitudeRange.Item2;

		// Alternative: return (l > maxln) ? minln : (l < minln) ? maxln : l;

		if (l > maxln) {
			return minln;
		}

		if (l < minln) {
			return maxln;
		}

		return l;
	}

	double wrapy(double l) {
		var w = LongitudeRange.Max;

		// Alternative: return (l >= 0) ? (-w + Math.Abs(l)) : (w - Math.Abs(l));

		if (l >= 0)
			return -w + Math.Abs(l);

		return w - Math.Abs(l);
	}

	public void MoveUp(double dis) {
		double move = Side ? Latitude + dis : Latitude - dis;

		var maxln = LongitudeRange.Max;
		var maxlt = LatitudeRange.Max;

		if (move > maxlt) {
			var movDif = maxlt - Latitude;
			var newMove = Latitude - movDif;

			Side = !Side;

			Latitude = newMove;
			Longitude = wrapy(Longitude);
		}
		else if (move < -maxlt) {
			var movDif = -maxlt - Latitude;
			var newMove = Latitude - movDif;

			Side = !Side;

			Latitude = newMove;
			Longitude = wrapy(Longitude);
		}
		else {
			Latitude = move;
		}
	}

	public void MoveRight(double dis) {
		Longitude = wrap(Longitude + dis);
	}

	public void SetDepth(double depth, Factor factor) {
		if (depth <= factor.Depth && depth >= 0) {
			Depth = depth;
		}
		else {
			throw new ArgumentException($"Depth: {depth} outside of DepthRange: ({0} to {factor.Depth})");
		}
	}

	public static PointF CoordinateToPointF(Coordinate coord, Rect viewRect, (double, double) scale) {
		var scaleW = scale.Item1;
		var scaleH = scale.Item2;

		var pf = new PointF();

		pf.X = (float)(((coord.Longitude + (viewRect.Width / 2)) * scaleW) + viewRect.X);
		pf.Y = (float)(((coord.Latitude + (viewRect.Height / 2)) * scaleH) + viewRect.Y);

		return pf;
	}

	public PointF ToPointF(Rect viewRect, (double, double) scale) => CoordinateToPointF(this, viewRect, scale);

	public static Coordinate XYToCoordinate(double X, double Y, Rect viewRect, (double W, double H) scale) =>
		new Coordinate(((X - viewRect.X) / scale.W) - (viewRect.Width / 2), ((Y - viewRect.Y) / scale.H) - (viewRect.Height / 2));

	public static (int X, int Y) CoordinateToAbsoluteSpace(Coordinate coord) {
		var w = LongitudeRange.Max * 2;
		var h = LatitudeRange.Max * 2;

		var o = coord.Longitude;
		var a = coord.Latitude;

		var ret = (
			(int)(((o + (w / 2)) / w) * w),
			(int)(((a + (h / 2)) / h) * h)
		);

		return ret;
	}

	public static (double Longitude, double Latitude) AbsoluteSpaceToCoordinateSpace(int x, int y) {
		(double Longitude, double Latitude) coord = (0, 0);

		coord.Longitude = x - LongitudeRange.Max;
		coord.Latitude = y - LatitudeRange.Max;

		return coord;
	}

	public (int X, int Y) ToAbsoluteSpace() => CoordinateToAbsoluteSpace(this);
}