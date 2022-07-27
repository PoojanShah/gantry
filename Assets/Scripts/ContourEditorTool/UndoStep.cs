using UnityEngine;

namespace ContourEditorTool
{
	public partial class ContourEditor
	{
		public struct UndoStep
		{
			public Vector3[] meshSnapshot;
			public int[] selectedVerts, deletedVerts, triangleSnapshot, blackoutLassoIndices;
			public Blackout[] blackouts;
			public Vector3[][] blackoutLassoMeshes;
			public int[][] blackoutTriangleSnapshots;

			public string Stringify()
			{
				return "meshSnapshot: " + meshSnapshot.Length + "\ntriangleSnapshot: " + triangleSnapshot.Length +
				       "\nselectedVerts: " + selectedVerts.Length + "\nblackouts: " + blackouts.Length +
				       "\n\nFirst 3 verts: " + meshSnapshot[0] + "," + meshSnapshot[1] + "," + meshSnapshot[2];
			}
		}
	}
}