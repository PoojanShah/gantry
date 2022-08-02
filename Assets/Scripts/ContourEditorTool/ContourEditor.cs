using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using ContourToolsAndUtilities;
using VideoPlaying;
using Core;

namespace ContourEditorTool
{
	public partial class ContourEditor : MonoBehaviour
	{
		public static bool IsToolsBlocked = true;

		public static KeyCode
			deleteBlackoutKey = KeyCode.LeftAlt,
			moveBlackoutKey = KeyCode.LeftShift,
			deselectBlackoutKey = KeyCode.LeftShift,
			buildLassoKey = KeyCode.M,
			centeredSelectionKey = KeyCode.LeftControl,
			addSelectKey = KeyCode.LeftShift,
			coarseInchKey = KeyCode.LeftShift,
			createLassoBlackoutKey = KeyCode.Return;

		public static List<int> selectedVertices = new List<int>();
		public static ToolMode toolMode = ToolMode.vertex;
		public static Vector3 rawSize = new Vector3(5, 0, 5);

		private static Color selectedBlackoutColor = new Color(0, 0, 0.5f, 0.5f);
		private static List<Vertex> vertexDots = new List<Vertex>();
		private static Texture2D[] icons;
		private static int background;
		private static int saveAsDefault = -1;
		private static List<Vector3> lassoPoints = new List<Vector3>();
		private static List<LineRenderer> lines = new List<LineRenderer>();
		private static List<Blackout> blackouts = new List<Blackout>();
		private static List<UndoStep> undos = new List<UndoStep>();
		private static int undo = 0;
		private static float editingLassoAlpha = 0.5f;
		public static ContourEditor instance;

