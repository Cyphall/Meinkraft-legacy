using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GlmSharp;
using SFML.Window;

namespace Meinkraft
{
	public class World : IDisposable
	{
		private readonly CameraManager _cameras = new CameraManager();
		
		public Dictionary<ivec2, Chunk> chunks { get; } = new Dictionary<ivec2, Chunk>();
		private readonly BlockingCollection<Chunk> _applyQueue = new BlockingCollection<Chunk>(8);

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
			foreach (KeyValuePair<ivec2, Chunk> keyValuePair in chunks)
			{
				keyValuePair.Value.Dispose();
			}
		}

		private void createChunk(ivec2 chunkPos, bool overrideAlert = true)
		{
			if (chunks.ContainsKey(chunkPos))
			{
				if (overrideAlert)
					Console.Error.WriteLine("A chunk has been overriden without prior deletion");
				return;
			}
			
			Chunk chunk = new Chunk(chunkPos);
			
			chunks.Add(chunkPos, chunk);
			
			ThreadPool.QueueUserWorkItem(_ =>
			{
				chunk.initialize();
				_applyQueue.Add(chunk);
			});
		}
		
		public void placeBlock(ivec3 blockPos, byte blockType)
		{
			ivec2 chunkPos = chunkPosFromBlockPos(blockPos);

			if (!chunks.ContainsKey(chunkPos))
			{
				Console.Error.WriteLine("Chunk where block is placed doesn't exists");
				return;
			}

			chunks[chunkPos].placeBlock(localBlockPosFromBlockPos(blockPos), blockType);
		}

		private static ivec2 chunkPosFromBlockPos(ivec3 blockPos)
		{
			ivec2 chunkPos = ivec2.Zero;
			
			chunkPos.x = (int)glm.Floor(blockPos.x / 16.0f);
			chunkPos.y = (int)glm.Floor(blockPos.z / 16.0f);

			return chunkPos;
		}

		private static ivec3 localBlockPosFromBlockPos(ivec3 blockPos)
		{
			ivec3 localBlockPos = ivec3.Zero;

			localBlockPos.x = MathUtils.mod(blockPos.x, 16);
			localBlockPos.y = blockPos.y;
			localBlockPos.z = MathUtils.mod(blockPos.z, 16);

			return localBlockPos;
		}
	
		private static ivec2 chunkPosFromPlayerPos(dvec3 playerPos)
		{
			ivec2 chunkPos = ivec2.Zero;

			chunkPos.x = (int)glm.Floor(playerPos.x / 16.0f);
			chunkPos.y = (int)glm.Floor(playerPos.z / 16.0f);

			return chunkPos;
		}
		
		public void update()
		{
			_cameras.current.update();

			ivec2 chunkWithPlayer = chunkPosFromPlayerPos(_cameras.current.position);

			float effectiveRenderDistance = 8 - 0.1f;
			
			KeyValuePair<ivec2, Chunk>[] chunkRemoveList = chunks.Where(pair => ivec2.Distance(pair.Key, chunkWithPlayer) > effectiveRenderDistance).ToArray();
		
			foreach (KeyValuePair<ivec2, Chunk> keyValuePair in chunkRemoveList)
			{
				chunks.Remove(keyValuePair.Key);
				keyValuePair.Value.Dispose();
			}
			
			for (int x = chunkWithPlayer.x ; x <= chunkWithPlayer.x + effectiveRenderDistance; x++)
			{
				for (int y = chunkWithPlayer.y ; y <= chunkWithPlayer.y + effectiveRenderDistance; y++)
				{
					if ((chunkWithPlayer.x - x) * (chunkWithPlayer.x - x) + (chunkWithPlayer.y - y) * (chunkWithPlayer.y - y) > effectiveRenderDistance * effectiveRenderDistance) continue;
				
					int xSym = chunkWithPlayer.x - (x - chunkWithPlayer.x);
					int ySym = chunkWithPlayer.y - (y - chunkWithPlayer.y);

					createChunk(new ivec2(x, y), false);
					createChunk(new ivec2(x, ySym), false);
					createChunk(new ivec2(xSym, y), false);
					createChunk(new ivec2(xSym, ySym), false);
				}
			}
			
			if (_applyQueue.TryTake(out Chunk chunk))
			{
				chunk.applyMesh();
			}
		}
	}
}