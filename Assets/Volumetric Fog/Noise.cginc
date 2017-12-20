#ifndef NOISE_INCLUDED
#define NOISE_INCLUDED

#include "UnityCG.cginc"

float4 random4(float4 c) {
	float j = 4096.0*sin(dot(c,float4(17.0, 59.4, 15.0, 24.8)));
	float4 r;
	r.w = frac(256.0*j);
	j *= .0625;
	r.z = frac(256.0*j);
	j *= .0625;
	r.x = frac(256.0*j);
	j *= .0625;
	r.y = frac(256.0*j);
	return r-0.5;
}

float4 skew(float4 p) {
	float skew = (sqrt(5.0) - 1.0) / 4.0;
	return p + dot(p, float4(skew, skew, skew, skew));
}

float4 unskew(float4 p) {
	float unskew = (5.0 - sqrt(5.0)) / 20.0;
	return p - dot(p, float4(unskew, unskew, unskew, unskew));
}

float4 simplex(float4 pos) {
	float4 skewedP0 = floor(skew(pos));
	float4 v0 = pos - unskew(skewedP0);
	
	float4 greaterBool0 = step(0.0, v0 - v0.yzwx);
	float4 greaterBool1 = step(0.0, v0 - v0.zwxy);
	float4 greaterCount = greaterBool0 + (1.0 - greaterBool0.wxyz) + greaterBool1;
	
	float4 i1 = step(3.0, greaterCount);
	float4 i2 = step(2.0, greaterCount);
	float4 i3 = step(1.0, greaterCount);
	
	float4 v1 = v0 - unskew(i1);
	float4 v2 = v0 - unskew(i2);
	float4 v3 = v0 - unskew(i3);
	float4 v4 = v0 - unskew(1.0);
	
	float4 weight = max(0.6 - float4(dot(v0, v0), dot(v1, v1), dot(v2, v2), dot(v3, v3)), 0.0);
	float weight4 = max(0.6 - dot(v4, v4), 0.0);
	
	weight *= weight;
	weight4 *= weight4;
	float4 contribution = weight * weight * float4(
		dot(random4(skewedP0), v0),
		dot(random4(skewedP0 + i1), v1),
		dot(random4(skewedP0 + i2), v2),
		dot(random4(skewedP0 + i3), v3)
	);
	float contribution4 = weight4 * weight4 * dot(random4(skewedP0 + 1.0), v4);
	
	return dot(27.0, contribution) + 27.0 * contribution4;
}
/*
greaterBool0
	x = x > y
	y = y > z
	z = z > w
	w = w > x

greaterBool1
	x = x > z
	y = y > w
	z = z > x
	w = w > y

greaterCount
	x = x > y + !(w > x) + x > z
	y = y > z + !(x > y) + y > w
	z = z > w + !(y > z) + z > x
	w = w > x + !(z > w) + w > y
	
*/

float simplexFractal(float3 pos, float time) {
	float4 posAndTime = float4(pos.x, pos.y, pos.z, time);
	
    return    1.0    * simplex(posAndTime)
			+ 0.5    * simplex(2.0 * posAndTime)
			+ 0.25   * simplex(4.0 * posAndTime)
			+ 0.125  * simplex(8.0 * posAndTime)
			+ 0.0625 * simplex(16.0 * posAndTime);
}

#endif