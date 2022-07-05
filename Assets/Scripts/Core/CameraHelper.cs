using UnityEngine;

namespace Core
{
	public static class CameraHelper
	{
		private static Camera _camera;

		public static void Init() => _camera = Camera.main;
		public static void SetBackgroundColor(Color color) => _camera.backgroundColor = color;
	}
}
