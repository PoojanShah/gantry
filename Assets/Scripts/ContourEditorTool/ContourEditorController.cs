using System;
using Screens.ContourEditorScreen;
using VideoPlaying;

namespace ContourEditorTool
{
	public class ContourEditorController
	{
		private readonly Projection _projection;
		private readonly ContourEditor _contourEditor;

		public ContourEditorController(Projection projection)
		{
			_projection = projection;
			_contourEditor = _projection.GetComponent<ContourEditor>();
		}

		public void Show(Action quitAction)
		{
			_projection.transform.parent.gameObject.SetActive(true);
			_projection.gameObject.SetActive(true);
			_projection.IsEditing = true;
			_projection.enabled = true;
			_projection.GetComponent<Toolbar>().enabled = _projection.GetComponent<InfoDisplay>().enabled = true;
			_projection.gameObject.GetComponentInChildren<ContourEditorUI>().ShowDensityPanel();

			_contourEditor.Init(quitAction);
			_contourEditor.Reset(); //after toolbar's Awake, so it can select.
			_contourEditor.Restart();
		}
	}
}
