/**
 * Representation of a 3D face (triangle).
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
    public class Face
    {
        /** first vertex */
        public Vertex v1;
        /** second vertex */
        public Vertex v2;
        /** third vertex */
        public Vertex v3;
        /** face status relative to a solid  */
        private int status;

        /** face status if it is still unknown */
        public static int UNKNOWN = 1;
        /** face status if it is inside a solid */
        public static int INSIDE = 2;
        /** face status if it is outside a solid */
        public static  int OUTSIDE = 3;
        /** face status if it is coincident with a solid face */
        public static  int SAME = 4;
        /** face status if it is coincident with a solid face with opposite orientation*/
        public static  int OPPOSITE = 5;

        /** point status if it is up relative to an edge - see linePositionIn_ methods */
        private static  int UP = 6;
        /** point status if it is down relative to an edge - see linePositionIn_ methods */
        private static  int DOWN = 7;
        /** point status if it is on an edge - see linePositionIn_ methods */
        private static  int ON = 8;
        /** point status if it isn't up, down or on relative to an edge - see linePositionIn_ methods */
        private static  int NONE = 9;

        /** tolerance value to test equalities */
        private static  double TOL = 1e-10f;

        //---------------------------------CONSTRUCTORS---------------------------------//

        /**
     * Constructs a face with unknown status.
     * 
     * @param v1 a face vertex
     * @param v2 a face vertex
     * @param v3 a face vertex
     */
        public Face(Vertex v1, Vertex v2, Vertex v3)
        {
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
            UpdateBound();
            status = UNKNOWN;
        }

        private Face()
        {
        }

        //-----------------------------------OVERRIDES----------------------------------//

        /**
     * Clones the face object
     * 
     * @return cloned face object
     */
        public Face Clone()
        {
            Face clone = new Face();
            clone.v1 = v1.Clone();
            clone.v2 = v2.Clone();
            clone.v3 = v3.Clone();
            clone.m_bound = m_bound;
            clone.status = status;
            return clone;
        }

        /**
     * Makes a string definition for the Face object
     * 
     * @return the string definition
     */
        public String toString()
        {
            return v1.toString() + "\n" + v2.toString() + "\n" + v3.toString();
        }

        /**
     * Checks if a face is equal to another. To be equal, they have to have equal
     * vertices in the same order
     * 
     * @param anObject the other face to be tested
     * @return true if they are equal, false otherwise. 
     */
        public bool equals(Face face)
        {
            bool cond1 = v1.equals(face.v1) && v2.equals(face.v2) && v3.equals(face.v3);
            bool cond2 = v1.equals(face.v2) && v2.equals(face.v3) && v3.equals(face.v1);
            bool cond3 = v1.equals(face.v3) && v2.equals(face.v1) && v3.equals(face.v2);

            return cond1 || cond2 || cond3;                 
        }

        //-------------------------------------GETS-------------------------------------//

        /**
     * Gets the face bound
     * 
     * @return face bound 
     */
     Bound m_bound;
     void UpdateBound()
     {
        m_bound = new Bound(v1.getPosition(), v2.getPosition(), v3.getPosition());
        UpdateNormal();
     }
        public Bound getBound() => m_bound;

        /**
     * Gets the face normal
     * 
     * @return face normal
     */
     Vector3d m_Normal;
     void UpdateNormal()
     {
            Point3d p1 = v1.getPosition();
            Point3d p2 = v2.getPosition();
            Point3d p3 = v3.getPosition();
            Vector3d xy, xz;

            xy = new Vector3d(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
            xz = new Vector3d(p3.x - p1.x, p3.y - p1.y, p3.z - p1.z);

            m_Normal = new Vector3d(0, 0, 0);
            m_Normal.cross(xy, xz);
            m_Normal.normalize();
     }
        public Vector3d getNormal() => m_Normal;

        /**
     * Gets the face status
     * 
     * @return face status - UNKNOWN, INSIDE, OUTSIDE, SAME OR OPPOSITE
     */
        public int getStatus()
        {
            return status;
        }

        /**
     * Gets the face area
     * 
     * @return face area
     */
        public double getArea()
        {
            //area = (a * c * sen(B))/2
            Point3d p1 = v1.getPosition();
            Point3d p2 = v2.getPosition();
            Point3d p3 = v3.getPosition();
            Vector3d xy = new Vector3d(p2.x - p1.x, p2.y - p1.y, p2.z - p1.z);
            Vector3d xz = new Vector3d(p3.x - p1.x, p3.y - p1.y, p3.z - p1.z);

            double a = p1.distance(p2);
            double c = p1.distance(p3);
            double B = xy.angle(xz);

            return (a * c * Math.Sin(B)) / 2d;
        }

        //-------------------------------------OTHERS-----------------------------------//

        /** Invert face direction (normal direction) */
        public void invert()
        {
            Vertex vertexTemp = v2;
            v2 = v1;
            v1 = vertexTemp;
            UpdateBound();
        }

        //------------------------------------CLASSIFIERS-------------------------------//

        /**
     * Classifies the face if one of its vertices are classified as INSIDE or OUTSIDE
     * 
     * @return true if the face could be classified, false otherwise 
     */
        public bool simpleClassify()
        {
            int status1 = v1.getStatus();
            int status2 = v2.getStatus();
            int status3 = v3.getStatus();

            if (status1 == Vertex.INSIDE || status1 == Vertex.OUTSIDE)
            {
                this.status = status1;
                return true; 
            }
            else if (status2 == Vertex.INSIDE || status2 == Vertex.OUTSIDE)
            {
                this.status = status2;
                return true;
            }
            else if (status3 == Vertex.INSIDE || status3 == Vertex.OUTSIDE)
            {
                this.status = status3;
                return true;
            }
            else
            {
                return false;
            }
        }

        /**
     * Classifies the face based on the ray trace technique
     * 
     * @param object object3d used to compute the face status 
     */
        public void rayTraceClassify(Object3D obj)
        {
            //creating a ray starting starting at the face baricenter going to the normal direction
            Point3d p0 = new Point3d();
            p0.x = (v1.x + v2.x + v3.x) / 3d;
            p0.y = (v1.y + v2.y + v3.y) / 3d;
            p0.z = (v1.z + v2.z + v3.z) / 3d;
            Line ray = new Line(getNormal(), p0);

            bool success;
            double dotProduct, distance; 
            Point3d intersectionPoint;
            Face closestFace = null;
            double closestDistance; 

            do
            {
                success = true;
                closestDistance = Double.MaxValue;
                //for each face from the other solid...
                for (int i = 0; i < obj.getNumFaces(); i++)
                {
                    Face face = obj.getFace(i);
                    dotProduct = face.getNormal().dot(ray.getDirection());
                    var a  = ray.computePlaneIntersection(face.getNormal(), face.v1.getPosition());

                    //if ray intersects the plane...  
                    if (a != null)
                    {
                        intersectionPoint = a.Value;
                        distance = ray.computePointToPointDistance(intersectionPoint);

                        //if ray lies in plane...
                        if (Math.Abs(distance) < TOL && Math.Abs(dotProduct) < TOL)
                        {
                            //disturb the ray in order to not lie into another plane 
                            ray.perturbDirection();
                            success = false;
                            break;
                        }

                        //if ray starts in plane...
                        if (Math.Abs(distance) < TOL && Math.Abs(dotProduct) > TOL)
                        {
                            //if ray intersects the face...
                            if (face.hasPoint(intersectionPoint))
                            {
                                //faces coincide
                                closestFace = face;
                                closestDistance = 0;
                                break;
                            }
                        }

                                    //if ray intersects plane... 
                                    else if (Math.Abs(dotProduct) > TOL && distance > TOL)
                        {
                            if (distance < closestDistance)
                            {
                                //if ray intersects the face;
                                if (face.hasPoint(intersectionPoint))
                                {
                                    //this face is the closest face untill now
                                    closestDistance = distance;
                                    closestFace = face;
                                }
                            }
                        }
                    }
                }
            } while(success == false);

            //none face found: outside face
            if (closestFace == null)
            {
                status = OUTSIDE;
            }
            //face found: test dot product
            else
            {
                dotProduct = closestFace.getNormal().dot(ray.getDirection());

                //distance = 0: coplanar faces
                if (Math.Abs(closestDistance) < TOL)
                {
                    if (dotProduct > TOL)
                    {
                        status = SAME;
                    }
                    else if (dotProduct < -TOL)
                    {
                        status = OPPOSITE;
                    }
                }

                    //dot product > 0 (same direction): inside face
                    else if (dotProduct > TOL)
                {
                    status = INSIDE;
                }

                    //dot product < 0 (opposite direction): outside face
                    else if (dotProduct < -TOL)
                {
                    status = OUTSIDE;
                }
            }
        }

        //------------------------------------PRIVATES----------------------------------//

        /**
     * Checks if the the face contains a point
     * 
     * @param point to be tested
     * @param true if the face contains the point, false otherwise 
     */ 
        private bool hasPoint(Point3d point)
        {
            int result1, result2, result3;
            bool hasUp, hasDown, hasOn;
            Vector3d normal = getNormal(); 

            //if x is constant...   
            if (Math.Abs(normal.x) > TOL)
            {
                //tests on the x plane
                result1 = linePositionInX(point, v1.getPosition(), v2.getPosition());
                result2 = linePositionInX(point, v2.getPosition(), v3.getPosition());
                result3 = linePositionInX(point, v3.getPosition(), v1.getPosition());
            }

            //if y is constant...
            else if (Math.Abs(normal.y) > TOL)
            {
                //tests on the y plane
                result1 = linePositionInY(point, v1.getPosition(), v2.getPosition());
                result2 = linePositionInY(point, v2.getPosition(), v3.getPosition());
                result3 = linePositionInY(point, v3.getPosition(), v1.getPosition());
            }
            else
            {
                //tests on the z plane
                result1 = linePositionInZ(point, v1.getPosition(), v2.getPosition());
                result2 = linePositionInZ(point, v2.getPosition(), v3.getPosition());
                result3 = linePositionInZ(point, v3.getPosition(), v1.getPosition());
            }

            //if the point is up and down two lines...      
            if (((result1 == UP) || (result2 == UP) || (result3 == UP)) && ((result1 == DOWN) || (result2 == DOWN) || (result3 == DOWN)))
            {
                return true;
            }
            //if the point is on of the lines...
            else if ((result1 == ON) || (result2 == ON) || (result3 == ON))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /** 
     * Gets the position of a point relative to a line in the x plane
     * 
     * @param point point to be tested
     * @param pointLine1 one of the line ends
     * @param pointLine2 one of the line ends
     * @return position of the point relative to the line - UP, DOWN, ON, NONE 
     */
        private static int linePositionInX(Point3d point, Point3d pointLine1, Point3d pointLine2)
        {
            double a, b, z;
            if ((Math.Abs(pointLine1.y - pointLine2.y) > TOL) && (((point.y >= pointLine1.y) && (point.y <= pointLine2.y)) || ((point.y <= pointLine1.y) && (point.y >= pointLine2.y))))
            {
                a = (pointLine2.z - pointLine1.z) / (pointLine2.y - pointLine1.y);
                b = pointLine1.z - a * pointLine1.y;
                z = a * point.y + b;
                if (z > point.z + TOL)
                {
                    return UP;          
                }
                else if (z < point.z - TOL)
                {
                    return DOWN;
                }
                else
                {
                    return ON;
                }
            }
            else
            {
                return NONE;
            }
        }

        /** 
     * Gets the position of a point relative to a line in the y plane
     * 
     * @param point point to be tested
     * @param pointLine1 one of the line ends
     * @param pointLine2 one of the line ends
     * @return position of the point relative to the line - UP, DOWN, ON, NONE 
     */

        private static int linePositionInY(Point3d point, Point3d pointLine1, Point3d pointLine2)
        {
            double a, b, z;
            if ((Math.Abs(pointLine1.x - pointLine2.x) > TOL) && (((point.x >= pointLine1.x) && (point.x <= pointLine2.x)) || ((point.x <= pointLine1.x) && (point.x >= pointLine2.x))))
            {
                a = (pointLine2.z - pointLine1.z) / (pointLine2.x - pointLine1.x);
                b = pointLine1.z - a * pointLine1.x;
                z = a * point.x + b;
                if (z > point.z + TOL)
                {
                    return UP;          
                }
                else if (z < point.z - TOL)
                {
                    return DOWN;
                }
                else
                {
                    return ON;
                }
            }
            else
            {
                return NONE;
            }
        }

        /** 
     * Gets the position of a point relative to a line in the z plane
     * 
     * @param point point to be tested
     * @param pointLine1 one of the line ends
     * @param pointLine2 one of the line ends
     * @return position of the point relative to the line - UP, DOWN, ON, NONE 
     */

        private static int linePositionInZ(Point3d point, Point3d pointLine1, Point3d pointLine2)
        {
            double a, b, y;
            if ((Math.Abs(pointLine1.x - pointLine2.x) > TOL) && (((point.x >= pointLine1.x) && (point.x <= pointLine2.x)) || ((point.x <= pointLine1.x) && (point.x >= pointLine2.x))))
            {
                a = (pointLine2.y - pointLine1.y) / (pointLine2.x - pointLine1.x);
                b = pointLine1.y - a * pointLine1.x;
                y = a * point.x + b;
                if (y > point.y + TOL)
                {
                    return UP;          
                }
                else if (y < point.y - TOL)
                {
                    return DOWN;
                }
                else
                {
                    return ON;
                }
            }
            else
            {
                return NONE;
            }
        }
    }
}
