using UnityEngine;
using UnityEngine.Video;

namespace VideoPlaying
{
	public class ProjectionOutputView : MonoBehaviour
	{
		public VideoPlayer Player;
		public Transform Transform;

		[SerializeField] private Camera _camera;
		[SerializeField] private GameObject _gameObject;
		[SerializeField] private Renderer _renderer;

		public void Init(int targetId)
		{
			_camera.targetDisplay = targetId;
			_camera.backgroundColor = Color.black;
		}

		public GameObject GetObject() => _gameObject;
		public bool IsActive() => _gameObject.activeSelf;
		public void SetActive(bool isActive) => _gameObject.SetActive(isActive);
		public void SetTexture(Texture texture) => _renderer.sharedMaterial.mainTexture = texture;

		public void Stop()
		{
			Player.url = null;
			Player.clip = null;
			Player.Stop();

			_renderer.sharedMaterial.mainTexture = null;
		}
	}
}