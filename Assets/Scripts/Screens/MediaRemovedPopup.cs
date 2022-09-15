using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class MediaRemovedPopup : MonoBehaviour
	{
		[SerializeField] private Button _okButton;

		public void Init() => _okButton.onClick.AddListener(Close);

		private void Close()
		{
			_okButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}