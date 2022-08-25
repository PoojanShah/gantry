using System;
using UnityEngine;

namespace Screens
{
	public class SwipeDetection : MonoBehaviour
	{
		private const  float SWIPE_MINIMAL_RANGE = 10f;
		private bool _isSwiping;
		private Vector2 _startPosition;
		private Action<bool> _onSwipeAction;

		public void Init(Action<bool> onSwipeAction) => 
			_onSwipeAction = onSwipeAction;
		
		private void Update()
		{
			if(Input.touchCount == 0 || _onSwipeAction == null)
				return;
			
			var inputPhase = Input.GetTouch(0).phase;
			
			switch (inputPhase)
			{
				case TouchPhase.Began:
					_isSwiping = true;
					_startPosition = Input.GetTouch(0).position;
					break;
				case TouchPhase.Canceled:
				case TouchPhase.Ended:
					Swipe();
					break;
				default:
					break;
			}
		}

		private void Swipe()
		{
			if (!_isSwiping)
				return;
			
			var endPos = Input.GetTouch(0).position;
			var swipeDelta = endPos - _startPosition;
			
			if (swipeDelta.magnitude < SWIPE_MINIMAL_RANGE)
				return;
			
			if (Mathf.Abs(swipeDelta.x) <= Mathf.Abs(swipeDelta.y)) //Check isHorisontal
				return;

			var isRight = swipeDelta.x < 0;

			_onSwipeAction?.Invoke(isRight);

			ResetValues();
		}

		private void ResetValues()
		{
			_isSwiping = false;
			_startPosition = Vector2.zero;
		}

		private void OnDestroy() => _onSwipeAction = null;
	}
}