using System;

namespace Media
{
	[Serializable]
	public class MediaContent
	{
		public int Id;
		public string Name;
		public string Path;
		public bool IsVideo;
	}
}