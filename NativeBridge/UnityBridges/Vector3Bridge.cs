using System.Runtime.InteropServices;
using JetBrains.Annotations;
using UnityEngine;

namespace UnityCpp.NativeBridge.UnityBridges
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vector3Bridge
    {
        [UsedImplicitly] public float x { get; set; }
        [UsedImplicitly] public float y { get; set; }
        [UsedImplicitly] public float z { get; set; }

        [UsedImplicitly] public float magnitude => new Vector3(x, y, z).magnitude;
        [UsedImplicitly] public float sqrMagnitude => new Vector3(x, y, z).sqrMagnitude;
        [UsedImplicitly] public Vector3Bridge normalized => new Vector3(x, y, z).normalized;
        
        [UsedImplicitly] public static Vector3Bridge back => Vector3.back;
        [UsedImplicitly] public static Vector3Bridge down => Vector3.down;
        [UsedImplicitly] public static Vector3Bridge forward => Vector3.forward;
        [UsedImplicitly] public static Vector3Bridge left => Vector3.left;
        [UsedImplicitly] public static Vector3Bridge negativeInfinity => Vector3.negativeInfinity;
        [UsedImplicitly] public static Vector3Bridge one => Vector3.one;
        [UsedImplicitly] public static Vector3Bridge positiveInfinity => Vector3.positiveInfinity;
        [UsedImplicitly] public static Vector3Bridge right => Vector3.right;
        [UsedImplicitly] public static Vector3Bridge up => Vector3.up;
        [UsedImplicitly] public static Vector3Bridge zero => Vector3.zero;

        private Vector3Bridge(Vector3 vector3)
        {
            x = vector3.x;
            y = vector3.y;
            z = vector3.z;
        }

        public Vector3Bridge(float x, float y) : this()
        {
            this.x = x;
            this.y = y;
        }

        public Vector3Bridge(float x, float y, float z) : this(x, y)
        {
            this.z = z;
        }

        [UsedImplicitly]
        public static float Angle(Vector3Bridge from, Vector3Bridge to) => Vector3.Angle(from, to);
        [UsedImplicitly]
        public static Vector3Bridge ClampMagnitude(Vector3Bridge vector, float maxLength) => Vector3.ClampMagnitude(vector, maxLength);
        [UsedImplicitly]
        public static Vector3Bridge Cross(Vector3Bridge lhs, Vector3Bridge rhs) => Vector3.Cross(lhs, rhs);
        [UsedImplicitly]
        public static float Distance(Vector3Bridge a, Vector3Bridge b) => Vector3.Distance(a, b);
        [UsedImplicitly]
        public static float Dot(Vector3Bridge lhs, Vector3Bridge rhs) => Vector3.Dot(lhs, rhs);
        [UsedImplicitly]
        public static Vector3Bridge Lerp(Vector3Bridge a, Vector3Bridge b, float t) => Vector3.Lerp(a, b, t);
        [UsedImplicitly]
        public static Vector3Bridge LerpUnclamped(Vector3Bridge a, Vector3Bridge b, float t) => Vector3.LerpUnclamped(a, b, t);
        [UsedImplicitly]
        public static Vector3Bridge Max(Vector3Bridge lhs, Vector3Bridge rhs) => Vector3.Max(lhs, rhs);
        [UsedImplicitly]
        public static Vector3Bridge Min(Vector3Bridge lhs, Vector3Bridge rhs) => Vector3.Min(lhs, rhs);
        [UsedImplicitly]
        public static Vector3Bridge MoveTowards(Vector3Bridge current, Vector3Bridge target, float maxDistanceDelta) => Vector3.MoveTowards(current, target, maxDistanceDelta);
        [UsedImplicitly]
        public static Vector3Bridge Normalize(Vector3Bridge value) => Vector3.Normalize(value);
        
        [UsedImplicitly]
        public static void OrthoNormalize(ref Vector3Bridge normal, ref Vector3Bridge tangent)
        {
            Vector3 n = normal;
            Vector3 t = tangent;
            Vector3.OrthoNormalize(ref n, ref t);
            normal = n;
            tangent = t;
        }

        [UsedImplicitly]
        public static Vector3Bridge Project(Vector3Bridge vector, Vector3Bridge onNormal) => Vector3.Project(vector, onNormal);
        [UsedImplicitly]
        public static Vector3Bridge ProjectOnPlane(Vector3Bridge vector, Vector3Bridge planeNormal) => Vector3.ProjectOnPlane(vector, planeNormal);
        [UsedImplicitly]
        public static Vector3Bridge Reflect(Vector3Bridge inDirection, Vector3Bridge inNormal) => Vector3.Reflect(inDirection, inNormal);
        [UsedImplicitly]
        public static Vector3Bridge RotateTowards(Vector3Bridge current, Vector3Bridge target, float maxRadiansDelta, float maxMagnitudeDelta) 
            => Vector3.RotateTowards(current, target, maxRadiansDelta, maxMagnitudeDelta);
        [UsedImplicitly]
        public static Vector3Bridge Scale(Vector3Bridge a, Vector3Bridge b) => Vector3.Scale(a, b);
        [UsedImplicitly]
        public static float SignedAngle(Vector3Bridge from, Vector3Bridge to, Vector3Bridge axis) => Vector3.SignedAngle(from, to, axis);
        [UsedImplicitly]
        public static Vector3Bridge Slerp(Vector3Bridge a, Vector3Bridge b, float t) => Vector3.Slerp(a, b, t);
        [UsedImplicitly]
        public static Vector3Bridge SlerpUnclamped(Vector3Bridge a, Vector3Bridge b, float t) => Vector3.SlerpUnclamped(a, b, t);
        
        [UsedImplicitly]
        public static Vector3Bridge SmoothDamp(Vector3Bridge current, Vector3Bridge target, ref Vector3Bridge currentVelocity, float smoothTime, float maxSpeed, float deltaTime)
        {
            Vector3 v = currentVelocity;
            Vector3Bridge ret = Vector3.SmoothDamp(current, target, ref v, smoothTime, maxSpeed, deltaTime);
            currentVelocity = v;
            return ret;
        }

        public static implicit operator Vector3Bridge(Vector3 vector3) => new Vector3Bridge(vector3);
        public static implicit operator Vector3(Vector3Bridge vector3Bridge) => new Vector3(vector3Bridge.x, vector3Bridge.y, vector3Bridge.z);
    }
}