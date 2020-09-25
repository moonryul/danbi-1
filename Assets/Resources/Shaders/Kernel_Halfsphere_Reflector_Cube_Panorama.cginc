// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

#include "DanbiIncludes.cginc"

/*
* Panorama 
*/
struct PanoramaData {
  float3 specular;
  float4x4 localToWorld;  
  int indexOffset;
  int indexCount;
  float high;
  float low;
  // float height;  
  // float radius;  
};
StructuredBuffer<PanoramaData> _PanoramaData;

/*
* Halfsphere
*/
struct HalfsphereData {
  float3 specular;
  float4x4 localToWorld;
  int indexOffset;
  int indexCount;
  float distance;
  float height;
  float radius;
};
StructuredBuffer<HalfsphereData> _HalfsphereData;

void IntersectMesh(Ray ray, inout RayHit bestHit, HalfsphereData data);
RayHit Collsion(Ray ray, int bounce, HalfsphereData shape, PanoramaData panorama);
void IntersectHalfsphere(Ray ray, inout RayHit resHit, HalfsphereData shape);
void IntersectWithPanorama(Ray ray, inout RayHit resHit, PanoramaData panorama);

void IntersectMesh(Ray ray, inout RayHit bestHit, HalfsphereData data) {
  uint offset = data.indexOffset;
  uint count = offset + data.indexCount;

  for (uint i = 0; i < count; i += 3) {
    // get the current triangle defined by v0, v1 and v2
    float3 vtx0 = mul(data.localToWorld, float4(_Vertices[_Indices[i]], 1)).xyz;
    float3 vtx1 = mul(data.localToWorld, float4(_Vertices[_Indices[i + 1]], 1)).xyz;
    float3 vtx2 = mul(data.localToWorld, float4(_Vertices[_Indices[i + 2]], 1)).xyz;
    
    float3x2 texcoords = float3x2(_Texcoords[_Indices[i]],
                                  _Texcoords[_Indices[i + 1]],
                                  _Texcoords[_Indices[i + 2]]);
    
    float t = 0;
    float u = 0;
    float v = 0;

    if (IntersectTriangle_MT97(ray, vtx0, vtx1, vtx2, t, u, v)) {
      // find the nearest hit point.
      if (t > 0.0 && t < bestHit.distance) {
        bestHit.distance = t;
        bestHit.position = ray.origin + t * ray.direction;
        bestHit.uvInTriangle = float2(u, v);
        bestHit.normal = normalize(cross(vtx1 - vtx0, vtx2 - vtx0));
        bestHit.vertexUVs = texcoords;                
        bestHit.specular = 0.1f;        
      }
    }
  }
}

RayHit Collsion(Ray ray, int bounce, HalfsphereData shape, PanoramaData panorama) {
  uint width = 0, height = 0;
  _DistortedImage.GetDimensions(width, height);;
  
  RayHit resHit = CreateRayHit();
  
  uint count = 0, stride = 0, i = 0;
  
  if (bounce == 0) {
    IntersectHalfsphere(ray, resHit, shape);    
  } else {
    IntersectWithPanorama(ray, resHit, panorama);   
  }
  return resHit;
}

void IntersectHalfsphere(Ray ray, inout RayHit resHit, HalfsphereData shape) {
  float4x4 frame = shape.localToWorld;
  float3 spherePos = float3(frame[0][3], frame[1][3], frame[2][3]);
  float3 d = ray.origin - spherePos;
  float p1 = -dot(ray.direction, d);
  float p2sqr = p1 * p1 - dot(d, d) + shape.radius * shape.radius;
  if (p2sqr < 0) {
    return;  
  }
  
  float p2 = sqrt(p2sqr);
  float t = p1 - p2 > 0 ? p1 - p2 : p1 + p2;
  
  if (t > 0 && t < resHit.distance) {
    resHit.distance = t;
    resHit.position = ray.origin + t * ray.direction;
    resHit.normal = normalize(resHit.position - spherePos);    
  }
}

void IntersectWithPanorama(Ray ray, inout RayHit resHit, PanoramaData panorama) {
  HalfsphereData data = (HalfsphereData)0;
  data.localToWorld = panorama.localToWorld;  
  data.indexOffset = panorama.indexOffset;
  data.indexCount = panorama.indexCount;
  
  IntersectMesh(ray, resHit, data);
}
