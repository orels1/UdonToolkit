#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace UdonToolkit {
  public class UTStyles {
    public static Texture2D ComponentBG = Resources.Load<Texture2D>("Component BG");
    public static Texture ArrowR = Resources.Load<Texture>("Arrow_R");
    public static Texture ArrowL = Resources.Load<Texture>("Arrow_L");
    public static Texture Utils = Resources.Load<Texture>("Utils");
    public static Texture Undo = Resources.Load<Texture>("Undo");

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
    
    // based on https://github.com/Xiexe/Xiexes-Unity-Shaders/blob/master/Editor/XSStyles.cs#L275
    public static bool FoldoutHeader(string text, bool value, ref bool showUtils) {
      var shown = value;
      var textColor = EditorGUIUtility.isProSkin ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.1f, 0.1f, 0.1f);
      GUI.backgroundColor = new Color(0.8f , 0.8f, 0.8f);
      var style = new GUIStyle(EditorStyles.helpBox) {
        stretchWidth = true,
        font = new GUIStyle(EditorStyles.label).font,
        fontSize = 10,
        contentOffset = new Vector2(20f, 0f),
        normal = new GUIStyleState() {
          textColor = textColor,
          background = EditorStyles.helpBox.normal.background
        }
      };
      var rect = GUILayoutUtility.GetRect(16f, 20, style);
      EditorGUI.LabelField(rect, new GUIContent(text), style);
      var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
      var utilsRect = rect;
      utilsRect.xMin = utilsRect.xMax - 20f;
      utilsRect.y += 4f;
      utilsRect.width = 13f;
      utilsRect.height = 13f;
      var e = Event.current;
      if (e.type == EventType.Repaint) {
        EditorStyles.foldout.Draw(toggleRect, false, false, value, false);
      }
      GUI.Box(utilsRect, Utils, new GUIStyle());
      GUI.backgroundColor = Color.white;
      
      if (e.type == EventType.MouseDown) {
        if (utilsRect.Contains(e.mousePosition)) {
          showUtils = !showUtils;
          e.Use();
        } else if (rect.Contains(e.mousePosition)) {
          shown = !shown;
          e.Use();
        }
      }

      return shown;
    }

    public static bool FoldoutHeader(string text, bool value) {
      bool stubRef = false;
      return FoldoutHeader(text, value, ref stubRef);
    }

    public static readonly GUIStyle MiniArrowLeft = new GUIStyle(EditorStyles.miniButtonLeft) {
      margin = new RectOffset(0, 0, 3, 3),
      fixedWidth = 20,
      fixedHeight = 17
    };
    
    public static readonly GUIStyle MiniArrowRight = new GUIStyle(EditorStyles.miniButtonRight) {
      margin = new RectOffset(0, 0, 3, 3),
      fixedWidth = 20,
      fixedHeight = 17
    };

    public static readonly GUIStyle UndoButton = new GUIStyle("button") {
      fixedHeight = 18,
      fixedWidth = 22,
      margin = new RectOffset(0, 0, 0, 0),
      padding = new RectOffset(0,0,0,0),
      normal = new GUIStyleState {
        background = (Texture2D) Undo
      },
      alignment = TextAnchor.LowerCenter,
      active = new GUIStyleState {
        background = (Texture2D) Undo
      }, 
    };
  }
}

#endif
