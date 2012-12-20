using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
 
namespace NBodyLib
{

    public interface INBodyState
    {
        double t { get; }
        int N { get; }
        double G { get; }
        double eps { get; } // softening length

        IList<double> m { get; }
        IList<Vector3> r { get; }
        IList<Vector3> v { get; }
    }

    public static class INBodyStateExtensions
    {
        static public Vector3 A_onFirstFromSecond(this INBodyState s, int first, int second)
        {
            var RDiff = s.r[second] - s.r[first];

            double R2 = RDiff * RDiff;
            double R = Math.Sqrt(R2);
            double eps2 = s.eps * s.eps;

            double factor = s.m[second] * s.G / (R * (R2 + s.eps*s.eps) );

            RDiff.c0 *= factor;
            RDiff.c1 *= factor;
            RDiff.c2 *= factor;

            return RDiff;
        }

        static public Vector3 A_onFirstFromAll(this INBodyState s, int first)
        {
            var N = s.N;

            Vector3 acc = s.A_onFirstFromSecond(first, 0);

            for (int i = 1; i < N; i++) acc += s.A_onFirstFromSecond(first, i);

            return acc;
        }

        static public Vector3[,] ComputeAccelerationMatrix(this INBodyState s)
        {
            var N = s.N;
            var acc = new Vector3[N, N];

            // compute accelerations
            for (int i = 0; i < N; i++)
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        var a = s.A_onFirstFromSecond(i, j);
                        acc[i, j] = a;
                    }
                    else
                    {
                        acc[i, j] = new Vector3();
                    }
            return acc;
        }

        public static Vector3[] ComputeAccelerationVectorFromMatrix(this INBodyState s, Vector3[,] accMatrix)
        {
            var N = s.N;
            var accVector = new Vector3[N];
            for (int i = 0; i < N; i++)
            {
                Vector3 sumV = new Vector3();
                for (int j = 0; j < N; j++)
                    sumV += accMatrix[i, j];
                accVector[i] = sumV;
            }
            return accVector;
        }

        public static Vector3[] ComputeAccelerationVectorDirect(this INBodyState s)
        {
            var N = s.N;
            var accVector = new Vector3[N];

            // compute accelerations
            Parallel.For(0, N, i =>
            {
                accVector[i] = new Vector3();
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        var a = s.A_onFirstFromSecond(i, j);
                        accVector[i].c0 += a.c0;
                        accVector[i].c1 += a.c1;
                        accVector[i].c2 += a.c2;
                    }
            });
            
            return accVector;
        }

        public static void Serialize(this INBodyState s, double? ekin, double? epot, StringBuilder sb)
        {
            sb.AppendLine(s.t.ToString() + " # t");
            sb.AppendLine(s.N.ToString() + " # N");
            sb.AppendLine(s.G.ToString() + " # G");
            sb.AppendLine(s.eps.ToString() + " # eps");
            ekin = ekin ?? s.Ekin();
            epot = epot ?? s.Epot();
            sb.AppendLine(ekin.ToString() + " # Ekin");
            sb.AppendLine(epot.ToString() + " # Epot");
            foreach (var m in s.m)
                sb.AppendLine(m.ToString());
            foreach (var r in s.r)
                sb.AppendLine(r.ToString());
            foreach (var v in s.v)
                sb.AppendLine(v.ToString());
        }



        static public double Etot(this INBodyState state)
        {
            double ekin = Ekin(state);
            double epot = Epot(state);
            return ekin + epot;
        }

        static public double Ekin(this INBodyState state)
        {
            double ekin = 0.0;
            var N = state.N;
            Parallel.For(0, N, i =>
            // for (int i = 0; i < N; i++)
            {
                var v = state.v[i];
                double m = state.m[i];
                ekin += m * (v * v);
            });
            ekin *= 0.5;
            return ekin;
        }

        static public double Epot(this INBodyState state)
        {
            double epot = 0.0;

            var N = state.N;
            var eps2 = state.eps * state.eps;

            Parallel.For(0, N, i =>
            // for (int i = 0; i < N; i++)
            {
                var ri = state.r[i];
                var mi = state.m[i];
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        var RDiff = (state.r[j] - ri);
                        var R2 = RDiff * RDiff;
                        epot += mi * state.m[j] / Math.Sqrt(R2 + eps2);
                    }
            });

            epot *= -state.G;

            return epot;
        }
    }

    public interface INBodyIntegrator
    {
        /// <summary>
        /// currentState should be available for at least [currentTMin;currentTMax]
        /// </summary>
        double currentTMin { get; }
        double currentTMax { get; }

        INBodyState currentState(double t);

        void Progress(double dt);
    }
}