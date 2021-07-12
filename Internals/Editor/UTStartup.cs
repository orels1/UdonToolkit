using System.Linq;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace UdonToolkit {
  public class UTBoot : AssetPostprocessor {
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
      string[] movedFromAssetPaths) {
      var isUpdated = importedAssets.Any(path => path.StartsWith("Assets/")) &&
                       deletedAssets.Any(path => path.Contains("UdonToolkit"));
      if (isUpdated) {
        InitializeOnLoad();
      }
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad() {
      UTStartup.Boot();
    }
  }
  public class UTStartup : EditorWindow {
    private static string bootKey = "UT/boot";

    public static void Boot() {
      #if !UNITY_POST_PROCESSING_STACK_V2
      Debug.LogWarning("PostProcessing is not installed in the project. For UdonToolkit CameraSystem to work please import PostProcessing from Window -> Package Manager");
      #endif
      var shouldBoot = true;
      if (EditorPrefs.HasKey(bootKey)) {
        shouldBoot = EditorPrefs.GetBool(bootKey);
      }
      if (!shouldBoot) return;
      ShowWindow();
      EditorPrefs.SetBool(bootKey, false);
    }
    [MenuItem("Window/UdonToolkit/Show Startup Screen",  false, 1999)]
    public static void Init() {
      ShowWindow();
    }

    public static void ShowWindow() {
      var window = (UTStartup) GetWindow(typeof(UTStartup), true, "Welcome to UdonToolkit");
      window.minSize = new Vector2( 256, 370 );
      window.maxSize = new Vector2( 250, 370 );
      window.Show();
    }

    private GUIStyle labelStyle = null;
    private static string DiscordURL = "https://discord.com/invite/fR869XP";
    private static string WikiURL = "https://ut.orels.sh/v/v1.x/";
    private static string GithubURL = "https://github.com/orels1/UdonToolkit";
    private static string PatreonURL = "https://www.patreon.com/orels1";

    private void OnGUI() {
      if (labelStyle == null) {
        labelStyle = new GUIStyle("BoldLabel") {
          fontSize = 13
        };
      }

      EditorGUILayout.BeginVertical();
      EditorGUILayout.LabelField("Welcome to UdonToolkit!", labelStyle);
      GUILayout.Label(@"Udon Toolkit is built to serve as a foundation for your next Udon-based world. It contains a lot of useful behaviours from Area Triggers, to Player Teleporters and Followers.

Also included are various fully-featured systems, like a Camera System that adds an enhanced camera functionality to your world. 

The last part of the Udon Toolkit is an expansive attributes system that allows you to create complex UI for your behaviours the same way Udon Toolkit does.

If you find that something is broken or works not as you would expect - please ping me in the discord server, or better - create an issue on github.", "WordWrappedMiniLabel", GUILayout.ExpandWidth(true));
      GUILayout.Space(10);
      if (GUILayout.Button("Documentation", GUILayout.ExpandWidth(true))) {
        Application.OpenURL(WikiURL);
      }
      if (GUILayout.Button("Github Repo", GUILayout.ExpandWidth(true))) {
        Application.OpenURL(GithubURL);
      }
      if (GUILayout.Button("Discord Server", GUILayout.ExpandWidth(true))) {
        Application.OpenURL(DiscordURL);
      }
      if (GUILayout.Button("Support UdonToolkit", GUILayout.ExpandWidth(true))) {
        Application.OpenURL(PatreonURL);
      }
      GUILayout.Space(2);
      UTStyles.RenderNote("This will only show once, you can open it again via Window / UdonToolkit / Show Startup Screen");
      EditorGUILayout.EndVertical();
    }
    
  }
}
