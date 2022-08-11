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

		public void Init(Action onEditMapAction, Action onSettingsAction, Action backToMenuAction)
		{
			_playButton.onClick.AddListener(() => backToMenuAction?.Invoke());
			_editMapButton.onClick.AddListener(() => { onEditMapAction?.Invoke(); });
			_settingsButton.onClick.AddListener(() => { onSettingsAction?.Invoke(); });
		}
	}
}