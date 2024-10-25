using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gali;

internal class Node {
	public Coordinate Point { get; set; }
	public Node? Left { get; set; } = null;
	public Node? Right { get; set; } = null;

	public Node(Coordinate point) {
		Point = point;
	}
}

internal class KDTree {
	public int K { get; set; } = 2;

	public KDTree(int k) {
		K = k;
	}

	public Node NewNode(Coordinate point) =>
		new Node(point);

	Node InsertRec(Node root, Coordinate pointC, int depth) {
		if (root == null) {
			return NewNode(pointC);
		}

		double[] point = { pointC.Longitude, pointC.Latitude };
		double[] rootP = { root.Point.Longitude, root.Point.Latitude };

		int cd = depth % K;

		if (point[cd] < rootP[cd]) {
			root.Left = InsertRec(root.Left, pointC, depth + 1);
		}
		else {
			root.Right = InsertRec(root.Right, pointC, depth + 1);
		}

		return root;
	}

	public Node Insert(Node root, Coordinate pointC) {
		return InsertRec(root, pointC, 0);
	}

	bool ArePointsSame(Coordinate point1C, Coordinate point2C) {
		double[] point1 = { point1C.Longitude, point1C.Latitude };
		double[] point2 = { point2C.Longitude, point2C.Latitude };

		for (int i = 0; i < K; i++) {
			if (point1[i] != point2[i]) {
				return false;
			}
		}

		return true;
	}

	bool SearchRec(Node root, Coordinate pointC, int depth) {
		if (root == null) {
			return false;
		}

		double[] point = { pointC.Longitude, pointC.Latitude };
		double[] rootP = { root.Point.Longitude, root.Point.Latitude };

		if (ArePointsSame(root.Point, pointC)) {
			return true;
		}

		int cd = depth % K;

		if (point[cd] < rootP[cd]) {
			return SearchRec(root.Left, pointC, depth + 1);
		}

		return SearchRec(root.Right, pointC, depth + 1);
	}

	public bool Search(Node root, Coordinate point) {
		return SearchRec(root, point, 0);
	}

	public List<Coordinate> FindNearest(Node? root, Coordinate target, int k) {
		var queue = new SortedSet<(double distance, Coordinate point)>();

		FindNearestRec(root, target, 0, k, queue);

		var result = new List<Coordinate>();

		foreach (var (_, point) in queue)
			result.Add(point);

		return result;
	}


	void FindNearestRec(Node? root, Coordinate target, int depth, int k, SortedSet<(double, Coordinate)> queue) {
		if (root == null) return;

		var distance = MathX.Distance(root.Point, target);
		queue.Add((distance, root.Point));

		if (queue.Count > k) {
			queue.Remove(queue.Max);
		}

		int axis = depth % K;
		double targetCoord = axis == 0 ? target.Longitude : target.Latitude;
		double rootCoord = axis == 0 ? root.Point.Longitude : root.Point.Latitude;

		Node? next = targetCoord < rootCoord ? root.Left : root.Right;
		Node? other = targetCoord < rootCoord ? root.Right : root.Left;

		FindNearestRec(next, target, depth + 1, k, queue);

		if (queue.Count < k || Math.Abs(rootCoord - targetCoord) < queue.Max.Item1) {
			FindNearestRec(other, target, depth + 1, k, queue);
		}
	}
}