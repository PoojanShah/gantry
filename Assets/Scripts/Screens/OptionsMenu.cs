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
		[SerializeField] private Toggle _sound,_rotation, _queCore;
		[SerializeField] private TMP_Dropdown _outputsNumber;
		[SerializeField] private TMP_InputField _cuoCoreIp, _cuoCorePort;

		private OptionsSettings _optionsSettings;

		public void Init(OptionsSettings settings)
		{
			_optionsSettings = settings;
			
			LoadValues(_optionsSettings);

			InitIpTitle();
		}
		private void InitIpTitle() => _serverIpTitle.text = NetworkHelper.GetMyIp().ToString();

		public void SaveAndExit()
		{
			_optionsSettings.Save(_sound.isOn, _rotation.isOn, _outputsNumber.value, 
				_cuoCoreIp.text, Convert.ToInt32(_cuoCorePort.text), _queCore.isOn);
		}

		private void LoadValues(OptionsSettings optionsSettings)
		{
			_sound.isOn = optionsSettings.IsSoundOn;
			_rotation.isOn = optionsSettings.IsRotationOn;
			_queCore.isOn = optionsSettings.IsCueCoreEnabled;
			_outputsNumber.value = optionsSettings.OutputsNumber;
			_cuoCoreIp.text = optionsSettings.CuoCoreIp;
			_cuoCorePort.text = optionsSettings.CuoCorePort.ToString();
		}
	}
}