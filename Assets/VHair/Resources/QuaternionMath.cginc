float4 QuatFromMat(float3x3 m)
{
	float3 u = m[0];
	float3 v = m[1];
	float3 w = m[2];

	uint u_sign = (asuint(u.x) & 0x80000000);
	float t = v.y + asfloat(asuint(w.z) ^ u_sign);
	uint4 u_mask = (int)u_sign >> 31;
	uint4 t_mask = asint(t) >> 31;

	float tr = 1.0f + abs(u.x);

	uint4 sign_flips = uint4(0x00000000, 0x80000000, 0x80000000, 0x80000000) ^ (u_mask & uint4(0x00000000, 0x80000000, 0x00000000, 0x80000000)) ^ (t_mask & uint4(0x80000000, 0x80000000, 0x80000000, 0x00000000));

	float4 value = float4(tr, u.y, w.x, v.z) + asfloat(asuint(float4(t, v.x, u.z, w.y)) ^ sign_flips);   // +---, +++-, ++-+, +-++

	value = asfloat((asuint(value) & ~u_mask) | (asuint(value.zwxy) & u_mask));
	value = asfloat((asuint(value.wzyx) & ~t_mask) | (asuint(value) & t_mask));
	value = normalize(value);

	return value;
}

float4 MulQuatQuat(float4 a, float4 b)
{
	return a.wwww * b + (a.xyzx * b.wwwx + a.yzxy * b.zxyy) * float4(1.0f, 1.0f, 1.0f, -1.0f) - a.zxyz * b.yzxz;
}

float3 MulQuatVec(float4 q, float3 vec)
{
	float3 t = 2 * cross(q.xyz, vec);
	return vec + q.w * t + cross(q.xyz, t);
}

float4 InverseQuaternion(float4 x)
{
	return rcp(dot(x, x)) * x * float4(-1.0f, -1.0f, -1.0f, 1.0f);
}

float4 LookRotation(float3 forward, float3 up)
{
	float3 t = normalize(cross(up, forward));
	return QuatFromMat(float3x3(t, cross(forward, t), forward));
}