		private static Tool[] toolBehaviour = new Tool[]
		{
			//For each,MouseDown,MouseDrag,Draw,MouseUp
			new Tool
			{
				//Vertex
				OnMouseDown = (p) =>
				{
					if (IsToolsBlocked)
						return;

					if (selectionShape == Shape.lasso) AddSelectionLassoPunkt(p);
				},
				OnDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					if (selectionShape == Shape.lasso) UpdateSelectionLasso(p);
				},
				OnFinishDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					Debug.Log("Vertex mode's OnFinishDrag(" + p + ") selectionShape: " + selectionShape +
					          ",downPoint: " + downPoint + ",downPoint.FlipY(): " + downPoint.FlipY());
					if (selectionShape == Shape.lasso)
					{
					}
					else
					{
						Vector2 startPoint = Input.GetKey(centeredSelectionKey) ? p + (downPoint - p) * 2 : downPoint;
						Debug.Log("Vertex mode's OnFinishDrag(" + p + ") startPoint: " + startPoint + ",downPoint: " +
						          downPoint + ",downPoint.FlipY(): " + downPoint.FlipY());
						/*if(dragging)*/
						GroupSelect(new Rect( //bandboxing
							Mathf.Min(startPoint.x, p.x), Mathf.Min(startPoint.y, p.y),
							Mathf.Abs(p.x - startPoint.x), Mathf.Abs(p.y - startPoint.y)));
					}
				},
				Draw = (p) =>
				{
					if (IsToolsBlocked)
						return;

					Vector2 startPoint = Input.GetKey(centeredSelectionKey)
						? p + (downPoint.FlipY() - p) * 2
						: downPoint.FlipY();
					if (dragging)
						if (selectionShape == Shape.rect)
							Graphics.DrawRectAround(startPoint, SRSUtilities.adjustedFlipped, Color.red);
						else if (selectionShape == Shape.ellipse)
							DrawEllipseAround(startPoint, SRSUtilities.adjustedFlipped); //Kopf
					if (Input.GetKeyDown(KeyCode.Return)) LassoSelection(Input.GetKey(addSelectKey));
					if (circle) Graphics.DrawCursorIcon(icons.ByName("Circle Mode"));
				},
				OnSingleClick = (p) =>
				{
					if (IsToolsBlocked)
						return;

					Debug.Log("Tool.OnSingleClick(" + p + ") (Vertex mode)");
					RaycastHit hit;
					if (!Physics.Raycast(CameraHelper.Camera.ScreenPointToRay(SRSUtilities.adjustedMousePosition), out hit,
						    Mathf.Infinity, 1 << LayerMask.NameToLayer("Projection")))
					{
						Debug.Log("Mouse didn't hit anything; deselecting.");
						downPoint = -Vector2.one;
						DeSelect();
						return;
					}
					else if (hit.collider.gameObject != instance.gameObject)
					{
						Debug.LogWarning("Mouse hit something other than the screen: " + hit.collider.name);
						downPoint = -Vector2.one;
						return;
					}
					else if (Vertex.dragging == null)
					{
						//Hit projection; Die naereste Vertex wahlen.
						Mesh m = instance.GetComponent<MeshFilter>().mesh;
						int nearestVertex = -1;
						float closestDistance = Mathf.Infinity, distance;

						for (int v = 0; v < m.vertices.Length; v++)
							if ((distance = Vector3.Distance(hit.point.Flatten(1),
								    instance.transform.TransformPoint(m.vertices[v]).Flatten(1))) < closestDistance)
							{
								nearestVertex = v;
								closestDistance = distance;
							}

						if (nearestVertex < 0)
						{
							Debug.LogError("Nearest vertex not found.");
							return;
						}

						Debug.Log("Nearest to hit point " + hit.point + " was " + nearestVertex + " (" +
						          instance.transform.TransformPoint(m.vertices[nearestVertex]) + ") at distance :" +
						          closestDistance + ".");
						if (!Input.GetKey(addSelectKey)) DeSelect(false);
						SelectVertex(nearestVertex);
					}
				},
				keyDowns = new Dictionary<KeyCode, Action>()
				{
					{
						KeyCode.Delete, () =>
						{

							if (IsToolsBlocked)
								return;

							Delete(ApplicableVertices());
							Debug.Log("Deleted triangles containing vertices " + ApplicableVertices().Stringify() +
							          ".");
						}
					},
					{ KeyCode.UpArrow, () => instance.MoveSelectedVerticesBy(Vector3.forward * inch) },
					{ KeyCode.DownArrow, () => instance.MoveSelectedVerticesBy(Vector3.back * inch) },
					{ KeyCode.LeftArrow, () => instance.MoveSelectedVerticesBy(Vector3.left * inch) },
					{ KeyCode.RightArrow, () => instance.MoveSelectedVerticesBy(Vector3.right * inch) },
					{ KeyCode.Escape, () => DeSelect(true) }
				}
			},
			new Tool
			{
				//Blackout
				OnMouseDown = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Down
					Debug.Log("Blackout.selected: " + Blackout.selected + ", count: " + blackouts.Count);
					if (Blackout.selected > -1 && blackouts[Blackout.selected].Contains(p.FlipY()))
					{
						blackouts[Blackout.moving = Blackout.selected].SetOffsetFrom(p.FlipY());
						Debug.Log("Starting to move blackout[" + Blackout.moving + "]. UndoStep's position: " +
						          new Vector2(blackouts[Blackout.moving].rect.x, blackouts[Blackout.moving].rect.y));
					}
					else if (Blackout.shape == Shape.lasso)
						for (int i = 0; i < 2; i++)
							AddLassoPoint(p, Color.black);
				},
				Draw = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Draw
					if (Blackout.moving < 0)
						switch (Blackout.shape)
						{
							case Shape.rect:
								if (dragging)
									Graphics.DrawBox(
										SRSUtilities.RectAround(
											Input.GetKey(centeredSelectionKey)
												? SRSUtilities.adjustedFlipped +
												  (downPoint.FlipY() - SRSUtilities.adjustedFlipped) * 2
												: downPoint.FlipY(), SRSUtilities.adjustedFlipped),
										new Color(0, 0, 0, 1));
								break;
							case Shape.ellipse:
								if (dragging)
									Graphics.DrawFilledEllipse(
										SRSUtilities.RectAround(
											Input.GetKey(centeredSelectionKey)
												? SRSUtilities.adjustedFlipped +
												  (downPoint.FlipY() - SRSUtilities.adjustedFlipped) * 2
												: downPoint.FlipY(), SRSUtilities.adjustedFlipped),
										new Color(0, 0, 0, 1));
								break;
							case Shape.lasso:
								if (Input.GetKey(createLassoBlackoutKey) && lassoBlackout != null)
									FinishBuildingLassoBlackout();
								break;
							default:
								Debug.LogError("Unbekannst Blackout.shape: " + Blackout.shape);
								break;
						}
				},
				OnDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					if (Blackout.moving > -1)
					{
						Debug.Log("Lasso mode's OnDrag; moving: " + Blackout.moving + ", lassoObj: " +
						          (blackouts[Blackout.moving].lassoObject != null));
						if (blackouts[Blackout.moving].lassoObject != null) Blackout.MoveSelectedBy(MouseHelper.delta, false);
						else blackouts[Blackout.moving].screenPosition = p.FlipY() + Blackout.moveOffset;
					}
					else if (Blackout.shape == Shape.lasso) MoveLastLassoPoint(p);
				},
				OnFinishDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Up
					if (Blackout.moving > -1) Blackout.moving = -1;
					else if (Blackout.shape != Shape.lasso)
					{
						//Wasn't moving a blackout,created a new one.
						blackouts.Add(new Blackout
						{
							rect = SRSUtilities.RectAround(
								Input.GetKey(centeredSelectionKey)
									? SRSUtilities.adjustedFlipped +
									  (downPoint.FlipY() - SRSUtilities.adjustedFlipped) * 2
									: downPoint.FlipY(), SRSUtilities.adjustedFlipped),
							elliptical = Blackout.shape == Shape.ellipse
						}); //Adding a blackout
						AddUndoStep(
 /*/*new UndoStep{createBlackout=true,blackout=blackouts.Last(),blackoutInd=blackouts.Count-1}*/);
					}
					else
					{
						//Added a lasso punkt

					}
				},
				OnSingleClick = (p) =>
				{
					if (IsToolsBlocked)
						return;

					int b;
					for (b = blackouts.Count - 1; b > -1; b--)
						if (blackouts[b].Contains(SRSUtilities.adjustedFlipped))
						{
							Blackout.Select(b);
							break;
						}

					if (b == -1)
					{
						Blackout.DeSelect();
					}
				},
				keyDowns = new Dictionary<KeyCode, Action>()
				{
					{ KeyCode.Delete, () => Blackout.Delete(Blackout.selected) },
					{ KeyCode.UpArrow, () => Blackout.MoveSelectedBy(Vector2.down * inch * 10) },
					{ KeyCode.DownArrow, () => Blackout.MoveSelectedBy(Vector2.up * inch * 10) },
					{ KeyCode.LeftArrow, () => Blackout.MoveSelectedBy(Vector2.left * inch * 10) },
					{ KeyCode.RightArrow, () => Blackout.MoveSelectedBy(Vector2.right * inch * 10) },
					{ KeyCode.Escape, Blackout.DeSelect },
				}
			},
			new Tool
			{
				//Whiteout
				OnMouseDown = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Down
					Debug.Log("Blackout.selected: " + Blackout.selected + ", count: " + blackouts.Count);
					if (Blackout.selected > -1 && blackouts[Blackout.selected].Contains(p.FlipY()))
					{
						blackouts[Blackout.moving = Blackout.selected].SetOffsetFrom(p.FlipY());
						Debug.Log("Starting to move blackout[" + Blackout.moving + "]. UndoStep's position: " +
						          new Vector2(blackouts[Blackout.moving].rect.x, blackouts[Blackout.moving].rect.y));
					}
					else if (Blackout.shape == Shape.lasso)
						for (int i = 0; i < 2; i++)
							AddLassoPoint(p, Color.white);
				},
				Draw = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Draw
					if (Blackout.moving < 0)
						switch (Blackout.shape)
						{
							case Shape.rect:
								if (dragging)
									Graphics.DrawBox(
										SRSUtilities.RectAround(
											Input.GetKey(centeredSelectionKey)
												? SRSUtilities.adjustedFlipped +
												  (downPoint.FlipY() - SRSUtilities.adjustedFlipped) * 2
												: downPoint.FlipY(), SRSUtilities.adjustedFlipped), Color.white);
								break;
							case Shape.ellipse:
								if (dragging)
									Graphics.DrawFilledEllipse(
										SRSUtilities.RectAround(
											Input.GetKey(centeredSelectionKey)
												? SRSUtilities.adjustedFlipped +
												  (downPoint.FlipY() - SRSUtilities.adjustedFlipped) * 2
												: downPoint.FlipY(), SRSUtilities.adjustedFlipped), Color.white);
								break;
							case Shape.lasso:
								if (Input.GetKey(createLassoBlackoutKey) && lassoBlackout != null)
									FinishBuildingLassoBlackout();
								break;
							default:
								Debug.LogError("Unbekannst Blackout.shape: " + Blackout.shape);
								break;
						}
				},
				OnDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					if (Blackout.moving > -1)
					{
						Debug.Log("Lasso mode's OnDrag; moving: " + Blackout.moving + ", lassoObj: " +
						          (blackouts[Blackout.moving].lassoObject != null));
						if (blackouts[Blackout.moving].lassoObject != null) Blackout.MoveSelectedBy(MouseHelper.delta, false);
						else blackouts[Blackout.moving].screenPosition = p.FlipY() + Blackout.moveOffset;
					}
					else if (Blackout.shape == Shape.lasso) MoveLastLassoPoint(p);
				},
				OnFinishDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Up
					if (Blackout.moving > -1) Blackout.moving = -1;
					else if (Blackout.shape != Shape.lasso)
					{
						//Wasn't moving a blackout,created a new one.
						blackouts.Add(new Blackout
						{
							rect = SRSUtilities.RectAround(
								Input.GetKey(centeredSelectionKey)
									? SRSUtilities.adjustedFlipped +
									  (downPoint.FlipY() - SRSUtilities.adjustedFlipped) * 2
									: downPoint.FlipY(), SRSUtilities.adjustedFlipped),
							elliptical = Blackout.shape == Shape.ellipse, farbe = Color.white
						}); //Adding a blackout
						AddUndoStep(
 /*/*new UndoStep{createBlackout=true,blackout=blackouts.Last(),blackoutInd=blackouts.Count-1}*/);
					} //else{//Added a lasso punkt
					//}
				},
				OnSingleClick = (p) =>
				{
					if (IsToolsBlocked)
						return;

					int b;
					for (b = blackouts.Count - 1; b > -1; b--)
						if ( /*!blackouts[i].deleted&&*/blackouts[b].Contains(SRSUtilities.adjustedFlipped))
						{
							Blackout.Select(b);
							break;
						}

					if (b == -1)
					{
						//if(hit.collider!=null)Debug.LogError("Hit a stray 3D blackout: "+hit.collider.name);
						Blackout.DeSelect();
					}
				},
				keyDowns = new Dictionary<KeyCode, Action>()
				{
					{ KeyCode.Delete, () => Blackout.Delete(Blackout.selected) },
					{ KeyCode.UpArrow, () => Blackout.MoveSelectedBy(Vector2.down * inch * 10) },
					{ KeyCode.DownArrow, () => Blackout.MoveSelectedBy(Vector2.up * inch * 10) },
					{ KeyCode.LeftArrow, () => Blackout.MoveSelectedBy(Vector2.left * inch * 10) },
					{ KeyCode.RightArrow, () => Blackout.MoveSelectedBy(Vector2.right * inch * 10) },
					{ KeyCode.Escape, Blackout.DeSelect },
				}
			},
			new Tool
			{
				//Scale
				OnMouseDown = (p) =>
				{
					if (IsToolsBlocked)
						return;

					if (ScaleZones().Any((r) => r.Contains(p, true))) BeginScale(p, scaleMode);
				},
				OnDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Update
					Vector3[] verts = instance.GetComponent<MeshFilter>().mesh.vertices;
					Vector2 clampMin =
							new Vector2(
								1 - (convergingEdgeScreen.x - oppositeScreenEdge.x) /
								(convergingEdgeScreen.x - oppositeEdgeScreen.x),
								1 - (convergingEdgeScreen.y - oppositeScreenEdge.y) /
								(convergingEdgeScreen.y - oppositeEdgeScreen.y)),
						clampMax = Vector2.one;
					for (int i = verts.Length - 1; i > -1; i--)
						if (!deleted.Contains(i))
						{
							//float perpendicularProximity=Mathf.Abs(scalePivot.y-(float)(i/columns)/(float)columns);
							float perpendicularProximity =
								(float)(scaleMode == ScaleMode.horizontal ? (i / columns) : (i % columns)) /
								(float)columns;
							if (scaleMode == ScaleMode.vertical) perpendicularProximity = 1 - perpendicularProximity;
							float pivot = scaleMode == ScaleMode.normal
								? -1
								: scalePivot[
									2 - (int)scaleMode]; //We have not explored all possible implications of the -1. It works for now.
							//float perspectiveFactor=(scaleMode==ScaleMode.normal?1:Mathf.Abs(1-pivot-perpendicularProximity));
							//float perspectiveFactor=(scaleMode==ScaleMode.normal?1:Mathf.Abs((scaleMode==ScaleMode.horizontal?1-(pivot+perpendicularProximity):(pivot+perpendicularProximity))));
							float perspectiveFactor = (scaleMode == ScaleMode.normal
								? 1
								: Mathf.Abs(1 - (pivot + perpendicularProximity)));
							//if(i==22||i==19)Debug.Log("Verts["+i+"] before: "+verts[i]+", perpendicularProximity: "+perpendicularProximity+", pivot: "+pivot+", perspectiveFactor: "+perspectiveFactor);
							if (i == 22 || i == 40)
								Debug.Log("pivot: " + pivot + ", perp: " + perpendicularProximity + ", scaleSnapshot[" +
								          i + "].z: " + scaleSnapshot[i].z + ", convergingEdgeWorld.y: " +
								          convergingEdgeWorld.y + ", tween: " +
								          Mathf.Clamp(
									          (SRSUtilities.adjustedMousePosition.y - downPoint.y) /
									          (convergingEdgeScreen.y - downPoint.y), clampMin.y, clampMax.y));
							verts[i] = new Vector3(
								scalePivot.x < 0
									? verts[i].x
									: SRSUtilities.Lerp(scaleSnapshot[i].x, convergingEdgeWorld.x,
										Mathf.Clamp(
											(SRSUtilities.adjustedMousePosition.x - downPoint.x) /
											(convergingEdgeScreen.x - downPoint.x), clampMin.x, clampMax.x) *
										(scaleMode == ScaleMode.horizontal
											? perspectiveFactor
											: 1)), //scalePivot range is [0,1]
								verts[i].y,
								scalePivot.y < 0
									? verts[i].z
									: SRSUtilities.Lerp(scaleSnapshot[i].z, convergingEdgeWorld.y,
										Mathf.Clamp(
											(SRSUtilities.adjustedMousePosition.y - downPoint.y) /
											(convergingEdgeScreen.y - downPoint.y), clampMin.y, clampMax.y) *
										(scaleMode == ScaleMode.vertical ? perspectiveFactor : 1)));
							vertexDots[i].transform.position = instance.transform.TransformPoint(verts[i]);
						}

					instance.GetComponent<MeshFilter>().mesh.vertices = verts;
					instance.GetComponent<MeshFilter>().mesh.RecalculateNormals();
				},
				OnFinishDrag = (p) =>
				{
					if (IsToolsBlocked)
						return;

					scaling = false;
					movingScaleZones = new int[2] { -1, -1 };
					AddUndoStep( /*new UndoStep{meshSnapshot=verts}*/);
				},
				Draw = (p) =>
				{
					if (IsToolsBlocked)
						return;

					//Debug.Log("Scale mode Draw().");
					if (scaleMode == ScaleMode.normal)
					{
						Rect[] scaleZones = ScaleZones();
						for (int i = 0; i < scaleZones.Length; i++)
							if (!movingScaleZones.Contains(i))
								Graphics.DrawRect(scaleZones[i], Color.magenta);
						for (int i = 0; i < scaleZones.Length; i++)
							if (movingScaleZones.Contains(i))
								Graphics.DrawRect(scaleZones[i], Color.cyan);
					}
					else
						DrawPerspectiveZones(
							scaleMode == ScaleMode.horizontal ? Color.yellow : new Color(1, 0.5f, 0, 1));
				},
			}
		};

		[SerializeField] private Projection _projection;

		public GameObject vertexDotPrefab, lassoBlackoutPrefab;
		public Texture2D ellipse, activeVertex, inactiveVertex;
		public Texture2D[] backgrounds;

		public enum ToolMode
		{
			vertex = 0,
			blackout,
			scale
		};

		public Toolbar toolbar;

		public void SetDensity(int densityOption)
		{
			Debug.Log("SET DENSITY OPTION TO: " + densityOption);

			originalColumns = columns = densityOption;

			Reset(-1, true, true);
		}
		
		private static void DrawEllipseAround(Vector2 a, Vector2 b)
		{
			GUI.DrawTexture(new Rect(a.x, a.y, b.x - a.x, b.y - a.y), instance.ellipse);
		}

		private static void StartSelectionLasso(Vector3 p)
		{
			lassoPoints.Clear();
			instance.lassoLine.positionCount = 2; //instance.lassoLine.SetVertexCount(2);
			for (int i = 0; i < 2; i++)
			{
				lassoPoints.Add(CameraHelper.Camera.ScreenToWorldPoint(p) + CameraHelper.Camera.transform.forward);
				instance.lassoLine.SetPosition(i, lassoPoints[i]);
			}
		}

		private static void AddSelectionLassoPunkt(Vector3 p)
		{
			if (lassoPoints.Count < 1)
				lassoPoints.Add(CameraHelper.Camera.ScreenToWorldPoint(p) +
				                CameraHelper.Camera.transform.forward); //Repeat the first one to start wit zwei.
			lassoPoints.Add(CameraHelper.Camera.ScreenToWorldPoint(p) + CameraHelper.Camera.transform.forward);
			lines.Add(
				(GameObject.Instantiate(instance.lassoLine.gameObject, Vector3.zero, Quaternion.identity) as GameObject)
				.GetComponent<LineRenderer>());
			lines.Last().startColor = lines.Last().endColor = Color.red;
			lines.Last().material.shader = Shader.Find("Unlit/Color");
			lines.Last().material.color = new Color(1, 0, 0, 0.5f);
			Debug.Log("AddSelectionLassoPunkt(" + p + ") lines.Count: " + lines.Count + ",lassoPoints.Count: " +
			          lassoPoints.Count);
			for (int i = 0; i < 2; i++) lines.Last().SetPosition(i, lassoPoints[lassoPoints.Count - 2 + i]);
		}

		private static void UpdateSelectionLasso(Vector3 p)
		{
			lassoPoints[lassoPoints.Count - 1] = CameraHelper.Camera.ScreenToWorldPoint(p) + CameraHelper.Camera.transform.forward;
			lines.Last().SetPosition(1, lassoPoints[lassoPoints.Count - 1]);
		}

		private static void WipeLasso()
		{
			foreach (LineRenderer l in lines) UnityEngine.Object.Destroy(l.gameObject);
			lines.Clear();
			lassoPoints.Clear();
		}

		private static void LassoSelection(bool additive = false)
		{
			if (lassoPoints.Count < 3)
			{
				WipeLasso();
				return;
			}

			if (!additive) DeSelect(false);
			for (int i = 0; i < vertexDots.Count; i++)
			{
				if (!deleted.Contains(i))
				{
					int intersections = 0;
					for (int p = 0; p < lassoPoints.Count; p++)
					{
						if (SRSUtilities.Intersect(
							    CameraHelper.Camera.WorldToScreenPoint(vertexDots[i].transform.position),
							    Vector2.zero,
							    CameraHelper.Camera.WorldToScreenPoint(lassoPoints[p]),
							    CameraHelper.Camera.WorldToScreenPoint(lassoPoints[(p + 1) % lassoPoints.Count])
						    ))
							intersections++;
					}

					if (intersections % 2 > 0) SelectVertex(i);
				}
			}

			WipeLasso();
		}


		private Action _quitButtonAction;

		public void Init(Action quitButtonAction)
		{
			_quitButtonAction = quitButtonAction;

			instance = this;

			GetComponent<MeshFilter>().mesh.Clear();
			scaling = false;
			icons = Resources.LoadAll<Texture2D>("UI Icons");
			Reset();
			//originalVerts=GetComponent<MeshFilter>().mesh.vertices;
			//originalTriangles=GetComponent<MeshFilter>().mesh.triangles;
			instance.transform.localScale = new Vector3(Settings.originalScaleX, 1, 1);
			foreach (GameObject o in new GameObject[] { lassoPoint, lassoLine.gameObject })
				Graphics.SetAlpha(o.GetComponent<Renderer>().material, editingLassoAlpha);
			ResetLassoVisuals();
			//Dictionary<GUIContent,Action>[] toolMenu=new Dictionary<GUIContent,Action>[3];//Set Up Toolbar
			ToolbarMenu.Item[][] toolMenu = new ToolbarMenu.Item[4][]; //Set Up Toolbar
			for (int i = 0; i < toolMenu.Length; i++) toolMenu[i] = new ToolbarMenu.Item[3];
			string[] shapes = Enum.GetNames(typeof(Shape));
			for (int s = 0; s < shapes.Length; s++)
			{
				int _s = s; //for closure
				toolMenu[0][s] = new ToolbarMenu.Item()
				{
					//SetUI
					buttonContent = new GUIContent(icons.ByName(shapes[_s] + " selection")),
					OnSelect = () => { selectionShape = (Shape)_s; },
					shortcut = new KeyCode[][]
					{
						new KeyCode[] { KeyCode.Keypad7 }, new KeyCode[] { KeyCode.Keypad8 },
						new KeyCode[] { KeyCode.Keypad9 }
					}[s],
					info = new string[]
					{
						"Rectangular Selection\n\nDrag the mouse to select vertices within a rectilinear area.",
						"Elliptical Selection\n\nDrag the mouse to select vertices within an elliptical area.",
						"Lasso Selection\n\nClick (and drag) the mouse clockwise to create a customized lasso shape. When finished,press <ENTER> to select the vertices within."
					}[s]
				};
				toolMenu[1][s] = new ToolbarMenu.Item()
				{
					buttonContent = new GUIContent(icons.ByName(shapes[_s] + " blackout")),
					OnSelect = () => { Blackout.shape = (Shape)_s; /*Blackout.farbe=Color.black;*/ },
					shortcut = new KeyCode[][]
					{
						new KeyCode[] { KeyCode.Keypad4 }, new KeyCode[] { KeyCode.Keypad5 },
						new KeyCode[] { KeyCode.Keypad6 }
					}[s],
					info = new string[]
					{
						"Rectangular Mask\n\nDrag the mouse to create a rectangular mask.",
						"Elliptical Mask\n\nDrag the mouse to create an elliptical mask.",
						"Lasso Mask\n\nClick (and drag) the mouse to create a customized lasso shape. When finished,press <ENTER> to create the mask."
					}[s]
				};
				toolMenu[2][s] = new ToolbarMenu.Item()
				{
					buttonContent = new GUIContent(icons.ByName(shapes[_s] + " whiteout")),
					OnSelect = () => { Blackout.shape = (Shape)_s; /*Blackout.farbe=Color.white;*/ },
					shortcut = new KeyCode[][]
					{
						new KeyCode[] { KeyCode.KeypadDivide }, new KeyCode[] { KeyCode.KeypadMultiply },
						new KeyCode[] { KeyCode.KeypadMinus }
					}[s],
					info = new string[]
					{
						"Rectangular Whiteout\n\nDrag the mouse to create a rectangular highlighted area.",
						"Elliptical Whiteout\n\nDrag the mouse to create an elliptical highlighted area.",
						"Lasso Whiteout\n\nClick (and drag) the mouse to create a customized lasso-shaped highlight. When finished,press <ENTER> to create the mask."
					}[s]
				};
				toolMenu[3][s] = new ToolbarMenu.Item()
				{
					buttonContent = new GUIContent(icons.ByName(Enum.GetNames(typeof(ScaleMode))[_s] + " scale")),
					OnSelect = () => { scaleMode = (ScaleMode)_s; },
					shortcut = new KeyCode[][]
					{
						new KeyCode[] { KeyCode.Keypad1 }, new KeyCode[] { KeyCode.Keypad2 },
						new KeyCode[] { KeyCode.Keypad3 }
					}[s],
					info = new string[]
					{
						"Scale\n\nClick and drag any of the edges to scale the canvas from that side.",
						"Horizontal Scale\n\nDrag the canvas from any of the four corner areas to squeeze and stretch the top or bottom horizontally.",
						"Vertical Scale\n\nDrag the canvas from any of the four corner areas to squeeze and stretch the left or right side vertically."
					}[s]
				};
			}

			for (int i = 0; i < toolMenu.Length; i++)
			for (int j = 0; j < toolMenu[i].Length; j++)
				toolMenu[i][j].buttonContent.tooltip =
					toolMenu[i][j].info.Split(new string[] { "\n" }, StringSplitOptions.None)[0];

			ToolbarMenu.Item[][] backgroundMenu = new ToolbarMenu.Item[1][]; //Background Menu.
			backgroundMenu[0] = new ToolbarMenu.Item[backgrounds.Length];
			for (int i = 0; i < backgrounds.Length; i++)
			{
				int _i = i; //closure
				backgroundMenu[0][i] = new ToolbarMenu.Item()
				{
					buttonContent = new GUIContent(backgrounds[i],
						SRSUtilities.CapFirsts(Regex.Replace(backgrounds[i].name, "_", " "))),
					OnSelect = () => GetComponent<Renderer>().material.mainTexture = backgrounds[background = _i]
				};
			}

			//backgroundMenu[0]=content;
			ToolbarMenu.Item[][] actionMenu = new ToolbarMenu.Item[][]
			{
				new ToolbarMenu.Item[]
				{
					//File Menu
					new ToolbarMenu.Item() { buttonContent = new GUIContent("File", "File Operations") },
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Save File"), "Save Configuration"), OnSelect =
							() =>
							{
								mode = Mode.save;
								UIHelper.ResetWindowPosition();
								saveAsDefault = -1;
								SRSUtilities.guiMatrixNormalized = false;
							},
						shortcut = new KeyCode[] { KeyCode.S }
					},
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Open File"), "Load Configuration"), OnSelect =
							() =>
							{
								mode = Mode.load;
								UIHelper.ResetWindowPosition();
								SRSUtilities.guiMatrixNormalized = false;
							},
						shortcut = new KeyCode[] { KeyCode.L }
					},
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Reset"), "Restart the Editor"), OnSelect = () =>
						{
							Reset(-1, true, true);
							Restart();
						}
					},
					//{new GUIContent("Information"),()=>infoOn=!infoOn},
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Quit"), "Quit to Menu"),
						OnSelect = SaveAndQuitToMenu
					}
				},
				new ToolbarMenu.Item[]
				{
					//Edit Menu
					new ToolbarMenu.Item() { buttonContent = new GUIContent("Edit", "Edit Operations") },
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Undo"), "Undo Last Action"),
						OnSelect = () => Undo(-1), shortcut = new KeyCode[] { KeyCode.Z }, disabled = true
					},
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Redo"), "Redo Undone Action"),
						OnSelect = () => Undo(1), shortcut = new KeyCode[] { KeyCode.D }, disabled = true
					},
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Select All"), "Select All"), OnSelect = SelectAll,
						shortcut = new KeyCode[] { KeyCode.A }
					}
				},
				new ToolbarMenu.Item[]
				{
					//View Menu
					new ToolbarMenu.Item() { buttonContent = new GUIContent("View", "View Options") },
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("X Mirror"), "Mirror Horizontally"),
						OnSelect = () => ToggleMirror(0), shortcut = new KeyCode[] { KeyCode.X }
					},
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Y Mirror"), "Mirror Vertically"),
						OnSelect = () => ToggleMirror(1), shortcut = new KeyCode[] { KeyCode.Y }
					},
					new ToolbarMenu.Item()
					{
						buttonContent = new GUIContent(icons.ByName("Circle Mode"), "Edit in a Circle"),
						OnSelect = () => circle = !circle, shortcut = new KeyCode[] { KeyCode.C }
					}
				}
			};
			Debug.Log("toolMenu.Length: " + toolMenu.Length + ",actionMenu.Length: " + actionMenu.Length);
			toolbar = GetComponent<Toolbar>();
			toolbar.SetMenu(new ToolbarMenu[]
			{
				new ToolbarMenu(toolMenu, true, (s) =>
				{
					toolMode = (ToolMode)s;
					ResetTools();
					Debug.Log("Changing toolmenu #0 to: " + s + ". selectedCategory previously: " +
					          toolbar.menus[0].selectedCategory);
					toolbar.menus[2].ItemByTooltip("Mirror Horizontally").disabled =
						toolbar.menus[2].ItemByTooltip("Mirror Vertically").disabled =
							s != 0 && s !=
							3; //toolbar.menus[0].selectedCategory!=0&&toolbar.menus[0].selectedCategory!=3;
					toolbar.menus[2].ItemByTooltip("Edit in a Circle").disabled =
						toolbar.menus[2].ItemByTooltip("Select All").disabled =
							s != 0; //toolbar.menus[0].selectedCategory==0;//Mirror and circle modes only affect vertex displacement; they therefore only make sense in that category.
					Debug.Log("Mirror disabled: " + toolbar.menus[2].ItemByTooltip("Mirror Horizontally").disabled);
				}, null, default(Vector2), 0) { cycleShortcut = new KeyCode[] { KeyCode.Keypad0 } },
				new ToolbarMenu(backgroundMenu, true) { selectedCategoryFarbe = Color.white },
				new ToolbarMenu(actionMenu, false)
			});
		}

		public void SetContourBackground(int background)
		{
			GetComponent<Renderer>().material.mainTexture = backgrounds[ContourEditor.background = background];
		}

		private static void ResetTools()
		{
			//WipeLasso();
			WipeIntermittents();
			Blackout.moving = -1;
			dragging = false;
		}

		//public static void AddUndoStep(UndoStep step=default(UndoStep),bool resetCurrentStep=true){
		public static void AddUndoStep(bool resetCurrentStep = true)
		{
			Debug.Log("Projection.AddUndoStep(" + resetCurrentStep + ") ANFANG, undo: " + undo + "/" + undos.Count);
			while (undo > -1 && undo < undos.Count - 1) undos.RemoveAt(undos.Count - 1);
			UndoStep us = new UndoStep();
			us.meshSnapshot = instance.GetComponent<MeshFilter>().mesh.vertices;
			us.selectedVerts = selectedVertices.ToArray();
			us.deletedVerts = deleted.ToArray();
			Debug.Log("Adding " + deleted.Count + " deleted.");
			us.triangleSnapshot = instance.GetComponent<MeshFilter>().mesh.triangles;
			us.blackouts = blackouts.ToArray();
			us.blackoutLassoMeshes = new Vector3[us.blackouts.Length][];
			us.blackoutTriangleSnapshots = new int[us.blackouts.Length][];
			for (int i = 0; i < blackouts.Count; i++)
				if (blackouts[i].lassoObject != null)
				{
					us.blackoutLassoMeshes[i] = blackouts[i].lassoObject.GetComponent<MeshFilter>().mesh.vertices;
					us.blackoutTriangleSnapshots[i] =
						blackouts[i].lassoObject.GetComponent<MeshFilter>().mesh.triangles;
				}

			undos.Add(us);
			if (resetCurrentStep) undo = undos.Count - 1;
			UpdateUndoMenu();
			Debug.Log("Projection.AddUndoStep(" + resetCurrentStep + ") ANSCHLUSS, undo: " + undo + "/" + undos.Count +
			          ", step:\n" + us.Stringify());
		}

		private static void UpdateUndoMenu()
		{
			instance.toolbar.menus[2].ItemByTooltip("Redo Undone Action").disabled = undo >= undos.Count - 1;
			instance.toolbar.menus[2].ItemByTooltip("Undo Last Action").disabled = undo < 1;
		}

		public void Undo(int dir)
		{
			if (undo + dir < 0 || undo + dir > undos.Count - 1 || undos.Count < 1)
			{
				Debug.LogWarning("ContourEditor.Undo(" + dir + ") returning, undo jetzt: " + undo + "/" + undos.Count);
				return;
			}

			undo += dir;
			UpdateUndoMenu();
			Debug.LogWarning("ContourEditor.Undo(" + dir + "), undo jetzt: " + undo + "/" + undos.Count + ", vor: " +
			                 (undo - dir) + " Treppe:\n" + undos[undo].Stringify() + ", deleted.Count: " +
			                 deleted.Count);
			//Reset();
			WipeIntermittents();
			WipeBlackouts();
			deleted = undos[undo].deletedVerts.ToList(); //Once here for ReconstructScreen's proper count
			ReconstructScreen(0, false, null, false);
			Mesh mesh = instance.GetComponent<MeshFilter>().mesh;
			mesh.vertices = undos[undo].meshSnapshot;
			mesh.triangles = undos[undo].triangleSnapshot;
			mesh.RecalculateNormals();
			mesh.RecalculateBounds();
			GroupSelect(undos[undo].selectedVerts);
			Debug.LogWarning("VOR deleted.Count: " + deleted.Count + ", undos[undo].deletedVerts.Count: " +
			                 (undos[undo].deletedVerts.Length));
			deleted = undos[undo].deletedVerts.ToList(); //And again here since ReconstructScreen reset it.
			//Debug.LogError("NACH deleted.Count: "+deleted.Count+", undos[undo].deletedVerts.Count: "+(undos[undo].deletedVerts.Length)+", vertexDots.Count: "+vertexDots.Count+", deleted: "+undos[undo].deletedVerts.Stringify());
			for (int i = 0; i < vertexDots.Count; i++)
				if (deleted.Contains(i))
				{
					vertexDots[i].gameObject.SetActive(false);
					Debug.LogWarning("Deactivating " + i + ".");
				}
				else vertexDots[i].transform.position = instance.transform.TransformPoint(mesh.vertices[i]);

			blackouts = undos[undo].blackouts.ToList();
			for (int i = 0; i < undos[undo].blackouts.Length; i++)
				if (undos[undo].blackoutLassoMeshes[i] != null)
				{
					blackouts[i].lassoObject = CreateLassoObject(blackouts[i].farbe);
					mesh = blackouts[i].lassoObject.GetComponent<MeshFilter>().mesh;
					mesh.vertices = undos[undo].blackoutLassoMeshes[i];
					mesh.triangles = undos[undo].blackoutTriangleSnapshots[i];
					;
					mesh.RecalculateNormals();
					mesh.RecalculateBounds();
					blackouts[i].lassoObject.GetComponent<MeshRenderer>().material.color = blackouts[i].lassoObject
						.GetComponent<MeshRenderer>().material.color.WithAlpha(1);
				}
		}

		private static void Undelete(int[] restoredTriangles)
		{
			Debug.LogWarning("ContourEditor.Undelete(" + restoredTriangles.Length + ")");
			instance.GetComponent<MeshFilter>().mesh.triangles = restoredTriangles;
			restoredTriangles.ToList().ForEach((v) => vertexDots[v].gameObject.SetActive(true));
			deleted.RemoveSet(restoredTriangles);
		}

		public static void WipeBlackouts()
		{
			Debug.Log("ContourEditor.WipeBlackouts(), gesammt: " + blackouts.Count);
			Blackout.DeSelect();
			for (int i = blackouts.Count - 1; i >= 0; i--) Blackout.Delete(i, false);
			blackouts.Clear();

			foreach (GameObject bo in GameObject.FindGameObjectsWithTag("Blackout"))
			{
				Debug.Log("Stray blackout object found: " + bo.name);
				Destroy(bo);
			}
		}

		public void Restart(int screenNum = -1)
		{
			//Start Anew.
			UIHelper.ResetWindowPosition();
			originalColumns = -1;
			//AddUndoStep();
		}

		public void Reset(int screenNum = -1, bool wipeBlackouts = true, bool addUndo = false)
		{
			//Start Anew.
			Debug.Log("Projection.Reset(" + screenNum + ")");
			
			if (instance == null)
				Init(null);

			Mesh mesh = GetComponent<MeshFilter>().mesh;
			//mesh.vertices=originalVerts;
			//mesh.triangles=originalTriangles;
			//mesh.RecalculateNormals();
			//mesh.RecalculateBounds();
			if (toolbar != null && toolbar.menus != null && toolbar.menus.Length > 0)
				toolbar.menus[0].SelectItem(0, 0);
			columns = originalColumns;
			deleted.Clear();
			ReconstructScreen(0, false);
			if (wipeBlackouts) WipeBlackouts();
			WipeIntermittents();
			transform.position = Vector3.zero;
			downPoint = -Vector2.one;
			//Debug.LogError(addUndo+", "+(addUndo?"add":"clear")+"ing.");
			if (addUndo) AddUndoStep();
			else undos.Clear();
		}

		public void ReconstructScreen(int delta = 0, bool setUndo = false, GameObject screen = null,
			bool omitDeleted = true)
		{
			//Delta -1 to halve,0 to maintain,1 to double.
			screen = screen ?? gameObject;
			DeSelect(setUndo && delta != 0);
			MeshFilter filter = screen.GetComponent<MeshFilter>();
			Mesh mesh = filter.mesh;
			Vector3[] oldVerts = mesh.vertices;
			mesh.Clear();
			float length = 10, width = 10;
			int oldColumns = columns;
			//Debug.Log("Projection.ReconstructScreen("+delta+"),columns vor: "+oldColumns+",delta: "+delta+",jetzt "+(delta>1?delta:Mathf.Max(2,Mathf.Min(41,(int)((columns-1)*Mathf.Pow(2,delta)+1)))));
			columns = delta > 1
				? delta
				: Mathf.Max(minDensity, Mathf.Min(maxDensity, (int)((columns - 1) * Mathf.Pow(2, delta) + 1)));
			Debug.Log("Projection.ReconstructScreen(" + delta + "), columns vor: " + oldColumns + ",delta: " + delta +
			          ", jetzt: " + columns);

			#region Vertices

			Vector3[] vertices = new Vector3[columns * columns];
			vertexDots.ForEach((v) => UnityEngine.Object.Destroy(v.gameObject));
			vertexDots.Clear();
			List<int> toDelete = new List<int>();
			for (int z = 0; z < columns; z++)
			for (int x = 0; x < columns; x++)
				if (!omitDeleted || !deleted.Contains(z * columns + x))
				{
					if (delta != 0)
					{
						int ox1 = x * (oldColumns - 1) / (columns - 1),
							ox2 = Mathf.Min(oldColumns - 1, ox1 + 1),
							oz1 = z * (oldColumns - 1) / (columns - 1),
							oz2 = Mathf.Min(oldColumns - 1, oz1 + 1);
						float total =
							(float)(columns - 1) / (float)(oldColumns - 1); //(vertices.Length-1)/(oldVerts.Length-1);
						vertices[x + z * columns] = Vector3.Lerp(
							Vector3.Lerp(oldVerts[ox1 + oz1 * oldColumns], oldVerts[ox2 + oz1 * oldColumns],
								x % total / total),
							Vector3.Lerp(oldVerts[ox1 + oz2 * oldColumns], oldVerts[ox2 + oz2 * oldColumns],
								x % total / total), z % total / total);
					}
					else
						vertices[x + z * columns] = new Vector3(((float)x / (columns - 1) - .5f) * width, 0f,
							((float)z / (columns - 1) - .5f) * length);

					if (_projection.IsEditing)
					{
						var vertex = Instantiate(instance.vertexDotPrefab,
								instance.transform.TransformPoint(vertices[x + z * columns]),
								Quaternion.Euler(90, 0, 0))
							.GetComponent<Vertex>();
						vertex.Init(_projection);

						vertexDots.Add(vertex);
						vertexDots.Last().transform.parent = instance.transform;
						vertexDots.Last().name = "Vertex " + (z * columns + x);
					}

					if (delta == 0 && deleted.Contains(x + z * columns)) toDelete.Add(x + z * columns);
				}

			#endregion

			#region Normals

			Vector3[] normals = new Vector3[vertices.Length];
			for (int n = 0; n < normals.Length; n++) normals[n] = Vector3.up;

			#endregion

			#region UVs

			Vector2[] uvs = new Vector2[vertices.Length];
			for (int v = 0; v < columns; v++)
			for (int u = 0; u < columns; u++)
				uvs[u + v * columns] = new Vector2((float)u / (columns - 1), (float)v / (columns - 1));

			#endregion

			#region Triangles

			int nbFaces = (columns - 1) * (columns - 1);
			int[] triangles = new int[nbFaces * 6];
			//		int t=0;
			for (int face = 0; face < nbFaces; face++)
				AddFace(triangles, face); //Retrieve lower left corner from face ind

			#endregion

			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			;
			//		columns=density;
			deleted.Clear();
			Delete(toDelete.ToArray(), null, false);
			if (setUndo && delta != 0) AddUndoStep( /*new UndoStep{ dir=delta }*/);
		}

		private static void AddFace(int[] triangles, int face)
		{
			int i = face % (columns - 1) + face / (columns - 1) * columns, t = face * 6;
			triangles[t++] = i + columns;
			triangles[t++] = i + 1;
			triangles[t++] = i;
			triangles[t++] = i + columns;
			triangles[t++] = i + columns + 1;
			triangles[t++] = i + 1;
		}

		private static bool SideX(int v)
		{
			return v % columns > (columns / 2 + 1);
		}

		private static bool SideY(int v)
		{
			return v / columns > (columns / 2 + 1);
		}

		public static bool[] MirroredOpposite(int vi)
		{
			return new bool[]
			{
				mirror[0] && SideX(vertexDots.IndexOf(Vertex.dragging)) != SideX(vi),
				mirror[1] && SideY(vertexDots.IndexOf(Vertex.dragging)) != SideY(vi)
			};
		}

		public void MoveSelectedVerticesBy(Vector3 differential, bool flipY = false)
		{
			if (IsToolsBlocked)
				return;

			if (selectedVertices.Count < 1)
			{
				Debug.LogError("Projection.MoveSelectedVerticesBy called with no selected vertex.");
				return;
			}

			Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
			int[] applicableVertices = ApplicableVertices();
			foreach (int v in applicableVertices)
			{
				bool[] mirroredOpposite = MirroredOpposite(v);
				differential.x =
					Mathf.Max(Mathf.Min(differential.x, rawSize.x - vertices[v].x * (mirroredOpposite[0] ? -1 : 1)),
						-rawSize.x - vertices[v].x * (mirroredOpposite[0] ? -1 : 1));
				differential.z = Mathf.Max(
					Mathf.Min(differential.z,
						transform.InverseTransformPoint(Projection.originalExtents).z -
						vertices[v].z * (mirroredOpposite[1] ? -1 : 1)),
					-transform.InverseTransformPoint(Projection.originalExtents).z -
					vertices[v].z * (mirroredOpposite[1] ? -1 : 1));
			}

			applicableVertices.ToList().ForEach((vi) =>
			{
				bool[] mirroredOpposite = MirroredOpposite(vi);
				//Vector3 diff=transform.InverseTransformPoint(new Vector3(differential.x*(mirroredOpposite[0]?1:-1),differential.y,differential.z*(mirroredOpposite[1]?1:-1)));
				Vector3 diff = transform.InverseTransformPoint(new Vector3(
					differential.x * (mirroredOpposite[0] ? -1 : 1), differential.y,
					differential.z * (mirroredOpposite[1] ? -1 : 1)));
				//vertices[vi]=new Vector3(vertices[vi].x+(mirror[0]?diff.x:0),Vertex.y,vertices[vi].z+(mirror[1]?diff.z*(flipY?-1:1):0));
				vertices[vi] = new Vector3(vertices[vi].x + diff.x, Vertex.y,
					vertices[vi].z + diff.z * (flipY ? -1 : 1));
				//Debug.Log("verts["+vi+"] vor: "+vertices[vi].ToPreciseString()+",differential: "+differential.ToPreciseString()+",diff: "+diff.ToPreciseString()+",Vertex.y: "+Vertex.y+",transformed: "+transform.InverseTransformPoint(differential).ToPreciseString());
			});

			selectedVertices.ForEach((i) =>
			{
				bool[] mirroredOpposite = MirroredOpposite(i);
				//bool[] mirroredOpposite=new bool[]{false,false};
				vertexDots[i].transform.position += new Vector3(differential.x * (mirroredOpposite[0] ? -1 : 1),
					differential.y, differential.z * (mirroredOpposite[1] ? -1 : 1));
			});
			GetComponent<MeshFilter>().mesh.vertices = vertices;
			GetComponent<MeshFilter>().mesh.RecalculateNormals();
			GetComponent<MeshFilter>().mesh.RecalculateBounds();
			var o_920_9_636284964395300148 = GetComponent<MeshFilter>().mesh;
			//lastMove=differential;
		}

		private static bool WithinBounds(int v)
		{
			//        Debug.Log("Projection.WithinBounds("+v+"): "+(v>-1&&v<instance.GetComponent<MeshFilter>().mesh.vertices.Length));
			return v > -1 && v < instance.GetComponent<MeshFilter>().mesh.vertices.Length;
		}

		private static int[] ApplicableVertices()
		{
			if (selectedVertices.Count < 1) return null;
			List<int> verts = new List<int>();
			foreach (int vert in selectedVertices)
			{
				List<int> neue = new List<int>();
				neue.Add(vert);
				if (circle)
				{
					int[] arc = SRSUtilities.ArcPattern(SRSUtilities.RadiusOf(Mathf.Abs(columns / 2 - vert % columns),
						Mathf.Abs(columns / 2 - vert / columns)));
					int v;
					for (int i = 0; i < arc.Length; i++)
					{
						v = columns / 2 * (columns + 1) + i + arc[i] * columns;
						if (WithinBounds(v)) neue.AddUnique(v);
						else Debug.LogWarning("Throwing out vertex " + v + " from the circle algorithm.");
					}

					for (int y = arc[arc.Length - 1]; y >= 0; y--)
						if (WithinBounds(v = columns / 2 * (columns + 1) + (arc.Length - 1) + y * columns))
							neue.AddUnique(v); //Fill in the gap on the x axis.
				}

				if (mirror[0] || circle)
					foreach (int i in neue.ToArray())
						neue.AddUnique(XMirror(i));
				if (mirror[1] || circle)
					foreach (int i in neue.ToArray())
						neue.AddUnique(YMirror(i));
				verts.AddRangeUnique(neue);
			}

			return verts.ToArray();
		}

		private static int XMirror(int i)
		{
			return (i / columns + 1) * columns - i % columns - 1;
		}

		private static int YMirror(int i)
		{
			return (instance.GetComponent<MeshFilter>().mesh.vertices.Length / columns - i / columns - 1) * columns +
			       i % columns;
		}

		private static void ApplyToSelected(Action<int> a, List<int> doneVerts = null)
		{
			Debug.Log("Projection.ApplyToSelected(" + a + ") selectedVertices: " + selectedVertices.Stringify());
			if (selectedVertices.Count < 1 || a == null) return;
			if (doneVerts == null) doneVerts = new List<int>();
			foreach (int vi in selectedVertices)
				if (!doneVerts.Contains(vi))
				{
					//			Debug.Log("i="+i+",p="+p+",d="+d+",v+i*d*p="+(v+i*d*p)+",verts["+vi+"]: "+instance.GetComponent<MeshFilter>().mesh.vertices[vi]+","+vi+">=0: "+(vi>=0)+",withinBounds("+vi+"): "+WithinBounds(vi)+",p==columns+1: "+(p==columns+1)+",vi/columns==selectedVertex/columns: "+(vi/columns==v/columns)+"("+vi+"/"+columns+": "+(vi/columns)+","+v+"/"+columns+":"+(v/columns)+")");
					a(vi);
					doneVerts.Add(vi);
				} //else Debug.LogWarning("i="+i+",p="+p+",d="+d+",v+i*d*p="+(selectedVertex+i*d*p)+",verts["+vi+"]: "+instance.GetComponent<MeshFilter>().mesh.vertices[vi]+","+vi+">=0: "+(vi>=0)+","+vi+"<"+instance.GetComponent<MeshFilter>().mesh.vertices.Length+": "+(vi<instance.GetComponent<MeshFilter>().mesh.vertices.Length)+",p==columns+1: "+(p==columns+1)+",vi/columns==selectedVertex/columns: "+(vi/columns==selectedVertex/columns)+"("+vi+"/"+columns+": "+(vi/columns)+","+selectedVertex+"/"+columns+":"+(selectedVertex/columns)+")");
		}

		private static bool EdgeVert(int v)
		{
			return v / columns == 0 || v / columns == columns - 1 || v % columns == 0 || v % columns == columns - 1;
		}

		private static bool CornerVert(int v)
		{
			return (v / columns == 0 || v / columns == columns - 1) && (v % columns == 0 || v % columns == columns - 1);
		}

		private static void Delete(int[] deletees, GameObject screen = null, bool addUndo = true)
		{
			Debug.Log("ContourEditor.Delete(" + deletees.Stringify() + "), children: " +
			          instance.transform.childCount); //Deleted triangles containing vertex "+selectedVertex+".");
			screen = screen ?? instance.gameObject;
			int[] oldTriangles = screen.GetComponent<MeshFilter>().mesh.triangles,
				newTriangles = new int[oldTriangles.Length - 3 * deletees.Length];
			for (int o = 0, n = 0; o < oldTriangles.Length; o += 3)
			{
				bool omit = false;
				for (int j = 0; j < 3; j++)
				{
					if (omit = (deletees.Contains(oldTriangles[o + j]) ||
					            (oldTriangles[o] < columns * (columns - 1) && j == 2 &&
					             o / 3 % 2 == 0 && //Adjacent triangles
					             deletees.Contains(oldTriangles[o + j] + columns + 1)) ||
					            (oldTriangles[o] > columns && j == 2 && o / 3 % 2 == 1 &&
					             deletees.Contains(oldTriangles[o + j] - 1))))
						break;
				}

				//			Debug.Log("n: "+n+",o: "+o+",j: "+j+",o+j: "+(o+j)+",newTriangles.Length: "+newTriangles.Length+",oldTriangles.Length: "+oldTriangles.Length);
				//if(omit){}// Debug.Log("omitting: oldTriangles["+o+"]: "+oldTriangles[o]);
				/*else*/
				if (!omit)
					for (int j = 0; j < 3; j++)
					{
						/*Debug.Log("o: "+o+",n: "+n+",j: "+j+",newTriangles.Length: "+newTriangles.Length+",oldTriangles.Length: "+oldTriangles.Length);*/
						newTriangles[n++] = oldTriangles[o + j];
					}
			}

			screen.GetComponent<MeshFilter>().mesh.triangles = newTriangles;
			deleted.AddRangeUnique(deletees.ToList());
			for (int i = 0; i < vertexDots.Count; i++)
				if (!newTriangles.Contains(i))
					vertexDots[i].gameObject.SetActive(false);
			DeSelect(false);
			if (addUndo)
				AddUndoStep(
 /*new UndoStep{deleteTrianglePoints=oldTriangles,selectVerts=selectedVertices.ToArray()}*/);
		}

		public static void DeSelect(bool undoStep = true)
		{

			if (IsToolsBlocked)
				return;

			Debug.Log("ContourEditor.DeSelect(" + undoStep + ")");
			selectedVertices.ForEach((i) => vertexDots[i].Select(false));
			Vector3[] verts = instance.GetComponent<MeshFilter>().mesh.vertices;
			//selectedVertices.ForEach((v)=>verts[v][1]=0);
			for (int v = 0; v < verts.Length; v++) verts[v][1] = 0;
			instance.GetComponent<MeshFilter>().mesh.vertices = verts;
			selectedVertices.Clear();
			//lastMove=Vector3.zero;
			if (undoStep /*&&selectedVertices.Count>0*/)
				AddUndoStep(
 /*new UndoStep{ selectVerts=selectedVertices.Count>0 ? selectedVertices.ToArray() : new int[]{ -1 } }*/);
		}

		public static Vector2
			downPoint = -Vector2.one; //Lesson learned: use one variable for one purpose. Optimize later _IF NEED BE_.

		private static int[] movingScaleZones = new int[2] { -1, -1 };

		private void MouseDown()
		{
			Debug.Log("Projection.MouseDown()");
			if (Draggable2D.draggingAnything)
				return; //See that the script that inherits from Draggable2D is called prior to this in the Update queue.
			downPoint = (Vector2)SRSUtilities.adjustedMousePosition;
			if (currentTool.OnMouseDown != null) currentTool.OnMouseDown(downPoint);
			Debug.Log("Projection.MouseDown() downPoint: " + downPoint + ",SRSUtilities.adjustedMousePosition: " +
			          SRSUtilities.adjustedMousePosition + ",flipped: " + SRSUtilities.adjustedFlipped + ",adjusted: " +
			          SRSUtilities.adjustedMousePosition + ",adjustedFlipped: " + SRSUtilities.adjustedFlipped +
			          ",screen dimensions: " + Screen.width + "," + Screen.height);
		}

		private static Blackout lassoBlackout = null;
		public LineRenderer lassoLine = new LineRenderer();
		public GameObject lassoPoint;

		private static void ResetLassoVisuals()
		{
			Debug.Log("Projection.ResetLassoVisuals()");
			instance.lassoPoint.transform.position = Vector3.up * 16;
			for (int i = 0; i < 2; i++) instance.lassoLine.SetPosition(i, Vector3.up * 16);
		}

		private static void AddLassoPoint(Vector2 p, Color c)
		{
			Debug.Log("Projection.AddLassoPoint(" + p + ")");
			if (lassoBlackout == null)
				blackouts.Add(lassoBlackout = new Blackout() { farbe = c.WithAlpha(editingLassoAlpha) });
			lassoPoints.Add(CameraHelper.Camera.ScreenToWorldPoint(p) + CameraHelper.Camera.transform.forward);
			UpdateLasso();
			Debug.Log("Projection.AddLassoPoint(" + p + ") anschluß. lassoBlackout: " + lassoBlackout +
			          ",lassoPoints.Count: " + lassoPoints.Count);
		}

		private static void MoveLastLassoPoint(Vector2 p)
		{
			Debug.Log("Projection.MoveLassoPoint(" + p + ")");
			if (lassoBlackout == null || lassoPoints.Count < 1)
			{
				Debug.LogError("MoveLastLassoPoint called with no lassoBlackout. lassoBlackout: " + lassoBlackout +
				               ",lassoPoints.Count: " + lassoPoints.Count);
				return;
			}

			lassoPoints[lassoPoints.Count - 1] = CameraHelper.Camera.ScreenToWorldPoint(p) + CameraHelper.Camera.transform.forward;
			UpdateLasso();
		}

		private static void UpdateLasso()
		{
			Debug.Log("Projection.UpdateLasso()");
			ResetLassoVisuals();
			if (lassoPoints.Count > 2) BuildLassoMesh(lassoBlackout, lassoPoints.ToArray());
			else if (lassoPoints.Count == 2)
				for (int i = 0; i < 2; i++)
					instance.lassoLine.SetPosition(i, lassoPoints[i]);
			else instance.lassoPoint.transform.position = lassoPoints[0];
		}

		private static GameObject CreateLassoObject(Color c = default(Color))
		{
			const string lassoGameObjectName = "Lasso Object";
			const string lassoObjectTag = "Blackout";
			const string guiTextShader = "GUI/Text Shader";

			var lassoObject = new GameObject(lassoGameObjectName, typeof(MeshFilter), typeof(MeshRenderer),
				typeof(MeshCollider));

			lassoObject.GetComponent<MeshRenderer>().material = instance.lassoPoint.GetComponent<MeshRenderer>().material;
			lassoObject.GetComponent<MeshCollider>().convex = true;
			lassoObject.layer = LayerMask.NameToLayer(lassoObjectTag);
			lassoObject.tag = lassoObjectTag;
			lassoObject.GetComponent<MeshRenderer>().material.shader = Shader.Find(guiTextShader);
			lassoObject.GetComponent<MeshRenderer>().material.color = c == default ? Color.black : c;

			_lassoObjects.Add(lassoObject);

			return lassoObject;
		}

		public static void ShowLassoObjects(bool isShow)
		{
			foreach (var lassoObject in _lassoObjects)
				lassoObject.SetActive(isShow);
		}

		private static void BuildLassoMesh(Blackout blackout, Vector3[] lassoPoints)
		{
			Debug.Log("BuildLassoMesh(" + blackout + "," + lassoPoints.Length + "), blackout farbe: " + blackout.farbe);
			//MeshFilter filter=(blackout.lassoObject=blackout.lassoObject??new GameObject("lassoObject",typeof(MeshFilter),typeof(MeshRenderer))).GetComponent<MeshFilter>();
			if (blackout.lassoObject == null)
				blackout.lassoObject = CreateLassoObject(blackout.farbe /*.WithAlpha(editingLassoAlpha)*/);
			Vector3 durchschnitt = Vector3.zero;
			for (int v = 0; v < lassoPoints.Length; v++) durchschnitt += lassoPoints[v];
			//lassoPoints.Insert(0,durchschnitt/lassoPoints.Count);
			//Debug.LogWarning(durchschnitt+"/"+lassoPoints.Length+"="+(durchschnitt/lassoPoints.Length)+",lassoPoints[0]: "+lassoPoints[0]);
			MeshFilter filter = blackout.lassoObject.GetComponent<MeshFilter>();
			Mesh mesh = filter.mesh;
			mesh.Clear();
			Vector3[] verts = new Vector3[lassoPoints.Length + 1];
			//verts[0]=durchschnitt/lassoPoints.Count;
			verts[0] = SRSUtilities.Midpoint(lassoPoints.ToArray());
			for (int i = 0; i < lassoPoints.Length; i++) verts[i + 1] = lassoPoints[i];
			mesh.vertices = verts;
			//mesh.vertices=lassoPoints.ToArray();
			//lassoPoints.Clear();
			Debug.Log("lassoPoints.Count: " + lassoPoints.Length + ",mesh.vertices.Length: " + mesh.vertices.Length +
			          ",0: " + mesh.vertices[0] + ",1: " + mesh.vertices[1] + ",2: " + mesh.vertices[2] + ",3: " +
			          mesh.vertices[3]);
			Vector3[] normals = new Vector3[mesh.vertices.Length];
			for (int n = 0; n < normals.Length; n++) normals[n] = Vector3.back;
			Vector2[] uvs = new Vector2[mesh.vertices.Length];
			for (int u = 0; u < uvs.Length; u++) uvs[u] = Vector2.zero;
			int[] triangles = new int[mesh.vertices.Length * 3];
			for (int v = 2, t = 0; v <= mesh.vertices.Length; v++)
			{
				triangles[t++] = 0;
				triangles[t++] = v - 1;
				triangles[t++] = v % mesh.vertices.Length > 0 ? v % mesh.vertices.Length : 1;
			}

			mesh.normals = normals;
			mesh.uv = uvs;
			mesh.triangles = triangles;
			mesh.RecalculateBounds();
			mesh.RecalculateNormals();
			
			blackout.lassoObject.GetComponent<MeshCollider>().sharedMesh = mesh;
			blackout.lassoObject.GetComponent<MeshRenderer>().material.color = blackout.farbe;
			//blackout.lassoObject.GetComponent<MeshRenderer>().material.color=Color.white;
		}

		private static float groupSelectThreshold = 4;
		private static List<int> deleted = new List<int>();

		private static void GroupDeSelect(int[] verts)
		{
			Debug.Log("Projection.GroupSelect(" + verts.Stringify() + ")");
			for (int i = 0; i < verts.Length; i++)
				if (verts[i] > -1)
					DeselectVertex(verts[i]);
		}

		private static void GroupSelect(int[] newVerts, bool deSelect = true)
		{
			Debug.Log("Projection.GroupSelect(" + newVerts.Stringify() + ")");
			if (deSelect) DeSelect(false);
			for (int i = 0; i < newVerts.Length; i++)
				if (newVerts[i] > -1)
					SelectVertex(newVerts[i]);
		}

		private static void GroupSelect(Rect r)
		{
			Debug.Log("Projection.GroupSelect(" + r + ")");
			if (!Input.GetKey(addSelectKey)) DeSelect(false);
			switch (selectionShape)
			{
				case Shape.rect:
					r.x -= 4; //Widen to capture the screen edge vertices.
					r.y -= 4;
					r.width += 8;
					r.height += 8;
					for (int i = 0; i < vertexDots.Count; i++)
						if (r.Contains(CameraHelper.Camera.WorldToScreenPoint(vertexDots[i].transform.position)) &&
						    !deleted.Contains(i))
							SelectVertex(i);
					break;
				case Shape.ellipse:
					for (int i = 0; i < vertexDots.Count; i++)
						if (r.EllipseContains(CameraHelper.Camera.WorldToScreenPoint(vertexDots[i].transform.position)) &&
						    !deleted.Contains(i))
							SelectVertex(i);
					break;
				default:
					Debug.Log("Unbekannst Shape: " + selectionShape);
					break;
			}

			AddUndoStep();
		}

		private static bool dragging
		{
			get
			{
				return downPoint != -Vector2.one && Vector2.Distance(downPoint, SRSUtilities.adjustedMousePosition) >
					groupSelectThreshold;
			}
			set { downPoint = value ? SRSUtilities.adjustedMousePosition : -Vector2.one; }
		}

		private static Tool currentTool
		{
			get { return toolBehaviour[(int)toolMode]; }
		}

		public void MouseUp()
		{
			Debug.Log("Projection.MouseUp(); downPoint: " + downPoint + ",beyond threshold: " +
			          (Vector2.Distance(downPoint, SRSUtilities.adjustedMousePosition) > groupSelectThreshold) +
			          ",distance: " +
			          Vector2.Distance(downPoint, SRSUtilities.adjustedMousePosition) + ",threshold: " +
			          groupSelectThreshold + ",adjustedMousePosition: " +
			          SRSUtilities.adjustedMousePosition /*+",didMouseDownSpecial: "+didMouseDownSpecial*/);
			if (dragging)
			{
				if (currentTool.OnFinishDrag != null) currentTool.OnFinishDrag(SRSUtilities.adjustedMousePosition);
			}
			else if (!Draggable2D.mouseOverAny)
			{
				//single-clicking
				Debug.Log("Single-clicking. downpoint: " + downPoint + ",adjustedMousePosition: " +
				          SRSUtilities.adjustedMousePosition + ",distance: " +
				          Vector2.Distance(downPoint, SRSUtilities.adjustedMousePosition) + ",groupSelectThreshold: " +
				          groupSelectThreshold + ",Projection layer: " + LayerMask.NameToLayer("Projection"));
				if (currentTool.OnSingleClick != null) currentTool.OnSingleClick(SRSUtilities.adjustedMousePosition);
			}

			downPoint = -Vector2.one;
			//		StartCoroutine(SRSUtilities.CallInFrames(()=>{if(vertexDot!=null)vertexDot.SendMessage("OnMouseDown");},1));//For some reason,every time this is called AFTER THE FIRST in a launch will fail to be hit by the ray cast on the dot quad,without delaying it a frame.
		}

		private static int _locus = 0, columns = 21, originalColumns = 21, minDensity = 1, maxDensity = 82; //41

		private static int locus
		{
			set
			{
				_locus = Mathf.Max(0, Mathf.Min(value, 10));
				//			if(selectedVertex>-1)instance.SelectVertex(selectedVertex);
			}
			get { return _locus; }
		}

		public static float selectedZMargin = 0.5f;
		private static bool[] mirror = new bool[] { false, false };

		private static bool circle = false;

		//private static Vector3 initialPosition;
		public static bool VertexIsSelected(Vertex v)
		{
			return selectedVertices.Contains(vertexDots.IndexOf(v));
		}

		public static void SelectVertex(Vertex v)
		{
			if (!Input.GetKey(addSelectKey)) DeSelect();
			SelectVertex(vertexDots.IndexOf(v));
		}

		public static void DeselectVertex(int v)
		{
			//		Debug.Log("Screen.SelectVertex("+v+"); locus: "+locus/*+",vertexDot: "+vertexDot*/);
			if (!selectedVertices.Contains(v)) return;
			if (vertexDots.Count < v + 1)
			{
				Debug.LogError("v (" + v + ") out of vertexDot range (" + vertexDots.Count + ")");
				return;
			}

			Vector3[] verts = instance.GetComponent<MeshFilter>().mesh.vertices;
			verts[v][1] = 0;
			vertexDots[v].Select(false);
			selectedVertices.Remove(v);
			instance.GetComponent<MeshFilter>().mesh.vertices = verts;
		}

		public static void SelectVertex(int v)
		{
			//		Debug.Log("Screen.SelectVertex("+v+"); locus: "+locus/*+",vertexDot: "+vertexDot*/);
			if (selectedVertices.Contains(v)) return;
			if (vertexDots.Count < v + 1)
			{
				Debug.LogError("v (" + v + ") out of vertexDot range (" + vertexDots.Count + ")");
				return;
			}

			Vector3[] verts = instance.GetComponent<MeshFilter>().mesh.vertices;
			verts[v][1] = /*Vector3.up**/selectedZMargin;
			vertexDots[v].Select(true);
			selectedVertices.Add(v);
			instance.GetComponent<MeshFilter>().mesh.vertices = verts;
		}

		public static void ToggleMirror(int m)
		{
			//		if(mirror[m]=!mirror[m]);
			mirror[m] = !mirror[m];
			//if(m==0)for(int v=0;v<selectedVertices.Count;v++)if(VertexIsSelected(v)&&v%columns>(columns/2+1)&&!VertexIsSelected(XMirror(v))){
			//        DeSelect();
			//}
			Debug.Log("Mirror " + (new char[] { 'X', 'Y' }[m]) + " now " + mirror[m] + ".");
		}

		public enum Shape
		{
			/*cross,*/
			rect,
			ellipse,
			lasso
		};

		//private static Shape selectionMode=Shape.rect;
		public static Shape selectionShape = Shape.rect;

		private enum ScaleMode
		{
			normal = 0,
			horizontal,
			vertical
		};

		private static ScaleMode scaleMode = ScaleMode.normal;
		private static float inch = 0.1f;

		private static Dictionary<KeyCode, Action> universalKeyDowns = new Dictionary<KeyCode, Action>()
		{
			{
				KeyCode.B,
				() =>
				{
					instance.GetComponent<Renderer>().material.mainTexture =
						instance.backgrounds[background = (background + 1) % instance.backgrounds.Length];
				}
			},
		};

		public static void SelectAll()
		{
			DeSelect();
			for (int i = instance.GetComponent<MeshFilter>().mesh.vertices.Length - 1; i > -1; i--)
				if (!deleted.Contains(i))
					SelectVertex(i);
		}

		//	private static Vector3 scaleSnapshot,posSnapshot;
		private static Vector3[] scaleSnapshot, startingScaleSnapshot;
		private static Vector3 posSnapshot;

		private static Vector2 convergingEdgeWorld /*,convergingScreenEdge*/, oppositeScreenEdge;

		//private enum Scale
		private static void BeginScale(Vector2 p, ScaleMode mode = ScaleMode.normal)
		{
			Rect[] scaleZones = ScaleZones();
			//		Rect screenRect=ScreenRect();
			screenRectSnapshot = ScreenRect();
			//		Rect meshRect=new Rect();
			scalePivot = Vector2.one - new Vector2(
				scaleZones[2].Contains(SRSUtilities.adjustedFlipped, true) ||
				scaleZones[3].Contains(SRSUtilities.adjustedFlipped, true)
					? Mathf.Round((p.x - screenRectSnapshot.x) / screenRectSnapshot.width)
					: 2,
				scaleZones[0].Contains(SRSUtilities.adjustedFlipped, true)
					? 1
					: (scaleZones[1].Contains(SRSUtilities.adjustedFlipped, true) ? 0 : 2));
			//		                                   scaleZones[0].Contains(SRSUtilities.adjustedFlipped,true)||scaleZones[1].Contains(SRSUtilities.adjustedFlipped,true)?Mathf.Round((p.FlipY().y-screenRectSnapshot.FlipY().y)/screenRectSnapshot.height):2);
			movingScaleZones = new int[2] { -1, -1 };
			for (int i = 0, n = 0; i < scaleZones.Length; i++)
				if (scaleZones[i].Contains(SRSUtilities.adjustedFlipped, true))
					movingScaleZones[n++] = i;
			scaleSnapshot = instance.GetComponent<MeshFilter>().mesh.vertices;
			oppositeEdgeScreen = new Vector2(screenRectSnapshot.x + screenRectSnapshot.width * (1 - scalePivot.x),
				screenRectSnapshot.y + screenRectSnapshot.height * (1 - scalePivot.y));
			Vector3[] verts = instance.GetComponent<MeshFilter>().mesh.vertices;
			Bounds rendererBounds = instance.GetComponent<Renderer>().bounds;
			switch (mode)
			{
				case ScaleMode.vertical: //Vertically-converging Perspective
					//convergingEdgeWorld=new Vector2(rendererBounds.center.x,rendererBounds.center.z);
					convergingEdgeWorld = new Vector2(rendererBounds.center.x, rendererBounds.center.z) + Vector2.up *
						(!mirror[1]
							? (rendererBounds.extents.z *
							   (Input.mousePosition.y > CameraHelper.Camera.WorldToScreenPoint(rendererBounds.center).y
								   ? -1
								   : 1))
							: 0);
					//convergingScreenEdge=CameraHelper.Camera.WorldToScreenPoint(rendererBounds.center);
					convergingEdgeScreen = screenRectSnapshot.center;
					break;
				case ScaleMode.horizontal: //Horizontal-converging Perspective
					convergingEdgeWorld = new Vector2(rendererBounds.center.x, rendererBounds.center.z) +
					                      Vector2.right * (!mirror[0]
						                      ? (rendererBounds.extents.x *
						                         (Input.mousePosition.x >
						                          CameraHelper.Camera.WorldToScreenPoint(rendererBounds.center).x
							                         ? -1
							                         : 1) * 0.75f)
						                      : 0);
					//convergingScreenEdge=CameraHelper.Camera.WorldToScreenPoint(rendererBounds.center);
					convergingEdgeScreen =
						screenRectSnapshot
							.center /*+Vector2.right*(mirror[0]?(screenRectSnapshot.size.x*0.5f*(Input.mousePosition.x>CameraHelper.Camera.WorldToScreenPoint(rendererBounds.center).x?-1:1)):0)*/
						;
					break;
				case ScaleMode.normal:
					convergingEdgeWorld = new Vector2(scalePivot.x == 1 ? verts.Max((v) => v.x) : verts.Min((v) => v.x),
						scalePivot.y == 0 ? verts.Max((v) => v.z) : verts.Min((v) => v.z));
					//convergingScreenEdge=new Vector2(Screen.width*scalePivot.x,Screen.height*(1-scalePivot.y));
					convergingEdgeScreen = new Vector2(screenRectSnapshot.x + screenRectSnapshot.width * scalePivot.x,
						screenRectSnapshot.y + screenRectSnapshot.height * scalePivot.y);
					break;
				default:
					Debug.LogError("Unbekannst Screen Mode: " + mode);
					break;
			}

			oppositeScreenEdge = new Vector2(Screen.width * (1 - scalePivot.x), Screen.height * (scalePivot.y));

			Debug.Log("Projection.BeginScale(" + p + "," + mode + "); full: " +
			          Mathf.Round((p.FlipY().y - screenRectSnapshot.FlipY().y) / screenRectSnapshot.height) +
			          ",condition: " +
			          (scaleZones[0].Contains(SRSUtilities.adjustedFlipped, true) ||
			           scaleZones[1].Contains(SRSUtilities.adjustedFlipped, true)) + ",pfy: " + p.y + ",sfy: " +
			          screenRectSnapshot.FlipY().y + ",sfh: " + screenRectSnapshot.height + " Rects: (" +
			          scaleZones[0].Contains(SRSUtilities.adjustedFlipped, true) + "," +
			          scaleZones[1].Contains(SRSUtilities.adjustedFlipped, true) + "," +
			          scaleZones[2].Contains(SRSUtilities.adjustedFlipped, true) + "," +
			          scaleZones[3].Contains(SRSUtilities.adjustedFlipped, true) + "),scalePivot: " + scalePivot +
			          ",convergingEdgeScreen: " + convergingEdgeScreen + ",screenRect: " + screenRectSnapshot +
			          ",screen dimensions: (" + Screen.width + "," + Screen.height + ")");
		}

		//private static int subTool[]=new int[Enum.GetValues(typeof(ToolMode)).Length];
		private static void FinishBuildingLassoBlackout(bool addUndo = true)
		{
			AddUndoStep(
 /*new UndoStep{ createBlackout=true,blackout=lassoBlackout,blackoutInd=blackouts.IndexOf(lassoBlackout) }*/);
			if (lassoBlackout == null)
			{
				Debug.LogError("lassoBlackout ist Null.");
				return;
			}

			Graphics.SetAlpha(lassoBlackout.lassoObject.GetComponent<MeshRenderer>().material, 1);
			lassoBlackout = null;
			lassoPoints.Clear();
		}

		private static void WipeIntermittents()
		{
			DeSelect(false);
			Blackout.DeSelect();
			WipeLasso();
			ClearVertexModes();
		}

		private static void ClearVertexModes()
		{
			for (int i = 0; i < mirror.Length; i++) mirror[i] = false;
			circle = false;
		}

		private void Update()
		{
			if (mode != Mode.normal) return;

			if (Input.GetKeyDown(KeyCode.Keypad7)) Debug.Log("KeyPad7.");
			inch = Input.GetKey(coarseInchKey) ? 0.1f : 0.005f; //Fine movement of vertex positions
			if (toolBehaviour[(int)toolMode].keyDowns != null)
				foreach (KeyValuePair<KeyCode, Action> kvp in toolBehaviour[(int)toolMode].keyDowns)
					if (Input.GetKeyDown(kvp.Key))
						kvp.Value();
			foreach (KeyValuePair<KeyCode, Action> kvp in universalKeyDowns)
				if (Input.GetKeyDown(kvp.Key))
					kvp.Value();
			if (Input.GetMouseButtonDown(0)) MouseDown();
			else if (Input.GetMouseButtonUp(0)) MouseUp();
			else if (dragging && currentTool.OnDrag != null) currentTool.OnDrag(SRSUtilities.adjustedMousePosition);
			if (toolMode == ToolMode.blackout && Input.GetKeyDown(buildLassoKey) && lassoBlackout != null)
				FinishBuildingLassoBlackout(lassoPoints.Count > 2);

		}

		//    private static Vector2 toolbarPosition=new Vector2(50,50);
		private static Vector2 scalePivot = -Vector3.one * 2;

		//	private static Vector2 convergingEdgeScreen{get{return new Vector2(scalePivot.x*Screen.width,scalePivot.y*Screen.height);}}//The edge/corner to which we're converging.
		private static Vector2
			convergingEdgeScreen, oppositeEdgeScreen; //The gantry edge/corner to which we're converging.

		//	private static Vector3[] scaleSnapshot;
		private static bool scaling
		{
			get { return scalePivot != -Vector2.one * 2; }
			set
			{
				if (!value) scalePivot = -Vector2.one * 2;
			}
		}

		//private static bool perspectiveScaling=false;
		//	private static Vector2 lerpSnapshot;
		private static Rect screenRectSnapshot;

		private static Rect ScreenRect()
		{
			if (vertexDots.Count < 1)
			{
				Debug.Log("ScreenRect() called with no vertex dots by which to gauge. Defaulting to (0,0,2048,768).");
				return new Rect(0, 0, 2048, 768);
			}

			return SRSUtilities.RectAround(
				CameraHelper.Camera.WorldToScreenPoint(new Vector3(vertexDots.Min((vd) => vd.transform.position.x),
					vertexDots.Min((vd) => vd.transform.position.y), vertexDots.Max((vd) => vd.transform.position.z))),
				CameraHelper.Camera.WorldToScreenPoint(new Vector3(vertexDots.Max((vd) => vd.transform.position.x),
					vertexDots.Max((vd) => vd.transform.position.y), vertexDots.Min((vd) => vd.transform.position.z)))
			);
		}

		private static Rect[] ScaleZones()
		{
			Rect screenRect = ScreenRect();
			Vector2 topLeft = screenRect.position, botRight = screenRect.position + screenRect.size;
			//		Debug.Log("topLeft: "+topLeft+",botRight: "+botRight+",First box: "+SRSUtilities.RectAround(new Vector2(topLeft.x,Mathf.Lerp(topLeft.y,botRight.y,0.75f)).FlipY(),botRight.FlipY())+",mouse cursor: "+SRSUtilities.adjustedMousePosition+",bounds: "+instance.GetComponent<MeshFilter>().mesh.bounds+",renderer bounds: "+instance.GetComponent<Renderer>().bounds);
			//		Debug.Log("topLeft: "+topLeft+",botRight: "+botRight);
			return new Rect[]
			{
				SRSUtilities.RectAround(new Vector2(topLeft.x, Mathf.Lerp(topLeft.y, botRight.y, 0.75f)).FlipY(),
					botRight.FlipY()), //Kopf
				SRSUtilities.RectAround(topLeft.FlipY(),
					new Vector2(botRight.x, Mathf.Lerp(topLeft.y, botRight.y, 0.25f)).FlipY()), //Tief
				SRSUtilities.RectAround(topLeft.FlipY(),
					new Vector2(Mathf.Lerp(topLeft.x, botRight.x, 0.25f), botRight.y).FlipY()), //Links
				SRSUtilities.RectAround(new Vector2(Mathf.Lerp(topLeft.x, botRight.x, 0.75f), topLeft.y).FlipY(),
					botRight.FlipY()) //Rechts
			};
		}

		private static void DrawPerspectiveZones(Color c)
		{
			//Debug.Log("ContourEditor.DrawPerspectiveZones()");
			Rect screenRect = ScreenRect();
			//Vector2 topLeft=screenRect.position,botRight=screenRect.position+screenRect.size;
			//		Debug.Log("topLeft: "+topLeft+",botRight: "+botRight+",First box: "+SRSUtilities.RectAround(new Vector2(topLeft.x,Mathf.Lerp(topLeft.y,botRight.y,0.75f)).FlipY(),botRight.FlipY())+",mouse cursor: "+SRSUtilities.adjustedMousePosition+",bounds: "+instance.GetComponent<MeshFilter>().mesh.bounds+",renderer bounds: "+instance.GetComponent<Renderer>().bounds);
			//		Debug.Log("topLeft: "+topLeft+",botRight: "+botRight);
			for (int x = 0; x < 2; x++)
			for (int y = 0; y < 2; y++)
			{
				Vector2 corner = new Vector2(screenRect.x + screenRect.width * x, screenRect.y + screenRect.height * y);
				Graphics.DrawRectAround(corner, Vector2.Lerp(corner, screenRect.center, 0.5f), c); //Kopf-links
			}
		}

		public static void DrawBlackouts(bool adjusted = false)
		{
			foreach (Blackout b in blackouts)
			{
				if (b.lassoObject == null)
				{
					//Lasso blackouts have their own meshes on objects of which the 3D engine will take care.
					Rect r = adjusted
						? new Rect(b.rect.x * Settings.ScreenWidth / Screen.width,
							b.rect.y * Settings.ScreenHeight / Screen.height, b.rect.width * Settings.ScreenWidth / Screen.width,
							b.rect.height * Settings.ScreenHeight / Screen.height)
						: b.rect;
					Graphics.DrawColoredTexture(r, b.elliptical ? Graphics.filledEllipse : Graphics.weiss1x1,
						b.farbe.WithAlpha(Blackout.selected > -1 && b == blackouts[Blackout.selected]
							? selectedBlackoutColor.a
							: 1));
					//Graphics.DrawColoredTexture(r,b.elliptical?Graphics.filledEllipse:Graphics.weiss1x1,Color.white);
				} //else Graphics.DrawRect(SRSUtilities.BoundingRect(b.lassoObject.GetComponent<MeshFilter>().mesh.vertices),Color.yellow);
			}
		}

		public enum Mode
		{
			normal,
			save,
			load
		};

		public static Mode mode = Mode.normal;
		private static string saveName = "";

		Vector2 scrollPosition = Vector2.zero;
		private static string fileToDelete = string.Empty;
