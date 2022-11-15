using TMPro;
using UnityEngine;

namespace Core
{
	public class InputBlocker : MonoBehaviour
	{
		private static GameObject _blocker;
		private static TMP_Text _message;
		private static Transform _transform;

		public void Awake()
		{
			_blocker = transform.GetChild(0).gameObject;
			_message = transform.GetChild(0).GetChild(0).GetComponent<TMP_Text>();
			_transform = transform;

			Unblock();
		}

		public static void Block(string message)
		{
			_blocker.SetActive(true);
			_message.text = message;

			_transform.SetAsLastSibling();
		}

		public static void Unblock() => _blocker.SetActive(false);
	}
}
