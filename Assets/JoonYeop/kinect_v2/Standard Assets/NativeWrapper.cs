using System;

namespace Helper {
  public static class NativeWrapper {
    public static System.IntPtr GetNativePtr(Object obj) {
      if (obj == null) {
        return System.IntPtr.Zero;
      }

      var nativeWrapperIface = obj as INativeWrapper;
      if (nativeWrapperIface != null) {
        return nativeWrapperIface.nativePtr;
      } else {
        throw new ArgumentException("Object must wrap native type");
      }
    }
  }
}