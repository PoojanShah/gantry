using Core;
using VideoPlaying;

namespace ContourEditorTool
{
	public class CounterEditorController
	{
		private readonly Projection _projection;

		public CounterEditorController(ICommonFactory factory, Projection projection)
		{
			_projection = projection;
		}

		public void Show()
		{
			_projection.transform.gameObject.SetActive(true);
			_projection.IsEditing = true;
			_projection.enabled = true;
			_projection.GetComponent<Toolbar>().enabled = _projection.GetComponent<InfoDisplay>().enabled = true;

			//_contourEditor.Reset(); //after toolbar's Awake, so it can select.
			//_contourEditor.Restart();
		}
	}
}
