using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Helpers
{
	public static class BallHelper
	{
		public static MyBall ToMyBall(this Ball ball)
		{
			return new MyBall(ball);
		}
	}
}