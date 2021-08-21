using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace NBodySim
{
	public class Body
	{
		public Vector2 Position;
		public Vector2 Velocity;
		public Texture2D Texture;
		protected int Mass;

		private Vector2 Center;

		private const float G = 1f;

		public Body(Vector2 position, int radius, int mass, Color color, Vector2 initialVelocity)
		{
			Position = position;
			Texture = Util.GenerateCircleTexture(radius, color);
			Center = Texture.Bounds.Center.ToVector2();
			Mass = mass;
			Velocity = initialVelocity;
		}

		public void Draw(SpriteBatch spriteBatch, Vector2 screenSize, Vector2 origin, int scale) => spriteBatch.Draw(
			Texture,
			(-origin + Position - Center) / scale + screenSize / 2,
			null,
			Color.White,
			0,
			new(),
			1f / scale,
			SpriteEffects.None,
			0);

		public void Update() => Position += Velocity;

		public static void SimulateGravity(List<Body> bodies)
		{
			// heavy <-> heavy
			for (int i = 0; i < bodies.Count - 1; i++)
			{
				for (int j = i + 1; j < bodies.Count; j++)
				{
					var body1 = bodies[i];
					var body2 = bodies[j];

					Vector2 t = G * (body2.Position - body1.Position)
						/ (
							  Vector2.Distance(body2.Position, body1.Position)
							* Vector2.DistanceSquared(body2.Position, body1.Position)
						);

					body1.Velocity += body2.Mass * t;
					body2.Velocity += body1.Mass * -t;
				}
			}
		}
	}
}
