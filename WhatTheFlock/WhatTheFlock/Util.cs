using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.DesignScript.Runtime;
using SharpDX;
using Point = Autodesk.DesignScript.Geometry.Point;
using Vector = Autodesk.DesignScript.Geometry.Vector;
using Math = System.Math;


namespace WhatTheFlock
{
    [IsVisibleInDynamoLibrary(false)]
    public static class Util
    {
        private const float by255 = 1f / 255f;

        public static Vector3 ToVector3(this Triple triple) => new Vector3(triple.X, triple.Z, -triple.Y);

        public static Point ToPoint(this Triple t) => Point.ByCoordinates(t.X, t.Y, t.Z);
        public static Vector ToVector(this Triple t) => Vector.ByCoordinates(t.X, t.Y, t.Z);

        public static Triple ToTriple(this Point point) => new Triple(point.X, point.Y, point.Z);
        public static Triple ToTriple(this Vector vector) => new Triple(vector.X, vector.Y, vector.Z);

        public static Vector ZeroVector => Vector.ByCoordinates(0.0, 0.0, 0.0);
        public static Point Duplicate(this Point point) => Point.ByCoordinates(point.X, point.Y, point.Z);
        public static Vector Duplicate(this Vector vector) => Vector.ByCoordinates(vector.X, vector.Y, vector.Z);

        public static bool IsAlmostZero(this float number, float tolerance = 1E-10f) => -tolerance < number && number < tolerance;
        public static bool IsAlmostZero(this double number, double tolerance = 1E-10) => -tolerance < number && number < tolerance;

        private static Random random = new Random();

        public static List<Triple> ToTriples(this IEnumerable<Point> points)
        {
            List<Triple> triples = new List<Triple>(points.Count());
            foreach (Point point in points)
                triples.Add(new Triple(point.X, point.Y, point.Z));
            return triples;
        }

        public static List<Triple> ToTriples(this IEnumerable<Vector> vectors)
        {
            List<Triple> triples = new List<Triple>(vectors.Count());
            foreach (Vector vector in vectors)
                triples.Add(new Triple(vector.X, vector.Y, vector.Z));
            return triples;
        }

        public static T[] InitializeArray<T>(int length, T value)
        {
            T[] array = new T[length];
            for (int i = 0; i < length; i++) array[i] = value;
            return array;
        }

        public static void FillArray<T>(this T[] array, T value)
        {
            for (int i = 0; i < array.Length; i++) array[i] = value;
        }


        public static Triple GetRandomPoint(float minX, float maxX, float minY, float maxY, float minZ, float maxZ)
        {
            return new Triple(
                random.NextFloat(minX, maxX),
                random.NextFloat(minY, maxY),
                random.NextFloat(minZ, maxZ));
        }

        public static Triple GetRandomUnitVector()
        {
            double phi = random.NextFloat(0f, 2f * (float)Math.PI);
            double theta = Math.Acos(random.NextFloat(-1f, 1f));
            return new Triple(
                (float)(Math.Sin(theta) * Math.Cos(phi)),
                (float)(Math.Sin(theta) * Math.Sin(phi)),
                (float)(Math.Cos(theta)));
        }


        public static Triple GetRandomUnitVectorXZ(float y = 0f)
        {
            double angle = random.NextFloat(0f, 2f * (float)Math.PI);
            return new Triple((float)Math.Cos(angle), 0f, (float)Math.Sin(angle));
        }


        private const float toRadian = (float)Math.PI / 180f;
        private const float toDegree = 180f / (float)Math.PI;

        public static float ToRadian(this float Degree) => Degree * toRadian;
        public static float ToDegree(this float Radian) => Radian * toDegree;
    }
}
