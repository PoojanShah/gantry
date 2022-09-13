using TMPro;
using UnityEngine;

namespace Screens
{
	public class MainMenuBase : MonoBehaviour
	{
		private const string QTS_VERSION_PREFIX = "v";

		[SerializeField] private TMP_Text _versionTitle;
		[SerializeField] private GameObject _uiInputBlocker;

		protected void InitVersionTitle() => _versionTitle.text = QTS_VERSION_PREFIX + Application.version;
		protected void SetUiBlocker(bool isBlocked) => _uiInputBlocker.SetActive(isBlocked);
	}
}
