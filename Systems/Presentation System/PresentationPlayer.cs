using System;
using System.Collections.Generic;
using UdonSharp;
using UdonToolkit;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Video.Components.Base;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace UdonToolkit {
  [CustomName("Presentation Player")]
  [HelpURL("https://ut.orels.sh/systems/presentation-system")]
  public class PresentationPlayer : UdonSharpBehaviour {
    [HelpBox(
      "Clicking this button will populate the list of talks with all the TLXTalk objects that are children of this Presentation Player")]
    [Toggle("Populate Talks")]
    [OnValueChanged("PopulateTalks")]
    public bool populateTalksBtn;

    public PresentationTalk[] talks;

    [SectionHeader("Video Players")] public PresentationVideoPlayer mainPlayer;
    public PresentationVideoPlayer lookaheadPlayer;

    #region UTFunctions

    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public void PopulateTalks(SerializedProperty prop) {
      var talkCount = transform.childCount;
      var talksList = new List<PresentationTalk>();
      for (int i = 0; i < talkCount; i++) {
        var talk = transform.GetChild(i).GetComponent<PresentationTalk>();
        if (talk == null) continue;
        talksList.Add(talk);
      }

      var talksProp = prop.serializedObject.FindProperty("talks");
      talksProp.arraySize = talksList.Count;
      for (int i = 0; i < talksList.Count; i++) {
        talksProp.GetArrayElementAtIndex(i).objectReferenceValue = talksList[i];
      }

      prop.boolValue = false;
    }
    #endif

    #endregion

    [SectionHeader("Access Control")]
    [Toggle]
    [HelpBox("You can limit the people who can control the presentation using the whitelist of names below",
      "@restrictAccess")]
    public bool restrictAccess;

    [HideIf("@!restrictAccess")] public string[] allowedUsers;

    #region OnTakenControl

    [FoldoutGroup("Callbacks")] [ListView("OnTakenControl")] [LVHeader("Behaviours")]
    public UdonSharpBehaviour[] takenControlTargets;

    [FoldoutGroup("Callbacks")]
    [ListView("OnTakenControl")]
    [LVHeader("Events")]
    [Popup("behaviour", "@takenControlTargets")]
    public string[] takenControlEvents;

    [FoldoutGroup("Callbacks")] [ListView("OnTakenControl")] [LVHeader("Targets")] [Popup("@netTargetTypes")]
    public int[] takenControlNetworkTargets;

    #endregion

    #region OnLostControl

    [FoldoutGroup("Callbacks")] [ListView("OnLostControl")] [LVHeader("Behaviours")]
    public UdonSharpBehaviour[] lostControlTargets;

    [FoldoutGroup("Callbacks")]
    [ListView("OnLostControl")]
    [LVHeader("Events")]
    [Popup("behaviour", "@lostControlTargets")]
    public string[] lostControlEvents;

    [FoldoutGroup("Callbacks")] [ListView("OnLostControl")] [LVHeader("Targets")] [Popup("@netTargetTypes")]
    public int[] lostControlNetworkTargets;

    #endregion

    #region OnTalkStart

    [FoldoutGroup("Callbacks")] [ListView("OnTalkStart")] [LVHeader("Behaviours")]
    public UdonSharpBehaviour[] talkStartTargets;

    [FoldoutGroup("Callbacks")] [ListView("OnTalkStart")] [LVHeader("Events")] [Popup("behaviour", "@talkStartTargets")]
    public string[] talkStartEvents;

    [FoldoutGroup("Callbacks")] [ListView("OnTalkStart")] [LVHeader("Targets")] [Popup("@netTargetTypes")]
    public int[] talkStartNetworkTargets;

    #endregion

    #region OnTalkEnd

    [FoldoutGroup("Callbacks")] [ListView("OnTalkEnd")] [LVHeader("Behaviours")]
    public UdonSharpBehaviour[] talkEndTargets;

    [FoldoutGroup("Callbacks")] [ListView("OnTalkEnd")] [LVHeader("Events")] [Popup("behaviour", "@talkEndTargets")]
    public string[] talkEndEvents;

    [FoldoutGroup("Callbacks")] [ListView("OnTalkEnd")] [LVHeader("Targets")] [Popup("@netTargetTypes")]
    public int[] talkEndNetworkTargets;

    #endregion

    #region OnNextSlide

    [FoldoutGroup("Callbacks")] [ListView("OnNextSlide")] [LVHeader("Behaviours")]
    public UdonSharpBehaviour[] nextSlideTargets;

    [FoldoutGroup("Callbacks")] [ListView("OnNextSlide")] [LVHeader("Events")] [Popup("behaviour", "@nextSlideTargets")]
    public string[] nextSlideEvents;

    [FoldoutGroup("Callbacks")] [ListView("OnNextSlide")] [LVHeader("Targets")] [Popup("@netTargetTypes")]
    public int[] nextSlideNetworkTargets;

    #endregion

    #region OnPrevSlide

    [FoldoutGroup("Callbacks")] [ListView("OnPrevSlide")] [LVHeader("Behaviours")]
    public UdonSharpBehaviour[] prevSlideTargets;

    [FoldoutGroup("Callbacks")] [ListView("OnPrevSlide")] [LVHeader("Events")] [Popup("behaviour", "@prevSlideTargets")]
    public string[] prevSlideEvents;

    [FoldoutGroup("Callbacks")] [ListView("OnPrevSlide")] [LVHeader("Targets")] [Popup("@netTargetTypes")]
    public int[] prevSlideNetworkTargets;

    #endregion

    [NonSerialized] public string[] netTargetTypes = new[] {"Local", "All", "Owner"};

    [HideInInspector] public string talkTitle;
    [HideInInspector] public string talkAuthor;

    [HideInInspector] [UdonSynced] public int talkIndex;
    private int localTalkIndex;

    [UdonSynced] private bool presenting;
    private bool localPresenting;

    [UdonSynced] private int currSlide;
    private int localCurrSlide;

    private bool isOwner;

    void Start() {
      isOwner = Networking.IsOwner(gameObject);
      PrepareTalk(0, 0);
    }

    private void PrepareTalk(int index, int seekTo) {
      if (talks.Length == 0) return;
      var clamped = Mathf.Clamp(index, 0, talks.Length - 1);
      var talk = talks[clamped];
      talk.mainPlayerController = mainPlayer;
      talk.lookaheadPlayerController = lookaheadPlayer;
      talk.Prepare(seekTo);
      talkTitle = talk.talkTitle;
      talkAuthor = talk.talkAuthor;
    }

    private bool CheckAccess(VRCPlayerApi toCheck) {
      if (!restrictAccess) return true;
      var hasAccess = false;
      var pName = toCheck.displayName;
      foreach (var u in allowedUsers) {
        if (String.Equals(u, pName, StringComparison.InvariantCultureIgnoreCase)) {
          hasAccess = true;
          break;
        }
      }

      return hasAccess;
    }

    private void FireCallbacks(UdonSharpBehaviour[] callbackTargets, string[] callbackEvents,
      int[] callbackNetTargets) {
      for (int i = 0; i < callbackTargets.Length; i++) {
        if (callbackNetTargets[i] == 0) {
          callbackTargets[i].SendCustomEvent(callbackEvents[i]);
          continue;
        }

        if (callbackNetTargets[i] == 2 && isOwner) {
          callbackTargets[i].SendCustomEvent(callbackEvents[i]);
          continue;
        }

        callbackTargets[i]
          .SendCustomNetworkEvent(callbackNetTargets[i] == 1 ? NetworkEventTarget.All : NetworkEventTarget.Owner,
            callbackEvents[i]);
      }
    }

    public void TakeControl() {
      // if already the owner - we'll just fire the callbacks
      if (isOwner) {
        FireCallbacks(takenControlTargets, takenControlEvents, takenControlNetworkTargets);
        return;
      }

      var player = Networking.LocalPlayer;
      if (!CheckAccess(player)) return;
      Networking.SetOwner(player, gameObject);
      foreach (var talk in talks) {
        Networking.SetOwner(player, talk.gameObject);
      }
    }

    public override void OnOwnershipTransferred(VRCPlayerApi player) {
      isOwner = Networking.IsOwner(gameObject);
      if (!isOwner) {
        FireCallbacks(lostControlTargets, lostControlEvents, lostControlNetworkTargets);
        return;
      }

      FireCallbacks(takenControlTargets, takenControlEvents, takenControlNetworkTargets);
    }

    public override void OnPlayerLeft(VRCPlayerApi player) {
      var newIsOwner = Networking.IsOwner(gameObject);
      if (newIsOwner == isOwner) return;
      isOwner = newIsOwner;
      if (!isOwner) {
        FireCallbacks(lostControlTargets, lostControlEvents, lostControlNetworkTargets);
        return;
      }

      FireCallbacks(takenControlTargets, takenControlEvents, takenControlNetworkTargets);
    }

    public void StartTalk() {
      if (!isOwner) return;
      presenting = true;
      localPresenting = true;
      talks[talkIndex].Play();
      Debug.Log("firing start callbacks");
      FireCallbacks(talkStartTargets, talkStartEvents, talkStartNetworkTargets);
    }

    public void StopTalk() {
      if (!isOwner) return;
      presenting = false;
      localPresenting = false;
      talks[talkIndex].Stop();
      currSlide = 0;
      localCurrSlide = 0;
      FireCallbacks(talkEndTargets, talkEndEvents, talkEndNetworkTargets);
    }

    public void PrevSlide() {
      if (!isOwner) return;
      talks[talkIndex].PrevSlide();
      currSlide = talks[talkIndex].currSlide;
      if (currSlide != localCurrSlide) {
        FireCallbacks(prevSlideTargets, prevSlideEvents, prevSlideNetworkTargets);
      }

      localCurrSlide = currSlide;
    }

    public void NextSlide() {
      if (!isOwner) return;
      talks[talkIndex].NextSlide();
      currSlide = talks[talkIndex].currSlide;
      if (currSlide != localCurrSlide) {
        FireCallbacks(nextSlideTargets, nextSlideEvents, nextSlideNetworkTargets);
      }

      localCurrSlide = currSlide;
    }

    public void NextTalk() {
      if (!isOwner) return;
      talkIndex = Mathf.Clamp(talkIndex + 1, 0, talks.Length - 1);
      localTalkIndex = talkIndex;
      PrepareTalk(talkIndex, 0);
    }

    public void PrevTalk() {
      if (!isOwner) return;
      talkIndex = Mathf.Clamp(talkIndex - 1, 0, talks.Length - 1);
      localTalkIndex = talkIndex;
      PrepareTalk(talkIndex, 0);
    }

    public void SelectTalk(int newIndex) {
      if (!isOwner) return;
      talkIndex = Mathf.Clamp(newIndex, 0, talks.Length - 1);
      localTalkIndex = talkIndex;
      PrepareTalk(talkIndex, 0);
    }

    // Non-synced play/pause for animated slides
    public void PlayTalkVideo() {
      talks[talkIndex].SendCustomNetworkEvent(NetworkEventTarget.All, "PlayContinuous");
    }

    public void PauseTalkVideo() {
      talks[talkIndex].SendCustomNetworkEvent(NetworkEventTarget.All, "PauseContinuous");
    }

    public override void OnDeserialization() {
      if (isOwner) return;
      if (localTalkIndex != talkIndex) {
        localCurrSlide = currSlide;
        localTalkIndex = talkIndex;
        PrepareTalk(talkIndex, currSlide);
        return;
      }

      if (localPresenting != presenting) {
        localPresenting = presenting;
        if (presenting) {
          talks[talkIndex].Play();
          FireCallbacks(talkStartTargets, talkStartEvents, talkStartNetworkTargets);
        }
        else {
          talks[talkIndex].Stop();
          FireCallbacks(talkEndTargets, talkEndEvents, talkEndNetworkTargets);
        }

        return;
      }

      if (localCurrSlide != currSlide) {
        localCurrSlide = currSlide;
        talks[talkIndex].SeekToSlide(currSlide);
      }
    }
  }
}
