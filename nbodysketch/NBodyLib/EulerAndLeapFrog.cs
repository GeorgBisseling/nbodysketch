using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Threading.Tasks;

using VectorLib;
using NBodyLibCPP;

namespace NBodyLib
{
    public class EulerState : INBodyState
    {
        public EulerState()
        {
        }
        
        public EulerState(int n, double gravitationalConstant, double defaultMass, double softeningLength)
        {
            currentTime = 0.0;
            N = n;
            G = gravitationalConstant;
            eps = softeningLength;
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
            eps = other.eps;
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
            eps = other.eps;
            mass = new List<double>(other.m);
            position = new List<Vector3>(N);
            velocity = new List<Vector3>(N);
            for (int i = 0; i < N; i++)
            {
                position.Add( new Vector3(other.r[i]) );
                velocity.Add( new Vector3(other.v[i]) );
            }
        }

        private string read(StreamReader sr)
        {
            var rawLine = sr.ReadLine();
            var chunks = rawLine.Split('#');
            var cleanChunk = chunks[0].Trim();
            return cleanChunk;
        }

        public EulerState(StreamReader sr)
        {
            string line;
            line = read(sr); currentTime = Double.Parse(line);
            line = read(sr); N = Int32.Parse(line);
            line = read(sr); G = Double.Parse(line);
            line = read(sr); eps = Double.Parse(line);
            line = read(sr); var ekin = Double.Parse(line);
            line = read(sr); var epot = Double.Parse(line);


            mass = new List<double>(N);
            position = new List<Vector3>(N);
            velocity = new List<Vector3>(N);

            for (int i = 0; i < N; i++)
            {
                line = read(sr); mass.Add( Double.Parse(line) );
            }
            for (int i = 0; i < N; i++)
            {
                line = read(sr);
                var components = line.Split(' ', '\t');
                position.Add( new Vector3(Double.Parse(components[0]), Double.Parse(components[1]), Double.Parse(components[2])) );
            }
            for (int i = 0; i < N; i++)
            {
                line = read(sr);
                var components = line.Split(' ', '\t');
                velocity.Add( new Vector3(Double.Parse(components[0]), Double.Parse(components[1]), Double.Parse(components[2])) );
            }
        }

        public double currentTime;
        public double t { get { return currentTime; } }

        public int N { get; set; }
        public double G { get; set; }
        public double eps { get; set; }

        public List<double> mass;
        public List<Vector3> position;
        public List<Vector3> velocity;

        public IList<double> m
        {
            get { return mass; }
        }

        public System.Collections.Generic.IList<Vector3> r
        {
            get { return position; }
        }

        public System.Collections.Generic.IList<Vector3> v
        {
            get { return velocity; }
        }


        public Vector3 EulerState_A_onFirstFromSecond(int first, int second, Vector3 rdiff_tmp)
        {
            var r1 = position[first];
            var r2 = position[second];

            rdiff_tmp.c0 = r2.c0 - r1.c0;
            rdiff_tmp.c1 = r2.c1 - r1.c1;
            rdiff_tmp.c2 = r2.c2 - r1.c2;

            double R2 = rdiff_tmp * rdiff_tmp;
            double R = Math.Sqrt(R2);
            double eps2 = eps * eps;

            double factor = mass[second] * G / (R * (R2 + eps * eps));

            rdiff_tmp.c0 *= factor;
            rdiff_tmp.c1 *= factor;
            rdiff_tmp.c2 *= factor;

            return rdiff_tmp;
        }

        public Vector3[] EulerState_ComputeAccelerationVectorDirect()
        {
            var accVector = new Vector3[N];
            var localG = G;
            var localEps = eps;
            var eps2 = localEps * localEps;

            // compute accelerations
            Parallel.For(0, N, i =>
            {
                var tmp = new Vector3();
                accVector[i] = new Vector3();
                var avi = accVector[i];
                var ri = position[i];
                for (int j = 0; j < N; j++)
                    if (j != i)
                    {
                        //var a = this.EulerState_A_onFirstFromSecond(i, j, tmp);
                        var a = EulerStateBoost.EulerStateBoost_A_onFirstFromSecond(ri, position[j], mass[j], localG, eps2, tmp);
                        avi.c0 += a.c0;
                        avi.c1 += a.c1;
                        avi.c2 += a.c2;
                    }
            });

            return accVector;
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

            var acceleration = state.EulerState_ComputeAccelerationVectorDirect();

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

        public LeapFrogState(int n, double gravitationalConstant, double defaultMass, double softeningLength)
            : base(n, gravitationalConstant, defaultMass, softeningLength)
        {

        }

        public LeapFrogState(INBodyState state)
            : base(state)
        {
            m_timeOfAccelerationCalculated = -1.0;
            m_accelerations = null;
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
            var lfState = state as LeapFrogState;
            Progress_LeapFrog(lfState, dt);
        }

        public static void Progress_LeapFrog(LeapFrogState lfState, double dt)
        {
            var N = lfState.N;
            var deltaTimeHalf = dt * 0.5;
            Vector3[] acceleration1;

            if (lfState.currentTime == lfState.m_timeOfAccelerationCalculated && lfState.m_accelerations != null) // && length matches, in case we merge or split particles
                acceleration1 = lfState.m_accelerations;
            else
                acceleration1 = lfState.EulerState_ComputeAccelerationVectorDirect();

            Parallel.For(0, N, i =>
            //for (int i = 0; i < N; i++)
            {
                lfState.velocity[i] += acceleration1[i] * deltaTimeHalf;
                lfState.position[i] += lfState.velocity[i] * dt;
            });

            var acceleration2 = lfState.EulerState_ComputeAccelerationVectorDirect();

            Parallel.For(0, N, i =>
            //for (int i = 0; i < N; i++)
            {
                lfState.velocity[i] += acceleration2[i] * deltaTimeHalf;
            });

            lfState.currentTime += dt;

            lfState.m_timeOfAccelerationCalculated = lfState.currentTime;
            lfState.m_accelerations = acceleration2;
        }
    }
}