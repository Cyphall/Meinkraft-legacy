using System;
using System.Collections.Generic;
using GlmSharp;
using OpenGL;

namespace Meinkraft
{
	public class Chunk : IDisposable
	{
		private byte[] _blocks;
		public ivec2 pos { get; private set; }
		private mat4 _model = mat4.Identity;
	
		private int _verticesCount;
	
		private uint _verticesBufferID;
		private uint _uvBufferID;
	
		private uint _vaoID;
		
		public Chunk(ivec2 chunkPos)
		{
			pos = chunkPos;

			_blocks = WorldGeneration.generateChunkBlocks(chunkPos, WorldGeneration.mountains);

			_model[3, 0] = pos.x * 16;
			_model[3, 2] = pos.y * 16;
			
			_verticesBufferID = Gl.GenBuffer();
			_uvBufferID = Gl.GenBuffer();

			_vaoID = Gl.GenVertexArray();
	
			rebuildMesh();
		}
		
		public void Dispose()
		{
			Gl.DeleteVertexArrays(_vaoID);
			Gl.DeleteBuffers(_verticesBufferID);
			Gl.DeleteBuffers(_uvBufferID);
		}

		public void render(mat4 viewProjection, uint shaderID)
		{
			mat4 mvp = viewProjection * _model;

			Gl.UniformMatrix4f(Gl.GetUniformLocation(shaderID, "modelViewProjection"), 1, false, mvp);
			
			Gl.BindVertexArray(_vaoID);
				Gl.DrawArrays(PrimitiveType.Quads, 0, _verticesCount);
			Gl.BindVertexArray(0);
		}

