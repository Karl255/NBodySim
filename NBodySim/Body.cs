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

		public bool IsVisible(int xStart, int xEnd, int yStart, int yEnd)
			=> (Position.X + Radius > xStart && Position.X - Radius < xEnd)
			&& (Position.Y + Radius > yStart && Position.Y - Radius < yEnd);

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
				List<Body> collidedBodies = new(3);

				do
				{
					// if collided
					if (Vector2.Distance(currentBody.Value.Position, testBody.Value.Position)
						< currentBody.Value.Radius + testBody.Value.Radius)
					{
						anyCollisions = currentCollided = true;
						collidedBodies.Add(testBody.Value);
						bodies.Remove(testBody);

					}

					testHasNext = testBody.Next is not null;
					if (testHasNext)
						testBody = testBody.Next;
				} while (testHasNext);

				if (currentCollided)
				{
					collidedBodies.Add(currentBody.Value);

					bodies.AddBefore(currentBody, CombineBodies(collidedBodies));
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
			int rSum = 0;
			int gSum = 0;
			int bSum = 0;

			for (int i = 0; i < bodies.Count; i++)
			{
				newMass += bodies[i].Mass;
				int rSq = bodies[i].Radius * bodies[i].Radius;
				newRadius += rSq;
				newPosition += bodies[i].Position * bodies[i].Mass;
				newVelocity += bodies[i].Velocity * bodies[i].Mass;
				rSum += bodies[i].Color.R * rSq;
				gSum += bodies[i].Color.G * rSq;
				bSum += bodies[i].Color.B * rSq;
			}

			Color newColor = new(rSum / newRadius, gSum / newRadius, bSum / newRadius);
			newRadius = (int)Math.Round(Math.Sqrt(newRadius));
			newPosition /= newMass;
			newVelocity /= newMass;

			return new(newPosition, newRadius, newMass, newColor, newVelocity);
		}

		public static void SimulateGravity(List<Body> bodies)
		{
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
