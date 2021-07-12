using System;
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class CameraLens : UdonSharpBehaviour {
      [SectionHeader("General")]
      public bool active;
      public Animator lensAnimator;
      public Animator cameraAnimator;
      
      [FoldoutGroup("User Interface")]
      [SectionHeader("Desktop UI")]
      public GameObject desktopUI;
      [FoldoutGroup("User Interface")]
      public Slider focusSlider;
      [FoldoutGroup("User Interface")]
      public Slider focalSlider;
      [FoldoutGroup("User Interface")]
      public Slider zoomSlider;
      
      [FoldoutGroup("User Interface")]
      [SectionHeader("VR UI")]
      [FoldoutGroup("User Interface")]
      public GameObject vrUI;
      [FoldoutGroup("User Interface")]
      public Slider vrFocusSlider;
      [FoldoutGroup("User Interface")]
      public Slider vrFocalSlider;
      [FoldoutGroup("User Interface")]
      public Slider vrZoomSlider;
      
      [FoldoutGroup("User Interface")]
      [SectionHeader("On Camera UI")]
      public Text positionText;
      [FoldoutGroup("User Interface")]
      public Text focusText;
      [FoldoutGroup("User Interface")]
      public Text zoomText;
      [FoldoutGroup("User Interface")]
      public Text focalText;
      [FoldoutGroup("User Interface")]
      public Image focusBg;
      [FoldoutGroup("User Interface")]
      public Image zoomBg;
      [FoldoutGroup("User Interface")]
      public Image focalBg;

      [FoldoutGroup("VR Controls")] [SectionHeader("Control Spheres")]
      public Transform vrFingerReference;
      [FoldoutGroup("VR Controls")]
      public float sphereRadius = 0.015f;
      [FoldoutGroup("VR Controls")]
      public Transform tutorialSphere;
      [FoldoutGroup("VR Controls")]
      public Transform flipSphere;
      [FoldoutGroup("VR Controls")]
      public Transform alwaysOnSphere;
      [FoldoutGroup("VR Controls")]
      public Transform focusSphere;
      [FoldoutGroup("VR Controls")]
      public Transform worldSpaceSphere;
      
      [SectionHeader("Camera Objects")]
      public GameObject viewSphere;
      public GameObject cameraObject;
      public GameObject visuals;
      public MeshRenderer fingerSphere;
      
      [SectionHeader("Auto Focus")]
      public bool autoFocus;
      public float autoFocusDistance = 30f;
      public Image autoFocusIcon;
      public Image alwaysOnIcon;
      [ColorUsage(true)]
      public Color activeControlColor;

      [SectionHeader("Tracking")]
      public Transform dropTarget;
      public UniversalTracker playspaceTracker;

      private VRC_Pickup pickup;
      private ParentConstraint constraint;
      private BoxCollider col;
      private bool isDesktop = true;
      private float focus;
      private float focal;
      private float zoom;
      private bool pictureTaken;
      private Color oldAFColor;
      private Color oldAlwaysOnColor;
      private Material oldLensMat;
      private MeshRenderer sR;
      private MeshRenderer cR;
      private Camera cam;
      private int activeControl;
      private bool alwaysOn;
      private bool canChangeControl = true;
      private bool worldSpace = true;
      private MaterialPropertyBlock fingerSphereOn;
      private MaterialPropertyBlock fingerSphereOff;
      private MaterialPropertyBlock sphereBlock;
      private Color inactiveColor = new Color(1, 1, 1, 0);
      private Color activeColor = new Color(1, 1, 1, 0.4f);
      private Color inactiveTextColor = new Color(1, 1, 1, 0.4f);
      private Color activeTextColor = new Color(0,0,0,1f);
      private bool closeHelpShown;
      private VRCPlayerApi player;
      private bool setupPlatform;

      private void SetupPlatform() {
        if (player == null) return;
        setupPlatform = true;
        if (player.IsUserInVR()) {
          if (isDesktop) {
            isDesktop = false;
          }

          SetupVR();
          return;
        }
        isDesktop = true;
        SetupDesktop();
      }

      private void SetupVR() {
        cameraAnimator.SetBool("IsVR", true);
        vrUI.SetActive(true);
        isDesktop = false;
        constraint.constraintActive = false;
        pickup.pickupable = true;
      }

      private void SetupDesktop() {
        desktopUI.SetActive(true);
        sphereBlock.SetInt("_Watermark", 1);
        sphereBlock.SetFloat("_SceneStartTime", Time.time - 1);
        sR.SetPropertyBlock(sphereBlock);
      }

      private void Start() {
        // property blocks
        sphereBlock = new MaterialPropertyBlock();
        fingerSphereOn = new MaterialPropertyBlock();
        fingerSphereOn.SetInt("_Enabled", 1);
        fingerSphereOff = new MaterialPropertyBlock();
        fingerSphereOff.SetInt("_Enabled", 0);
        // handle default values
        cam = cameraObject.GetComponent<Camera>();
        zoom = Remap(cam.focalLength, 5, 60, 0, 1);
        zoomSlider.value = zoom;
        vrZoomSlider.value = zoom;
        focal = 0.5f;
        lensAnimator.SetFloat("Focal", focal);
        focalSlider.value = focal;
        vrFocalSlider.value = focal;
        // handle vr/desktop switch
        col = gameObject.GetComponent<BoxCollider>();
        constraint = gameObject.GetComponent<ParentConstraint>();
        cR = visuals.GetComponent<MeshRenderer>();
        sR = viewSphere.GetComponent<MeshRenderer>();
        fingerSphere.gameObject.SetActive(false);
        pickup = (VRC_Pickup) GetComponent(typeof(VRC_Pickup));
        player = Networking.LocalPlayer;
        if (player == null) return;
      }

      // Sometimes
      public override void OnDeserialization() {
        if (setupPlatform) return;
        SetupPlatform();
      }

      private void Activate() {
        active = true;
        cameraObject.SetActive(true);
        if (!isDesktop) {
          viewSphere.SetActive(true);
          fingerSphere.gameObject.SetActive(true);
          fingerSphere.SetPropertyBlock(fingerSphereOn);
        }
        cameraAnimator.SetBool("UIShown", true);
        if (!isDesktop) return;
        constraint.enabled = true;
        constraint.constraintActive = true;
        constraint.weight = 1;
        cameraAnimator.SetBool("Active", true);
        col.enabled = false;
      }

      private void Deactivate() {
        if (!alwaysOn) {
          cameraObject.SetActive(false);
          viewSphere.SetActive(false);
        }
        fingerSphere.gameObject.SetActive(false);
        fingerSphere.SetPropertyBlock(fingerSphereOff);
        constraint.weight = 0;
        constraint.enabled = false;
        cameraAnimator.SetBool("Active", false);
        cameraAnimator.SetBool("UIShown", false);
        active = false;
        col.enabled = true;
        if (isDesktop) {
          transform.position = transform.position + Vector3.down * 0.04f;
        }
      }

      public override void Interact() {
        if (!isDesktop) return;
        Activate();
      }

      public override void OnPickup() {
        if (isDesktop) return;
        transform.parent = null;
        playspaceTracker.ResetOffsets();
        Activate();
      }
      
      public override void OnDrop() {
        if (isDesktop) return;
        if (!worldSpace) {
          playspaceTracker.ResetOffsets();
          transform.SetParent(dropTarget);
        }
        Deactivate();
      }

      public void SwitchPosition() {
        worldSpace = !worldSpace;
        positionText.text = worldSpace ? "World Position" : "Local Position";
      }

      private bool tutorialPassed;
      public void PassTutorial() {
        tutorialPassed = true;
        cameraAnimator.SetBool("TChecked", true);
      }

      public void ToggleWatermark() {
        sR.GetPropertyBlock(sphereBlock);
        var curr = sphereBlock.GetInt("_Watermark");
        if (curr == 1) {
          sphereBlock.SetInt("_Watermark", 0);
        }
        else {
          sphereBlock.SetInt("_Watermark", 1);
        }
        sR.SetPropertyBlock(sphereBlock);
      }

      private void Update() {
        CheckVRThresholds();
        
        if (autoFocus) {
          AutoFocus();
        }

        if (Time.timeSinceLevelLoad > 5 && !setupPlatform) {
          SetupPlatform();
        }
        // global "P" hotkey for desktop
        if (!active && isDesktop) {
          if (Input.GetKeyDown("p")) {
            Activate();
          }
          return;
        }
        
        if (!autoFocus) {
          FocusInputs();
        }

        FocalInputs();
        ZoomInputs();
        SwitchControlInputs();

        if (!isDesktop) return;

        if (Input.GetKeyDown("f")) {
          sR.GetPropertyBlock(sphereBlock);
          sphereBlock.SetInt("_ForceShow", 1);
          sR.SetPropertyBlock(sphereBlock);
          viewSphere.SetActive(true);
          if (!alwaysOn) {
            cameraObject.SetActive(false);
          }
          pictureTaken = true;
          if (!closeHelpShown) {
            sR.GetPropertyBlock(sphereBlock);
            sphereBlock.SetFloat("_EndFadeTime", Time.time + 2);
            sR.SetPropertyBlock(sphereBlock);
            closeHelpShown = true;
          }
        }
        if (Input.GetKeyDown("p")) {
          if (pictureTaken) {
            sR.GetPropertyBlock(sphereBlock);
            sphereBlock.SetInt("_ForceShow", 0);
            sR.SetPropertyBlock(sphereBlock);
            viewSphere.SetActive(false);
            cameraObject.SetActive(true);
            pictureTaken = false;
            return;
          }
          Deactivate();
        }

        if (Input.GetKeyDown("x")) {
          ToggleAlwaysOn();
        }

        if (Input.GetKeyDown("h")) {
          cameraAnimator.SetBool("UIShown", !cameraAnimator.GetBool("UIShown"));
        }

        if (Input.GetKeyDown("y")) {
          SwitchAF();
        }
      }

      public void FlipCamera() {
        var lRot = cameraObject.transform.localRotation.eulerAngles;
        cameraObject.transform.localRotation = Quaternion.Euler(lRot.x, lRot.y + 180, lRot.z);
        var block = new MaterialPropertyBlock();
        cR.GetPropertyBlock(block);
        var curr = block.GetInt("_Flipped");
        block.SetInt("_Flipped", curr == 0 ? 1 : 0);
        cR.SetPropertyBlock(block);
      }

      public void SwitchAF() {
        if (!autoFocus) {
          oldAFColor = autoFocusIcon.color;
          autoFocusIcon.color = activeControlColor;
        }
        else {
          autoFocusIcon.color = oldAFColor;
        }

        autoFocus = !autoFocus;
      }

      public void SwitchControl() {
        activeControl += 1;
        if (activeControl > 2) {
          activeControl = 0;
        }
        focusBg.color = inactiveColor;
        zoomBg.color = inactiveColor;
        focalBg.color = inactiveColor;
        focusText.color = inactiveTextColor;
        zoomText.color = inactiveTextColor;
        focalText.color = inactiveTextColor;
        switch (activeControl) {
          case 0:
            focusBg.color = activeColor;
            focusText.color = activeTextColor;
            break;
          case 1:
            zoomBg.color = activeColor;
            zoomText.color = activeTextColor;
            break;
          case 2:
            focalBg.color = activeColor;
            focalText.color = activeTextColor;
            break;
        }
        cameraAnimator.SetTrigger($"ToggleControl{activeControl}");
      }

      public void ToggleAlwaysOn() {
        if (!alwaysOn) {
          oldAlwaysOnColor = alwaysOnIcon.color;
          alwaysOnIcon.color = activeControlColor;
        }
        else {
          alwaysOnIcon.color = oldAlwaysOnColor;
        }

        alwaysOn = !alwaysOn;
      }

      private void SwitchControlInputs() {
        if (!canChangeControl && Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") < 0.2) {
          canChangeControl = true;
        }
        if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.8 && canChangeControl) {
          SwitchControl();
          canChangeControl = false;
        }
      }
      
      private float VRInputs(float source, bool flipped) {
        var res = source;
        var vertical = Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical");
        if (flipped) {
          vertical *= -1;
        }
        if (vertical > 0 ) {
          res = Mathf.Min(res + (vertical / 5) * Time.deltaTime, 1);
        }
        else {
          res = Mathf.Max(res + (vertical / 5) * Time.deltaTime, 0);
        }

        return res;
      }
      
      private void FocusInputs() {
        if (!isDesktop) {
          if (activeControl != 0) return;
          focus = VRInputs(focus, false);
        }
        else {
          if (Input.GetKey("e")) {
            focus = Mathf.Min(focus + 0.2f * Time.deltaTime, 1);
          } else if (Input.GetKey("q")) {
            focus = Mathf.Max(focus - 0.2f * Time.deltaTime, 0);
          }
        }
        lensAnimator.SetFloat("Focus", focus);
        focusSlider.value = focus;
        vrFocusSlider.value = focus;
      }

      private void ZoomInputs() {
        if (!isDesktop) {
          if (activeControl != 1) return;
          zoom = VRInputs(zoom, false);
        }
        else {
          if (Input.GetKey("2")) {
            zoom = Mathf.Min(zoom + 0.2f * Time.deltaTime, 1);
          } else if (Input.GetKey("1")) {
            zoom = Mathf.Max(zoom - 0.2f * Time.deltaTime, 0);
          }
        }

        var fov = Remap(zoom, 0, 1, 5, 60);
        cam.focalLength = fov;
        zoomSlider.value = zoom;
        vrZoomSlider.value = zoom;
      }

      private void FocalInputs() {
        if (!isDesktop) {
          if (activeControl != 2) return;
          focal = VRInputs(focal, true);
        }
        else {
          if (Input.GetKey("3")) {
            focal = Mathf.Min(focal + 0.2f * Time.deltaTime, 1);
          } else if (Input.GetKey("4")) {
            focal = Mathf.Max(focal - 0.2f * Time.deltaTime, 0);
          }
        }

        lensAnimator.SetFloat("Focal", focal);
        focalSlider.value = 1 - focal;
        vrFocalSlider.value = 1 - focal;
      }
      
      private float Remap(float s, float a1, float a2, float b1, float b2)
      {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
      }

      private void AutoFocus() {
        RaycastHit hit;
        var distance = 10f;
        var fwd = cameraObject.transform.forward;
        if (Physics.Raycast(
          cameraObject.transform.position + fwd * 0.1f, fwd,
          out hit,
          autoFocusDistance, Int32.MaxValue, QueryTriggerInteraction.Ignore)) {
          distance = hit.distance;
        }

        focus = distance / 10f;
        lensAnimator.SetFloat("Focus", focus);
        focusSlider.value = focus;
        vrFocusSlider.value = focus;
      }

      private bool isInTutorial;
      private bool isInFlip;
      private bool isInAlwaysOn;
      private bool isInFocus;
      private bool isInWorldSpace;

      private float GetSphereDistSq(Vector3 source, Vector3 sphere) {
        var flipDir = source - sphere;
        return flipDir.x * flipDir.x + flipDir.y * flipDir.y + flipDir.z * flipDir.z;
      }

      private void CheckVRThresholds() {
        if (isDesktop) return;
        var radSq = sphereRadius * sphereRadius;
        var fingerPos =  vrFingerReference.position;
        
        // TUTORIAL
        var tutorialDistSqr = GetSphereDistSq(fingerPos, tutorialSphere.position);
        if (tutorialDistSqr < radSq) {
          if (!isInTutorial) {
            if (!tutorialPassed) {
              PassTutorial();
              cameraAnimator.SetTrigger("TBubblePulse");
            }
            isInTutorial = true;
          }
        }
        else {
          isInTutorial = false;
        }
        
        // FLIP
        var flipDistSqr = GetSphereDistSq(fingerPos, flipSphere.position);
        if (flipDistSqr < radSq) {
          if (!isInFlip) {
            FlipCamera();
            cameraAnimator.SetTrigger("Pulse1");
            isInFlip = true;
          }
        }
        else {
          isInFlip = false;
        }
        
        // ALWAYS ON
        var alwaysOnDistSqr = GetSphereDistSq(fingerPos, alwaysOnSphere.position);
        if (alwaysOnDistSqr < radSq) {
          if (!isInAlwaysOn) {
            ToggleAlwaysOn();
            cameraAnimator.SetTrigger("Pulse2");
            cameraAnimator.SetTrigger("Toggle2");
            isInAlwaysOn = true;
            alwaysOnSphere.GetChild(0).gameObject.SetActive(false);
            alwaysOnSphere.GetChild(1).gameObject.SetActive(true);
          }
          else {
            alwaysOnSphere.GetChild(0).gameObject.SetActive(true);
            alwaysOnSphere.GetChild(1).gameObject.SetActive(false);
          }
        }
        else {
          isInAlwaysOn = false;
        }
        
        // AUTO FOCUS
        var focusDistSqr = GetSphereDistSq(fingerPos, focusSphere.position);
        if (focusDistSqr < radSq) {
          if (!isInFocus) {
            SwitchAF();
            cameraAnimator.SetTrigger("Pulse3");
            cameraAnimator.SetTrigger("Toggle3");
            isInFocus = true;
            focusSphere.GetChild(0).gameObject.SetActive(false);
            focusSphere.GetChild(1).gameObject.SetActive(true);
          }
          else {
            focusSphere.GetChild(0).gameObject.SetActive(false);
            focusSphere.GetChild(1).gameObject.SetActive(true);
          }
        }
        else {
          isInFocus = false;
        }
        
        // WORLD SPACE
        var worldSpaceDistSqr = GetSphereDistSq(fingerPos, worldSpaceSphere.position);
        if (worldSpaceDistSqr < radSq) {
          if (!isInWorldSpace) {
            SwitchPosition();
            cameraAnimator.SetTrigger("Pulse4");
            cameraAnimator.SetTrigger("Toggle4");
            isInWorldSpace = true;
            worldSpaceSphere.GetChild(0).gameObject.SetActive(false);
            worldSpaceSphere.GetChild(1).gameObject.SetActive(true);
          }
          else {
            worldSpaceSphere.GetChild(0).gameObject.SetActive(true);
            worldSpaceSphere.GetChild(1).gameObject.SetActive(false);
          }
        }
        else {
          isInWorldSpace = false;
        }
      }

      #region DEBUG
      #if UNITY_EDITOR
      public void DEBUG_SetVR() {
        SetupVR();
      }
      
      public void DEBUG_Pickup() {
        OnPickup();
      }
      #endif
      #endregion
    }
}
