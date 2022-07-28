using Core;
using UnityEngine;

namespace ContourEditorTool
{
	public partial class ContourEditor
	{
		public class Blackout
		{
			public static Vector2 moveOffset;
			public static int moving = -1, selected = -1;
			public bool elliptical /*,deleted*/;
			public Rect rect;
			public GameObject lassoObject = null;
			public Color farbe = Color.black;

			public static Shape shape = Shape.rect;

			//public static Color farbe=Color.black;
			public static void Select(int b)
			{
				Debug.Log("Hit a " + (blackouts[b].lassoObject == null ? "non-" : "") + "lasso blackout: ");
				if (selected > -1) DeSelect();
				selected = (Input.GetKey(deselectBlackoutKey) && Blackout.selected == b) ? -1 : b;
				if (selected > -1 && blackouts[selected].lassoObject != null)
					blackouts[selected].lassoObject.GetComponent<MeshRenderer>().material.color = selectedBlackoutColor;
				//}else Debug.Log("Selected a non-lasso blackout.");
			}

			public bool Contains(Vector2 p)
			{
				if (lassoObject != null)
				{
					//lassoed Blackout
					//Debug.Log("Single-clicked in blackout mode. hit3D: "+hit3D+", hit.collider: "+hit.collider);
					Vector3[] verts = lassoObject.GetComponent<MeshFilter>().mesh.vertices;
					int[] triangles = lassoObject.GetComponent<MeshFilter>().mesh.triangles;
					if (verts.Length < 3) return false;
					for (int t = 0; t < triangles.Length - 3; t++)
						if (SRSUtilities.PointInTriangle(p.FlipY(),
							    lassoObject.transform.InverseTransformPoint(
								    CameraHelper.Camera.WorldToScreenPoint(verts[triangles[t]])),
							    lassoObject.transform.InverseTransformPoint(
								    CameraHelper.Camera.WorldToScreenPoint(verts[triangles[t + 1]])),
							    lassoObject.transform.InverseTransformPoint(
								    CameraHelper.Camera.WorldToScreenPoint(verts[triangles[t + 2]]))))
							return true;
					return false;
				}
				else return rect.Contains(p, true);
			}

			public void SetOffsetFrom(Vector2 p)
			{
				moveOffset = screenPosition - p;
			}

			public Vector2 screenPosition
			{
				get
				{
					return lassoObject != null
						? CameraHelper.Camera.WorldToScreenPoint(lassoObject.transform.position).ToVector2XZ()
						: rect.position;
				}
				set
				{
					Debug.Log("set screen position - was: " + rect.position + ", now: " + value);
					if (lassoObject != null)
					{
						Vector3 p3d =
							lassoObject.transform.InverseTransformPoint(CameraHelper.Camera.ScreenToWorldPoint(value));
						lassoObject.transform.position = new Vector3(p3d.x, 0, p3d.z);
					}
					else rect.position = value;
				}
			}

			public static void MoveSelectedBy(Vector2 by, bool addUndo = true)
			{
				if (IsToolsBlocked)
					return;

				Debug.Log("MoveSelectedBy(" + by + "," + addUndo + ")");
				if (selected < 0) return;
				if (blackouts[selected].lassoObject != null)
				{
					Vector3[] verts = blackouts[selected].lassoObject.GetComponent<MeshFilter>().mesh.vertices;
					for (int i = 0; i < verts.Length; i++)
						verts[i] += CameraHelper.Camera.ScreenToWorldPoint(by) - CameraHelper.Camera.ScreenToWorldPoint(Vector2.zero);
					blackouts[selected].lassoObject.GetComponent<MeshFilter>().mesh.vertices = verts;
				}
				else blackouts[selected].screenPosition += by;

				if (addUndo)
					AddUndoStep(
						/*new UndoStep{moveBlackout=true,blackout=blackouts[selected],blackoutInd=selected,delta=blackouts[selected].screenPosition,
												              meshSnapshot=blackouts[selected].lassoObject!=null?blackouts[selected].lassoObject.GetComponent<MeshFilter>().mesh.vertices:null}*/); //moved a blackout
			}

			public static void DeSelect()
			{
				if (IsToolsBlocked)
					return;

				if (selected > -1 && blackouts.Count > selected && blackouts[selected].lassoObject != null)
					blackouts[selected].lassoObject.GetComponent<MeshRenderer>().material.color =
						blackouts[selected].farbe.WithAlpha(1);
				selected = -1;
			}

			public static void Delete(int b, bool addUndo = true)
			{
				if (IsToolsBlocked)
					return;

				Debug.Log("ContourEditor.Blackout.Delete(" + b + "," + addUndo + ") lassoObject not null: " +
				          (blackouts[b].lassoObject != null));
				if (selected == b) DeSelect();
				else if (selected >= b) selected--;
				if (blackouts[b].lassoObject != null) UnityEngine.Object.Destroy(blackouts[b].lassoObject);
				blackouts.RemoveAt(b);
				//		Blackout[] boa=blackouts.ToArray();
				//		boa[b].deleted=true;
				//		blackouts=boa.ToList<Blackout>();
				if (addUndo)
				{
					//if(undos.Last().moveBlackout&&undos.Last().blackoutInd==b) undos.Remove(undos.Last());//Remove undo step added when we started moving it from the beginning of the single click.
					AddUndoStep( /*new UndoStep{deleteBlackout=true,blackout=blackouts[b],blackoutInd=b }*/);
				}
			}
		}
	}
}