using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Xml.Serialization;

namespace NBodyLib
{
    public class EulerState : INBodyState
    {
        public EulerState()
        {
        }
        
        public EulerState(int n, double gravitationalConstant, double defaultMass)
        {
            currentTime = 0.0;
            N = n;
            G = gravitationalConstant;
            mass = new List<double>(N);
            position = new List<Vector3>(N);
            velocity = new List<Vector3>(N);
            for (int i = 0; i < N; i++)
            {
                mass.Add( defaultMass );
                position.Add( new Vector3() );
                velocity.Add( new Vector3() );
            }
        }

        public EulerState(EulerState other)
        {
            currentTime = other.currentTime;
            N = other.N;
            G = other.G;
            mass =  new List<double>( other.mass );
            position = new List<Vector3>(N); 
            velocity = new List<Vector3>(N);
            for (int i = 0; i < N; i++)
            {
                position.Add( new Vector3(other.position[i]));
                velocity.Add( new Vector3(other.velocity[i]));
            }           
        }

        public EulerState(INBodyState other)
        {
            currentTime = other.t;
            N = other.N;
            G = other.G;
            mass = new List<double>(other.mList);
            position = new List<Vector3>(N);
            velocity = new List<Vector3>(N);
            for (int i = 0; i < N; i++)
            {
                position.Add( new Vector3(other.r(i)));
                velocity.Add( new Vector3(other.v(i)));
            }
        }

        public double currentTime;
        public double t { get { return currentTime; } }

        public int N { get; set; }
        public double G { get; set; }

        public List<double> mass;
        public double m(int i)
        {
            return mass[i];
        }

        public List<Vector3> position;
        public Vector3 r(int i)
        {
            return position[i];
        }

        public List<Vector3> velocity;
        public Vector3 v(int i)
        {
            return velocity[i];
        }


        public IList<double> mList
        {
            get { return mass; }
        }

        public System.Collections.Generic.IList<Vector3> rList
        {
            get { return position; }
        }

        public System.Collections.Generic.IList<Vector3> vList
        {
            get { return velocity; }
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

        public virtual void Progress(double dt)
        {
            var N = state.N;

            var acceleration = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.position[i] += state.velocity[i] * dt;
                state.velocity[i] += acceleration[i] * dt;
            }

            state.currentTime += dt;
        }


    }

    public class LeapFrogState : EulerState
    {
        [XmlIgnore]
        internal double m_timeOfAccelerationCalculated;
        [XmlIgnore]
        internal Vector3[] m_accelerations;

        public LeapFrogState()
            : base()
        {
        }

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

        public LeapFrogIntegrator(EulerState s)
            : base(new LeapFrogState(s))
        {
        }


        public override void Progress(double dt)
        {
            var N = state.N;
            var deltaTimeHalf = dt * 0.5;
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
                state.position[i] += state.velocity[i] * dt;
            }

            var acceleration2 = state.ComputeAccelerationVectorDirect();

            for (int i = 0; i < N; i++)
            {
                state.velocity[i] += acceleration2[i] * deltaTimeHalf;
            }

            state.currentTime += dt;

            if (null != lfState)
            {
                lfState.m_timeOfAccelerationCalculated = state.currentTime;
                lfState.m_accelerations = acceleration2;
            }
        }

    }

}