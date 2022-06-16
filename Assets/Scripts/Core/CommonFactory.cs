using UnityEngine;

namespace Core
{
	public interface ICommonFactory
	{
		T InstantiateObject<T>(GameObject prefab, Transform parent = null) where T : Object;
	}

	public class CommonFactory : ICommonFactory
	{
		T ICommonFactory.InstantiateObject<T>(GameObject prefab, Transform parent)
		{
			if (prefab == null)
				return default;

			var go = Object.Instantiate(prefab, parent);

			return go == null ? default : go.GetComponent<T>();
		}
	}
}