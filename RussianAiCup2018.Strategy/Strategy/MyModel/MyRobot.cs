using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel
{
	public class MyRobot : IEntity
	{
		private readonly Robot Robot;

		public MyRobot(Robot robot)
		{
			Robot = robot;
		}

		public int Id => Robot.id;
		public int PlayerId => Robot.player_id;
		public double Radius
		{
			get => Robot.radius;
			set => Robot.radius = value;
		}

		public double NitroAmount
		{
			get => Robot.nitro_amount;
			set => Robot.nitro_amount = value;
		}

		public bool Touch
		{
			get => Robot.touch;
			set => Robot.touch = value;
		}

		public Vector Position
		{
			get => Vector.FromCoordinates(Robot.x, Robot.y, Robot.z);
			set
			{
				Robot.x = value.X;
				Robot.y = value.Y;
				Robot.z = value.Z;
			}
		}

		public Vector Velocity
		{
			get => Vector.FromCoordinates(Robot.velocity_x, Robot.velocity_y, Robot.velocity_z);
			set
			{
				Robot.velocity_x = value.X;
				Robot.velocity_y = value.Y;
				Robot.velocity_z = value.Z;
			}
		}

		public Vector TouchNormal
		{
			get =>
				Robot.touch_normal_x.HasValue
				&& Robot.touch_normal_y.HasValue
				&& Robot.touch_normal_z.HasValue
					? Vector.FromCoordinates(Robot.touch_normal_x.Value,
						Robot.touch_normal_y.Value,
						Robot.touch_normal_z.Value)
					: null;
			set
			{
				Robot.touch_normal_x = value.X;
				Robot.touch_normal_y = value.Y;
				Robot.touch_normal_z = value.Z;
			}
		}

		public double Mass { get; set; }
		public double RadiusChangeSpeed { get; set; }
		public double ArenaE { get; set; }
	}
}