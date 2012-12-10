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


        double m(int i);

        Vector3 r(int i);
        Vector3 v(int i);

        IList<double> mList { get; }
        IList<Vector3> rList { get; }
        IList<Vector3> vList { get; }
    }

    public static class INBodyStateExtensions
    {
        static public Vector3 A_onFirstFromSecond(this INBodyState s, int first, int second)
        {
            var RDiff = s.rList[second] - s.rList[first];

            double RDiffNorm = RDiff.euklid_Norm();

            double factor = s.m(second) * s.G / Math.Pow(RDiffNorm, 3.0);

            Vector3 acc = factor * RDiff;

            return acc;
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
            var zero = new Vector3();

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
                        acc[i, j] = zero;
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

            var po = new ParallelOptions {MaxDegreeOfParallelism = System.Environment.ProcessorCount * 2 };

            // compute accelerations
            //for (int i = 0; i < N; i++)
            Parallel.For(0, N, po, i =>
            {
                accVector[i] = new Vector3();
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        var a = s.A_onFirstFromSecond(i, j);
                        accVector[i] += a;
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
            for (int i = 0; i < N; i++)
            {
                var v = state.v(i);
                double m = state.m(i);
                ekin += m * (v * v);
            }
            ekin *= 0.5;
            return ekin;
        }

        static public double Epot(this INBodyState state)
        {
            double epot = 0.0;

            var N = state.N;

            for (int i = 0; i < N; i++)
            {

                for(int j=0; j<N; j++)
                    if (j != i)
                    {
                        double R = (state.r(j) - state.r(i)).euklid_Norm();
                        epot += state.m(i) * state.m(j) / R;
                    }
            }

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