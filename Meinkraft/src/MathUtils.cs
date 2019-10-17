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
	}
}