#ifndef FLUID_SIMULATION
# define FLUID_SIMULATION

float _BorderMode;
float4 _Size;

// TODO: replac all 0.5 by cell size

float2 GetAdvectedPosTexCoords(Texture2D<float2> velocity, float2 pos, int2 id, float deltaTime, int2 size)
{
    pos -= deltaTime * velocity[id];
    // position to UV
    float2 uv = pos / size + 0.5 / size;

    // Warp pos
    if (_BorderMode == 2)
    {
        uv %= 1;
        uv += size * -min(0, sign(uv));
    }

    return uv;
}

float3 GetAdvectedPosTexCoords(Texture3D<float3> velocity, float3 pos, int3 id, float deltaTime, int3 size)
{
    pos -= deltaTime * velocity[id];
    // position to UV
    float3 uv = pos / size + 0.5 / size;

    // Warp pos
    if (_BorderMode == 2)
    {
        uv %= 1;
        uv += size * -min(0, sign(uv));
    }

    return uv;
}

float getDeltaTimeDissipation(float dissipation, float deltaTime)
{
    float f = saturate(1.0f - dissipation);

    f *= deltaTime * 10;

    // return saturate(rcp(deltaTime * 10));
    return saturate(1.0f - f);
}

void AdvectBuffer(Texture2D<float2> source, Texture2D<float2> velocity, RWTexture2D<float2> destination, int2 id, float dissipation, float deltaTime, int2 size)
{
	float2 uv = GetAdvectedPosTexCoords(velocity, id, id, deltaTime, size);

   	destination[id] = source.SampleLevel(s_linear_clamp_sampler, uv, 0) * getDeltaTimeDissipation(dissipation, deltaTime);
}

void AdvectBuffer(Texture2D<float> source, Texture2D<float2> velocity, RWTexture2D<float> destination, int2 id, float dissipation, float deltaTime, int2 size)
{
	float2 uv = GetAdvectedPosTexCoords(velocity, id, id, deltaTime, size);

   	destination[id] = source.SampleLevel(s_linear_clamp_sampler, uv, 0) * getDeltaTimeDissipation(dissipation, deltaTime);
}

void AdvectBuffer(Texture3D<float3> source, Texture3D<float3> velocity, RWTexture3D<float3> destination, int3 id, float dissipation, float deltaTime, int3 size)
{
	float3 uv = GetAdvectedPosTexCoords(velocity, id, id, deltaTime, size);

   	destination[id] = source.SampleLevel(s_linear_clamp_sampler, uv, 0) * getDeltaTimeDissipation(dissipation, deltaTime);
}

void AdvectBuffer(Texture3D<float> source, Texture3D<float3> velocity, RWTexture3D<float> destination, int3 id, float dissipation, float deltaTime, int3 size)
{
	float3 uv = GetAdvectedPosTexCoords(velocity, id, id, deltaTime, size);

   	destination[id] = source.SampleLevel(s_linear_clamp_sampler, uv, 0) * getDeltaTimeDissipation(dissipation, deltaTime);
}

int3 CalculateBorderCoord(int3 coord)
{
    if (_BorderMode == 2)
    {
        coord = coord % uint3(_Size.xyz);
        coord += _Size.xyz * -min(0, sign(coord));
    }
    else
    {
        coord = clamp(coord, 0, int3(_Size.xyz) - 1);
    }

    return coord;
}

int2 CalculateBorderCoord(int2 coord)
{
    if (_BorderMode == 2)
    {
        coord = coord % uint2(_Size.xy);
        coord += _Size.xy * -min(0, sign(coord));
    }
    else
    {
        coord = clamp(coord, 0, int2(_Size.xy) - 1);
    }

    return coord;
}

void ComputeNeighbourPositions(int3 id, int3 size, out int3 idxL, out int3 idxR, out int3 idxB, out int3 idxT, out int3 idxD, out int3 idxU)
{
    idxL = CalculateBorderCoord(int3(id.x - 1, id.y, id.z));
    idxR = CalculateBorderCoord(int3(id.x + 1, id.y, id.z));

    idxB = CalculateBorderCoord(int3(id.x, id.y - 1, id.z));
    idxT = CalculateBorderCoord(int3(id.x, id.y + 1, id.z));

    idxD = CalculateBorderCoord(int3(id.x, id.y, id.z - 1));
    idxU = CalculateBorderCoord(int3(id.x, id.y, id.z + 1));
}

