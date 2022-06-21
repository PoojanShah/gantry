using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class ExitPopUp : MonoBehaviour
	{
		[SerializeField] private Button _quitButton, _cancelButton;

		public void Init(Action onQuitAction)
		{
			_cancelButton.onClick.AddListener(Close);
			_quitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });
		}

		private void Close() => Destroy(gameObject);
	}
}