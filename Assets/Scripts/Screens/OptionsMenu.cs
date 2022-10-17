using System;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class OptionsMenu : MonoBehaviour
	{
		[SerializeField] private TMP_Text _serverIpTitle;
		[SerializeField] private Toggle _sound,_rotation, _queCore, _debug;
		[SerializeField] private TMP_Dropdown _outputsNumber;
		[SerializeField] private TMP_InputField _cuoCoreIp, _cuoCorePort, _deviceId;

		private OptionsSettings _optionsSettings;

		public void Init(OptionsSettings settings, Transform debugPanel)
		{
			_optionsSettings = settings;

			LoadValues(_optionsSettings);
			
			InitDebugToggle(debugPanel);

			//InitIpTitle();

			InitDeviceId();
		}

		private void InitDeviceId() => _deviceId.text = SystemInfo.deviceUniqueIdentifier;
		private void InitIpTitle() => _serverIpTitle.text = NetworkHelper.GetMyIp().ToString();

		public void SaveAndExit()
		{
			_optionsSettings.Save(_sound.isOn, _rotation.isOn, _outputsNumber.value == 1, 
				_cuoCoreIp.text, Convert.ToInt32(_cuoCorePort.text), _queCore.isOn, _debug.isOn);
		}

		private void LoadValues(OptionsSettings optionsSettings)
		{
			_sound.isOn = optionsSettings.IsSoundOn;
			_rotation.isOn = optionsSettings.IsRotationOn;
			_queCore.isOn = optionsSettings.IsCueCoreEnabled;
			_debug.isOn = optionsSettings.IsDebugPanelOn;
			_outputsNumber.value = optionsSettings.IsDuoOutput ? 1 : 0;
			_cuoCoreIp.text = optionsSettings.CuoCoreIp;
			_cuoCorePort.text = optionsSettings.CuoCorePort.ToString();
		}

		private void InitDebugToggle(Transform debugPanel)
		{ 
			_debug.onValueChanged.AddListener((state) =>
			{
				debugPanel.gameObject.SetActive(state);
			});
		}
	}
}