void ComputeNeighbourPositions(int2 id, int2 size, out int2 idxL, out int2 idxR, out int2 idxB, out int2 idxT)
{
    idxL = CalculateBorderCoord(int2(id.x - 1, id.y));
    idxR = CalculateBorderCoord(int2(id.x + 1, id.y));

    idxB = CalculateBorderCoord(int2(id.x, id.y - 1));
    idxT = CalculateBorderCoord(int2(id.x, id.y + 1));
}

void LoadPressureNeighbours(int3 id, int3 size, Texture3D<float> pressure, Texture3D<float> obstacles, out float L, out float R, out float B, out float T, out float D, out float U, inout float3 mask)
{
    int3 idxL, idxR, idxB, idxT, idxD, idxU;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT, idxD, idxU);
    
	L = pressure[ idxL ];
    R = pressure[ idxR ];
    
    B = pressure[ idxB ];
    T = pressure[ idxT ];
    
    D = pressure[ idxD ];
    U = pressure[ idxU ];
    
    float C = pressure[id];

    if(obstacles[idxL] > 0.1) { L = C; mask.x = 0; }
    if(obstacles[idxR] > 0.1) { R = C; mask.x = 0; }
    
    if(obstacles[idxB] > 0.1) { B = C; mask.y = 0; }
    if(obstacles[idxT] > 0.1) { T = C; mask.y = 0; }
    
    if(obstacles[idxD] > 0.1) { D = C; mask.z = 0; }
    if(obstacles[idxU] > 0.1) { U = C; mask.z = 0; }
}

void LoadPressureNeighbours(int2 id, int2 size, Texture2D<float> pressure, Texture2D<float> obstacles, out float L, out float R, out float B, out float T, inout float2 mask)
{
    int2 idxL, idxR, idxB, idxT;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT);
    
	L = pressure[ idxL ];
    R = pressure[ idxR ];
    
    B = pressure[ idxB ];
    T = pressure[ idxT ];
    
    float C = pressure[id];

    if(obstacles[idxL] > 0.1) { L = C; mask.x = 0; }
    if(obstacles[idxR] > 0.1) { R = C; mask.x = 0; }
    
    if(obstacles[idxB] > 0.1) { B = C; mask.y = 0; }
    if(obstacles[idxT] > 0.1) { T = C; mask.y = 0; }
}

void Vorticity(int3 id, int3 size, Texture3D<float3> velocity, RWTexture3D<float3> output, float deltaTime, float epsilon)
{
    int3 idxL, idxR, idxB, idxT, idxD, idxU;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT, idxD, idxU);
    
	float3 L = velocity[ idxL ];
    float3 R = velocity[ idxR ];
    
    float3 B = velocity[ idxB ];
    float3 T = velocity[ idxT ];
    
    float3 D = velocity[ idxD ];
    float3 U = velocity[ idxU ];
    
    float3 vorticity = 0.5 * float3( (( T.z - B.z ) - ( U.y - D.y )) , (( U.x - D.x ) - ( R.z - L.z )) , (( R.y - L.y ) - ( T.x - B.x )) );

    output[id] = vorticity;
}

void Vorticity(int2 id, int2 size, Texture2D<float2> velocity, RWTexture2D<float2> output, float deltaTime, float epsilon)
{
    int2 idxL, idxR, idxB, idxT;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT);
    
	float2 L = velocity[ idxL ];
    float2 R = velocity[ idxR ];
    
    float2 B = velocity[ idxB ];
    float2 T = velocity[ idxT ];

    // Not sure about this formula
    float2 vorticity = 0.5 * float2((R.y - L.y) - (T.x - B.x), (T.y - B.y) - (R.x - L.x));
			
    output[id] = vorticity;
}

void VelocityConfinment(int3 id, int3 size, Texture3D<float3> vorticity, Texture3D<float3> velocityR, RWTexture3D<float3> velocityW, float deltaTime, float epsilon)
{
    int3 idxL, idxR, idxB, idxT, idxD, idxU;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT, idxD, idxU);
    
	float omegaL = length(vorticity[ idxL ]);
    float omegaR = length(vorticity[ idxR ]);
    
    float omegaB = length(vorticity[ idxB ]);
    float omegaT = length(vorticity[ idxT ]);
    
    float omegaD = length(vorticity[ idxD ]);
    float omegaU = length(vorticity[ idxU ]);
    
    float3 omega = vorticity[id];
    
    float3 eta = 0.5 * float3( omegaR - omegaL, omegaT - omegaB, omegaU - omegaD );

    eta = normalize( eta + float3(0.001,0.001,0.001) );
    
    float3 force = deltaTime * epsilon * float3( eta.y * omega.z - eta.z * omega.y, eta.z * omega.x - eta.x * omega.z, eta.x * omega.y - eta.y * omega.x );
	
    velocityW[id] = velocityR[id] + force;
}

