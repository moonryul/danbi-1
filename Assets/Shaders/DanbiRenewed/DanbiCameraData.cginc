/*
* Fields
*/

float _FOV_Rad;

int _IterativeCounter;
int _IterativeSafeCounter;
float4 _IterativeThreshold;

struct CameraLensDistortionParams {
  float3 RadialCoefficient;
  float2 TangentialCoefficient;
  float2 PrincipalPoint;
  float2 FocalLength;
  float SkewCoefficient; // Rarely used recently since cameras has been becoming better.
};

StructuredBuffer<CameraLensDistortionParams> _CameraLensDistortionParams;

/*
* Behaviours
*/

//
//
//
float2 normalize(float x_u, float y_u, in CameraLensDistortionParams param);
//
//
//
float2 denormalize(float x_u, float y_u, in CameraLensDistortionParams param);
//
//
//
float2 distort_normalized(float x_nu, float y_nu, in CameraLensDistortionParams param);


/*
* Bodies
*/

float2 normalize(float x_u, float y_u, in CameraLensDistortionParams param) {
  float fx = param.FocalLength.x;
  float fy = param.FocalLength.y;
  float cx = param.PrincipalPoint.x;
  float cy = param.PrincipalPoint.y;

  // return float2(x_n, y_n).
  return float2((y_u - cy) / fy, (x_u - cx) / fx);
}

float2 denormalize(float x_u, float y_u, in CameraLensDistortionParams param) {
  float fx = param.FocalLength.x;
  float fy = param.FocalLength.y;

  float cx = param.PrincipalPoint.x;
  float cy = param.PrincipalPoint.y;

  return float2((fx * x) + cx, (fy * y) + cy);

}

float2 distort_normalized(float x_nu, float y_nu, in CameraLensDistortionParams param) {
  float k1 = param.RadialCoefficient.x;
  float k2 = param.RadialCoefficient.y;
  // k3 is in no use.

  float p1 = param.TangentialCoefficient.x;
  float p2 = param.TangentialCoefficient.y;

  float r2 = x_nu * x_nu + y_nu * y_nu;

  float radial_d = 1.0f +
                   (k1 * r2) +
                   (k2 * r2 * r2);

  float x_nd = (radial_d * x_nu) + 
               (2 * p1 * x_nu * y_nu) +
               (p2 * (r2 + 2 * x_nu * y_nu));

  float y_nd = (radial_d * y_nu) +
               (p1 * (r2 + 2 * y_nu * y_nu)) +
               (2 * p2 * x_nu * y_nu);
  return float2(x_nd, y_nd);
}