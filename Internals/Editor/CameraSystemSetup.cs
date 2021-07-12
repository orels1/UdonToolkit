using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
#if UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using VRC.Udon;
using Object = UnityEngine.Object;

namespace UdonToolkit {
  public class CameraSystemSetup: EditorWindow {
    private string GetToolkitPath() {
      var ms = MonoScript.FromScriptableObject( this );
      var scriptFilePath = AssetDatabase.GetAssetPath( ms );
 
      var fileInfo = new FileInfo(scriptFilePath);
      var dirInfo = fileInfo.Directory.Parent.Parent;
      var scriptFolder = dirInfo.ToString().Replace("\\", "/");
      var assetsPath = Application.dataPath;
      scriptFolder = scriptFolder.Replace(assetsPath, "Assets");
      return scriptFolder;
    }
    
    
    private string toolkitPath;
    
    [MenuItem("Window/UdonToolkit/Camera System Setup")]
    public static void ShowWindow() {
      var pos = new Vector2(0, 0);
      var size = new Vector2(450, 750);
      var window = GetWindowWithRect(typeof(CameraSystemSetup), new Rect(pos, size),  true, "Camera System Setup") as CameraSystemSetup;
      window.Show();
    }

    private void OnEnable() {
      toolkitPath = GetToolkitPath();
    }

    private enum GuideStyle {
      Futuristic,
      Tiki
    }

    private static string WikiURL = "https://github.com/orels1/UdonToolkit/wiki";
    private Transform cameraSpot;
    private bool addGuide;
    private GuideStyle guideStyle = GuideStyle.Futuristic;
    private int cameraPPLayer = 27;
    private Texture watermark;
    private bool setup;

    private bool incorrectCollision;

    private List<string> layers = new List<string>();

    private void CheckIncorrectCollision(int layerIndex) {
      var res = false;
      for (int i = 0; i < 32; i++) {
        var layerName = LayerMask.LayerToName(i);
        if (!Physics.GetIgnoreLayerCollision(layerIndex, i) && i != layerIndex && layerName.Length > 0) {
          res = true;
          break;
        }
      }

      incorrectCollision = res;
    }

    private GUIStyle standPreview;
    private UnityEngine.Object standObj;
    private UnityEditor.Editor standObjEditor;
    private int emptyLayer;

    #if UNITY_POST_PROCESSING_STACK_V2
    private void OnGUI() {
      emptyLayer = 0;
      UTUtils.GetLayerMasks();
      UTStyles.RenderHeader("Camera System Setup");
      UTStyles.RenderNote("This tool will set up a camera system in your scene, for more details on how to customize the camera - consult the documentation page.");
      if (GUILayout.Button("Documentation", GUILayout.ExpandWidth(true))) {
        Application.OpenURL(WikiURL);
      }
      
      UTStyles.RenderSectionHeader("Position and Style");

      var newCameraSpot = (Transform) EditorGUILayout.ObjectField("Camera Position", cameraSpot, typeof(Transform), true);
      if (newCameraSpot != cameraSpot) {
        setup = false;
      }

      cameraSpot = newCameraSpot;
      EditorGUILayout.BeginHorizontal();
      EditorGUILayout.LabelField("Watermark Overlay");
      watermark = (Texture) EditorGUILayout.ObjectField(watermark, typeof(Texture), true);
      EditorGUILayout.EndHorizontal();
      UTStyles.RenderNote("Overlay should be a 16:9 transparent texture");
      addGuide = EditorGUILayout.Toggle("Add Camera Guide", addGuide);
      if (addGuide) {
        var newGuideStyle = (GuideStyle) EditorGUILayout.EnumPopup("Guide Panel Style", guideStyle);
        if (newGuideStyle != guideStyle || standObj == null || standObjEditor == null) {
          var standPath = newGuideStyle == GuideStyle.Futuristic ? SciFiStandPath : TikiStandPath;
          standObj = AssetDatabase.LoadAssetAtPath(GetAssetPath(standPath), typeof(object));
          standObjEditor = UnityEditor.Editor.CreateEditor(standObj);
        }
        var r = GUILayoutUtility.GetRect(450, 150);
        standObjEditor.OnPreviewGUI(r, EditorStyles.helpBox);
        guideStyle = newGuideStyle;
      }

      UTStyles.RenderSectionHeader("PostProcessing and Collisions");
      UTStyles.RenderNote("Layers are very important to the overall setup. Make sure you follow the instructions below");
      
      layers.Clear();
      for (int i = 0; i < 32; i++) {
        var layerName = LayerMask.LayerToName(i);
        if (layerName.Length > 0) {
          layers.Add($"[{i}] {layerName}");
        }
        else {
          if (i > 22 && emptyLayer == 0) {
            emptyLayer = i;
          }
          layers.Add($"[{i}] -- not set --");
        }
      }

      var layersArr = layers.ToArray();
      
      if (emptyLayer != 0) {
        if (GUILayout.Button("Setup Layers")) {
          SetupLayers();
        }
        UTStyles.RenderNote("It seems like you have empty layers available, UdonToolkit can set up everything for you if you click the button above");
        UTStyles.HorizontalLine();
      }

      cameraPPLayer = EditorGUILayout.Popup("Camera PP Layer", cameraPPLayer, layersArr);
      UTStyles.RenderNote("This layer will be used for detecting PP volumes for the Camera System");
      if (cameraPPLayer == 22) {
        EditorGUILayout.HelpBox("It is crucial to not put Camera PP volumes on the same layer as the main PostProcessing, or your own view will be affected by the camera effects", MessageType.Warning);
      }

      if (setup) {
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.green;
        UTStyles.RenderNote("Camera system has been set up in the scene!");
        GUI.backgroundColor = oldColor;
      }
      if (GUILayout.Button("Create Camera System", GUILayout.Height(30))) {
        RunCameraSetup();  
      }

      if (GUILayout.Button("Add Guide Stand Only", GUILayout.Height(20))) {
        AddGuide();
      }
    }