#if UNITY_EDITOR
		private static string backupDir = "../bkp";
#else
    private static string backupDir = "/home/motions/bkp";
#endif

		//Called from ContourEditorUI
		public void SetVertexAmount(int verts)
		{
			originalColumns = columns = verts;
			Reset(-1, true, true);
			
		}

		public static bool HideOldUI = false;
		public static bool HideGUI = false;
		private static List<GameObject> _lassoObjects = new List<GameObject>();

		private void OnGUI()
		{
			if (HideGUI)
				return;
			DrawBlackouts();

			if (originalColumns < minDensity)
			{
				SRSUtilities.NormalizeGUIMatrix();

				return;
			}

			Draw();
		}

		private static void Draw()
		{
			GUI.color = Color.green;
			for (int i = 0; i < mirror.Length; i++)
				if (mirror[i])
					GUI.DrawTexture(
						new Rect(
							(1 - i) * (Screen.width * (0.5f - 0.25f * (Projection.DisplaysAmount - 1) *
								Mathf.Sign(CameraHelper.Camera.transform.position.x)) - 1),
							i * (Screen.height * 0.5f - 1), (1 - i) * 4 + i * Screen.width,
							i * 4 + (1 - i) * Screen.height), Graphics.weiss1x1);
			GUI.color = Color.white;
			if (toolBehaviour[(int)toolMode].Draw != null)
				toolBehaviour[(int)toolMode].Draw(SRSUtilities.adjustedFlipped);
		}

		private void SaveAndQuitToMenu()
		{
			_lassoObjects.Clear();

			DeSelect();

			WipeLasso();

			instance.gameObject.SetActive(false);

			WipeBlackouts();

			_projection.IsEditing = false;
			
			_quitButtonAction?.Invoke();
		}

		public static void LoadConfigurationByName(string name)
		{
			instance.LoadConfiguration(name);
			
			SaveDefaultConfiguration(name);
		}
		
		public static void SaveConfiguration(string fileName)
		{
			/*#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
				return;
			#endif*/
			
			if (!fileName.EndsWith(Constants.GantryExtension)) 
				fileName += Constants.GantryExtension;

			var path = Path.Combine(Settings.GantryPatternsPath, fileName);

			BinaryWriter bw;

			try
			{
				if (!Directory.Exists(Settings.GantryPatternsPath))
					Directory.CreateDirectory(Settings.GantryPatternsPath);

				bw = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));
			}
			catch (IOException e)
			{
				Debug.LogError("Could not open or create file \"" + fileName + "\": " + e.Message);
				return;
			}

			try
			{
				bw.Write(Convert.ToInt32(Settings.monitorMode));
				Vector3[] verts = instance.GetComponent<MeshFilter>().mesh.vertices;
				bw.Write(verts.Length);
				for (int i = 0; i < verts.Length; i++)
				for (int j = 0; j < 3; j++)
					bw.Write((double)verts[i][j]);
				bw.Write(deleted.Count);
				for (int i = 0; i < deleted.Count; i++) bw.Write(deleted[i]);
				bw.Write(blackouts.Count);
				for (int i = 0; i < blackouts.Count; i++)
				{
					bw.Write(blackouts[i].elliptical);
					bw.Write(blackouts[i].farbe.r == 1);

					for (int j = 0; j < 4; j++) bw.Write((double)blackouts[i].rect.Elements()[j]);

					if (blackouts[i].lassoObject != null)
					{
						Mesh m = blackouts[i].lassoObject.GetComponent<MeshFilter>().mesh;
						bw.Write(m.vertexCount - 1);
						for (int v = 1; v < m.vertexCount; v++)
						for (int c = 0; c < 3; c++)
							bw.Write(m.vertices[v][
								c]); //We omit the first which is the center point which we can reconstruct from the remainder.
					}
					else bw.Write(0);
				}

				if (blackouts.Count > 0) Debug.LogWarning("First blackout rect: " + blackouts[0].rect);
			}
			catch (IOException e)
			{
				Debug.LogError("Could not write to file \"" + fileName + "\": " + e.Message);
				return;
			}

			bw.Close();

			if (saveAsDefault > -1) 
				SaveDefaultConfiguration(fileName);

			mode = Mode.normal;
		}

		private static void SaveDefaultConfiguration(string fileName)
		{
			PlayerPrefs.SetString(Constants.DefaultConfigHash, fileName);
			PlayerPrefs.Save();
		}

		public void LoadConfiguration(string fileName, int screen = -1)
		{
			Debug.Log("Projection.LoadConfiguration(" + fileName + ")");
			
			var screenObj = screen > -1 ? _projection.Screens[screen].GetObject() : instance.gameObject;

			BinaryReader br;
			try
			{
				br = new BinaryReader(new FileStream(Path.GetFullPath(fileName), FileMode.Open));
			}
			catch (IOException e)
			{
				Debug.LogError("Could not open file stream for \"" + fileName + "\": " + e.Message);
				return;
			}

			try
			{
				Reset(-1, screen != 1); //If screen is 1, we're loading the second configuration in play mode, so keep the first screen's blackouts.
				Settings.monitorMode = (Settings.MonitorMode)br.ReadInt32();
				Vector3[] verts = new Vector3[br.ReadInt32()];
				for (int i = 0; i < verts.Length; i++)
				for (int j = 0; j < 3; j++)
					verts[i][j] = (float)br.ReadDouble();
				columns = (int)Mathf.Sqrt(verts.Length);
				ReconstructScreen(0, false, screenObj);
				deleted.Clear();
				int numDeletions = br.ReadInt32();
				List<int> deletees = new List<int>();
				for (int i = 0; i < numDeletions; i++) deletees.Add(br.ReadInt32());
				Delete(deletees.ToArray(), screenObj);
				screenObj.GetComponent<MeshFilter>().mesh.vertices = verts;
				if (_projection)
				{
					for (int i = 0; i < vertexDots.Count; i++)
						vertexDots[i].transform.position = instance.transform.TransformPoint(verts[i]);
					blackouts.Clear();
				}

				int numBlackouts = br.ReadInt32();
				Debug.Log($"[Loader] blackouts {numBlackouts}");
				for (int i = 0; i < numBlackouts; i++)
				{
					bool ellip = br.ReadBoolean();
					Debug.Log($"[Loader] isEllip? {ellip}");

					Color c = br.ReadBoolean() ? Color.white : Color.black;
					Debug.Log($"[Loader] Color {c}");

					Rect r = new Rect((float)br.ReadDouble(), (float)br.ReadDouble(), (float)br.ReadDouble(),
						(float)br.ReadDouble());
					Debug.Log($"[Loader] rect size {r.x} {r.y} {r.width} {r.height}");

					Blackout bo = new Blackout { elliptical = ellip, rect = r, farbe = c };
					int numLassoPoints = br.ReadInt32();
					Debug.Log($"[Loader] num lasso {numLassoPoints}");

					if (numLassoPoints > 0)
					{
						lassoPoints.Clear();
						for (int l = 0; l < numLassoPoints; l++)
							lassoPoints.Add(new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle()));
						BuildLassoMesh(bo, lassoPoints.ToArray());
						bo.lassoObject.GetComponent<MeshRenderer>().material.color = c;
						Debug.Log("Farbe: " + c);
						if (screen > -1) bo.lassoObject.transform.position += _projection.ScreenPosition(screen);
					}

					Debug.Log("Processing blackout " + i + ": " + r + ",screen: " + screenObj.name + ",equal: " +
					          (screenObj == _projection.Screens[1].GetObject()) + ",numLassoPoints: " + numLassoPoints);
					blackouts.Add(bo);
				}

				Debug.Log("Read " + verts.Length + " vertices (" + columns + " columns) and " + numBlackouts +
				          " blackouts (using " + blackouts.Count + ") from file " + fileName + ".");
			}
			catch (IOException e)
			{
				Debug.LogError("Could not read from file \"" + fileName + "\": " + e.Message);
				return;
			}

			br.Close();
			undos.Clear();
			undo = 0;
			mode = Mode.normal;
		}
	}
}