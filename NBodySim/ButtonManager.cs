using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace NBodySim
{
	public class ButtonManager
	{
		public record Button(Vector2 Offset, string Text, Action Callback);

		public const int Size = 17;
		public Button[] ButtonsTL;
		public Button[] ButtonsTR;
		private Texture2D Texture;

		public ButtonManager(Texture2D texture) => Texture = texture;

		public bool Click(Point position, Point windowSize)
		{
			bool hit = false;
			int xr = windowSize.X - position.X;
			int xy = windowSize.Y - position.Y;

			for (int i= 0; i < ButtonsTL.Length; i++)
			{
				if (position.X >= ButtonsTL[i].Offset.X && position.X < ButtonsTL[i].Offset.X + Size &&
					position.Y >= ButtonsTL[i].Offset.Y && position.Y < ButtonsTL[i].Offset.Y + Size)
				{
					ButtonsTL[i].Callback();
					hit = true;
				}
			}

			for (int i= 0; i < ButtonsTR.Length; i++)
			{
				if (windowSize.X - position.X >= ButtonsTR[i].Offset.X && windowSize.X - position.X < ButtonsTR[i].Offset.X + Size &&
					position.Y >= ButtonsTR[i].Offset.Y && position.Y < ButtonsTR[i].Offset.Y + Size)
				{
					ButtonsTR[i].Callback();
					hit = true;
				}
			}

			return hit;
		}

		public void Draw(SpriteBatch spriteBatch, SpriteFont font, Vector2 windowSize)
		{
			for (int i= 0; i < ButtonsTL.Length; i++)
			{
				spriteBatch.Draw(Texture, ButtonsTL[i].Offset, Color.White);
				int textWidth = (int)font.MeasureString(ButtonsTL[i].Text).X;
				spriteBatch.DrawString(font,
					ButtonsTL[i].Text,
					new(
						ButtonsTL[i].Offset.X + Size / 2 - textWidth / 2,
						ButtonsTL[i].Offset.Y -2
					),
					Color.White);
			}

			for (int i= 0; i < ButtonsTR.Length; i++)
			{
				spriteBatch.Draw(Texture, new Vector2(windowSize.X - ButtonsTR[i].Offset.X - Size, ButtonsTR[i].Offset.Y), Color.White);
				int textWidth = (int)font.MeasureString(ButtonsTR[i].Text).X;
				spriteBatch.DrawString(font,
					ButtonsTR[i].Text,
					new(
						windowSize.X - ButtonsTR[i].Offset.X - Size / 2 - textWidth / 2,
						ButtonsTR[i].Offset.Y -2
					),
					Color.White);
			}
		}
	}
}
