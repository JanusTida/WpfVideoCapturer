using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace CDFCStatic.CMethods {
    public static class PtrInvokeMethods {
        public static IntPtr GetPtrFromStructure<T>(this T structure, short level = 1) where T : struct {
            int sizeOfStructure = Marshal.SizeOf(typeof(T));
            int sizeOfIntPtr = Marshal.SizeOf(typeof(IntPtr));

            IntPtr ptr = Marshal.AllocHGlobal(sizeOfStructure);
            Marshal.StructureToPtr(structure, ptr, true);

            try {
                for (int index = 1; index < level; index++) {
                    var innerPtr = Marshal.AllocHGlobal(sizeOfIntPtr);
                    Marshal.StructureToPtr(ptr, innerPtr, true);
                    Marshal.FreeHGlobal(innerPtr);
                    ptr = innerPtr;
                }
                return ptr;
            }
            catch (Exception ex) {
                throw ex;
            }
        }

        /// <summary>
        /// 由指针类型转为结构体类型;
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="ptr"></param>
        /// <param name="level">//其中level为传入指针的级别;</param>
        /// <returns></returns>
        [HandleProcessCorruptedStateExceptions]
        public static T GetStructure<T>(this IntPtr ptr, short level = 1) where T : struct {
            T entity;
            IntPtr resPtr = ptr;
            for (int index = 1; index < level; index++) {
                var innerPtr = Marshal.PtrToStructure<IntPtr>(resPtr);
                resPtr = innerPtr;
            }
            try { 
                entity = (T)Marshal.PtrToStructure(resPtr, typeof(T));
            }
            catch(AccessViolationException ex) {
                EventLogger.Logger.WriteLine("PtrToStructure<>出错!" + ex.Message + ex.Source);
                throw ex;
            }
            return entity;
        }
    }
}
