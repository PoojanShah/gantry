using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Screens.ContourEditorScreen
{
	public class ButtonEventsHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		private Action _onPointerEnterAction, _onPointerExitAction;

		public Action OnPointerEnterAction
		{
			get => _onPointerEnterAction;
			set => _onPointerEnterAction = value;
		}
		
		public Action OnPointerExitAction
		{
			get => _onPointerExitAction;
			set => _onPointerExitAction = value;
		}

		public void OnPointerEnter(PointerEventData eventData) =>
			_onPointerEnterAction?.Invoke();

		public void OnPointerExit(PointerEventData eventData) =>
			_onPointerExitAction?.Invoke();
	}
}