using System;
using ContourEditorTool;
using UnityEngine;

namespace Screens.ContourEditorScreen.PopUps.DensityPanel
{
	public class DensityPanel : MonoBehaviour
	{
		[SerializeField] private DensityButton[] _densityButtons;

		public void Init(Action onClose)
		{
			foreach (var b in _densityButtons)
			{
				b.Button.onClick.AddListener(() =>
				{
					ContourEditor.instance.SetDensity(b.Density);

					onClose?.Invoke();

					Destroy(gameObject);
				});
			}
		}
	}
}