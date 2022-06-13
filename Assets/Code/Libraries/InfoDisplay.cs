using UnityEngine;
using System.Collections;
public class InfoDisplay:Draggable2D{
    public string message="";
    private void OnGUI(){
        //Debug.Log("InfoDisplay.OnGUI(), message: \""+message+"\"");
        //Rect hudRect=new Rect(0,Screen.height-216,160,216);
        //Rect hudRect=new Rect(0,0,Screen.width,Screen.height);
        GUI.color=Color.white;
        GUI.DrawTexture(rect,Graphics.schwarz1x1);
        GUI.color=Color.red;
        GUI.Label(rect,message);
        //Debug.Log("Bla.");
        //GUI.Label(hudRect,
        //        "Mirror (X/Y) ["+(mirror[0] ? "X" : " ")+"] ["+(mirror[1] ? "Y" : " ")+"]\n"+
        //        "[C]ircle Mode: "+(circle ? "On" : "Off")+"\n"+
        //        "[D]elete\n"+
        //        "Z: Undo\n"+
        //        "Shift-Z: Redo\n"+
        //        "+/-: Adjust Resolution\n"+
        //        "E: Elliptical Selection\n"+
        //        "R: Rectangular Selection\n"+
        //        "B: Change Background\n"+
        //        "S: Save\n"+
        //        "L: Load\n"+
        //        "I: Info\n"+
        //        "Ctrl: Scale\n"+
        //        "Alt: Draw Blackouts\n"
        //);
    }
}