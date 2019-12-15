using System;
using System.IO;
using StbImageSharp;
using static SharpGL.OpenGL;

namespace Meinkraft
{
	public class Texture : IDisposable
	{
		uint _textureID;

		public Texture(string name)
		{
			ImageResult image;

			try
			{
				using (Stream stream = File.OpenRead($"resources/textures/{name}.png"))
				{
					image = new ImageStreamLoader().Load(stream, ColorComponents.RedGreenBlueAlpha);
				}
			}
			catch (IOException e)
			{
				Console.Error.WriteLine(e);
				return;
			}

			uint[] temp = new uint[1];
			ToolBox.gl.GenTextures(1, temp);
			_textureID = temp[0];

			if (_textureID == 0)
			{
				uint code = ToolBox.gl.GetError();
				Console.Error.WriteLine($"Error while creating texture for {name}: {code}");
				return;
			}

			ToolBox.gl.BindTexture(GL_TEXTURE_2D, _textureID);

			ToolBox.gl.TexParameter(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
			ToolBox.gl.TexParameter(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

			ToolBox.gl.TexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, image.Width, image.Height, 0, GL_RGBA, GL_UNSIGNED_BYTE, image.Data);
		}

		public void Dispose()
		{
			ToolBox.gl.DeleteTextures(1, new[] {_textureID});
		}

		public bool bind()
		{
			if (_textureID == 0) return false;
			ToolBox.gl.BindTexture(GL_TEXTURE_2D, _textureID);
			return true;
		}

		public static void unbind()
		{
			ToolBox.gl.BindTexture(GL_TEXTURE_2D, 0);
		}
	}
}