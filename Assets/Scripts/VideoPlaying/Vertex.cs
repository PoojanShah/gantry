using ContourEditorTool;
using UnityEngine;
using VideoPlaying;

namespace VideoPlaying
{
	public class Vertex : MonoBehaviour
	{
		public static Vertex dragging = null;//Tracking the state ourselves instead of OnMouseDrag, since OnMouseDown may be invoked manually
		public static Vector3 dragDifferential, worldStartPoint;
		public static float y;

		private Projection _projection;

		public void Init(Projection projection)
		{
			_projection = projection;

			y = transform.position.y;
		}

		private void OnMouseDown()
		{
			Debug.Log("Vertex.OnMouseDown() position: " + transform.position);
			if (ContourEditor.toolMode != ContourEditor.ToolMode.vertex || ContourEditor.selectionShape == ContourEditor.Shape.lasso) return;
			if (Input.GetKey(KeyCode.LeftControl)) return;
			RaycastHit hit;
			if (!ContourEditor.VertexIsSelected(this)) ContourEditor.SelectVertex(this);
			Debug.DrawRay(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction * 128, Color.yellow, 30);
			if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit) || hit.collider.gameObject != gameObject)
			{
				Debug.LogWarning("We didn't hit the vertex dot, hitting " + hit.collider.name + " instead.");
				return;//Might be called by Screen when selecting the vertex.
			}
			dragDifferential = transform.position - hit.point;
			Debug.Log("We hit at " + hit.point + ". position " + transform.position + " - hit point " + hit.point + " = " + dragDifferential);
			y = Mathf.Min(_projection.transform.position.y + 0.1f, Camera.main.transform.position.y - 0.1f);
			dragging = this;
			worldStartPoint = transform.position;
		}

		private void MouseDrag()
		{
			RaycastHit hit;

			if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
				return;

			Vector3 newPos = new Vector3(hit.point.x + dragDifferential.x, y, hit.point.z + dragDifferential.z);
			ContourEditor.instance.MoveSelectedVerticesBy(newPos - transform.position);
			ContourEditor.downPoint = -Vector2.one;
		}
		private void Update()
		{
			if (ContourEditor.toolMode != ContourEditor.ToolMode.vertex) return;
			if (dragging == this)
			{
				MouseDrag();
				if (Input.GetMouseButtonUp(0)) MouseUp();
			}
		}
		private void MouseUp()
		{
			Debug.Log("Vertex.MouseUp()");
			dragging = null;
			ContourEditor.AddUndoStep();
		}
    
		public void Select(bool active = true)
		{
			GetComponent<Renderer>().material.mainTexture = active ? ContourEditor.instance.activeVertex : ContourEditor.instance.inactiveVertex;
			transform.position = new Vector3(transform.position.x, active ? ContourEditor.selectedZMargin : 0, transform.position.z);
		}
	}
}
