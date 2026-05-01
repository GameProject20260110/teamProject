#ifndef ORBITING_PARTICLES_INCLUDED
#define ORBITING_PARTICLES_INCLUDED

// 둥근 사각형 SDF
// p: 픽셀 위치 (-0.5 ~ 0.5)
// b: 사각형 절반 크기 (width/2, height/2)
// r: 모서리 반경
float sdRoundedBox(float2 p, float2 b, float r)
{
    float2 q = abs(p) - b + r;
    return min(max(q.x, q.y), 0.0) + length(max(q, 0.0)) - r;
}

// 둥근 사각형 둘레 위 한 점의 위치 계산
// t: 진행도 (0 ~ 1, 한 바퀴)
// halfSize: 사각형 절반 크기
// radius: 모서리 반경
float2 roundedBoxPoint(float t, float2 halfSize, float radius)
{
    // 둘레 길이 계산: 직선 4개 + 모서리 호 4개(=원 1개)
    float straightX = (halfSize.x - radius) * 2.0; // 위/아래 직선
    float straightY = (halfSize.y - radius) * 2.0; // 좌/우 직선
    float arcLength = 6.28318 * radius * 0.25; // 모서리 1개 호 길이
    float total = straightX * 2.0 + straightY * 2.0 + arcLength * 4.0;
    
    float dist = t * total; // 현재 진행 거리
    
    // 둘레를 따라가며 위치 계산
    // 시작: 오른쪽 변 중앙(시계방향)
    float seg1 = straightY * 0.5; // 오른쪽 위 절반
    float seg2 = seg1 + arcLength; // 오른쪽 위 모서리
    float seg3 = seg2 + straightX; // 위쪽 직선
    float seg4 = seg3 + arcLength; // 왼쪽 위 모서리
    float seg5 = seg4 + straightY; // 왼쪽 직선
    float seg6 = seg5 + arcLength; // 왼쪽 아래 모서리
    float seg7 = seg6 + straightX; // 아래쪽 직선
    float seg8 = seg7 + arcLength; // 오른쪽 아래 모서리
    float seg9 = seg8 + straightY * 0.5; // 오른쪽 아래 절반
    
    float2 pos;
    
    if (dist < seg1)
    {
        // 오른쪽 변 위쪽 절반
        pos = float2(halfSize.x, dist);
    }
    else if (dist < seg2)
    {
        // 오른쪽 위 모서리
        float a = (dist - seg1) / arcLength * 1.5708; // 0 ~ PI/2
        float2 corner = float2(halfSize.x - radius, halfSize.y - radius);
        pos = corner + float2(cos(a), sin(a)) * radius;
    }
    else if (dist < seg3)
    {
        // 위쪽 직선
        float a = (dist - seg2) / straightX;
        pos = float2(halfSize.x - radius - a * straightX, halfSize.y);
    }
    else if (dist < seg4)
    {
        // 왼쪽 위 모서리
        float a = (dist - seg3) / arcLength * 1.5708 + 1.5708;
        float2 corner = float2(-(halfSize.x - radius), halfSize.y - radius);
        pos = corner + float2(cos(a), sin(a)) * radius;
    }
    else if (dist < seg5)
    {
        // 왼쪽 변
        float a = (dist - seg4) / straightY;
        pos = float2(-halfSize.x, halfSize.y - radius - a * straightY);
    }
    else if (dist < seg6)
    {
        // 왼쪽 아래 모서리
        float a = (dist - seg5) / arcLength * 1.5708 + 3.14159;
        float2 corner = float2(-(halfSize.x - radius), -(halfSize.y - radius));
        pos = corner + float2(cos(a), sin(a)) * radius;
    }
    else if (dist < seg7)
    {
        // 아래쪽 직선
        float a = (dist - seg6) / straightX;
        pos = float2(-(halfSize.x - radius) + a * straightX, -halfSize.y);
    }
    else if (dist < seg8)
    {
        // 오른쪽 아래 모서리
        float a = (dist - seg7) / arcLength * 1.5708 + 4.71239;
        float2 corner = float2(halfSize.x - radius, -(halfSize.y - radius));
        pos = corner + float2(cos(a), sin(a)) * radius;
    }
    else
    {
        // 오른쪽 변 아래쪽 절반
        float a = (dist - seg8) / (straightY * 0.5);
        pos = float2(halfSize.x, -halfSize.y + radius + a * (straightY * 0.5 - radius));
    }
    
    return pos;
}

void OrbitingParticles_float(
    float2 UV,
    float ParticleCount,
    float BoxWidth,
    float BoxHeight,
    float CornerRadius,
    float ParticleSize,
    float Time,
    float TrailLength,
    out float Result)
{
    float2 center = UV - 0.5;
    float2 halfSize = float2(BoxWidth, BoxHeight) * 0.5;
    
    float total = 0;
    
    [unroll(20)]
    for (int p = 0; p < 20; p++)
    {
        if (p >= ParticleCount)
            break;
        
        // 입자별 진행도 (균등 분배 + 시간에 따라 이동)
        float t = frac((p / ParticleCount) + Time);
        
        // 둥근 사각형 둘레 위 입자 위치
        float2 particlePos = roundedBoxPoint(t, halfSize, CornerRadius);
        
        // 입자 본체 (가우시안 글로우)
        float d = length(center - particlePos);
        float particle = exp(-d * d / (ParticleSize * ParticleSize));
        
        // 트레일 (입자 뒤쪽 흔적)
        [unroll(8)]
        for (int s = 1; s <= 8; s++)
        {
            float trailT = frac(t - (s / 8.0) * TrailLength);
            float2 trailPos = roundedBoxPoint(trailT, halfSize, CornerRadius);
            float td = length(center - trailPos);
            float fade = 1.0 - (s / 8.0);
            particle += exp(-td * td / (ParticleSize * ParticleSize)) * fade * 0.4;
        }
        
        total += particle;
    }
    
    Result = saturate(total);
}

#endif