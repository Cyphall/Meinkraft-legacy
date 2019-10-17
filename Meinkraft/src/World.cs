
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GlmSharp;
using OpenGL;
using SFML.Window;

namespace Meinkraft
{
	public class World : IDisposable
	{
		private Window _window;
		private Shader _chunkShader;
		private Texture _chunkTexture;
		private Camera _camera;
		
		private Dictionary<ivec2, Chunk> _chunks = new Dictionary<ivec2, Chunk>();
		
		private bool _shaderShowNormals = false;
		
		public World(Window window)
		{
			_window = window;
			
			if (File.Exists("resources/shaders/chunk_custom.vert") && File.Exists("resources/shaders/chunk_custom.frag"))
				_chunkShader = new Shader("chunk_custom");
			else
				_chunkShader = new Shader("chunk");
			
			_chunkTexture = new Texture("Block_Texture");
			_camera = new Camera(window);
		}

		public void Dispose()
		{
			_chunkShader.Dispose();
			_chunkTexture.Dispose();
			
			foreach (KeyValuePair<ivec2, Chunk> keyValuePair in _chunks)
			{
				keyValuePair.Value.Dispose();
			}
		}

		public void render()
		{
			mat4 viewProjection = _camera.getViewProjection();
			
			if (_chunkShader.bind())
			{
				if (_chunkTexture.bind())
				{
					Gl.Uniform1i(Gl.GetUniformLocation(_chunkShader.programID, "showNormals"), 1, _shaderShowNormals ? 1 : 0);
					Gl.Uniform3f(Gl.GetUniformLocation(_chunkShader.programID, "cameraPos"), 1, _camera.position);
					
					foreach (KeyValuePair<ivec2, Chunk> keyValuePair in _chunks)
					{
						keyValuePair.Value.render(viewProjection, _chunkShader.programID);
					}
				}
				_chunkTexture.unbind();
			}
			_chunkTexture.unbind();
			
			ErrorCode err;
			while((err = Gl.GetError()) != ErrorCode.NoError)
			{
				Console.Error.WriteLine(err);
			}
		}

		private void createChunk(ivec2 chunkPos, bool overrideAlert = true)
		{
			if (_chunks.ContainsKey(chunkPos))
			{
				if (overrideAlert)
					Console.Error.WriteLine("A chunk has been overriden without prior deletion");
				return;
			}
		
			Chunk chunk = new Chunk(chunkPos);
		
			_chunks.Add(chunkPos, chunk);
		}
		
		public void placeBlock(ivec3 blockPos, byte blockType)
		{
			ivec2 chunkPos = chunkPosFromBlockPos(blockPos);

			if (!_chunks.ContainsKey(chunkPos))
			{
				Console.Error.WriteLine("Chunk where block is placed doesn't exists");
				return;
			}

			_chunks[chunkPos].placeBlock(localBlockPosFromBlockPos(blockPos), blockType);
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
			float effectiveRenderDistance = 8 - 0.1f;
		
			ivec2 chunkWithPlayer = chunkPosFromPlayerPos(_camera.position);

			KeyValuePair<ivec2, Chunk>[] chunkRemoveList = _chunks.Where(pair => ivec2.Distance(pair.Key, chunkWithPlayer) > effectiveRenderDistance).ToArray();
		
			foreach (KeyValuePair<ivec2, Chunk> keyValuePair in chunkRemoveList)
			{
				keyValuePair.Value.Dispose();
				_chunks.Remove(keyValuePair.Key);
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
		}
	}
}