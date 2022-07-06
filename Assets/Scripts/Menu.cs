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
	private Action displayerWas;
	private static bool wrongPass, passPrompt;
	private static Vector2 windowDragOffset = -Vector2.one;
	private static Vector2 libraryScroll = Vector2.zero;
	private static Texture2D[] thumbs;
	private static bool showExtensions;
	private static KeyValuePair<GUIContent, Action>[] menu;
	private static float backButtonMargin = 16;
	private static bool loadingMovie = false;
	private static string[] screenNames = { "gantry", "wall", "gantrywall" };
	public static bool _drawUI;

	public GameObject UIObject, QuitConfirmation, AdminLogin, AdminMenu;
	[SerializeField]private Projection _projection;
	[SerializeField] private ContourEditor _contourEditor;
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

	private static Rect BackButtonRect => new Rect(backButtonMargin, Settings.ScreenH - 56 - backButtonMargin, 96, 56);

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

		
		(_projection = _projection ?? FindObjectOfType<Projection>()).gameObject.SetActive(false);

		var categoryTextures = Resources.LoadAll<Texture2D>("categories");
		var catList = new List<KeyValuePair<GUIContent, Action>>();


		//categoryMenu = catList.ToArray();

		//Nate: I'm guessing this is here to call the awake function for Projection.
		_projection.gameObject.SetActive(true);
		_projection.gameObject.SetActive(false);
		StartCoroutine(CheckForCommands());

		Settings.monitorMode = Settings.MonitorMode.Single;
		Debug.Log("Fullscreen was: " + Screen.fullScreen);
		Screen.fullScreen = false;

		bool onStartRan = false;

		Debug.Log("onStartRan: " + onStartRan);

		if (!onStartRan) SetMenu();
	}

	private void OnGUI()
	{
		if (!_drawUI)
			return;

		GUI.skin = gantrySkin;

		Settings.NormalizeGUIMatrix();
	}

	private void Update()
	{
		if (_projection.IsPlaying && Input.GetKeyDown(KeyCode.Escape))
		{

			ContourEditor.WipeBlackouts();
			//SetMenu(categoryMenu);
			_projection.IsPlayMode = false;
			UIObject?.SetActive(true);

			menuBackground.SetActive(false);
			DestroyPreviews();
			_projection.gameObject.SetActive(false);
			Camera.main.transform.Find("Scrolling Background").gameObject.SetActive(true);
			FindObjectsOfType(typeof(VideoPlayer)).ToList().ForEach((mto) =>
			{
				Debug.Log("Stopping \"" + mto.name + "\".");
				(mto as VideoPlayer).Stop();
			});
			Settings.ShowCursor();
			//Displayer = () => EditContour(0);
			//superPass = string.Empty;
			//Displayer = OptionsMenu;
		}

		if (DraggingWindow) windowPosition.position = (Vector2)SRSUtilities.adjustedFlipped - windowDragOffset;
	}

	public void AdministratorLogin() => AdminLogin.SetActive(true);
	public void ShowQuitUI() => QuitConfirmation.SetActive(true);
	public void QuitApplication() => Application.Quit();

	public void EditContour()
	{
		_drawUI = true;

		UIObject.SetActive(false);
	}

	public void ShowOptions()
	{
		UIObject.SetActive(false);
		_drawUI = true;
	}

	public static void ResetWindowPosition()
	{
		Debug.Log("Menu.ResetWindowPosition() Settings.menuScreenW: " + Settings.ScreenW + ", saveWindowSize.x: " +
		          Settings.saveWindowSize.x);

		windowPosition = new Rect(Settings.ScreenW * 0.5f - Settings.saveWindowSize.x * 0.5f,
			Screen.height * 0.5f - Settings.saveWindowSize.y * 0.5f, Settings.saveWindowSize.x, Settings.saveWindowSize.y);
	}

	private static string[][] ReadConfigFile(string fileName)
	{
		Debug.Log("ReadConfigFile(" + fileName + ")");

		if (!File.Exists(fileName))
		{
			//Format: variable name on the left, followed by "=", followed by value.
			Debug.LogWarning("Config file \"" + fileName + "\" not found.");
			return null;
		}

		var ergebnis = new string[4][];
		var reader = new StreamReader(fileName);
		var l = 0;

		while (reader.ReadLine() is { } line)
		{
			if (line.Trim().StartsWith("#")) continue;
			ergebnis[l++] = line.Split(","[0]);
		}

		reader.Close();

		if (l != 4)
			Debug.LogWarning("Category file is corrupt with " + l + " lines.");

		return ergebnis;
	}

	private static bool WriteConfigFile(string fileName, string[][] data)
	{
		Debug.Log("WriteConfigFile(" + fileName + ")");

		try
		{
			var sw = new StreamWriter(fileName);

			foreach (var dataSnipped in data)
				sw.WriteLine(string.Join(",", dataSnipped));

			sw.Close();
		}
		catch (Exception e)
		{
			Debug.LogError("Error writing to file " + fileName + ": " + e.Message);
			return false;
		}

		return true;
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

	private IEnumerator CheckForCommands()
	{
		Debug.Log("Menu.CheckForCommands(), standing by: " + standbyForCommands);

		while (true)
		{
			yield return new WaitForSeconds(1);

			if (!standbyForCommands || !File.Exists(Settings.commandFile))
				continue;

			Debug.Log("Command file found.");
			//RunCommand(File.ReadAllText(commandFile));
			foreach (string command in File.ReadAllText(Settings.commandFile).Trim().Split("\n"[0]))
				RunCommand(command.Trim());
			File.Delete(Settings.commandFile);
		}
	}

	private void RunCommand(string command)
	{
		Debug.Log("Running command: \"" + command + "\".");
		int screenNum;
		switch (command.Split(":"[0])[0].Trim())
		{
			case "wiedergaben":

				string movieName = command.Split(":"[0]).Last<string>().Trim();
				screenNum = command.Split(":"[0]).Length > 2
					? Array.IndexOf(screenNames, command.Split(":"[0])[1].Trim())
					: 0;

				string pattern = "^Test.jpg(\\.(jpg|png|ogg))?$";
				string subject = "Test";

				Debug.Log("Test: \"" + subject + "\", \"" + Regex.Replace("Test.jpg", "\\.\\w{3,4}$", "") + "\" / " +
				          pattern + "   : " + Regex.IsMatch(Regex.Replace(subject, "\\.\\w{3,4}$", ""), pattern,
					          RegexOptions.IgnoreCase));

				_projection.StartMovie(screenNum);

				break;
			case "playpatient": //Show patient photos
				string patientName = command.Split(":"[0]).Last<string>().Trim();
				screenNum = command.Split(":"[0]).Length > 2
					? Array.IndexOf(screenNames, command.Split(":"[0])[1].Trim())
					: 0;
				Debug.Log("Playing slides of patient \"" + patientName + "\" on screen " + screenNum + ".");
				break;
			case "halt":
				if (_projection.IsPlaying)
					_projection.StopMovie(command.Split(":"[0]).Length > 1
						? Array.IndexOf(new string[] { "gantry", "wall" }, command.Split(":"[0])[1].Trim())
						: -1); //"gantrywall" will return -1 from Array.IndexOf.
				limbo = true;
				Projection.currentSlideLoop++;
				break;
			case "screen":
				Settings.screenMode =
					(Settings.ScreenMode)Enum.Parse(typeof(Settings.ScreenMode), command.Split(":"[0])[1].Trim());
				break;
			case "rotate":
				_projection.Rotate(command.Split(":"[0])[1] == "Wall" ? 1 : 0);
				break;
			default:
				Debug.LogError("Ungueltiges kommand: " + command.Split(":"[0])[0].Trim() + " (from command: " +
				               command + ")");
				break;
		}
	}

	private void DestroyPreviews()
	{
		Debug.Log("Menu.DestroyPreviews()");
		foreach (Transform t in transform)
		{
			Destroy(t.gameObject);
		}
	}

	private void Overlays(Texture footerTexture)
	{
		GUI.DrawTexture(new Rect(0, 0, Settings.ScreenW, Settings.ScreenH * 0.127617148554337f),
			illuminationsHeader);
		GUI.DrawTexture(
			new Rect(0, Settings.ScreenH * (1 - 0.1246261216350947f), Settings.ScreenW,
				Settings.ScreenH * 0.1246261216350947f), footerTexture);
	}

	private void DrawConfirmQuit()
	{
		GUI.Window(0, windowPosition, (id) =>
		{
			GUI.Label(
				new Rect(windowPosition.width * 0.25f, windowPosition.height * 0.25f, windowPosition.width * 0.5f, 32),
				"Really quit?", gantrySkin.customStyles[2]);
			if (GUI.Button(
				    new Rect(windowPosition.width * 0.2f, windowPosition.height * 0.75f, windowPosition.width * 0.2f,
					    32), "Yes")) Application.Quit();
		}, "Confirmation");
	}

	private static string soundTemp = string.Empty;
	private static bool useSoundTemp = false;

	public void SetMenu(KeyValuePair<GUIContent, Action>[] m = null, int[] disabled = null,
		Texture2D background = null, GUIStyle style = null)
	{
		Debug.Log("Menu.SetMenu(" + m + "," + disabled + "," + background + ")");
#if UNITY_IPHONE
		m = m??categoryMenu;
		background = background??instance.categoryBackground;
		style = style??instance.gantrySkin.customStyles[1];
#endif
		Camera.main.transform.position = Vector3.up * 5; //in case it's skewed from IsEditing the contour map.
		if (_projection.gameObject.activeSelf) _projection.StopAllScreens();
		//m = m ?? mainMenu;
		//if (m == categoryMenu)
		//{
		//	style = style ?? instance.gantrySkin.customStyles[1];
		//	adminPass = string.Empty;
		//}

		//instance.menuBackground.SetActive(m != mainMenu);
		DestroyPreviews();
		_projection.gameObject.SetActive(false);
		Camera.main.transform.Find("Scrolling Background").gameObject.SetActive(true);
		FindObjectsOfType(typeof(VideoPlayer)).ToList().ForEach((mto) =>
		{
			Debug.Log("Stopping \"" + mto.name + "\".");
			(mto as VideoPlayer).Stop();
		});
		Settings.ShowCursor();

		transform.DestroyChildren();
		//Displayer = () => ShowMenu(m ?? mainMenu, disabled, style);
		Settings.ShowCursor();
		passPrompt = false;
	}

	//private void ShowMenu(KeyValuePair<GUIContent, Action>[] m, int[] disabled = null, GUIStyle style = null)
	//{
	//	float buttonWidth = Settings.menuScreenW * 0.5f, buttonHeight = 48, margin = 16, catButtonSize = 128;



	//	GUI.enabled = true;
	//}

	private void EditContour(int screenNum)
	{
		Debug.Log("Menu.EditContour(" + screenNum + ")");
		
		_projection.transform.gameObject.SetActive(true);
		_projection.IsEditing = true; //Keep before ContourEditor initialization.
		menuBackground.SetActive(false);
		Camera.main.transform.Find("Scrolling Background").gameObject.SetActive(false);
		_projection.enabled = true;
		Camera.main.transform.position = -_projection.ScreenPosition(screenNum) + Vector3.up * 5;
		Settings.monitorMode = (Settings.MonitorMode)screenNum;
		_projection.GetComponent<Toolbar>().enabled =
			_projection.GetComponent<InfoDisplay>().enabled = true;
		_contourEditor.Reset(); //after toolbar's Awake, so it can select.
		_contourEditor.Restart();
	}

}
