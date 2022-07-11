using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;
using System.Net;
using ContourEditorTool;
using Configs;
using UnityEngine.Networking;
using UnityEngine.Video;
using VideoPlaying;
using Debug = UnityEngine.Debug;

public class Menu : MonoBehaviour
{
	public static bool limbo;
	public static int heartbeatTriesRemaining = Settings.allowConnectionAttempts;
	

	private static bool standbyForCommands = true;
	private static Vector2 windowDragOffset = -Vector2.one;
	private static string[] screenNames = { "gantry", "wall", "gantrywall" };
	public static bool _drawUI;

	public GameObject menuBackground;
	public GUISkin gantrySkin;
	public Texture2D illuminationsHeader, categoryFooter, mediaFooter, backArrow, blankScreen, adminButton;
	public static Rect windowPosition;

	public static bool DraggingWindow
	{
		get => windowDragOffset != -Vector2.one;
		set
		{
			windowDragOffset = value ? (Vector2)SRSUtilities.adjustedFlipped - windowPosition.position : -Vector2.one;
			Debug.Log("windowDragOffset set to: " + windowDragOffset + " (value: " + value + ")");
		}
	}

	private void Awake()
	{
#if UNITY_EDITOR
		//Nate: Added this so saving and video playback works on my local machine. 

		// Settings.libraryDir = Settings.GetVideoFolder();
		Settings.appDir = Application.persistentDataPath;
#endif
		foreach (Func<IEnumerator> f in new Func<IEnumerator>[] { UpdateCheck}) StartCoroutine(f());
		Settings.Load();
		
		//Settings.dongleKeys=new string[]{"DE2E19C984E2925D","D85D6EA1539B7493"};

		
		var categoryTextures = Resources.LoadAll<Texture2D>("categories");
		var catList = new List<KeyValuePair<GUIContent, Action>>();

		Settings.monitorMode = Settings.MonitorMode.Single;
		Debug.Log("Fullscreen was: " + Screen.fullScreen);
		Screen.fullScreen = false;
	}

	public static void ResetWindowPosition()
	{
		Debug.Log("Menu.ResetWindowPosition() Settings.menuScreenW: " + Settings.ScreenW + ", saveWindowSize.x: " +
		          Settings.saveWindowSize.x);

		windowPosition = new Rect(Settings.ScreenW * 0.5f - Settings.saveWindowSize.x * 0.5f,
			Screen.height * 0.5f - Settings.saveWindowSize.y * 0.5f, Settings.saveWindowSize.x, Settings.saveWindowSize.y);
	}

