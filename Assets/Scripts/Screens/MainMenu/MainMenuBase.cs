using Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class MainMenuBase : MonoBehaviour
	{
		private const string QTS_VERSION_PREFIX = "v";

		[SerializeField] private TMP_Text _versionTitle;
		[SerializeField] private Image _outputType;
		[SerializeField] private GameObject _duoOutput, _singleOutput;

		protected void InitVersionTitle() => _versionTitle.text = QTS_VERSION_PREFIX + Application.version;

		protected void SetCurrentOutputType(bool isDuo)
		{
			_duoOutput.SetActive(isDuo);
			_singleOutput.SetActive(!isDuo);
		}
	}
}
