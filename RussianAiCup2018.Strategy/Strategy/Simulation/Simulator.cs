using System;
using System.Linq;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Geometry;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Helpers;
using Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.MyModel;
using Action = Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Model.Action;

namespace Com.CodeGame.CodeBall2018.DevKit.CSharpCgdk.Strategy.Simulation
{
	internal static class Simulator
	{
		private static Arena arena;
		private static Game game;
		private static Action action;
		private static readonly Random rnd = new Random();

		public static void Tick()
		{
			var deltaTime = 1 / Constants.TICKS_PER_SECOND;
			for (var microTick = 0; microTick < Constants.MICROTICKS_PER_TICK - 1; microTick++)
			{
				Update(deltaTime / Constants.MICROTICKS_PER_TICK);
			}

			foreach (var pack in game.nitro_packs.Select(p => p.ToMyNitroPack()))
			{
				if (pack.IsAlive())
					continue;
				pack.RespawnTicks -= 1;
			}
		}

		private static void Update(double deltaTime)
		{
			Shuffle(ref game.robots);
			foreach (var robot in game.robots.Select(r => r.ToMyRobot()))
			{
				if (robot.Touch)
				{
					var targetVelocity = new Vector
					{
						X = action.target_velocity_x,
						Y = action.target_velocity_y,
						Z = action.target_velocity_z
					}.Clamp(
						Constants.ROBOT_MAX_GROUND_SPEED);
					var touchNormal = robot.TouchNormal;
					targetVelocity -= touchNormal * touchNormal.DotProduct(targetVelocity);
					var targetVelocityChange = targetVelocity - robot.Velocity;
					if (targetVelocityChange.Length > 0)
					{
						var acceleration = Constants.ROBOT_ACCELERATION * Math.Max(0, robot.TouchNormal.Y);

						var velocity = robot.Velocity;
						velocity += (targetVelocityChange.Normalize * acceleration * deltaTime).Clamp(targetVelocityChange.Length);
					}
				}

				if (action.use_nitro)
				{
					var targetVelocityChange = (Vector.FromCoordinates(action.target_velocity_x,
						                              action.target_velocity_y, action.target_velocity_z) -
					                              robot.Velocity).Clamp(robot.NitroAmount *
					                                                    Constants.NITRO_POINT_VELOCITY_CHANGE);
					if (targetVelocityChange.Length > 0)
					{
						var acceleration = targetVelocityChange.Normalize * Constants.ROBOT_NITRO_ACCELERATION;
						var velocityChange = (acceleration * deltaTime).Clamp(targetVelocityChange.Length);
						robot.Velocity += velocityChange;
						robot.NitroAmount -= velocityChange.Length / Constants.NITRO_POINT_VELOCITY_CHANGE;
					}
				}
				Move(robot, deltaTime);
				robot.Radius = Constants.ROBOT_MIN_RADIUS + (Constants.ROBOT_MAX_RADIUS - Constants.ROBOT_MIN_RADIUS)
				               * action.jump_speed / Constants.ROBOT_MAX_JUMP_SPEED;
				robot.RadiusChangeSpeed = action.jump_speed;
			}

			Move(game.ball.ToMyBall(), deltaTime);

			for (var i = 0; i < game.robots.Length - 1; i++)
			for (var j = 0; j < i - 1; j++)
				CollideEntities(game.robots[i].ToMyRobot(), game.robots[j].ToMyRobot());

			foreach (var gameRobot in game.robots)
			{
				var myRobot = gameRobot.ToMyRobot();
				CollideEntities(myRobot, game.ball.ToMyBall());
				var collisionNormal = CollideWithArena(myRobot);
				if (collisionNormal == null)
				{
					myRobot.Touch = false;
				}
				else
				{
					myRobot.Touch = true;
					myRobot.TouchNormal = collisionNormal;
				}
			}

			CollideWithArena(game.ball.ToMyBall());
			if (Math.Abs(game.ball.ToMyBall().Position.Z) > arena.depth / 2 + game.ball.radius)
			{
				GoalScored();
			}

			foreach (var robot in game.robots.Select(r => r.ToMyRobot()))
			{
				if (Math.Abs(robot.NitroAmount - Constants.MAX_NITRO_AMOUNT) < double.Epsilon)
					continue;
				foreach (var pack in game.nitro_packs.Select(p => p.ToMyNitroPack()))
				{
					if (!pack.IsAlive())
						continue;
					if ((robot.Position - pack.Position).Length <= robot.Radius + pack.Radius)
					{
						robot.NitroAmount = Constants.MAX_NITRO_AMOUNT;
						pack.RespawnTicks = Constants.NITRO_PACK_RESPAWN_TICKS;
					}
				}
			}
		}

		private static void GoalScored()
		{
		}

