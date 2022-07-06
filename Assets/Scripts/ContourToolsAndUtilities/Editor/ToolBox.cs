using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
public class ToolBox:EditorWindow{
	[MenuItem("Window/SRS ToolBox")]
	public static void ShowWindow(){
		EditorWindow.GetWindow(typeof(ToolBox));
	}
	private void OnGUI(){
//		if(GUI.Button(new Rect(5,5,128,32),"Bababooey"))foreach(Transform t in GameObject.Find("Obstacle Timbers").transform)t.DestroyChildren(null,true);//Debug.Log("Children: "+t.childCount);
		if(GUI.Button(new Rect(5,5,128,32),"Transient Tool"))TransientTool();
	}
	private static void TransientTool(){
		List<GameObject> deletables=new List<GameObject>();
		foreach(Transform t in GameObject.Find("Obstacle Timbers").transform)foreach(Transform tt in t)deletables.Add(tt.gameObject);
		foreach(GameObject dObj in deletables)Undo.DestroyObjectImmediate(dObj);//Debug.Log("Children: "+t.childCount);
	}
}
