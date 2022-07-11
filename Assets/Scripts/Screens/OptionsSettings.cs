namespace Screens
{
	public static class OptionsSettings
	{
		public static bool Sound, Rotation;
		public static int OutputsNumber;
		public static string CuoCoreIp = "192.168.1.10";
		public static int CuoCorePort = 7000;

		public static void Save(bool sound, bool rotation, int outputsNumber, string cuoCoreIp, int cuoCorePort)
		{
			Sound = sound;
			Rotation = rotation;
			OutputsNumber = outputsNumber;
			CuoCoreIp = cuoCoreIp;
			CuoCorePort = cuoCorePort;
		}
	}
}