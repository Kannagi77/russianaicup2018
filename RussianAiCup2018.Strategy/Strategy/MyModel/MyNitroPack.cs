using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel
{
	public class MyNitroPack : IEntity
	{
		private readonly NitroPack Pack;

		public MyNitroPack(NitroPack pack)
		{
			Pack = pack;
		}

		public double Radius
		{
			get => Pack.radius;
			set => Pack.radius = value;
		}

		public Vector Position
		{
			get => Vector.FromCoordinates(Pack.x, Pack.y, Pack.z);
			set
			{
				Pack.x = value.X;
				Pack.y = value.Y;
				Pack.z = value.Z;
			}
		}

		public Vector Velocity
		{
			get => Vector.FromCoordinates(0, 0, 0);
			set { }
		}

		public double Mass { get; set; }
		public double RadiusChangeSpeed { get; set; }
		public double ArenaE { get; set; }

		public int? RespawnTicks
		{
			get => Pack.respawn_ticks;
			set => Pack.respawn_ticks = value;
		}
		public bool IsAlive()
		{
			return Pack.respawn_ticks.HasValue && Pack.respawn_ticks > 0;
		}
	}
}