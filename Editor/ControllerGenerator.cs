using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using VRC.Udon.Common.Interfaces;
using VRC.Udon.Editor.ProgramSources;

namespace UdonToolkit {
  public class ControllerGenerator : EditorWindow {
    [MenuItem("Window/UdonToolkit/Controller Generator")]
    public static void ShowWindow() {
      var pos = new Vector2(0, 0);
      var size = new Vector2(450, 500);
      var window = GetWindowWithRect(typeof(ControllerGenerator), new Rect(pos, size),  true, "Controller Generator") as ControllerGenerator;
      window.Show();
    }

    private struct VariableItem {
      public bool synced;
      public string type;
      public string name;
      public string finalCode;
    }

    private UdonProgramAsset source;
    private string controllerName = "";
    private bool customFilename;
    private string controllerFilename = "";
    private List<VariableItem> variables = new List<VariableItem>();
    private List<string> events = new List<string>();
    private bool useExistingObject;
    private GameObject targetObject;

    private Vector2 scrollPos;

    private bool generated;

    private void OnGUI() {
      UTStyles.RenderHeader("Controller Generator");
      UTStyles.RenderNote("Controller Generator will generate a basic UdonToolkit controller for your behaviour you can extend later.\n" +
                          "It is made to speed up the custom UI creation process by pre-populating fields and methods for you");
      scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(450), GUILayout.Height(410));
      var newSource = (UdonProgramAsset) EditorGUILayout.ObjectField("Source Behaviour", source, typeof(UdonProgramAsset), true);
      if (source != newSource) {
        generated = false;
        controllerFilename = "";
        controllerName = "";
        variables.Clear();
        events.Clear();
        source = newSource;
      }
      if (source != null) {
        UTStyles.RenderSectionHeader("Variables");
        RenderVars();
      }
      UTStyles.RenderSectionHeader("Settings");
      EditorGUI.BeginDisabledGroup(source == null);
      controllerName = EditorGUILayout.TextField("Controller Name", controllerName);
      customFilename = EditorGUILayout.Toggle("Use Custom Filename", customFilename);
      if (controllerName.Length == 0 && source != null) {
        controllerName = source.name.Replace("Udon C# Program Asset","");
      }
      EditorGUI.BeginDisabledGroup(!customFilename);
      if (!customFilename && controllerName.Length > 2) {
        controllerFilename = controllerName.Substring(0, 1).ToUpper() + controllerName.Substring(1).Replace(" ", "") + "Controller.cs";
        EditorGUILayout.TextField("Controller Filename", controllerFilename);
      }
      else {
        controllerFilename = EditorGUILayout.TextField("Controller Filename", controllerFilename);
      }
      EditorGUI.EndDisabledGroup();
      // useExistingObject = EditorGUILayout.Toggle("Use Existing GameObject", useExistingObject);
      // UTStyles.RenderNote("Enabling this option will allow you to attach a controller to an existing GameObject, otherwise a fresh object will be generated with both UdonBehaviour and Controller attached");
      // EditorGUI.BeginDisabledGroup(!useExistingObject);
      // targetObject =
      //   (GameObject) EditorGUILayout.ObjectField("Target Object", targetObject, typeof(GameObject), true);
      // EditorGUI.EndDisabledGroup();
      if (generated) {
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        UTStyles.RenderNote($"Created UTControllers/{controllerFilename}");
        GUI.backgroundColor = oldColor;
      }
      if (GUILayout.Button("Generate", GUILayout.Height(30))) {
        generated = false;
        Generate();
      }
      EditorGUI.EndDisabledGroup();
      EditorGUILayout.EndScrollView();
    }
    
    private Dictionary<Type, string> aliases = new Dictionary<Type, string>() {
      { typeof(String[]), "string[]" },
      { typeof(String), "string"},
      { typeof(Single[]), "float[]" },
      { typeof(Single), "float" },
      { typeof(Int32[]), "int[]" },
      { typeof(Int32), "int" },
      { typeof(Boolean[]), "bool[]" },
      { typeof(Boolean), "bool" }
      
    };

    private bool showVars = true;
    private bool showEvents = true;
    
