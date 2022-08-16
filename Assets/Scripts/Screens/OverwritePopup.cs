using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class OverwritePopup : MonoBehaviour
	{
		[SerializeField] private Button _confirmButton, _cancelButton;

		public void Init(Action confirmAction)
		{
			_cancelButton.onClick.AddListener(Close);
			_confirmButton.onClick.AddListener(() =>
			{
				confirmAction();

				Close();
			});
		}

		private void Close()
		{
			_cancelButton.onClick.RemoveAllListeners();
			_confirmButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}