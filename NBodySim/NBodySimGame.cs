using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
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

		private Vector2 Origin = new();
		private int Scale = 4;
		private List<Body> Bodies;

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
				new Body(new Vector2(0, 0), 8, 1000, Color.Yellow, new Vector2(0, 0.7f)),
				new Body(new Vector2(200, 0), 3, 500, Color.Red, new Vector2(0, -1.4f)),
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

			if (mouseState.LeftButton == ButtonState.Pressed && PreviousMouseState.LeftButton == ButtonState.Pressed)
				Origin += (PreviousMouseState.Position - mouseState.Position).ToVector2() * Scale;

			if (mouseState.ScrollWheelValue != PreviousMouseState.ScrollWheelValue)
			{
				int prevScale = Scale;
				Scale = MathHelper.Max(1, Scale + (PreviousMouseState.ScrollWheelValue - mouseState.ScrollWheelValue) / 120);
			}

			PreviousMouseState = mouseState;

			Body.SimulateGravity(Bodies);

			for (int i = 0; i < Bodies.Count; i++)
				Bodies[i].Update();

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			GraphicsDevice.Clear(Color.Black);

			SpriteBatch.Begin();

			for (int i = 0; i < Bodies.Count; i++)
				Bodies[i].Draw(SpriteBatch, ScreenSize, Origin, Scale);

			SpriteBatch.DrawString(UIFont, $"Scale: {Scale}", new(0, 0), Color.White);
			SpriteBatch.DrawString(UIFont, $"Origin: ({Origin.X}, {Origin.Y})", new(0, UIFont.LineSpacing), Color.White);

			SpriteBatch.End();

			base.Draw(gameTime);
		}
	}
}
