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

/*
*  Random utilities.
*/

float _Seed;

float rand(float2 input) {
  float res = frac(sin(_Seed / 100.0f * dot(input, float2(12.9898f, 78.233f))) * 43758.5453f);
  ++_Seed;
  return result;
}
