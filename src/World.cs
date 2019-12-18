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
		private readonly ChunkGenerator _chunkGenerator = new ChunkGenerator();

		public Dictionary<ivec3, Chunk> chunks { get; } = new Dictionary<ivec3, Chunk>();

		private readonly List<ivec3> _chunksToCreate = new List<ivec3>(106);

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
				chunk.destroyed = true;
				if (chunk.initialized)
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

		public void setBlock(ivec3 blockPos, byte blockType)
		{
			ivec3 chunkPos = MathUtils.chunkPosFromBlockPos(blockPos);

			if (!chunks.ContainsKey(chunkPos))
			{
				Console.Error.WriteLine("Chunk where block is placed doesn't exists");
				return;
			}

			chunks[chunkPos].setBlock(MathUtils.localBlockPosFromBlockPos(blockPos), blockType);
		}

		public void update()
		{
			_cameras.current.update();

			ivec3 playerChunk = MathUtils.chunkPosFromPlayerPos(_cameras.current.position);
			float renderDistance = 16;
			
			
			// Deleting chunks out of the render distance
			List<ivec3> chunksToRemove = chunks.Keys.Where(chunkPos => ivec3.Distance(chunkPos, playerChunk) > renderDistance + 0.1f).ToList();

			foreach (ivec3 chunkPos in chunksToRemove)
			{
				Chunk chunk = chunks[chunkPos];
				chunks.Remove(chunkPos);
				chunk.destroyed = true;
				if (chunk.initialized)
					chunk.Dispose();
			}
			

			// Creating missing chunks that are within render distance
			if (chunks.Count == 0)
				_chunksToCreate.Add(playerChunk);

			foreach (KeyValuePair<ivec3, Chunk> pair in chunks)
			{
				ivec3 xpos = pair.Key + new ivec3(1, 0, 0);
				if (!_chunksToCreate.Contains(xpos) && !chunks.ContainsKey(xpos) && ivec3.Distance(xpos, playerChunk) < renderDistance)
					_chunksToCreate.Add(xpos);

				ivec3 xneg = pair.Key + new ivec3(-1, 0, 0);
				if (!_chunksToCreate.Contains(xneg) && !chunks.ContainsKey(xneg) && ivec3.Distance(xneg, playerChunk) < renderDistance)
					_chunksToCreate.Add(xneg);
				
				ivec3 ypos = pair.Key + new ivec3(0, 1, 0);
				if (!_chunksToCreate.Contains(ypos) && !chunks.ContainsKey(ypos) && ivec3.Distance(ypos, playerChunk) < renderDistance)
					_chunksToCreate.Add(ypos);
				
				ivec3 yneg = pair.Key + new ivec3(0, -1, 0);
				if (!_chunksToCreate.Contains(yneg) && !chunks.ContainsKey(yneg) && ivec3.Distance(yneg, playerChunk) < renderDistance)
					_chunksToCreate.Add(yneg);
				
				ivec3 zpos = pair.Key + new ivec3(0, 0, 1);
				if (!_chunksToCreate.Contains(zpos) && !chunks.ContainsKey(zpos) && ivec3.Distance(zpos, playerChunk) < renderDistance)
					_chunksToCreate.Add(zpos);
				
				ivec3 zneg = pair.Key + new ivec3(0, 0, -1);
				if (!_chunksToCreate.Contains(zneg) && !chunks.ContainsKey(zneg) && ivec3.Distance(zneg, playerChunk) < renderDistance)
					_chunksToCreate.Add(zneg);

				// Limit: 100 new chunks per frame
				if (_chunksToCreate.Count > 100) break;
			}
			
			_chunksToCreate.ForEach(c => createChunk(c));
			
			_chunksToCreate.Clear();
		}
	}
}