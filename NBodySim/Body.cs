using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace NBodySim
{
	public class Body
	{
		public Vector2 Position;
		public Vector2 Velocity;
		public Texture2D Texture;
		protected int Mass;
		protected int Radius;
		protected Color Color;

		private Vector2 Center;

		private const float G = 1f;

		public Body(Vector2 position, int radius, int mass, Color color, Vector2 initialVelocity)
		{
			Position = position;
			Radius = radius;
			Texture = Util.GenerateCircleTexture(radius, color);
			Center = Texture.Bounds.Center.ToVector2();
			Mass = mass;
			Color = color;
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

		public static List<Body> SimulateCollisions(List<Body> originalBodies)
		{
			if (originalBodies.Count < 2)
				return originalBodies;

			LinkedList<Body> bodies = new(originalBodies);
			bool anyCollisions = false;

			var currentBody = bodies.First;
			bool hasNext;

			do
			{
				var testBody = currentBody.Next;
				bool testHasNext;
				bool currentCollided = false;
				LinkedList<Body> collidedBodies = new();

				do
				{
					// if collided
					if (Vector2.Distance(currentBody.Value.Position, testBody.Value.Position)
						< currentBody.Value.Radius + testBody.Value.Radius)
					{
						anyCollisions = currentCollided = true;
						collidedBodies.AddLast(testBody.Value);
						bodies.Remove(testBody);

					}

					testHasNext = testBody.Next is not null;
					if (testHasNext)
						testBody = testBody.Next;
				} while (testHasNext);

				if (currentCollided)
				{
					collidedBodies.AddLast(currentBody.Value);

					bodies.AddBefore(currentBody, CombineBodies(new(collidedBodies)));
					bodies.Remove(currentBody);
				}

				hasNext = currentBody.Next is not null;
				if (hasNext)
					currentBody = currentBody.Next;
			} while (hasNext && currentBody.Next is not null);

			return anyCollisions ? new(bodies) : originalBodies;
		}

		private static Body CombineBodies(List<Body> bodies)
		{
			int newMass = 0;
			int newRadius = 0;
			Vector2 newPosition = new(0, 0);
			Vector2 newVelocity = new(0, 0);

			for (int i = 0; i < bodies.Count; i++)
			{
				newMass += bodies[i].Mass;
				newRadius += bodies[i].Radius * bodies[i].Radius;
				newPosition += bodies[i].Position * bodies[i].Mass;
				newPosition += bodies[i].Velocity * bodies[i].Mass;
			}

			newRadius = (int)Math.Round(Math.Sqrt(newRadius));
			newPosition /= bodies.Count * newMass;
			newVelocity /= bodies.Count * newMass;

			Color newColor = bodies[0].Color; // TODO: mix all colors

			return new(newPosition, newRadius, newMass, newColor, newVelocity);
		}

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
