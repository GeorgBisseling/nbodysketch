using System;
using System.Collections;
using System.Text;
using System.Diagnostics;

namespace NBodyLib
{
    public class EulerState : INBodyState
    {
        public EulerState(int n, double gravitationalConstant, double defaultMass)
        {
            currentTime = 0.0;
            N = n;
            G = gravitationalConstant;
            mass = new double[N];
            position = new Vector3[N];
            velocity = new Vector3[N];
            for (int i = 0; i < N; i++)
            {
                mass[i] = defaultMass;
                position[i] = new Vector3();
                velocity[i] = new Vector3();
            }
        }

        public EulerState(EulerState other)
        {
            currentTime = other.currentTime;
            N = other.N;
            G = other.G;
            mass = new double[N];
            position = new Vector3[N];
            velocity = new Vector3[N];
            for (int i = 0; i < N; i++)
            {
                mass[i] = other.mass[i];
                position[i] = new Vector3(other.position[i]);
                velocity[i] = new Vector3(other.velocity[i]);
            }
        }


        public double currentTime;

        public int N { get; set; }
        public double G { get; set; }

        public double[] mass;
        public double m(int i)
        {
            return mass[i];
        }

        public Vector3[] position;
        public Vector3 r(int i)
        {
            return position[i];
        }

        public Vector3[] velocity;
        public Vector3 v(int i)
        {
            return velocity[i];
        }
    }

    
    public class EulerIntegrator : INBodyIntegrator
    {

        protected EulerState state;

        public EulerIntegrator()
        {
            throw new NotImplementedException();
        }

        public EulerIntegrator(EulerState s)
        {
            state = s;
        }

        public double currentTMin { get { return state.currentTime; } }

        public double currentTMax { get { return state.currentTime; } }

        public INBodyState currentState(double t)
        {
            if (t != state.currentTime)
                throw new InvalidOperationException("Don't have state for this time.");
            return state;
        }

        public virtual void Progress(double deltaTime)
        {
            var N = state.N;

            var acceleration = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.position[i] += state.velocity[i] * deltaTime;
                state.velocity[i] += acceleration[i] * deltaTime;
            }

            state.currentTime += deltaTime;
        }


    }

    public class LeapFrogState : EulerState
    {
        internal double m_timeOfAccelerationCalculated;
        internal Vector3[] m_accelerations;

        public LeapFrogState(int n, double gravitationalConstant, double defaultMass)
            : base(n, gravitationalConstant, defaultMass)
        {

        }

        public LeapFrogState(EulerState state)
            : base(state)
        {
            m_timeOfAccelerationCalculated = -1.0;
            m_accelerations = null;
        }

        public LeapFrogState(LeapFrogState state)
            : base(state)
        {
            m_timeOfAccelerationCalculated = state.m_timeOfAccelerationCalculated;
            m_accelerations = state.m_accelerations;
        }
    }


    public class LeapFrogIntegrator : EulerIntegrator
    {
        public LeapFrogIntegrator()
        {
            throw new NotImplementedException();
        }

        public LeapFrogIntegrator(LeapFrogState s) 
            : base(s)
        {
        }

        public override void Progress(double deltaTime)
        {
            var N = state.N;
            var deltaTimeHalf = deltaTime * 0.5;
            var lfState = state as LeapFrogState;
            lfState = null;
            Vector3[] acceleration1;

            if (null != lfState && state.currentTime == lfState.m_timeOfAccelerationCalculated && lfState.m_accelerations != null)
                acceleration1 = lfState.m_accelerations;
            else
                acceleration1 = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.velocity[i] += acceleration1[i] * deltaTimeHalf;
            }

            for (int i = 0; i < N; i++)
            {
                state.position[i] += state.velocity[i] * deltaTime;
            }

            var acceleration2 = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.velocity[i] += acceleration2[i] * deltaTimeHalf;
            }

            state.currentTime += deltaTime;

            if (null != lfState)
            {
                lfState.m_timeOfAccelerationCalculated = state.currentTime;
                lfState.m_accelerations = acceleration2;
            }
        }

    }

}