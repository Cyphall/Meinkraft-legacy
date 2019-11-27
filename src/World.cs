using System;
using System.Collections.Generic;
using System.Linq;
using GLFW;
using GlmSharp;

namespace Meinkraft
{
	public class World : IDisposable
	{
		private readonly CameraManager _cameras = new CameraManager();
		private ChunkGenerator _chunkGenerator = new ChunkGenerator();
		
		public Dictionary<ivec3, Chunk> chunks { get; } = new Dictionary<ivec3, Chunk>();

		public World(Window window)
		{
			_cameras.add("rasterisation", new RasterisationCamera(window, this));
		}

		public void render()
		{
			_cameras.current.render();
		}

		public void Dispose()
		{
			foreach (Chunk chunk in chunks.Values)
			{
				chunk.Dispose();
			}

			_chunkGenerator.Dispose();
		}

		private void createChunk(ivec3 chunkPos, bool showOverrideError = true)
		{
			if (chunks.ContainsKey(chunkPos))
			{
				if (showOverrideError)
					Console.Error.WriteLine("A chunk has been overriden without prior deletion");
				return;
			}
			
			Chunk chunk = new Chunk(chunkPos);
			
			chunks.Add(chunkPos, chunk);
			
			_chunkGenerator.enqueue(chunk);
		}
		
		public void placeBlock(ivec3 blockPos, byte blockType)
		{
			ivec3 chunkPos = MathUtils.chunkPosFromBlockPos(blockPos);

			if (!chunks.ContainsKey(chunkPos))
			{
				Console.Error.WriteLine("Chunk where block is placed doesn't exists");
				return;
			}

			chunks[chunkPos].placeBlock(MathUtils.localBlockPosFromBlockPos(blockPos), blockType);
		}

		public void update()
		{
			_cameras.current.update();

			ivec3 playerChunk = MathUtils.chunkPosFromPlayerPos(_cameras.current.position);

			float renderDistance = 16;
			
			KeyValuePair<ivec3, Chunk>[] chunkRemoveList = chunks.Where(pair => ivec3.Distance(pair.Key, playerChunk) > renderDistance).ToArray();
		
			foreach (KeyValuePair<ivec3, Chunk> pair in chunkRemoveList)
			{
				chunks.Remove(pair.Key);
				pair.Value.destroyed = true;
				if (pair.Value.initialized)
					pair.Value.Dispose();
			}
			
			for (int x = 0; x < renderDistance; x++)
			{
				for (int y = 0; y < renderDistance; y++)
				{
					for (int z = 0; z < renderDistance; z++)
					{
						if (x*x + y*y + z*z > renderDistance * renderDistance) continue;

						ivec3 normal = playerChunk + new ivec3(x, y, z);
						ivec3 symetry = playerChunk - new ivec3(x, y, z);

						createChunk(new ivec3(normal.x, normal.y, normal.z), false);
						createChunk(new ivec3(normal.x, normal.y, symetry.z), false);
						createChunk(new ivec3(normal.x, symetry.y, normal.z), false);
						createChunk(new ivec3(normal.x, symetry.y, symetry.z), false);
						createChunk(new ivec3(symetry.x, normal.y, normal.z), false);
						createChunk(new ivec3(symetry.x, normal.y, symetry.z), false);
						createChunk(new ivec3(symetry.x, symetry.y, normal.z), false);
						createChunk(new ivec3(symetry.x, symetry.y, symetry.z), false);
					}
				}
			}
		}
	}
}