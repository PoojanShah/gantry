using Core;
using UnityEngine;

namespace VideoPlaying
{
	public class ProjectionController
	{
		private readonly ICommonFactory _commonFactory;
		private readonly GameObject _prefab;

		private ProjectionView _projectionView;

		public ProjectionController(ICommonFactory commonFactory, GameObject prefab)
		{
			_commonFactory = commonFactory;
			_prefab = prefab;
		}

		public void Play()
		{
			_projectionView = _commonFactory.InstantiateObject<ProjectionView>(_prefab);
			_projectionView.Projection.StartMovie();
		}
	}
}
