using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VectorLib;

namespace NBodyLib
{

    public class MultiStepState : LeapFrogState
    {
        public Dictionary<string, object> memory = new Dictionary<string, object>();

        public MultiStepState()
            : base()
        {
        }

        public MultiStepState(INBodyState state)
            : base(state)
        {
        }

        public MultiStepState(EulerState state)
            : base(state)
        {
        }

        public MultiStepState(LeapFrogState state)
            : base(state)
        {
        }

        public MultiStepState(MultiStepState state)
            : base(state)
        {
            foreach (var k in state.memory.Keys)
            {
                memory[k] = state.memory[k];
            }
        }

    }

    public class MultiStepIntegrator : INBodyIntegrator
    {


        protected MultiStepState state;


        public enum Flavor { ms2, ms4, ms4pc, ms6, ms8 }

        public readonly Flavor flavor;

        public MultiStepIntegrator()
        {
            throw new NotImplementedException();
        }

        public MultiStepIntegrator(EulerState s, Flavor f)
        {
            state = new MultiStepState(s);
            flavor = f;
        }

        public double currentTMin { get { return state.currentTime; } }

        public double currentTMax { get { return state.currentTime; } }

        public INBodyState currentState(double t)
        {
            if (t != state.currentTime)
                throw new InvalidOperationException("Do not have state for that time.");
            return state;
        }

        public void Progress(double dt)
        {
            switch (flavor)
            {
                case Flavor.ms2: Progress_ms2(state, dt); return;
                case Flavor.ms4: Progress_ms4(state, dt); return;
                case Flavor.ms4pc: Progress_ms4pc(state, dt); return;
                case Flavor.ms6: Progress_ms6(state, dt); return;
                case Flavor.ms8: Progress_ms8(state, dt); return;
            }

            throw new NotImplementedException();
        }

