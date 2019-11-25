using System;
using GlmSharp;
using OpenGL;

namespace Meinkraft
{
	public class Chunk : IDisposable
	{
		private NativeArray<byte> _blocks;

		private readonly ivec3 _pos;
		private readonly mat4 _model = mat4.Identity;

		private int _verticesCount;

		private readonly uint _verticesBufferID;
		private readonly uint _uvsBufferID;
		private readonly uint _normalsBufferID;

		private readonly uint _vaoID;
		
		public bool initialized { get; private set; }
		public bool destroyed { get; set; }

		public Chunk(ivec3 chunkPos)
		{
			_pos = chunkPos;

			_model[3, 0] = _pos.x * 16;
			_model[3, 1] = _pos.y * 16;
			_model[3, 2] = _pos.z * 16;

			_verticesBufferID = Gl.GenBuffer();
			_uvsBufferID = Gl.GenBuffer();
			_normalsBufferID = Gl.GenBuffer();

			_vaoID = Gl.GenVertexArray();

			Gl.BindVertexArray(_vaoID);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
			Gl.VertexAttribIPointer(0, 3, VertexAttribType.UnsignedByte, 0, IntPtr.Zero);
			Gl.EnableVertexAttribArray(0);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
			Gl.VertexAttribPointer(1, 2, VertexAttribType.HalfFloat, false, 0, IntPtr.Zero);
			Gl.EnableVertexAttribArray(1);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
			Gl.VertexAttribIPointer(2, 3, VertexAttribType.Byte, 0, IntPtr.Zero);
			Gl.EnableVertexAttribArray(2);

			Gl.BindVertexArray(0);
		}

		public void Dispose()
		{
			Gl.DeleteVertexArrays(_vaoID);
			Gl.DeleteBuffers(_verticesBufferID);
			Gl.DeleteBuffers(_uvsBufferID);
			Gl.DeleteBuffers(_normalsBufferID);
			
			_blocks.Dispose();
		}

		public void render(mat4 viewProjection, uint shaderID)
		{
			if (!initialized) return;

			mat4 mvp = viewProjection * _model;

			Gl.UniformMatrix4f(Gl.GetUniformLocation(shaderID, "modelViewProjection"), 1, false, mvp);

			Gl.BindVertexArray(_vaoID);
				Gl.DrawArrays(PrimitiveType.Triangles, 0, _verticesCount);
			Gl.BindVertexArray(0);
		}

		public void initialize()
		{
			_blocks = WorldGeneration.generateChunkBlocks(_pos, WorldGeneration.mountains);

			rebuildMesh();
			
			if (destroyed)
				Dispose();
			else
				initialized = true;
		}

		public byte getBlock(int x, int y, int z)
		{
			return _blocks[x + y * 16 + z * 256];
		}

		private void rebuildMesh()
		{
			NativeList<byte> vertices = new NativeList<byte>(); // byte3
			NativeList<Half> uvs = new NativeList<Half>(); // float2
			NativeList<sbyte> normals = new NativeList<sbyte>(); // sbyte3s

			_verticesCount = 0;

			for (byte z = 0; z < 16; z++)
			{
				for (byte y = 0; y < 16; y++)
				{
					for (byte x = 0; x < 16; x++)
					{
						if (getBlock(x, y, z) == BlockType.AIR) continue;

						vec2 uvOffset = BlockType.get(getBlock(x, y, z)).uvOffset;

						// x + 1
						if ((x + 1 < 16 && getBlock(x+1, y, z) == BlockType.AIR) || x + 1 == 16)
						{
							_verticesCount += 6;
							vertices.add((byte) (x + 1), y, (byte) (z + 1));
							vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));
							vertices.add((byte) (x + 1), y, z);
							vertices.add((byte) (x + 1), y, z);
							vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));
							vertices.add((byte) (x + 1), (byte) (y + 1), z);

							uvs.add((Half)(uvOffset.x + 0.1875f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.1875f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.25f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.25f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.1875f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.25f), (Half)(uvOffset.y + 0.125f));

							normals.add(1, 0, 0);
							normals.add(1, 0, 0);
							normals.add(1, 0, 0);
							normals.add(1, 0, 0);
							normals.add(1, 0, 0);
							normals.add(1, 0, 0);
						}

						// x - 1
						if ((x - 1 > -1 && getBlock(x-1, y, z) == BlockType.AIR) || x - 1 == -1)
						{
							_verticesCount += 6;
							vertices.add(x, y, z);
							vertices.add(x, (byte) (y + 1), z);
							vertices.add(x, y, (byte) (z + 1));
							vertices.add(x, y, (byte) (z + 1));
							vertices.add(x, (byte) (y + 1), z);
							vertices.add(x, (byte) (y + 1), (byte) (z + 1));

							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.125f));

