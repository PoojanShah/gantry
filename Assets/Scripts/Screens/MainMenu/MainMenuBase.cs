using Core;
using TMPro;
using UnityEngine;

namespace Screens
{
	public class MainMenuBase : MonoBehaviour
	{
		private const string QTS_LOADING_MESSAGE = "Loading...";
		private const string QTS_VERSION_PREFIX = "v";

		[SerializeField] private TMP_Text _versionTitle;

		protected void InitVersionTitle() => _versionTitle.text = QTS_VERSION_PREFIX + Application.version;
		protected void SetUiBlocker(bool isBlocked)
		{
			if(isBlocked) 
				InputBlocker.Block(QTS_LOADING_MESSAGE);
			else 
				InputBlocker.Unblock();
		}
	}
}
