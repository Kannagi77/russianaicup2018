using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Helpers
{
	public static class DoubleHelper
	{
		public static double Clamp(this double value, double maxValue)
		{
			return value > maxValue
				? maxValue
				: value;
		}

		public static double Clamp(this double value, double min, double max)
		{
			return value < min
				? min
				: value > max
					? max
					: value;

		}

		public static Vector Clamp(this Vector v, double maxLength)
		{
			if (v.Length < maxLength)
				return v;
			var currentLength = v.Length;
			return new Vector
			{
				X = v.X * currentLength / maxLength,
				Y = v.Y * currentLength / maxLength,
				Z = v.Z * currentLength / maxLength
			};
		}
	}
}