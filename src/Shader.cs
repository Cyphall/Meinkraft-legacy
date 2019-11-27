using System;
using System.IO;
using System.Text;
using static SharpGL.OpenGL;

namespace Meinkraft
{
	public class Shader : IDisposable
	{
		private uint _vertexID;
		private uint _fragmentID;
		private uint _programID;

		public uint programID
		{
			get => _programID;
			private set => _programID = value;
		}
		
		public Shader(string name)
		{
			if (!createShaderProgram(out _programID, out _vertexID, out _fragmentID, $"resources/shaders/{name}.vert", $"resources/shaders/{name}.frag"))
			{
				freeMemory();
			}
		}
		
		public void Dispose()
		{
			freeMemory();
		}

		private static bool compileShader(out uint shaderID, uint type, string path)
		{
			shaderID = ToolBox.gl.CreateShader(type);

			if (shaderID == 0)
			{
				uint code = ToolBox.gl.GetError();
				Console.Error.WriteLine($"Error while creating shader for {path}: {code}");
				return false;
			}
			
			string source = "";

			try
			{
				source = File.ReadAllText(path);
			}
			catch (IOException e)
			{
				Console.Error.WriteLine(e);
			}

			ToolBox.gl.ShaderSource(shaderID, source);
			ToolBox.gl.CompileShader(shaderID);
			
			int[] compileSuccess = new int[1];
			ToolBox.gl.GetShader(shaderID, GL_COMPILE_STATUS, compileSuccess);
			
			if(compileSuccess[0] == GL_FALSE)
			{
				int[] length = new int[1];
				ToolBox.gl.GetShader(shaderID, GL_INFO_LOG_LENGTH, length);
		
				StringBuilder error = new StringBuilder(length[0]);
				ToolBox.gl.GetShaderInfoLog(shaderID, length[0], IntPtr.Zero, error);
		
				Console.Error.WriteLine($"Error while compiling shader {path}: {error}");
				
				return false;
			}

			return true;
		}

		private static bool createShaderProgram(out uint programID, out uint vertexID, out uint fragmentID, string vertexPath, string fragmentPath)
		{
			programID = 0;
			vertexID = 0;
			fragmentID = 0;
			
			if (!compileShader(out vertexID, GL_VERTEX_SHADER, vertexPath))
				return false;
	
	
			if(!compileShader(out fragmentID, GL_FRAGMENT_SHADER, fragmentPath))
				return false;
	
	
			programID = ToolBox.gl.CreateProgram();
			if (programID == 0)
			{
				uint code = ToolBox.gl.GetError();
				Console.Error.WriteLine($"Error while creating program for ({vertexPath}, {fragmentPath}): {code}");
				return false;
			}
			
			ToolBox.gl.AttachShader(programID, vertexID);
			ToolBox.gl.AttachShader(programID, fragmentID);
	
			ToolBox.gl.LinkProgram(programID);
			
			
			int[] linkSuccess = new int[1];
			ToolBox.gl.GetProgram(programID, GL_LINK_STATUS, linkSuccess);
			
			if(linkSuccess[0] == GL_FALSE)
			{
				int[] length = new int[1];
				ToolBox.gl.GetProgram(programID, GL_INFO_LOG_LENGTH, length);
		
				StringBuilder error = new StringBuilder(length[0]);
				ToolBox.gl.GetProgramInfoLog(programID, length[0], IntPtr.Zero, error);
		
				Console.Error.WriteLine($"Error while linking shaders ({vertexPath}, {fragmentPath}) to program: {error}");
				
				return false;
			}
			
			return true;
		}

		private void freeMemory()
		{
			ToolBox.gl.DeleteBuffers(2, new []{_vertexID, _fragmentID});
			ToolBox.gl.DeleteProgram(_programID);

			_vertexID = 0;
			_fragmentID = 0;
			_programID = 0;
		}

		public bool bind()
		{
			if (_programID == 0) return false;
			ToolBox.gl.UseProgram(_programID);
			return true;
		}

		public void unbind()
		{
			ToolBox.gl.UseProgram(0);
		}
	}
}