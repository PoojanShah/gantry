using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Screens.ContourEditorScreen
{
	public class ButtonEventsHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		public Action _onPointerEnterAction;
		public Action _onPointerExitAction;

		public void OnPointerEnter(PointerEventData eventData)
		{
			_onPointerEnterAction?.Invoke();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_onPointerExitAction?.Invoke();
		}
	}
}