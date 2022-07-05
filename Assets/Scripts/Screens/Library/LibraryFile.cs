using UnityEngine;
using UnityEngine.UI;

namespace Library
{
	public class LibraryFile : MonoBehaviour
	{
		public Image ThumbnailImage;
		public Text FileNameText, ColorText;
		public Button NextColorButton, PreviousColorButton;

		private int _id;

		public void Init(int id)
		{
			_id = id;
		}

		public void Close()
		{
			Destroy(gameObject);
		}
	}
}
