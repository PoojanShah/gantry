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
		private readonly ICommonFactory _factory;
		private readonly GameObject _editorUiPrefab;
		private readonly ProjectionController _projectionController;
		
		private Projection _projection;
		private ContourEditor _contourEditor;
		private ContourEditorUI _editorUi;

		public ContourEditorController(ProjectionController projectionController, ICommonFactory factory,
			GameObject editorUiPrefab)
		{
			_factory = factory;
			_editorUiPrefab = editorUiPrefab;
			_projectionController = projectionController;
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
			_projection = _projectionController.GetProjection();
			_contourEditor = _projection.GetComponent<ContourEditor>();
			
			void InitEditor()
			{
				void CloseEditor()
				{
					Debug.Log("Point CE1");
					ShowTools(false);
					Debug.Log("Point CE2");

					quitAction?.Invoke();
					Debug.Log("Point CE3");
				}
				Debug.Log("Point show1");
				_contourEditor.Init(CloseEditor); 
				Debug.Log("Point show2");
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
				_projection.GetComponent<ContourEditor>().enabled = true;
			}

			InitProjection();

			ShowTools(true);

			_editorUi.ShowDensityPanel();

			InitEditor();
		}
	}
}
