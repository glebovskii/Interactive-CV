// #define PI 3.14159265358979323846f

void Sobel_float(in float4 uv, in float edgeStrength, in float threshold, in float thickness, in float edgePower, in float softness, in float4 color, in float minEdge, in float depth, in float offsetRed, in float offsetGreen, in float offsetBlue, in float frameComparison, in float aberrationMinDepth, out float3 Out)
{
    uv = saturate(uv);
    float edge=0;
    #ifdef _USE_ABERRATION
    float4 prev = SAMPLE_TEXTURE2D(_PrevFrameTex, sampler_CameraOpaqueTexture, uv);
    #endif
    float4 c = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
    #ifdef _USE_EDGE
    float2 texel = ((thickness) / _ScreenParams.xy);
    float2 uv_tl = uv.xy + texel * float2( -1, 1);
    float2 uv_l = uv.xy + texel * float2( -1, 0);
    float2 uv_bl = uv.xy + texel * float2( -1, -1);
    float2 uv_t = uv.xy + texel * float2(0, 1);
    float2 uv_b = uv.xy + texel * float2(0, -1);
    float2 uv_tr = uv.xy + texel * float2(1, 1);
    float2 uv_r = uv.xy + texel * float2(1, 0);
    float2 uv_br = uv.xy + texel * float2(1, -1);

    float4 tl = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_tl);
    float4 l = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_l);
    float4 bl = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_bl);
    float4 t = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_t);

    
    float4 b = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_b);
    float4 tr = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_tr);
    float4 r = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_r);
    float4 br = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv_br);


    float tl_g = dot(tl.rgb, float3(0.299, 0.587, 0.114));
    float t_g = dot(t.rgb, float3(0.299, 0.587, 0.114));
    float tr_g = dot(tr.rgb, float3(0.299, 0.587, 0.114));

    float l_g = dot(l.rgb, float3(0.299, 0.587, 0.114));
    float c_g = dot(c.rgb, float3(0.299, 0.587, 0.114));
    float r_g = dot(r.rgb, float3(0.299, 0.587, 0.114));

    float bl_g = dot(bl.rgb, float3(0.299, 0.587, 0.114));
    float b_g = dot(b.rgb, float3(0.299, 0.587, 0.114));
    float br_g = dot(br.rgb, float3(0.299, 0.587, 0.114));

    float gx = -tl_g - 2.0 * l_g - bl_g + tr_g + 2.0 * r_g + br_g;
    float gy = -tl_g - 2.0 * t_g - tr_g + bl_g + 2.0 * b_g + br_g;

    // float edge = sqrt(gx * gx + gy * gy);
    edge = abs(gx) + abs(gy);
    edge *= edgeStrength;
    edge = smoothstep(threshold - softness, threshold + softness, edge);
    edge = pow(edge, edgePower)*minEdge;
    #endif
    float3 cMain = c.rgb;
    #ifdef _USE_ABERRATION
float3 finalColor = abs(c.rgb - prev.rgb);
finalColor = pow(dot(finalColor.rgb, float3(0.299, 0.587, 0.114)), 1);
    if (length(finalColor) < frameComparison)
    {
        float4 c_red = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + offsetRed *finalColor.x *aberrationMinDepth);
        float4 c_green = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + offsetGreen * finalColor.y *aberrationMinDepth);
        float4 c_blue = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + offsetBlue * finalColor.z *aberrationMinDepth);
        c = float4(c_red.r * c_red.a, c_green.g * c_green.a, c_blue.b * c_blue.a, ((c_red.a + c_green.a + c_blue.a) / 3));
    }
 c.rgb = max(cMain.rgb, c.rgb);
 #endif
    Out = lerp(c.rgb, color.rgb, edge);
}

