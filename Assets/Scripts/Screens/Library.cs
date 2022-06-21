using System;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	public class Library : MonoBehaviour
	{
		[SerializeField] private Button _exitButton;

		public void Init(Action onQuitAction)
		{
			_exitButton.onClick.AddListener(() => { onQuitAction?.Invoke(); });
		}
	}
}