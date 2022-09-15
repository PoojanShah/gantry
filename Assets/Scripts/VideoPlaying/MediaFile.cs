using System;
using UnityEngine;

namespace VideoPlaying
{
	[Serializable]
	public class MediaFile
	{
		public string thumbnail;
		public string media;
	}

	[Serializable]
	public class MediaFilesList
	{
		public MediaFile[] MediaFiles;
	}

	public class MediaFilesHelper
	{
		public MediaFilesList GetList(string content)
		{
			var result = JsonUtility.FromJson<MediaFilesList>("{\"MediaFiles\":" + content + "}");

			return result;
		}
	}
}