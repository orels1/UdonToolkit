using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class SoundOcclusion : UdonSharpBehaviour {
      public AudioLowPassFilter filter;
      public AudioReverbFilter reverb;
      public GameObject source;
      public AudioSource sourceA;
      [Range(100, 22000)]
      public float muffledFilter = 1883f;
      [Range(0, 1)]
      public float muffledVolume = 0.5f;
      [Range(100, 22000)]
      public float quietZoneFilter = 1000f;
      [Range(0, 1)]
      public float quietZoneVolume = 0.2f;

      public float reverbDryTarget;
      public float reverbRoomTarget;

      public float minDistance = 4f;
      public float maxDistance = 10f;
      [Range(0, 1)]
      public float modifier = 0.5f;
      public float speed = 2f;

      public LayerMask occluderLayers;
      public Vector3 currProxy;
      public bool inProxyVolume;
      public float proxyModifier = 1f;

      private float startVolume;
      private float startFilter;
      private bool oldMuffled;
      private bool muffled;
      private float currDistance;
      private int mask;
      private QueryTriggerInteraction qT;
      private float targetVolume;
      private float targetFilter;
      private bool quietZoneActive;
      private float startReverbDry;
      private float startReverbRoom;
      private float adjustedReverbDryTarget;
      private float adjustedReverbRoomTarget;

      private void Start() {
        reverbDryTarget = reverb.dryLevel;
        reverbRoomTarget = reverb.room;
        startReverbDry = reverbDryTarget;
        startReverbRoom = reverbRoomTarget;
        startVolume = sourceA.volume;
        startFilter = filter.cutoffFrequency;
        mask = occluderLayers.value;
        qT = QueryTriggerInteraction.Collide;
      }

      public void ReverbReset() {
        reverb.dryLevel = startReverbDry;
        reverb.room = startReverbRoom;
        reverbDryTarget = startReverbDry;
        reverbRoomTarget = startReverbRoom;
      }

      private void Update() {
        var transAlpha = speed * Time.deltaTime;
        sourceA.volume = Mathf.Lerp(sourceA.volume, targetVolume, transAlpha);
        filter.cutoffFrequency = Mathf.Lerp(filter.cutoffFrequency, targetFilter, transAlpha);
        reverb.room = Mathf.Lerp(reverb.room, reverbRoomTarget, transAlpha);
        reverb.dryLevel = Mathf.Lerp(reverb.dryLevel, reverbDryTarget, transAlpha);
        // transform.LookAt(source.transform);
        var cPos = transform.position;
        var dir = source.transform.position - cPos;
        currDistance = Vector3.Distance(cPos, source.transform.position);
        muffled = false;
        if (Physics.Raycast(cPos, dir, currDistance, mask, qT)) {
          if (Physics.Raycast(cPos + Vector3.right * 0.1f, dir, currDistance, mask, qT)) {
            if (Physics.Raycast(cPos + Vector3.right * -0.1f, dir, currDistance, mask, qT)) {
              muffled = true;   
            }
          }
        }

        // if (muffled != oldMuffled) {
        //   Debug.LogFormat("Muffled is now {0}", muffled);
        //   SetMuffled();
        //   oldMuffled = muffled;
        // }

        var clamped = Mathf.Clamp(currDistance, minDistance, maxDistance);
        var alpha = Remap(clamped, minDistance, maxDistance, 0, 1);

        if (!muffled) {
          targetVolume = Mathf.Lerp(startVolume, startVolume * (1 - modifier), alpha);
          targetFilter = startFilter;
          return;
        }
        var quietAdjustedFilter = quietZoneActive ? quietZoneFilter : muffledFilter;
        var quietAdjustedVolume = quietZoneActive ? quietZoneVolume : muffledVolume;
        quietAdjustedFilter *= proxyModifier;
        quietAdjustedVolume *= proxyModifier;
        targetVolume = Mathf.Lerp(quietAdjustedVolume, quietAdjustedVolume * (1 - modifier) , alpha);
        targetFilter = Mathf.Lerp(quietAdjustedFilter, Mathf.Max(1000, quietAdjustedFilter * (1 - modifier)), alpha);
      }
      
      private float Remap(float s, float a1, float a2, float b1, float b2) {
        return b1 + (s-a1)*(b2-b1)/(a2-a1);
      }

      // private void SetMuffled() {
      //   source.volume = muffled ? muffledVolume : startVolume;
      //   filter.cutoffFrequency = muffled ? muffledFilter : startFilter;
      // }
      public void EnterQuietZone() {
        quietZoneActive = true;
      }
      
      public void ExitQuietZone() {
        quietZoneActive = false;
      }
    }
    
    
}