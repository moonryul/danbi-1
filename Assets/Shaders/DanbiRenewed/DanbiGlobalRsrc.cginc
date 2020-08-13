//#include "Assets/Shaders/DanbiRenewed/DanbiMathUtils.cginc"
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

float4x4 _CameraToWorldMat;
float3 _CameraPosInWorld;
float3 _CameraViewDirection;

float4x4 _Projection;
float4x4 _CameraInverseProjection;

float2 _PixelOffset;

Texture2D<float4> _RoomTexture;
SamplerState sampler_RoomTexture;

Texture2D<float4> _PredisortedImage;
SamplerState sampler_PredistortedImage;

int _MaxBounce;

RWTexture2D<float4> _Result;

struct Ray {
  float3 curPixelIn;
  float3 direction;
  float3 localDirectionInCamera;
  float3 energy;
};

struct RayHit {
  float3 position; // hit position on the surface.
  float2 uvInTriangle; // relative barycentric coords of the hit position.
  float3x2 vertexUVs;
  float distance;
  float3 normal; // normal at the ray hit position.
  // Material properties.
  float3 albedo;
  float3 specular;
  float smoothness;
  float3 emission;

};

struct MeshAdditionalData {
  float4x4 localToWorld;
  float3 albedo;
  float3 specular;
  float smoothness;
  float3 emission;
  int indicesOffset;
  int indicesCount;
};

StructuredBuffer<MeshAdditionalData> _MeshAdditionalData;

/*
*  Behaviours
*/

Ray CreateRay(float3 curPixelIn, float3 direction, float3 localDirectionInCamera);
Ray CreateCameraRay(float2 undistortedNDC);
RayHit CreateRayHit();

/*
* Bodies
*/

Ray CreateRay(float3 curPixelIn, float3 direction, float3 localDirectionInCamera) {
  Ray res = (Ray)0;
  res.curPixelIn = curPixelIn;
  res.direction = direction;
  res.localDirectionInCamera = localDirectionInCamera;
  res.energy = float3(1.0f, 1.0f, 1.0f);
  return res;
}

Ray CreateCameraRay(float2 undistortedNDC) {
  uint width = 0, height = 0;
  _Result.GetDimensions(width, height);

  // Transform the camera curPixelIn onto the world space.
  float3 cameraCurPixelInWorld = mul(_CameraToWorldMat, float4(0.0f, 0.0f, 0.0f, 1.0f)).xyz;
  // Invert the perspective projection of the view-space position.
  float3 posInCameraZero = mul(_CameraInverseProjection, float4(undistortedNDC, 0.0f, 1.0f)).xyz;
  float3 localDirectionInCamera = normalize(posInCameraZero);
  // Transform the direction from camera to world space and normalize.
  float3 dirInWorld = mul(_CameraToWorldMat, float4(posInCameraZero, 0.0f)).xyz;
  dirInWorld = normalize(dirInWorld);

  return CreateRay(cameraCurPixelInWorld, dirInWorld, localDirectionInCamera);
}

RayHit CreateRayHit() {
  RayHit res = (RayHit)0;
  res.position = (float3)0;
  res.vertexUVs = (float3x2)0;
  res.distance = 1.#INF;
  res.normal = (float3)0;
  res.uvInTriangle = (float2)0;
  res.albedo = (float3)0;
  res.specular = (float3)0;
  res.smoothness = 0.0f;
  res.emission = (float3)0;

  return res;
}

// eof
