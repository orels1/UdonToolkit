using UnityEditor;
using UnityEngine;

namespace UdonToolkit {
  public class UTEditorStyles {
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
        GUI.Box(utilsRect, UTStyles.Utils, new GUIStyle());
      }
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
  }
}
