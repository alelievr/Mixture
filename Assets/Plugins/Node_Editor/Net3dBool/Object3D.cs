/**
 * Data structure about a 3d solid to apply boolean operations in it.
 * 
 * <br><br>Tipically, two 'Object3d' objects are created to apply boolean operation. The
 * methods splitFaces() and classifyFaces() are called in this sequence for both objects,
 * always using the other one as parameter. Then the faces from both objects are collected
 * according their status.
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

    public class Object3D
    {
        /** solid vertices  */
        private List<Vertex> vertices;
        /** solid faces */
        private List<Face> faces;
        /** object representing the solid extremes */
        private Bound bound;

        /** tolerance value to test equalities */
        private static double TOL = 1e-10f;

        //----------------------------------CONSTRUCTOR---------------------------------//

        /** 
     * Constructs a Object3d object based on a solid file.
     *
     * @param solid solid used to construct the Object3d object  
     */ 
        public Object3D(Solid solid)
        {
            Vertex v1, v2, v3, vertex;
            Point3d[] verticesPoints = solid.getVertices();
            int[] indices = solid.getIndices();
            Color3f[] colors = solid.getColors();
            var verticesTemp = new List<Vertex>();

			Dictionary<int,int> revlookup = new Dictionary<int, int> ();
			for (int d=0; d<indices.Length; d++)
				revlookup [indices [d]] = d;

            //create vertices
            vertices = new List<Vertex>();
            for (int i = 0; i < verticesPoints.Length; i++)
            {
				Color3f col = new Color3f(1, 1, 1);
				if(colors.Length > 0)
					col = colors[i];

                vertex = addVertex(verticesPoints[i], col, Vertex.UNKNOWN);
                verticesTemp.Add(vertex); 
            }

            //create faces
            faces = new List<Face>();
            for (int i = 0; i < indices.Length; i = i + 3)
            {
                v1 = verticesTemp[indices[i]];
                v2 = verticesTemp[indices[i + 1]];
                v3 = verticesTemp[indices[i + 2]];
                addFace(v1, v2, v3);
            }

            //create bound
            bound = new Bound(verticesPoints);
        }

        private Object3D()
        {
        }

        //-----------------------------------OVERRIDES----------------------------------//

        /**
     * Clones the Object3D object
     * 
     * @return cloned Object3D object
     */
        public Object3D Clone()
        {
            Object3D clone = new Object3D();
            clone.vertices = new List<Vertex>();
            for (int i = 0; i < vertices.Count; i++)
            {
                clone.vertices.Add(vertices[i].Clone());
            }
            clone.faces = new List<Face>();
            for (int i = 0; i < vertices.Count; i++)
            {
                clone.faces.Add(faces[i].Clone());
            }
            clone.bound = bound;

            return clone;
        }

        //--------------------------------------GETS------------------------------------//

        /**
     * Gets the number of faces
     * 
     * @return number of faces
     */
        public int getNumFaces()
        {
            return faces.Count;
        }

        /**
     * Gets a face reference for a given position
     * 
     * @param index required face position
     * @return face reference , null if the position is invalid
     */
        public Face getFace(int index)
        {
            if (index < 0 || index >= faces.Count)
            {
                return null;
            }
            else
            {
                return faces[index];
            }
        }

        /**
     * Gets the solid bound
     * 
     * @return solid bound
     */
        public Bound getBound()
        {
            return bound;
        }

        //------------------------------------ADDS----------------------------------------//

        /**
     * Method used to add a face properly for internal methods
     * 
     * @param v1 a face vertex
     * @param v2 a face vertex
     * @param v3 a face vertex
     */
        private Face addFace(Vertex v1, Vertex v2, Vertex v3)
        {
            if (!(v1.equals(v2) || v1.equals(v3) || v2.equals(v3)))
            {
                Face face = new Face(v1, v2, v3);
                if (face.getArea() > TOL)
                {        
                    faces.Add(face);
                    return face;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        /**
     * Method used to add a vertex properly for internal methods
     * 
     * @param pos vertex position
     * @param color vertex color
     * @param status vertex status
     * @return the vertex inserted (if a similar vertex already exists, this is returned)
     */
        private Vertex addVertex(Point3d pos, Color3f color, int status)
        {
            int i;
            //if already there is an equal vertex, it is not inserted
            Vertex vertex = new Vertex(pos, color, status);
            for (i = 0; i < vertices.Count; i++)
            {
                if (vertex.equals(vertices[i]))
                    break;           
            }
            if (i == vertices.Count)
            {
                vertices.Add(vertex);
                return vertex;
            }
            else
            {
                vertex = vertices[i];
                vertex.setStatus(status);
                return vertex;
            }

        }

        //-------------------------FACES_SPLITTING_METHODS------------------------------//

        /**
     * Split faces so that none face is intercepted by a face of other object
     * 
     * @param object the other object 3d used to make the split 
     */
        public void splitFaces(Object3D obj)
        {
            Line line;
            Face face1, face2;
            Segment[] segments;
            Segment segment1;
            Segment segment2;
            double distFace1Vert1, distFace1Vert2, distFace1Vert3, distFace2Vert1, distFace2Vert2, distFace2Vert3;
            int signFace1Vert1, signFace1Vert2, signFace1Vert3, signFace2Vert1, signFace2Vert2, signFace2Vert3;
            int numFacesBefore = getNumFaces();
            int numFacesStart = getNumFaces();
            int facesIgnored = 0;

            //if the objects bounds overlap...                              
            if (getBound().overlap(obj.getBound()))
            {           
                //for each object1 face...
                for (int i = 0; i < getNumFaces(); i++)
                {
                    //if object1 face bound and object2 bound overlap ...
                    face1 = getFace(i);

                    if (face1.getBound().overlap(obj.getBound()))
                    {
                        //for each object2 face...
                        for (int j = 0; j < obj.getNumFaces(); j++)
                        {
                            //if object1 face bound and object2 face bound overlap...  
                            face2 = obj.getFace(j);
                            if (face1.getBound().overlap(face2.getBound()))
                            {
                                //PART I - DO TWO POLIGONS INTERSECT?
                                //POSSIBLE RESULTS: INTERSECT, NOT_INTERSECT, COPLANAR

                                //distance from the face1 vertices to the face2 plane
                                distFace1Vert1 = computeDistance(face1.v1, face2);
                                distFace1Vert2 = computeDistance(face1.v2, face2);
                                distFace1Vert3 = computeDistance(face1.v3, face2);

                                //distances signs from the face1 vertices to the face2 plane 
                                signFace1Vert1 = (distFace1Vert1 > TOL ? 1 : (distFace1Vert1 < -TOL ? -1 : 0)); 
                                signFace1Vert2 = (distFace1Vert2 > TOL ? 1 : (distFace1Vert2 < -TOL ? -1 : 0));
                                signFace1Vert3 = (distFace1Vert3 > TOL ? 1 : (distFace1Vert3 < -TOL ? -1 : 0));

                                //if all the signs are zero, the planes are coplanar
                                //if all the signs are positive or negative, the planes do not intersect
                                //if the signs are not equal...
                                if (!(signFace1Vert1 == signFace1Vert2 && signFace1Vert2 == signFace1Vert3))
                                {
                                    //distance from the face2 vertices to the face1 plane
                                    distFace2Vert1 = computeDistance(face2.v1, face1);
                                    distFace2Vert2 = computeDistance(face2.v2, face1);
                                    distFace2Vert3 = computeDistance(face2.v3, face1);

                                    //distances signs from the face2 vertices to the face1 plane
                                    signFace2Vert1 = (distFace2Vert1 > TOL ? 1 : (distFace2Vert1 < -TOL ? -1 : 0)); 
                                    signFace2Vert2 = (distFace2Vert2 > TOL ? 1 : (distFace2Vert2 < -TOL ? -1 : 0));
                                    signFace2Vert3 = (distFace2Vert3 > TOL ? 1 : (distFace2Vert3 < -TOL ? -1 : 0));

                                    //if the signs are not equal...
                                    if (!(signFace2Vert1 == signFace2Vert2 && signFace2Vert2 == signFace2Vert3))
                                    {
                                        line = new Line(face1, face2);

                                        //intersection of the face1 and the plane of face2
                                        segment1 = new Segment(line, face1, signFace1Vert1, signFace1Vert2, signFace1Vert3);

                                        //intersection of the face2 and the plane of face1
                                        segment2 = new Segment(line, face2, signFace2Vert1, signFace2Vert2, signFace2Vert3);

                                        //if the two segments intersect...
                                        if (segment1.intersect(segment2))
                                        {
                                            //PART II - SUBDIVIDING NON-COPLANAR POLYGONS
                                            int lastNumFaces = getNumFaces();
                                            this.splitFace(i, segment1, segment2);

                                            //prevent from infinite loop (with a loss of faces...)
                                            //if(numFacesStart*20<getNumFaces())
                                            //{
                                            //  System.out.println("possible infinite loop situation: terminating faces split");
                                            //  return;
                                            //}

                                            //if the face in the position isn't the same, there was a break 
                                            if (face1 != getFace(i))
                                            {
                                                //if the generated solid is equal the origin...
                                                if (face1.equals(getFace(getNumFaces() - 1)))
                                                {
                                                    //return it to its position and jump it
                                                    if (i != (getNumFaces() - 1))
                                                    {
                                                        faces.RemoveAt(getNumFaces() - 1);
                                                        faces.Insert(i, face1);
                                                    }
                                                    else
                                                    {
                                                        continue;
                                                    }
                                                }
                                                                                    //else: test next face
                                                                                    else
                                                {
                                                    i--;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /**
     * Computes closest distance from a vertex to a plane
     * 
     * @param vertex vertex used to compute the distance
     * @param face face representing the plane where it is contained
     * @return the closest distance from the vertex to the plane
     */
        private double computeDistance(Vertex vertex, Face face)
        {
            Vector3d normal = face.getNormal();
            double a = normal.x;
            double b = normal.y;
            double c = normal.z;
            double d = -(a * face.v1.x + b * face.v1.y + c * face.v1.z);
            return a * vertex.x + b * vertex.y + c * vertex.z + d;
        }

        /**
     * Split an individual face
     * 
     * @param facePos face position on the array of faces
     * @param segment1 segment representing the intersection of the face with the plane
     * of another face
     * @return segment2 segment representing the intersection of other face with the
     * plane of the current face plane
     */   
        private void splitFace(int facePos, Segment segment1, Segment segment2)
        {
            Vertex startPosVertex, endPosVertex;
            Point3d startPos, endPos;
            int startType, endType, middleType;
            double startDist, endDist;

            Face face = getFace(facePos);
            Vertex startVertex = segment1.getStartVertex();
            Vertex endVertex = segment1.getEndVertex();

            //starting point: deeper starting point         
            if (segment2.getStartDistance() > segment1.getStartDistance() + TOL)
            {
                startDist = segment2.getStartDistance();
                startType = segment1.getIntermediateType();
                startPos = segment2.getStartPosition();
            }
            else
            {
                startDist = segment1.getStartDistance();
                startType = segment1.getStartType();
                startPos = segment1.getStartPosition();
            }

            //ending point: deepest ending point
            if (segment2.getEndDistance() < segment1.getEndDistance() - TOL)
            {
                endDist = segment2.getEndDistance();
                endType = segment1.getIntermediateType();
                endPos = segment2.getEndPosition();
            }
            else
            {
                endDist = segment1.getEndDistance();
                endType = segment1.getEndType();
                endPos = segment1.getEndPosition();
            }       
            middleType = segment1.getIntermediateType();

            //set vertex to BOUNDARY if it is start type        
            if (startType == Segment.VERTEX)
            {
                startVertex.setStatus(Vertex.BOUNDARY);
            }

            //set vertex to BOUNDARY if it is end type
            if (endType == Segment.VERTEX)
            {
                endVertex.setStatus(Vertex.BOUNDARY);
            }

            //VERTEX-_______-VERTEX 
            if (startType == Segment.VERTEX && endType == Segment.VERTEX)
            {
                return;
            }

            //______-EDGE-______
            else if (middleType == Segment.EDGE)
            {
                //gets the edge 
                int splitEdge;
                if ((startVertex == face.v1 && endVertex == face.v2) || (startVertex == face.v2 && endVertex == face.v1))
                {
                    splitEdge = 1;
                }
                else if ((startVertex == face.v2 && endVertex == face.v3) || (startVertex == face.v3 && endVertex == face.v2))
                {     
                    splitEdge = 2; 
                }
                else
                {
                    splitEdge = 3;
                } 

                //VERTEX-EDGE-EDGE
                if (startType == Segment.VERTEX)
                {
                    breakFaceInTwo(facePos, endPos, splitEdge);
                    return;
                }

                    //EDGE-EDGE-VERTEX
                    else if (endType == Segment.VERTEX)
                {
                    breakFaceInTwo(facePos, startPos, splitEdge);
                    return;
                }

                    // EDGE-EDGE-EDGE
                    else if (startDist == endDist)
                {
                    breakFaceInTwo(facePos, endPos, splitEdge);
                }
                else
                {
                    if ((startVertex == face.v1 && endVertex == face.v2) || (startVertex == face.v2 && endVertex == face.v3) || (startVertex == face.v3 && endVertex == face.v1))
                    {
                        breakFaceInThree(facePos, startPos, endPos, splitEdge);
                    }
                    else
                    {
                        breakFaceInThree(facePos, endPos, startPos, splitEdge);
                    }
                }
                return;
            }

            //______-FACE-______

            //VERTEX-FACE-EDGE
            else if (startType == Segment.VERTEX && endType == Segment.EDGE)
            {
                breakFaceInTwo(facePos, endPos, endVertex);
            }
            //EDGE-FACE-VERTEX
            else if (startType == Segment.EDGE && endType == Segment.VERTEX)
            {
                breakFaceInTwo(facePos, startPos, startVertex);
            }
            //VERTEX-FACE-FACE
            else if (startType == Segment.VERTEX && endType == Segment.FACE)
            {
                breakFaceInThree(facePos, endPos, startVertex);
            }
            //FACE-FACE-VERTEX
            else if (startType == Segment.FACE && endType == Segment.VERTEX)
            {
                breakFaceInThree(facePos, startPos, endVertex);
            }
            //EDGE-FACE-EDGE
            else if (startType == Segment.EDGE && endType == Segment.EDGE)
            {
                breakFaceInThree(facePos, startPos, endPos, startVertex, endVertex);
            }
            //EDGE-FACE-FACE
            else if (startType == Segment.EDGE && endType == Segment.FACE)
            {
                breakFaceInFour(facePos, startPos, endPos, startVertex);
            }
            //FACE-FACE-EDGE
            else if (startType == Segment.FACE && endType == Segment.EDGE)
            {
                breakFaceInFour(facePos, endPos, startPos, endVertex);
            }
            //FACE-FACE-FACE
            else if (startType == Segment.FACE && endType == Segment.FACE)
            {
                Vector3d segmentVector = new Vector3d(startPos.x - endPos.x, startPos.y - endPos.y, startPos.z - endPos.z);

                //if the intersection segment is a point only...
                if (Math.Abs(segmentVector.x) < TOL && Math.Abs(segmentVector.y) < TOL && Math.Abs(segmentVector.z) < TOL)
                {
                    breakFaceInThree(facePos, startPos);
                    return;
                }

                //gets the vertex more lined with the intersection segment
                int linedVertex;
                Point3d linedVertexPos;
                Vector3d vertexVector = new Vector3d(endPos.x - face.v1.x, endPos.y - face.v1.y, endPos.z - face.v1.z);
                vertexVector.normalize();
                double dot1 = Math.Abs(segmentVector.dot(vertexVector));
                vertexVector = new Vector3d(endPos.x - face.v2.x, endPos.y - face.v2.y, endPos.z - face.v2.z);
                vertexVector.normalize();
                double dot2 = Math.Abs(segmentVector.dot(vertexVector));
                vertexVector = new Vector3d(endPos.x - face.v3.x, endPos.y - face.v3.y, endPos.z - face.v3.z);
                vertexVector.normalize();
                double dot3 = Math.Abs(segmentVector.dot(vertexVector));
                if (dot1 > dot2 && dot1 > dot3)
                {
                    linedVertex = 1;
                    linedVertexPos = face.v1.getPosition();
                }
                else if (dot2 > dot3 && dot2 > dot1)
                {
                    linedVertex = 2;
                    linedVertexPos = face.v2.getPosition();
                }
                else
                {
                    linedVertex = 3;
                    linedVertexPos = face.v3.getPosition();
                }

                // Now find which of the intersection endpoints is nearest to that vertex.
                if (linedVertexPos.distance(startPos) > linedVertexPos.distance(endPos))
                {
                    breakFaceInFive(facePos, startPos, endPos, linedVertex);
                }
                else
                {
                    breakFaceInFive(facePos, endPos, startPos, linedVertex);
                }
            }
        }

        /**
     * Face breaker for VERTEX-EDGE-EDGE / EDGE-EDGE-VERTEX
     * 
     * @param facePos face position on the faces array
     * @param newPos new vertex position
     * @param edge that will be split 
     */     
        private void breakFaceInTwo(int facePos, Point3d newPos, int splitEdge)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex = addVertex(newPos, face.v1.getColor(), Vertex.BOUNDARY); 

            if (splitEdge == 1)
            {
                addFace(face.v1, vertex, face.v3);
                addFace(vertex, face.v2, face.v3);
            }
            else if (splitEdge == 2)
            {
                addFace(face.v2, vertex, face.v1);
                addFace(vertex, face.v3, face.v1);
            }
            else
            {
                addFace(face.v3, vertex, face.v2);
                addFace(vertex, face.v1, face.v2);
            }
        }

        /**
     * Face breaker for VERTEX-FACE-EDGE / EDGE-FACE-VERTEX
     * 
     * @param facePos face position on the faces array
     * @param newPos new vertex position
     * @param endVertex vertex used for splitting 
     */     
        private void breakFaceInTwo(int facePos, Point3d newPos, Vertex endVertex)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex = addVertex(newPos, face.v1.getColor(), Vertex.BOUNDARY);

            if (endVertex.equals(face.v1))
            {
                addFace(face.v1, vertex, face.v3);
                addFace(vertex, face.v2, face.v3);
            }
            else if (endVertex.equals(face.v2))
            {
                addFace(face.v2, vertex, face.v1);
                addFace(vertex, face.v3, face.v1);
            }
            else
            {
                addFace(face.v3, vertex, face.v2);
                addFace(vertex, face.v1, face.v2);
            }
        }

        /**
     * Face breaker for EDGE-EDGE-EDGE
     * 
     * @param facePos face position on the faces array
     * @param newPos1 new vertex position
     * @param newPos2 new vertex position 
     * @param splitEdge edge that will be split
     */
        private void breakFaceInThree(int facePos, Point3d newPos1, Point3d newPos2, int splitEdge)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex1 = addVertex(newPos1, face.v1.getColor(), Vertex.BOUNDARY);   
            Vertex vertex2 = addVertex(newPos2, face.v1.getColor(), Vertex.BOUNDARY);

            if (splitEdge == 1)
            {
                addFace(face.v1, vertex1, face.v3);
                addFace(vertex1, vertex2, face.v3);
                addFace(vertex2, face.v2, face.v3);
            }
            else if (splitEdge == 2)
            {
                addFace(face.v2, vertex1, face.v1);
                addFace(vertex1, vertex2, face.v1);
                addFace(vertex2, face.v3, face.v1);
            }
            else
            {
                addFace(face.v3, vertex1, face.v2);
                addFace(vertex1, vertex2, face.v2);
                addFace(vertex2, face.v1, face.v2);
            }
        }

        /**
     * Face breaker for VERTEX-FACE-FACE / FACE-FACE-VERTEX
     * 
     * @param facePos face position on the faces array
     * @param newPos new vertex position
     * @param endVertex vertex used for the split
     */
        private void breakFaceInThree(int facePos, Point3d newPos, Vertex endVertex)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex = addVertex(newPos, face.v1.getColor(), Vertex.BOUNDARY);

            if (endVertex.equals(face.v1))
            {
                addFace(face.v1, face.v2, vertex);
                addFace(face.v2, face.v3, vertex);
                addFace(face.v3, face.v1, vertex);
            }
            else if (endVertex.equals(face.v2))
            {
                addFace(face.v2, face.v3, vertex);
                addFace(face.v3, face.v1, vertex);
                addFace(face.v1, face.v2, vertex);
            }
            else
            {
                addFace(face.v3, face.v1, vertex);
                addFace(face.v1, face.v2, vertex);
                addFace(face.v2, face.v3, vertex);
            }
        }

        /**
     * Face breaker for EDGE-FACE-EDGE
     * 
     * @param facePos face position on the faces array
     * @param newPos1 new vertex position
     * @param newPos2 new vertex position 
     * @param startVertex vertex used the new faces creation
     * @param endVertex vertex used for the new faces creation
     */
        private void breakFaceInThree(int facePos, Point3d newPos1, Point3d newPos2, Vertex startVertex, Vertex endVertex)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex1 = addVertex(newPos1, face.v1.getColor(), Vertex.BOUNDARY);
            Vertex vertex2 = addVertex(newPos2, face.v1.getColor(), Vertex.BOUNDARY);

            if (startVertex.equals(face.v1) && endVertex.equals(face.v2))
            {
                addFace(face.v1, vertex1, vertex2);
                addFace(face.v1, vertex2, face.v3);
                addFace(vertex1, face.v2, vertex2);
            }
            else if (startVertex.equals(face.v2) && endVertex.equals(face.v1))
            {
                addFace(face.v1, vertex2, vertex1);
                addFace(face.v1, vertex1, face.v3);
                addFace(vertex2, face.v2, vertex1);
            }
            else if (startVertex.equals(face.v2) && endVertex.equals(face.v3))
            {
                addFace(face.v2, vertex1, vertex2);
                addFace(face.v2, vertex2, face.v1);
                addFace(vertex1, face.v3, vertex2);
            }
            else if (startVertex.equals(face.v3) && endVertex.equals(face.v2))
            {
                addFace(face.v2, vertex2, vertex1);
                addFace(face.v2, vertex1, face.v1);
                addFace(vertex2, face.v3, vertex1);
            }
            else if (startVertex.equals(face.v3) && endVertex.equals(face.v1))
            {
                addFace(face.v3, vertex1, vertex2);
                addFace(face.v3, vertex2, face.v2);
                addFace(vertex1, face.v1, vertex2);
            }
            else
            {
                addFace(face.v3, vertex2, vertex1);
                addFace(face.v3, vertex1, face.v2);
                addFace(vertex2, face.v1, vertex1);
            }
        }

        /**
     * Face breaker for FACE-FACE-FACE (a point only)
     * 
     * @param facePos face position on the faces array
     * @param newPos new vertex position
     */
        private void breakFaceInThree(int facePos, Point3d newPos)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex = addVertex(newPos, face.v1.getColor(), Vertex.BOUNDARY);

            addFace(face.v1, face.v2, vertex);
            addFace(face.v2, face.v3, vertex);
            addFace(face.v3, face.v1, vertex);
        }

        /**
     * Face breaker for EDGE-FACE-FACE / FACE-FACE-EDGE
     * 
     * @param facePos face position on the faces array
     * @param newPos1 new vertex position
     * @param newPos2 new vertex position 
     * @param endVertex vertex used for the split
     */ 
        private void breakFaceInFour(int facePos, Point3d newPos1, Point3d newPos2, Vertex endVertex)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex1 = addVertex(newPos1, face.v1.getColor(), Vertex.BOUNDARY);
            Vertex vertex2 = addVertex(newPos2, face.v1.getColor(), Vertex.BOUNDARY);

            if (endVertex.equals(face.v1))
            {
                addFace(face.v1, vertex1, vertex2);
                addFace(vertex1, face.v2, vertex2);
                addFace(face.v2, face.v3, vertex2);
                addFace(face.v3, face.v1, vertex2);
            }
            else if (endVertex.equals(face.v2))
            {
                addFace(face.v2, vertex1, vertex2);
                addFace(vertex1, face.v3, vertex2);
                addFace(face.v3, face.v1, vertex2);
                addFace(face.v1, face.v2, vertex2);
            }
            else
            {
                addFace(face.v3, vertex1, vertex2);
                addFace(vertex1, face.v1, vertex2);
                addFace(face.v1, face.v2, vertex2);
                addFace(face.v2, face.v3, vertex2);
            }
        }

        /**
     * Face breaker for FACE-FACE-FACE
     * 
     * @param facePos face position on the faces array
     * @param newPos1 new vertex position
     * @param newPos2 new vertex position 
     * @param linedVertex what vertex is more lined with the interersection found
     */     
        private void breakFaceInFive(int facePos, Point3d newPos1, Point3d newPos2, int linedVertex)
        {
            Face face = faces[facePos];
            faces.RemoveAt(facePos);

            Vertex vertex1 = addVertex(newPos1, face.v1.getColor(), Vertex.BOUNDARY);
            Vertex vertex2 = addVertex(newPos2, face.v1.getColor(), Vertex.BOUNDARY);

            double cont = 0;        
            if (linedVertex == 1)
            {
                addFace(face.v2, face.v3, vertex1);
                addFace(face.v2, vertex1, vertex2);
                addFace(face.v3, vertex2, vertex1);
                addFace(face.v2, vertex2, face.v1);
                addFace(face.v3, face.v1, vertex2);
            }
            else if (linedVertex == 2)
            {
                addFace(face.v3, face.v1, vertex1);
                addFace(face.v3, vertex1, vertex2);
                addFace(face.v1, vertex2, vertex1);
                addFace(face.v3, vertex2, face.v2);
                addFace(face.v1, face.v2, vertex2);
            }
            else
            {
                addFace(face.v1, face.v2, vertex1);
                addFace(face.v1, vertex1, vertex2);
                addFace(face.v2, vertex2, vertex1);
                addFace(face.v1, vertex2, face.v3);
                addFace(face.v2, face.v3, vertex2);
            }
        }

        //-----------------------------------OTHERS-------------------------------------//

        /**
     * Classify faces as being inside, outside or on boundary of other object
     * 
     * @param object object 3d used for the comparison
     */
        public void classifyFaces(Object3D obj)
        {
            //calculate adjacency information
            Face face;
            for (int i = 0; i < this.getNumFaces(); i++)
            {
                face = this.getFace(i);
                face.v1.addAdjacentVertex(face.v2);
                face.v1.addAdjacentVertex(face.v3);
                face.v2.addAdjacentVertex(face.v1);
                face.v2.addAdjacentVertex(face.v3);
                face.v3.addAdjacentVertex(face.v1);
                face.v3.addAdjacentVertex(face.v2);
            }

            //for each face
            for (int i = 0; i < getNumFaces(); i++)
            {
                face = getFace(i);

                //if the face vertices aren't classified to make the simple classify
                if (face.simpleClassify() == false)
                {
                    //makes the ray trace classification
                    face.rayTraceClassify(obj);

                    //mark the vertices
                    if (face.v1.getStatus() == Vertex.UNKNOWN)
                    {
                        face.v1.mark(face.getStatus());
                    }
                    if (face.v2.getStatus() == Vertex.UNKNOWN)
                    {
                        face.v2.mark(face.getStatus());
                    }
                    if (face.v3.getStatus() == Vertex.UNKNOWN)
                    {
                        face.v3.mark(face.getStatus());
                    }
                }
            }
        }

        /** Inverts faces classified as INSIDE, making its normals point outside. Usually
     *  used into the second solid when the difference is applied. */
        public void invertInsideFaces()
        {
            Face face;
            for (int i = 0; i < getNumFaces(); i++)
            {
                face = getFace(i);
                if (face.getStatus() == Face.INSIDE)
                {
                    face.invert();
                }
            }
        }
    }
}

