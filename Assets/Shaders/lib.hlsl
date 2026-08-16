
float hash(uint n)
{
				// integer hash copied from Hugo Elias
    n = (n << 13U) ^ n;
    n = n * (n * n * 15731U + 0x789221U) + 0x1376312589U;
    return float(n & uint(0x7fffffffU)) / float(0x7fffffff);
}

float3 mod289(float3 x)
{
    return x - floor(x / 289.0) * 289.0;
}

float4 mod289(float4 x)
{
    return x - floor(x / 289.0) * 289.0;
}

float4 permute(float4 x)
{
    return mod289((x * 34.0 + 1.0) * x);
}

float4 taylorInvSqrt(float4 r)
{
    return 1.79284291400159 - r * 0.85373472095314;
}

float snoise(float3 v)
{
    const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);

    // First corner
    float3 i = floor(v + dot(v, C.yyy));
    float3 x0 = v - i + dot(i, C.xxx);

    // Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

    // x1 = x0 - i1  + 1.0 * C.xxx;
    // x2 = x0 - i2  + 2.0 * C.xxx;
    // x3 = x0 - 1.0 + 3.0 * C.xxx;
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy;
    float3 x3 = x0 - 0.5;

    // Permutations
    i = mod289(i); // Avoid truncation effects in permutation
    float4 p =
      permute(permute(permute(i.z + float4(0.0, i1.z, i2.z, 1.0))
                            + i.y + float4(0.0, i1.y, i2.y, 1.0))
                            + i.x + float4(0.0, i1.x, i2.x, 1.0));

    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float4 j = p - 49.0 * floor(p / 49.0); // mod(p,7*7)

    float4 x_ = floor(j / 7.0);
    float4 y_ = floor(j - 7.0 * x_); // mod(j,N)

    float4 x = (x_ * 2.0 + 0.5) / 7.0 - 1.0;
    float4 y = (y_ * 2.0 + 0.5) / 7.0 - 1.0;

    float4 h = 1.0 - abs(x) - abs(y);

    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);

    //float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
    //float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
    float4 s0 = floor(b0) * 2.0 + 1.0;
    float4 s1 = floor(b1) * 2.0 + 1.0;
    float4 sh = -step(h, 0.0);

    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    float3 g0 = float3(a0.xy, h.x);
    float3 g1 = float3(a0.zw, h.y);
    float3 g2 = float3(a1.xy, h.z);
    float3 g3 = float3(a1.zw, h.w);

    // Normalise gradients
    float4 norm = taylorInvSqrt(float4(dot(g0, g0), dot(g1, g1), dot(g2, g2), dot(g3, g3)));
    g0 *= norm.x;
    g1 *= norm.y;
    g2 *= norm.z;
    g3 *= norm.w;

    // Mix final noise value
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    m = m * m;

    float4 px = float4(dot(x0, g0), dot(x1, g1), dot(x2, g2), dot(x3, g3));
    return 42.0 * dot(m, px);
}

bool IsOutsideFrustum(float4 clipPos, float margin)
{
    if (clipPos.w <= 0.0)
        return true;

    float2 ndc = clipPos.xy / clipPos.w;

    return abs(ndc.x) > 1.0 + margin ||
           abs(ndc.y) > 1.0 + margin;
}

bool IsOutOfBounds(float3 p, float3 lower, float3 higher)
{
    return p.x < lower.x || p.x > higher.x || p.y < lower.y || p.y > higher.y || p.z < lower.z || p.z >
        higher.z;
}

bool IsPointOutOfFrustum(half4 positionCS, float _Tolerance)
{
    half3 culling = positionCS.xyz;
    half w = positionCS.w;
    // UNITY_RAW_FAR_CLIP_VALUE is either 0 or 1, depending on graphics API
    // Most use 0, however OpenGL uses 1
    half3 lowerBounds = half3(-w - _Tolerance, -w - _Tolerance, -w * _ProjectionParams.w - _Tolerance);
    half3 higherBounds = half3(w + _Tolerance, w + _Tolerance, w + _Tolerance);
    return IsOutOfBounds(culling, lowerBounds, higherBounds);
}


void Vertex_float(float3 normal, float shellIndex, float shellCount, float atten, float shellLength, float curvature, float3 shellDirection, float displacementStrength, float3 pos,
    float time, float3 windDirection, float windStrength, float windFrequency, float windHeightAttenuation, float turbulenceStrength, in float3 id, in float4 hClip, inout bool isOutside, in float margin, bool isFar, out float3 displacement)
{
    isOutside = IsPointOutOfFrustum(hClip, margin);
    if (isOutside)
        return;
    float rawShellHeight = saturate(shellIndex / max(shellCount, 1.0)); //* (1 - isOutside);
    float shellHeight = pow(rawShellHeight, max(atten, 0.001));
    float shellCurve = pow(shellHeight, max(curvature, 0.001));

    displacement = pos;
    displacement += normal * shellLength * shellHeight;
    
    if(isFar)
        return;

    float xPeriod = 0.05f; // Repetition of lines in x direction
    float yPeriod = 0.1f; // Repitition of lines in y direction
    float turbSize = 2.0f;

    float xyValue = id.x * xPeriod + id.y * yPeriod + turbulenceStrength * snoise(id * turbSize);
    float sineValue = (sin((xyValue + time) * windFrequency) + 1.5f) * windStrength;

    float windHeight = pow(rawShellHeight, max(windHeightAttenuation, 0.001));

    displacement += windDirection * sineValue * windHeight;
}


