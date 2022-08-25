using Network;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class ServerLoginPopup : MonoBehaviour
	{
		private readonly Vector2Int _ipValueRange = new(2, 255);

		[SerializeField] private Button _connectButton;
		[SerializeField] private TMP_InputField _ipEnd;
		[SerializeField] private TMP_Text _ipStart;

		private static bool _isConnectionStarted = false;

		public void Init()
		{
			if(!_isConnectionStarted)
				TryUseSavedIp();
			
			_connectButton.onClick.AddListener(ConnectClicked);
			_ipEnd.onEndEdit.AddListener(VerifyIpNumber);

			InitIpLabel();
		}

		private void InitIpLabel() => _ipStart.text = NetworkHelper.GetMyIpWithoutLastNumberString();

		private void ConnectClicked()
		{
			if (!int.TryParse(_ipEnd.text, out var number))
				return;

			LocalNetworkClient.Connect(number);

			Close();
		}

		private void Update()
		{
			if(Input.GetKeyDown(KeyCode.Return))
				_connectButton.onClick.Invoke();
		}

		private void VerifyIpNumber(string currentValue)
		{
			if (!int.TryParse(currentValue, out var number)) 
				return;

			if (number < _ipValueRange.x)
				number = _ipValueRange.x;
			else if (number > _ipValueRange.y)
				number = _ipValueRange.y;

			_ipEnd.text = number.ToString();

			NetworkHelper.LastIpNumber = int.Parse(_ipEnd.text);
		}

		private void TryUseSavedIp()
		{
			if (!NetworkHelper.IsSavedIpValid())
				return;

			_isConnectionStarted = true;

			_ipEnd.text = NetworkHelper.LastIpNumber.ToString();

			ConnectClicked();
		}

		private void Close()
		{
			_ipEnd.onEndEdit.RemoveAllListeners();
			_connectButton.onClick.RemoveAllListeners();

			Destroy(gameObject);
		}
	}
}