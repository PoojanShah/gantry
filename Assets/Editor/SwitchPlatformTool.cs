using UnityEditor;
using UnityEditor.SceneManagement;

namespace Editor
{
	public class SwitchPlatformTool
	{
#if UNITY_STANDALONE && UNITY_EDITOR
		[MenuItem("Tools/Switch to Android")]
		public static void Switch()
		{
			var scenes = EditorBuildSettings.scenes;

			scenes[0].enabled = false;	//desktop
			scenes[1].enabled = true;	//android

			EditorBuildSettings.scenes = scenes;

			EditorSceneManager.OpenScene(EditorBuildSettings.scenes[1].path);

			EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
		}
	}
#elif UNITY_ANDROID && UNITY_EDITOR
		[MenuItem("Tools/Switch to Desktop")]
	public static void Switch()
	{
		var scenes = EditorBuildSettings.scenes;

		scenes[0].enabled = true;	//desktop
		scenes[1].enabled = false;	//android

		EditorBuildSettings.scenes = scenes;

		EditorSceneManager.OpenScene(EditorBuildSettings.scenes[0].path);

		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
	}
}
#endif
}
