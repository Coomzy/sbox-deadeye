
using Sandbox;
using Sandbox.Citizen;
using System;
using System.Diagnostics;
using System.Text.Json;
using static Sandbox.Gizmo;

public struct SplinePoint
{
	public Vector3 start;
	public Vector3? curve; // Not sure what this is called
	public Vector3 end;
}

public class Spline : Component
{
	[Group("Config"), Property] public int segments { get; set; } = 100;
	[Group("Config"), Property] public float speed { get; set; } = 350.0f;

	[Group("Runtime"), Property] public float splineTime => GetTotalSplineTime();
	[Group("Runtime"), Property] public float splineLength => CalculateTotalSplineLength();
	[Group("Runtime"), Property] public Vector3 endOfSplinePoint => GetEndOfSplinePoint();

	[Group("Debug"), Property] public bool drawGizmo { get; set; } = true;
	[Group("Debug"), Property] public float? testTime { get; set; }

	[Group("Runtime"), Property, ReadOnly] public List<SplinePoint> points { get; set; } = new List<SplinePoint>();

	protected override void OnAwake()
	{
		base.OnAwake();

		CalculateSplinePoints();
	}

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		bool isSelectedAtAll = Gizmo.IsSelected || Gizmo.IsChildSelected || drawGizmo;
		if (!isSelectedAtAll)
		{
			return;
		}

		Gizmo.Transform = Game.ActiveScene.Transform.World;

		CalculateSplinePoints();

		List<Vector3> curvedPoints = new List<Vector3>();
		foreach (var point in points)
		{
			if (!point.curve.HasValue)
			{
				Gizmo.Draw.Line(point.start, point.end);
				continue;
			}

			curvedPoints.Clear();
			for (int i = 0; i <= segments; i++)
			{
				float t = i / (float)segments;
				Vector3 curvePoint = QuadraticBezier(point.start, point.curve.Value, point.end, t);
				curvedPoints.Add(curvePoint);
			}

			if (curvedPoints.Count < 2)
				continue;

			for (int i = 1; i < curvedPoints.Count; i++)
			{
				var start = curvedPoints[i-1];
				var end = curvedPoints[i];

				Gizmo.Draw.Line(start, end);
			}
		}

