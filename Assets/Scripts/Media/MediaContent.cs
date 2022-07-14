using System;

namespace Media
{
	[Serializable]
	public class MediaContent
	{
		public UnityEngine.Object Content;
		public string Name;
		public string Path;
		public bool IsVideo;
	}
}