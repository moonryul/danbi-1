
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

#ifndef DANBI_MESH
#define DANBI_MESH

#include "DanbiGlobalRsrc.cginc"
#include "DanbiMathUtils.cginc"

StructuredBuffer<float3> _Vertices;
StructuredBuffer<int> _Indices;
StructuredBuffer<float2> _Texcoords;

struct MeshAdditionalData {
  float4x4 localToWorld;
  float3 specular;
  int indicesOffset;
  int indicesCount;
};

StructuredBuffer<MeshAdditionalData> _MeshAdditionalData;

bool IntersectTriangle_MT97(Ray ray, float3 vtx0, float3 vtx1, float3 vtx2, out float t, out float u, out float v);
void IntersectMesh(Ray ray, inout RayHit bestHit, MeshAdditionalData data);

bool IntersectTriangle_MT97(Ray ray, float3 vtx0, float3 vtx1, float3 vtx2, out float t, out float u, out float v) {
  t = 1.#INF;
  // find vectors for two edges sharing vertices.
  float3 edge1 = vtx1 - vtx0;
  float3 edge2 = vtx2 - vtx0;

  // begin calculating determinant - it's also used to calculate U param.
  float3 pvec = cross(ray.direction, edge2);

  // if determinant is near zero, ray lies in plane of triangle.
  float det = dot(edge1, pvec);

  // use backface culling.
  if (abs(det) < EPS) {
    return false;
  }

  float inv_det = 1.0 / det;

  // calculate distance from vertex0 to ray.origin.
  float3 tvec = ray.origin - vtx0;

  // calculate U param and test bounds.
  u = dot(tvec, pvec) * inv_det;

  if (u < 0.0 || u > 1.0) {
    v = 1.#INF;
    return false;
  }

  float3 qvec = cross(tvec, edge1);
  
  // prepare to test V param.
  v = dot(ray.direction, qvec) * inv_det;

  if (v < 0.0 || u + v > 1.0) {
    return false;
  }
    

  // calculate t param, ray intersects on the triangle.
  t = dot(edge2, qvec) * inv_det;
  
  return true;
}

void IntersectMesh(Ray ray, inout RayHit bestHit, MeshAdditionalData data) {
  uint offset = data.indicesOffset;
  uint count = offset + data.indicesCount;

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
        bestHit.specular = data.specular;        
      }
    }
  }
}


#endif