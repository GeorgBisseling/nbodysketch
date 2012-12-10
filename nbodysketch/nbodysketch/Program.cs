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


namespace nbodysketch
{
    class Program
    {
        static void Main(string[] args)
        {
            //Vector3.Test();

            int N = 10;
            double G = 1.0;
            double mass = 1.0;

            var startState = new EulerState(N, gravitationalConstant: G, defaultMass: mass);

            startState.position[0][0] = -1.0;
            startState.velocity[0][1] = -0.5;

            startState.position[1][0] = 1.0;
            startState.velocity[1][1] = 0.5;

            for (int particle = 2; particle < N; particle++)
            {
                double angle = particle * 2.0 * Math.PI / (N - 2);
                startState.position[particle] = new Vector3(2.0 * Math.Sin(angle), 2.00 * Math.Cos(angle), 0.0);
                startState.velocity[particle] = new Vector3(-0.75 * Math.Cos(angle), 0.75 * Math.Sin(angle), 0.0);
                startState.mass[particle] = 0.0005;
            }

            INBodyIntegrator integrator = new LeapFrogIntegrator(startState);
            //INBodyIntegrator integrator = new RungeKuttaIntegrator(startState, RungeKuttaIntegrator.Flavor.yo8);

            const double delta = 0.01;
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

            var UnitedStates = new List<EulerState>();
            UnitedStates.Add(new EulerState(startState));

            var timeProgress = new Stopwatch();
            var beginTime = DateTime.Now;

            while (integrator.currentTMax < 20.0)
            {
                oldTime = integrator.currentTMax;

                timeProgress.Start();
                integrator.Progress(delta);
                timeProgress.Stop();

                newTime = integrator.currentTMax;
                newState = integrator.currentState(newTime);

                if (0 == count % 10)
                    UnitedStates.Add(new EulerState(newState));

                if (0 == (count % 50))
                {
                    ekin = newState.Ekin();
                    epot = newState.Epot();
                    etot = ekin + epot;
                    Console.WriteLine("t:{0} Ekin:{1} Epot:{2} Etot:{3} Ediff:{4}", newTime, ekin, epot, etot, etot - etot_start);
                }
                count++;
            }

            var endTime = DateTime.Now;
            var elapsedTime = endTime - beginTime;


            Console.Error.WriteLine("iterations per second: {0}", ((double)count) / elapsedTime.TotalSeconds);
            Console.Error.WriteLine("{0} seconds total time", elapsedTime.TotalSeconds);
            Console.Error.WriteLine("{0} seconds in Progress", timeProgress.Elapsed.TotalSeconds);
            
            string stateFileName = Path.Combine(Path.GetTempPath(), "data.xml");
            Console.Error.WriteLine("Storing to \"{0}\"", stateFileName);
            var stateSerializer = new XmlSerializer(UnitedStates.GetType());
            using (var stateFile = new System.IO.FileStream(stateFileName, FileMode.Create, FileAccess.Write))
            {
                stateSerializer.Serialize(stateFile, UnitedStates);
            }

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Error.WriteLine("Press ENTER to finish.");
                Console.ReadLine();
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
