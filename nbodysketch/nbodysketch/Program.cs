using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

using NBodyLib;
using VectorLib;

namespace nbodysketch
{
    class Program
    {
        static void Main(string[] args)
        {
            bool cancelled = false;

            Console.TreatControlCAsInput = false;
            Console.CancelKeyPress += (s, ev) =>
            {
                Console.WriteLine("Ctrl+C pressed");
                ev.Cancel = true;
                cancelled = true;
            };



            Exception caught = null;

            //Vector3.Test();

            int N = 1000;
            double G = 1.0;
            double mass = 1.0;
            double softeningLength = 0.1;
            var startState = new EulerState(N, gravitationalConstant: G, defaultMass: mass, softeningLength: softeningLength);



            StartUp_Ring(N, startState);

            //StartUp_TwoOnCircle(startState);

            //StartUp_ThreeOnEight(startState);

            //StartUp_ColdCollapse8(startState);


            INBodyIntegrator integrator = new LeapFrogIntegrator(startState);
            //INBodyIntegrator integrator = new RungeKuttaIntegrator(startState, RungeKuttaIntegrator.Flavor.rk4);
            //INBodyIntegrator integrator = new MultiStepIntegrator(startState, MultiStepIntegrator.Flavor.ms8);

            const double delta = 0.001;
            double oldTime;
            double newTime;
            INBodyState newState;
            double ekin_start = startState.Ekin();
            double epot_start = startState.Epot();
            double etot_start = ekin_start + epot_start;

            double ekin;
            double epot;
            double etot;

            int count = 0;

            string stateFileName = Path.Combine(Path.GetTempPath(), "data.txt");
            Console.Error.WriteLine("Storing to \"{0}\"", stateFileName);

            const int printModulus = 50;
            const int saveModulus = 10;

            var timeProgress = new Stopwatch();
            var beginTime = DateTime.Now;

            try
            {
                using (var stateFile = new System.IO.FileStream(stateFileName, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var stateWriter = new StreamWriter(stateFile, Encoding.UTF8))
                {
                    stateWriter.AutoFlush = true;
                    
                    
                    while (!cancelled && integrator.currentTMax < 240.0)
                    {
                        oldTime = integrator.currentTMax;

                        timeProgress.Start();
                        integrator.Progress(delta);
                        timeProgress.Stop();

                        newTime = integrator.currentTMax;
                        newState = integrator.currentState(newTime);

                        if (0 == (count % saveModulus) || 0 == (count % printModulus))
                        {
                            ekin = newState.Ekin();
                            epot = newState.Epot();
                            etot = ekin + epot;

                            if (0 == (count % printModulus))
                            {
                                Console.WriteLine("t:{0} Ekin:{1} Epot:{2} Etot:{3} Ediff:{4}", newTime, ekin, epot, etot, etot - etot_start);
                            }

                            if (0 == count % saveModulus)
                            {
                                var sb = new StringBuilder();
                                newState.Serialize(ekin, epot, sb);
                                stateWriter.Write(sb.ToString());
                            }
                        }

                        count++;
                    }
                }

                var endTime = DateTime.Now;
                var elapsedTime = endTime - beginTime;


                Console.Error.WriteLine("iterations per second: {0}", ((double)count) / elapsedTime.TotalSeconds);
                Console.Error.WriteLine("{0} seconds total time", elapsedTime.TotalSeconds);
                Console.Error.WriteLine("{0} seconds in Progress", timeProgress.Elapsed.TotalSeconds);
            }
            catch (Exception e)
            {
                caught = e;
            }
            finally
            {
            }

            if (null != caught)
            {
                Console.Error.WriteLine(caught.ToString());
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Error.WriteLine("Press ENTER to finish.");
                Console.ReadLine();
            }
        }

        private static void StartUp_Ring(int N, EulerState startState)
        {
            var rand = new Random();
            for (int particle = 0; particle < N; particle++)
            {
                double angle = particle * 2.0 * Math.PI / (N - 2);
                double a = 0.01 * rand.NextDouble();
                double b = 0.01 * rand.NextDouble();
                double rFactor = 1.0 + (particle % 10) * 0.1; 
                startState.position[particle] = new Vector3(1.0 * Math.Sin(angle), 1.0 * Math.Cos(angle), 0.0) * rFactor;
                startState.velocity[particle] = new Vector3( a - 0.1 * Math.Cos(angle), b + 0.1 * Math.Sin(angle), 0.0) * rFactor * rFactor;
                //startState.position[particle] = new Vector3(2 - 4 * rand.NextDouble(), 2 - 4 * rand.NextDouble(), 0.0);
                //startState.velocity[particle] = new Vector3(0.0, 0.0, 0.0);
                startState.mass[particle] = 0.0005;
            }
        }

        private static void StartUp_TwoOnCircle(EulerState startState)
        {
            if (2 != startState.N)
                throw new ArgumentOutOfRangeException("Need two bodies here.");

            // two in a circle
            startState.position[0][0] = -1.0;
            startState.velocity[0][1] = -0.5;

            startState.position[1][0] = 1.0;
            startState.velocity[1][1] = 0.5;
        }

        private static void StartUp_ThreeOnEight(EulerState startState)
        {
            if (3 != startState.N)
                throw new ArgumentOutOfRangeException("Need three bodies here.");
            // three in an eight
            startState.position[0] = new Vector3(0.9700436, -0.24308753);
            startState.velocity[0] = new Vector3(0.466203685, 0.43236573);

            startState.position[1] = new Vector3(-0.9700436, 0.24308753);
            startState.velocity[1] = new Vector3(0.466203685, 0.43236573);

            startState.position[2] = new Vector3();
            startState.velocity[2] = new Vector3(-0.93240737, -0.86473146);
        }

        private static void StartUp_ColdCollapse8(EulerState startState)
        {
            if (8 != startState.N)
                throw new ArgumentOutOfRangeException("Need eight bodies here.");
            for (int i = 0; i < 8; i++)
            {
                double x = (i & 1) * 1.0;
                double y = (i & 2) * 0.5;
                double z = (i & 4) * 0.25;
                startState.position[i] = new Vector3(x, y, z);
                startState.velocity[i] = new Vector3();
                startState.mass[i] = 1.0 + i * 0.1;
            }
        }


        private static void CheckComputationOfEpot(INBodyState newState, double epot)
        {
            var revState = new LeapFrogState(newState as LeapFrogState);
            revState.position.Reverse();
            revState.velocity.Reverse();
            revState.mass.Reverse();
            var revEpot = revState.Epot();
            if (!CompareDouble(revEpot, epot))
            {
                Console.Error.WriteLine("Epot = {0}, Epot rev = {1}", epot, revEpot);
            }
        }

        private static bool CompareDouble(double left, double right, double delta = 1.0e-5)
        {
            Debug.Assert(delta > 0.0);

            var relativeError = Math.Abs(left - right) / (0.5 * (Math.Abs(left) + Math.Abs(right)));

            return relativeError < delta;
        }
    }
}
