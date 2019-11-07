using System;
using GlmSharp;
using OpenGL;

namespace Meinkraft
{
	public class Chunk : IDisposable
	{
		private NativeArray<byte> _blocks;

		private readonly ivec2 _pos;
		private readonly mat4 _model = mat4.Identity;

		private int _verticesCount;

		private readonly uint _verticesBufferID;
		private readonly uint _uvsBufferID;
		private readonly uint _normalsBufferID;

		private readonly uint _vaoID;

		private NativeList<byte> _vertices; // byte3
		private NativeList<float> _uvs; // float2
		private NativeList<sbyte> _normals; // sbyte3s

		private bool _initialized;
		private bool _destroyed;

		public static implicit operator bool(Chunk chunk)
		{
			return !chunk._destroyed;
		}

		public Chunk(ivec2 chunkPos)
		{
			_pos = chunkPos;

			_model[3, 0] = _pos.x * 16;
			_model[3, 2] = _pos.y * 16;

			_verticesBufferID = Gl.GenBuffer();
			_uvsBufferID = Gl.GenBuffer();
			_normalsBufferID = Gl.GenBuffer();

			_vaoID = Gl.GenVertexArray();

			Gl.BindVertexArray(_vaoID);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
			Gl.VertexAttribIPointer(0, 3, VertexAttribType.UnsignedByte, 0, IntPtr.Zero);
			Gl.EnableVertexAttribArray(0);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
			Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 0, IntPtr.Zero);
			Gl.EnableVertexAttribArray(1);

			Gl.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
			Gl.VertexAttribIPointer(2, 3, VertexAttribType.Byte, 0, IntPtr.Zero);
			Gl.EnableVertexAttribArray(2);

			Gl.BindVertexArray(0);
		}

		public void Dispose()
		{
			_destroyed = true;

			Gl.DeleteVertexArrays(_vaoID);
			Gl.DeleteBuffers(_verticesBufferID);
			Gl.DeleteBuffers(_uvsBufferID);
			Gl.DeleteBuffers(_normalsBufferID);
			
			if (_initialized) _blocks.Dispose();
		}

		public void render(mat4 viewProjection, uint shaderID)
		{
			if (!_initialized) return;

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
		}

		public void applyMesh()
		{
			if (!_destroyed)
			{
				Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, _vertices.size * sizeof(byte), _vertices, BufferUsage.StaticDraw);

				Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, _uvs.size * sizeof(float), _uvs, BufferUsage.StaticDraw);