		private static void Shuffle(ref Robot[] gameRobots)
		{
			gameRobots = gameRobots.OrderBy(i => rnd.Next()).ToArray();
		}

		private static void CollideEntities(IEntity a, IEntity b)
		{
			var delta = b.Position - a.Position;
			var distance = delta.Length;
			var penetration = a.Radius + b.Radius - distance;
			if (penetration <= 0)
				return;
			var aMassReverse = 1 / a.Mass;
			var bMassReverse = 1 / b.Mass;
			var divisor = aMassReverse + bMassReverse;
			var aK = aMassReverse / divisor;
			var bK = bMassReverse / divisor;
			var normal = delta.Normalize;
			a.Position -= normal * penetration * aK;
			b.Position += normal * penetration * bK;
			var deltaVelocity = normal.DotProduct(b.Velocity - a.Velocity) + b.RadiusChangeSpeed - a.RadiusChangeSpeed;
			if (deltaVelocity >= 0)
				return;
			var impulse = (1 + rnd.Next(Constants.MIN_HIT_E, Constants.MAX_HIT_E)) * deltaVelocity * normal;
			a.Velocity += impulse * aK;
			b.Velocity -= impulse * bK;
		}

		private static Vector CollideWithArena(IEntity e)
		{
			var (distance, normal) = DanToArena(e.Position);
			var penetration = e.Radius - distance;
			if (penetration > 0)
			{
				e.Position += penetration * normal;
				var velocity = e.Velocity.DotProduct(normal) - e.RadiusChangeSpeed;
				if (velocity < 0)
				{
					e.Velocity -= (1 + e.ArenaE) * velocity * normal;
					return normal;
				}
			}

			return null;
		}

		private static void Move(IEntity e, double deltaTime)
		{
			e.Velocity = e.Velocity.Clamp(Constants.MAX_ENTITY_SPEED);
			e.Position += e.Velocity * deltaTime;
			e.Position.Y -= Constants.GRAVITY * deltaTime * deltaTime / 2;
			e.Velocity.Y -= Constants.GRAVITY * deltaTime;
		}

		private static (double Distance, Vector Normal) DanToPlane(Vector point, Vector pointOnPlane, Vector planeNormal)
		{
			return
			(
				Distance: (point - pointOnPlane).DotProduct(planeNormal),
				Normal: planeNormal
			);
		}

		private static (double Distance, Vector Normal) DanToSphereInner(Vector point, Vector sphereCenter, double sphereRadius)
		{
			return
			(
				Distance: sphereRadius - (point - sphereCenter).Length,
				Normal: (sphereCenter - point).Normalize
			);
		}

		private static (double Distance, Vector Normal) DanToSphereOuter(Vector point, Vector sphereCenter, double sphereRadius)
		{
			return
			(
				Distance: (point - sphereCenter).Length - sphereRadius,
				Normal: (point - sphereCenter).Normalize
			);
		}

		private static (double Distance, Vector Normal) DanToArena(Vector point)
		{
			var negateX = point.X < 0;
			var negateZ = point.Z < 0;
			if (negateX)
				point.X = -point.X;
			if (negateZ)
				point.Z = -point.Z;
			var result = DanToArenaQuarter(point);
			if (negateX)
				result.Normal.X = -result.Normal.X;
			if (negateZ)
				result.Normal.Z = -result.Normal.Z;
			return result;
		}

