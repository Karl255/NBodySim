using Microsoft.Xna.Framework;
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

		private ButtonManager Buttons;

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

			Bodies = new(256)
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

			int ls = UIFont.LineSpacing;
			int size = ButtonManager.Size;
			Buttons = new(Content.Load<Texture2D>("button"))
			{
				ButtonsTL = new[]
				{
					new ButtonManager.Button(new((int)(120 + 0.25 * size), 5 * ls + 1), "+", () => RadiusInput++),
					new ButtonManager.Button(new((int)(120 + 1.50 * size), 5 * ls + 1), "-", () => RadiusInput = Math.Max(RadiusInput - 1, 1)),
					new ButtonManager.Button(new((int)(120 + 0.25 * size), 6 * ls + 1), "+", () => MassInput += 100),
					new ButtonManager.Button(new((int)(120 + 1.50 * size), 6 * ls + 1), "-", () => MassInput = Math.Max(MassInput - 100, 100)),

					new ButtonManager.Button(new((int)(1.75 * size), 10 * size), "-", () => VelocityYInput -= 0.1f),
					new ButtonManager.Button(new((int)(0.75 * size), 11 * size), "-", () => VelocityXInput -= 0.1f),
					new ButtonManager.Button(new((int)(2.75 * size), 11 * size), "+", () => VelocityXInput += 0.1f),
					new ButtonManager.Button(new((int)(1.75 * size), 12 * size), "+", () => VelocityYInput += 0.1f),
				}
			};
		}

		protected override void Update(GameTime gameTime)
		{
			var mouseState = Mouse.GetState();
			var keyboardState = Keyboard.GetState();

			// left click
			if (mouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Released)
			{
				if (!Buttons.Click(mouseState.Position, Window.ClientBounds.Size))
				{
					Bodies.Add(new(
						Origin + (mouseState.Position.ToVector2() - ScreenSize / 2) * Scale,
						RadiusInput,
						MassInput,
						Util.GetRandomColor(),
						new Vector2(VelocityXInput, VelocityYInput)));
				}
			}

			// right click or middle click
			if (mouseState.RightButton  == ButtonState.Pressed && PreviousMouseState.RightButton  == ButtonState.Pressed ||
				mouseState.MiddleButton == ButtonState.Pressed && PreviousMouseState.MiddleButton == ButtonState.Pressed)
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
				Scale = 2;
			}

			// +/-
			if (keyboardState.IsKeyDown(Keys.Add))
				RadiusInput += 1;
			if (keyboardState.IsKeyDown(Keys.Subtract))
				RadiusInput = Math.Max(1, RadiusInput - 1);

			// page up/page down
			if (keyboardState.IsKeyDown(Keys.PageUp))
				MassInput += 100;
			if (keyboardState.IsKeyDown(Keys.PageDown))
				MassInput = Math.Max(100, MassInput - 100);

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

			// bodies
			for (int i = 0; i < Bodies.Count; i++)
				if (Bodies[i].IsVisible(screenXStart, screenXEnd, screenYStart, screenYEnd))
					Bodies[i].Draw(SpriteBatch, ScreenSize, Origin, Scale);

			// texts
			int row = 0;
			SpriteBatch.DrawString(UIFont, $"Position: ({Origin.X}, {Origin.Y})",                     new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Body count: {Bodies.Count}",                             new(0, row++ * UIFont.LineSpacing), Color.White);
			row++;
			SpriteBatch.DrawString(UIFont, "Input values:",                                           new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Collisions: {(AllowCollisions ? "on" : "off")}",         new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Radius: {RadiusInput}",                                  new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Mass: {MassInput}",                                      new(0, row++ * UIFont.LineSpacing), Color.White);
			SpriteBatch.DrawString(UIFont, $"Velocity: ({VelocityXInput:0.0}, {VelocityYInput:0.0})", new(0, row++ * UIFont.LineSpacing), Color.White);

			// buttons
			Buttons.Draw(SpriteBatch, UIFont, Window.ClientBounds.Size.ToVector2());

			// status
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
