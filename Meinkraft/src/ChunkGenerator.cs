using System;
using System.Collections.Concurrent;
using System.Threading;
using GLFW;
using OpenGL;
using Monitor = GLFW.Monitor;

namespace Meinkraft
{
	public class ChunkGenerator : IDisposable
	{
		private Thread _generator;
		private ThreadParams _parameters;
		private BlockingCollection<Chunk> _chunkList = new BlockingCollection<Chunk>();
		
		public ChunkGenerator()
		{
			_generator = new Thread(threadFunc) {IsBackground = true};

			_parameters = new ThreadParams(_chunkList, Glfw.CreateWindow(1, 1, "", Monitor.None, Window.None));
			
			_generator.Start(_parameters);
		}

		public void enqueue(Chunk chunk)
		{
			_chunkList.Add(chunk);
		}

		public void Dispose()
		{
			_parameters.running = false;
		}
		
		private static void threadFunc(object p)
		{
			ThreadParams parameters = ((ThreadParams) p);
				
				
			BlockingCollection<Chunk> chunkList = parameters.chunkList;

			Glfw.MakeContextCurrent(parameters.window);

			Gl.GenBuffer();
			parameters.running = false;
				
			while (parameters.running)
				chunkList.Take().initialize();
		}
		
		private class ThreadParams
		{
			public BlockingCollection<Chunk> chunkList { get; }
			public Window window { get; }
			public bool running { get; set; }

			public ThreadParams(BlockingCollection<Chunk> chunkList, Window window)
			{
				this.chunkList = chunkList;
				this.window = window;
				running = true;
			}
		}
	}
}