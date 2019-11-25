using System;
using GlmSharp;
using LibNoise;
using LibNoise.Primitive;

namespace Meinkraft
{
	public static class WorldGeneration
	{
//		private static SimplexPerlin _noiseGen = new SimplexPerlin(new Random().Next(), NoiseQuality.Standard);
		private static SimplexPerlin _noiseGen = new SimplexPerlin(0, NoiseQuality.Standard);
		
		public static NativeArray<byte> generateChunkBlocks(ivec3 chunkPos, Func<ivec3, BiomeParams> biome)
		{
			BiomeParams biomeParams = biome(chunkPos);
			
			NativeArray<byte> blocks = new NativeArray<byte>(4096, BlockType.AIR);
		
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					int surfaceHeight = biomeParams.minY + (int) (biomeParams.heightMap[x, z] * (biomeParams.maxY - biomeParams.minY));
					int fullRockHeight = biomeParams.rockMin + (int) (biomeParams.fullRockHeightMap[x, z] * (biomeParams.rockMax - biomeParams.rockMin));

					for (int y = 0; y < 16; y++)
					{
						ivec3 blockWorldPos = MathUtils.getBlockWorldPos(chunkPos, new ivec3(x, y, z));
						
						if (surfaceHeight > fullRockHeight)
						{
							if (blockWorldPos.y < surfaceHeight)
								blocks[x + y*16 + z*256] = BlockType.STONE;
						}
						else
						{
							if (blockWorldPos.y < surfaceHeight - 4)
							{
								blocks[x + y*16 + z*256] = BlockType.STONE;
							}
							else if (blockWorldPos.y < surfaceHeight - 1)
							{
								blocks[x + y*16 + z*256] = BlockType.DIRT;
							}
							else if (blockWorldPos.y < surfaceHeight)
							{
								blocks[x + y*16 + z*256] = BlockType.GRASS;
							}
						}
					}
				}
			}

			return blocks;
		}
	
		public static BiomeParams mountains(ivec3 chunkPos)
		{
			BiomeParams biomeParams = new BiomeParams
			{
				heightMap = new float[16, 16],
				fullRockHeightMap = new float[16, 16],
			
				minY = 64,
				maxY = 255,
			
				rockMin = 160,
				rockMax = 180
			};

			ivec2 chunkPos2d = new ivec2(chunkPos.x, chunkPos.z);
			
			for (int x = 0; x<16; x++)
			{
				for (int y = 0; y<16; y++)
				{
					biomeParams.heightMap[x, y] = getNoise(chunkPos2d, new ivec2(x, y), 6, 12, persistance:0.4f);
					biomeParams.fullRockHeightMap[x, y] = getNoise(chunkPos2d, new ivec2(x, y), 1, 4);
				}
			}

			return biomeParams;
		}

		private static float getNoise(ivec2 chunkPos, ivec2 blockPos, int octaves, float scale, float lacunarity = 2f, float persistance = 0.5f)
		{
			float result = 0;
		
			float divider = 0;

			float frequency = 1;
			float amplitude = 1;
				
			for (int i = 0; i < octaves; i++)
			{
				result += getRawNoise(chunkPos, blockPos, scale, frequency, amplitude);
			
				divider += amplitude;
			
				frequency *= lacunarity;
				amplitude *= persistance;
			}

			result /= divider;

			return result * 0.5f + 0.5f;
		}
	
		private static float getRawNoise(ivec2 chunkPos, ivec2 blockPos, float scale, float frequency = 1f, float amplitude = 1f)
		{
			vec2 noisePos = (new vec2(chunkPos) + new vec2(blockPos) / 16f) * frequency / scale;

			return glm.Clamp(_noiseGen.GetValue(noisePos.x, noisePos.y), -1, 1) * amplitude;
		}
	}
}

public struct BiomeParams
{
	public float[,] heightMap;
	public float[,] fullRockHeightMap;
	
	public int minY;
	public int maxY;
	
	public int rockMin;
	public int rockMax;
}