/**
 * Class representing a 3D solid.
 *  
 * original author: Danilo Balby Silva Castanheira (danbalby@yahoo.com)
 * 
 * Ported from Java to C# by Sebastian Loncar, Web: http://loncar.de
 * Project: https://github.com/Arakis/Net3dBool
 */

using System;
using System.IO;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;

namespace Net3dBool
{
    public class Solid : Shape3D
    {
		// The last matrix applied to the vertices 
		private Matrix4x4	mat;

        /** array of indices for the vertices from the 'vertices' attribute */
        protected int[] indices;
        /** array of points defining the solid's vertices */
        protected Point3d[] vertices;
        /** array of color defining the vertices colors */
        protected Color3f[] colors;

        //--------------------------------CONSTRUCTORS----------------------------------//

        /** Constructs an empty solid. */           
        public Solid()
        {
            setInitialFeatures();
        }

        /**
     * Construct a solid based on data arrays. An exception may occur in the case of 
     * abnormal arrays (indices making references to inexistent vertices, there are less
     * colors than vertices...)
     * 
     * @param vertices array of points defining the solid vertices
     * @param indices array of indices for a array of vertices
     * @param colors array of colors defining the vertices colors 
     */
        public Solid(Point3d[] vertices, int[] indices, Color3f[] colors)
            : this()
        {
            setData(vertices, indices, colors);     
        }


		public Solid(Vector3 [] vertices, int [] indices, Color[] colors)
			: this()
		{
			setData (vertices, indices, colors);
		}
        /**
     * Constructs a solid based on a coordinates file. It contains vertices and indices, 
     * and its format is like this:
     * 
     * <br><br>4
     * <br>0 -5.00000000000000E-0001 -5.00000000000000E-0001 -5.00000000000000E-0001
     * <br>1  5.00000000000000E-0001 -5.00000000000000E-0001 -5.00000000000000E-0001
     * <br>2 -5.00000000000000E-0001  5.00000000000000E-0001 -5.00000000000000E-0001
     * <br>3  5.00000000000000E-0001  5.00000000000000E-0001 -5.00000000000000E-0001
     * 
     * <br><br>2
     * <br>0 0 2 3
     * <br>1 3 1 0 
     * 
     * @param solidFile file containing the solid coordinates
     * @param color solid color
     */
        public Solid(FileInfo solidFile, Color3f color)
            : this()
        {
            loadCoordinateFile(solidFile, color);
        }

        /** Sets the initial features common to all constructors */
        protected void setInitialFeatures()
        {
            vertices = new Point3d[0];
            colors = new Color3f[0];
            indices = new int[0];

//            setCapability(Shape3D.ALLOW_GEOMETRY_WRITE);
//            setCapability(Shape3D.ALLOW_APPEARANCE_WRITE);
//            setCapability(Shape3D.ALLOW_APPEARANCE_READ);
        }

        //---------------------------------------GETS-----------------------------------//

        /**
     * Gets the solid vertices
     * 
     * @return solid vertices
     */ 
        public Point3d[] getVertices()
        {
            Point3d[] newVertices = new Point3d[vertices.Length];
            for (int i = 0; i < newVertices.Length; i++)
            {
                newVertices[i] = vertices[i];
            }
            return newVertices;
        }

        /** Gets the solid indices for its vertices
     * 
     * @return solid indices for its vertices
     */
        public int[] getIndices()
        {
            int[] newIndices = new int[indices.Length];
            Array.Copy(indices, 0, newIndices, 0, indices.Length);
            return newIndices;
        }

        /** Gets the vertices colors
     * 
     * @return vertices colors
     */
        public Color3f[] getColors()
        {
            Color3f[] newColors = new Color3f[colors.Length];
            for (int i = 0; i < newColors.Length; i++)
            {
                newColors[i] = colors[i];
            }
            return newColors;
        }

