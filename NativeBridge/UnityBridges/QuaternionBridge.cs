using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityCpp.NativeBridge.UnityBridges
{
    [StructLayout(LayoutKind.Sequential)]
    public struct QuaternionBridge
    {
        [UsedImplicitly] public float x { get; set; }
        [UsedImplicitly] public float y { get; set; }
        [UsedImplicitly] public float z { get; set; }
        [UsedImplicitly] public float w { get; set; }

        [UsedImplicitly]
        public Vector3Bridge eulerAngles
        {
            get => new Quaternion(x, y, z, w).eulerAngles;
            set
            {
                Quaternion quat = Quaternion.Euler(value);
                UpdateValues(quat);
            }
        }

        [UsedImplicitly] public QuaternionBridge normalized => new Quaternion(x, y, z, w).normalized;

        [UsedImplicitly] public static QuaternionBridge identify = Quaternion.identity;

        private QuaternionBridge(Quaternion quaternion) : this()
        {
            UpdateValues(quaternion);
        }

        public QuaternionBridge(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        [UsedImplicitly]
        public void Set(float newX, float newY, float newZ, float newW)
        {
            x = newX;
            y = newY;
            z = newZ;
            w = newW;
        }

        [UsedImplicitly]
        public void SetFromToRotation(Vector3Bridge fromDirection, Vector3Bridge toDirection)
        {
            Quaternion quat = this;
            quat.SetFromToRotation(fromDirection, toDirection);
            UpdateValues(quat);
        }

        [UsedImplicitly]
        public void SetLookRotation(Vector3Bridge view, Vector3Bridge up)
        {
            Quaternion quat = this;
            quat.SetLookRotation(view, up);
            UpdateValues(quat);
        }

        [UsedImplicitly]
        public void ToAngleAxis(out float angle, out Vector3Bridge axis)
        {
            Quaternion quat = this;
            quat.ToAngleAxis(out angle, out Vector3 a);
            axis = a;
        }

        [UsedImplicitly]
        public static float Angle(QuaternionBridge a, QuaternionBridge b) => Quaternion.Angle(a, b);
        [UsedImplicitly]
        public static QuaternionBridge AngleAxis(float angle, Vector3Bridge axis) => Quaternion.AngleAxis(angle, axis);
        [UsedImplicitly]
        public static float Dot(QuaternionBridge a, QuaternionBridge b) => Quaternion.Dot(a, b);
        [UsedImplicitly]
        public static QuaternionBridge Euler(float x, float y, float z) => Quaternion.Euler(x, y, z);
        [UsedImplicitly]
        public static QuaternionBridge FromToRotation(Vector3Bridge fromDirection, Vector3Bridge toDirection) => Quaternion.FromToRotation(fromDirection, toDirection);
        [UsedImplicitly]
        public static QuaternionBridge Inverse(QuaternionBridge rotation) => Quaternion.Inverse(rotation);
        [UsedImplicitly]
        public static QuaternionBridge Lerp(QuaternionBridge a, QuaternionBridge b, float t) => Quaternion.Lerp(a, b, t);
        [UsedImplicitly]
        public static QuaternionBridge LerpUnclamped(QuaternionBridge a, QuaternionBridge b, float t) => Quaternion.LerpUnclamped(a, b, t);
        [UsedImplicitly]
        public static QuaternionBridge LookRotation(Vector3Bridge forward, Vector3Bridge upwards) => Quaternion.LookRotation(forward, upwards);
        [UsedImplicitly]
        public static QuaternionBridge Normalize(QuaternionBridge q) => Quaternion.Normalize(q);
        [UsedImplicitly]
        public static QuaternionBridge RotateTowards(QuaternionBridge from, QuaternionBridge to, float maxDegreesDelta) => Quaternion.RotateTowards(from, to, maxDegreesDelta);
        [UsedImplicitly]
        public static QuaternionBridge Slerp(QuaternionBridge a, QuaternionBridge b, float t) => Quaternion.Slerp(a, b, t);
        [UsedImplicitly]
        public static QuaternionBridge SlerpUnclamped(QuaternionBridge a, QuaternionBridge b, float t) => Quaternion.SlerpUnclamped(a, b, t);
        
        public static implicit operator QuaternionBridge(Quaternion quaternion) => new QuaternionBridge(quaternion);
        public static implicit operator Quaternion(QuaternionBridge quaternionBridge)
        {
            return new Quaternion(quaternionBridge.x, quaternionBridge.y, quaternionBridge.z, quaternionBridge.w);
        }
        
        private void UpdateValues(Quaternion quat)
        {
            x = quat.x;
            y = quat.y;
            z = quat.z;
            w = quat.w;
        }
    }
}