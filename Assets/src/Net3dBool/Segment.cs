/**
 * Represents a line segment resulting from a intersection of a face and a plane.
 * 
 * <br><br>See: 
 * D. H. Laidlaw, W. B. Trumbore, and J. F. Hughes.  
 * "Constructive Solid Geometry for Polyhedral Objects" 
 * SIGGRAPH Proceedings, 1986, p.161. 
 * 
 * original author: Danilo Balby Silva Castanheira (danbalby@yahoo.com)
 * 
 * Ported from Java to C# by Sebastian Loncar, Web: http://loncar.de
 * Project: https://github.com/Arakis/Net3dBool
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Net3dBool
{
    public class Segment
    {
        /** line resulting from the two planes intersection */
        private Line line;
        /** shows how many ends were already defined */
        private int index;

        /** distance from the segment starting point to the point defining the plane */
        private double startDist;
        /** distance from the segment ending point to the point defining the plane */
        private double endDist;

        /** starting point status relative to the face */
        private int startType;
        /** intermediate status relative to the face */
        private int middleType;
        /** ending point status relative to the face */
        private int endType;

        /** nearest vertex from the starting point */
        private Vertex startVertex;
        /** nearest vertex from the ending point */
        private Vertex endVertex;

        /** start of the intersection point */
        private Point3d startPos;
        /** end of the intersection point */
        private Point3d endPos;

        /** define as vertex one of the segment ends */
        public static int VERTEX = 1;
        /** define as face one of the segment ends */
        public static  int FACE = 2;
        /** define as edge one of the segment ends */
        public static  int EDGE = 3;

        /** tolerance value to test equalities */
        private static  double TOL = 1e-10f;

        //---------------------------------CONSTRUCTORS---------------------------------//

        /**
     * Constructs a Segment based on elements obtained from the two planes relations 
     * 
     * @param line resulting from the two planes intersection
     * @param face face that intersects with the plane
     * @param sign1 position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)
     * @param sign2 position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)
     * @param sign3 position of the face vertex1 relative to the plane (-1 behind, 1 front, 0 on)  
     */
        public Segment(Line line, Face face, int sign1, int sign2, int sign3)
        {
            this.line = line;
            index = 0;

            //VERTEX is an end
            if (sign1 == 0)
            {
                setVertex(face.v1);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign2 == sign3)
                {
                    setVertex(face.v1);
                }
            }

            //VERTEX is an end
            if (sign2 == 0)
            {
                setVertex(face.v2);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign1 == sign3)
                {
                    setVertex(face.v2);
                }
            }

            //VERTEX is an end
            if (sign3 == 0)
            {
                setVertex(face.v3);
                //other vertices on the same side - VERTEX-VERTEX VERTEX
                if (sign1 == sign2)
                {
                    setVertex(face.v3);
                }
            }

            //There are undefined ends - one or more edges cut the planes intersection line
            if (getNumEndsSet() != 2)
            {
                //EDGE is an end
                if ((sign1 == 1 && sign2 == -1) || (sign1 == -1 && sign2 == 1))
                {
                    setEdge(face.v1, face.v2);
                }
                //EDGE is an end
                if ((sign2 == 1 && sign3 == -1) || (sign2 == -1 && sign3 == 1))
                {
                    setEdge(face.v2, face.v3);
                }
                //EDGE is an end
                if ((sign3 == 1 && sign1 == -1) || (sign3 == -1 && sign1 == 1))
                {
                    setEdge(face.v3, face.v1);
                }
            }
        }

        private Segment()
        {
        }

        //-----------------------------------OVERRIDES----------------------------------//

        /**
     * Clones the Segment object
     * 
     * @return cloned Segment object
     */
        public Segment Clone()
        {
            Segment clone = new Segment();
            clone.line = line.Clone();
            clone.index = index;
            clone.startDist = startDist;
            clone.endDist = endDist;
            clone.startDist = startType;
            clone.middleType = middleType;
            clone.endType = endType;
            clone.startVertex = startVertex.Clone();
            clone.endVertex = endVertex.Clone();
            clone.startPos = startPos.Clone();
            clone.endPos = endPos.Clone();

            return clone;
        }

        //-------------------------------------GETS-------------------------------------//

        /**
     * Gets the start vertex
     * 
     * @return start vertex
     */
        public Vertex getStartVertex()
        {
            return startVertex;
        }

        /**
     * Gets the end vertex
     * 
     * @return end vertex
     */
        public Vertex getEndVertex()
        {
            return endVertex;
        }

        /**
     * Gets the distance from the origin until the starting point
     * 
     * @return distance from the origin until the starting point
     */
        public double getStartDistance()
        {
            return startDist;
        }

        /**
     * Gets the distance from the origin until ending point
     * 
     * @return distance from the origin until the ending point
     */
        public double getEndDistance()
        {
            return endDist;
        }

        /**
     * Gets the type of the starting point
     * 
     * @return type of the starting point
     */
        public int getStartType()
        {
            return startType;
        }

        /**
     * Gets the type of the segment between the starting and ending points
     * 
     * @return type of the segment between the starting and ending points
     */
        public int getIntermediateType()
        {
            return middleType;
        }

        /**
     * Gets the type of the ending point
     * 
     * @return type of the ending point
     */
        public int getEndType()
        {
            return endType;
        }

        /**
     * Gets the number of ends already set
     *
     * @return number of ends already set
     */
        public int getNumEndsSet()
        {
            return index;
        }

        /**
     * Gets the starting position
     * 
     * @return start position
     */
        public Point3d getStartPosition()
        {
            return startPos;
        }

        /**
     * Gets the ending position
     * 
     * @return ending position
     */
        public Point3d getEndPosition()
        {
            return endPos;
        }

        //------------------------------------OTHERS------------------------------------//

        /**
     * Checks if two segments intersect
     * 
     * @param segment the other segment to check the intesection
     * @return true if the segments intersect, false otherwise
     */
        public bool intersect(Segment segment)
        {
            if (endDist < segment.startDist + TOL || segment.endDist < startDist + TOL)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        //---------------------------------PRIVATES-------------------------------------//

        /**
     * Sets an end as vertex (starting point if none end were defined, ending point otherwise)
     * 
     * @param vertex the vertex that is an segment end 
     * @return false if all the ends were already defined, true otherwise
     */
        private bool setVertex(Vertex vertex)
        {
            //none end were defined - define starting point as VERTEX
            if (index == 0)
            {
                startVertex = vertex;
                startType = VERTEX;
                startDist = line.computePointToPointDistance(vertex.getPosition());
                startPos = startVertex.getPosition();
                index++;
                return true;
            }
            //starting point were defined - define ending point as VERTEX
            if (index == 1)
            {
                endVertex = vertex;
                endType = VERTEX;
                endDist = line.computePointToPointDistance(vertex.getPosition());
                endPos = endVertex.getPosition();
                index++;

                //defining middle based on the starting point
                //VERTEX-VERTEX-VERTEX
                if (startVertex.equals(endVertex))
                {
                    middleType = VERTEX;
                }
                    //VERTEX-EDGE-VERTEX
                    else if (startType == VERTEX)
                {
                    middleType = EDGE;
                }

                //the ending point distance should be smaller than  starting point distance 
                if (startDist > endDist)
                {
                    swapEnds();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /**
     * Sets an end as edge (starting point if none end were defined, ending point otherwise)
     * 
     * @param vertex1 one of the vertices of the intercepted edge 
     * @param vertex2 one of the vertices of the intercepted edge
     * @return false if all ends were already defined, true otherwise
     */
        private bool setEdge(Vertex vertex1, Vertex vertex2)
        {
            Point3d point1 = vertex1.getPosition();
            Point3d point2 = vertex2.getPosition();
            Vector3d edgeDirection = new Vector3d(point2.x - point1.x, point2.y - point1.y, point2.z - point1.z);
            Line edgeLine = new Line(edgeDirection, point1);

            if (index == 0)
            {
                startVertex = vertex1;
                startType = EDGE;
                startPos = line.computeLineIntersection(edgeLine);
                startDist = line.computePointToPointDistance(startPos);
                middleType = FACE;
                index++;
                return true;        
            }
            else if (index == 1)
            {
                endVertex = vertex1;
                endType = EDGE;
                endPos = line.computeLineIntersection(edgeLine);
                endDist = line.computePointToPointDistance(endPos);
                middleType = FACE;
                index++;

                //the ending point distance should be smaller than  starting point distance 
                if (startDist > endDist)
                {
                    swapEnds();
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /** Swaps the starting point and the ending point */    
        private void swapEnds()
        {
            double distTemp = startDist;
            startDist = endDist;
            endDist = distTemp;

            int typeTemp = startType;
            startType = endType;
            endType = typeTemp;

            Vertex vertexTemp = startVertex;
            startVertex = endVertex;
            endVertex = vertexTemp;

            Point3d posTemp = startPos;
            startPos = endPos;
            endPos = posTemp;       
        }
    }
}
