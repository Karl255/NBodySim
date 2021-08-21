using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NBodySim
{
	public static class Util
	{
		public static GraphicsDevice GraphicsDevice;

		public static Texture2D GenerateCircleTexture(int radius, Color color)
		{
			int dimension = 2 * radius - 1;
			(int x, int y) center = (radius - 1, radius - 1);
			Color[] data = new Color[dimension * dimension];

			for (int y = 0; y < dimension; y++)
			{
				for (int x = 0; x < dimension; x++)
				{
					int dx = center.x - x;
					int dy = center.y - y;

					data[y * dimension + x] = Math.Sqrt(dx * dx + dy * dy) > radius ? Color.Transparent : color;
				}
			}

			Texture2D texture = new(GraphicsDevice, dimension, dimension);
			texture.SetData(data);

			return texture;
		}
	}
}
