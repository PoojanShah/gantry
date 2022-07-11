using System;
using UnityEngine;

namespace Screens
{
	public class OptionsSettings
	{
		private const string CUO_CORE_IP_KEY = "CueCoreIP";
		private const string CUE_CORE_PORT_KEY = "CueCorePort";
		private const string OUTPUTS_NUMBER_KEY = "OutputsNumber";
		private const string ROTATION_KEY = "Rotation";
		private const string SOUND_KEY = "Sound";
		
		public bool IsSoundOn, IsRotationOn;
		public int OutputsNumber;
		public string CuoCoreIp = "192.168.1.10";
		public int CuoCorePort = 7000;

		public OptionsSettings()
		{
			CuoCoreIp = PlayerPrefs.GetString(CUO_CORE_IP_KEY, CuoCoreIp);
			CuoCorePort = PlayerPrefs.GetInt(CUE_CORE_PORT_KEY, CuoCorePort);
			OutputsNumber = PlayerPrefs.GetInt(OUTPUTS_NUMBER_KEY, OutputsNumber);
			IsRotationOn = Convert.ToBoolean(PlayerPrefs.GetInt(ROTATION_KEY, Convert.ToInt32(IsRotationOn)));
			var sound = PlayerPrefs.GetInt(SOUND_KEY, Convert.ToInt32(IsSoundOn));
			IsSoundOn = Convert.ToBoolean(sound);
		}
		
		public void Save(bool sound, bool rotation, int outputsNumber, string cuoCoreIp, int cuoCorePort)
		{
			IsSoundOn = sound;
			IsRotationOn = rotation;
			OutputsNumber = outputsNumber;
			CuoCoreIp = cuoCoreIp;
			CuoCorePort = cuoCorePort;
			
			SaveToPlayerPrefs();
		}

		private void SaveToPlayerPrefs()
		{
			PlayerPrefs.SetString(CUO_CORE_IP_KEY, CuoCoreIp);
			PlayerPrefs.SetInt(CUE_CORE_PORT_KEY, CuoCorePort);
			PlayerPrefs.SetInt(OUTPUTS_NUMBER_KEY, OutputsNumber);
			PlayerPrefs.SetInt(ROTATION_KEY, Convert.ToInt32(IsRotationOn));
			PlayerPrefs.SetInt(SOUND_KEY, Convert.ToInt32(IsSoundOn));
		}
	}
}