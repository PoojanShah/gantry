using System;
using UnityEngine;
using UnityEngine.Video;

namespace VideoPlaying
{
	[Serializable]
	public class VideoPlayerScreen
	{
		public Transform Transform;
		public VideoPlayer Player;

		[SerializeField] private GameObject _gameObject;
		[SerializeField] private Renderer _renderer;

		public GameObject GetObject() => _gameObject;
		public bool IsActive() => _gameObject.activeSelf;
		public void SetActive(bool isActive) => _gameObject.SetActive(isActive);
		public void SetTexture(Texture texture) => _renderer.sharedMaterial.mainTexture = texture;

		public void Stop()
		{
			Player.Stop();
			Player.clip = null;

			_renderer.sharedMaterial.mainTexture = null;
		}
	}
}