using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Net;
using System.Net.Sockets;
using System.Linq;
using Core;
public static class SRSUtilities:System.Object
{
    public static bool StretchedButtonLabel(Rect r, string text, GUIStyle style, float stretchBy)
    {
        bool ergebnis = GUI.Button(r, "", style);
        Matrix4x4 matrixBackup = GUI.matrix;
        Texture2D normal = style.normal.background, hover = style.hover.background, active = style.active.background;
        TextAnchor alignment = style.alignment;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.background = style.hover.background = style.active.background = null;
        GUI.matrix *= Matrix4x4.Scale(new Vector3(stretchBy, 1, 1));
        //        GUI.Label(new Rect(r.x+r.width*0.5f,r.y+r.height*0.5f,r.width,r.height),text,darkAgesSkin.customStyles[8]);
        //        GUI.Label(r,text,darkAgesSkin.customStyles[8]);
        //        GUI.Label(new Rect(r.x/stretchBy,r.y,r.width/stretchBy,r.height),text,darkAgesSkin.customStyles[8]);
        GUI.Label(new Rect(r.x / stretchBy, r.y, r.width / stretchBy, r.height), text, style);
        GUI.matrix = matrixBackup;
        style.normal.background = normal;
        style.hover.background = hover;
        style.active.background = active;
        style.alignment = alignment;
        return ergebnis;
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
		Vector2 pRelative=new Vector2(p.x-r.center.x,p.y-r.center.y),radii=new Vector2(r.width*0.5f,r.height*0.5f);
		return (pRelative.x*pRelative.x)/(radii.x*radii.x)+(pRelative.y*pRelative.y)/(radii.y*radii.y)<=1;
	}
	public static Rect RectAround(Vector2 v1,Vector2 v2){
		return new Rect(v1.x,v1.y,v2.x-v1.x,v2.y-v1.y);
	}
	
	
	public static void RemoveSet(this List<int> l,int[] set){
		foreach(int i in set)l.Remove(i);
	}
	public static float Lerp(float from,float to,float lerp){
		return from+(to-from)*lerp;
	}
	public static Vector2 adjustedMousePosition{
		get{return (Vector2)Input.mousePosition;}
	}
	public static Vector2 adjustedFlipped{
        get{return adjustedMousePosition.FlipY();}
 
    }

    public static Vector3 Midpoint(Vector3[] vects){
        Vector3 min=vects[0],max=vects[0];
        for(int v=1;v<vects.Length;v++){
            min=Vector3.Min(vects[v],vects[v-1]);
            max=Vector3.Max(vects[v],vects[v-1]);
        }
        return Vector3.Lerp(min,max,0.5f);
    }

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
   
    public static object KeyOfIndex(this OrderedDictionary od,int i,bool wrap=false){
        if(wrap)i=(i%od.Count+od.Count)%od.Count;
        return i>od.Count-1||i<0?null:od.Cast<DictionaryEntry>().ElementAt(i).Key;
    }
    public static Rect ConstrainTo(this Rect r,Rect zu,bool preserveSize=false,bool incremental=false){
        if(incremental){
            while(r.xMin<zu.xMin)r.x+=r.width;
            while(r.xMax>zu.xMax)r.x-=r.width;
            
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
        return doIntersect;
    }
    public static string CapFirsts(string input){
        return Regex.Replace(input,@"(^\w|\s\w)",c=>c.Value.ToUpper());
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