    private void RenderVars() {
      var program = source.SerializedProgramAsset.RetrieveProgram();
      var symbolTable = program.SymbolTable;
      var fields = symbolTable.GetExportedSymbols();
      var uEvents = program.EntryPoints.GetExportedSymbols();
      foreach (var e in uEvents) {
        if (!events.Contains(e) && !e.StartsWith("_")) {
          events.Add(e);
        }
      }
      foreach (var field in fields) {
        var type = symbolTable.GetSymbolType(field);
        var typeString = type.Name;
        if (aliases.ContainsKey(type)) {
          typeString = aliases[type];
        }

        if (variables.FindIndex(i => i.name == field) == -1) {
          variables.Add(new VariableItem() {
            synced = true,
            name = field,
            type = typeString,
            finalCode = $"        [UdonPublic]\r\n" +
                        $"        public {typeString} {field};\r\n\r\n"
          });
        }
      }
      showVars = UTStyles.FoldoutHeader("Public Variables List", showVars);
      var temp = variables.ToArray();
      if (showVars) {
        EditorGUI.indentLevel++;
        // var newSynced = false;
        // if (temp.Length == 0) return;
        // var changedIndex = -100;
        foreach (var (item, index) in temp.WithIndex()) {
          EditorGUILayout.BeginHorizontal();
          // TODO: fix synced bool switch
          // newSynced = EditorGUILayout.Toggle(item.synced);
          // if (newSynced != item.synced) {
          //   Debug.LogFormat("Changed synced {0}", newSynced);
          //   changedIndex = index;
          //   break;
          // }
          // newSynced = item.synced;
          EditorGUILayout.LabelField(item.type);
          EditorGUILayout.LabelField(item.name);
          EditorGUILayout.EndHorizontal();
        }

        EditorGUI.indentLevel--;
        // Debug.LogFormat("Changed Index {0}, {1}", changedIndex, newSynced);
        // if (changedIndex != -100) {
        //   variables[changedIndex] = new VariableItem() {
        //     synced = newSynced,
        //     name = variables[changedIndex].name,
        //     type = variables[changedIndex].type,
        //     finalCode =  variables[changedIndex].finalCode
        //   };
        // }
      }

      showEvents = UTStyles.FoldoutHeader("Events List", showEvents);
      var tempEv = events.ToArray();
      if (showEvents) {
        EditorGUI.indentLevel++;
        foreach (var (item, index) in tempEv.WithIndex()) {
          EditorGUILayout.BeginHorizontal();
          EditorGUILayout.LabelField(item);
          EditorGUILayout.EndHorizontal();
        }
        EditorGUI.indentLevel--;
      }
    }

    private void Generate() {
      var finalString = @"#if UNITY_EDITOR
using UdonToolkit;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace CustomControllers
{
    [CustomName(<NAME>)]
    public class <CLASSNAME> : UTController
    {
<VARIABLES>
<EVENTS>
    }   
}

#endif";

      finalString = finalString.Replace("<NAME>", $"\"{controllerName}\"");
      finalString = finalString.Replace("<CLASSNAME>", controllerFilename.Substring(0, controllerFilename.Length - 3));
      var finalVars = String.Join("", variables.Select(i => i.finalCode).ToArray());
      finalString = finalString.Replace("<VARIABLES>", finalVars);
      var finalEvents = String.Join("", events.Select(e => {
        var formatted = e.Substring(0, 1).ToUpper() + e.Substring(1);
        return $"        [Button(\"{formatted}\")]\r\n" +
               $"        public void {formatted}()\r\n" +
               "        {\r\n" +
               "            if (uB == null) return;\r\n" +
               $"            uB.SendCustomEvent(\"{e}\");\r\n" +
               "        }\r\n\r\n";
      }).ToArray());
      finalString = finalString.Replace("<EVENTS>", finalEvents);
      var lines = finalString.Split('\n');
      var path = Application.dataPath + "/UTControllers";
      if (!Directory.Exists(path)) {
        AssetDatabase.CreateFolder("Assets", "UTControllers");
      }
      File.WriteAllText(Application.dataPath + $"/UTControllers/{controllerFilename}", finalString);
      Debug.Log(finalString);
      AssetDatabase.Refresh();
      generated = true;
    }
  }
}