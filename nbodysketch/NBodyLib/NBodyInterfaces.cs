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

        IList<double> m { get; }
        IList<Vector3> r { get; }
        IList<Vector3> v { get; }
    }

    public static class INBodyStateExtensions
    {
        static public Vector3 A_onFirstFromSecond(this INBodyState s, int first, int second)
        {
            var RDiff = s.r[second] - s.r[first];

            double R = RDiff.euklid_Norm();

            double factor = (s.m[second] * s.G / (R * R * R));

            RDiff.c[0] *= factor;
            RDiff.c[1] *= factor;
            RDiff.c[2] *= factor;

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
                        var c = s.A_onFirstFromSecond(i, j).c;
                        var lc = accVector[i].c;
                        lc[0] += c[0];
                        lc[1] += c[1];
                        lc[2] += c[2];
                    }
            });
            
            return accVector;
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

            Parallel.For(0, N, i =>
            // for (int i = 0; i < N; i++)
            {
                var ri = state.r[i];
                var mi = state.m[i];
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        double R = (state.r[j] - ri).euklid_Norm();
                        epot += mi * state.m[j] / R;
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