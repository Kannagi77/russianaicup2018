using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Helpers
{
	public static class NitroPackHelper
	{
		public static MyNitroPack ToMyNitroPack(this NitroPack pack)
		{
			return new MyNitroPack(pack);
		}
	}
}