void VelocityConfinment(int2 id, int2 size, Texture2D<float2> vorticity, Texture2D<float2> velocityR, RWTexture2D<float2> velocityW, float deltaTime, float epsilon)
{
    int2 idxL, idxR, idxB, idxT;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT);
    
	float omegaL = length(vorticity[ idxL ]);
    float omegaR = length(vorticity[ idxR ]);
    
    float omegaB = length(vorticity[ idxB ]);
    float omegaT = length(vorticity[ idxT ]);
    
    float2 omega = vorticity[id];
    
    float2 eta = 0.5 * float2( omegaR - omegaL, omegaT - omegaB);

    eta = normalize( eta + float2(0.001,0.001) );
    
    // Not sure about this formula
    float2 force = deltaTime * epsilon * float2( eta.y * omega.x - eta.x * omega.y, eta.x * omega.y - eta.y * omega.x );
	
    velocityW[id] = velocityR[id] + force;
}

void Divergence(int3 id, int3 size, Texture3D<float3> velocity, Texture3D<float> obstacles, RWTexture3D<float> divergence)
{
    int3 idxL, idxR, idxB, idxT, idxD, idxU;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT, idxD, idxU);
    
	float3 L = velocity[ idxL ];
    float3 R = velocity[ idxR ];
    
    float3 B = velocity[ idxB ];
    float3 T = velocity[ idxT ];
    
    float3 D = velocity[ idxD ];
    float3 U = velocity[ idxU ];
    
    float3 obstacleVelocity = float3(0,0,0);
    
    if(obstacles[idxL] > 0.1) L = obstacleVelocity;
    if(obstacles[idxR] > 0.1) R = obstacleVelocity;
    
    if(obstacles[idxB] > 0.1) B = obstacleVelocity;
    if(obstacles[idxT] > 0.1) T = obstacleVelocity;
    
    if(obstacles[idxD] > 0.1) D = obstacleVelocity;
    if(obstacles[idxU] > 0.1) U = obstacleVelocity;

    float finalDivergence =  0.5 * ( ( R.x - L.x ) + ( T.y - B.y ) + ( U.z - D.z ) );
    
    divergence[id] = finalDivergence;
}

void Divergence(int2 id, int2 size, Texture2D<float2> velocity, Texture2D<float> obstacles, RWTexture2D<float> divergence)
{
    int2 idxL, idxR, idxB, idxT;
    ComputeNeighbourPositions(id, size, idxL, idxR, idxB, idxT);
    
	float2 L = velocity[ idxL ];
    float2 R = velocity[ idxR ];
    
    float2 B = velocity[ idxB ];
    float2 T = velocity[ idxT ];
    
    float2 obstacleVelocity = float2(0,0);
    
    if(obstacles[idxL] > 0.1) L = obstacleVelocity;
    if(obstacles[idxR] > 0.1) R = obstacleVelocity;
    
    if(obstacles[idxB] > 0.1) B = obstacleVelocity;
    if(obstacles[idxT] > 0.1) T = obstacleVelocity;
    
    float finalDivergence =  0.5 * ( ( R.x - L.x ) + ( T.y - B.y ) );
    
    divergence[id] = finalDivergence;
}

