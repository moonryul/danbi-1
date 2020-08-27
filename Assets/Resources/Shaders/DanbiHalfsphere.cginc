//
//
//

#ifndef DANBI_HALFSPHERE
#define DANBI_HALFSPHERE

#include "DanbiGlobalRsrc.cginc"
#include "DanbiMathUtils.cginc"
#include "DanbiMesh.cginc"
#include "DanbiPanorama.cginc"

struct Shape {
  float4x4 localToWorld;
  
  float height;  
  float radius;
};

StructuredBuffer<Shape> _Shape;

RayHit Collsion(Ray ray, int bounce, Shape shape, Panorama panorama);
void IntersectHalfsphere(Ray ray, inout RayHit resHit, Shape shape);

RayHit Collsion(Ray ray, int bounce, Shape shape, Panorama panorama) {
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

void IntersectHalfsphere(Ray ray, inout RayHit resHit, Shape shape) {
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



#endif