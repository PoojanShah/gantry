using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Screens
{
	public class OptionsMenu : MonoBehaviour
	{
		[SerializeField] private Button _cancelButton;
		[SerializeField] private Toggle _sound;
		[SerializeField] private Toggle _rotation;
		[SerializeField] private TMP_Dropdown _outputsNumber;
		[SerializeField] private TMP_InputField _cuoCoreIp;
		[SerializeField] private TMP_InputField _cuoCorePort;
		
		public void Init()
		{
			SetDefaultValue();
			
			_cancelButton.onClick.AddListener(() =>
			{
				OptionsSettings.Save(_sound,_rotation, _outputsNumber.value, _cuoCoreIp.text, Convert.ToInt32(_cuoCorePort.text));
				Destroy(gameObject);
			});
		}

		private void SetDefaultValue()
		{
			_sound.isOn = OptionsSettings.Sound;
			_rotation.isOn = OptionsSettings.Rotation;
			_outputsNumber.value = OptionsSettings.NumberOfOutputs;
			_cuoCoreIp.text = OptionsSettings.CuoCoreIp;
			_cuoCorePort.text = OptionsSettings.CuoCorePort.ToString();
		}
	}

	public static class OptionsSettings
	{
		public static bool Sound, Rotation;
		public static int NumberOfOutputs;
		public static string CuoCoreIp = "192.168.1.10";
		public static int CuoCorePort = 7000;

		public static void Save(bool sound, bool rotation, int outputsNumber, string cuoCoreIp, int cuoCorePort)
		{
			Sound = sound;
			Rotation = rotation;
			NumberOfOutputs = outputsNumber;
			CuoCoreIp = cuoCoreIp;
			CuoCorePort = cuoCorePort;
		}
	}
}