using UnityEngine;
using System;
using Configs;
using Core;
using UnityEngine.UI;
using VideoPlaying;

namespace Screens
{
	public class MainMenu : MonoBehaviour
	{
		[SerializeField] private Button _settingButton, _exitButton;
		[SerializeField] private Transform _parent;

		public void Init(Action<int> playVideoAction, Action onSettingAction, Action onQuitAction, VideosConfig videos, ICommonFactory factory)
		{
			_settingButton.onClick.AddListener(() => { onSettingAction?.Invoke(); });
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });

			InitVideoItems(videos, factory, playVideoAction);
		}

		private void InitVideoItems(VideosConfig config, ICommonFactory commonFactory, Action<int> playVideoAction)
		{
			for (var i = 0; i < config.Videos.Length; i++)
			{
				var videoItem = commonFactory.InstantiateObject<VideoItem>(config.VideoItemPrefab, _parent);
				videoItem.Init(i, playVideoAction, config.Videos[i].name);
			}
		}
	}
}