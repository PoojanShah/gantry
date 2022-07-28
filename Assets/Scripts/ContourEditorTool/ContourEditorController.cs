using System;
using ContourToolsAndUtilities;
using Core;
using Screens.ContourEditorScreen;
using UnityEngine;
using VideoPlaying;
using Object = UnityEngine.Object;

namespace ContourEditorTool
{
	public class ContourEditorController
	{
		private readonly Projection _projection;
		private readonly ContourEditor _contourEditor;
		private readonly ICommonFactory _factory;
		private readonly GameObject _editorUiPrefab;

		private ContourEditorUI _editorUi;

		public ContourEditorController(Projection projection, ICommonFactory factory,
			GameObject editorUiPrefab)
		{
			_factory = factory;
			_editorUiPrefab = editorUiPrefab;
			_projection = projection;
			_contourEditor = _projection.GetComponent<ContourEditor>();
		}

		public void ShowTools(bool isShow)
		{
			if (isShow)
			{
				_editorUi = _factory.InstantiateObject<ContourEditorUI>(_editorUiPrefab, _projection.transform);
				_editorUi.Init(_factory);
			}
			else
				Object.Destroy(_editorUi.gameObject);
		}

		public void Show(Action quitAction)
		{
			void InitEditor()
			{
				void CloseEditor()
				{
					ShowTools(false);

					quitAction?.Invoke();
				}

				_contourEditor.Init(CloseEditor); 
				_contourEditor.Reset();
				_contourEditor.Restart();
			}

			void InitProjection()
			{
				_projection.transform.parent.gameObject.SetActive(true);
				_projection.gameObject.SetActive(true);
				_projection.IsEditing = true;
				_projection.enabled = true;
				_projection.GetComponent<Toolbar>().enabled = true;
			}

			InitProjection();

			ShowTools(true);

			_editorUi.ShowDensityPanel();

			InitEditor();
		}
	}
}
