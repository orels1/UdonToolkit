using UnityEngine;

namespace UdonToolkit
{
    internal static class RigidbodyExtensions
    {
        public static Vector3 GetLinearVelocity(this Rigidbody rigidbody)
        {
#if UNITY_6000_0_OR_NEWER
            return rigidbody.linearVelocity;
#else
            return rigidbody.velocity;
#endif
        }

        public static void SetLinearVelocity(this Rigidbody rigidbody, Vector3 newVelocity)
        {
#if UNITY_6000_0_OR_NEWER
            rigidbody.linearVelocity = newVelocity;
#else
            rigidbody.velocity = newVelocity;
#endif
        }

        public static float GetAngularDamping(this Rigidbody rigidbody)
        {
#if UNITY_6000_0_OR_NEWER
            return rigidbody.angularDamping;
#else
            return rigidbody.angularDrag;
#endif
        }
    }
}