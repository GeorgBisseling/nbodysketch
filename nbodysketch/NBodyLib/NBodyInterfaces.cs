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
          
            var r1 = s.r[first];
            var r2 = s.r[second];

            var Rdiff = r2 - r1;

            double R2 = Rdiff * Rdiff;
            double R = Math.Sqrt(R2);
            var eps = s.eps;
            double eps2 = eps * eps;

            double factor = s.m[second] * s.G / (R * (R2 + eps*eps) );

            Rdiff *= factor;
            return Rdiff;
        }

        static public Vector3 A_onFirstFromAll(this INBodyState s, int first)
        {
            var N = s.N;

            var tmp = new Vector3();

            Vector3 acc = s.A_onFirstFromSecond(first, 0);

            for (int i = 1; i < N; i++) acc += s.A_onFirstFromSecond(first, i);

            return acc;
        }

        public static Vector3[] ComputeAccelerationVectorDirect(this INBodyState s)
        {
            var N = s.N;
            var accVector = new Vector3[N];

            // compute accelerations
            Parallel.For(0, N, i =>
            {
                accVector[i] = new Vector3();
                var avi = accVector[i];
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        var a = s.A_onFirstFromSecond(i, j);
                        avi.c0 += a.c0;
                        avi.c1 += a.c1;
                        avi.c2 += a.c2;
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
                var RDiff = new Vector3();
                var ri = state.r[i];
                var mi = state.m[i];
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        var rj = state.r[j];
                        //var RDiff = (state.r[j] - ri);
                        RDiff.c0 = rj.c0 - ri.c0;
                        RDiff.c1 = rj.c1 - ri.c1;
                        RDiff.c2 = rj.c2 - ri.c2;
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