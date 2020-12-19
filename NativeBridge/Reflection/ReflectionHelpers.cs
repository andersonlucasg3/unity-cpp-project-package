using System;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UnityCpp.NativeBridge.Reflection
{
    public enum MemberType
    {
        constructor = 0,
        field,
        property,
        method
    }
    
    public static class ReflectionHelpers
    {
        public static IntPtr AllocMemberPtr(IntPtr typePtr, string name, MemberType memberType)
        {
            if (TryGetMember(typePtr, name, memberType, out MemberInfo info))
            {
                return (IntPtr) GCHandle.Alloc(info);
            }

            return IntPtr.Zero;
        }

        public static IntPtr AllocObjectPtr(object objectInstance)
        {
            if (objectInstance == null)
            {
                return IntPtr.Zero;
            }
            return (IntPtr) GCHandle.Alloc(objectInstance);
        }

        public static void DeallocPtr(IntPtr instancePtr)
        {
            ((GCHandle) instancePtr).Free();
        }

        public static TOutput ConvertPtrTo<TOutput>(IntPtr intPtr)
        {
            GCHandle handle = (GCHandle) intPtr;
            return (TOutput) handle.Target;
        }

        public static void GetObjectAndInfo<TInfo>(IntPtr objectPtr, IntPtr infoPtr, out object objectInstance, out TInfo info)
        {
            objectInstance = ConvertPtrTo<object>(objectPtr);
            info = ConvertPtrTo<TInfo>(infoPtr);
        }
        
        private static bool TryGetMember(IntPtr typePtr, string name, MemberType memberType, out MemberInfo memberInfo)
        {
            Type type = ConvertPtrTo<Type>(typePtr);
            const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            switch (memberType)
            {
                case MemberType.field: memberInfo = type.GetField(name, flags); break;
                case MemberType.property: memberInfo = type.GetProperty(name, flags); break;
                case MemberType.method: memberInfo = type.GetMethod(name, flags); break;
                default:
                    throw new MissingMemberException();
            }
            if (memberInfo != null) return true;
#if DEBUG
            Debug.Log($"Available members: {string.Join(", ", Array.ConvertAll(type.GetMembers(flags), each => $"{each.Name}: {each.GetType()}"))}");
#endif
            Debug.LogError($"Unmanaged code trying to get member {name} of type {type} that does not exists.");
            return false;
        }
    }
}