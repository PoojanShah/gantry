using UnityEngine;

namespace Core
{
	public static class CameraHelper
	{
		public static Camera Camera;

		public static void Init() => Camera = Camera.main;
		public static void SetCameraPosition(Vector3 position) => Camera.transform.position = position;
		public static void SetBackgroundColor(Color color) => Camera.backgroundColor = color;
	}
}
