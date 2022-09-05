using Core;
using UnityEngine;
using UnityEngine.Video;

namespace VideoPlaying
{
	public class ProjectionOutputView : MonoBehaviour
	{
		private const string SecondaryNameFormat = "Secondary_{0}";
		private const string PrimaryName = "Primary";

		public VideoPlayer Player;
		public Transform Transform;

		[SerializeField] private Camera _camera;
		[SerializeField] private GameObject _gameObject;
		[SerializeField] private Renderer _renderer;

		private Material _tempMaterial;

		public void Init(int targetId)
		{
			_camera.targetDisplay = targetId;
			_camera.backgroundColor = Color.black;

			if (targetId > 0)
			{
				_camera.transform.localRotation =
					Quaternion.Euler(_camera.transform.localRotation.eulerAngles +
					                 Vector3.up * Constants.CameraRotationSecondaryOutputs);

				transform.name = string.Format(SecondaryNameFormat, targetId);
			}
			else
			{
				transform.name = PrimaryName;

				if (!_camera.TryGetComponent(typeof(AudioListener), out _))
					_camera.gameObject.AddComponent<AudioListener>();
			}

			_tempMaterial = new Material(_renderer.material);
			_renderer.material = _tempMaterial;
		}

		public GameObject GetObject() => _gameObject;
		public bool IsActive() => _gameObject.activeSelf;
		public void SetActive(bool isActive) => _gameObject.SetActive(isActive);
		public void SetTexture(Texture texture)
		{
			StopVideo();

			_tempMaterial.mainTexture = texture;
		}

		public void ApplyRotation(bool isRotationEnabled)
		{
			const float rotationSetting = 180.0f;

			var rotation =!isRotationEnabled
				? Quaternion.Euler(90.0f, 0.0f, 0.0f)
				: Quaternion.Euler(90.0f, rotationSetting, 0.0f);

			_camera.transform.localRotation = rotation;
		}

		public void StopVideo()
		{
			Player.Stop();
			Player.clip = null;
		}
	}
}