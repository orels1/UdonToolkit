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
      window.minSize = new Vector2( 650, 380 );
      window.maxSize = new Vector2( 650, 380 );
      window.Show();
    }

    private GUIStyle labelStyle = null;
    private static string DiscordURL = "https://discord.com/invite/fR869XP";
    private static string WikiURL = "https://github.com/orels1/UdonToolkit/wiki";
    private static string GithubURL = "https://github.com/orels1/UdonToolkit";
    private static string PatreonURL = "https://www.patreon.com/orels1";

    private void OnGUI() {
      GUILayout.Box("", new GUIStyle(EditorStyles.helpBox) {
        normal = new GUIStyleState() {
          background = UTStyles.Splash
        }
      }, GUILayout.Height(150));
      if (labelStyle == null) {
        labelStyle = new GUIStyle("BoldLabel") {
          fontSize = 13
        };
      }

      EditorGUILayout.BeginVertical();
      EditorGUILayout.LabelField("Welcome to UdonToolkit!", labelStyle);
      GUILayout.Label(@"Udon Toolkit is a project aimed at simplifying usage of Udon by providing a set of easy to use purpose-built behaviours, from generic triggers and actions, to more complete systems like Key Items, Cabinet Drawers, etc.

This also includes a system of attributes that allow you to utilize the same UI elements when building your own behaviours!

If you find that something is broken or works not as you would expect - please ping me in the discord server, or better - create an issue on github.", "WordWrappedMiniLabel", GUILayout.ExpandWidth(true));
      GUILayout.Space(10);
      EditorGUILayout.BeginHorizontal();
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
      EditorGUILayout.EndHorizontal();
      GUILayout.Space(2);
      UTStyles.RenderNote("This will only show once, you can open it again via Window / UdonToolkit / Show Startup Screen");
      EditorGUILayout.EndVertical();
    }
    
  }
}