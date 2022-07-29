using TMPro;
using UnityEngine;

namespace Network
{
	public class NetworkDebugger : MonoBehaviour
	{
		private static TMP_Text _debugText;

		private void Awake() => _debugText = GetComponent<TMP_Text>();
		public static void SetMessage(string message) => _debugText.text = message;
	}
}