void Kuwahara_float(in float2 uv, in float4 TFM,
                    in float kernelSize, in float n,
                    in float hardness, in float q, in float alpha, in float zeroCrossing, in float zeta, out float4 Out)
{
    float2 texel = (1 / _ScreenParams.xy);
    float4 t = TFM;

    int kernelRadius = kernelSize / 2;
    float a = float((kernelRadius)) * clamp((alpha + t.w) / alpha, 0.1f, 2.0f);
    float b = float((kernelRadius)) * clamp(alpha / (alpha + t.w), 0.1f, 2.0f);

    float cos_phi = cos(t.z);
    float sin_phi = sin(t.z);

    float2x2 R = {
        cos_phi, -sin_phi,
        sin_phi, cos_phi
    };

    float2x2 S = {
        0.5f / a, 0.0f,
        0.0f, 0.5f / b
    };

    float2x2 SR = mul(S, R);

    int max_x = int(sqrt(a * a * cos_phi * cos_phi + b * b * sin_phi * sin_phi));
    int max_y = int(sqrt(a * a * sin_phi * sin_phi + b * b * cos_phi * cos_phi));

    float zeroCross = zeroCrossing;
    float sinZeroCross = sin(zeroCross);
    float eta = (zeta + cos(zeroCross)) / (sinZeroCross * sinZeroCross);
    int k;
    float4 m[8];
    float3 s[8];

    for (k = 0; k < n; ++k)
    {
        m[k] = 0.0f;
        s[k] = 0.0f;
    }

    [loop]
    for (int y = -max_y; y <= max_y; ++y)
    {
        [loop]
        for (int x = -max_x; x <= max_x; ++x)
        {
            float2 v = mul(SR, float2(x, y));
            if (dot(v, v) <= 0.25f)
            {
                float3 c = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(x, y) * texel.xy).rgb;
                c = saturate(c);
                float sum = 0;
                float w[8];
                float z, vxx, vyy;

                /* Calculate Polynomial Weights */
                vxx = zeta - eta * v.x * v.x;
                vyy = zeta - eta * v.y * v.y;
                z = max(0, v.y + vxx);
                w[0] = z * z;
                sum += w[0];
                z = max(0, -v.x + vyy);
                w[2] = z * z;
                sum += w[2];
                z = max(0, -v.y + vxx);
                w[4] = z * z;
                sum += w[4];
                z = max(0, v.x + vyy);
                w[6] = z * z;
                sum += w[6];
                v = sqrt(2.0f) / 2.0f * float2(v.x - v.y, v.x + v.y);
                vxx = zeta - eta * v.x * v.x;
                vyy = zeta - eta * v.y * v.y;
                z = max(0, v.y + vxx);
                w[1] = z * z;
                sum += w[1];
                z = max(0, -v.x + vyy);
                w[3] = z * z;
                sum += w[3];
                z = max(0, -v.y + vxx);
                w[5] = z * z;
                sum += w[5];
                z = max(0, v.x + vyy);
                w[7] = z * z;
                sum += w[7];

                float g = exp(-3.125f * dot(v, v)) / sum;

                for (int k = 0; k < 8; ++k)
                {
                    float wk = w[k] * g;
                    m[k] += float4(c * wk, wk);
                    s[k] += c * c * wk;
                }
            }
        }
    }

    float4 output = 0;
    for (k = 0; k < n; ++k)
    {
        m[k].rgb /= m[k].w;
        s[k] = abs(s[k] / m[k].w - m[k].rgb * m[k].rgb);

        float sigma2 = s[k].r + s[k].g + s[k].b;
        float w = 1.0f / (1.0f + pow(abs(hardness * 1000.0f * sigma2), 0.5f * q));

        output += float4(m[k].rgb * w, w);
    }

    Out = saturate(output / output.w);
}

