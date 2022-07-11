using System;
using UnityEngine;

namespace Screens
{
	public static class OptionsSettings
	{
		public static bool Sound, Rotation;
		public static int OutputsNumber;
		public static string CuoCoreIp = "192.168.1.10";
		public static int CuoCorePort = 7000;

		public static void Init()
		{
			CuoCoreIp = PlayerPrefs.GetString("CueCoreIP", CuoCoreIp);
			CuoCorePort = PlayerPrefs.GetInt("CueCorePort", CuoCorePort);
			OutputsNumber = PlayerPrefs.GetInt("OutputsNumber", OutputsNumber);
			Rotation = Convert.ToBoolean(PlayerPrefs.GetInt("Rotation", Convert.ToInt32(Rotation)));
			Sound = Convert.ToBoolean(PlayerPrefs.GetInt("Sound", Convert.ToInt32(Sound)));
		}
		
		public static void Save(bool sound, bool rotation, int outputsNumber, string cuoCoreIp, int cuoCorePort)
		{
			Sound = sound;
			Rotation = rotation;
			OutputsNumber = outputsNumber;
			CuoCoreIp = cuoCoreIp;
			CuoCorePort = cuoCorePort;
			
			SaveToPlayerPrefs();
		}

		private static void SaveToPlayerPrefs()
		{
			PlayerPrefs.SetString("CueCoreIP", CuoCoreIp);
			PlayerPrefs.SetInt("CueCorePort", CuoCorePort);
			PlayerPrefs.SetInt("OutputsNumber", OutputsNumber);
			PlayerPrefs.SetInt("Rotation", Convert.ToInt32(Rotation));
			PlayerPrefs.SetInt("Sound", Convert.ToInt32(Sound));
		}
	}
}