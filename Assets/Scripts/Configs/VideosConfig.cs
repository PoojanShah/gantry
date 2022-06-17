using UnityEngine;
using UnityEngine.Video;

namespace Configs
{
	[CreateAssetMenu(fileName = "VideosConfig", menuName = "Configs/Videos config")]
	public class VideosConfig : ScriptableObject
	{
		public VideoClip[] Videos;

		public VideoClip GetFirstClip() => Videos[0];
	}
}
