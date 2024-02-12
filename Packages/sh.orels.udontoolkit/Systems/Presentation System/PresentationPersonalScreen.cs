using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace UdonToolkit {
  [CustomName("Personal Screen")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  [HelpMessage("This will spawn a personal screen with the presentation for users to take a closer look\nFor Desktop it will use the provided Desktop Button, for VR you need to press both triggers together")]
  public class PresentationPersonalScreen : UdonSharpBehaviour {
    public GameObject[] screenObjects;
    [HelpBox("This will be attached to the desktop/vr ref")]
    public GameObject mainScreen;

    public PresentationStreamController streamController;

    public Collider screenCollider;
    public string desktopButton = "k";
    [HelpBox("The position of the spawned screen for Desktop players")]
    public GameObject desktopSpawnRef;
    [HelpBox("The position of the spawned screen for VR players")]
    public GameObject vrSpawnRef;

    private bool isVR;
    private VRCPlayerApi player;
    [NonSerialized] public bool shown;

    private void Start() {
      player = Networking.LocalPlayer;
    }

    private float upHeldTime;

    private void Update() {
      if (!isVR && Time.timeSinceLevelLoad < 5) {
        isVR = player.IsUserInVR();
        if (!isVR) {
          screenCollider.enabled = false;
        }
      }
      if (Input.GetKeyDown(desktopButton)) {
        if (isVR) return;
        ToggleDesktopScreen();
      }

      if (Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryThumbstickVertical") > 0.7f)
      {
        upHeldTime += Time.deltaTime;
      }
      else
      {
        upHeldTime = 0;
      }

      if (upHeldTime >= 2f && Input.GetAxisRaw("Oculus_CrossPlatform_SecondaryIndexTrigger") > 0.7f)
      {
        ToggleVRScreen();
      }
    }

    private void ToggleDesktopScreen() {
      shown = !shown;
      foreach (var o in screenObjects) {
        o.SetActive(shown);
      }

      streamController.SetStreamBool();

      if (shown) {
        mainScreen.transform.SetPositionAndRotation(desktopSpawnRef.transform.position, desktopSpawnRef.transform.rotation);
      }
    }
    
    private void ToggleVRScreen() {
      shown = !shown;
      foreach (var o in screenObjects) {
        o.SetActive(shown);
      }
      streamController.SetStreamBool();
      if (shown) {
        mainScreen.transform.SetPositionAndRotation(vrSpawnRef.transform.position, vrSpawnRef.transform.rotation);
      }
    }
  }
}
