#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace UdonToolkit {
  public class UTStyles {
    public static Texture2D ComponentBG = (Texture2D) EditorGUIUtility.Load("Assets/UdonToolkit/Internals/Resources/Component BG.png");
    public static Texture ArrowR = (Texture) EditorGUIUtility.Load("Assets/UdonToolkit/Internals/Resources/Arrow_R.png");
    public static Texture ArrowL = (Texture) EditorGUIUtility.Load("Assets/UdonToolkit/Internals/Resources/Arrow_L.png");
    public static Texture Utils = (Texture) EditorGUIUtility.Load("Assets/UdonToolkit/Internals/Resources/Utils.png");

    public static GUIStyle CreatePreviewStyle(Texture2D bg) {
      return new GUIStyle(EditorStyles.helpBox) {
        stretchWidth = true,
        fixedHeight = 150,
        padding = new RectOffset(10, 0, 0, 0),
        alignment = TextAnchor.MiddleCenter,
        border = new RectOffset(0, 0, 0, 0),
        normal = new GUIStyleState() {
          background = bg
        }
      };
    }

    public static GUIStyle Header = new GUIStyle(EditorStyles.helpBox) {
      stretchWidth = true,
      font = (Font) EditorGUIUtility.Load("Assets/UdonToolkit/Internals/Resources/Nunito-SemiBold.ttf"),
      fontSize = 12,
      padding = new RectOffset(10, 0, 0, 0),
      alignment = TextAnchor.MiddleLeft,
      border = new RectOffset(0, 0, 0, 0),
      fixedHeight = 26,
      normal = new GUIStyleState {
        textColor = Color.white,
        background = ComponentBG
      }
    };

    public static void RenderHeader(string name) {
      EditorGUILayout.LabelField(name, Header);
    }

    public static void RenderSectionHeader(string name) {
      GUI.backgroundColor = new Color(0.5f , 0.5f, 0.5f);
      var textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.2f, 0.2f, 0.2f);
      EditorGUILayout.LabelField(new GUIContent(name), new GUIStyle(EditorStyles.helpBox) {
        normal = new GUIStyleState() {
          textColor = textColor,
          background = EditorStyles.helpBox.normal.background
        },
        stretchWidth = true,
      });
      GUI.backgroundColor = Color.white;
    }
    
    public static void RenderSectionHeader(ref Rect position, string name) {
      GUI.backgroundColor = new Color(0.5f , 0.5f, 0.5f);
      EditorGUI.LabelField(position, new GUIContent(name), new GUIStyle(EditorStyles.helpBox) {
        stretchWidth = true,
      });
      GUI.backgroundColor = Color.white;
    }

    public static void RenderNote(string text) {
      var textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.5f) : new Color(0f, 0f, 0f, 0.8f);
      EditorGUILayout.LabelField(text, new GUIStyle(EditorStyles.helpBox) {
        fontSize = 10,
        stretchWidth = true,
        normal = new GUIStyleState {
          background = EditorStyles.helpBox.normal.background,
          textColor = textColor
        }
      });
    }
    
    public static void RenderNote(ref Rect position, string text) {
      var textColor = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.5f) : new Color(0f, 0f, 0f, 0.8f);
      EditorGUILayout.LabelField(text, new GUIStyle(EditorStyles.helpBox) {
        fontSize = 10,
        stretchWidth = true,
        normal = new GUIStyleState {
          background = EditorStyles.helpBox.normal.background,
          textColor = textColor
        }
      });
    }

    public static Texture2D MakeTex(int width, int height, Color col) {
      Color[] pix = new Color[width * height];
      for (int i = 0; i < pix.Length; ++i) {
        pix[i] = col;
      }

      Texture2D result = new Texture2D(width, height);
      result.SetPixels(pix);
      result.Apply();
      return result;
    }

    private static readonly GUIStyle horizontalLine = new GUIStyle {
      normal = {background = EditorGUIUtility.whiteTexture},
      margin = new RectOffset(6, 6, 4, 4),
      fixedHeight = 1
    };

    public static void HorizontalLine(Color color) {
      var c = GUI.color;
      GUI.color = color;
      GUILayout.Box(GUIContent.none, horizontalLine);
      GUI.color = c;
    }

    public static void HorizontalLine() {
      HorizontalLine(new Color(0.5f, 0.5f, 0.5f, 0.5f));
    }

    public static GUIStyle smallButton = new GUIStyle("button") {
      fontSize = 9,
      fixedHeight = 16
    };
  }
}

#endif