							normals.add(-1, 0, 0);
							normals.add(-1, 0, 0);
							normals.add(-1, 0, 0);
							normals.add(-1, 0, 0);
							normals.add(-1, 0, 0);
							normals.add(-1, 0, 0);
						}

						// y + 1
						if ((y + 1 < 16 && getBlock(x, y+1, z) == BlockType.AIR) || y + 1 == 16)
						{
							_verticesCount += 6;
							vertices.add(x, (byte) (y + 1), z);
							vertices.add((byte) (x + 1), (byte) (y + 1), z);
							vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							vertices.add((byte) (x + 1), (byte) (y + 1), z);
							vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));

							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.1875f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.1875f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.1875f));

							normals.add(0, 1, 0);
							normals.add(0, 1, 0);
							normals.add(0, 1, 0);
							normals.add(0, 1, 0);
							normals.add(0, 1, 0);
							normals.add(0, 1, 0);
						}

						// y - 1
						if ((y - 1 > -1 && getBlock(x, y-1, z) == BlockType.AIR) || y - 1 == -1)
						{
							_verticesCount += 6;
							vertices.add((byte) (x + 1), y, z);
							vertices.add(x, y, z);
							vertices.add((byte) (x + 1), y, (byte) (z + 1));
							vertices.add((byte) (x + 1), y, (byte) (z + 1));
							vertices.add(x, y, z);
							vertices.add(x, y, (byte) (z + 1));

							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.0f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.0f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.0f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.0625f));

							normals.add(0, -1, 0);
							normals.add(0, -1, 0);
							normals.add(0, -1, 0);
							normals.add(0, -1, 0);
							normals.add(0, -1, 0);
							normals.add(0, -1, 0);
						}

						// z + 1
						if ((z + 1 < 16 && getBlock(x, y, z+1) == BlockType.AIR) || z + 1 == 16)
						{
							_verticesCount += 6;
							vertices.add(x, y, (byte) (z + 1));
							vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							vertices.add((byte) (x + 1), y, (byte) (z + 1));
							vertices.add((byte) (x + 1), y, (byte) (z + 1));
							vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));

							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.1875f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.1875f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.125f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.1875f), (Half)(uvOffset.y + 0.125f));

							normals.add(0, 0, 1);
							normals.add(0, 0, 1);
							normals.add(0, 0, 1);
							normals.add(0, 0, 1);
							normals.add(0, 0, 1);
							normals.add(0, 0, 1);
						}

						// z - 1
						if ((z - 1 > -1 && getBlock(x, y, z-1) == BlockType.AIR) || z - 1 == -1)
						{
							_verticesCount += 6;
							vertices.add((byte) (x + 1), y, z);
							vertices.add((byte) (x + 1), (byte) (y + 1), z);
							vertices.add(x, y, z);
							vertices.add(x, y, z);
							vertices.add((byte) (x + 1), (byte) (y + 1), z);
							vertices.add(x, (byte) (y + 1), z);

							uvs.add((Half)(uvOffset.x + 0.0f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.0f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.0625f));
							uvs.add((Half)(uvOffset.x + 0.0f), (Half)(uvOffset.y + 0.125f));
							uvs.add((Half)(uvOffset.x + 0.0625f), (Half)(uvOffset.y + 0.125f));

							normals.add(0, 0, -1);
							normals.add(0, 0, -1);
							normals.add(0, 0, -1);
							normals.add(0, 0, -1);
							normals.add(0, 0, -1);
							normals.add(0, 0, -1);
						}
					}
				}
			}
			
			Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, vertices.size, vertices, BufferUsage.DynamicDraw);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, uvs.size * 2, uvs, BufferUsage.DynamicDraw);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, normals.size, normals, BufferUsage.DynamicDraw);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
			
			vertices.Dispose();
			uvs.Dispose();
			normals.Dispose();
		}

		private void setBlock(ivec3 blockPos, byte blockType)
		{
			if (_pos.y < 0 || _pos.y > 255)
			{
				Console.Error.WriteLine("Cannot place a block bellow height 0 or above height 255");
				return;
			}

			_blocks[blockPos.y + blockPos.z * 255 + blockPos.x * 4080] = blockType;
		}

		public void placeBlock(ivec3 blockPos, byte blockType)
		{
			setBlock(blockPos, blockType);

			rebuildMesh();
		}
	}
}