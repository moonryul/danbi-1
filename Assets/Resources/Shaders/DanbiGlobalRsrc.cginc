
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

#ifndef DANBI_GLOBAL_RSRC
#define DANBI_GLOBAL_RSRC

float4x4 _CameraToWorldMat;
float3 CameraPosInWorld; /*Internal*/
float3 CameraViewDirection; /*Internal*/

float4x4 _Projection;
float4x4 _CameraInverseProjection;

float2 _PixelOffset;

Texture2D<float4> _PanoramaImage;
SamplerState sampler_PanoramaImage;

//Texture2D<float4> _PredisortedImage;
//SamplerState sampler_PredistortedImage;

int _MaxBounce;

RWTexture2D<float4> _DistortedImage;

struct Ray {
  float3 origin;
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
  float3 specular;
};

/*
*  Behaviours
*/

Ray CreateRay(float3 origin, float3 direction, float3 localDirectionInCamera);
Ray CreateCameraRay(float2 undistortedNDC);
RayHit CreateRayHit();
float3 Shade(inout Ray ray, RayHit resHit);

/*
* Bodies
*/

Ray CreateRay(float3 origin, float3 direction, float3 localDirectionInCamera) {
  Ray res = (Ray)0;
  res.origin = origin;
  res.direction = direction;
  res.localDirectionInCamera = localDirectionInCamera;
  res.energy = float3(1.0f, 1.0f, 1.0f);
  return res;
}

Ray CreateCameraRay(float2 undistortedNDC) {
  uint width = 0, height = 0;
  _DistortedImage.GetDimensions(width, height);

  // Transform the camera origin onto the world space.
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
  res.specular = (float3)0;

  return res;
}

float3 Shade(inout Ray ray, RayHit resHit) {
  uint width = 0, height = 0;
  _DistortedImage.GetDimensions(width, height);
  
  ray.origin = resHit.position + resHit.normal * 0.001;
  ray.direction = reflect(ray.direction, resHit.normal);
  ray.energy *= resHit.specular;
  
  float2 uv = resHit.uvInTriangle;
  float2 uvTex = (1 - uv[0] - uv[1]) * resHit.vertexUVs[0]
               + uv[0] * resHit.vertexUVs[1]
               + uv[1] * resHit.vertexUVs[2];
  return _PanoramaImage.SampleLevel(sampler_PanoramaImage, uvTex, 0).xyz;
}

// eof
#endif