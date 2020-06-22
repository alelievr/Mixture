using System;
using System.Collections;
using System.Collections.Generic;

namespace Net3dBool
{

    public struct Color3f
    {

        public double r;
        public double g;
        public double b;

        public Color3f(double r, double g, double b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public Color3f Clone()
        {
            return new Color3f(r, g, b);
        }
    }

    public class Shape3D
    {

    }

    public struct Point3d
    {
        public double x;
        public double y;
        public double z;

        public Point3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public double distance(Point3d p1)
        {
            double dx, dy, dz;

            dx = this.x - p1.x;
            dy = this.y - p1.y;
            dz = this.z - p1.z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        public Point3d Clone()
        {
            return new Point3d(x, y, z);
        }

    }

    public struct Vector3d
    {
        public double x;
        public double y;
        public double z;

        public Vector3d Clone()
        {
            return new Vector3d(x, y, z);
        }

        public double length()
        {
            return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }

        public double angle(Vector3d v1)
        { 
            double vDot = this.dot(v1) / (this.length() * v1.length());
            if (vDot < -1.0)
                vDot = -1.0;
            if (vDot > 1.0)
                vDot = 1.0;
            return((double)(Math.Acos(vDot)));
        }

        public double dot(Vector3d v1)
        {
            return (this.x * v1.x + this.y * v1.y + this.z * v1.z);
        }

        public Vector3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public void cross(Vector3d v1, Vector3d v2)
        { 
            double x, y;

            x = v1.y * v2.z - v1.z * v2.y;
            y = v2.x * v1.z - v2.z * v1.x;
            this.z = v1.x * v2.y - v1.y * v2.x;
            this.x = x;
            this.y = y;
        }

        public void normalize()
        {
            double norm;

            norm = 1.0 / Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
            this.x *= norm;
            this.y *= norm;
            this.z *= norm;
        }

    }

    public static class Helper
    {
        public static void fill<T>(this T[] self, T value)
        {
            for (var i = 0; i < self.Length; i++)
            {
                self[i] = value;
            }
        }

        //        public static double dot(this Vector3d self, Vector3d v)
        //        {
        //            double dot = self.X * v.X + self.Y * v.Y + self.Z * v.Z;
        //            return dot;
        //        }
        //
        //        public static Vector3d cross(this Vector3d self, Vector3d v)
        //        {
        //            double crossX = self.Y * v.Z - v.Y * self.Z;
        //            double crossY = self.Z * v.X - v.Z * self.X;
        //            double crossZ = self.X * v.Y - v.X * self.Y;
        //            return new Vector3d(crossX, crossY, crossZ);
        //        }
        //
        //        public static double distance(this Vector3d v1, Vector3d v2)
        //        {
        //            double dx = v1.X - v2.X;
        //            double dy = v1.Y - v2.Y;
        //            double dz = v1.Z - v2.Z;
        //            return (double)Math.Sqrt(dx * dx + dy * dy + dz * dz);
        //        }

        private static Random rnd = new Random();

        public static double random()
        {
            return rnd.NextDouble();
        }
    }
}