        private void Progress_ms8(MultiStepState state, double dt)
        {
            if (!state.memory.ContainsKey("ap7"))
            {
                state.memory["ap7"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo8(state, dt);
            }
            else if (!state.memory.ContainsKey("ap6"))
            {
                state.memory["ap6"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo8(state, dt);
            }
            else if (!state.memory.ContainsKey("ap5"))
            {
                state.memory["ap5"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo8(state, dt);
            }
            else if (!state.memory.ContainsKey("ap4"))
            {
                state.memory["ap4"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo8(state, dt);
            }
            else if (!state.memory.ContainsKey("ap3"))
            {
                state.memory["ap3"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo8(state, dt);
            }
            else if (!state.memory.ContainsKey("ap2"))
            {
                state.memory["ap2"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo8(state, dt);
            }
            else if (!state.memory.ContainsKey("ap1"))
            {
                state.memory["ap1"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo8(state, dt);
            }
            else
            {
                var N = state.N;

                var ap0 = state.EulerState_ComputeAccelerationVectorDirect();
                Vector3[] ap1 = (Vector3[])state.memory["ap1"];
                Vector3[] ap2 = (Vector3[])state.memory["ap2"];
                Vector3[] ap3 = (Vector3[])state.memory["ap3"];
                Vector3[] ap4 = (Vector3[])state.memory["ap4"];
                Vector3[] ap5 = (Vector3[])state.memory["ap5"];
                Vector3[] ap6 = (Vector3[])state.memory["ap6"];
                Vector3[] ap7 = (Vector3[])state.memory["ap7"];

                var jdt = new Vector3[N];
                var sdt2 = new Vector3[N];
                var cdt3 = new Vector3[N];
                var pdt4 = new Vector3[N];
                var xdt5 = new Vector3[N];
                var ydt6 = new Vector3[N];
                var zdt7 = new Vector3[N];
                Parallel.For(0, N, i =>
                //for (int i = 0; i < N; i++)
                {
                    jdt[i]  = (ap0[i]*1089 - ap1[i]*2940 + ap2[i]*4410 - ap3[i]*4900 + ap4[i]*3675 - ap5[i]*1764 + ap6[i]*490 - ap7[i]*60)/420 ;
                    sdt2[i] = (ap0[i]*938 - ap1[i]*4014 + ap2[i]*7911 - ap3[i]*9490 + ap4[i]*7380 - ap5[i]*3618 + ap6[i]*1019 - ap7[i]*126)/180;
                    cdt3[i] = (ap0[i]*967 - ap1[i]*5104 + ap2[i]*11787 - ap3[i]*15560 + ap4[i]*12725 - ap5[i]*6432 + ap6[i]*1849 - ap7[i]*232)/120;
                    pdt4[i] = (ap0[i]*56 - ap1[i]*333 + ap2[i]*852 - ap3[i]*1219 + ap4[i]*1056 - ap5[i]*555 + ap6[i]*164 - ap7[i]*21)/6;
                    xdt5[i] = (ap0[i]*46 - ap1[i]*295 + ap2[i]*810 - ap3[i]*1235 + ap4[i]*1130 - ap5[i]*621 + ap6[i]*190 - ap7[i]*25)/6;
                    ydt6[i] = ap0[i]*4 - ap1[i]*27 + ap2[i]*78 - ap3[i]*125 + ap4[i]*120 - ap5[i]*69 + ap6[i]*22 - ap7[i]*3;
                    zdt7[i] = ap0[i] - ap1[i]*7 + ap2[i]*21 - ap3[i]*35 + ap4[i]*35 - ap5[i]*21 + ap6[i]*7 - ap7[i];
                    state.r[i] += (state.v[i]+(ap0[i]+(jdt[i]+(sdt2[i]+(cdt3[i]+(pdt4[i]+(xdt5[i]+ydt6[i]/8)/7)/6)/5)/4)/3)*dt/2)*dt;
                    state.v[i] += (ap0[i] + (jdt[i] + (sdt2[i] + (cdt3[i] + (pdt4[i] + (xdt5[i] + (ydt6[i] + zdt7[i] / 8) / 7) / 6) / 5) / 4) / 3) / 2) * dt;
                });
                state.memory["ap7"] = ap6;
                state.memory["ap6"] = ap5;
                state.memory["ap5"] = ap4;
                state.memory["ap4"] = ap3;
                state.memory["ap3"] = ap2;
                state.memory["ap2"] = ap1;
                state.memory["ap1"] = ap0;

                state.currentTime += dt;
            }
        }

        private void Progress_ms6(MultiStepState state, double dt)
        {
            if (!state.memory.ContainsKey("ap5"))
            {
                state.memory["ap5"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo6(state, dt);
            }
            else if (!state.memory.ContainsKey("ap4"))
            {
                state.memory["ap4"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo6(state, dt);
            }
            else if (!state.memory.ContainsKey("ap3"))
            {
                state.memory["ap3"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo6(state, dt);
            }
            else if (!state.memory.ContainsKey("ap2"))
            {
                state.memory["ap2"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo6(state, dt);
            }
            else if (!state.memory.ContainsKey("ap1"))
            {
                state.memory["ap1"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_yo6(state, dt);
            }
            else
            {
                var N = state.N;

                var ap0 = state.EulerState_ComputeAccelerationVectorDirect();
                Vector3[] ap1 = (Vector3[])state.memory["ap1"];
                Vector3[] ap2 = (Vector3[])state.memory["ap2"];
                Vector3[] ap3 = (Vector3[])state.memory["ap3"];
                Vector3[] ap4 = (Vector3[])state.memory["ap4"];
                Vector3[] ap5 = (Vector3[])state.memory["ap5"];

                var jdt = new Vector3[N];
                var sdt2 = new Vector3[N];
                var cdt3 = new Vector3[N];
                var pdt4 = new Vector3[N];
                var xdt5 = new Vector3[N];
                Parallel.For(0, N, i =>
                //for (int i = 0; i < N; i++)
                {
                    jdt[i] = (ap0[i]*137 - ap1[i]*300 + ap2[i]*300 - ap3[i]*200 + ap4[i]*75 - ap5[i]*12)/60;
                    sdt2[i] = (ap0[i]*45 - ap1[i]*154 + ap2[i]*214 - ap3[i]*156 + ap4[i]*61 - ap5[i]*10)/12;
                    cdt3[i] = (ap0[i]*17 - ap1[i]*71 + ap2[i]*118 - ap3[i]*98 + ap4[i]*41 - ap5[i]*7)/4;
                    pdt4[i] = ap0[i]*3 - ap1[i]*14 + ap2[i]*26 - ap3[i]*24 + ap4[i]*11 - ap5[i]*2;
                    xdt5[i] = ap0[i] - ap1[i] * 5 + ap2[i] * 10 - ap3[i] * 10 + ap4[i] * 5 - ap5[i];
                    state.r[i] += (state.v[i]+(ap0[i]+ (jdt[i]+(sdt2[i]+(cdt3[i]+pdt4[i]/6)/5)/4)/3)*dt/2)*dt;
                    state.v[i] += (ap0[i] + (jdt[i] + (sdt2[i] + (cdt3[i] + (pdt4[i] + xdt5[i] / 6) / 5) / 4) / 3) / 2) * dt;
                });
                state.memory["ap5"] = ap4;
                state.memory["ap4"] = ap3;
                state.memory["ap3"] = ap2;
                state.memory["ap2"] = ap1;
                state.memory["ap1"] = ap0;

                state.currentTime += dt;
            }
        }

        private void Progress_ms4pc(MultiStepState state, double dt)
        {
            if (!state.memory.ContainsKey("ap3"))
            {
                state.memory["ap3"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_rk4(state, dt);
            }
            else if (!state.memory.ContainsKey("ap2"))
            {
                state.memory["ap2"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_rk4(state, dt);
            }
            else if (!state.memory.ContainsKey("ap1"))
            {
                state.memory["ap1"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_rk4(state, dt);
            }
            else
            {
                var N = state.N;

                var ap0 = state.EulerState_ComputeAccelerationVectorDirect();
                Vector3[] ap1 = (Vector3[])state.memory["ap1"];
                Vector3[] ap2 = (Vector3[])state.memory["ap2"];
                Vector3[] ap3 = (Vector3[])state.memory["ap3"];

                var jdt = new Vector3[N];
                var sdt2 = new Vector3[N];
                var cdt3 = new Vector3[N];
                Parallel.For(0, N, i =>
                //for (int i = 0; i < N; i++)
                {
                    jdt[i] = ap0[i] * (11.0 / 6.0) - ap1[i] * 3.0 + ap2[i] * 1.5 - ap3[i] / 3.0;
                    sdt2[i] = ap0[i] * 2.0 - ap1[i] * 5.0 + ap2[i] * 4.0 - ap3[i];
                    cdt3[i] = ap0[i] - ap1[i] * 3.0 + ap2[i] * 3.0 - ap3[i];
                    state.r[i] += state.v[i] * dt + (ap0[i] / 2.0 + jdt[i] / 6.0 + sdt2[i] / 24.0) * dt * dt;
                    // we only need the predicted 
                    //state.v[i] += ap0[i] * dt + (jdt[i] / 2.0 + sdt2[i] / 6.0 + cdt3[i] / 24.0) * dt;
                });
                state.memory["ap3"] = ap3 = ap2;
                state.memory["ap2"] = ap2 = ap1;
                state.memory["ap1"] = ap1 = ap0;
                ap0 = state.EulerState_ComputeAccelerationVectorDirect();
                Parallel.For(0, N, i =>
                //for (int i = 0; i < N; i++)
                {
                    jdt[i] = ap0[i] * (11.0 / 6.0) - ap1[i] * 3.0 + ap2[i] * 1.5 - ap3[i] / 3.0;
                    sdt2[i] = ap0[i] * 2.0 - ap1[i] * 5.0 + ap2[i] * 4.0 - ap3[i];
                    cdt3[i] = ap0[i] - ap1[i] * 3.0 + ap2[i] * 3.0 - ap3[i];
                    state.r[i] += state.v[i] * dt + (ap0[i] / 2.0 + jdt[i] / 6.0 + sdt2[i] / 24.0) * dt * dt;
                    state.v[i] += ap0[i] * dt + (jdt[i] / 2.0 + sdt2[i] / 6.0 + cdt3[i] / 24.0) * dt;
                });


                state.currentTime += dt;
            }
        }

        private void Progress_ms4(MultiStepState state, double dt)
        {
            if (!state.memory.ContainsKey("ap3"))
            {
                state.memory["ap3"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_rk4(state, dt);
            }
            else if (!state.memory.ContainsKey("ap2"))
            {
                state.memory["ap2"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_rk4(state, dt);
            }
            else if (!state.memory.ContainsKey("ap1"))
            {
                state.memory["ap1"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
                RungeKuttaIntegrator.Progress_rk4(state, dt);
            }
            else
            {
                var N = state.N;

                var ap0 = state.EulerState_ComputeAccelerationVectorDirect();
                Vector3[] ap1 = (Vector3[])state.memory["ap1"];
                Vector3[] ap2 = (Vector3[])state.memory["ap2"];
                Vector3[] ap3 = (Vector3[])state.memory["ap3"];

                var jdt = new Vector3[N];
                var sdt2 = new Vector3[N];
                var cdt3 = new Vector3[N];
                Parallel.For(0, N, i =>
                //for (int i = 0; i < N; i++)
                {
                    jdt[i] = ap0[i] * (11.0 / 6.0) - ap1[i] * 3.0 + ap2[i] * 1.5 - ap3[i] / 3.0;
                    sdt2[i] = ap0[i] * 2.0 - ap1[i] * 5.0 + ap2[i] * 4.0 - ap3[i];
                    cdt3[i] = ap0[i] - ap1[i] * 3.0 + ap2[i] * 3.0 - ap3[i];
                    state.r[i] += state.v[i] * dt + (ap0[i] / 2.0 + jdt[i] / 6.0 + sdt2[i] / 24.0) * dt * dt;
                    state.v[i] += ap0[i] * dt + (jdt[i] / 2.0 + sdt2[i] / 6.0 + cdt3[i] / 24.0) * dt;
                });
                state.memory["ap3"] = ap2;
                state.memory["ap2"] = ap1;
                state.memory["ap1"] = ap0;

                state.currentTime += dt;
            }
        }

        private static void Progress_ms2(MultiStepState state, double dt)
        {
            var N = state.N;

            if (!state.memory.Keys.Contains("prev_acc"))
            {
                RungeKuttaIntegrator.Progress_rk2(new LeapFrogState(state), dt);
                state.memory["prev_acc"] = (Vector3[])
                    state.EulerState_ComputeAccelerationVectorDirect();
            }
            else
            {
                var prev_acc = (Vector3[]) state.memory["prev_acc"];
                Vector3[] old_acc = state.EulerState_ComputeAccelerationVectorDirect();

                var jdt = new Vector3[N];
                Parallel.For(0, N, i =>
                //for (int i = 0; i < N; i++)
                {
                    jdt[i] = old_acc[i] - prev_acc[i];
                    state.r[i] += state.v[i] * dt + old_acc[i] * 0.5 * dt * dt;
                    state.v[i] += old_acc[i] * dt + jdt[i] * 0.5 * dt;
                });
                state.memory["prev_acc"] = old_acc;

                state.currentTime += dt;
            }
        }
    }
}
