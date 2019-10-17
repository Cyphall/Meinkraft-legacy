using System;
using OpenGL;
using SFML.Window;

namespace Meinkraft
{
	class Program
	{
		static void Main(string[] args)
		{
			Gl.Initialize();
			
			VideoMode mode = VideoMode.DesktopMode;
			Window window = new Window(
				mode,
				"Meinkraft",
				Styles.Fullscreen,
				new ContextSettings(24, 0, 0, 3, 3, ContextSettings.Attribute.Default, false)
			);
			
			window.SetVerticalSyncEnabled(true);
			window.SetKeyRepeatEnabled(false);
			window.SetMouseCursorVisible(false);
			window.SetActive();
			

			Gl.Enable(EnableCap.DepthTest);
			
			Gl.Enable(EnableCap.CullFace);
			Gl.FrontFace(FrontFaceDirection.Cw);
			
			Gl.ClearColor(0x87 / 255.0f, 0xCE / 255.0f, 0xFA / 255.0f, 0xFF / 255.0f);
			
			World world = new World(window);
			
			bool running = true;
			window.Closed += (sender, eventArgs) => { running = false;};
			window.KeyPressed += (sender, eventArgs) => { if (eventArgs.Code == Keyboard.Key.Escape) running = false; };
			while(running)
			{
				window.DispatchEvents();

				world.update();
		
				Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				
				world.render();
		
				window.Display();
			}
			
			world.Dispose();
		}
	}
}