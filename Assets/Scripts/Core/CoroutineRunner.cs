using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core
{
	public class CoroutineRunner : MonoBehaviour
	{
		public static CoroutineRunner Instance;

		private List<IEnumerator> _coroutines;

		public void Init()
		{
			Instance = this;

			_coroutines = new List<IEnumerator>();
		}

		public void LaunchCoroutine(IEnumerator coroutine)
		{
			if(_coroutines.Contains(coroutine))
				return;

			_coroutines.Add(coroutine);

			StartCoroutine(coroutine);
		}

		private void OnDestroy()
		{
			foreach (var enumerator in _coroutines)
				StopCoroutine(enumerator);

			_coroutines.Clear();
		}
	}
}
