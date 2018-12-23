using System;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry
{
	public class Vector
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public double Length => Math.Sqrt(X * X + Y * Y + Z * Z);

		public Vector Normalize => new Vector
		{
			X = X / Length,
			Y = Y / Length,
			Z = Z / Length
		};

		public double DotProduct(Vector v) => X * v.X + Y * v.Y + Z * v.Z;

		public static Vector operator -(Vector a, Vector b) => new Vector
		{
			X = a.X - b.X,
			Y = a.Y - b.Y,
			Z = a.Z - b.Z
		};

		public static Vector operator +(Vector a, Vector b) => new Vector
		{
			X = a.X + b.X,
			Y = a.Y + b.Y,
			Z = a.Z + b.Z
		};

		public static Vector operator *(double d, Vector v) => v * d;

		public static Vector operator *(Vector v, double d) => new Vector
		{
			X = v.X * d,
			Y = v.Y * d,
			Z = v.Z * d
		};

		public static Vector operator /(Vector v, double d) => new Vector
		{
			X = v.X / d,
			Y = v.Y / d,
			Z = v.Z / d
		};

		public static Vector FromCoordinates(double x, double y, double z) => new Vector
		{
			X = x,
			Y = y,
			Z = z
		};
	}
}
