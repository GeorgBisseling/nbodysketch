using System;
using System.Collections;
using System.Text;

namespace NBodyLib
{

    public interface INBodyState
    {
        int N { get; }
        double G { get; }

        double m(int i);

        Vector3 r(int i);
        Vector3 v(int i);
    }

    public static class INBodyStateExtensions
    {
        static public Vector3 A_onFirstFromSecond(this INBodyState s, int first, int second)
        {
            var result = new Vector3();

            var First = s.r(first);
            var Second = s.r(second);

            var RDiff = Second - First;

            double RDiffNorm = RDiff.euklid_Norm();

            double factor = s.m(second) * s.G / Math.Pow(RDiffNorm, 3.0);

            Vector3 force = factor * RDiff;

            return force;
        }

        static public Vector3 A_onFirstFromAll(this INBodyState s, int first)
        {
            Vector3 Force = new Vector3();

            var N = s.N;

            for (int i = 0; i < N; i++) Force += s.A_onFirstFromSecond(first, i);

            return Force;
        }

        static public Vector3[,] ComputeAccelerationMatrix(this INBodyState s)
        {
            var N = s.N;
            var acc = new Vector3[N, N];

            // compute accelerations
            for (int i = 0; i < N; i++)
                for (int j = i; j < N; j++)
                    if (j != i)
                    {
                        var a = s.A_onFirstFromSecond(i, j);
                        acc[i, j] = a;
                        acc[j, i] = -a;
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

            for (int i = 0; i < N; i++)
                accVector[i] = new Vector3();

            // compute accelerations
            for (int i = 0; i < N; i++)
                for (int j = i; j < N; j++)
                    if (j != i)
                    {
                        var a = s.A_onFirstFromSecond(i, j);
                        accVector[i] += a;
                        accVector[j] -= a;
                    }
            
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

            double Mass = 0.0; 
            for (int i = 0; i < N; i++) Mass += state.m(i);
            Vector3 cog = new Vector3();
            for (int i = 0; i < N; i++) cog += state.m(i) * state.r(i);
            cog /= Mass;

            for (int i = 0; i < N; i++)
            {
                Vector3 ri = state.r(i);
                Vector3 ri_relative_to_cog = state.r(i) - cog;
                double R = ri_relative_to_cog.euklid_Norm();
                epot += state.m(i) * (Mass - state.m(i)) / R;
            }
            epot *= -1.0 * state.G;

            return epot;


            //var leftMasses = new double[N];

            //leftMasses[0] = 0.0;
            //var leftCenterOfGravity = new Vector3[N];
            //leftCenterOfGravity[0] = new Vector3();
            //for (int i = 1; i < N; i++)
            //{
            //    leftMasses[i] = leftMasses[i - 1] + state.m(i-1);
            //    leftCenterOfGravity[i] = leftCenterOfGravity[i - 1] + (state.r(i-1) * state.m(i-1));
            //}

            //for (int i = 0; i < N; i++)
            //{
            //    leftCenterOfGravity[i] /= leftMasses[i];
            //}


            //for (int i = 1; i < N; i++)
            //{
            //    Vector3 ri = state.r(i);
            //    Vector3 ri_relative_to_cog = state.r(i) - leftCenterOfGravity[i];
            //    double R = ri_relative_to_cog.euklid_Norm();
            //    epot += state.m(i) * leftMasses[i] / R;
            //}
            //epot *= -1.0 * state.G;

            //return epot;
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

        void Progress(double deltaTime);
    }
}