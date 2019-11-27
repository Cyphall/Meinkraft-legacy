using System;
using System.Collections.Concurrent;
using System.Threading;
using GLFW;
using SharpGL;
using Monitor = GLFW.Monitor;

namespace Meinkraft
{
	public class ChunkGenerator : IDisposable
	{
		private readonly ThreadParams _parameters;
		private readonly BlockingCollection<Chunk> _chunkList = new BlockingCollection<Chunk>();
		
		public ChunkGenerator()
		{
			Thread generator = new Thread(p =>
			{
				ThreadParams parameters = ((ThreadParams) p);
			
			
				BlockingCollection<Chunk> chunkList = parameters.chunkList;
			
				OpenGL gl = new OpenGL();
				gl.MakeCurrent();

				Glfw.MakeContextCurrent(parameters.window);

				while (parameters.running)
					chunkList.Take().initialize(gl);
			}) {IsBackground = true};

			_parameters = new ThreadParams(_chunkList, Glfw.CreateWindow(1, 1, "", Monitor.None, ToolBox.window));
			
			generator.Start(_parameters);
		}

		public void enqueue(Chunk chunk)
		{
			_chunkList.Add(chunk);
		}

		public void Dispose()
		{
			_parameters.running = false;
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