		private void rebuildMesh()
		{
			List<gvec3<byte>> vertices = new List<gvec3<byte>>();
			List<vec2> uvs = new List<vec2>();
	
			int vCount = 0;
	
			for (byte z = 0; z < 16; z++)
			{
				for (byte y = 0; y < 255; y++)
				{
					for (byte x = 0; x < 16; x++)
					{
						if (_blocks[y + z * 255 + x * 4080] == BlockType.AIR) continue;

						vec2 uvOffset = BlockType.get(_blocks[y + z * 255 + x * 4080]).uvOffset;

						// x + 1
						if ((x + 1 < 16 && _blocks[y + z * 255 + (x+1) * 4080] == BlockType.AIR) || x + 1 == 16)
						{
							vCount += 4;
							vertices.Add(new gvec3<byte>((byte)(x + 1), y, (byte)(z + 1)));
							vertices.Add(new gvec3<byte>((byte)(x + 1), (byte)(y + 1), (byte)(z + 1)));
							vertices.Add(new gvec3<byte>((byte)(x + 1), (byte)(y + 1), z));
							vertices.Add(new gvec3<byte>((byte)(x + 1), y, z));

							uvs.Add(uvOffset + new vec2(0.1875f, 0.0625f));
							uvs.Add(uvOffset + new vec2(0.1875f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.25f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.25f, 0.0625f));
						}
						// x - 1
						if ((x - 1 > -1 && _blocks[y + z * 255 + (x-1) * 4080] == BlockType.AIR) || x - 1 == -1)
						{
							vCount += 4;
							vertices.Add(new gvec3<byte>(x, y, z));
							vertices.Add(new gvec3<byte>(x, (byte)(y + 1), z));
							vertices.Add(new gvec3<byte>(x, (byte)(y + 1), (byte)(z + 1)));
							vertices.Add(new gvec3<byte>(x, y, (byte)(z + 1)));

							uvs.Add(uvOffset + new vec2(0.0625f, 0.0625f));
							uvs.Add(uvOffset + new vec2(0.0625f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.125f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.125f, 0.0625f));
						}
						// y + 1
						if ((y + 1 < 255 && _blocks[(y+1) + z * 255 + x * 4080] == BlockType.AIR) || y + 1 == 255)
						{
							vCount += 4;
							vertices.Add(new gvec3<byte>(x, (byte)(y + 1), z));
							vertices.Add(new gvec3<byte>((byte)(x + 1), (byte)(y + 1), z));
							vertices.Add(new gvec3<byte>((byte)(x + 1), (byte)(y + 1), (byte)(z + 1)));
							vertices.Add(new gvec3<byte>(x, (byte)(y + 1), (byte)(z + 1)));

							uvs.Add(uvOffset + new vec2(0.0625f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.0625f, 0.1875f));
							uvs.Add(uvOffset + new vec2(0.125f, 0.1875f));
							uvs.Add(uvOffset + new vec2(0.125f, 0.125f));
						}
						// y - 1
						if ((y - 1 > -1 && _blocks[(y-1) + z * 255 + x * 4080] == BlockType.AIR) || y - 1 == -1)
						{
							vCount += 4;
							vertices.Add(new gvec3<byte>((byte)(x + 1), y, z));
							vertices.Add(new gvec3<byte>(x, y, z));
							vertices.Add(new gvec3<byte>(x, y, (byte)(z + 1)));
							vertices.Add(new gvec3<byte>((byte)(x + 1), y, (byte)(z + 1)));

							uvs.Add(uvOffset + new vec2(0.0625f, 0f));
							uvs.Add(uvOffset + new vec2(0.0625f, 0.0625f));
							uvs.Add(uvOffset + new vec2(0.125f, 0.0625f));
							uvs.Add(uvOffset + new vec2(0.125f, 0f));
						}
						// z + 1
						if ((z + 1 < 16 && _blocks[y + (z+1) * 255 + x * 4080] == BlockType.AIR) || z + 1 == 16)
						{
							vCount += 4;
							vertices.Add(new gvec3<byte>(x, y, (byte)(z + 1)));
							vertices.Add(new gvec3<byte>(x, (byte)(y + 1), (byte)(z + 1)));
							vertices.Add(new gvec3<byte>((byte)(x + 1), (byte)(y + 1), (byte)(z + 1)));
							vertices.Add(new gvec3<byte>((byte)(x + 1), y, (byte)(z + 1)));

							uvs.Add(uvOffset + new vec2(0.0f, 0.0625f));
							uvs.Add(uvOffset + new vec2(0.0f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.0625f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.0625f, 0.0625f));
						}
						// z - 1
						if ((z - 1 > -1 && _blocks[y + (z-1) * 255 + x * 4080] == BlockType.AIR) || z - 1 == -1)
						{
							vCount += 4;
							vertices.Add(new gvec3<byte>((byte)(x + 1), y, z));
							vertices.Add(new gvec3<byte>((byte)(x + 1), (byte)(y + 1), z));
							vertices.Add(new gvec3<byte>(x, (byte)(y + 1), z));
							vertices.Add(new gvec3<byte>(x, y, z));

							uvs.Add(uvOffset + new vec2(0.125f, 0.0625f));
							uvs.Add(uvOffset + new vec2(0.125f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.1875f, 0.125f));
							uvs.Add(uvOffset + new vec2(0.1875f, 0.0625f));
						}
					}
				}
			}
	
			_verticesCount = vCount;
	
			Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, (uint)vertices.Count * 3, vertices.ToArray(), BufferUsage.StaticDraw);
	
			Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvBufferID);
				Gl.BufferData(BufferTarget.ArrayBuffer, (uint)uvs.Count * sizeof(float) * 2, uvs.ToArray(), BufferUsage.StaticDraw);
	
			Gl.BindBuffer(BufferTarget.ArrayBuffer, 0);


			Gl.BindVertexArray(_vaoID);
	
				Gl.BindBuffer(BufferTarget.ArrayBuffer, _verticesBufferID);
				Gl.VertexAttribIPointer(0, 3, VertexAttribType.UnsignedByte, 0, IntPtr.Zero);
				Gl.EnableVertexAttribArray(0);
		
				Gl.BindBuffer(BufferTarget.ArrayBuffer, _uvBufferID);
				Gl.VertexAttribPointer(1, 2, VertexAttribType.Float, false, 0, IntPtr.Zero);
				Gl.EnableVertexAttribArray(1);
	
			Gl.BindVertexArray(0);
		}
		
		private void setBlock(ivec3 blockPos, byte blockType)
		{
			if (pos.y < 0 || pos.y > 255)
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