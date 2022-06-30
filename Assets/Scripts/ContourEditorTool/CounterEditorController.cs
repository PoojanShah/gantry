using Core;
using VideoPlaying;

namespace ContourEditorTool
{
	public class CounterEditorController
	{
		private readonly Projection _projection;
		private readonly ContourEditor _contourEditor;

		public CounterEditorController(ICommonFactory factory, Projection projection)
		{
			_projection = projection;
			_contourEditor = _projection.GetComponent<ContourEditor>();
		}

		public void Show()
		{
			_projection.gameObject.SetActive(true);
			_projection.IsEditing = true;
			_projection.enabled = true;
			_projection.GetComponent<Toolbar>().enabled = _projection.GetComponent<InfoDisplay>().enabled = true;

			_contourEditor.Reset(); //after toolbar's Awake, so it can select.
			_contourEditor.Restart();
		}
	}
}
