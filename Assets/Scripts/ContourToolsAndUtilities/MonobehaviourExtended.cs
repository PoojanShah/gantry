using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
static public class MethodExtensionForMonoBehaviourTransform{
	static public T GetOrAddComponent<T>(this Component child) where T:Component{//Gets or add a component. Usage example: BoxCollider boxCollider = transform.GetOrAddComponent<BoxCollider>();
		T result=child.GetComponent<T>();
		if(result==null)result=child.gameObject.AddComponent<T>();
		return result;
	}
	static public Transform LastChild(this Transform trans){
		Transform child=null;
		foreach(Transform t in trans)child=t;
		return child;
	}
	static public GameObject LastChildGameObject(this Transform trans){
		Transform lastChild=trans.LastChild();
		return lastChild!=null?lastChild.gameObject:null;
	}
	static public string Stringify(this ArrayList arrlist){
		string str="";
		foreach(UnityEngine.Object o in arrlist)str+=(str!=""?", ":"")+o.ToString();
		return str;
	}
//	static public GameObject FindOrNew(string objectName)
	public static bool HasChildWithNameContaining(this Transform subject,string needle){
		foreach(Transform t in subject)if(t.name.Contains(needle))return true;
		return false;
	}
	public static bool HasChildOfNameStartingWith(this Transform subject,string needle){
		foreach(Transform t in subject)if(t.name.StartsWith(needle))return true;
		return false;
	}
	public static void DestroyChildren(this Transform subject,Func<Transform,bool> tester=null){
		List<Transform> children=new List<Transform>();
		foreach(Transform t in subject)if(tester==null||tester(t))children.Add(t);
		foreach(Transform t in children)UnityEngine.Object.Destroy(t.gameObject);
	}
	public static void ReparentChildren(this Transform subject,Transform newParent,Func<Transform,bool> tester=null){
		List<Transform> children=new List<Transform>();
		foreach(Transform t in subject)if(tester==null||tester(t))children.Add(t);
		foreach(Transform t in children)t.parent=newParent;
	}
	public static void Shuffle<T>(this IList<T> list){
		for(int i=list.Count-1;i>0;i--){
			int k=UnityEngine.Random.Range(0,i+1);
			T tmp=list[k];
			list[k]=list[i];
			list[i]=tmp;
		}
	}
	public static string Stringify(this int[] ints){
		if(ints==null)return "(null)";
		else if(ints.Length<1)return "[]";
		string str="[";
		for(int i=0;i<ints.Length;i++)str+=ints[i]+",";
		return str.Substring(0,str.Length-1)+"] ("+ints.Length+")";
	}
	public static Vector2 ToVector2XZ(this Vector3 v){
		return new Vector2(v.x,v.z);
	}
	public static Vector2 ToVector3XZ(this Vector2 v){
		return new Vector3(v.x,0,v.y);
	}
	public static Vector2 FlipY(this Vector3 v){
		return new Vector2(v.x,Screen.height-v.y);
	}
	public static Vector2 FlipY(this Vector2 v){
		return new Vector2(v.x,Screen.height-v.y);
	}
	public static Rect FlipY(this Rect r){
		return new Rect(r.x,Screen.height-r.y,r.width,r.height);
	}
//	public static Vector2 Position(this Rect r){
//		return new Vector2(r.x,r.y);
//	}
//	public static Vector2 PositionFlipped(this Rect r){
//		return new Vector2(r.x,Screen.height-r.y);
//	}
	public static bool SRSContains(this Rect r,Vector2 p){
		return r.Contains(p);
	}
	public static float[] Elements(this Rect r){
		return new float[]{r.x,r.y,r.width,r.height};
	}
    public static Color WithAlpha(this Color c,float a){
        return new Color(c.r,c.g,c.b,a);
    }
}