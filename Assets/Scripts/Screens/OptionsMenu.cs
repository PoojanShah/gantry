using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Screens
{
	public class OptionsMenu : MonoBehaviour
	{
		[SerializeField] private Button _cancelButton;

		public void Init()
		{
			_cancelButton.onClick.AddListener(() => Destroy(gameObject));
		}
	}
}