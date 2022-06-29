using UnityEngine;
using System;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Screens
{
	public class Options : MonoBehaviour
	{
		[SerializeField] private Button _cancelButton;

		public void Init()
		{
			_cancelButton.onClick.AddListener(Close);
		}
		
		private void Close() => Object.Destroy(gameObject);
	}
}