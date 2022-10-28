using UnityEngine;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class ToggleGroupController : MonoBehaviour
	{
		[SerializeField] private Toggle _bothToggle;
		[SerializeField] private Toggle _primaryToggle;
		[SerializeField] private Toggle _secondaryToggle;
		
		private OptionsSettings _settings;
		
		public void Init(OptionsSettings settings)
		{
			_settings = settings;
			
			_bothToggle.onValueChanged.AddListener((isOn) => ChangeState(_bothToggle.isOn, OutputType.Both));
			_primaryToggle.onValueChanged.AddListener((isOn) => ChangeState(_primaryToggle.isOn, OutputType.Primary));
			_secondaryToggle.onValueChanged.AddListener((isOn) => ChangeState(_secondaryToggle.isOn, OutputType.Secondary));
			
			_settings.SwitchOutputType(OutputType.Both);
		}

		private void ChangeState(bool isOn, OutputType type)
		{
			if (isOn)
				_settings.SwitchOutputType(type);
		}
	}
}