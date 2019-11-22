using System;
using System.IO;
using System.Text;
using OpenGL;

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

		private static bool compileShader(out uint shaderID, ShaderType type, string path)
		{
			shaderID = Gl.CreateShader(type);

			if (shaderID == 0)
			{
				ErrorCode code = Gl.GetError();
				Console.Error.WriteLine($"Error while creating shader for {path}: {code}");
				return false;
			}
			
			string[] source = new []{""};

			try
			{
				source[0] = File.ReadAllText(path);
			}
			catch (IOException e)
			{
				Console.Error.WriteLine(e);
			}

			Gl.ShaderSource(shaderID, source);
			Gl.CompileShader(shaderID);
			
			
			Gl.GetShader(shaderID, ShaderParameterName.CompileStatus, out int compileSuccess);
			
			if(compileSuccess == Gl.FALSE)
			{
				Gl.GetShader(shaderID, ShaderParameterName.InfoLogLength, out int length);
		
				StringBuilder error = new StringBuilder(length);
				Gl.GetShaderInfoLog(shaderID, length, out _, error);
		
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
			
			if (!compileShader(out vertexID, ShaderType.VertexShader, vertexPath))
				return false;
	
	
			if(!compileShader(out fragmentID, ShaderType.FragmentShader, fragmentPath))
				return false;
	
	
			programID = Gl.CreateProgram();
			if (programID == 0)
			{
				ErrorCode code = Gl.GetError();
				Console.Error.WriteLine($"Error while creating program for ({vertexPath}, {fragmentPath}): {code}");
				return false;
			}
			
			Gl.AttachShader(programID, vertexID);
			Gl.AttachShader(programID, fragmentID);
	
			Gl.LinkProgram(programID);
			
			
			Gl.GetProgram(programID, ProgramProperty.LinkStatus, out int linkSuccess);
			
			if(linkSuccess == Gl.FALSE)
			{
				Gl.GetProgram(programID, ProgramProperty.InfoLogLength, out int length);
		
				StringBuilder error = new StringBuilder(length);
				Gl.GetProgramInfoLog(programID, length, out _, error);
		
				Console.Error.WriteLine($"Error while linking shaders ({vertexPath}, {fragmentPath}) to program: {error}");
				
				return false;
			}
			
			return true;
		}

		private void freeMemory()
		{
			Gl.DeleteBuffers(_vertexID);
			Gl.DeleteBuffers(_fragmentID);
			Gl.DeleteProgram(_programID);

			_vertexID = 0;
			_fragmentID = 0;
			_programID = 0;
		}

		public bool bind()
		{
			if (_programID == 0) return false;
			Gl.UseProgram(_programID);
			return true;
		}

		public void unbind()
		{
			Gl.UseProgram(0);
		}
	}
}