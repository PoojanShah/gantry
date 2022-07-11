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
			_outputsNumber.value = OptionsSettings.OutputsNumber;
			_cuoCoreIp.text = OptionsSettings.CuoCoreIp;
			_cuoCorePort.text = OptionsSettings.CuoCorePort.ToString();
		}
	}
}