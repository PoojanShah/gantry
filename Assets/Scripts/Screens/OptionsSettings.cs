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
		private const string CUE_CORE_KEY = "CueCore";
		
		public bool IsSoundOn, IsRotationOn, IsCueCoreEnabled;
		public int OutputsNumber;
		public string CuoCoreIp = "192.168.1.10";
		public int CuoCorePort = 7000;

		public OptionsSettings() => Load();

		public void Save(bool sound, bool rotation, int outputsType, string cuoCoreIp, int cuoCorePort, bool isCueCore)
		{
			IsSoundOn = sound;
			IsRotationOn = rotation;
			OutputsNumber = outputsType;
			CuoCoreIp = cuoCoreIp;
			CuoCorePort = cuoCorePort;
			IsCueCoreEnabled = isCueCore;

			SaveToPlayerPrefs();
		}

		public void Load()
		{
			CuoCoreIp = PlayerPrefs.GetString(CUO_CORE_IP_KEY, CuoCoreIp);
			CuoCorePort = PlayerPrefs.GetInt(CUE_CORE_PORT_KEY, CuoCorePort);
			OutputsNumber = PlayerPrefs.GetInt(OUTPUTS_NUMBER_KEY, OutputsNumber);
			IsRotationOn = Convert.ToBoolean(PlayerPrefs.GetInt(ROTATION_KEY, Convert.ToInt32(IsRotationOn)));
			IsSoundOn = Convert.ToBoolean(PlayerPrefs.GetInt(SOUND_KEY, Convert.ToInt32(IsSoundOn)));
			IsCueCoreEnabled = Convert.ToBoolean(PlayerPrefs.GetInt(CUE_CORE_KEY, Convert.ToInt32(IsCueCoreEnabled)));
		}

		private void SaveToPlayerPrefs()
		{
			PlayerPrefs.SetString(CUO_CORE_IP_KEY, CuoCoreIp);
			PlayerPrefs.SetInt(CUE_CORE_PORT_KEY, CuoCorePort);
			PlayerPrefs.SetInt(OUTPUTS_NUMBER_KEY, OutputsNumber);
			PlayerPrefs.SetInt(ROTATION_KEY, Convert.ToInt32(IsRotationOn));
			PlayerPrefs.SetInt(SOUND_KEY, Convert.ToInt32(IsSoundOn));
			PlayerPrefs.SetInt(CUE_CORE_KEY, Convert.ToInt32(IsCueCoreEnabled));
		}

		public void SwitchSound()
		{
			IsSoundOn = !IsSoundOn;

			PlayerPrefs.SetInt(SOUND_KEY, Convert.ToInt32(IsSoundOn));
		}
	}
}