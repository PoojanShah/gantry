using UnityEngine;
using UnityEngine.Video;
 
public class PlayRuntime : MonoBehaviour
{
	private VideoPlayer MyVideoPlayer;

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			MyVideoPlayer = GetComponent<VideoPlayer>();
			// play video player
			MyVideoPlayer.Play();
		}
	}
}