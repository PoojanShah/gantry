using UnityEngine;
using System.Collections;
public class PreviewButton:MonoBehaviour{
//    public string movieFile;
//    public int movieFile;
	public string movieName;
	private void OnMouseDown(){
		Debug.Log("PreviewButton.OnMouseDown() on "+name+". Projection: "+GameObject.Find("Projection"));
		transform.parent.SendMessage("DestroyPreviews");
//        Menu.StartMovie(movieFile,true);
        Menu.instance.projection.gameObject.SetActive(true);
        Projection.instance.StartMovie(movieName);
	}
}
