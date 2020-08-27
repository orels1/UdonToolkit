using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace UdonToolkit {
    public class SoundOcclusion : UdonSharpBehaviour {
      public AudioLowPassFilter filter;
      public AudioSource source;
      [Range(100, 22000)]
      public float muffledFilter = 1883f;
      [Range(0, 1)]
      public float muffledVolume = 0.5f;
      [Range(100, 22000)]
      public float quietZoneFilter = 1000f;
      [Range(0, 1)]
      public float quietZoneVolume = 0.2f;

      public float minDistance = 4f;
      public float maxDistance = 10f;
      [Range(0, 1)]
      public float modifier = 0.5f;
      public float speed = 2f;

      public LayerMask occluderLayers;

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

      private void Start() {
        startVolume = source.volume;
        startFilter = filter.cutoffFrequency;
        mask = occluderLayers.value;
        qT = QueryTriggerInteraction.Collide;
      }

      private void Update() {
        var transAlpha = speed * Time.deltaTime;
        source.volume = Mathf.Lerp(source.volume, targetVolume, transAlpha);
        filter.cutoffFrequency = Mathf.Lerp(filter.cutoffFrequency, targetFilter, transAlpha);
        transform.LookAt(source.transform);
        var cPos = transform.position;
        currDistance = Vector3.Distance(cPos, source.transform.position);
        muffled = false;
        if (Physics.Raycast(cPos, transform.forward, currDistance, mask, qT)) {
          if (Physics.Raycast(cPos + transform.right * 0.1f, transform.forward, currDistance, mask, qT)) {
            if (Physics.Raycast(cPos + transform.right * -0.1f, transform.forward, currDistance, mask, qT)) {
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
        targetVolume = Mathf.Lerp(quietAdjustedVolume, quietAdjustedVolume * (1 - modifier), alpha);
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