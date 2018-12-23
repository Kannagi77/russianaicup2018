using System;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Helpers
{
	public static class RandomHelper
	{
		public static double Next(this Random rnd, double from, double to)
		{
			return rnd.NextDouble() * (to - from) + from;
		}
	}
}