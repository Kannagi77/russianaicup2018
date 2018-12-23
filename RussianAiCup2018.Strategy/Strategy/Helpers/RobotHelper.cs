using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Helpers
{
	public static class RobotHelper
	{
		public static MyRobot ToMyRobot(this Robot robot) => new MyRobot(robot);
	}
}