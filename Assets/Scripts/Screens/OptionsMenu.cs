using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class OptionsMenu : MonoBehaviour
	{
		[SerializeField] private Toggle _sound,_rotation;
		[SerializeField] private TMP_Dropdown _outputsNumber;
		[SerializeField] private TMP_InputField _cuoCoreIp, _cuoCorePort;
		[SerializeField] private Button _exitButton, _switchButton;

		private OptionsSettings _optionsSettings;
		
		public void Init(Action exitButtonAction, Action switchButtonAction)
		{
			_exitButton.onClick.AddListener(exitButtonAction.Invoke);
			_switchButton.onClick.AddListener(switchButtonAction.Invoke);
			
			_optionsSettings = new OptionsSettings();
			
			LoadValues(_optionsSettings);
		}

		public void SaveAndExit()
		{
			_optionsSettings.Save(_sound.isOn,
				_rotation.isOn,
				_outputsNumber.value,
				_cuoCoreIp.text,
				Convert.ToInt32(_cuoCorePort.text));
			
			_exitButton.onClick.RemoveAllListeners();
			_switchButton.onClick.RemoveAllListeners();
		}

		private void LoadValues(OptionsSettings optionsSettings)
		{
			_sound.isOn = optionsSettings.IsSoundOn;
			_rotation.isOn = optionsSettings.IsRotationOn;
			_outputsNumber.value = optionsSettings.OutputsNumber;
			_cuoCoreIp.text = optionsSettings.CuoCoreIp;
			_cuoCorePort.text = optionsSettings.CuoCorePort.ToString();
		}
	}
}