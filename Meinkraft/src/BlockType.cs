using System.Collections.Generic;
using GlmSharp;

namespace Meinkraft
{
	public class BlockType
	{
		private static Dictionary<byte, BlockType> types;

		public const byte AIR = 0;
		public const byte STONE = 1;
		public const byte GRASS = 2;
		public const byte DIRT = 3;
		public const byte WOOD = 4;
		public const byte IRON = 5;

		static BlockType()
		{
			types = new Dictionary<byte, BlockType>
			{
				{AIR, null},
				{STONE, new BlockType(new vec2(0.75f, 0f))},
				{GRASS, new BlockType(new vec2(0f, 0f))},
				{DIRT, new BlockType(new vec2(0f, 0.25f))},
				{WOOD, new BlockType(new vec2(0.5f, 0f))},
				{IRON, new BlockType(new vec2(0.25f, 0f))}
			};
		}

		public static BlockType get(byte i)
		{
			return types[i];
		}

		public vec2 uvOffset { get; private set; }

		private BlockType(vec2 uvOffset)
		{
			this.uvOffset = uvOffset;
		}
	}
}