void CalculateEigenvectors_float(in float2 uv, out float4 Out)
{
    float2 d = 1/_ScreenParams.xy;

    float3 Sx = (
        1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(-d.x, -d.y)).rgb +
        2.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(-d.x, 0.0)).rgb +
        1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(-d.x, d.y)).rgb +
        -1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(d.x, -d.y)).rgb +
        -2.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(d.x, 0.0)).rgb +
        -1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(d.x, d.y)).rgb
    ) / 4.0f;

    float3 Sy = (
        1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(-d.x, -d.y)).rgb +
        2.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(0.0, -d.y)).rgb +
        1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(d.x, -d.y)).rgb +
        -1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(-d.x, d.y)).rgb +
        -2.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(0.0, d.y)).rgb +
        -1.0f * SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(d.x, d.y)).rgb
    ) / 4.0f;

    Out = float4(dot(Sx, Sx), dot(Sy, Sy), dot(Sx, Sy), 1.0f);
}

float Gaussian(float sigma, float pos)
{
    return (1.0f / sqrt(2.0f * PI * sigma * sigma)) * exp(-(pos * pos) / (2.0f * sigma * sigma));
}

void Blur_1_float( in float2 uv, out float4 blur)
{
    int kernelRadius = 5;

    float4 col = 0;
    float kernelSum = 0.0f;

    for (int x = -kernelRadius; x <= kernelRadius; ++x)
    {
        float4 c = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(x, 0) * 1/_ScreenParams.xy);
        float gauss = Gaussian(2.0f, x);

        col += c * gauss;
        kernelSum += gauss;
    }

    blur = col / kernelSum;
}

void Blur_2_float( in float2 uv, out float4 blur)
{
    int kernelRadius = 5;

    float4 col = 0;
    float kernelSum = 0.0f;

    for (int y = -kernelRadius; y <= kernelRadius; ++y) {
        float4 c = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv + float2(0, y) * 1/_ScreenParams.xy);
        float gauss = Gaussian(2.0f, y);

        col += c * gauss;
        kernelSum += gauss;
    }

    float3 g = col.rgb / kernelSum;

    float lambda1 = 0.5f * (g.y + g.x + sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));
    float lambda2 = 0.5f * (g.y + g.x - sqrt(g.y * g.y - 2.0f * g.x * g.y + g.x * g.x + 4.0f * g.z * g.z));

    float2 v = float2(lambda1 - g.x, -g.z);
    float2 t = length(v) > 0.0 ? normalize(v) : float2(0.0f, 1.0f);
    float phi = -atan2(t.y, t.x);

    float A = (lambda1 + lambda2 > 0.0f) ? (lambda1 - lambda2) / (lambda1 + lambda2) : 0.0f;

    blur = float4(t,phi, A);
}

float3 mean[4] = {
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0}
                };
 
                float3 sigma[4] = {
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0},
                    {0, 0, 0}
                };
void Oil_float(in float2 uv, in float Radius, in float thickness, in float minDepth, out float4 Out)
{
    Out= float4(0,0,0,1);   

    if(minDepth <=0)
    discard;
                float2 texel = thickness/_ScreenParams.xy;
                float2 start[4] = {{-Radius, -Radius}, {-Radius, 0}, {0, -Radius}, {0, 0}};
                float2 pos;
                float3 col;

                for (int k = 0; k < 4; k++) {
                    for(int i = 0; i <= Radius; i++) {
                        for(int j = 0; j <= Radius; j++) {
                            pos = float2(i, j) + start[k];
                            col = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, float4(uv + float2(pos.x * texel.x, pos.y * texel.y), 0., 0.)).rgb;
                            mean[k] += col;
                            sigma[k] += col * col;
                        }
                    }
                }
 
                float sigma2;
 
                float n = pow(Radius + 1, 2);//*minDepth;
                float4 color = SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uv);
                float min = 1;
 
                for (int l = 0; l < 4; l++) {
                    mean[l] /= n;
                    sigma[l] = abs(sigma[l] / n - mean[l] * mean[l]);
                    sigma2 = sigma[l].r + sigma[l].g + sigma[l].b;
 
                    if (sigma2 < min) {
                        min = sigma2;
                        color.rgb = mean[l].rgb;
                    }
                }
                Out = color;
}