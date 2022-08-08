using System;
using Core;
using Library;
using UnityEngine;
using UnityEngine.UI;

namespace Screens
{
	//TODO: Change name when screen will have name
	public class LibraryOptions : MonoBehaviour
	{
		[SerializeField] private OptionsMenu _optionsMenu;
		[SerializeField] private LibraryScreen _libraryScreen;
		
		public void Init(ICommonFactory factory, Action quitAction)
		{
			_optionsMenu.Init(() =>Quit(quitAction), SwitchScreens);
			_libraryScreen.Init(factory, () => Quit(quitAction), SwitchScreens);
			
			_libraryScreen.gameObject.SetActive(false);
			_optionsMenu.gameObject.SetActive(true);
		}

		private void Quit(Action quitAction)
		{
			_optionsMenu.SaveAndExit();
			_libraryScreen.SaveAndExit();
			
			quitAction?.Invoke();
			
			Destroy(gameObject);
		}

		private void SwitchScreens()
		{
			var optionsIsActive = _optionsMenu.gameObject.activeSelf;
			
			_optionsMenu.gameObject.SetActive(!optionsIsActive);
			_libraryScreen.gameObject.SetActive(optionsIsActive);
		}
	}
}