				Gl.BindBuffer(BufferTarget.ArrayBuffer, _normalsBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, _normals.size * sizeof(sbyte), _normals, BufferUsage.StaticDraw);

				Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);
			}
			else
				_blocks.Dispose();

			_vertices.Dispose();
			_uvs.Dispose();
			_normals.Dispose();

			_vertices = null;
			_uvs = null;
			_normals = null;

			_initialized = true;
		}

		private void rebuildMesh()
		{
			_vertices = new NativeList<byte>(1024);
			_uvs = new NativeList<float>(1024);
			_normals = new NativeList<sbyte>(1024);


			_verticesCount = 0;

			for (byte z = 0; z < 16; z++)
			{
				for (byte y = 0; y < 255; y++)
				{
					for (byte x = 0; x < 16; x++)
					{
						if (_blocks[y + z * 255 + x * 4080] == BlockType.AIR) continue;

						vec2 uvOffset = BlockType.get(_blocks[y + z * 255 + x * 4080]).uvOffset;

						// x + 1
						if ((x + 1 < 16 && _blocks[y + z * 255 + (x + 1) * 4080] == BlockType.AIR) || x + 1 == 16)
						{
							_verticesCount += 6;
							_vertices.add((byte) (x + 1), y, (byte) (z + 1));
							_vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));
							_vertices.add((byte) (x + 1), y, z);
							_vertices.add((byte) (x + 1), y, z);
							_vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));
							_vertices.add((byte) (x + 1), (byte) (y + 1), z);

							_uvs.add(uvOffset.x + 0.1875f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.1875f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.25f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.25f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.1875f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.25f, uvOffset.y + 0.125f);

							_normals.add(1, 0, 0);
							_normals.add(1, 0, 0);
							_normals.add(1, 0, 0);
							_normals.add(1, 0, 0);
							_normals.add(1, 0, 0);
							_normals.add(1, 0, 0);
						}

						// x - 1
						if ((x - 1 > -1 && _blocks[y + z * 255 + (x - 1) * 4080] == BlockType.AIR) || x - 1 == -1)
						{
							_verticesCount += 6;
							_vertices.add(x, y, z);
							_vertices.add(x, (byte) (y + 1), z);
							_vertices.add(x, y, (byte) (z + 1));
							_vertices.add(x, y, (byte) (z + 1));
							_vertices.add(x, (byte) (y + 1), z);
							_vertices.add(x, (byte) (y + 1), (byte) (z + 1));

							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.125f);

							_normals.add(-1, 0, 0);
							_normals.add(-1, 0, 0);
							_normals.add(-1, 0, 0);
							_normals.add(-1, 0, 0);
							_normals.add(-1, 0, 0);
							_normals.add(-1, 0, 0);
						}

						// y + 1
						if ((y + 1 < 255 && _blocks[(y + 1) + z * 255 + x * 4080] == BlockType.AIR) || y + 1 == 255)
						{
							_verticesCount += 6;
							_vertices.add(x, (byte) (y + 1), z);
							_vertices.add((byte) (x + 1), (byte) (y + 1), z);
							_vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							_vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							_vertices.add((byte) (x + 1), (byte) (y + 1), z);
							_vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));

							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.1875f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.1875f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.1875f);

							_normals.add(0, 1, 0);
							_normals.add(0, 1, 0);
							_normals.add(0, 1, 0);
							_normals.add(0, 1, 0);
							_normals.add(0, 1, 0);
							_normals.add(0, 1, 0);
						}

						// y - 1
						if ((y - 1 > -1 && _blocks[(y - 1) + z * 255 + x * 4080] == BlockType.AIR) || y - 1 == -1)
						{
							_verticesCount += 6;
							_vertices.add((byte) (x + 1), y, z);
							_vertices.add(x, y, z);
							_vertices.add((byte) (x + 1), y, (byte) (z + 1));
							_vertices.add((byte) (x + 1), y, (byte) (z + 1));
							_vertices.add(x, y, z);
							_vertices.add(x, y, (byte) (z + 1));

							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.0f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.0f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.0f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.0625f);

							_normals.add(0, -1, 0);
							_normals.add(0, -1, 0);
							_normals.add(0, -1, 0);
							_normals.add(0, -1, 0);
							_normals.add(0, -1, 0);
							_normals.add(0, -1, 0);
						}

						// z + 1
						if ((z + 1 < 16 && _blocks[y + (z + 1) * 255 + x * 4080] == BlockType.AIR) || z + 1 == 16)
						{
							_verticesCount += 6;
							_vertices.add(x, y, (byte) (z + 1));
							_vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							_vertices.add((byte) (x + 1), y, (byte) (z + 1));
							_vertices.add((byte) (x + 1), y, (byte) (z + 1));
							_vertices.add(x, (byte) (y + 1), (byte) (z + 1));
							_vertices.add((byte) (x + 1), (byte) (y + 1), (byte) (z + 1));

							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.1875f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.1875f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.125f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.1875f, uvOffset.y + 0.125f);

							_normals.add(0, 0, 1);
							_normals.add(0, 0, 1);
							_normals.add(0, 0, 1);
							_normals.add(0, 0, 1);
							_normals.add(0, 0, 1);
							_normals.add(0, 0, 1);
						}

						// z - 1
						if ((z - 1 > -1 && _blocks[y + (z - 1) * 255 + x * 4080] == BlockType.AIR) || z - 1 == -1)
						{
							_verticesCount += 6;
							_vertices.add((byte) (x + 1), y, z);
							_vertices.add((byte) (x + 1), (byte) (y + 1), z);
							_vertices.add(x, y, z);
							_vertices.add(x, y, z);
							_vertices.add((byte) (x + 1), (byte) (y + 1), z);
							_vertices.add(x, (byte) (y + 1), z);

							_uvs.add(uvOffset.x + 0.0f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.0f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.0625f);
							_uvs.add(uvOffset.x + 0.0f, uvOffset.y + 0.125f);
							_uvs.add(uvOffset.x + 0.0625f, uvOffset.y + 0.125f);

							_normals.add(0, 0, -1);
							_normals.add(0, 0, -1);
							_normals.add(0, 0, -1);
							_normals.add(0, 0, -1);
							_normals.add(0, 0, -1);
							_normals.add(0, 0, -1);
						}
					}
				}
			}
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