﻿using SEWorldGenPlugin.ObjectBuilders;
using System;
using VRage.Utils;
using VRageMath;

namespace SEWorldGenPlugin.Generator.Asteroids
{
    public struct AsteroidRingShape
    {
        public Vector3D center;
        public double radius;
        public int width;
        public int height;
        public Vector3D rotation;

        public static AsteroidRingShape CreateFromRingItem(MyPlanetRingItem ring)
        {
            AsteroidRingShape shape = new AsteroidRingShape();
            shape.center = ring.Center;
            shape.radius = ring.Radius;
            shape.width = ring.Width;
            shape.height = ring.Height;
            shape.rotation = new Vector3D(1, 0, 0);

            double angle = 2 * Math.PI / 360 * (ring.AngleDegrees);
            shape.rotation.X = Math.Cos(angle);
            shape.rotation.Y = Math.Sin(angle);

            return shape;
        }

        public ContainmentType Contains(Vector3D point)
        {
            Vector3D relPosition = Vector3D.Subtract(point, center);
            Vector3D planeNormal = new Vector3D(-rotation.Y, rotation.X, 0);
            Vector3D horVector = Vector3D.ProjectOnPlane(ref relPosition, ref planeNormal);
            Vector3D vertVector = Vector3D.Subtract(relPosition, horVector);

            double length = horVector.Length();

            if (length > radius + width || length < radius) return ContainmentType.Disjoint;

            double vertLength = vertVector.Length();
            double ringHeight = GetHeightAtRad(length);

            if (vertLength <= ringHeight) return ContainmentType.Contains;

            return ContainmentType.Disjoint;
        }

        public Vector3D LocationInRing(int angle)
        {
            double rad = radius + width / 2f;
            return center + (new Vector3D(rotation.X * rad * Math.Cos(angle), rotation.Y * rad * Math.Cos(angle), Math.Sin(angle) * rad));
        }

        private double GetHeightAtRad(double rad)
        {
            if (rad < radius || rad > radius + width) throw new ArgumentOutOfRangeException("The radius " + rad + " has to be less than " + (radius + width) + " and larger than " + radius);
            return Math.Sin((rad - radius) * Math.PI / width) * Math.Sin((rad - radius) * Math.PI / width) * 0.5 * height + 100;//Plus 100 to make asteroids on edges possible
        }
    }
}
