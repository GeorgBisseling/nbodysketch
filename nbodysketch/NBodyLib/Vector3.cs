using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace NBodyLib
{
    [TestClass]
    public class Vector3
    {

        public double[] c;

        public Vector3()
        {
            c = new double[3];
        }

        public Vector3(Vector3 v)
        {
            c = new double[3];
            v.c.CopyTo(c, 0);
        }

        public Vector3(params double[] v)
        {
            c = new double[3];
            v.CopyTo(c, 0);
        }

        public double this [int i] {get {return c[i];} set{c[i] = value;}}


        public static Vector3 operator + (Vector3 l, Vector3 r)
        {
            return new Vector3( l.c[0] + r.c[0], l.c[1] + r.c[1], l.c[2] + r.c[2]);
        }

        public static Vector3 operator - (Vector3 l, Vector3 r)
        {
            return new Vector3(l.c[0] - r.c[0], l.c[1] - r.c[1], l.c[2] - r.c[2]);
        }

        public static Vector3 operator -(Vector3 r)
        {
            return new Vector3( - r.c[0], - r.c[1], - r.c[2]);
        }

        public static double operator *(Vector3 l, Vector3 r)
        {
            return l.c[0] * r.c[0] + l.c[1] * r.c[1] + l.c[2] * r.c[2];
        }

        public static Vector3 operator *(double l, Vector3 r)
        {
            return new Vector3(r.c[0] * l, r.c[1] * l, r.c[2] * l);
        }

        public static Vector3 operator *(Vector3 l, double r )
        {
            return new Vector3(l.c[0] * r, l.c[1] * r, l.c[2] * r);
        }

        public static Vector3 operator / (Vector3 l, double r)
        {
            return l * (1.0 / r);
        }

        public double euklid_Norm()
        {
            double norm = 0.0;
            for (int i = 0; i < c.Length; i++)
                norm += c[i] * c[i];
            return Math.Sqrt(norm);
        }

        public Vector3 unit()
        {
            double n = euklid_Norm();
            return new Vector3(c[0] / n, c[1] / n, c[2] / n);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append(":");
            foreach (var d in c)
            {
                sb.AppendFormat(" {0}", d);
            }
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            if (null == obj) return false;
            if (GetType() != obj.GetType()) return false;
            Vector3 o = obj as Vector3;
            return c[0] == o.c[0] && c[1] == o.c[1] && c[2] == o.c[2];
        }

        public override int GetHashCode()
        {
            var hash = base.GetHashCode();
            if (null != c)
                hash ^= c.GetHashCode();
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
