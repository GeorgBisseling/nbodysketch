using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace NBodyLib
{
    public class RungeKuttaIntegrator : INBodyIntegrator
    {
        private EulerState state;

        public enum Flavor { rk2, rk4 }

        public readonly Flavor flavor;

        public RungeKuttaIntegrator()
        {
            throw new NotImplementedException();
        }

        public RungeKuttaIntegrator(EulerState s, Flavor f)
        {
            state = s;
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
                case Flavor.rk2: Progress_rk2(dt); break;
                case Flavor.rk4: Progress_rk4(dt); break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void Progress_rk2(double dt)
        {
            var N = state.N;
            var old_pos = state.position.Select(p => new Vector3(p)).ToList();

            var a0 = state.ComputeAccelerationVectorDirect();

            var half_vel = new List<Vector3>(N);

            for (int i = 0; i < N; i++) 
                half_vel[i] = state.velocity[i] + a0[i] * 0.5 * dt;

            for (int i = 0; i < N; i++)
            {
                state.position[i] += state.velocity[i] * 0.5 * dt;
            }

            var a1 = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.velocity[i] += a1[i] * dt;
                state.position[i] = old_pos[i] + half_vel[i] * dt;
            }

            state.currentTime += dt;
        }


        private void Progress_rk4(double dt)
        {
            var N = state.N;
            var old_pos = state.position.Select(oldpos => new Vector3(oldpos)).ToList();

            var a0 = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.position[i] = old_pos[i] 
                    + state.velocity[i] * 0.5 * dt 
                    + a0[i] * 0.125 * dt * dt;
            }

           var a1 = state.ComputeAccelerationVectorDirect();

           for (int i = 0; i < N; i++)
           {
               state.position[i] = old_pos[i]
                   + state.velocity[i] * dt
                   + a0[i] * 0.5 * dt * dt;
           }

            var a2 = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.position[i] = old_pos[i]
                    + state.velocity[i] * dt
                    + (a0[i]+a1[i]*2.0) * (1.0/6.0) * dt * dt;
                state.velocity[i] += (a0[i] + a1[i] * 4.0 + a2[i]) * (1.0 / 6.0) * dt;
            }

            state.currentTime += dt;
        }

    }
}
