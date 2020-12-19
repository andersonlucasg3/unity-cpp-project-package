using System;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace UnityCpp.NativeBridge
{
    [StructLayout(LayoutKind.Sequential)]
    public struct UnmanagedValue
    {
        private static readonly byte[] byteBuffer = new byte[1];
        private static readonly short[] shortBuffer = new short[1];
        private static readonly int[] intBuffer = new int[1];
        private static readonly long[] longBuffer = new long[1];
        private static readonly float[] floatBuffer = new float[1];
        private static readonly double[] doubleBuffer = new double[1];
        
        [UsedImplicitly]
        private IntPtr _value;
        [UsedImplicitly]
        private readonly Type _type;

        public UnmanagedValue(IntPtr value, Type type)
        {
            _value = value;
            _type = type;
        }
        
        [UsedImplicitly]
        public void FromManaged(object input)
        {
            if (input == null)
            {
                _value = IntPtr.Zero;
                return;
            }
            switch (_type)
            {
                case Type.boolType:
                case Type.byteType:
                    byteBuffer[0] = (byte)input;
                    Marshal.Copy(byteBuffer, 0, _value, 1);
                    break;
                
                case Type.shortType:
                case Type.ushortType:
                    shortBuffer[0] = (short) input;
                    Marshal.Copy(shortBuffer, 0, _value, 1);
                    break;
                
                case Type.intType:
                case Type.uintType:
                    intBuffer[0] = (int) input;
                    Marshal.Copy(intBuffer, 0, _value, 1);
                    break;
                
                case Type.longType:
                case Type.ulongType:
                    longBuffer[0] = (long) input;
                    Marshal.Copy(longBuffer, 0, _value, 1);
                    break;
                
                case Type.floatType:
                    floatBuffer[0] = (float) input;
                    Marshal.Copy(floatBuffer, 0, _value, 1);
                    break;
                
                case Type.doubleType:
                    doubleBuffer[0] = (double) input;
                    Marshal.Copy(doubleBuffer, 0, _value, 1);
                    break;
                
                case Type.stringType:
                    _value = Marshal.StringToHGlobalAnsi((string)input);
                    break;
                
                case Type.pointerType:
                    _value = (IntPtr) GCHandle.Alloc(input);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }   
        }

        [UsedImplicitly]
        public object ToManaged()
        {
            switch (_type)
            {
                case Type.boolType:
                    Marshal.Copy(_value, byteBuffer, 0, 1);
                    return byteBuffer[0] != 0;

                case Type.byteType:
                    Marshal.Copy(_value, byteBuffer, 0, 1);
                    return byteBuffer[0];

                case Type.shortType:
                case Type.ushortType:
                    Marshal.Copy(_value, shortBuffer, 0, 1);
                    return shortBuffer[0];

                case Type.intType:
                case Type.uintType:
                    Marshal.Copy(_value, intBuffer, 0, 1);
                    return intBuffer[0];

                case Type.longType:
                case Type.ulongType:
                    Marshal.Copy(_value, longBuffer, 0, 1);
                    return longBuffer[0];

                case Type.floatType:
                    Marshal.Copy(_value, floatBuffer, 0, 1);
                    return floatBuffer[0];

                case Type.doubleType:
                    Marshal.Copy(_value, doubleBuffer, 0, 1);
                    return doubleBuffer[0];

                case Type.stringType:
                    return Marshal.PtrToStringAnsi(_value);

                case Type.pointerType:
                    return ConvertPtrTo<object>(_value);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static TOutput ConvertPtrTo<TOutput>(IntPtr intPtr)
        {
            GCHandle handle = (GCHandle) intPtr;
            return (TOutput) handle.Target;
        }

        [UsedImplicitly]
        public enum Type
        {
            boolType = 0,
            byteType,
            shortType,
            ushortType,
            intType,
            uintType,
            longType,
            ulongType,
            floatType,
            doubleType,
            stringType,
            pointerType
        }
    }
}