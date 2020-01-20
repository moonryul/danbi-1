#include "UnityCG.cginc"

struct Ray {
  float3 origin;
  float3 direction;
  float3 energy;
};

struct RayHit {
  float3 position;
  float distance;
  float3 normal;
  float3 albedo;
  float3 specular;
  float3 emission;
};

struct MeshObject {
  float4x4 localToWorldMatrix;
  int indicesStride;
  int indicesCount;
  int colorMode;
  
};

float4x4 _CameraToWorldSpace;
float4x4 _CameraInverseProjection;
float4x4 _DisplayCameraTRS;
float2 _PixelOffset;
int _MaxBounceCount;
float _FOV; // in radian added by Moon

RWTexture2D<float4> _Result;

static const float PI2 = 6.28318528;
static const float PI = 3.14159265;
static const int EPS = 1e-8;

Ray CreateRay(float3 origin, float3 direction);
RayHit CreateRayHit();
Ray CreateRayFromCamera(float2 uv);

RayHit Trace(Ray ray);
float3 Shade(inout Ray ray, RayHit hit);

bool IntersectTriangle_internal(Ray ray, float3x3 vtx, inout float3 tuv, int cullMode);
void IntersectTriangle(Ray ray, inout RayHit bestHit, MeshObject mesh);
void IntersectMesh(Ray ray, inout RayHit bestHit, MeshObject mesh);
void IntersectProjectorQuadMesh(Ray ray, inout RayHit bestHit, MeshObject mesh);

Ray CreateRay(float3 origin, float3 direction) {
  Ray ray = (Ray)0;
  ray.origin = origin;
  ray.direction = direction;
  ray.energy = (float3)1;
  return ray;
}

RayHit CreateRayHit() {
  RayHit hit = (RayHit)0;
  hit.position = (float3)0;
  hit.distance = 1.#INF;
  hit.normal = (float3)0;
  hit.albedo = (float3)0;
  hit.specular = (float3)0;
  hit.emission = (float3)0;
  return hit;
}

Ray CreateRayFromCamera(float2 uv) {
  // Transform the camera origin to world space.
  float3 origin = mul(_CameraToWorldSpace, float4((float3)0, 1)).xyz;
  // Invert the perspective projection of the view-space position..
  // --> transform the view-space position.
  //
  // _CameraInverseProjection : The inverse of projection from the camera space to screen space.
  // Transform the direction from camera to world space and normalize. 
  float3 direction = mul(_CameraInverseProjection, float4(uv, 0, 1)).xyz;
  direction = normalize(mul(_CameraToWorldSpace, float4(direction, 0)).xyz);
  return CreateRay(origin, direction);
}

bool IntersectTriangle_internal(Ray ray, float3x3 vtx, inout float3 tuv, int cullMode) {
  // Find vectors for two edges sharing vertex #0.
  float3 edge1 = vtx[1] - vtx[0];
  float3 edge2 = vtx[2] - vtx[0];
  // Begin calculating determinant - also used to calculate U parameter.
  float3 pvec = cross(ray.direction, edge2);
  // if determinant is near zero, ray lies in plane of the triangle.
  float dter = dot(edge1, pvec);
  // Use backface culling by determinant.
  // cullMode = 0 : Backface.
  // cullMode = 1 : Frontface.
  /*if (cullMode == 0) {
    if (dter < EPS) {
      return false;
    }
  }

  if (cullMode == 1) {
    if (dter > EPS) {
      return false;
    }
  } */

  float invDter = 1.0 / dter;
  // calculate distance from vertex #0 to ray origin.
  float3 tvec = ray.origin - vtx[0];
  // calculate U parameter and test its boundary.
  tuv[1] = dot(tvec, pvec) * invDter;
  if (tuv[1] < 0.0 || tuv[1] > 1.0) {
    // Ray is not intersecting the triangle.
    return false;
  }

  // prepare to test v parameter.
  float qvec = cross(tvec, edge1);
  // calculate v parameter and test its boundary.
  tuv[2] = dot(ray.direction, qvec) * invDter;
  if (tuv[2] < 0.0 || tuv[1] + tuv[2] > 1.0) {
    // Ray is not intersecting the triangle.
    return false;
  }

  // calculate t, ray intersects triangle.
  tuv[0] = dot(edge2, qvec) * invDter;
  return true;
}
