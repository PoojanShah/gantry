using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ContourEditorTool;

public class Toolbar : Draggable2D
{
    public static string tooltip = "", toolMessage = "";
    public static Vector2 defaultButtonSize = Vector2.one * 32, defaultPosition = Vector2.zero;
    public static float defaultButtonMargin = 4;
    public float menuMargin = 8;
    public static bool clickedThisFrame = false;
    private bool vertical = true;//,direction=true;//Getue
    public bool isNeedToShow;
    public InfoDisplay info;
    public static GUISkin gantrySkin;
    private static Rect tooltipRect = new Rect(Screen.width * 0.5f - 128, Screen.height - 64, 256, 32);
    private void Awake()
    {
        gantrySkin = Resources.Load<GUISkin>("Gantry Skin");
        info = GetComponent<InfoDisplay>();
        
        isNeedToShow = false;
        info.isNeedToShow = false;
    }
    private void Start()
    {
        menus[0].SelectItem(0, 0);
    }
    public void ToggleInfo()
    {
        Debug.Log("Toolbar.ToggleInfo(), enabled previously: " + info.enabled);
        info.enabled = !info.enabled;
    }
    public void SetMenu(ToolbarMenu m)
    {
        Debug.Log("Toolar.SetMenu(" + m + ")");
        SetMenu(new ToolbarMenu[] { m });
    }
    public void SetMenu(ToolbarMenu[] m)
    {
        menus = m;
        rect.size = new Vector2(menuMargin, titleBarHeight + menuMargin);
        //for(int i=0;i<menus.Length;i++)rect.size+=menus[i].Size(vertical)[vertical?1:0];
        for (int i = 0; i < menus.Length; i++)
        {
            Vector2 menuSize = menus[i].Size(vertical);
            Debug.Log(i + " menuSize: " + menuSize + ",rect.size:" + rect.size);
            rect.size = new Vector2(vertical ? Mathf.Max(rect.size.x, menuSize.x + menuMargin * 2) : rect.size.x + menuSize.x + menuMargin, vertical ? rect.size.y + menuSize.y + menuMargin : Mathf.Max(rect.size.y, menuSize.y + menuMargin * 2 + titleBarHeight));
        }
        Debug.Log("Toolar.SetMenu([" + m.Length + "]), rect.size: " + rect.size);
    }
    public void Reset()
    {
        Debug.Log("Toolar.Reset()");
        //rect=new Rect(0,0,buttonSize*2+buttonMargin*2,(buttonSize+buttonMargin)*9.5f+titleBarHeight);
        rect.position = defaultPosition;
        for (int i = 0; i < menus.Length; i++) menus[i].Reset();
    }
    
    public ToolbarMenu[] menus;
    public static void DrawTooltip(string tip)
    {
        //GUI.color=Color.white;
        //GUI.Box(tooltipRect,Graphics.schwarz1x1);
        tooltipRect.position = SRSUtilities.adjustedFlipped + Vector2.one * 16;
        tooltipRect.size = gantrySkin.customStyles[4].CalcSize(new GUIContent(tip)) + new Vector2(16, 4);
        GUI.DrawTexture(tooltipRect, Graphics.schwarz1x1);
        GUI.Label(tooltipRect, tip, gantrySkin.customStyles[4]);
    }
    //public int[] selectedCategory{
    //    get{
    //        List<int>
    //        return menus;
    //    }
    //}
    //public int[][] selectedSubitems;
    public bool Contains(Vector2 p)
    {
        return menus != null && menus.Length > 0 && menus.Any(m => m.unfurled) || rect.Contains(p);
    }
    // public bool Contains(Vector2 p){
    //     for(int m=0;m<menus.Length;m++)if(menus[m].unfurled){
    //         Rect r=new Rect(p.x+(vertical?buttonSize.x*j*(direction?1:-1)+anfang:0),
    //for(int j=0;j<items[menus[m].selectedSubitems].Length);j++){
    //                         p.y+(!vertical?buttonSize.y*j*(direction?1:-1)+anfang:0),buttonSize.x,buttonSize.y))
    //             }
    //         }
    //     }
    //     return rect.Contains(p);
    // }
    public void OnGUI()
    {
        if (!isNeedToShow)
            return;
        
        GUI.depth = -1;
        GUI.skin = gantrySkin;
        GUI.color = Color.white;
        tooltip = "";
        clickedThisFrame = false;
        if (!ContourEditor.HideOldUI)
        {
            GUI.Box(rect, "Tools", gantrySkin.window);
            Vector2 p = rect.position + new Vector2(menuMargin, titleBarHeight + menuMargin);
            
            if (menus.Length > 0)
                for (int m = 0; m < menus.Length; m++)
                {
                    //We draw content outside the window function to allow for overflow.
                    menus[m].Draw(p, vertical);
                    p[vertical ? 1 : 0] += menus[m].Size(vertical)[vertical ? 1 : 0] + menuMargin;
                }
        }

        GUI.enabled = true;
        //Debug.Log("tooltip now: "+GUI.tooltip+", Toolbar.tooltip: "+tooltip);
        //info.message=tooltip!=""?tooltip:toolMessage;
        if (tooltip != "")
        {
            info.message = tooltip;
            Debug.Log("Set message to tooltip: \"" + tooltip + "\"");
            tooltip = "";
        }
        else if (toolMessage != "")
        {
            info.message = toolMessage;
            Debug.Log("Set message to toolMessage: \"" + toolMessage + "\", length: " + toolMessage.Length);
            toolMessage = "";
        }
        //if(infoOn)DrawInfo();
    }
    public override void Update()
    {
        base.Update();
        for (int m = 0; m < menus.Length; m++) menus[m].CheckShortcuts();
    }
}