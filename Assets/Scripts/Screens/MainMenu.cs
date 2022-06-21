using UnityEngine;
using System;
using UnityEngine.UI;

namespace Screens
{
	public class MainMenu : MonoBehaviour
	{
		[SerializeField] private Button _settingButton, _exitButton;

		public void Init(Action onSettingAction, Action onQuitAction)
		{
			_settingButton.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });
		}
	}
}