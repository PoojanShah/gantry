using Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class MainMenuBase : MonoBehaviour
	{
		private const string QTS_VERSION_PREFIX = "v";

		[SerializeField] protected OutputTypesConfig OutputTypesConfig;
		[SerializeField] private TMP_Text _versionTitle;
		[SerializeField] private Image _outputType;

		protected void InitVersionTitle() => _versionTitle.text = QTS_VERSION_PREFIX + Application.version;
		protected void SetCurrentOutputType(Sprite type) => _outputType.sprite = type;
	}
}
