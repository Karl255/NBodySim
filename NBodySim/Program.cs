using System;

namespace NBodySim
{
	public static class Program
	{
		[STAThread]
		static void Main()
		{
			using var game = new NBodySimGame();
			game.Run();
		}
	}
}
