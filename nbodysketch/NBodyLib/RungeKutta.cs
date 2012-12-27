using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

using VectorLib;

namespace NBodyLib
{
    public class RungeKuttaIntegrator : INBodyIntegrator
    {
        private LeapFrogState state;

        public enum Flavor { rk2, rk4, yo4, yo6, yo8 }

        public readonly Flavor flavor;

        public RungeKuttaIntegrator()
        {
            throw new NotImplementedException();
        }

        public RungeKuttaIntegrator(EulerState s, Flavor f)
        {
            state = new LeapFrogState(s);
            flavor = f;
        }

        public double currentTMin { get { return state.currentTime; } }

        public double currentTMax { get { return state.currentTime; } }

        public INBodyState currentState(double t)
        {
            if (t != state.currentTime)
                throw new InvalidOperationException("Don't have state for this time.");
            return state;
        }

        public virtual void Progress(double dt)
        {
            switch (flavor)
            {
                case Flavor.rk2: Progress_rk2(state, dt); break;
                case Flavor.rk4: Progress_rk4(state, dt); break;
                case Flavor.yo4: Progress_yo4(state, dt); break;
                case Flavor.yo6: Progress_yo6(state, dt); break;
                case Flavor.yo8: Progress_yo8(state, dt); break;
                default:
                    throw new NotImplementedException();
            }
        }

        public static void Progress_rk2(LeapFrogState state, double dt)
        {
            var N = state.N;
            var old_pos = state.position.Select(p => new Vector3(p)).ToList();

            var a0 = state.EulerState_ComputeAccelerationVectorDirect();

            var half_vel = new Vector3[N];

            Parallel.For(0, N, i =>
            //for (int i = 0; i < N; i++)
            {
                half_vel[i] = state.velocity[i] + a0[i] * 0.5 * dt;
                state.position[i] += state.velocity[i] * 0.5 * dt;
            });

            var a1 = state.EulerState_ComputeAccelerationVectorDirect();

            Parallel.For(0, N, i =>
            //for (int i = 0; i < N; i++)
            {
                state.velocity[i] += a1[i] * dt;
                state.position[i] = old_pos[i] + half_vel[i] * dt;
            });

            state.currentTime += dt;
        }


        public static void Progress_rk4(LeapFrogState state, double dt)
        {
            var po = new ParallelOptions { MaxDegreeOfParallelism = System.Environment.ProcessorCount };

            var N = state.N;
            var old_pos = state.position.AsParallel().Select(oldpos => new Vector3(oldpos)).ToList();

            var a0 = state.EulerState_ComputeAccelerationVectorDirect();

            Parallel.For(0, N, po, i =>
            //for (int i = 0; i < N; i++)
            {
                state.position[i] = old_pos[i]
                    + state.velocity[i] * 0.5 * dt
                    + a0[i] * 0.125 * dt * dt;
            });

            var a1 = state.EulerState_ComputeAccelerationVectorDirect();

            Parallel.For(0, N, po, i =>
            //for (int i = 0; i < N; i++)
            {
                state.position[i] = old_pos[i]
                    + state.velocity[i] * dt
                    + a0[i] * 0.5 * dt * dt;
            });

            var a2 = state.EulerState_ComputeAccelerationVectorDirect();

            Parallel.For(0, N, po, i =>
            //for (int i = 0; i < N; i++)
            {
                state.position[i] = old_pos[i]
                    + state.velocity[i] * dt
                    + (a0[i] + a1[i] * 2.0) * (1.0 / 6.0) * dt * dt;
                state.velocity[i] += (a0[i] + a1[i] * 4.0 + a2[i]) * (1.0 / 6.0) * dt;
            });

            state.currentTime += dt;
        }

        public static void Progress_yo4(LeapFrogState state, double dt)
        {

            var d = new double[] { 1.351207191959657, -1.702414383919315 };

            LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[0]);
            LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[1]);
            LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[0]);
        }

        public static void Progress_yo6(LeapFrogState state, double dt)
        {
            var d = new double[] { 0.784513610477560e0, 0.235573213359357e0, -1.17767998417887e0, 1.31518632068391e0 };

            var index = new int[] { 0, 1, 2 };

            foreach (var i in index) LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[i]);
            LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[3]);
            foreach (var i in index) LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[i]);
        }

        public static void Progress_yo8(LeapFrogState state, double dt)
        {
            var d = new double[] { 0.104242620869991e1, 0.182020630970714e1, 0.157739928123617e0, 0.244002732616735e1, -0.716989419708120e-2, -0.244699182370524e1, -0.161582374150097e1, -0.17808286265894516e1 };

            var index = new int[] { 0, 1, 2, 3, 4, 5, 6 };

            foreach (var i in index) LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[i]);
            LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[7]);
            foreach (var i in index) LeapFrogIntegrator.Progress_LeapFrog(state, dt * d[i]);
        }

    
    }
}