    private void AddGuide() {
      var standPath = guideStyle == GuideStyle.Futuristic ? SciFiStandPath : TikiStandPath;
      var standPrefab = AssetDatabase.LoadAssetAtPath(GetAssetPath(standPath), typeof(GameObject));
      var instancedStand = PrefabUtility.InstantiatePrefab(standPrefab) as GameObject;
      instancedStand.transform.position = cameraSpot.position + Vector3.down * 0.275f + cameraSpot.forward * -0.110f + cameraSpot.right * -0.043f;
      instancedStand.transform.rotation = cameraSpot.rotation;
      PrefabUtility.UnpackPrefabInstance(instancedStand, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
      Undo.RegisterCreatedObjectUndo(instancedStand, "Spawn Camera Stand");
    }

    private void SetupLayers() {
      UTUtils.CreateLayer("CameraSystem", emptyLayer);
      for (int i = 0; i < 32; i++) {
        if (i == emptyLayer) {
          continue;
        }
        Physics.IgnoreLayerCollision(emptyLayer, i, true);
      }

      cameraPPLayer = emptyLayer;
    }

    private static readonly string systemPath = "Camera System";
    private static readonly string SciFiStandPath = "Camera Stand.prefab";
    private static readonly string TikiStandPath = "Camera Stand Tiki.prefab";
    private static readonly string CameraSystemPath = "UT Camera System.prefab";
    private static readonly string DefaultPPProfilePath = "Assets/Camera Lens PP.asset";
    private static readonly string FocusFarPPProfilePath = "Assets/Camera Lens PP Far.asset";
    private static readonly string FocalNearPPProfilePath = "Assets/Camera Lens PP Focal.asset";

    private string GetAssetPath(string path) {
      return $"{toolkitPath}/Systems/{systemPath}/{path}";
    }

    private void RunCameraSetup() {
      setup = false;
      if (cameraSpot == null) return;
      Undo.SetCurrentGroupName("Setup Camera System");
      int group = Undo.GetCurrentGroup();

      // spawn stand
      if (addGuide) {
        AddGuide();
      }
      
      // spawn camera
      var cameraPrefab = AssetDatabase.LoadAssetAtPath(GetAssetPath(CameraSystemPath), typeof(GameObject));
      var instancedCamera = PrefabUtility.InstantiatePrefab(cameraPrefab) as GameObject;
      PrefabUtility.UnpackPrefabInstance(instancedCamera, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

      var instancedCameraRoot = instancedCamera.transform;
      // setup layers
      var profilesParent = instancedCameraRoot.Find("Camera Lens Profiles").transform;
      for (int i = 0; i < profilesParent.childCount; i++) {
        var child = profilesParent.GetChild(i);
        Undo.RecordObject(child.gameObject, "Adjusted Camera Lens PP Layers");
        child.gameObject.layer = cameraPPLayer;
      }
      
      var cameraObject = instancedCameraRoot.Find("Camera Lens").gameObject;
      var cameraLens = cameraObject.transform.Find("Lens Camera").gameObject;

      // Set watermark
      if (watermark != null) {
        var overlaySphere = instancedCameraRoot.Find("Camera Tracker").Find("Sphere");
        overlaySphere.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_WatermarkImage", watermark);
      }
      
      // add PP stuff, we have to do it manually as having these scripts in a prefab causes build time erorrs
      // TODO: extract this into a function
      var cameraPPComp = cameraLens.AddComponent<PostProcessLayer>();
      cameraPPComp.volumeTrigger = cameraLens.transform;
      cameraPPComp.volumeLayer = LayerMask.GetMask(LayerMask.LayerToName(cameraPPLayer));
      var defaultPPVolumeGo = profilesParent.GetChild(0).gameObject;
      var focusFarPPVolumeGo = profilesParent.GetChild(1).gameObject;
      var focalNearPPVolumeGo = profilesParent.GetChild(2).gameObject;
      Undo.RecordObjects(new Object[] { defaultPPVolumeGo, focusFarPPVolumeGo, focalNearPPVolumeGo }, "Add PP Volume Components");
      var defaultPPVolume = defaultPPVolumeGo.AddComponent<PostProcessVolume>();
      var focusFarPPVolume = focusFarPPVolumeGo.AddComponent<PostProcessVolume>();
      var focalNearPPVolume = focalNearPPVolumeGo.AddComponent<PostProcessVolume>();
      defaultPPVolume.isGlobal = true;
      defaultPPVolume.weight = 1;
      defaultPPVolume.sharedProfile =
        (PostProcessProfile) AssetDatabase.LoadAssetAtPath(GetAssetPath(DefaultPPProfilePath), typeof(PostProcessProfile));
      focusFarPPVolume.isGlobal = true;
      focusFarPPVolume.weight = 0;
      focusFarPPVolume.priority = 1;
      focusFarPPVolume.sharedProfile =
        (PostProcessProfile) AssetDatabase.LoadAssetAtPath(GetAssetPath(FocusFarPPProfilePath), typeof(PostProcessProfile));
      focalNearPPVolume.isGlobal = true;
      focalNearPPVolume.weight = 0;
      focalNearPPVolume.priority = 1;
      focalNearPPVolume.sharedProfile =
        (PostProcessProfile) AssetDatabase.LoadAssetAtPath(GetAssetPath(FocalNearPPProfilePath), typeof(PostProcessProfile));

      // move to root
      var childCount = instancedCamera.transform.childCount;
      for (int i = 0; i < childCount; i++) {
        var child = instancedCamera.transform.GetChild(0);
        child.parent = null;
        Undo.RegisterCreatedObjectUndo(child.gameObject, "Spawn Camera Lens");
      }
      DestroyImmediate(instancedCamera);
      
      // setup constraints
      var constraint = cameraObject.GetComponent<ParentConstraint>();
      Undo.RecordObject(constraint, "Adjust Constraint Settings");
      constraint.constraintActive = false;
      constraint.locked = false;
      constraint.translationAtRest = cameraSpot.position;
      constraint.rotationAtRest = cameraSpot.rotation.eulerAngles;
      cameraObject.transform.position = cameraSpot.position;
      cameraObject.transform.rotation = cameraSpot.rotation;
      constraint.translationOffsets = new[] {Vector3.zero};
      constraint.rotationOffsets = new[] {Vector3.zero};
      constraint.locked = true;
      constraint.constraintActive = true;

      Undo.CollapseUndoOperations(group);
      setup = true;
    }
    #else
    private void OnGUI() {
      UTStyles.RenderHeader("Camera System Setup");
      EditorGUILayout.HelpBox("Camera System requires PostProcessing, please install it by searching for PostProcessing in Window -> Package Manager", MessageType.Error);
    }
    #endif
  }
}