        /**
     * Gets if the solid is empty (without any vertex)
     * 
     * @return true if the solid is empty, false otherwise
     */
        public bool isEmpty()
        {
            if (indices.Length == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //---------------------------------------SETS-----------------------------------//

        /**
     * Sets the solid data. Each vertex may have a different color. An exception may 
     * occur in the case of abnormal arrays (e.g., indices making references to  
     * inexistent vertices, there are less colors than vertices...)
     * 
     * @param vertices array of points defining the solid vertices
     * @param indices array of indices for a array of vertices
     * @param colors array of colors defining the vertices colors 
     */
        public void setData(Point3d[] vertices, int[] indices, Color3f[] colors)
        {
            this.vertices = new Point3d[vertices.Length];
            this.colors = new Color3f[colors.Length];
            this.indices = new int[indices.Length];
            if (indices.Length != 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    this.vertices[i] = vertices[i].Clone();
                    this.colors[i] = colors[i].Clone();
                }
                Array.Copy(indices, 0, this.indices, 0, indices.Length);

                defineGeometry();
            }
        }

		public void setData(Vector3[] vertices, int[] indices, Color[] colors)
		{
			this.vertices = new Point3d[vertices.Length];
			this.colors = new Color3f[colors.Length];
			this.indices = new int[indices.Length];
			if (indices.Length != 0)
			{
				for (int i = 0; i < vertices.Length; i++)
					this.vertices[i] = new Point3d(vertices[i].x, vertices[i].y, vertices[i].z) ;
				for(int j=0; j<colors.Length; j++)
					this.colors[j] = new Color3f(colors[j].r, colors[j].g, colors[j].b);
				Array.Copy(indices, 0, this.indices, 0, indices.Length);
				
				defineGeometry();
			}
		}


        /**
     * Sets the solid data. Defines the same color to all the vertices. An exception may 
     * may occur in the case of abnormal arrays (e.g., indices making references to  
     * inexistent vertices...)
     * 
     * @param vertices array of points defining the solid vertices
     * @param indices array of indices for a array of vertices
     * @param color the color of the vertices (the solid color) 
     */
        public void setData(Point3d[] vertices, int[] indices, Color3f color)
        {
            Color3f[] colors = new Color3f[vertices.Length];
            colors.fill(color);
            setData(vertices, indices, colors);
        }

        //-------------------------GEOMETRICAL_TRANSFORMATIONS-------------------------//

	
		public void ApplyMatrix(Matrix4x4 m)
		{
			mat = m;
			for (int i = 0; i < vertices.Length; i++)
			{
				Point3d p = this.vertices[i];
				// TODO: WARNING DATA LOSS!!!!
				Vector3 v = m.MultiplyPoint3x4( new Vector3((float)p.x, (float)p.y, (float)p.z) );
				this.vertices[i] = new Point3d(v.x, v.y, v.z);
			}
			
			defineGeometry();
		}

        /**
     * Applies a translation into a solid
     * 
     * @param dx translation on the x axis
     * @param dy translation on the y axis
     */
        public void translate(double dx, double dy)
        {
            if (dx != 0 || dy != 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].x += dx;
                    vertices[i].y += dy;
                }

                defineGeometry();
            }
        }

