using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class AdminMenu : MonoBehaviour
	{
		[SerializeField] private Button _playButton;
		[SerializeField] private Button _editMapButton;
		[SerializeField] private Button _optionButton;
		[SerializeField] private Button _libraryButton;
		[SerializeField] private Button _exitButton;

		public void Init(Action onPlayAction, Action onEditMapAction, Action onOptionAction, Action onLibraryAction,
			Action onExitAction)
		{
			_playButton.onClick.AddListener(() => { onPlayAction?.Invoke(); });
			_editMapButton.onClick.AddListener(() => { onEditMapAction?.Invoke(); });
			_optionButton.onClick.AddListener(() => { onOptionAction?.Invoke(); });
			_libraryButton.onClick.AddListener(() => { onLibraryAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onExitAction?.Invoke(); });
		}
	}
}