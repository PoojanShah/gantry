using UnityEngine;
using System.Collections;
public static class SRSGraphics{
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
}