        /**
     * Applies a rotation into a solid
     * 
     * @param dx rotation on the x axis
     * @param dy rotation on the y axis
     */
        public void rotate(double dx, double dy)
        {
            double cosX = Math.Cos(dx);
            double cosY = Math.Cos(dy);
            double sinX = Math.Sin(dx);
            double sinY = Math.Sin(dy);

            if (dx != 0 || dy != 0)
            {
                //get mean
                Point3d mean = getMean();

                double newX, newY, newZ;
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].x -= mean.x; 
                    vertices[i].y -= mean.y; 
                    vertices[i].z -= mean.z; 

                    //x rotation
                    if (dx != 0)
                    {
                        newY = vertices[i].y * cosX - vertices[i].z * sinX;
                        newZ = vertices[i].y * sinX + vertices[i].z * cosX;
                        vertices[i].y = newY;
                        vertices[i].z = newZ;
                    }

                    //y rotation
                    if (dy != 0)
                    {
                        newX = vertices[i].x * cosY + vertices[i].z * sinY;
                        newZ = -vertices[i].x * sinY + vertices[i].z * cosY;
                        vertices[i].x = newX;
                        vertices[i].z = newZ;
                    }

                    vertices[i].x += mean.x; 
                    vertices[i].y += mean.y; 
                    vertices[i].z += mean.z;
                }
            }

            defineGeometry();
        }

        /**
     * Applies a zoom into a solid
     * 
     * @param dz translation on the z axis
     */
        public void zoom(double dz)
        {
            if (dz != 0)
            {
                for (int i = 0; i < vertices.Length; i++)
                {
                    vertices[i].z += dz;
                }

                defineGeometry();
            }
        }

        /**
     * Applies a scale changing into the solid
     * 
     * @param dx scale changing for the x axis 
     * @param dy scale changing for the y axis
     * @param dz scale changing for the z axis
     */
        public void scale(double dx, double dy, double dz)
        {
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i].x *= dx;
                vertices[i].y *= dy;
                vertices[i].z *= dz;
            }

            defineGeometry();
        }

        //-----------------------------------PRIVATES--------------------------------//

        /** Creates a geometry based on the indexes and vertices set for the solid */
        protected void defineGeometry()
        {
//            GeometryInfo gi = new GeometryInfo(GeometryInfo.TRIANGLE_ARRAY);
//            gi.setCoordinateIndices(indices);
//            gi.setCoordinates(vertices);
//            NormalGenerator ng = new NormalGenerator();
//            ng.generateNormals(gi);
//
//            gi.setColors(colors);
//            gi.setColorIndices(indices);
//            gi.recomputeIndices();
//
//            setGeometry(gi.getIndexedGeometryArray());
        }

        /**
     * Loads a coordinates file, setting vertices and indices 
     * 
     * @param solidFile file used to create the solid
     * @param color solid color
     */
        protected void loadCoordinateFile(FileInfo solidFile, Color3f color)
        {
//            try
//            {
//                BufferedReader reader = new BufferedReader(new FileReader(solidFile));
//
//                String line = reader.readLine();
//                int numVertices = Integer.parseInt(line);
//                vertices = new Point3d[numVertices];
//
//                StringTokenizer tokens;
//                String token;
//
//                for(int i=0;i<numVertices;i++)
//                    {
//                        line = reader.readLine();
//                        tokens = new StringTokenizer(line);
//                        tokens.nextToken();
//                        vertices[i]= new Point3d(Double.parseDouble(tokens.nextToken()), Double.parseDouble(tokens.nextToken()), Double.parseDouble(tokens.nextToken()));
//                    }
//
//                reader.readLine();
//
//                line = reader.readLine();
//                int numTriangles = Integer.parseInt(line);
//                indices = new int[numTriangles*3];
//
//                for(int i=0,j=0;i<numTriangles*3;i=i+3,j++)
//                    {
//                        line = reader.readLine();
//                        tokens = new StringTokenizer(line);
//                        tokens.nextToken();
//                        indices[i] = Integer.parseInt(tokens.nextToken());
//                        indices[i+1] = Integer.parseInt(tokens.nextToken());
//                        indices[i+2] = Integer.parseInt(tokens.nextToken());
//                    }
//
//                colors = new Color3f[vertices.Length];
//                Arrays.fill(colors, color);
//
//                defineGeometry();
//            }
//
//            catch(IOException e)
//            {
//                System.out.println("invalid file!");
//                e.printStackTrace();
//            }
        }

        /**
     * Gets the solid mean
     * 
     * @return point representing the mean
     */
        protected Point3d getMean()
        {
            Point3d mean = new Point3d();
            for (int i = 0; i < vertices.Length; i++)
            {
                mean.x += vertices[i].x;
                mean.y += vertices[i].y;
                mean.z += vertices[i].z;
            }
            mean.x /= vertices.Length;
            mean.y /= vertices.Length;
            mean.z /= vertices.Length;

            return mean;
        }
    }
}

