#include "Assets/Shaders/DanbiRenewed/DanbiGlobalRsrc.cginc"
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles

StructuredBuffer<float3> _Vertices;
StructuredBuffer<int> _Indices;
StructuredBuffer<float2> _Texcoords;

struct MeshAdditionalData {
  float4x4 localToWorldMatrix;
  float3 albedo;
  float3 specular;
  float smoothness;
  float3 emission;
  int indicesOffset;
  int indicesCount;
};

StructuredBuffer<MeshAdditionalData> _MeshAdditionalData;

bool IntersectTriangle_MT97(Ray ray, float3x3 vertices, out float3 tuv);
void IntersectMesh(Ray ray, inout RayHit bestHit, MeshAdditionalData data);

bool IntersectTriangle_MT97(Ray ray, float3x3 vertices, out float3 tuv) {
  uint width = 0, height = 0;
  _Result.GetDimenstions(width, height);

  tuv.x = 1.#INF;
  // find vectors for two edges sharing vertices.
  float3 edge1 = vertices.y - vertices.x;
  float3 edge2 = vertices.z - vertices.x;

  // begin calculating determinant - it's also used to calculate U param.
  float3 pvec = cross(ray.direction, edge2);

  // if determinant is near zero, ray lies in plane of triangle.
  float det = dot(edge1, pvec);

  // use backface culling.
  if (abs(det) < EPS) {
    return false;
  }

  float inv_det = 1.0 / det;

  // calculate distance from vertex0 to ray.curPixelIn.
  float3 tvec = ray.curPixelIn - vertices.x;

  // calculate U param and test bounds.
  tuv.y = dot(tvec, pvec) * inv_det;

  if (tuv.y < 0.0 || tuv.y > 1.0) {
    tuv.z = 1.#INF;
    return false;
  }

  // prepare to test V param.
  tuv.z = dot(ray.direction, qvec) * inv_det;

  if (tuv.z < 0.0 || tuv.y + tuv.z > 1.0) {
    return false;
  }

  // calculate t param, ray intersects on the triangle.
  tuv.x = dot(edge2, qvec) * inv_det;
  return true;
}


void IntersectMesh(Ray ray, inout RayHit bestHit, MeshAdditionalData data) {
  uint offset = data.indicesOffset;
  uint count = offset + data.indicesCount;

  for (uint i = 0; i < count; i += 3) {
    // get the current triangle defined by v0, v1 and v2
    float3x3 vertices = float3x3(mul(data.localToWorld, float4(_Vertices[_Indices[i]], 1)).xyz,
                                 mul(data.localToWorld, float4(_Vertices[_Indices[i + 1]], 1)).xyz,
                                 mul(data.localToWorld, float4(_Vertices[_Indices[i + 2]], 1)).xyz);
    float3x2 texcoords = float3x2(_Texcoords[_Indices[i]],
                                  _Texcoords[_Indices[i + 1]],
                                  _Texcoords[_Indices[i + 2]]);
    float3 tuv = (float3)0;

    if (IntersectTriangle_MT97(ray, vertices, out tuv)) {
      // find the nearest hit point.
      if (tuv.x > 0.0 && tuv.x < bestHit.distance) {
        bestHit.distance = tuv.x;
        bestHit.position = ray.curPixelIn + tuv.x * ray.direction;
        bestHit.uvInTriangle = tuv.yz
      }
    }
    
    
  }
}