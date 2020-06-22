using UnityEngine;
using System.Collections;

public static class CSGPrimitives {

    public static string MakePrimitiveName(int hashid)
    {
        string newname = "Primitive_" + hashid;
        return newname;
    }

    public static GameObject MakeGameObject(int hashid)
    {
        string newname = MakePrimitiveName(hashid);
        GameObject go = GameObject.Find(newname);
        if (go != null) GameObject.DestroyImmediate(go);           
        go = new GameObject(newname);

        // Need a mesh renderer and a default material
        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr == null) mr = go.AddComponent<MeshRenderer>();
        mr.material = new Material(Shader.Find("Diffuse"));
        return go;
    }

    // Use this for initialization
    public static GameObject CreatePlane(int hashid)
    {
        GameObject go = MakeGameObject(hashid);
        // You can change that line to provide another MeshFilter
        MeshFilter filter = go.AddComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.Clear();

        float length = 1f;
        float width = 1f;
        int resX = 2; // 2 minimum
        int resZ = 2;

        #region Vertices
        Vector3[] vertices = new Vector3[resX * resZ];
        for (int z = 0; z < resZ; z++)
        {
            // [ -length / 2, length / 2 ]
            float zPos = ((float)z / (resZ - 1) - .5f) * length;
            for (int x = 0; x < resX; x++)
            {
                // [ -width / 2, width / 2 ]
                float xPos = ((float)x / (resX - 1) - .5f) * width;
                vertices[x + z * resX] = new Vector3(xPos, 0f, zPos);
            }
        }
        #endregion

        #region Normales
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < normales.Length; n++)
            normales[n] = Vector3.up;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int v = 0; v < resZ; v++)
        {
            for (int u = 0; u < resX; u++)
            {
                uvs[u + v * resX] = new Vector2((float)u / (resX - 1), (float)v / (resZ - 1));
            }
        }
        #endregion

        #region Triangles
        int nbFaces = (resX - 1) * (resZ - 1);
        int[] triangles = new int[nbFaces * 6];
        int t = 0;
        for (int face = 0; face < nbFaces; face++)
        {
            // Retrieve lower left corner from face ind
            int i = face % (resX - 1) + (face / (resZ - 1) * resX);

            triangles[t++] = i + resX;
            triangles[t++] = i + 1;
            triangles[t++] = i;

            triangles[t++] = i + resX;
            triangles[t++] = i + resX + 1;
            triangles[t++] = i + 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        return go;
    }

	// Use this for initialization
	public static GameObject CreateCone(int hashid)
	{
		GameObject go = MakeGameObject(hashid);
        MeshFilter filter = go.GetComponent<MeshFilter>();
        if(filter == null) filter = go.AddComponent<MeshFilter>();

        Mesh mesh = new Mesh();
		mesh.Clear();
		 
		float height = 1f;
		float bottomRadius = .25f;
		float topRadius = .05f;
		int nbSides = 18;
		int nbHeightSeg = 1; // Not implemented yet
		 
		int nbVerticesCap = nbSides + 1;
		#region Vertices
		 
		// bottom + top + sides
		Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * nbHeightSeg * 2 + 2];
		int vert = 0;
		float _2pi = Mathf.PI * 2f;
		 
		// Bottom cap
		vertices[vert++] = new Vector3(0f, 0f, 0f);
		while( vert <= nbSides )
		{
			float rad = (float)vert / nbSides * _2pi;
			vertices[vert] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius);
			vert++;
		}
		 
		// Top cap
		vertices[vert++] = new Vector3(0f, height, 0f);
		while (vert <= nbSides * 2 + 1)
		{
			float rad = (float)(vert - nbSides - 1)  / nbSides * _2pi;
			vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
			vert++;
		}
		 
		// Sides
		int v = 0;
		while (vert <= vertices.Length - 4 )
		{
			float rad = (float)v / nbSides * _2pi;
			vertices[vert] = new Vector3(Mathf.Cos(rad) * topRadius, height, Mathf.Sin(rad) * topRadius);
			vertices[vert + 1] = new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius);
			vert+=2;
			v++;
		}
		vertices[vert] = vertices[ nbSides * 2 + 2 ];
		vertices[vert + 1] = vertices[nbSides * 2 + 3 ];
		#endregion
		 
		#region Normales
		 
		// bottom + top + sides
		Vector3[] normales = new Vector3[vertices.Length];
		vert = 0;
		 
		// Bottom cap
		while( vert  <= nbSides )
		{
			normales[vert++] = Vector3.down;
		}
		 
		// Top cap
		while( vert <= nbSides * 2 + 1 )
		{
			normales[vert++] = Vector3.up;
		}
		 
		// Sides
		v = 0;
		while (vert <= vertices.Length - 4 )
		{			
			float rad = (float)v / nbSides * _2pi;
			float cos = Mathf.Cos(rad);
			float sin = Mathf.Sin(rad);
		 
			normales[vert] = new Vector3(cos, 0f, sin);
			normales[vert+1] = normales[vert];
		 
			vert+=2;
			v++;
		}
		normales[vert] = normales[ nbSides * 2 + 2 ];
		normales[vert + 1] = normales[nbSides * 2 + 3 ];
		#endregion
		 
		#region UVs
		Vector2[] uvs = new Vector2[vertices.Length];
		 
		// Bottom cap
		int u = 0;
		uvs[u++] = new Vector2(0.5f, 0.5f);
		while (u <= nbSides)
		{
			float rad = (float)u / nbSides * _2pi;
			uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
			u++;
		}
		 
		// Top cap
		uvs[u++] = new Vector2(0.5f, 0.5f);
		while (u <= nbSides * 2 + 1)
		{
			float rad = (float)u / nbSides * _2pi;
			uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
			u++;
		}
		 
		// Sides
		int u_sides = 0;
		while (u <= uvs.Length - 4 )
		{
			float t = (float)u_sides / nbSides;
			uvs[u] = new Vector3(t, 1f);
			uvs[u + 1] = new Vector3(t, 0f);
			u += 2;
			u_sides++;
		}
		uvs[u] = new Vector2(1f, 1f);
		uvs[u + 1] = new Vector2(1f, 0f);
		#endregion 
		 
		#region Triangles
		int nbTriangles = nbSides + nbSides + nbSides*2;
		int[] triangles = new int[nbTriangles * 3 + 3];
		 
		// Bottom cap
		int tri = 0;
		int i = 0;
		while (tri < nbSides - 1)
		{
			triangles[ i ] = 0;
			triangles[ i+1 ] = tri + 1;
			triangles[ i+2 ] = tri + 2;
			tri++;
			i += 3;
		}
		triangles[i] = 0;
		triangles[i + 1] = tri + 1;
		triangles[i + 2] = 1;
		tri++;
		i += 3;
		 
		// Top cap
		//tri++;
		while (tri < nbSides*2)
		{
			triangles[ i ] = tri + 2;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = nbVerticesCap;
			tri++;
			i += 3;
		}
		 
		triangles[i] = nbVerticesCap + 1;
		triangles[i + 1] = tri + 1;
		triangles[i + 2] = nbVerticesCap;		
		tri++;
		i += 3;
		tri++;
		 
		// Sides
		while( tri <= nbTriangles )
		{
			triangles[ i ] = tri + 2;
			triangles[ i+1 ] = tri + 1;
			triangles[ i+2 ] = tri + 0;
			tri++;
			i += 3;
		 
			triangles[ i ] = tri + 1;
			triangles[ i+1 ] = tri + 2;
			triangles[ i+2 ] = tri + 0;
			tri++;
			i += 3;
		}
		#endregion
		 
		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;
		 
		mesh.RecalculateBounds();
		mesh.Optimize();
        filter.sharedMesh = mesh;
		
		return go;
	}

    // Use this for initialization
    public static GameObject CreateCube(int hashid)
    {
        GameObject go = MakeGameObject(hashid);

        // You can change that line to provide another MeshFilter
        MeshFilter filter = go.AddComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.Clear();

        float length = 1f;
        float width = 1f;
        float height = 1f;

        #region Vertices
        Vector3 p0 = new Vector3(-length * .5f, -width * .5f, height * .5f);
        Vector3 p1 = new Vector3(length * .5f, -width * .5f, height * .5f);
        Vector3 p2 = new Vector3(length * .5f, -width * .5f, -height * .5f);
        Vector3 p3 = new Vector3(-length * .5f, -width * .5f, -height * .5f);

        Vector3 p4 = new Vector3(-length * .5f, width * .5f, height * .5f);
        Vector3 p5 = new Vector3(length * .5f, width * .5f, height * .5f);
        Vector3 p6 = new Vector3(length * .5f, width * .5f, -height * .5f);
        Vector3 p7 = new Vector3(-length * .5f, width * .5f, -height * .5f);

        Vector3[] vertices = new Vector3[]
		{
			// Bottom
			p0, p1, p2, p3,
		 
			// Left
			p7, p4, p0, p3,
		 
			// Front
			p4, p5, p1, p0,
		 
			// Back
			p6, p7, p3, p2,
		 
			// Right
			p5, p6, p2, p1,
		 
			// Top
			p7, p6, p5, p4
		};
        #endregion

        #region Normales
        Vector3 up = Vector3.up;
        Vector3 down = Vector3.down;
        Vector3 front = Vector3.forward;
        Vector3 back = Vector3.back;
        Vector3 left = Vector3.left;
        Vector3 right = Vector3.right;

        Vector3[] normales = new Vector3[]
		{
			// Bottom
			down, down, down, down,
		 
			// Left
			left, left, left, left,
		 
			// Front
			front, front, front, front,
		 
			// Back
			back, back, back, back,
		 
			// Right
			right, right, right, right,
		 
			// Top
			up, up, up, up
		};
        #endregion

        #region UVs
        Vector2 _00 = new Vector2(0f, 0f);
        Vector2 _10 = new Vector2(1f, 0f);
        Vector2 _01 = new Vector2(0f, 1f);
        Vector2 _11 = new Vector2(1f, 1f);

        Vector2[] uvs = new Vector2[]
		{
			// Bottom
			_11, _01, _00, _10,
		 
			// Left
			_11, _01, _00, _10,
		 
			// Front
			_11, _01, _00, _10,
		 
			// Back
			_11, _01, _00, _10,
		 
			// Right
			_11, _01, _00, _10,
		 
			// Top
			_11, _01, _00, _10,
		};
        #endregion

        #region Triangles
        int[] triangles = new int[]
		{
			// Bottom
			3, 1, 0,
			3, 2, 1,			
		 
			// Left
			3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
			3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
		 
			// Front
			3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
			3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
		 
			// Back
			3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
			3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
		 
			// Right
			3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
			3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
		 
			// Top
			3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
			3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
		 
		};
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        return go;
    }

    // Use this for initialization
    public static GameObject CreateSphere(int hashid)
    {
        GameObject go = MakeGameObject(hashid);

        MeshFilter filter = go.AddComponent<MeshFilter>();
        Mesh mesh = filter.mesh;
        mesh.Clear();

        float radius = 1f;
        // Longitude |||
        int nbLong = 24;
        // Latitude ---
        int nbLat = 16;

        #region Vertices
        Vector3[] vertices = new Vector3[(nbLong + 1) * nbLat + 2];
        float _pi = Mathf.PI;
        float _2pi = _pi * 2f;

        vertices[0] = Vector3.up * radius;
        for (int lat = 0; lat < nbLat; lat++)
        {
            float a1 = _pi * (float)(lat + 1) / (nbLat + 1);
            float sin1 = Mathf.Sin(a1);
            float cos1 = Mathf.Cos(a1);

            for (int lon = 0; lon <= nbLong; lon++)
            {
                float a2 = _2pi * (float)(lon == nbLong ? 0 : lon) / nbLong;
                float sin2 = Mathf.Sin(a2);
                float cos2 = Mathf.Cos(a2);

                vertices[lon + lat * (nbLong + 1) + 1] = new Vector3(sin1 * cos2, cos1, sin1 * sin2) * radius;
            }
        }
        vertices[vertices.Length - 1] = Vector3.up * -radius;
        #endregion

        #region Normales
        Vector3[] normales = new Vector3[vertices.Length];
        for (int n = 0; n < vertices.Length; n++)
            normales[n] = vertices[n].normalized;
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        uvs[0] = Vector2.up;
        uvs[uvs.Length - 1] = Vector2.zero;
        for (int lat = 0; lat < nbLat; lat++)
            for (int lon = 0; lon <= nbLong; lon++)
                uvs[lon + lat * (nbLong + 1) + 1] = new Vector2((float)lon / nbLong, 1f - (float)(lat + 1) / (nbLat + 1));
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        //Top Cap
        int i = 0;
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = lon + 2;
            triangles[i++] = lon + 1;
            triangles[i++] = 0;
        }

        //Middle
        for (int lat = 0; lat < nbLat - 1; lat++)
        {
            for (int lon = 0; lon < nbLong; lon++)
            {
                int current = lon + lat * (nbLong + 1) + 1;
                int next = current + nbLong + 1;

                triangles[i++] = current;
                triangles[i++] = current + 1;
                triangles[i++] = next + 1;

                triangles[i++] = current;
                triangles[i++] = next + 1;
                triangles[i++] = next;
            }
        }

        //Bottom Cap
        for (int lon = 0; lon < nbLong; lon++)
        {
            triangles[i++] = vertices.Length - 1;
            triangles[i++] = vertices.Length - (lon + 2) - 1;
            triangles[i++] = vertices.Length - (lon + 1) - 1;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        return go;
    }

    // Use this for initialization
    public static GameObject CreateTorus(int hashid)
    {
        GameObject go = MakeGameObject(hashid);

        MeshFilter filter = go.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.Clear();

        float radius1 = 1f;
        float radius2 = .3f;
        int nbRadSeg = 24;
        int nbSides = 18;

        #region Vertices
        Vector3[] vertices = new Vector3[(nbRadSeg + 1) * (nbSides + 1)];
        float _2pi = Mathf.PI * 2f;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                int currSide = side == nbSides ? 0 : side;

                Vector3 normale = Vector3.Cross(r1, Vector3.up);
                float t2 = (float)currSide / nbSides * _2pi;
                Vector3 r2 = Quaternion.AngleAxis(-t1 * Mathf.Rad2Deg, Vector3.up) * new Vector3(Mathf.Sin(t2) * radius2, Mathf.Cos(t2) * radius2);

                vertices[side + seg * (nbSides + 1)] = r1 + r2;
            }
        }
        #endregion

        #region Normales
        Vector3[] normales = new Vector3[vertices.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            int currSeg = seg == nbRadSeg ? 0 : seg;

            float t1 = (float)currSeg / nbRadSeg * _2pi;
            Vector3 r1 = new Vector3(Mathf.Cos(t1) * radius1, 0f, Mathf.Sin(t1) * radius1);

            for (int side = 0; side <= nbSides; side++)
            {
                normales[side + seg * (nbSides + 1)] = (vertices[side + seg * (nbSides + 1)] - r1).normalized;
            }
        }
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];
        for (int seg = 0; seg <= nbRadSeg; seg++)
            for (int side = 0; side <= nbSides; side++)
                uvs[side + seg * (nbSides + 1)] = new Vector2((float)seg / nbRadSeg, (float)side / nbSides);
        #endregion

        #region Triangles
        int nbFaces = vertices.Length;
        int nbTriangles = nbFaces * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        int i = 0;
        for (int seg = 0; seg <= nbRadSeg; seg++)
        {
            for (int side = 0; side <= nbSides - 1; side++)
            {
                int current = side + seg * (nbSides + 1);
                int next = side + (seg < (nbRadSeg) ? (seg + 1) * (nbSides + 1) : 0);

                if (i < triangles.Length - 6)
                {
                    triangles[i++] = current;
                    triangles[i++] = next;
                    triangles[i++] = next + 1;

                    triangles[i++] = current;
                    triangles[i++] = next + 1;
                    triangles[i++] = current + 1;
                }
            }
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        filter.sharedMesh = mesh;

        return go;
    }

    // Use this for initialization
    public static GameObject CreateTube(int hashid)
    {
        GameObject go = MakeGameObject(hashid);

        MeshFilter filter = go.AddComponent<MeshFilter>();
        Mesh mesh = new Mesh();
        mesh.Clear();

        float height = 1f;
        int nbSides = 24;

        // Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2
        float bottomRadius1 = .5f;
        float bottomRadius2 = .15f;
        float topRadius1 = .5f;
        float topRadius2 = .15f;

        int nbVerticesCap = nbSides * 2 + 2;
        int nbVerticesSides = nbSides * 2 + 2;
        #region Vertices

        // bottom + top + sides
        Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];
        int vert = 0;
        float _2pi = Mathf.PI * 2f;

        // Bottom cap
        int sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0f, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0f, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);
            vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vert += 2;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            vertices[vert] = new Vector3(cos * (topRadius1 + topRadius2 * .5f), height, sin * (topRadius1 + topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 + bottomRadius2 * .5f), 0, sin * (bottomRadius1 + bottomRadius2 * .5f));
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;
            float cos = Mathf.Cos(r1);
            float sin = Mathf.Sin(r1);

            vertices[vert] = new Vector3(cos * (topRadius1 - topRadius2 * .5f), height, sin * (topRadius1 - topRadius2 * .5f));
            vertices[vert + 1] = new Vector3(cos * (bottomRadius1 - bottomRadius2 * .5f), 0, sin * (bottomRadius1 - bottomRadius2 * .5f));
            vert += 2;
        }
        #endregion

        #region Normales

        // bottom + top + sides
        Vector3[] normales = new Vector3[vertices.Length];
        vert = 0;

        // Bottom cap
        while (vert < nbVerticesCap)
        {
            normales[vert++] = Vector3.down;
        }

        // Top cap
        while (vert < nbVerticesCap * 2)
        {
            normales[vert++] = Vector3.up;
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normales[vert] = new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1));
            normales[vert + 1] = normales[vert];
            vert += 2;
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            sideCounter = sideCounter == nbSides ? 0 : sideCounter;

            float r1 = (float)(sideCounter++) / nbSides * _2pi;

            normales[vert] = -(new Vector3(Mathf.Cos(r1), 0f, Mathf.Sin(r1)));
            normales[vert + 1] = normales[vert];
            vert += 2;
        }
        #endregion

        #region UVs
        Vector2[] uvs = new Vector2[vertices.Length];

        vert = 0;
        // Bottom cap
        sideCounter = 0;
        while (vert < nbVerticesCap)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(0f, t);
            uvs[vert++] = new Vector2(1f, t);
        }

        // Top cap
        sideCounter = 0;
        while (vert < nbVerticesCap * 2)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(0f, t);
            uvs[vert++] = new Vector2(1f, t);
        }

        // Sides (out)
        sideCounter = 0;
        while (vert < nbVerticesCap * 2 + nbVerticesSides)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(t, 0f);
            uvs[vert++] = new Vector2(t, 1f);
        }

        // Sides (in)
        sideCounter = 0;
        while (vert < vertices.Length)
        {
            float t = (float)(sideCounter++) / nbSides;
            uvs[vert++] = new Vector2(t, 0f);
            uvs[vert++] = new Vector2(t, 1f);
        }
        #endregion

        #region Triangles
        int nbFace = nbSides * 4;
        int nbTriangles = nbFace * 2;
        int nbIndexes = nbTriangles * 3;
        int[] triangles = new int[nbIndexes];

        // Bottom cap
        int i = 0;
        sideCounter = 0;
        while (sideCounter < nbSides)
        {
            int current = sideCounter * 2;
            int next = sideCounter * 2 + 2;

            triangles[i++] = next + 1;
            triangles[i++] = next;
            triangles[i++] = current;

            triangles[i++] = current + 1;
            triangles[i++] = next + 1;
            triangles[i++] = current;

            sideCounter++;
        }

        // Top cap
        while (sideCounter < nbSides * 2)
        {
            int current = sideCounter * 2 + 2;
            int next = sideCounter * 2 + 4;

            triangles[i++] = current;
            triangles[i++] = next;
            triangles[i++] = next + 1;

            triangles[i++] = current;
            triangles[i++] = next + 1;
            triangles[i++] = current + 1;

            sideCounter++;
        }

        // Sides (out)
        while (sideCounter < nbSides * 3)
        {
            int current = sideCounter * 2 + 4;
            int next = sideCounter * 2 + 6;

            triangles[i++] = current;
            triangles[i++] = next;
            triangles[i++] = next + 1;

            triangles[i++] = current;
            triangles[i++] = next + 1;
            triangles[i++] = current + 1;

            sideCounter++;
        }


        // Sides (in)
        while (sideCounter < nbSides * 4)
        {
            int current = sideCounter * 2 + 6;
            int next = sideCounter * 2 + 8;

            triangles[i++] = next + 1;
            triangles[i++] = next;
            triangles[i++] = current;

            triangles[i++] = current + 1;
            triangles[i++] = next + 1;
            triangles[i++] = current;

            sideCounter++;
        }
        #endregion

        mesh.vertices = vertices;
        mesh.normals = normales;
        mesh.uv = uvs;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.Optimize();

        filter.sharedMesh = mesh;

        return go;
    }
}
