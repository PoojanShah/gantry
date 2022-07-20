//2015-02-12
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Linq;
public static class SRSUtilities:System.Object{
    static private float stretchFactor=0.875f;
//	public static float ScreenWidth=1024,ScreenHeight=768;
	public static float ScreenW=1280,ScreenH=1024;
	static public IEnumerator CallInFrames(Action f,int frames){
		Debug.Log("SRSUtilities.CallInFrames("+f.ToString()+", "+frames+")");
		for(int i=0;i<=frames;i++){
//			Debug.Log("Calling an action in "+(frames-i)+" frames.");
			yield return null;
		}
		f();
	}
    private static GUISkin _darkAgesSkin;
    private static GUISkin darkAgesSkin{
        get{
            if(_darkAgesSkin==null)_darkAgesSkin=Resources.Load<GUISkin>("Dark Ages");
            return _darkAgesSkin;
        }
    }
//	static public IEnumerator CallInSeconds(Action f,float seconds){
//		Debug.Log("SRSUtilities.CallInSeconds("+f.ToString()+", "+seconds+")");
//		yield return null;//new WaitForSeconds(seconds);
//		Debug.Log("SRSUtilities.CallInSeconds(), after yield.");
////		StartCoroutine(f,seconds);
//		f();
//	}
	public static IEnumerator CallInSeconds(Action f,float seconds=0){
//		Debug.Log("TestRoutine UNO.");
		yield return new WaitForSeconds(seconds);
//		Debug.Log("TestRoutine DOS.");
		f();
	}
    public static void StretchedLabel(Rect r,string text,float stretchBy){
        StretchedLabel(r,text,GUI.skin.label,stretchBy);
    }
    public static float CalcStretchedFontHeight(GUIStyle style,string text,float width,float stretchedBy){
//        float height=darkAgesSkin.customStyles[9].CalcHeight(new GUIContent(text),width);
//        return height-(stretchedBy-1)*height;
        return darkAgesSkin.customStyles[9].CalcHeight(new GUIContent(text),width-(stretchedBy-1)*width);
    }
    public static void StretchedLabel(Rect r,string text,GUIStyle style,float stretchBy){
        Matrix4x4 matrixBackup=GUI.matrix;
        GUI.matrix*=Matrix4x4.Scale(new Vector3(stretchBy,1,1));
//        Debug.Log("Alignment: "+style.alignment.ToString());
        if(style.alignment.ToString().EndsWith("Center")){
//                    r.width/=stretchBy;
//            r.width-=r.width-r.width/stretchBy;
            r.width-=(stretchBy-1)*r.width;
//            r.x-=(stretchBy*r.width-r.width)*0.5f;
        }
        GUI.Label(r,text,style);
        GUI.matrix=matrixBackup;
    }
    public static bool StretchedButtonLabel(Rect r,string text,float stretchBy){
        return StretchedButtonLabel(r,text,GUI.skin.button,stretchBy);
    }
    public static bool CorrectedButtonLabel(Rect r,string text,GUIStyle style){
        return StretchedButtonLabel(r,text,style,stretchFactor*(1024f/768f)/((float)Screen.width/(float)Screen.height));
    }
    public static bool StretchedButtonLabel(Rect r,string text,GUIStyle style,float stretchBy){
        //        Debug.Log("SRSUtilities.StretchedButtonLabel stretchby: "+stretchBy+"; style's font size: "+style.fontSize);
        bool ergebnis=GUI.Button(r,"",style);
        Matrix4x4 matrixBackup=GUI.matrix;
        Texture2D normal=style.normal.background,hover=style.hover.background,active=style.active.background;
        TextAnchor alignment=style.alignment;
        style.alignment=TextAnchor.MiddleCenter;
        style.normal.background=style.hover.background=style.active.background=null;
        GUI.matrix*=Matrix4x4.Scale(new Vector3(stretchBy,1,1));
        //        GUI.Label(new Rect(r.x+r.width*0.5f,r.y+r.height*0.5f,r.width,r.height),text,darkAgesSkin.customStyles[8]);
        //        GUI.Label(r,text,darkAgesSkin.customStyles[8]);
        //        GUI.Label(new Rect(r.x/stretchBy,r.y,r.width/stretchBy,r.height),text,darkAgesSkin.customStyles[8]);
        GUI.Label(new Rect(r.x/stretchBy,r.y,r.width/stretchBy,r.height),text,style);
        GUI.matrix=matrixBackup;
        style.normal.background=normal;
        style.hover.background=hover;
        style.active.background=active;
        style.alignment=alignment;
        return ergebnis;
    }
    public static string Stringify<TKey,TValue>(this Dictionary<TKey,TValue> d,string delimiter=","){
        string str="{";
        foreach(KeyValuePair<TKey,TValue> kvp in d)str+=kvp.Key+":"+kvp.Value+delimiter;
        return str.Substring(0,str.Length-1)+"}";
    }
    public static void ApplyRecursively(this Transform trans,Action<Transform> a,bool includeSelf=true){
//        Debug.Log(trans.name+".ApplyRecursively("+a+"), children: "+trans.childCount);
        if(includeSelf)a(trans);
        foreach(Transform t in trans)t.ApplyRecursively(a);
    }
	public static bool guiMatrixNormalized=false;
    public static void NormalizeGUIMatrix(){
        GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3((float)1.0*Screen.width/Settings.ScreenWidth,(float)1.0*Screen.height/Settings.ScreenHeight,1.0f));
//		guiMatrixNormalized=true;
    }
    public static void ApplyToAllMaterials(GameObject o,Action<Material> a){
        foreach(Renderer r in o.GetComponentsInChildren<Renderer>())foreach(Material m in r.materials)a(m);
    }
    public static bool Approximate(Vector3 v1,Vector3 v2,float threshold=0.1f){
        return Vector3.Distance(v1,v2)<threshold;
    }
    public static string Vector3ToString(Vector3 v){
        return "("+v.x+","+v.y+","+v.z+")";
    }
    public static string ToPreciseString(this Vector3 v){
        return "("+v.x+","+v.y+","+v.z+")";
    }
    public static Vector3 Vector3Multiply(Vector3 v1,Vector3 v2){return new Vector3(v1.x*v2.x,v1.y*v2.y,v1.z*v2.z);}
    public static void DrawPlane(Plane p,Color edgeColor=default(Color),Color normalColor=default(Color),float time=30,float scale=1){
           DrawPlane(p.normal*p.distance,p.normal,edgeColor,normalColor,time,scale);
    }
    public static void DrawPlane(Vector3 position,Vector3 normal,Color edgeColor=default(Color),Color normalColor=default(Color),float time=30,float scale=1){
      //      Debug.Log("Default color:"+default(Color));
        edgeColor=edgeColor==default(Color)?Color.green:edgeColor;
        normalColor=normalColor==default(Color)?Color.red:normalColor;
        Vector3 v3=Vector3.Cross(normal,normal.normalized==Vector3.forward?Vector3.up:Vector3.forward).normalized*normal.magnitude*scale;
        Vector3 corner0=position+v3;
        Vector3 corner2=position-v3;
        var q=Quaternion.AngleAxis(90,normal);
        v3=q*v3;
        Vector3 corner1=position+v3;
        Vector3 corner3=position-v3;
        Debug.DrawLine(corner0,corner2,edgeColor,time);
        Debug.DrawLine(corner1,corner3,edgeColor,time);
        Debug.DrawLine(corner0,corner1,edgeColor,time);
        Debug.DrawLine(corner1,corner2,edgeColor,time);
        Debug.DrawLine(corner2,corner3,edgeColor,time);
        Debug.DrawLine(corner3,corner0,edgeColor,time);
        Debug.DrawRay(position,normal,normalColor,time);
    }
    public static void Draw3DCrosshair(Vector3 p,float size=1,Color color=default(Color),float duration=0){
        if(color==default(Color))color=Color.red;
        for(int i=0;i<3;i++)for(int s=-1;s<=1;s+=2){
          Vector3 v=Vector3.zero;
          v[i]=size*s;
          Debug.DrawLine(p,p+v,color,duration);
        }
    }
    public static void UniformMenu(Rect r,KeyValuePair<string,Action>[] entries,GUIStyle style=null,float margin=8){
        for(int i=0;i<entries.Length;i++)if(entries[i].Key!=""){
          if(GUI.Button(r,entries[i].Key,style??GUI.skin.button))entries[i].Value();
          r.y+=r.height+margin;
        }
    }
	public static void AddUnique<T>(this List<T> list,T newValue){//TODO: Find out how to generalize to List<T>.
		if(!list.Contains(newValue))list.Add(newValue);
	}
	public static void AddRangeUnique<T>(this List<T> list,List<T> newValues){//TODO: Find out how to generalize to List<T>.
		foreach(T v in newValues)list.AddUnique(v);
	}
	public static string Stringify<T>(this IEnumerable<T> list,string delimiter=","){
		if(list==null)return "(null)";
		string str="{";
		foreach(T obj in list)str+=obj+delimiter;
		return (str.Length>1?str.Substring(0,str.Length-delimiter.Length):str)+"}";
	}
	public static int[] ArcPattern(int r){//90-degree arc
		int x=0;
		int y=r;
		int p=3-2*r;
		int[] pattern=new int[r+1];
		for(var i=0;i<pattern.Length;i++)pattern[i]=0;
		while(x<=y){
			pattern[x]=Mathf.Max(pattern[x],y);
			pattern[y]=Mathf.Max(pattern[y],x);
			if(p<0)p+=4*x+++6;
			else p+=4*(x++-y--)+10;
		}
		Debug.Log("SRSUtilities.ArcPattern("+r+"): "+pattern.Stringify());
		return pattern;
	}
	public static bool Contains(this int[] ints,int needle){
		if(ints==null)return false;
		for(int i=0;i<ints.Length;i++)if(ints[i]==needle)return true;
		return false;
	}
	public static int RadiusOf(int x,int y){
		Debug.Log("SRSUtilities.RadiusOf("+x+","+y+")");
		int r=x-1;
//        for(bool collinear=false;!collinear;r++){
        while(ArcPattern(++r)[x]<y);
		Debug.Log("SRSUtilities.RadiusOf("+x+","+y+"): "+r);
		return r;
	}
	public static Vector3 Flatten(this Vector3 vec,int dimension){
		vec[dimension]=0;
		return vec;
	}
	public static bool EllipseContains(this Rect r,Vector2 p){
//		float major=Mathf.Max(r.width,r.height),minor=Mathf.Min(r.width,r.height);
//		float distance=Vector2.Distance(r.center,p);
		Vector2 pRelative=new Vector2(p.x-r.center.x,p.y-r.center.y),radii=new Vector2(r.width*0.5f,r.height*0.5f);
//		return distance<major&&Mathf.Abs(p[-r.center.y)<major*minor/major;
//		return distance<minor||(distance<major&&Mathf.Abs(p[-r.center.y)<major*minor/major;
		return (pRelative.x*pRelative.x)/(radii.x*radii.x)+(pRelative.y*pRelative.y)/(radii.y*radii.y)<=1;
	}
	public static Rect RectAround(Vector2 v1,Vector2 v2){
		return new Rect(v1.x,v1.y,v2.x-v1.x,v2.y-v1.y);
	}
	
	public static int[] Adjacents(int v,int rows,int cols=-1){
		if(cols==-1)cols=rows;
		List<int> adj=new List<int>();
		int k;
		for(int x=-1;x<=1;x++)for(int y=-cols;y<=cols;y+=cols)if((k=v+x+y)!=v&&(y!=0||k/cols==v/cols)&&k>-1&&k<cols*rows)adj.Add(v+x+y);
		return adj.ToArray();
	}
	public static void RemoveSet(this List<int> l,int[] set){
		foreach(int i in set)l.Remove(i);
	}
	public static float Lerp(float from,float to,float lerp){
		return from+(to-from)*lerp;
	}
	public static Vector2 adjustedMousePosition{
//		get{return new Vector2(Input.mousePosition.x*Settings.ScreenWidth/Screen.width,Input.mousePosition.y*Settings.ScreenHeight/Screen.height);}
//		get{return Input.mousePosition;}
		//get{return GUI.matrix==Matrix4x4.identity?(Vector2)Input.mousePosition:new Vector2(Input.mousePosition.x*Settings.ScreenWidth/Screen.width,Input.mousePosition.y*Settings.ScreenHeight/Screen.height);}
		get{return (Vector2)Input.mousePosition;}
	}
	public static Vector2 adjustedFlipped{
        get{return adjustedMousePosition.FlipY();}
        //get{return new Vector2(adjustedMousePosition.x,(GUI.matrix==Matrix4x4.identity?Screen.height:SRSUtilities.ScreenHeight)-adjustedMousePosition.y);}
        //get{return new Vector2(adjustedMousePosition.x,(GUI.matrix==Matrix4x4.identity?Screen.height:SRSUtilities.ScreenHeight)-adjustedMousePosition.y);}

    }
    public static void ForEach<T>(this IEnumerable<T> source, Action<T> action){
        //source.ThrowIfNull("source");
        //action.ThrowIfNull("action");
        foreach(T element in source)action(element);
    }
    //public static void Apply<T>(this IEnumerable<T> source, Action<T> action){
    //    //source.ThrowIfNull("source");
    //    //action.ThrowIfNull("action");

    //    foreach(T element in source)action(element);
    //}
    public static void AddElement<T>(ref T[] source,T element){
        List<T> list=new List<T>();
        foreach(T e in source)list.Add(e);
        list.Add(element);
        source=list.ToArray();
    }
    public static void RemoveElement<T>(ref T[] source,T element){
        List<T> list=new List<T>();
        foreach(T e in source)list.Add(e);
        list.Remove(element);
        source=list.ToArray();
    }
    public static Vector3 Midpoint(Vector3[] vects){
        Vector3 min=vects[0],max=vects[0];
        for(int v=1;v<vects.Length;v++){
            min=Vector3.Min(vects[v],vects[v-1]);
            max=Vector3.Max(vects[v],vects[v-1]);
        }
        return Vector3.Lerp(min,max,0.5f);
    }
