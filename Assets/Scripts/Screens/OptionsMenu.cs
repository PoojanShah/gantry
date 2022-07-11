using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

		private OptionsSettings _optionsSettings;
		
		public void Init()
		{
			_optionsSettings = new OptionsSettings();
			_optionsSettings.Init();
			
			LoadValues(_optionsSettings);
			
			_cancelButton.onClick.AddListener(() =>
			{
				_optionsSettings.Save(_sound.isOn,_rotation.isOn, _outputsNumber.value, _cuoCoreIp.text, Convert.ToInt32(_cuoCorePort.text));
				Destroy(gameObject);
			});
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