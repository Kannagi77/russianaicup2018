using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel
{
	internal interface IEntity
	{
		Vector Position { get; set; }
		double Radius { get; set; }
		double Mass { get; set; }
		Vector Velocity { get; set; }
		double RadiusChangeSpeed { get; set; }
		double ArenaE { get; set; }
	}
}