		private static (double Distance, Vector Normal) DanToArenaQuarter(Vector point)
		{
			// Ground
			var dan = DanToPlane(point, Vector.FromCoordinates(0, 0, 0), Vector.FromCoordinates(0, 1, 0));
			// Ceiling
			dan = Min(dan,
				DanToPlane(point,
					Vector.FromCoordinates(0, arena.height, 0),
					Vector.FromCoordinates(0, -1, 0)));
			// Side x
			dan = Min(dan,
				DanToPlane(point,
					Vector.FromCoordinates(arena.width / 2, 0, 0),
					Vector.FromCoordinates(-1, 0, 0)));
			// Side z (goal)
			dan = Min(dan,
				DanToPlane(point,
					Vector.FromCoordinates(0, 0, (arena.depth / 2) + arena.goal_depth),
					Vector.FromCoordinates(0, 0, -1)));
			// Side z
			var v = Vector.FromCoordinates(point.X, point.Y, 0) - Vector.FromCoordinates(
				        (arena.goal_width / 2) - arena.goal_top_radius,
				        arena.goal_height - arena.goal_top_radius,
				        0);
			if ((point.X >= (arena.goal_width / 2) + arena.goal_side_radius)
			    || (point.Y >= arena.goal_height + arena.goal_side_radius)
			    || (v.X > 0 && v.Y > 0 && v.Length >= arena.goal_top_radius + arena.goal_side_radius))
			{
				dan = Min(dan,
					DanToPlane(point,
						Vector.FromCoordinates(0, 0, arena.depth / 2),
						Vector.FromCoordinates(0, 0, -1)));
			}

			// Side x & ceiling (goal)
			if (point.Z >= (arena.depth / 2) + arena.goal_side_radius)
			{
				// x
				dan = Min(dan,
					DanToPlane(point,
						Vector.FromCoordinates(arena.goal_width / 2, 0, 0),
						Vector.FromCoordinates(-1, 0, 0)));
				// y
				dan = Min(dan,
					DanToPlane(point,
						Vector.FromCoordinates(0, arena.goal_height, 0),
						Vector.FromCoordinates(0, -1, 0)));
			}

			// Goal back corners
			//todo: Assert arena.bottom_radius == arena.goal_top_radius
			if (point.Z > (arena.depth / 2) + arena.goal_depth - arena.bottom_radius)
			{
				dan = Min(dan, DanToSphereInner(
					point,
					Vector.FromCoordinates(
						point.X.Clamp(arena.bottom_radius - (arena.goal_width / 2),
							(arena.goal_width / 2) - arena.bottom_radius),
						point.Y.Clamp(arena.bottom_radius, arena.goal_height - arena.goal_top_radius),
						(arena.depth / 2) + arena.goal_depth - arena.bottom_radius),
					arena.bottom_radius));
			}

			// Corner
			if (point.X > (arena.width / 2) - arena.corner_radius
			    && point.Z > (arena.depth / 2) - arena.corner_radius)
			{
				dan = Min(dan,
					DanToSphereInner(point,
						Vector.FromCoordinates((arena.width / 2) - arena.corner_radius,
							point.Y,
							(arena.depth / 2) - arena.corner_radius),
						arena.corner_radius));
			}

			// Goal outer corner
			if (point.Z < (arena.depth / 2) + arena.goal_side_radius)
			{
				// Side x
				if (point.X < (arena.goal_width / 2) + arena.goal_side_radius)
				{
					dan = Min(dan, DanToSphereOuter(
						point,
						Vector.FromCoordinates(
							(arena.goal_width / 2) + arena.goal_side_radius,
							point.Y,
							(arena.depth / 2) + arena.goal_side_radius
						),
						arena.goal_side_radius));
				}

				// Ceiling
				if (point.Y < arena.goal_height + arena.goal_side_radius)
				{
					dan = Min(dan, DanToSphereOuter(
						point,
						Vector.FromCoordinates(
							point.X,
							arena.goal_height + arena.goal_side_radius,
							(arena.depth / 2) + arena.goal_side_radius
						),
						arena.goal_side_radius));
				}

				// Top corner
				var o = Vector.FromCoordinates(
					(arena.goal_width / 2) - arena.goal_top_radius,
					arena.goal_height - arena.goal_top_radius,
					0);
				v = Vector.FromCoordinates(point.X, point.Y, 0) - o;
				if (v.X > 0 && v.Y > 0)
				{
					o = o + v.Normalize * (arena.goal_top_radius + arena.goal_side_radius);
					dan = Min(dan,
						DanToSphereOuter(point,
							Vector.FromCoordinates(o.X, o.Y, (arena.depth / 2) + arena.goal_side_radius),
							arena.goal_side_radius));
				}
			}

			// Goal inside top corners
			if (point.Z > (arena.depth / 2) + arena.goal_side_radius
			    && point.Y > arena.goal_height - arena.goal_top_radius)
			{
				// Side x
				if (point.X > (arena.goal_width / 2) - arena.goal_top_radius)
				{
					dan = Min(dan,
						DanToSphereInner(point,
							Vector.FromCoordinates(
								(arena.goal_width / 2) - arena.goal_top_radius,
								arena.goal_height - arena.goal_top_radius,
								point.Z),
							arena.goal_top_radius));
				}

				// Side z
				if (point.Z > (arena.depth / 2) + arena.goal_depth - arena.goal_top_radius)
				{
					dan = Min(dan,
						DanToSphereInner(point,
							Vector.FromCoordinates(point.X,
								arena.goal_height - arena.goal_top_radius,
								(arena.depth / 2) + arena.goal_depth - arena.goal_top_radius),
							arena.goal_top_radius));
				}

				// Bottom corners
				if (point.Y < arena.bottom_radius)
				{
					// Side x
					if (point.X > (arena.width / 2) - arena.bottom_radius)
					{
						dan = Min(dan,
							DanToSphereInner(point,
								Vector.FromCoordinates((arena.width / 2) - arena.bottom_radius,
									arena.bottom_radius,
									point.Z),
								arena.bottom_radius));
					}

					// Side z
					if (point.Z > (arena.depth / 2) - arena.bottom_radius
					    && point.X >= (arena.goal_width / 2) + arena.goal_side_radius)
					{
						dan = Min(dan,
							DanToSphereInner(point,
								Vector.FromCoordinates(
									point.X,
									arena.bottom_radius,
									(arena.depth / 2) - arena.bottom_radius
								),
								arena.bottom_radius));
					}

					// Side z (goal)
					if (point.Z > (arena.depth / 2) + arena.goal_depth - arena.bottom_radius)
					{
						dan = Min(dan,
							DanToSphereInner(point,
								Vector.FromCoordinates(point.X,
									arena.bottom_radius,
									(arena.depth / 2) + arena.goal_depth - arena.bottom_radius
								),
								arena.bottom_radius));
					}

					// Goal outer corner
					var o = Vector.FromCoordinates(
						(arena.goal_width / 2) + arena.goal_side_radius,
						(arena.depth / 2) + arena.goal_side_radius,
						0);
					v = Vector.FromCoordinates(point.X, point.Z, 0) - o;
					if (v.X < 0 && v.Y < 0 && v.Length < arena.goal_side_radius + arena.bottom_radius)
					{
						o = o + v.Normalize * (arena.goal_side_radius + arena.bottom_radius);
						dan = Min(dan,
							DanToSphereInner(point,
								Vector.FromCoordinates(o.X, arena.bottom_radius, o.Y),
								arena.bottom_radius));
					}

					// Side x (goal)
					if (point.Z >= (arena.depth / 2) + arena.goal_side_radius
					    && point.X > (arena.goal_width / 2) - arena.bottom_radius)
					{
						dan = Min(dan,
							DanToSphereInner(point,
								Vector.FromCoordinates(
									(arena.goal_width / 2) - arena.bottom_radius,
									arena.bottom_radius,
									point.Z),
								arena.bottom_radius));
					}

					// Corner
					if (point.X > (arena.width / 2) - arena.corner_radius
					    && point.Z > (arena.depth / 2) - arena.corner_radius)
					{
						var cornerO = Vector.FromCoordinates(
							(arena.width / 2) - arena.corner_radius,
							(arena.depth / 2) - arena.corner_radius,
							0);
						var n = Vector.FromCoordinates(point.X, point.Z, 0) - cornerO;
						var dist = n.Length;
						if (dist > arena.corner_radius - arena.bottom_radius)
						{
							n = n / dist;
							var o2 = cornerO + n * (arena.corner_radius - arena.bottom_radius);
							dan = Min(dan,
								DanToSphereInner(point,
									Vector.FromCoordinates(o2.X, arena.bottom_radius, o2.Y),
									arena.bottom_radius));

						}
					}

					// Ceiling corners
					if (point.Y > arena.height - arena.top_radius)
					{
						// Side x
						if (point.X > (arena.width / 2) - arena.top_radius)
						{
							dan = Min(dan,
								DanToSphereInner(point,
									Vector.FromCoordinates(
										(arena.width / 2) - arena.top_radius,
										arena.height - arena.top_radius,
										point.Z),
									arena.top_radius));
						}

						// Side z
						if (point.Z > (arena.depth / 2) - arena.top_radius)
						{
							dan = Min(dan,
								DanToSphereInner(point,
									Vector.FromCoordinates(
										point.X,
										arena.height - arena.top_radius,
										(arena.depth / 2) - arena.top_radius),
									arena.top_radius));
						}

						// Corner
						if (point.X > (arena.width / 2) - arena.corner_radius
						    && point.Z > (arena.depth / 2) - arena.corner_radius)
						{
							var cornerO = Vector.FromCoordinates(
								(arena.width / 2) - arena.corner_radius,
								(arena.depth / 2) - arena.corner_radius,
								0);
							var dv = Vector.FromCoordinates(point.X, point.Z, 0) - cornerO;
							if (dv.Length > arena.corner_radius - arena.top_radius)
							{
								var n = dv.Normalize;
								var o2 = cornerO + n * (arena.corner_radius - arena.top_radius);
								dan = Min(dan,
									DanToSphereInner(point,
										Vector.FromCoordinates(o2.X, arena.height - arena.top_radius, o2.Y),
										arena.top_radius));
							}
						}
					}
				}
			}

			return dan;
		}

		private static (double Distance, Vector Normal) Min((double Distance, Vector Normal) first, (double Distance, Vector Normal) second)
		{
			if (first.Distance > second.Distance)
				return first;
			if (first.Distance < second.Distance)
				return second;
			// note: common sense tells that this line should not be executed, but it's RAIC...
			return rnd.Next(2) == 1
				? first
				: second;
		}
	}
}