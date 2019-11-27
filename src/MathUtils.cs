using GlmSharp;

namespace Meinkraft
{
	public static class MathUtils
	{
		public static int mod(int value, int modulo)
		{
			int r = value % modulo;
			return r < 0 ? r + modulo : r;
		}
		
		public static float mod(float value, float modulo)
		{
			float r = value % modulo;
			return r < 0 ? r + modulo : r;
		}
		
		public static double mod(double value, double modulo)
		{
			double r = value % modulo;
			return r < 0 ? r + modulo : r;
		}
		
		public static ivec3 chunkPosFromBlockPos(ivec3 blockPos)
		{
			ivec3 chunkPos = ivec3.Zero;
			
			chunkPos.x = (int)glm.Floor(blockPos.x / 16.0f);
			chunkPos.y = (int)glm.Floor(blockPos.y / 16.0f);
			chunkPos.z = (int)glm.Floor(blockPos.z / 16.0f);

			return chunkPos;
		}

		public static ivec3 localBlockPosFromBlockPos(ivec3 blockPos)
		{
			ivec3 localBlockPos = ivec3.Zero;

			localBlockPos.x = MathUtils.mod(blockPos.x, 16);
			localBlockPos.y = MathUtils.mod(blockPos.y, 16);
			localBlockPos.z = MathUtils.mod(blockPos.z, 16);

			return localBlockPos;
		}
	
		public static ivec3 chunkPosFromPlayerPos(dvec3 playerPos)
		{
			ivec3 chunkPos = ivec3.Zero;

			chunkPos.x = (int)glm.Floor(playerPos.x / 16.0f);
			chunkPos.y = (int)glm.Floor(playerPos.y / 16.0f);
			chunkPos.z = (int)glm.Floor(playerPos.z / 16.0f);

			return chunkPos;
		}

		public static ivec3 getBlockWorldPos(ivec3 chunkPos, ivec3 localPos)
		{
			return chunkPos * 16 + localPos;
		}
	}
}