// This compute shader is responsible for 
// Math Utilities.

static const float PI = 3.14159265f;
static const float EPS = 1e-8;

float sdot(float3 x, float3 y, float f = 1.0f) {
  return saturate(dot(x, y) * f);
}

float energy(float3 color) {
  return dot(color, 1.0f / 3.0f);
}

float3x3 getTangentSpace(float3 normal) {
  // Choose a helper vector for the cross-product.
  float3 helper = float3(1, 0, 0);
  if (abs(normal.x) > 0.99f) {
    helper = float3(0, 0, 1);
  }

  float3 tangent = normalize(cross(normal, helper));
  float3 binormal = normalize(cross(normal, tangent));
  return float3x3(tangent, binormal, normal);
}

float3 sampleHemisphere(float3 normal, float alpha, float2 pixel) {
  // Sample the hemisphere, where alpha determines the kind of the sampling.
  float costTheta = pow(rand(pixel), 1.0f / (alpha + 1.0f));
  float sinTheta = sqrt(1.0f - (cosTheta * costTheta));
  float phi = 2 * PI * rand(pixel);
  float3 tangentSpaceDir = float3(cos(phi) * sinTheta, sin(phi) * sinTheta, costTheta);

  // Transform direction to world space.
  return mul(targentSpaceDir, GetTangentSpace(normal));
}

float SmoothnessToPhongAlpha(float s) {
  return pow(1000.0f, s * s);
}

/*
*  Random utilities.
*/

float _Seed;

float rand(float2 input) {
  float res = frac(sin(_Seed / 100.0f * dot(input, float2(12.9898f, 78.233f))) * 43758.5453f);
  ++_Seed;
  return result;
}
