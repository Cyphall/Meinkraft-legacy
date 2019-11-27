using System;
using System.Runtime.InteropServices;
using GLFW;
using SharpGL;

namespace Meinkraft
{
	internal class Program
	{
		public static void Main()
		{
			ToolBox.gl = new OpenGL();
			Glfw.Init();
			
			Glfw.SetErrorCallback((code, message) => Console.WriteLine($"GLFW {code}: {Marshal.PtrToStringAnsi(message)}"));
			
			Glfw.WindowHint(Hint.Resizable, false);
			Glfw.WindowHint(Hint.Visible, false);
			Glfw.WindowHint(Hint.ContextVersionMajor, 4);
			Glfw.WindowHint(Hint.ContextVersionMinor, 6);

			VideoMode mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);
			Window window = Glfw.CreateWindow(mode.Width, mode.Height, "Meinkraft", Glfw.PrimaryMonitor, Window.None);
			
			ToolBox.window = window;
			
			Glfw.MakeContextCurrent(window);
			Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
			
			
			ToolBox.gl.Enable(OpenGL.GL_DEPTH_TEST);
			
			ToolBox.gl.Enable(OpenGL.GL_CULL_FACE);
			ToolBox.gl.FrontFace(OpenGL.GL_CW);
			
			ToolBox.gl.ClearColor(0x87 / 255.0f, 0xCE / 255.0f, 0xFA / 255.0f, 0xFF / 255.0f);
			
			ToolBox.world = new World(window);
			
			while(!Glfw.WindowShouldClose(window))
			{
				Glfw.PollEvents();
				
				if (Glfw.GetKey(window, Keys.Escape) == InputState.Press) Glfw.SetWindowShouldClose(window, true);

				ToolBox.world.update();
		
				ToolBox.gl.Clear(OpenGL.GL_COLOR_BUFFER_BIT | OpenGL.GL_DEPTH_BUFFER_BIT);
				
				ToolBox.world.render();
		
				Glfw.SwapBuffers(window);
			}
			
			ToolBox.world.Dispose();
			
			Glfw.Terminate();
		}
	}
}