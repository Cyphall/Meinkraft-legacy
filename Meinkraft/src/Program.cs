﻿using System;
using System.Threading;
using GLFW;
using OpenGL;

namespace Meinkraft
{
	class Program
	{
		static void Main()
		{
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
			
			Gl.Initialize();
			Glfw.Init();
			
			Glfw.SetErrorCallback((code, message) => Console.WriteLine(code));
			
			Glfw.WindowHint(Hint.Resizable, false);
			Glfw.WindowHint(Hint.Visible, false);

			VideoMode mode = Glfw.GetVideoMode(Glfw.PrimaryMonitor);
			Window window = Glfw.CreateWindow(mode.Width, mode.Height, "Meinkraft", Glfw.PrimaryMonitor, Window.None);
			
			Glfw.MakeContextCurrent(window);
			Glfw.SetInputMode(window, InputMode.Cursor, (int)CursorMode.Disabled);
			
			
			Gl.Enable(EnableCap.DepthTest);
			
			Gl.Enable(EnableCap.CullFace);
			Gl.FrontFace(FrontFaceDirection.Cw);
			
			Gl.ClearColor(0x87 / 255.0f, 0xCE / 255.0f, 0xFA / 255.0f, 0xFF / 255.0f);
			
			ToolBox.world = new World(window);
			
			while(!Glfw.WindowShouldClose(window))
			{
				Glfw.PollEvents();
				
				if (Glfw.GetKey(window, Keys.Escape) == InputState.Press) Glfw.SetWindowShouldClose(window, true);

				ToolBox.world.update();
		
				Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
				
				ToolBox.world.render();
		
				Glfw.SwapBuffers(window);
			}
			
			ToolBox.world.Dispose();
		}
	}
}