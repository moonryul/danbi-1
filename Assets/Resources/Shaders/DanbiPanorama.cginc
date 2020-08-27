#ifndef DANBI_CUBE_PANORAMA
#define DANBI_CUBE_PANORAMA

#include "DanbiGlobalRsrc.cginc"
#include "DanbiMathUtils.cginc"
#include "DanbiMesh.cginc"

struct Panorama {
  float4x4 localToWorld;
  float3 specular;
  float height;  
  float radius;
  int indicesOffset;
  int indicesCount;
};

StructuredBuffer<Panorama> _Panorama;

void IntersectWithPanorama(Ray ray, inout RayHit resHit, Panorama panorama);

void IntersectWithPanorama(Ray ray, inout RayHit resHit, Panorama panorama) {
  MeshAdditionalData data = (MeshAdditionalData)0;
  data.localToWorld = panorama.localToWorld;
  data.specular = panorama.specular;
  data.indicesOffset = panorama.indicesOffset;
  data.indicesCount = panorama.indicesCount;
  
  IntersectMesh(ray, resHit, data);
}
#endif