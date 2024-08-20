
using Sandbox.Citizen;
using System;

public static class Extensions
{
	public static bool ContainsIndex<T>(this List<T> list, int index)
	{
		if (index < 0)
			return false;

		if (index >= list.Count)
			return false;

		return true;
	}

	public static string ToFixed(this decimal value, int decimalPlaces)
	{
		return Math.Round(value, decimalPlaces).ToString($"F{decimalPlaces}");
	}

	public static string ToFixed(this float value, int decimalPlaces)
	{
		return Math.Round(value, decimalPlaces).ToString($"F{decimalPlaces}");
	}

	public static string ToFixed(this double value, int decimalPlaces)
	{
		return Math.Round(value, decimalPlaces).ToString($"F{decimalPlaces}");
	}
}
