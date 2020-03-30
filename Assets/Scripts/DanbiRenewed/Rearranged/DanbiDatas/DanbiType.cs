namespace Danbi {

  // https://docs.microsoft.com/en-us/dotnet/visual-basic/language-reference/error-messages/return-type-of-function-procedurename-is-not-cls-compliant
  // UnityEngine.Vector2/3/4 is not CLS-compliant type -> causer.
  // TODO: Change these 3 structures to be recongized by compiler as CLS-compliant at last.

  // TODO: need to implement the SIMD acceleration.
  public struct Float2 {
    public float X, Y;

    #region Ctor
    public Float2(float x, float y) {
      X = x; Y = y;
    }

    public Float2(Float3 other) {
      X = other.X; Y = other.Y;
    }

    public Float2(Float4 other) {
      X = other.X; Y = other.Y;
    }

    public Float2(UnityEngine.Vector2 other) {
      X = other.x; Y = other.y;
    }

    public Float2(UnityEngine.Vector3 other) {
      X = other.x; Y = other.y;
    }

    public Float2(UnityEngine.Vector4 other) {
      X = other.x; Y = other.y;
    }
    #endregion

    #region Helpers
    public Float3 ToFloat3(float z = 0.0f) => new Float3(X, Y, z);

    public Float4 ToFloat4(float z = 0.0f, float w = 0.0f) => new Float4(X, Y, z, w);

    public UnityEngine.Vector2 ToVec2 => new UnityEngine.Vector2(X, Y);

    public UnityEngine.Vector3 ToVec3(float newZ = 0.0f) => new UnityEngine.Vector3(X, Y, newZ);

    public Float2 GetSafeNormalized() =>
      //Float2 res = Float2()
      default;
    #endregion

    #region Swizzle
    public Float2 XX => new Float2(X, X);
    public Float2 XY => new Float2(X, Y);

    public Float2 YX => new Float2(Y, X);
    public Float2 YY => new Float2(Y, Y);
    #endregion
  };

  // TODO: need to implement the SIMD acceleration.
  public struct Float3 {
    public float X, Y, Z;

    #region Ctor 
    public Float3(float x, float y, float z) {
      X = x; Y = y; Z = z;
    }

    public Float3(Float2 other, float z = 0.0f) {
      X = other.X; Y = other.Y; Z = z;
    }

    public Float3(Float3 other) {
      X = other.X; Y = other.Y; Z = other.Z;
    }

    public Float3(Float4 other) {
      X = other.X; Y = other.Y; Z = other.Z;
    }

    public Float3(UnityEngine.Vector2 other, float z = 0.0f) {
      X = other.x; Y = other.y; Z = z;
    }

    public Float3(UnityEngine.Vector3 other) {
      X = other.x; Y = other.y; Z = other.z;
    }

    public Float3(UnityEngine.Vector4 other) {
      X = other.x; Y = other.y; Z = other.z;
    }
    #endregion

    #region Helpers
    public Float2 ToFloat2 => new Float2(X, Y);
    public Float4 ToFloat4(float w = 0.0f) => new Float4(X, Y, Z, w);

    public UnityEngine.Vector2 ToVec2 => new UnityEngine.Vector2(X, Y);
    public UnityEngine.Vector3 ToVec3 => new UnityEngine.Vector3(X, Y, Z);
    public UnityEngine.Vector4 ToVec4(float w = 0.0f) => new UnityEngine.Vector4(X, Y, Z, w);
    #endregion

    #region Swizzle
    public Float3 XXX => new Float3(X, X, X);
    public Float3 XXY => new Float3(X, X, Y);
    public Float3 XXZ => new Float3(X, X, Z);

    public Float3 XYX => new Float3(X, Y, X);
    public Float3 XYY => new Float3(X, Y, Y);
    public Float3 XYZ => new Float3(X, Y, Z);

    public Float3 XZX => new Float3(X, Z, X);
    public Float3 XZY => new Float3(X, Z, Y);
    public Float3 XZZ => new Float3(X, Z, Z);

    public Float3 YXX => new Float3(Y, X, X);
    public Float3 YXY => new Float3(Y, X, Y);
    public Float3 YXZ => new Float3(Y, X, Z);

    public Float3 YYX => new Float3(Y, Y, X);
    public Float3 YYY => new Float3(Y, Y, Y);
    public Float3 YYZ => new Float3(Y, Y, Z);

    public Float3 YZX => new Float3(Y, Z, X);
    public Float3 YZY => new Float3(Y, Z, Y);
    public Float3 YZZ => new Float3(Y, Z, Z);

    public Float3 ZXX => new Float3(Z, X, X);
    public Float3 ZXY => new Float3(Z, X, Y);
    public Float3 ZXZ => new Float3(Z, X, Z);

    public Float3 ZYX => new Float3(Z, Y, X);
    public Float3 ZYY => new Float3(Z, Y, Y);
    public Float3 ZYZ => new Float3(Z, Y, Z);

    public Float3 ZZX => new Float3(Z, Z, X);
    public Float3 ZZY => new Float3(Z, Z, Y);
    public Float3 ZZZ => new Float3(Z, Z, Z);

    public Float2 XX => new Float2(X, X);
    public Float2 XY => new Float2(X, Y);
    public Float2 XZ => new Float2(X, Z);

    public Float2 YX => new Float2(Y, X);
    public Float2 YY => new Float2(Y, Y);
    public Float2 YZ => new Float2(Y, Z);

    public Float2 ZX => new Float2(Z, X);
    public Float2 ZY => new Float2(Z, Y);
    public Float2 ZZ => new Float2(Z, Z);
    #endregion
  };

  // TODO: need to implement the SIMD acceleration.
  public struct Float4 {
    public float X, Y, Z, W;

    #region Ctor
    public Float4(float x, float y, float z, float w) {
      X = x; Y = y; Z = z; W = w;
    }

    public Float4(Float2 other, float z = 0.0f, float w = 0.0f) {
      X = other.X; Y = other.Y; Z = z; W = w;
    }

    public Float4(Float3 other, float w = 0.0f) {
      X = other.X; Y = other.Y; Z = other.Z; W = w;
    }

    public Float4(Float4 other) {
      X = other.X; Y = other.Y; Z = other.Z; W = other.W;
    }

    public Float4(UnityEngine.Vector2 other, float z = 0.0f, float w = 0.0f) {
      X = other.x; Y = other.y; Z = z; W = w;
    }

    public Float4(UnityEngine.Vector3 other, float w = 0.0f) {
      X = other.x; Y = other.y; Z = other.z; W = w;
    }

    public Float4(UnityEngine.Vector4 other) {
      X = other.x; Y = other.y; Z = other.z; W = other.w;
    }
    #endregion

    #region Helper
    public Float2 ToFloat2 => new Float2(X, Y);
    public Float3 ToFloat3 => new Float3(X, Y, Z);
    public UnityEngine.Vector2 ToVec2 => new UnityEngine.Vector2(X, Y);
    public UnityEngine.Vector3 ToVec3 => new UnityEngine.Vector3(X, Y, Z);
    public UnityEngine.Vector4 ToVec4 => new UnityEngine.Vector4(X, Y, Z, W);
    #endregion

    #region Swizzle
    //
    // X---
    //     
    public Float4 XXXX => new Float4(X, X, X, X);
    public Float4 XXXY => new Float4(X, X, X, Y);
    public Float4 XXXZ => new Float4(X, X, X, Z);
    public Float4 XXXW => new Float4(X, X, X, W);

    public Float4 XXYX => new Float4(X, X, Y, X);
    public Float4 XXYY => new Float4(X, X, Y, Y);
    public Float4 XXYZ => new Float4(X, X, Y, Z);
    public Float4 XXYW => new Float4(X, X, Y, W);

    public Float4 XXZX => new Float4(X, X, Z, X);
    public Float4 XXZY => new Float4(X, X, Z, Y);
    public Float4 XXZZ => new Float4(X, X, Z, Z);
    public Float4 XXZW => new Float4(X, X, Z, W);

    public Float4 XXWX => new Float4(X, X, W, X);
    public Float4 XXWY => new Float4(X, X, W, Y);
    public Float4 XXWZ => new Float4(X, X, W, Z);
    public Float4 XXWW => new Float4(X, X, W, W);

    public Float4 XYXX => new Float4(X, Y, X, X);
    public Float4 XYXY => new Float4(X, Y, X, Y);
    public Float4 XYXZ => new Float4(X, Y, X, Z);
    public Float4 XYXW => new Float4(X, Y, X, W);

    public Float4 XYYX => new Float4(X, Y, Y, X);
    public Float4 XYYY => new Float4(X, Y, Y, Y);
    public Float4 XYYZ => new Float4(X, Y, Y, Z);
    public Float4 XYYW => new Float4(X, Y, Y, W);

    public Float4 XYZX => new Float4(X, Y, Z, X);
    public Float4 XYZY => new Float4(X, Y, Z, Y);
    public Float4 XYZZ => new Float4(X, Y, Z, Z);
    public Float4 XYZW => new Float4(X, Y, Z, W);

    public Float4 XYWX => new Float4(X, Y, W, X);
    public Float4 XYWY => new Float4(X, Y, W, Y);
    public Float4 XYWZ => new Float4(X, Y, W, Z);
    public Float4 XYWW => new Float4(X, Y, W, W);

    public Float4 XZXX => new Float4(X, Z, X, X);
    public Float4 XZXY => new Float4(X, Z, X, Y);
    public Float4 XZXZ => new Float4(X, Z, X, Z);
    public Float4 XZXW => new Float4(X, Z, X, W);

    public Float4 XZYX => new Float4(X, Z, Y, X);
    public Float4 XZYY => new Float4(X, Z, Y, Y);
    public Float4 XZYZ => new Float4(X, Z, Y, Z);
    public Float4 XZYW => new Float4(X, Z, Y, W);

    public Float4 XZZX => new Float4(X, Z, Z, X);
    public Float4 XZZY => new Float4(X, Z, Z, Y);
    public Float4 XZZZ => new Float4(X, Z, Z, Z);
    public Float4 XZZW => new Float4(X, Z, Z, W);

    public Float4 XZWX => new Float4(X, Z, W, X);
    public Float4 XZWY => new Float4(X, Z, W, Y);
    public Float4 XZWZ => new Float4(X, Z, W, Z);
    public Float4 XZWW => new Float4(X, Z, W, W);

    public Float4 XWXX => new Float4(X, W, X, X);
    public Float4 XWXY => new Float4(X, W, X, Y);
    public Float4 XWXZ => new Float4(X, W, X, Z);
    public Float4 XWXW => new Float4(X, W, X, W);

    public Float4 XWYX => new Float4(X, W, Y, X);
    public Float4 XWYY => new Float4(X, W, Y, Y);
    public Float4 XWYZ => new Float4(X, W, Y, Z);
    public Float4 XWYW => new Float4(X, W, Y, W);

    public Float4 XWZX => new Float4(X, W, Z, X);
    public Float4 XWZY => new Float4(X, W, Z, Y);
    public Float4 XWZZ => new Float4(X, W, Z, Z);
    public Float4 XWZW => new Float4(X, W, Z, W);

    public Float4 XWWX => new Float4(X, W, W, X);
    public Float4 XWWY => new Float4(X, W, W, Y);
    public Float4 XWWZ => new Float4(X, W, W, Z);
    public Float4 XWWW => new Float4(X, W, W, W);
    //
    // Y---
    // 
    public Float4 YXXX => new Float4(Y, X, X, X);
    public Float4 YXXY => new Float4(Y, X, X, Y);
    public Float4 YXXZ => new Float4(Y, X, X, Z);
    public Float4 YXXW => new Float4(Y, X, X, W);
                  
    public Float4 YXYX => new Float4(Y, X, Y, X);
    public Float4 YXYY => new Float4(Y, X, Y, Y);
    public Float4 YXYZ => new Float4(Y, X, Y, Z);
    public Float4 YXYW => new Float4(Y, X, Y, W);
                  
    public Float4 YXZX => new Float4(Y, X, Z, X);
    public Float4 YXZY => new Float4(Y, X, Z, Y);
    public Float4 YXZZ => new Float4(Y, X, Z, Z);
    public Float4 YXZW => new Float4(Y, X, Z, W);
                  
    public Float4 YXWX => new Float4(Y, X, W, X);
    public Float4 YXWY => new Float4(Y, X, W, Y);
    public Float4 YXWZ => new Float4(Y, X, W, Z);
    public Float4 YXWW => new Float4(Y, X, W, W);
                  
    public Float4 YYXX => new Float4(Y, Y, X, X);
    public Float4 YYXY => new Float4(Y, Y, X, Y);
    public Float4 YYXZ => new Float4(Y, Y, X, Z);
    public Float4 YYXW => new Float4(Y, Y, X, W);
                  
    public Float4 YYYX => new Float4(Y, Y, Y, X);
    public Float4 YYYY => new Float4(Y, Y, Y, Y);
    public Float4 YYYZ => new Float4(Y, Y, Y, Z);
    public Float4 YYYW => new Float4(Y, Y, Y, W);
                  
    public Float4 YYZX => new Float4(Y, Y, Z, X);
    public Float4 YYZY => new Float4(Y, Y, Z, Y);
    public Float4 YYZZ => new Float4(Y, Y, Z, Z);
    public Float4 YYZW => new Float4(Y, Y, Z, W);
                  
    public Float4 YYWX => new Float4(Y, Y, W, X);
    public Float4 YYWY => new Float4(Y, Y, W, Y);
    public Float4 YYWZ => new Float4(Y, Y, W, Z);
    public Float4 YYWW => new Float4(Y, Y, W, W);
                  
    public Float4 YZXX => new Float4(Y, Z, X, X);
    public Float4 YZXY => new Float4(Y, Z, X, Y);
    public Float4 YZXZ => new Float4(Y, Z, X, Z);
    public Float4 YZXW => new Float4(Y, Z, X, W);
                  
    public Float4 YZYX => new Float4(Y, Z, Y, X);
    public Float4 YZYY => new Float4(Y, Z, Y, Y);
    public Float4 YZYZ => new Float4(Y, Z, Y, Z);
    public Float4 YZYW => new Float4(Y, Z, Y, W);
                  
    public Float4 YZZX => new Float4(Y, Z, Z, X);
    public Float4 YZZY => new Float4(Y, Z, Z, Y);
    public Float4 YZZZ => new Float4(Y, Z, Z, Z);
    public Float4 YZZW => new Float4(Y, Z, Z, W);
                  
    public Float4 YZWX => new Float4(Y, Z, W, X);
    public Float4 YZWY => new Float4(Y, Z, W, Y);
    public Float4 YZWZ => new Float4(Y, Z, W, Z);
    public Float4 YZWW => new Float4(Y, Z, W, W);
                  
    public Float4 YWXX => new Float4(Y, W, X, X);
    public Float4 YWXY => new Float4(Y, W, X, Y);
    public Float4 YWXZ => new Float4(Y, W, X, Z);
    public Float4 YWXW => new Float4(Y, W, X, W);
                  
    public Float4 YWYX => new Float4(Y, W, Y, X);
    public Float4 YWYY => new Float4(Y, W, Y, Y);
    public Float4 YWYZ => new Float4(Y, W, Y, Z);
    public Float4 YWYW => new Float4(Y, W, Y, W);
                  
    public Float4 YWZX => new Float4(Y, W, Z, X);
    public Float4 YWZY => new Float4(Y, W, Z, Y);
    public Float4 YWZZ => new Float4(Y, W, Z, Z);
    public Float4 YWZW => new Float4(Y, W, Z, W);
                  
    public Float4 YWWX => new Float4(Y, W, W, X);
    public Float4 YWWY => new Float4(Y, W, W, Y);
    public Float4 YWWZ => new Float4(Y, W, W, Z);
    public Float4 YWWW => new Float4(Y, W, W, W);
    //
    // Z---
    //
    public Float4 ZXXX => new Float4(Z, X, X, X);
    public Float4 ZXXY => new Float4(Z, X, X, Y);
    public Float4 ZXXZ => new Float4(Z, X, X, Z);
    public Float4 ZXXW => new Float4(Z, X, X, W);
                  
    public Float4 ZXYX => new Float4(Z, X, Y, X);
    public Float4 ZXYY => new Float4(Z, X, Y, Y);
    public Float4 ZXYZ => new Float4(Z, X, Y, Z);
    public Float4 ZXYW => new Float4(Z, X, Y, W);
                  
    public Float4 ZXZX => new Float4(Z, X, Z, X);
    public Float4 ZXZY => new Float4(Z, X, Z, Y);
    public Float4 ZXZZ => new Float4(Z, X, Z, Z);
    public Float4 ZXZW => new Float4(Z, X, Z, W);
                  
    public Float4 ZXWX => new Float4(Z, X, W, X);
    public Float4 ZXWY => new Float4(Z, X, W, Y);
    public Float4 ZXWZ => new Float4(Z, X, W, Z);
    public Float4 ZXWW => new Float4(Z, X, W, W);
                  
    public Float4 ZYXX => new Float4(Z, Y, X, X);
    public Float4 ZYXY => new Float4(Z, Y, X, Y);
    public Float4 ZYXZ => new Float4(Z, Y, X, Z);
    public Float4 ZYXW => new Float4(Z, Y, X, W);
                  
    public Float4 ZYYX => new Float4(Z, Y, Y, X);
    public Float4 ZYYY => new Float4(Z, Y, Y, Y);
    public Float4 ZYYZ => new Float4(Z, Y, Y, Z);
    public Float4 ZYYW => new Float4(Z, Y, Y, W);
                  
    public Float4 ZYZX => new Float4(Z, Y, Z, X);
    public Float4 ZYZY => new Float4(Z, Y, Z, Y);
    public Float4 ZYZZ => new Float4(Z, Y, Z, Z);
    public Float4 ZYZW => new Float4(Z, Y, Z, W);
                  
    public Float4 ZYWX => new Float4(Z, Y, W, X);
    public Float4 ZYWY => new Float4(Z, Y, W, Y);
    public Float4 ZYWZ => new Float4(Z, Y, W, Z);
    public Float4 ZYWW => new Float4(Z, Y, W, W);
                  
    public Float4 ZZXX => new Float4(Z, Z, X, X);
    public Float4 ZZXY => new Float4(Z, Z, X, Y);
    public Float4 ZZXZ => new Float4(Z, Z, X, Z);
    public Float4 ZZXW => new Float4(Z, Z, X, W);
                  
    public Float4 ZZYX => new Float4(Z, Z, Y, X);
    public Float4 ZZYY => new Float4(Z, Z, Y, Y);
    public Float4 ZZYZ => new Float4(Z, Z, Y, Z);
    public Float4 ZZYW => new Float4(Z, Z, Y, W);
                  
    public Float4 ZZZX => new Float4(Z, Z, Z, X);
    public Float4 ZZZY => new Float4(Z, Z, Z, Y);
    public Float4 ZZZZ => new Float4(Z, Z, Z, Z);
    public Float4 ZZZW => new Float4(Z, Z, Z, W);
                  
    public Float4 ZZWX => new Float4(Z, Z, W, X);
    public Float4 ZZWY => new Float4(Z, Z, W, Y);
    public Float4 ZZWZ => new Float4(Z, Z, W, Z);
    public Float4 ZZWW => new Float4(Z, Z, W, W);
                  
    public Float4 ZWXX => new Float4(Z, W, X, X);
    public Float4 ZWXY => new Float4(Z, W, X, Y);
    public Float4 ZWXZ => new Float4(Z, W, X, Z);
    public Float4 ZWXW => new Float4(Z, W, X, W);
                  
    public Float4 ZWYX => new Float4(Z, W, Y, X);
    public Float4 ZWYY => new Float4(Z, W, Y, Y);
    public Float4 ZWYZ => new Float4(Z, W, Y, Z);
    public Float4 ZWYW => new Float4(Z, W, Y, W);
                  
    public Float4 ZWZX => new Float4(Z, W, Z, X);
    public Float4 ZWZY => new Float4(Z, W, Z, Y);
    public Float4 ZWZZ => new Float4(Z, W, Z, Z);
    public Float4 ZWZW => new Float4(Z, W, Z, W);
                  
    public Float4 ZWWX => new Float4(Z, W, W, X);
    public Float4 ZWWY => new Float4(Z, W, W, Y);
    public Float4 ZWWZ => new Float4(Z, W, W, Z);
    public Float4 ZWWW => new Float4(Z, W, W, W);
    //
    // W---
    //
    public Float4 WXXX => new Float4(W, X, X, X);
    public Float4 WXXY => new Float4(W, X, X, Y);
    public Float4 WXXZ => new Float4(W, X, X, Z);
    public Float4 WXXW => new Float4(W, X, X, W);
                  
    public Float4 WXYX => new Float4(W, X, Y, X);
    public Float4 WXYY => new Float4(W, X, Y, Y);
    public Float4 WXYZ => new Float4(W, X, Y, Z);
    public Float4 WXYW => new Float4(W, X, Y, W);
                  
    public Float4 WXZX => new Float4(W, X, Z, X);
    public Float4 WXZY => new Float4(W, X, Z, Y);
    public Float4 WXZZ => new Float4(W, X, Z, Z);
    public Float4 WXZW => new Float4(W, X, Z, W);
                  
    public Float4 WXWX => new Float4(W, X, W, X);
    public Float4 WXWY => new Float4(W, X, W, Y);
    public Float4 WXWZ => new Float4(W, X, W, Z);
    public Float4 WXWW => new Float4(W, X, W, W);
                  
    public Float4 WYXX => new Float4(W, Y, X, X);
    public Float4 WYXY => new Float4(W, Y, X, Y);
    public Float4 WYXZ => new Float4(W, Y, X, Z);
    public Float4 WYXW => new Float4(W, Y, X, W);
                  
    public Float4 WYYX => new Float4(W, Y, Y, X);
    public Float4 WYYY => new Float4(W, Y, Y, Y);
    public Float4 WYYZ => new Float4(W, Y, Y, Z);
    public Float4 WYYW => new Float4(W, Y, Y, W);
                  
    public Float4 WYZX => new Float4(W, Y, Z, X);
    public Float4 WYZY => new Float4(W, Y, Z, Y);
    public Float4 WYZZ => new Float4(W, Y, Z, Z);
    public Float4 WYZW => new Float4(W, Y, Z, W);
                  
    public Float4 WYWX => new Float4(W, Y, W, X);
    public Float4 WYWY => new Float4(W, Y, W, Y);
    public Float4 WYWZ => new Float4(W, Y, W, Z);
    public Float4 WYWW => new Float4(W, Y, W, W);
                  
    public Float4 WZXX => new Float4(W, Z, X, X);
    public Float4 WZXY => new Float4(W, Z, X, Y);
    public Float4 WZXZ => new Float4(W, Z, X, Z);
    public Float4 WZXW => new Float4(W, Z, X, W);
                  
    public Float4 WZYX => new Float4(W, Z, Y, X);
    public Float4 WZYY => new Float4(W, Z, Y, Y);
    public Float4 WZYZ => new Float4(W, Z, Y, Z);
    public Float4 WZYW => new Float4(W, Z, Y, W);
                  
    public Float4 WZZX => new Float4(W, Z, Z, X);
    public Float4 WZZY => new Float4(W, Z, Z, Y);
    public Float4 WZZZ => new Float4(W, Z, Z, Z);
    public Float4 WZZW => new Float4(W, Z, Z, W);
                  
    public Float4 WZWX => new Float4(W, Z, W, X);
    public Float4 WZWY => new Float4(W, Z, W, Y);
    public Float4 WZWZ => new Float4(W, Z, W, Z);
    public Float4 WZWW => new Float4(W, Z, W, W);
                  
    public Float4 WWXX => new Float4(W, W, X, X);
    public Float4 WWXY => new Float4(W, W, X, Y);
    public Float4 WWXZ => new Float4(W, W, X, Z);
    public Float4 WWXW => new Float4(W, W, X, W);
                  
    public Float4 WWYX => new Float4(W, W, Y, X);
    public Float4 WWYY => new Float4(W, W, Y, Y);
    public Float4 WWYZ => new Float4(W, W, Y, Z);
    public Float4 WWYW => new Float4(W, W, Y, W);
                  
    public Float4 WWZX => new Float4(W, W, Z, X);
    public Float4 WWZY => new Float4(W, W, Z, Y);
    public Float4 WWZZ => new Float4(W, W, Z, Z);
    public Float4 WWZW => new Float4(W, W, Z, W);
                  
    public Float4 WWWX => new Float4(W, W, W, X);
    public Float4 WWWY => new Float4(W, W, W, Y);
    public Float4 WWWZ => new Float4(W, W, W, Z);
    public Float4 WWWW => new Float4(W, W, W, W);

    public Float3 XXX => new Float3(X, X, X);
    public Float3 XXY => new Float3(X, X, Y);
    public Float3 XXZ => new Float3(X, X, Z);
    public Float3 XXW => new Float3(X, X, W);

    public Float3 XYX => new Float3(X, Y, X);
    public Float3 XYY => new Float3(X, Y, Y);
    public Float3 XYZ => new Float3(X, Y, Z);
    public Float3 XYW => new Float3(X, Y, W);

    public Float3 XZX => new Float3(X, Z, X);
    public Float3 XZY => new Float3(X, Z, Y);
    public Float3 XZZ => new Float3(X, Z, Z);
    public Float3 XZW => new Float3(X, Z, W);

    public Float3 XWX => new Float3(X, W, X);
    public Float3 XWY => new Float3(X, W, Y);
    public Float3 XWZ => new Float3(X, W, Z);
    public Float3 XWW => new Float3(X, W, W);

    public Float3 YXX => new Float3(Y, X, X);
    public Float3 YXY => new Float3(Y, X, Y);
    public Float3 YXZ => new Float3(Y, X, Z);
    public Float3 YXW => new Float3(Y, X, W);

    public Float3 YYX => new Float3(Y, Y, X);
    public Float3 YYY => new Float3(Y, Y, Y);
    public Float3 YYZ => new Float3(Y, Y, Z);
    public Float3 YYW => new Float3(Y, Y, W);

    public Float3 YZX => new Float3(Y, Z, X);
    public Float3 YZY => new Float3(Y, Z, Y);
    public Float3 YZZ => new Float3(Y, Z, Z);
    public Float3 YZW => new Float3(Y, Z, W);

    public Float3 YWX => new Float3(Y, W, X);
    public Float3 YWY => new Float3(Y, W, Y);
    public Float3 YWZ => new Float3(Y, W, Z);
    public Float3 YWW => new Float3(Y, W, W);

    public Float3 ZXX => new Float3(Z, X, X);
    public Float3 ZXY => new Float3(Z, X, Y);
    public Float3 ZXZ => new Float3(Z, X, Z);
    public Float3 ZXW => new Float3(Z, X, W);

    public Float3 ZYX => new Float3(Z, Y, X);
    public Float3 ZYY => new Float3(Z, Y, Y);
    public Float3 ZYZ => new Float3(Z, Y, Z);
    public Float3 ZYW => new Float3(Z, Y, W);

    public Float3 ZZX => new Float3(Z, Z, X);
    public Float3 ZZY => new Float3(Z, Z, Y);
    public Float3 ZZZ => new Float3(Z, Z, Z);
    public Float3 ZZW => new Float3(Z, Z, W);

    public Float3 ZWX => new Float3(Z, W, X);
    public Float3 ZWY => new Float3(Z, W, Y);
    public Float3 ZWZ => new Float3(Z, W, Z);
    public Float3 ZWW => new Float3(Z, W, W);

    public Float3 WXX => new Float3(W, X, X);
    public Float3 WXY => new Float3(W, X, Y);
    public Float3 WXZ => new Float3(W, X, Z);
    public Float3 WXW => new Float3(W, X, W);

    public Float3 WYX => new Float3(W, Y, X);
    public Float3 WYY => new Float3(W, Y, Y);
    public Float3 WYZ => new Float3(W, Y, Z);
    public Float3 WYW => new Float3(W, Y, W);

    public Float3 WZX => new Float3(W, Z, X);
    public Float3 WZY => new Float3(W, Z, Y);
    public Float3 WZZ => new Float3(W, Z, Z);
    public Float3 WZW => new Float3(W, Z, W);

    public Float3 WWX => new Float3(W, W, X);
    public Float3 WWY => new Float3(W, W, Y);
    public Float3 WWZ => new Float3(W, W, Z);
    public Float3 WWW => new Float3(W, W, W);

    public Float2 XX => new Float2(X, X);
    public Float2 XY => new Float2(X, Y);
    public Float2 XZ => new Float2(X, Z);
    public Float2 XW => new Float2(X, W);

    public Float2 YY => new Float2(Y, Y);
    public Float2 YZ => new Float2(Y, Z);
    public Float2 YX => new Float2(Y, X);
    public Float2 YW => new Float2(Y, W);

    public Float2 ZZ => new Float2(Z, Z);
    public Float2 ZY => new Float2(Z, Y);
    public Float2 ZX => new Float2(Z, X);
    public Float2 ZW => new Float2(Z, W);

    public Float2 WW => new Float2(W, W);
    public Float2 WX => new Float2(W, X);
    public Float2 WY => new Float2(W, Y);
    public Float2 WZ => new Float2(W, Z);
    #endregion
  };
}; // namespace Danbi