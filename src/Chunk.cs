using System;
using System.Linq;
using GlmSharp;
using SharpGL;
using static SharpGL.OpenGL;

namespace Meinkraft
{
	public class Chunk : IDisposable
	{
		private NativeArray<byte> _blocks;

		private readonly ivec3 _pos;
		
		public bool initialized { get; private set; }
		public bool destroyed { get; set; }

		public Chunk(ivec3 chunkPos)
		{
			_pos = chunkPos;
		}

		public void Dispose()
		{
			_blocks.Dispose();
		}


		public void initialize(OpenGL gl)
		{
			_blocks = WorldGeneration.generateChunkBlocks(_pos, WorldGeneration.mountains);
			
			if (destroyed)
				Dispose();
			else
				initialized = true;
		}

		public byte getBlock(int x, int y, int z)
		{
			return _blocks[x + y * 16 + z * 256];
		}

		public void setBlock(ivec3 blockPos, byte blockType)
		{
			if (_pos.y < 0 || _pos.y > 255)
			{
				Console.Error.WriteLine("Cannot place a block bellow height 0 or above height 255");
				return;
			}

			_blocks[blockPos.y + blockPos.z * 255 + blockPos.x * 4080] = blockType;
		}
	}
}