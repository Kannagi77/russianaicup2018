using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel
{
	public class MyBall : IEntity
	{
		private readonly Ball Ball;

		public MyBall(Ball ball)
		{
			Ball = ball;
		}

		public double Radius
		{
			get => Ball.radius;
			set => Ball.radius = value;
		}

		public Vector Position
		{
			get => Vector.FromCoordinates(Ball.x, Ball.y, Ball.z);
			set
			{
				Ball.x = value.X;
				Ball.y = value.Y;
				Ball.z = value.Z;
			}
		}

		public Vector Velocity
		{
			get => Vector.FromCoordinates(Ball.velocity_x, Ball.velocity_y, Ball.velocity_z);
			set
			{
				Ball.velocity_x = value.X;
				Ball.velocity_y = value.Y;
				Ball.velocity_z = value.Z;
			}
		}

		public double Mass { get; set; }
		public double RadiusChangeSpeed { get; set; }
		public double ArenaE { get; set; }
	}
}