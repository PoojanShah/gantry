using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class AdminMenu : MonoBehaviour
	{
		[SerializeField] private Button _playButton;
		[SerializeField] private Button _editMapButton;
		[SerializeField] private Button _settingsButton;
		[SerializeField] private Button _exitButton;

		public void Init(Action onEditMapAction, Action onSettingsAction,
			Action onExitAction)
		{
			_playButton.onClick.AddListener(() => onExitAction?.Invoke());
			_editMapButton.onClick.AddListener(() => { onEditMapAction?.Invoke(); });
			_settingsButton.onClick.AddListener(() => { onSettingsAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onExitAction?.Invoke(); });
		}
	}
}