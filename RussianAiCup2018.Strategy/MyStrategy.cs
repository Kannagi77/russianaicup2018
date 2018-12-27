using System;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Helpers;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Simulation;
using Action = Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model.Action;
using Vector = Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry.Vector;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk
{
	public sealed class MyStrategy : IStrategy
	{
		public void Act(Robot me, Rules rules, Game game, Action action)
		{
			// The strategy only plays on the ground
			// So, if we are not touching the ground, use nitro
			// to go back as soon as possible
			if (!me.touch)
			{
				action.target_velocity_x = 0.0;
				action.target_velocity_y = -Constants.MAX_ENTITY_SPEED;
				action.target_velocity_z = 0.0;
				action.jump_speed = 0.0;
				action.use_nitro = true;
				return;
			}

			// Lets jump if we would hit the ball, and
			// we are on the same side of the ball as out net, so 
			// the ball would go into opponent's side of the arena
			var jump = Math.Sqrt(Math.Pow(me.x - game.ball.x, 2)
			                     + Math.Pow(me.y - game.ball.y, 2)
			                     + Math.Pow(me.z - game.ball.z, 2))
			           < Constants.BALL_RADIUS + Constants.ROBOT_MAX_RADIUS
			           && me.z < game.ball.z;

			// Since there are multiple robots in out team lets determine out role - attacker or defender
			// We will be attacker if there is friendly robot closer
			// to out net than current one.
			var is_attacker = game.robots.Length == 2;
			foreach(var robot in game.robots)
			{
				if (robot.is_teammate && robot.id != me.id)
				{
					if (robot.y < me.y)
					{
						is_attacker = true;
					}
				}
			}

			var ball = game.ball.ToMyBall();
			var meRobot = me.ToMyRobot();
			if (is_attacker)
			{
				// Attacker strategy:
				// Simulate approximate location of the ball for the next 10 seconds with 0.1 second precision
				for (var i = 0; i < 100; i++)
				{
					var t = i * 0.1;
					var ball_pos = ball.Position + ball.Velocity * t;
					// If ball will not leave arena boundary
					// (collision with the arena would happen, but we are not considering it),
					// and the ball will be closer to opponent's net than the robot,
					if (ball_pos.Y > meRobot.Position.Y
						&& Math.Abs(ball_pos.X) < (rules.arena.width / 2.0)
						&& Math.Abs(ball_pos.Y) < (rules.arena.depth / 2.0))
					{
						// Compute the speed robot needs to run with
						// To be at ball's location at the same time as the ball
						var delta_pos = Vector.FromCoordinates(ball_pos.X, ball_pos.Y, 0)
						                - Vector.FromCoordinates(meRobot.Position.X, meRobot.Position.Y, 0);
						var need_speed = delta_pos.Length / t;
						// If the speed is in acceptable range
						if (0.5 * Constants.ROBOT_MAX_GROUND_SPEED < need_speed
						    && need_speed < Constants.ROBOT_MAX_GROUND_SPEED)
						{
							// Then this is out action
							var attacker_target_velocity = Vector.FromCoordinates(delta_pos.X, delta_pos.Y, 0).Normalize * need_speed;
							action.target_velocity_x = attacker_target_velocity.X;
							action.target_velocity_y = 0.0;
							action.target_velocity_z = attacker_target_velocity.Y;
							action.jump_speed = jump ? Constants.ROBOT_MAX_JUMP_SPEED : 0.0;
							action.use_nitro = false;
							return;
						}
					}
				}
			}

			// Defender's strategy (or attacker's who did not find good moment):
			// Standing in the middle of out net
			var target_pos = Vector.FromCoordinates(0.0, -(rules.arena.depth / 2.0) + rules.arena.bottom_radius, 0);
			// And, if the ball is rolling towars it
			if (ball.Velocity.Y < -double.Epsilon)
			{
				// Find time and place where ball crosses the net line
				var t = (target_pos.Y - ball.Position.Y) / ball.Velocity.Y;
				var x = ball.Position.X + ball.Velocity.X * t;
				// If this place is inside the net
				if (Math.Abs(x) < (rules.arena.goal_width / 2.0))
				{
					// Go defend there
					target_pos.X = x;
				}
			}

			// Setting fields of the needed action
			var target_velocity = Vector.FromCoordinates(target_pos.X - meRobot.Position.X,
				                      target_pos.Y - meRobot.Position.Y,
				                      0) * Constants.ROBOT_MAX_GROUND_SPEED;
			action.target_velocity_x = target_velocity.X;
			action.target_velocity_y = 0.0;
			action.target_velocity_z = target_velocity.Y;
			action.jump_speed = jump ? Constants.ROBOT_MAX_JUMP_SPEED : 0.0;
			action.use_nitro = false;
		}
	}
}