#if UNITY_EDITOR
    public static string slashChar = "\\";
    //public static string slashChar="/";
#elif UNITY_STANDALONE_LINUX
    public static string slashChar="/";
#else
    public static string slashChar="\\";
#endif
    //public static int IndexOfFirstMatch(this IEnumerable<T> list,Func<T,bool> tester){
    //    return list.Select((value,i)=>new{value,i=i+1}).Where(pair=>tester(pair.value)).Select(pair=>pair.i).FirstOrDefault()-1;
    //}
    public static bool TCPMessage(string message,string ip,int port){//Sends a string to the IP and port.
        Debug.Log("SRSUtilities.TCPMessage(\""+message+"\",\""+ip+"\","+port+")");//Sends a string to the IP and port.
        Socket soc;
        IPEndPoint remoteEP;
        try{
            soc=new Socket(AddressFamily.InterNetwork,SocketType.Stream,ProtocolType.Tcp);
            remoteEP=new IPEndPoint(IPAddress.Parse(ip),port);
            if(soc==null){
                Debug.LogError("Keine Verbindung zu "+ip+":"+port+".");
                return false;
            }
            soc.Connect(remoteEP);
            soc.Send(System.Text.Encoding.ASCII.GetBytes(message));
            soc.Disconnect(false);
            soc.Close();
        }catch(SocketException e){
            Debug.LogError("Exception connecting to "+ip+":"+port+";\nSource: \""+e.Source+"\"\nMessage: \""+e.Message+"\"");
            return false;
        }catch(Exception e){
            Debug.LogError("Exception connecting to "+ip+":"+port+";\nSource: \""+e.Source+"\"\nMessage: \""+e.Message+"\"");
            return false;
        }
        return true;
    }
    public static int IndexOfFirstMatch<T>(this T[] list,Func<T,bool> tester){
        for(int i=0;i<list.Length;i++)if(tester(list[i]))return i;
        return -1;
    }
    public static int IndexOfKey(this OrderedDictionary dic,object key){
        int i=-1;
        foreach(object k in dic.Keys){
            i++;
            if(k.Equals(key))return i;
        }
        return -1;
    }
    public static object KeyOfIndex(this OrderedDictionary od,int i,bool wrap=false){
        if(wrap)i=(i%od.Count+od.Count)%od.Count;
        return i>od.Count-1||i<0?null:od.Cast<DictionaryEntry>().ElementAt(i).Key;
    }
    public static Rect ConstrainTo(this Rect r,Rect zu,bool preserveSize=false,bool incremental=false){
        if(incremental){
            while(r.xMin<zu.xMin)r.x+=r.width;
            while(r.xMax>zu.xMax)r.x-=r.width;
            //while(r.bottom<zu.bottom)r.y+=r.height;
            //while(r.top>zu.top)r.y-=r.height;
            while(r.yMax>zu.yMax){
                r.y-=r.height;
                Debug.Log("+Neue Höhe: "+r.y+", bottom: "+r.yMax+", zu.y: "+zu.y+", zu.bottom: "+zu.yMax);
                if(r.y>2000)break;
            }
            while(r.yMin<zu.yMin){
                r.y+=r.height;
                Debug.Log("-Neue Höhe: "+r.y+", kopf: "+r.yMin+", zu.y: "+zu.y+", zu.kopt: "+zu.yMin);
                if(r.y<-2000)break;
            }
        }else{
            r.position=Vector2.Max(r.position,zu.position);
            if(preserveSize)r.position=Vector2.Min(r.position,zu.max-r.size);
            else r.max=Vector2.Min(r.max,zu.max);
        }
        return r;
    }
    public static Rect ConstrainToScreen(this Rect r,bool preserveSize=false,bool incremental=false){
        return r.ConstrainTo(new Rect(0,0,Screen.width,Screen.height),preserveSize,incremental);
    }
    public static bool ComboDown(KeyCode[] keys){
        if(keys==null)return false;
        for(int i=0;i<keys.Length;i++)if(new KeyCode[]{KeyCode.LeftShift,KeyCode.RightShift,KeyCode.LeftControl,KeyCode.RightControl,KeyCode.LeftAlt,KeyCode.RightAlt}.Contains(keys[i])) {
                if(!Input.GetKey(keys[i]))return false;
        }else if(!Input.GetKeyDown(keys[i]))return false;
        return true;
    }
    public static bool Intersect(Vector2 a1,Vector2 a2,Vector2 b1,Vector2 b2){
        if(a1.x>a2.x){
            Vector2 tmp=a1;
            a1=a2;
            a2=tmp;
        }
        if(b1.x>b2.x){
            Vector2 tmp=b1;
            b1=b2;
            b2=tmp;
        }
        Vector2 a=a2-a1,b=b1-b2,c=a1-b1;
        float alphaNumerator=b.y*c.x-b.x*c.y,
            alphaDenominator=a.y*b.x-a.x*b.y,
            betaNumerator=a.x*c.y-a.y*c.x,
            betaDenominator=alphaDenominator;/*2013/07/05,fixbyDeniz*/
        bool doIntersect=true;
        if(alphaDenominator==0||betaDenominator==0)doIntersect=false;
        else{
            if(alphaDenominator>0){
                if(alphaNumerator<0||alphaNumerator>alphaDenominator)doIntersect=false;
            }else if(alphaNumerator>0||alphaNumerator<alphaDenominator)doIntersect=false;
            if(doIntersect&&betaDenominator>0){
                if(betaNumerator<0||betaNumerator>betaDenominator)doIntersect=false;
            }else if(betaNumerator>0||betaNumerator<betaDenominator)doIntersect=false;
        }
        //Debug.Log("SRSUtilities.Intersect("+a1+","+a2+","+b1+","+b2+") alphaNumerator: "+alphaNumerator+", alphaDenominator: "+alphaDenominator+", betaNumerator: "+betaNumerator+", betaDenominator: "+betaDenominator+", doIntersect: "+doIntersect);
        return doIntersect;
    }
    public static string CapFirsts(string input){
        return Regex.Replace(input,@"(^\w|\s\w)",c=>c.Value.ToUpper());
    }
    public static Rect BoundingRect(Vector3[] v){
        if(v==null||v.Length<1)return new Rect(0,0,0,0);
        Rect r=new Rect(0,0,0,0);
        r.position=Camera.main.WorldToScreenPoint(v[0]);
        for(int i=0;i<v.Length;i++){
            r.min=Vector2.Min(r.min,Camera.main.WorldToScreenPoint(v[i]));
            r.max=Vector2.Max(r.max,Camera.main.WorldToScreenPoint(v[i]));
            //Debug.LogWarning("V["+i+"]: "+v[i]);
        }
        return r;
    }
    public static bool PointInTriangle(Vector2 p,Vector2 p0,Vector2 p1,Vector2 p2){
        float s=p0.y*p2.x-p0.x*p2.y+(p2.y-p0.y)*p.x+(p0.x-p2.x)*p.y;
        float t=p0.x*p1.y-p0.y*p1.x+(p0.y-p1.y)*p.x+(p1.x-p0.x)*p.y;
        if((s<0)!=(t<0))return false;
        float A=-p1.y*p2.x+p0.y*(p2.x-p1.x)+p0.x*(p1.y-p2.y)+p1.x*p2.y;
        if(A<0.0){
            s=-s;
            t=-t;
            A=-A;
        }
        return s>0&&t>0&&(s+t)<=A;
    }
    public static int Wrap(int i,int limit) {
        return (i%limit+limit)%limit;
    }
}