using System;
using System.IO;
using System.Net;
using OpenGL;
using StbImageSharp;

namespace Meinkraft
{
	public class Texture : IDisposable
	{
		uint _textureID;
		
		public Texture(string name)
		{
			ImageResult image = null;
			
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
			
			_textureID = Gl.GenTexture();
			
			if (_textureID == 0)
			{
				ErrorCode code = Gl.GetError();
				Console.Error.WriteLine($"Error while creating texture for {name}: {code}");
				return;
			}

			Gl.BindTexture(TextureTarget.Texture2d, _textureID);
				
			Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
			Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
				
			Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);
		}
		
		public void Dispose()
		{
			Gl.DeleteTextures(_textureID);
		}

		public bool bind()
		{
			if (_textureID == 0) return false;
			Gl.BindTexture(TextureTarget.Texture2d, _textureID);
			return true;
		}

		public void unbind()
		{
			Gl.BindTexture(TextureTarget.Texture2d, 0);
		}
	}
}