using UnityEngine;

public class PreviewButton : MonoBehaviour
{
	private string _videoName;

	public void Init(string videoName) => _videoName = (videoName);

	private void OnMouseDown()
	{
		transform.parent.SendMessage("DestroyPreviews");

		Menu.instance.projection.gameObject.SetActive(true);

		Projection.instance.StartMovie();
	}
}
