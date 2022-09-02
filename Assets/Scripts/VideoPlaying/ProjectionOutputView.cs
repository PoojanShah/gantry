using Core;
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

			if (targetId > 0)
				_camera.transform.localRotation =
					Quaternion.Euler(_camera.transform.localRotation.eulerAngles +
					                 Vector3.up * Constants.CameraRotationSecondaryOutputs);
		}

		public GameObject GetObject() => _gameObject;
		public bool IsActive() => _gameObject.activeSelf;
		public void SetActive(bool isActive) => _gameObject.SetActive(isActive);
		public void SetTexture(Texture texture) => _renderer.sharedMaterial.mainTexture = texture;

		public void ApplyRotation(bool isRotationEnabled)
		{
			const float rotationSetting = 180.0f;

			var rotation =!isRotationEnabled
				? Quaternion.Euler(90.0f, 0.0f, 0.0f)
				: Quaternion.Euler(90.0f, rotationSetting, 0.0f);

			_camera.transform.localRotation = rotation;
		}

		public void Stop()
		{
			Player.Stop();
			Player.clip = null;

			_renderer.sharedMaterial.mainTexture = null;
		}
	}
}