		float pointTime = testTime ?? Time.Now;
		var pointOnSpline = GetPointAlongSplineAtTime(pointTime);
		Gizmo.Draw.LineSphere(pointOnSpline, 3.5f);
	}

	void CalculateSplinePoints()
	{
		points.Clear();
		for (int i = 0; i < GameObject.Children.Count; i++)
		{
			var child = GameObject.Children[i];

			if (child.Name != "Point")
			{
				continue;
			}

			var point = new SplinePoint();
			point.start = child.Transform.Position;
			bool foundEnd = false;

			for (int j = i + 1; j < GameObject.Children.Count; j++)
			{
				var childAlt = GameObject.Children[j];

				if (childAlt.Name == "Curve")
				{
					point.curve = childAlt.Transform.Position;
					continue;
				}

				if (childAlt.Name == "Point")
				{
					point.end = childAlt.Transform.Position;
					foundEnd = true;
					break;
				}
			}

			if (!foundEnd)
				continue;

			points.Add(point);
		}
	}

	public Vector3 GetEndOfSplinePoint()
	{
		Vector3 point = Transform.Position;

		if (GameObject.Children.Count != 0)
		{
			// Maybe could check if it's a 'point' but eh game jam
			return GameObject.Children[GameObject.Children.Count - 1].Transform.Position;
		}
		return point;
	}

	public Vector3 QuadraticBezier(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		float u = 1 - t;
		Vector3 point = (u * u) * p0 + (2 * u * t) * p1 + (t * t) * p2;
		return point;
	}

	public Vector2 CubicBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		float u = 1 - t;
		float tt = t * t;
		float uu = u * u;
		float uuu = uu * u;
		float ttt = tt * t;

		Vector3 point = uuu * p0; // (1-t)^3 * p0
		point += 3 * uu * t * p1; // 3 * (1-t)^2 * t * p1
		point += 3 * u * tt * p2; // 3 * (1-t) * t^2 * p2
		point += ttt * p3;        // t^3 * p3

		return point;
	}

	public float GetSegmentLength(Vector3 p0, Vector3 p1, Vector3 p2, int? segmentsOverride = null)
	{
		float curSegments = segmentsOverride ?? segments;

		float length = 0.0f;
		Vector3 prevPoint = p0;

		for (int i = 1; i <= segments; i++)
		{
			float t = i / (float)segments;
			Vector3 point = QuadraticBezier(p0, p1, p2, t);
			length += Vector3.DistanceBetween(prevPoint, point);
			prevPoint = point;
		}

		return length;
	}

	public void CalculateArcLengthLookup(Vector3 p0, Vector3 p1, Vector3 p2, int segments, List<float> arcLengths, List<float> tValues)
	{
		float totalLength = 0.0f;
		arcLengths.Clear();
		tValues.Clear();

		arcLengths.Add(0.0f);
		tValues.Add(0.0f);

		Vector3 prevPoint = p0;
		for (int i = 1; i <= segments; i++)
		{
			float t = i / (float)segments;
			Vector3 point = QuadraticBezier(p0, p1, p2, t);
			totalLength += Vector3.DistanceBetween(prevPoint, point);

			arcLengths.Add(totalLength);
			tValues.Add(t);

			prevPoint = point;
		}

		// Normalize arc lengths to [0, 1]
		for (int i = 0; i < arcLengths.Count; i++)
		{
			arcLengths[i] /= totalLength;
		}
	}

	public float FindTForArcLength(float desiredLength, List<float> arcLengths, List<float> tValues)
	{
		for (int i = 1; i < arcLengths.Count; i++)
		{
			if (arcLengths[i] >= desiredLength)
			{
				float t1 = tValues[i - 1];
				float t2 = tValues[i];
				float l1 = arcLengths[i - 1];
				float l2 = arcLengths[i];

				// Linear interpolation to find the exact t
				float t = t1 + (desiredLength - l1) / (l2 - l1) * (t2 - t1);
				return t;
			}
		}

		return 1.0f; // If we somehow reach here, return t=1 (end of the spline)
	}

	public float CalculateTotalSplineLength(int? segmentsOverride = null)
	{
		int curSegments = segmentsOverride ?? segments;
		float totalLength = 0.0f;

		foreach (var point in points)
		{
			if (point.curve.HasValue)
			{
				totalLength += GetSegmentLength(point.start, point.curve.Value, point.end, curSegments);
			}
			else
			{
				totalLength += Vector3.DistanceBetween(point.start, point.end);
			}
		}

		return totalLength;
	}

	public float GetTotalSplineTime(float? speedOverride = null, int? segmentsOverride = null)
	{
		float curSpeed = speedOverride ?? speed;

		// Calculate the total length of the spline
		float totalSplineLength = CalculateTotalSplineLength(segmentsOverride);

		// Calculate the total time to traverse the entire spline at the given speed
		float totalTime = totalSplineLength / curSpeed;

		return totalTime;
	}

	public Vector3 GetPointAlongSplineAtTime(float time, float? speedOverride = null, int? segmentsOverride = null)
	{
		float curSpeed = speedOverride ?? speed;
		int curSegments = segmentsOverride ?? segments;

		// Get the total time it takes to travel the entire spline
		float totalTime = GetTotalSplineTime(speedOverride, segmentsOverride);

		// Loop the time using modulo with totalTime
		float loopedTime = time % totalTime;

		// Calculate the distance traveled based on the looped time
		float distanceTraveled = curSpeed * loopedTime;

		// Traverse the spline points to find the corresponding segment and position
		float accumulatedLength = 0.0f;

		foreach (var point in points)
		{
			float segmentLength;
			if (point.curve.HasValue)
			{
				segmentLength = GetSegmentLength(point.start, point.curve.Value, point.end, curSegments);
			}
			else
			{
				segmentLength = Vector3.DistanceBetween(point.start, point.end);
			}

			if (accumulatedLength + segmentLength >= distanceTraveled)
			{
				float remainingLength = distanceTraveled - accumulatedLength;
				if (point.curve.HasValue)
				{
					// Calculate t for the remaining length in this segment
					List<float> arcLengths = new List<float>();
					List<float> tValues = new List<float>();
					CalculateArcLengthLookup(point.start, point.curve.Value, point.end, curSegments, arcLengths, tValues);
					float t = FindTForArcLength(remainingLength / segmentLength, arcLengths, tValues);
					return QuadraticBezier(point.start, point.curve.Value, point.end, t);
				}
				else
				{
					// Linear interpolation for a straight line segment
					return Vector3.Lerp(point.start, point.end, remainingLength / segmentLength);
				}
			}

			accumulatedLength += segmentLength;
		}

		// If we reach the end of the spline, return the last point
		return points[points.Count - 1].end;
	}

	public Vector3 GetDirectionAtTime(float time, float? speedOverride = null, int? segmentsOverride = null)
	{
		float curSpeed = speedOverride ?? speed;
		int curSegments = segmentsOverride ?? segments;

		MathX.AlmostEqual(time, curSpeed);
		float epsilon = 0.001f; // A small value to compute the difference

		// Get the position at the given time
		Vector3 position1 = GetPointAlongSplineAtTime(time, curSpeed, curSegments);

		// Get the position at a slightly later time
		Vector3 position2 = GetPointAlongSplineAtTime(time + epsilon, curSpeed, curSegments);

		// Compute the direction vector (tangent)
		Vector3 direction = position2 - position1;

		// Normalize the direction to get the unit vector
		return direction.Normal;
	}

}
