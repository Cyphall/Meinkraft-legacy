using System;
using System.Collections.Generic;
using System.Linq;
using GLFW;
using GlmSharp;
using static SharpGL.OpenGL;

namespace Meinkraft
{
	public class RasterisationCamera : Camera, IDisposable
	{
		private readonly mat4 _projection;
		private mat4 viewProjection => _projection * mat4.LookAt(position, position + orientation, new vec3(0, 1, 0));

		private readonly Shader _chunkShader;
		private readonly Texture _chunkTexture;

		private const bool SHADER_SHOW_NORMALS = false;

		private readonly World _world;
		
		public RasterisationCamera(Window window, World world) : base(window)
		{
			ivec2 size = new ivec2();
			Glfw.GetWindowSize(window, out size.x, out size.y);
			_projection = mat4.Perspective(glm.Radians(82.0f), (float)size.x / size.y, 0.01f, 1000.0f);

			_chunkShader = new Shader("chunk");
			_chunkTexture = new Texture("Block_Texture");

			_world = world;
		}

		public void Dispose()
		{
			_chunkShader.Dispose();
			_chunkTexture.Dispose();
		}

		public override void render()
		{
			mat4 vp = viewProjection;
			if (_chunkShader.bind())
			{
				if (_chunkTexture.bind())
				{
					ToolBox.gl.Uniform1(4, 1, new[]{SHADER_SHOW_NORMALS ? 1 : 0});
					
					foreach (KeyValuePair<ivec3, Chunk> keyValuePair in _world.chunks)
					{
						keyValuePair.Value.render(vp);
					}
				}
				Texture.unbind();
			}
			_chunkShader.unbind();
			
			uint err;
			while((err = ToolBox.gl.GetError()) != GL_NO_ERROR)
			{
				Console.Error.WriteLine(err);
			}
		}
	}
}