	private IEnumerator UpdateCheck()
	{
		//Security feature
		//Settings.version=1;//TMP
		while (true)
		{
			if (!Settings.serverCheck)
			{
				Debug.Log("---Security checks off at " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
				          "; silently looping.---");
				yield return new WaitForSeconds(Settings.updatePeriod);
				continue;
			}

			Debug.Log("--------------------------------------\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") +
			          " Menu.UpdateCheck() connecting to: " + Settings.heartbeatServerAddress);
			WWWForm form = new WWWForm();
			form.AddField("l", Settings.clientLogin);
			form.AddField("version", Settings.version);
			WWW w = new WWW(Settings.heartbeatServerAddress, form.data);
			yield return w;
			string ergebnis = w.text.Split("\n"[0])[0];
			Debug.Log("Server returned with:\n" + w.text + "\n\nergebnis: \"" + ergebnis +
			          "\"\ncomparison: \"Gesetzlich\", fehler: \"" + w.error + "\"\n\n");
			string /*int*/[] unlocked = null;
			int serverVersion;
			if (!ergebnis.StartsWith("Gesetzlich") || !string.IsNullOrEmpty(w.error) ||
			    !int.TryParse(w.text.Split("\n"[0])[1], out serverVersion) || (w.text.Split("\n"[0]).Length > 2 &&
			                                                                   (unlocked = w.text.Split("\n"[0])[2]
				                                                                   .Split(","[
					                                                                   0]) /*.Select(n=>Convert.ToInt32(n))*/
				                                                                   .ToArray()) == null))
			{
				Debug.LogWarning("Fehler mit verbindung. Ergebnis: \"" + ergebnis + "\"; error: \"" + w.error +
				                 "\", tries remaining: " + heartbeatTriesRemaining + "\n\n");
				if (--heartbeatTriesRemaining <= 0)
				{
					Application.Quit();
					yield break;
				}
			}
			else
			{
				heartbeatTriesRemaining = Settings.allowConnectionAttempts;
				Debug.Log(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " Server version: " + serverVersion +
				          ", our version: " + Settings.version + ", neuer: " + (serverVersion > Settings.version) +
				          ", unlocked: \"" + unlocked.Stringify() + "\"\n--------------------------------------\n");
				if (serverVersion > Settings.version) StartCoroutine(UpdateVersion(serverVersion));
				for (int i = 0; i < unlocked.Length; i++)
				{
					//Each is in format: 1-char BGMW bitfield, id, media extension abbreviation, thumb extension abbreviation.
					bool[] bgmw = new bool[4];
					for (int j = 0; j < bgmw.Length; j++)
						bgmw[j] = Convert.ToBoolean(Convert.ToInt32(unlocked[i].Substring(0, 1), 16));
					string ext = Regex.Replace(unlocked[i], @"^.\d+([^\d])\w$", "$1"),
						fn = Settings.libraryDir + SRSUtilities.slashChar +
						     Regex.Replace(unlocked[i], @"^.(\d+)[A-Za-z]+$", "$1") + "." +
						     (new string[] { "jpg", "png", "ogg" }.First(s => s.Substring(0, 1) == ext));
					if (!File.Exists(fn))
					{
						Debug.Log("Downloading \"" + Settings.dlServeraddress + "?dl=movies/" + Path.GetFileName(fn) +
						          "\" to \"" + fn + "\".");
						new WebClient().DownloadFileAsync(
							new Uri(Settings.dlServeraddress + "?dl=movies/" + Path.GetFileName(fn)), fn);
					}
					else Debug.Log("\"" + fn + "\" exists.");

					ext = Regex.Replace(unlocked[i], @"^.\d+[^\d](\w)$", "$1");
					fn = Settings.thumbsDir + SRSUtilities.slashChar +
					     Regex.Replace(unlocked[i], @"^.(\d+)[A-Za-z]+$", "$1") + "." +
					     (new string[] { "jpg", "png", "ogg" }.First(s => s.Substring(0, 1) == ext));
					if (!File.Exists(fn))
					{
						Debug.Log("Downloading \"" + Settings.dlServeraddress + "?dl=thumbs/" + Path.GetFileName(fn) +
						          "\" to \"" + fn +
						          "\"."); //Debug.Log("Downloading \""+Settings.dlServeraddress+"?dl="+Path.GetFileName(fn)+"&thumb"+"\" to \""+fn+"\".");
						new WebClient().DownloadFileAsync(
							new Uri(Settings.dlServeraddress + "?dl=thumbs/" + Path.GetFileName(fn)),
							fn); //new WebClient().DownloadFileAsync(new Uri(Settings.dlServeraddress+"?dl="+Path.GetFileName(fn)+"&thumb"),fn);
					}

					Debug.Log("\"" + fn + "\" exists.");
				}

				yield return new WaitForSeconds(Settings.updatePeriod);
			}
		}
	}

	private static IEnumerator UpdateVersion(int toVersion)
	{
		Debug.Log("Menu.Update(" + toVersion + ")");

		var bkpPath = Settings.binaryPath + ".bkp";

		while (File.Exists(bkpPath)) bkpPath += "I";
		//PlayerPrefs.SetInt("Version",toVersion);
		Settings.version = toVersion;
		//Process.Start(Settings.binaryPath);
		var w = new UnityWebRequest(Settings.newBinaryURL);

		yield return w.SendWebRequest();

		Debug.Log("Downloaded. Moving: " + Settings.binaryPath + " to " + bkpPath);

		File.Move(Settings.binaryPath, bkpPath);
		File.WriteAllBytes(Settings.binaryPath, w.downloadHandler.data);
		Process.Start(Settings.binaryPath);
		Application.Quit();
	}

	public static IEnumerator ReportAndQuit(string server, string msg)
	{
		Debug.Log("Menu.ReportAndQuit(" + server + "," + msg + ")");

		var form = new WWWForm();
		form.AddField("msg", msg);
		form.AddField("l", Settings.clientLogin);
		WWW w = new WWW(server, form.data);
		yield return w;
		string ergebnis = w.text.Split("\n"[0])[0];
		Debug.Log("Server \"" + server + "\" returned with:\n" + w.text + "\n\nergebnis: \"" + ergebnis +
		          "\"\nerror: \"" + w.error + "\"\n");
		Application.Quit();
	}
}
