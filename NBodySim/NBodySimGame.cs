﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace NBodySim
{
	public class NBodySimGame : Game
	{
		private GraphicsDeviceManager Graphics;
		private SpriteBatch SpriteBatch;
		private Vector2 ScreenSize = new(0, 0);

		private SpriteFont UIFont;
		private MouseState PreviousMouseState;
		private KeyboardState PreviousKeyboardState;

		private Vector2 Origin = new();
		private int Scale = 2;
		private List<Body> Bodies;

		private bool AllowCollisions = true;
		private bool IsPaused = true;
		private int RadiusInput = 10; // +-1
		private int MassInput = 1000; // +-10
		private float VelocityXInput = 0; // +-0.1f
		private float VelocityYInput = 0; // +-0.1f

		public NBodySimGame()
		{
			Graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = true;
			Window.AllowUserResizing = true;
			Util.GraphicsDevice = GraphicsDevice;
		}

		protected override void Initialize()
		{
			ScreenSize.X = Graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
			ScreenSize.Y = Graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
			Graphics.ApplyChanges();

			Window.AllowUserResizing = true;
			Window.ClientSizeChanged += (o, e) =>
			{
				ScreenSize.X = Graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
				ScreenSize.Y = Graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
				Graphics.ApplyChanges();
			};

			Util.GraphicsDevice = GraphicsDevice;

			Bodies = new()
			{
				//new Body(new Vector2(0, 0), 20, 1000, Color.Yellow, new Vector2(0, 0.7f)),
				//new Body(new Vector2(300, 0), 9, 500, Color.Red, new Vector2(0, -1.4f)),
				//new Body(new Vector2(300, 50), 3, 1, Color.Blue, new Vector2(2.6f, -0.2f)),
			};

			base.Initialize();
		}

		protected override void LoadContent()
		{
			SpriteBatch = new SpriteBatch(GraphicsDevice);
			UIFont = Content.Load<SpriteFont>("uiFont");
		}

		protected override void Update(GameTime gameTime)
		{
			var mouseState = Mouse.GetState();
			var keyboardState = Keyboard.GetState();

			// left click
			if (mouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released)
			{
				Bodies.Add(new(
					Origin + (mouseState.Position.ToVector2() - ScreenSize / 2) * Scale,
					RadiusInput,
					MassInput,
					Util.GetRandomColor(),
					new Vector2(VelocityXInput, VelocityYInput)));
			}

			// right click
			if (mouseState.RightButton == ButtonState.Pressed && PreviousMouseState.RightButton == ButtonState.Pressed)
				Origin += (PreviousMouseState.Position - mouseState.Position).ToVector2() * Scale;

			// scroll
			if (mouseState.ScrollWheelValue != PreviousMouseState.ScrollWheelValue)
				Scale = MathHelper.Max(1, Scale + (PreviousMouseState.ScrollWheelValue - mouseState.ScrollWheelValue) / 120);

			// C
			if (keyboardState.IsKeyDown(Keys.C) && PreviousKeyboardState.IsKeyUp(Keys.C))
				AllowCollisions = !AllowCollisions;

			// P
			if (keyboardState.IsKeyDown(Keys.P) && PreviousKeyboardState.IsKeyUp(Keys.P))
				IsPaused = !IsPaused;

			// R
			if (keyboardState.IsKeyDown(Keys.R) && PreviousKeyboardState.IsKeyUp(Keys.R))
			{
				Bodies.Clear();
				Origin = new(0, 0);
			}

			// +/-
			if (keyboardState.IsKeyDown(Keys.Add))
				RadiusInput += 1;
			if (keyboardState.IsKeyDown(Keys.Subtract))
				RadiusInput = Math.Max(1, RadiusInput - 1);

			// page up/page down
			if (keyboardState.IsKeyDown(Keys.PageUp))
				MassInput += 10;
			if (keyboardState.IsKeyDown(Keys.PageDown))
				MassInput = Math.Max(10, MassInput - 10);

			// right/left
			if (keyboardState.IsKeyDown(Keys.Right))
				VelocityXInput += 0.1f;
			if (keyboardState.IsKeyDown(Keys.Left))
				VelocityXInput -= 0.1f;
			
			// up/down
			if (keyboardState.IsKeyDown(Keys.Up))
				VelocityYInput -= 0.1f;
			if (keyboardState.IsKeyDown(Keys.Down))
				VelocityYInput += 0.1f;

			PreviousMouseState = mouseState;
			PreviousKeyboardState = keyboardState;

			if (!IsPaused)
			{
				if (AllowCollisions)
					Bodies = Body.SimulateCollisions(Bodies);

				Body.SimulateGravity(Bodies);
				for (int i = 0; i < Bodies.Count; i++)
					Bodies[i].Update();
			}

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			SpriteBatch.Begin();

			int screenXStart = (int)Origin.X - (int)ScreenSize.X / 2 * Scale;
			int screenYStart = (int)Origin.Y - (int)ScreenSize.Y / 2 * Scale;
			int screenXEnd   = (int)Origin.X + (int)ScreenSize.X / 2 * Scale;
			int screenYEnd   = (int)Origin.Y + (int)ScreenSize.Y / 2 * Scale;

			for (int i = 0; i < Bodies.Count; i++)
				if (Bodies[i].IsVisible(screenXStart, screenXEnd, screenYStart, screenYEnd))
					Bodies[i].Draw(SpriteBatch, ScreenSize, Origin, Scale);

			int row = 0;
			SpriteBatch.DrawString(UIFont, $"Position: ({Origin.X}, {Origin.Y})", new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Body count: {Bodies.Count}",         new(0, row++ * UIFont.LineSpacing), Color.White);
			row++;
			SpriteBatch.DrawString(UIFont, "Input values:",                                   new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Radius: {RadiusInput}",                          new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Mass: {MassInput}",                              new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Velocity: ({VelocityXInput}, {VelocityYInput})", new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Collisions: {(AllowCollisions ? "on" : "off")}", new(0, row++ * UIFont.LineSpacing), Color.White);

			if (IsPaused)
			{
				string text = "Paused";
				SpriteBatch.DrawString(UIFont, text, new(ScreenSize.X - UIFont.MeasureString(text).X, 0), Color.White);
			}

			SpriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
