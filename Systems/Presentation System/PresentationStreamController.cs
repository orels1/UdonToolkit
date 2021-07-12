using System;
using UdonSharp;
using UdonToolkit;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
  [HelpMessage("Stream Controller allows you to press a button to pull up the stream feed on your stream account. It will also teleport the user who pressed the button to a special streamer spot below the main stage for best audio quality")]
  public class PresentationStreamController : UdonSharpBehaviour {
    public PresentationPlayer presentationPlayer;
    [SectionHeader("Access Control")]
    [Toggle]
    [HelpBox("You can limit the people who can turn on the stream screen. It is highly recommended to limit the streaming controller only to the event hosts", "@restrictAccess")]
    public bool restrictAccess;
    [HideIf("@!restrictAccess")]
    public string[] allowedUsers;
    public string engageKey;
    [HelpBox("These objects will be toggled on and off when pressing the stream key")]
    public GameObject[] streamObjects;
    public Camera streamCamera;
    public Animator streamAnimator;
    public Transform streamSpot;
    public Transform resetPosition;
    [SectionHeader("Text Elements")]
    public Text talkTitle;
    public Text talkAuthor;

    private bool active;
    private bool shown;
    private bool streamPlaying;
    
    private void Start() {
      var player = Networking.LocalPlayer;
      var pName = player.displayName;
      if (!restrictAccess) {
        active = true;
        return;
      }
      foreach (var user in allowedUsers) {
        if (user != pName) continue;
        active = true;
        break;
      }
    }

    private void Update() {
      if (!active) return;
      if (Input.GetKeyDown(engageKey)) {
        ToggleStreamObjects();
      }
    }

    public void ToggleStreamObjects() {
      shown = !shown;
      foreach (var o in streamObjects) {
        o.SetActive(shown);
      }
      streamCamera.enabled = shown;
      if (shown) {
        Networking.LocalPlayer.TeleportTo(streamSpot.position, streamSpot.rotation);
      }
      else {
        Networking.LocalPlayer.TeleportTo(resetPosition.position, resetPosition.rotation);
      }
      streamAnimator.SetBool("Active", streamPlaying);
    }

    public void SetStreamBool() {
      streamAnimator.SetBool("Active", streamPlaying);
    }

    public void HandleTalkStart() {
      talkTitle.text = presentationPlayer.talkTitle;
      talkAuthor.text = presentationPlayer.talkAuthor;
      streamPlaying = true;
      streamAnimator.SetBool("Active", true);
    }

    public void HandleTalkEnd() {
      streamPlaying = false;
      streamAnimator.SetBool("Active", false);
    }
  }
}
