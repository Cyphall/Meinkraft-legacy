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

			uint[] buffers = new uint[3];
			ToolBox.gl.GenBuffers(3, buffers);
			_verticesBufferID = buffers[0];
			_uvsBufferID = buffers[1];
			_normalsBufferID = buffers[2];

			uint[] array = new uint[1];
			ToolBox.gl.GenVertexArrays(1, array);
			_vaoID = array[0];

			ToolBox.gl.BindVertexArray(_vaoID);

				ToolBox.gl.BindBuffer(GL_ARRAY_BUFFER, _verticesBufferID);
					ToolBox.gl.VertexAttribIPointer(0, 3, GL_UNSIGNED_BYTE, 0, IntPtr.Zero);
					ToolBox.gl.EnableVertexAttribArray(0);

				ToolBox.gl.BindBuffer(GL_ARRAY_BUFFER, _uvsBufferID);
					ToolBox.gl.VertexAttribPointer(1, 2, 5131, false, 0, IntPtr.Zero);
					ToolBox.gl.EnableVertexAttribArray(1);

				ToolBox.gl.BindBuffer(GL_ARRAY_BUFFER, _normalsBufferID);
					ToolBox.gl.VertexAttribIPointer(2, 3, GL_BYTE, 0, IntPtr.Zero);
					ToolBox.gl.EnableVertexAttribArray(2);

			ToolBox.gl.BindVertexArray(0);
		}

		public void Dispose()
		{
			ToolBox.gl.DeleteVertexArrays(1, new[]{_vaoID});
			ToolBox.gl.DeleteBuffers(3, new[]{_verticesBufferID, _uvsBufferID, _normalsBufferID});
			
			_blocks?.Dispose();
		}

		public void render(mat4 viewProjection, uint shaderID)
		{
			if (!initialized) return;

			mat4 mvp = viewProjection * _model;

			ToolBox.gl.UniformMatrix4(3, 1, false, mvp.ToArray());

			ToolBox.gl.BindVertexArray(_vaoID);
				ToolBox.gl.DrawArrays(GL_TRIANGLES, 0, _verticesCount);
			ToolBox.gl.BindVertexArray(0);
		}

		public void initialize(OpenGL gl)
		{
			_blocks = WorldGeneration.generateChunkBlocks(_pos, WorldGeneration.mountains);

			rebuildMesh(gl);
			
			if (destroyed)
				Dispose();
			else
				initialized = true;
		}

		public byte getBlock(int x, int y, int z)
		{
			return _blocks[x + y * 16 + z * 256];
		}

		private void rebuildMesh(OpenGL gl)
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
			
			gl.BindBuffer(GL_ARRAY_BUFFER, _verticesBufferID);
				gl.BufferData(GL_ARRAY_BUFFER, (int)(vertices.size), vertices, GL_DYNAMIC_DRAW);

			gl.BindBuffer(GL_ARRAY_BUFFER, _uvsBufferID);
				gl.BufferData(GL_ARRAY_BUFFER, (int)(uvs.size * 2), uvs, GL_DYNAMIC_DRAW);

			gl.BindBuffer(GL_ARRAY_BUFFER, _normalsBufferID);
				gl.BufferData(GL_ARRAY_BUFFER, (int)(normals.size), normals, GL_DYNAMIC_DRAW);

			gl.BindBuffer(GL_ARRAY_BUFFER, 0);
			
			gl.Finish();
			
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

			rebuildMesh(ToolBox.gl);
		}
	}
}