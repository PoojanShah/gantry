using System;
using UnityEngine;

namespace VideoPlaying
{
	[Serializable]
	public class MediaFile
	{
		public int id;
		public string title;
		public string thumbnail;
		public string media;
		public int sort;
		public string created_at;
		public string updated_at;
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
			Debug.Log(result);
			return result;
		}
	}
}