void Fragment_float(in float2 uv, in float density, in float shellIndex, in float shellCount, in float noiseMin, in float noiseMax, in float thickness, in float3 lightPos, in float attenuation, in float occlusionBias, in float3 shellColor, in float3 normal, in float characterPlace, in float maxCutAmount, in float groundMask, in bool isOutside, out float4 color)
{
    if(isOutside)
        discard;
				// As explained in the video, this multiplies the uv coordinates to create more strands because it generates more seeds
    float2 newUV = uv * density;

				// In order to operate in the local space uv coordinates after expanding them to a wider range, we take the fractional component
				// since uv coordinates by default range from 0 to 1 so then the fractional part is in 0 to 1 so it just works (tm) also we multiply
				// by 2 and subtract 1 to convert from 0 to 1 to -1 to 1 in order to shift the origin of these local uvs to the center for a calculation below
    float2 localUV = frac(newUV) * 2 - 1;
				
				// This is the local distance from the local center, the pythagorean distance technically
    float localDistanceFromCenter = length(localUV);

				// This casts the above uvs to uint so it can be more easily passed into the hashing function without doing a ton of annoying casts because
				// type casting can be really annoying and really ruin your day and you will generally not notice for potentially hours sometimes
    uint2 tid = newUV;
    uint seed = tid.x + 100 * tid.y + 100 * 10;

				// This is kind of complicated, we generate a random number from our seed which returns a number from 0 -> 1, which is then used
				// as an interpolator argument between the minimum noise value and the maximum noise value, which controls how short the hair can be
				// and how long the hair can be. We could just use the hash output itself, but this gives a little bit more control over the appearance
				// and length of the hair instead of giving all the power to the rng
    float rand = lerp(noiseMin, noiseMax, hash(seed));

				// This is the normalized shell height as described above in the vertex shader
    float h = (shellIndex * groundMask / shellCount);

				// This is the condition for discarding pixels, if the distance from the local center exceeds the thickness parameter we discard it,
				// and we also modify the thickness and make it thinner as height increases based on the height of the blade occupying this space that way
				// there aren't like weird hard cutoff tapers, you can try deleting the rand or replacing it with like 1 or something to see how this changes
				// the appearance of the grass or hair
    int outsideThickness = (localDistanceFromCenter) > (thickness * (rand - h));// || characterPlace > thickness * h;
    float cutStrength = saturate(characterPlace) * maxCutAmount;
    float keepHeight = 1.0 - cutStrength;
    bool cutByInteraction = h > keepHeight && shellIndex > 0;
	if(groundMask<=0.5f)
        discard;
				// This culls the pixel if it is outside the thickness of the strand, it also ensures that the base shell is fully opaque that way there aren't
				// any real holes in the mesh, although there's certainly better ways to do that
    if (outsideThickness || cutByInteraction)
        discard;
                
				// This is the lighting output since at this point we have determined we are not discarding the pixel, so we have to color it
				// This lighting model is a modification of the Valve's half lambert as described in the video. It is not physically based, but it looks cool I think.
				// What's going on here is we take the dot product between the normal and the direction of the main Unity light source (the sun) which returns a value
				// between -1 to 1, which is then clamped to 0 to 1 by the DotClamped function provided by Unity, we then convert the 0 to 1 to 0.5 to 1 with the following
				// multiplication and addition.
    float ndotl = saturate(dot(normal, lightPos)) * 0.5f + 0.5f;

				// Valve's half lambert squares the ndotl output, which is going to bring values down, once again you can see how this looks on desmos by graphing x^2
    ndotl = ndotl * ndotl;

				// In order to fake ambient occlusion, we take the normalized shell height and take it to an attenuation exponent, which will do the same exact thing
				// I have explained that exponents will do to numbers between 0 and 1. A higher attenuation value means the occlusion of ambient light will become much stronger,
				// as the number is brought down closer to 0, and if we multiply a color with 0 then it'll be black aka in shadow.
    float ambientOcclusion = pow(h, attenuation);

				// This is a additive bias on the ambient occlusion, if you don't want the gradient to go towards black then you can add a bit to this in order to prevent
				// such a harsh gradient transition
    ambientOcclusion += occlusionBias;

				// Since the bias can push the ambient occlusion term above 1, we want to clamp it to 0 to 1 in order to prevent breaking the laws of physics by producing
				// more light than was received since if you multiply a color with a number greater than 1, it'll become brighter, and that just physically does not make
				// sense in this context
    ambientOcclusion = saturate(ambientOcclusion);

				// We put it all together down here by multiplying the color with Valve's half lambert and our fake ambient occlusion. You can remove some of these terms
				// to see how it changes the lighting and shadowing.
    color = float4(shellColor * ndotl * ambientOcclusion, 1.0);
}