// Optimized pressure solver with poisson kernel: https://www.shahinrabbani.ca/torch2pd.html
#define PRESSURE_KERNEL_LENGTH 7
static float pressureKernelWeights[PRESSURE_KERNEL_LENGTH + 1] =
{
    0.642708,
    0.405773,
    0.257644,
    0.16144,
    0.097961,
    0.0576097,
    0.032294,
    0.0174269
        // -0.57843719174047891762313611252466216683387756,
        // -0.36519596949351723624843657489691395312547684,
        // -0.23187988879520393670219391424325294792652130,
        // -0.14529589353342881041797340913035441190004349,
        // -0.08816487385701118895120487195526948198676109,
        // -0.05184872885140315995622373179685382638126612,
        // -0.02906462467884733322232015950703498674556613,
        // -0.01568417195708781547436316827770497184246778,
        // -0.00797551194591553797019756189001782331615686,
        // -0.00389324328050090267758220896610055206110701,
        // -0.00177144992084785437604088631502463613287546,
        // -0.00077326180913540193146565870918607288331259,
        // -0.00031094371809950585106366305332414867734769,
        // -0.00012005971084960072336501879508574575083912,
        // -0.00004212915051050346369569676863164886526647,
        // -0.00001422423213116925993983619830318687604631,
        // -0.00000429414617778126274816753032981253568323,
        // -0.00000125119726480673493973678773311108969324,
        // -0.00000031947214210228772767906010865424093481,
        // -0.00000007904055391492848047027151760918206591,
        // -0.00000001670294209444911564530304173229607878,
        // -0.00000000343568697107984592759899747577752999,
        // -0.00000000058356643720489347685487130885993549,
        // -0.00000000009692335621185182294609988560099047,
        // -0.00000000001268654430582195306026822187324998,
        // -0.00000000000162885588824899855173069786019885,
        // -0.00000000000015374438965028492391092588224910,
        // -0.00000000000001419579825262500523490893301566,
        // -0.00000000000000085625738754467783860438162309,
        // -0.00000000000000004924476449361170701159838477,
        // -0.00000000000000000141513942892312597724531791,
        // -0.00000000000000000003280063287919295937777096
};

void Pressure(int2 id, int2 size, Texture2D<float> pressureR, Texture2D<float> obstacles, Texture2D<float> divergence, RWTexture2D<float> pressureW)
{
#if 0 // This is not working

    // int2 direction = int2(1, 0);
    float result = 0;
    float t = 0;

    for (int x = -PRESSURE_KERNEL_LENGTH; x <= PRESSURE_KERNEL_LENGTH; x++)
    {
        for (int y = -PRESSURE_KERNEL_LENGTH; y <= PRESSURE_KERNEL_LENGTH; y++)
        {
            float weight = -(pressureKernelWeights[abs(x)] * pressureKernelWeights[abs(y)]);
            t += weight;
            int2 fetchId = CalculateBorderCoord(id + int2(x, y));
            float div = pressureR[fetchId];

            result += weight * div;
        }
    }

    result /= t;

    float divergenceF = divergence[id];
    pressureW[id] = result - divergenceF;

#else // this is working

    float L, R, B, T;
    float2 unused;
    LoadPressureNeighbours(id, size, pressureR, obstacles, L, R, B, T, unused);

    float divergenceF = divergence[id];
    pressureW[id] = ( L + R + B + T - divergenceF ) / 4.0;

#endif
}

void Pressure(int3 id, int3 size, Texture3D<float> pressureR, Texture3D<float> obstacles, Texture3D<float> divergence, RWTexture3D<float> pressureW)
{
    float L, R, B, T, D, U;
    float3 unused;
    LoadPressureNeighbours(id, size, pressureR, obstacles, L, R, B, T, D, U, unused);

    float divergenceF = divergence[id];
    pressureW[id] = ( L + R + B + T + U + D - divergenceF ) / 6.0;
}

// void Pressure(int2 id, int2 size, Texture2D<float> pressureR, Texture2D<float> obstacles, Texture2D<float> divergence, RWTexture2D<float> pressureW)
// {
//     float L, R, B, T;
//     float2 unused;
//     LoadPressureNeighbours(id, size, pressureR, obstacles, L, R, B, T, unused);

//     float divergenceF = divergence[id];
//     pressureW[id] = ( L + R + B + T - divergenceF ) / 4.0;
// }

void Project(int3 id, int3 size, Texture3D<float> pressureR, Texture3D<float> obstacles, Texture3D<float3> velocityR, RWTexture3D<float3> velocityW)
{
	if(obstacles[id] > 0.1)
	{
		 velocityW[id] = float3(0,0,0);
		 return;
	}

    float L, R, B, T, D, U;
    float3 mask = float3(1,1,1);
    LoadPressureNeighbours(id, size, pressureR, obstacles, L, R, B, T, D, U, mask);

    velocityW[id] = velocityR[id] - float3( R - L, T - B, U - D ) * 0.5;
}

void Project(int2 id, int2 size, Texture2D<float> pressureR, Texture2D<float> obstacles, Texture2D<float2> velocityR, RWTexture2D<float2> velocityW)
{
	if(obstacles[id] > 0.1)
	{
		 velocityW[id] = float2(0,0);
		 return;
	}

    float L, R, B, T;
    float2 mask = float2(1,1);
    LoadPressureNeighbours(id, size, pressureR, obstacles, L, R, B, T, mask);

    velocityW[id] = velocityR[id] - float2( R - L, T - B ) * 0.5;
}


#endif // FLUID_SIMULATION