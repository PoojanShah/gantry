using UnityEngine;
using UnityEngine.Video;
using Object = UnityEngine.Object;

namespace Configs
{
	[CreateAssetMenu(fileName = "MediaConfig", menuName = "Configs/Media config")]
	public class MediaConfig : ScriptableObject
	{
		public GameObject MediaItemPrefab;
		public Object[] MediaFiles;

		public VideoClip GetFirstClip() => MediaFiles[0] as VideoClip;
	}
}
