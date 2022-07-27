using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Screens.ContourEditorScreen.Toolbar
{
	public class ButtonEventsHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public Action OnPointerEnterAction { get; set; }
		public Action OnPointerExitAction { get; set; }

		public void OnPointerEnter(PointerEventData eventData) =>
			OnPointerEnterAction?.Invoke();

		public void OnPointerExit(PointerEventData eventData) =>
			OnPointerExitAction?.Invoke();

		private void OnDestroy()
		{
			OnPointerEnterAction = null;
			OnPointerExitAction = null;
		}
	}
}