using UnityEngine;
using System.Collections.Generic;


public abstract class Draggable2D:MonoBehaviour{//Note: set this to execute first in Update, so that other scripts can accurately check draggingAnything.
	public float titleBarHeight=16;
    public bool constrainToScreen=false;
	//protected Rect rect;
	public Rect rect;
	public bool dragging{
        get{return draggingItems.Contains(this);}
        set{if(value)draggingItems.AddUnique(this);else draggingItems.Remove(this);}
    }
    public static bool draggingAnything{get{return draggingItems.Count>0;}}
    public static bool mouseOverAny{get{
            foreach(Draggable2D d in Resources.FindObjectsOfTypeAll(typeof(Draggable2D)))if(d.rect.Contains(SRSUtilities.adjustedMousePosition))return true;
            return false;
        }}
    public static List<Draggable2D> draggingItems{
        get{
            _draggingItems=_draggingItems??new List<Draggable2D>();
            return _draggingItems;
        }
    }
    public static List<Draggable2D> _draggingItems;
    private Vector2 startPoint,offset;//where the rect started the drag, and the offset of the mouse cursor from the top-left corner.
    public bool TryMouseDown(Vector2 p){
		//if(!dragging&&dragging=new Rect(rect.x,rect.y,rect.width,titleBarHeight).Contains(p)){
		if(dragging=new Rect(rect.x,rect.y,rect.width,titleBarHeight).Contains(p)){
            offset=(Vector2)(p-rect.position);
            //draggingItems.Add(this);
        }
        Debug.Log("Draggable2D.TryMouseDown("+p+"), rect: "+rect+", ergebnis: "+dragging);
        return dragging;
    }
    public virtual void Update(){
        if(dragging)
            if(Input.GetMouseButtonUp(0))dragging=false;
			else{
                rect.position=-offset+(Vector2)SRSUtilities.adjustedFlipped;
                if(constrainToScreen)rect=rect.ConstrainToScreen(true);
            }
        else if(Input.GetMouseButtonDown(0))TryMouseDown(SRSUtilities.adjustedFlipped);
    }
}
public static class Graphics{
	public static Texture2D schwarz1x1{
		get{
			if(_schwarz1x1==null){
				_schwarz1x1=new Texture2D(1,1);
				_schwarz1x1.SetPixel(0,0,new Color(0,0,0,1));
//				_schwarz1x1.SetPixel(0,0,new Color(0,0,0,0.5f));
				_schwarz1x1.Apply();
			}
//			Debug.Log("GetPixel: "+_schwarz1x1.GetPixel(0,0));
			return _schwarz1x1;//=new Texture2D(1,1);
		}
	}
	public static Texture2D weiss1x1{
		get{
			if(_weiss1x1==null){
				_weiss1x1=new Texture2D(1,1);
				_weiss1x1.SetPixel(0,0,Color.white);
				_weiss1x1.Apply();
			}
			//			Debug.Log("GetPixel: "+_weiss1x1.GetPixel(0,0));
			return _weiss1x1;//=new Texture2D(1,1);
		}
	}
	public static Texture2D filledEllipse{
		get{
			if(_filledEllipse==null){
				int ellipseTexSize=256;
				_filledEllipse=new Texture2D(ellipseTexSize,ellipseTexSize);
				for(int i=0;i<ellipseTexSize*ellipseTexSize;i++)_filledEllipse.SetPixel(i%ellipseTexSize,i/ellipseTexSize,Color.clear);
				Circle(_filledEllipse,-1,-1,-1,Color.white);
				_filledEllipse.Apply();
			}
			return _filledEllipse;
		}
	}
	private static Texture2D _schwarz1x1,_weiss1x1,_filledEllipse;
    static public bool OutlinedButton(Rect r,string t,int strength,GUIStyle buttonStyle,GUIStyle labelStyle,Color outlineFarbe=default(Color)){
        bool ergebnis=GUI.Button(r,"",buttonStyle);
        OutlinedLabel(r,t,strength,labelStyle,outlineFarbe);
        return ergebnis;
    }
    static public bool OutlinedStretchedButton(Rect r,string t,int strength,GUIStyle buttonStyle,GUIStyle labelStyle,float stretchBy=1,Color outlineFarbe=default(Color)){
        bool ergebnis=GUI.Button(r,"",buttonStyle);
        OutlinedStretchedLabel(r,t,strength,labelStyle,stretchBy,outlineFarbe);
        return ergebnis;
    }
    static public void OutlinedLabel(Rect r,string t,int strength,GUIStyle style,Color outlineFarbe=default(Color)){
        OutlinedStretchedLabel(r,t,strength,style,1,outlineFarbe);
    }
//    static public void OutlinedLabel(Rect r,string t,int strength,GUIStyle style,Color outlineFarbe=default(Color)){
//        Color colorBackup=GUI.color;
//        GUI.color=outlineFarbe==default(Color)?Color.black:outlineFarbe;//new Color(0,0,0,1);
//        for(int i=-strength;i<=strength;i++)if(i!=0){
//            GUI.Label(new Rect(r.x-strength,r.y+i,r.width,r.height),t,style);
//            GUI.Label(new Rect(r.x+strength,r.y+i,r.width,r.height),t,style);
//        }
//        for(int i=-strength+1;i<=strength-1;i++)if(i!=0){
//            GUI.Label(new Rect(r.x+i,r.y-strength,r.width,r.height),t,style);
//            GUI.Label(new Rect(r.x+i,r.y+strength,r.width,r.height),t,style);
//        }
//        GUI.color=colorBackup;
//        GUI.Label(r,t,style);
//    }
    static public void OutlinedStretchedLabel(Rect r,string t,int strength,GUIStyle style,float stretchBy=1,Color outlineFarbe=default(Color)){
        Color colorBackup=GUI.color;
        GUI.color=outlineFarbe==default(Color)?Color.black:outlineFarbe;//new Color(0,0,0,1);
        for(int i=-strength;i<=strength;i++)if(i!=0){
            SRSUtilities.StretchedButtonLabel(new Rect(r.x-strength,r.y+i,r.width,r.height),t,style,stretchBy);
            SRSUtilities.StretchedButtonLabel(new Rect(r.x+strength,r.y+i,r.width,r.height),t,style,stretchBy);
        }
        for(int i=-strength+1;i<=strength-1;i++)if(i!=0){
            SRSUtilities.StretchedButtonLabel(new Rect(r.x+i,r.y-strength,r.width,r.height),t,style,stretchBy);
            SRSUtilities.StretchedButtonLabel(new Rect(r.x+i,r.y+strength,r.width,r.height),t,style,stretchBy);
        }
        GUI.color=colorBackup;
        SRSUtilities.StretchedButtonLabel(r,t,style,stretchBy);
    }
	public static void DrawRect(Rect r,Color farbe=default(Color)){
		Color backupFarbe=GUI.color;
		if(farbe!=default(Color))GUI.color=farbe;
		GUI.DrawTexture(new Rect(r.x-1,r.y-1,r.width+2,3),weiss1x1);//Kopf
		GUI.DrawTexture(new Rect(r.x-1,r.y-1,3,r.height+2),weiss1x1);//Links
		GUI.DrawTexture(new Rect(r.x-1,r.y+r.height-1,r.width+2,3),weiss1x1);//Tief
		GUI.DrawTexture(new Rect(r.x+r.width-1,r.y-1,3,r.height+2),weiss1x1);//Rechts
		GUI.color=backupFarbe;
	}
	public static void DrawRectAround(Vector2 v1,Vector2 v2,Color farbe=default(Color)){
		DrawRect(SRSUtilities.RectAround(v1,v2),farbe);
		//		Color backupFarbe=GUI.color;
		//		if(farbe!=default(Color))GUI.color=farbe;
		//		GUI.DrawTexture(new Rect(v1.x-1,v1.y-1,v2.x-v1.x+2,3),schwarz1x1);//Kopf
		//		GUI.DrawTexture(new Rect(v1.x-1,v1.y-1,3,v2.y-v1.y+2),schwarz1x1);//Links
		//		GUI.DrawTexture(new Rect(v1.x-1,v2.y-1,v2.x-v1.x+2,3),schwarz1x1);//Tief
		//		GUI.DrawTexture(new Rect(v2.x-1,v1.y-1,3,v2.y-v1.y+2),schwarz1x1);//Rechts
		//		GUI.color=backupFarbe;
	}
	public static void DrawBox(Rect r,Color c=default(Color)){
		DrawColoredTexture(r,weiss1x1,c);
	}
	public static void DrawFilledEllipse(Rect r,Color c=default(Color)){
		DrawColoredTexture(r,filledEllipse,c);
	}
	public static void DrawColoredTexture(Rect r,Texture2D tex,Color c=default(Color)){
		Color colorBackup=GUI.color;
		if(c!=default(Color))GUI.color=c;
		GUI.DrawTexture(r,tex);
		GUI.color=colorBackup;
	}
	public static void Circle(Texture2D tex,int cx=-1,int cy=-1,int r=-1,Color col=default(Color)){
		int x,y,px,nx,py,ny,d;
		if(cx<0)cx=tex.width/2;
		if(cy<0)cy=tex.height/2;
		if(r<0)r=tex.width/2-1;
		for(x=0;x<=r;x++){
			d=(int)Mathf.Ceil(Mathf.Sqrt(r*r-x*x));
			for(y=0;y<=d;y++){
				px=cx+x;
				nx=cx-x;
				py=cy+y;
				ny=cy-y;
				tex.SetPixel(px,py,col);
				tex.SetPixel(nx,py,col);
				tex.SetPixel(px,ny,col);
				tex.SetPixel(nx,ny,col);
			}
		}    
	}
	public static Texture2D ByName(this Texture2D[] textures,string name){
		for(int i=0;i<textures.Length;i++)if(textures[i].name==name)return textures[i];
		return null;
	}
	public static void ConvertColor(this Texture2D tex,Color from,Color to){
//		for(int x=0;x<tex.width;x++)for(int y=0;y<tex.height;y++)if(tex.GetPixel(x,y)==from)tex.SetPixel(x,y,to);
		for(int x=0;x<tex.width;x++)for(int y=0;y<tex.height;y++)tex.SetPixel(x,y,tex.GetPixel(x,y)==from?to:tex.GetPixel(x,y));
		tex.Apply();
	}
    public static void SetAlpha(Material m,float a){
        m.color=new Color(m.color.r,m.color.g,m.color.b,a);
    }
    public static void DrawCursorIcon(Texture2D icon){
        //GUI.DrawTexture(new Rect(SRSUtilities.adjustedFlipped.x,SRSUtilities.adjustedFlipped.y,icon.width,icon.height),icon);
        GUI.DrawTexture(new Rect(SRSUtilities.adjustedFlipped.x,SRSUtilities.adjustedFlipped.y,32,32),icon);
    }
    //public static Texture2D HighlightCircle(int radius=32){
    //    Texture2D tex=new Texture2D(radius,radius);
    //    tex.SetPixels(0,0,radius,radius,new Color[]{Color.white});
    //    int[] arc=SRSUtilities.ArcPattern(radius);
    //    int v;
    //    for(int i=0;i<arc.Length;i++){
    //        //v=columns/2*(columns+1)+i+arc[i]*columns;
    //        tex.SetPixel(i,arc[i],Color.white);
    //        //if(WithinBounds(v))neue.AddUnique(v);
    //        //else Debug.LogWarning("Throwing out vertex "+v+" from the circle algorithm.");
    //    }
    //    //for(int y=arc[arc.Length-1];y>=0;y--) if(WithinBounds(v=columns/2*(columns+1)+(arc.Length-1)+y*columns)) neue.AddUnique(v);//Fill in the gap on the x axis.
    //    return tex;
    //}
    public static bool Toggle(Rect r,bool curValue,string label,Vector2 buttonSize=default(Vector2),float margin=8,GUIStyle style=null){//Makes the label's area precisely clickable.
        if(buttonSize==default(Vector2))buttonSize=Vector2.one*32;
        style=style??GUI.skin.toggle;
        r.width=style.CalcSize(new GUIContent(label)).x;//+margin+buttonSize.x;
        int overflowRightWas=style.overflow.right;
        style.overflow.right=(int)(-r.width+buttonSize.x);
        bool ergeb=GUI.Toggle(r,curValue,label,style);
        style.overflow.right=overflowRightWas;
        return ergeb;
    }
}