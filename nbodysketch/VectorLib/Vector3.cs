using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace VectorLib
{
    [TestClass]
    public class Vector3
    {
        public double c0, c1, c2;

        public Vector3()
        {
        }

        public Vector3(Vector3 v)
        {
            c0 = v.c0; c1 = v.c1; c2 = v.c2;
        }

        public Vector3(double x, double y = 0.0, double z = 0.0)
        {
            c0 = x; c1 = y; c2 = z;
        }

        public double this [int i] 
        {
            get {
                switch (i)
                {
                    case 0: return c0;
                    case 1: return c1;
                    case 2: return c2;
                }
                throw new IndexOutOfRangeException();
            } 
            set {
                switch (i)
                {
                    case 0: c0 = value; return;
                    case 1: c1 = value; return;
                    case 2: c2 = value; return;
                }
                throw new IndexOutOfRangeException();
            }
        }

        public static Vector3 operator + (Vector3 l, Vector3 r)
        {
            var v = new Vector3();
            v.c0 = l.c0 + r.c0;
            v.c1 = l.c1 + r.c1;
            v.c2 = l.c2 + r.c2;
            return v;
        }

        public static Vector3 operator - (Vector3 l, Vector3 r)
        {
            var v = new Vector3();
            v.c0 = l.c0 - r.c0;
            v.c1 = l.c1 - r.c1;
            v.c2 = l.c2 - r.c2;
            return v;
        }

        public static Vector3 operator -(Vector3 r)
        {
            var v = new Vector3();
            v.c0 =  - r.c0;
            v.c1 =  - r.c1;
            v.c2 =  - r.c2;
            return v;
        }

        public static double operator *(Vector3 l, Vector3 r)
        {
            return l.c0 * r.c0 + l.c1 * r.c1 + l.c2 * r.c2;
        }

        public static Vector3 operator *(double l, Vector3 r)
        {
            var v = new Vector3();
            v.c0 = l * r.c0;
            v.c1 = l * r.c1;
            v.c2 = l * r.c2;
            return v;
        }

        public static Vector3 operator *(Vector3 l, double r )
        {
            var v = new Vector3();
            v.c0 = l.c0 * r;
            v.c1 = l.c1 * r;
            v.c2 = l.c2 * r;
            return v;
        }

        public static Vector3 operator / (Vector3 l, double r)
        {
            var v = new Vector3();
            v.c0 = l.c0 / r;
            v.c1 = l.c1 / r;
            v.c2 = l.c2 / r;
            return v;
        }

        public double euklid_Norm()
        {
            return Math.Sqrt(c0*c0+c1*c1+c2*c2);
        }

        public Vector3 unit()
        {
            double n = euklid_Norm();
            var v = new Vector3();
            v.c0 = c0 / n;
            v.c1 = c1 / n;
            v.c2 = c2 / n;
            return v;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", c0, c1, c2);
        }

        public override bool Equals(object obj)
        {
            if (null == obj) return false;
            if (GetType() != obj.GetType()) return false;
            Vector3 o = obj as Vector3;
            return c0 == o.c0 && c1 == o.c1 && c2 == o.c2;
        }

        public override int GetHashCode()
        {
            var hash = base.GetHashCode();
            hash ^= c0.GetHashCode();
            hash ^= c1.GetHashCode();
            hash ^= c2.GetHashCode();
            return hash;
        }

        public static Vector3[] CopyArray(Vector3[] source)
        {
            if (null == source)
                return null;

            var L = source.Length;
            var dest = new Vector3[L];
            for (int i = 0; i < L; i++)
                dest[i] = new Vector3(source[i]);
            return dest;
        }


        [TestMethod]
        public void Test()
        {
            var vx = new Vector3(1, 0, 0);
            var vy = new Vector3(0, 1, 0);
            var vz = new Vector3(0, 0, 1);
            var v0 = new Vector3();
            var v1 = new Vector3(1, 1, 1);

            Debug.Assert(v1.Equals(vx + vy + vz));

            Debug.Assert(vx.Equals(-vx * -1.0));
            Debug.Assert(vy.Equals(-vy * -1.0));
            Debug.Assert(vz.Equals(-vz * -1.0));

            Debug.Assert(vx.Equals(-vx / -1.0));
            Debug.Assert(vy.Equals(-vy / -1.0));
            Debug.Assert(vz.Equals(-vz / -1.0));

            Debug.Assert(v0.Equals(v1 - v1));
            Debug.Assert(v0.Equals(vx - vx));
            Debug.Assert(v0.Equals(vy - vy));
            Debug.Assert(v0.Equals(vz - vz));

            Debug.Assert(0.0 == v0 * v1);
            Debug.Assert(0.0 == v0 * vx);
            Debug.Assert(0.0 == v0 * vy);
            Debug.Assert(0.0 == v0 * vz);

            Debug.Assert(3.0 == v1 * v1);

            Debug.Assert(0.0 == v0.euklid_Norm());
            Debug.Assert(1.0 == vx.euklid_Norm());
            Debug.Assert(1.0 == vy.euklid_Norm());
            Debug.Assert(1.0 == vz.euklid_Norm());
            Debug.Assert(Math.Sqrt(3.0) == v1.euklid_Norm());
        }
    }
}
