#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace UdonToolkit {
  public class UTStyles {
    public static Texture2D ComponentBG = (Texture2D) EditorGUIUtility.Load("Assets/UdonToolkit/Resources/Component BG.png");
    public static Texture Arrow = (Texture) EditorGUIUtility.Load("Assets/UdonToolkit/Resources/Arrow.png");
    
    public static GUIStyle Header = new GUIStyle(EditorStyles.helpBox) {
      stretchWidth = true,
      font = (Font) EditorGUIUtility.Load("Assets/UdonToolkit/Resources/Nunito-SemiBold.ttf"),
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
      EditorGUILayout.LabelField(new GUIContent(name), new GUIStyle(EditorStyles.helpBox) {
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

    // based on https://github.com/Xiexe/Xiexes-Unity-Shaders/blob/master/Editor/XSStyles.cs#L275
    public static bool FoldoutHeader(string text, bool value) {
      var shown = value;
      GUI.backgroundColor = new Color(0.8f , 0.8f, 0.8f);
      var style = new GUIStyle(EditorStyles.helpBox) {
        stretchWidth = true,
        font = new GUIStyle(EditorStyles.label).font,
        fontSize = 10,
        contentOffset = new Vector2(20f, 0f)
      };
      var rect = GUILayoutUtility.GetRect(16f, 20, style);
      EditorGUI.LabelField(rect, new GUIContent(text), style);
      var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
      var e = Event.current;
      if (e.type == EventType.Repaint) {
        EditorStyles.foldout.Draw(toggleRect, false, false, value, false);
      }
      GUI.backgroundColor = Color.white;

      if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition)) {
        shown = !shown;
        e.Use();
      }

      return shown;
    }

    public static void RenderNote(string text) {
      EditorGUILayout.LabelField(text, new GUIStyle(EditorStyles.helpBox) {
        fontSize = 10,
        stretchWidth = true,
        normal = new GUIStyleState {
          background = EditorStyles.helpBox.normal.background,
          textColor = new Color(1, 1, 1, 0.5f)
        }
      });
    }
    
    public static void RenderNote(ref Rect position, string text) {
      EditorGUI.LabelField(position, text, new GUIStyle(EditorStyles.helpBox) {
        fontSize = 10,
        stretchWidth = true,
        normal = new GUIStyleState {
          background = EditorStyles.helpBox.normal.background,
          textColor = new Color(1, 1, 1, 0.5f)
        }
      });
    }

    private static Texture2D MakeTex(int width, int height, Color col) {
      Color[] pix = new Color[width * height];
      for (int i = 0; i < pix.Length; ++i) {
        pix[i] = col;
      }

      Texture2D result = new Texture2D(width, height);
      result.SetPixels(pix);
      result.Apply();
      return result;
    }
  }
}

#endif