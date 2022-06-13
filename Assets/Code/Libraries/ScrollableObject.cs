using UnityEngine;
using System.Collections;
public class ScrollableObject:MonoBehaviour{
  public float[] bottom,top;
  public bool[] constraint;
  private Vector3 screenPoint;
  private Vector3 offset;
  public Camera guiCamera;
  public GameObject objectToMove;
  private bool mouseDown=false;
  private static float momentumStarterThreshold=10;
  private Vector2 momentum=Vector2.zero;
  private static Vector2 worldUnitToPixelRatios;
  private void Awake(){
    Debug.Log("ScrollingArea Awake on: "+name);
//    guiCamera=guiCamera??(GameObject.FindWithTag("GUICamera")!=null?GameObject.FindWithTag("GUICamera").camera:Camera.main);
    if(guiCamera==null)guiCamera=GameObject.FindWithTag("GUICamera")!=null?GameObject.FindWithTag("GUICamera").GetComponent<Camera>():Camera.main;
    if(guiCamera==null)Debug.LogError(name+" couldn't find the GUICamera. Object with tag: "+GameObject.FindWithTag("GUICamera")+", main: "+Camera.main.name);
    objectToMove=objectToMove??gameObject;
    for(int i=0;i<bottom.Length;i++)bottom[i]=Mathf.Min(objectToMove.transform.localPosition[i],top[i]);
    mouseDown=false;
    worldUnitToPixelRatios=new Vector2((guiCamera.aspect*guiCamera.orthographicSize*2)/Screen.width,(guiCamera.orthographicSize*2)/Screen.height);
    if(bottom.Length<3||top.Length<3||constraint.Length<3){
//      Debug.LogError("Aborting mouse drag, as bottom and top aren't set.");
      Debug.LogError(name+": Incomplete bottom ("+bottom.Length+"), top ("+top.Length+"), or constraint ("+constraint.Length+") settings.");
      return;
    }
//    Debug.Log("worldUnitToPixelRatios: ("+worldUnitToPixelRatios.x+","+worldUnitToPixelRatios.y+")");
  }
//  private void OnDown(){
  private void OnMouseDown(){
    MouseDown();
  }
  private void MouseDown(){
	Debug.Log("ScrollableObject.MouseDown(); Input.mousePosition: "+Input.mousePosition+", screen dimensions: ("+Screen.width+","+Screen.height+")");
    if(guiCamera==null)Debug.LogError("You need to assign guiCamera to object: "+name);
    screenPoint=guiCamera.WorldToScreenPoint(objectToMove.transform.localPosition);
    offset=objectToMove.transform.localPosition-guiCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,screenPoint.z));
    Debug.Log(name+" ScrollableObject.MouseDown(). Offset: "+offset+", CanMove(): "+CanMove());
    mouseDown=true;
    momentum=Vector2.zero;
  }
//  private void Update(){
  private void FixedUpdate(){
    if(mouseDown){
      //Debug.Log("Mouse is DOWN on: "+name);
      MouseDrag();
      if(!(mouseDown=Input.GetMouseButton(0)))MouseUp();
    }else objectToMove.transform.localPosition=new Vector3(Mathf.Clamp(objectToMove.transform.localPosition.x+(constraint[0]?0:(momentum.x*Time.deltaTime*worldUnitToPixelRatios.x)),bottom[0],top[0]),
                                                      objectToMove.transform.localPosition.y,
                                                           Mathf.Clamp(objectToMove.transform.localPosition.z+(constraint[2]?0:(momentum.y*Time.deltaTime*worldUnitToPixelRatios.y)),bottom[2],top[2]));
  }
  private void MouseUp(){
    //Debug.Log("MouseUp() on "+name+". delta: "+Input.mouseScrollDelta);
    if(Mathf.Abs(Input.mouseScrollDelta.y)>momentumStarterThreshold)momentum=Input.mouseScrollDelta;
  }
  private void MouseDrag(){
    //Debug.Log("MouseDrag()");
    Vector3 newPoint=guiCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y,screenPoint.z))+offset;
    objectToMove.transform.localPosition=new Vector3(constraint[0]?objectToMove.transform.localPosition.x:Mathf.Clamp(newPoint.x,bottom[0],top[0]),
                                                     constraint[1]?objectToMove.transform.localPosition.y:Mathf.Clamp(newPoint.y,bottom[1],top[1]),
                                                     constraint[2]?objectToMove.transform.localPosition.z:Mathf.Clamp(newPoint.z,bottom[2],top[2]));
  }
  public bool CanMove(){
    for(int i=0;i<constraint.Length;i++)if(!constraint[i]&&bottom[i]<top[i]){
      Debug.Log("CanMove(): "+name+" can move on axis #"+i+" (bottom: "+bottom[i]+", top: "+top[i]+").");
      return true;
    }
    Debug.Log("CanMove(): "+name+" cannot.");
    return false;
  }
  public void Stop(){
    momentum=Vector2.zero;
  }
}