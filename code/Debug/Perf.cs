using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

public static class Perf
{
	public static Stopwatch stopwatch;

	public static void Begin()
	{
		stopwatch = new Stopwatch();
		stopwatch.Start();
	}

	public static void End()
	{
		stopwatch.Stop();

		long t = stopwatch.ElapsedMilliseconds;

		float time = Time.Now;

		if (t > 4)
		{
			Log.Info($"{time} STUTTER OF {t}ms");
		}
	}
}
