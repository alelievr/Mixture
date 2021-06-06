#ifndef FLUID_SIMULATION
# define FLUID_SIMULATION

// template for 2D and 3D fluid simulations
#ifdef FLUID_2D
# define TEXTURE_TYPE Texture2D
# define RW_TEXTURE_TYPE RWTexture2D
# define FLOAT_X float2
# define FLOAT_X_OR_FLOAT float2
# define INT_X int2
# define SWIZZLE_X xy
# include "Packages/com.alelievr.mixture/Runtime/Shaders/FluidSimulationTemplate.hlsl"
#elif defined(FLUID_3D)
# define TEXTURE_TYPE Texture3D
# define RW_TEXTURE_TYPE RWTexture3D
# define FLOAT_X float3
# define FLOAT_X_OR_FLOAT float3
# define INT_X int3
# define SWIZZLE_X xyz
# include "Packages/com.alelievr.mixture/Runtime/Shaders/FluidSimulationTemplate.hlsl"
#else
# error Fluid dimension not defined!
#endif

// Generate functions override with float parameters
#undef FLOAT_X_OR_FLOAT
#define FLOAT_X_OR_FLOAT float
#define SECOND_PASS
# include "Packages/com.alelievr.mixture/Runtime/Shaders/FluidSimulationTemplate.hlsl"

// Utility functions (that doesn't require templates)

void ComputeNeighbourPositions(int3 id, int3 size, out int3 idxL, out int3 idxR, out int3 idxB, out int3 idxT, out int3 idxD, out int3 idxU)
{
    idxL = int3(max(0, id.x-1), id.y, id.z);
    idxR = int3(min(size.x-1, id.x+1), id.y, id.z);

    idxB = int3(id.x, max(0, id.y-1), id.z);
    idxT = int3(id.x, min(size.y-1, id.y+1), id.z);

    idxD = int3(id.x, id.y, max(0, id.z-1));
    idxU = int3(id.x, id.y, min(size.z-1, id.z+1));
}

void ComputeNeighbourPositions(int2 id, int2 size, out int2 idxL, out int2 idxR, out int2 idxB, out int2 idxT)
{
    idxL = int2(max(0, id.x-1), id.y);
    idxR = int2(min(size.x-1, id.x+1), id.y);

    idxB = int2(id.x, max(0, id.y-1));
    idxT = int2(id.x, min(size.y-1, id.y+1));
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

void Pressure(int3 id, int3 size, Texture3D<float> pressureR, Texture3D<float> obstacles, Texture3D<float> divergence, RWTexture3D<float> pressureW)
{
    float L, R, B, T, D, U;
    float3 unused;
    LoadPressureNeighbours(id, size, pressureR, obstacles, L, R, B, T, D, U, unused);

    float divergenceF = divergence[id];
    pressureW[id] = ( L + R + B + T + U + D - divergenceF ) / 6.0;
}

void Pressure(int2 id, int2 size, Texture2D<float> pressureR, Texture2D<float> obstacles, Texture2D<float> divergence, RWTexture2D<float> pressureW)
{
    float L, R, B, T;
    float2 unused;
    LoadPressureNeighbours(id, size, pressureR, obstacles, L, R, B, T, unused);

    float divergenceF = divergence[id];
    pressureW[id] = ( L + R + B + T - divergenceF ) / 4.0;
}

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

    float3 v = velocityR[id] - float3( R - L, T - B, U - D ) * 0.5;
    
    velocityW[id] = v * mask;
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

    float2 v = velocityR[id] - float2( R - L, T - B ) * 0.5;

    velocityW[id] = v * mask;
}


#endif // FLUID_SIMULATION