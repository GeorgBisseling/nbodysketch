using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using NBodyLib;


namespace nbodysketch
{
    class Program
    {
        static void Main(string[] args)
        {
            //Vector3.Test();

            int N = 5;
            double G = 1.0;
            double mass = 1.0;

            var startState = new LeapFrogState(N, gravitationalConstant: G, defaultMass: mass);

            startState.position[0][0] = -1.0;
            startState.velocity[0][1] = -0.5;

            startState.position[1][0] = 1.0;
            startState.velocity[1][1] = 0.5;

            for (int particle = 2; particle < N; particle++)
            {
                double angle = particle * 0.5*Math.PI/(N-1);
                startState.position[particle] = new Vector3(2.0 * Math.Sin(angle), 2.0 * Math.Cos(angle), 0.0);
                startState.mass[particle] = 0.001;
            }

            INBodyIntegrator integrator = new LeapFrogIntegrator(startState);

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

            var timeProgress= new Stopwatch();
            Debug.Assert(!timeProgress.IsRunning);
            var beginTime = DateTime.Now;

            using (var file0 = new System.IO.FileStream("body0.csv", FileMode.Create, FileAccess.Write))
            using (var sw0 = new StreamWriter(file0))
            using (var file1 = new System.IO.FileStream("body1.csv", FileMode.Create, FileAccess.Write))
            using (var sw1 = new StreamWriter(file1))
                while (integrator.currentTMax < 12.5)
            {

                oldTime = integrator.currentTMax;

                timeProgress.Start();
                integrator.Progress(delta);
                timeProgress.Stop();

                newTime = integrator.currentTMax;
                newState = integrator.currentState(newTime);
                ekin = newState.Ekin();
                epot = newState.Epot();
                etot = ekin + epot;

                // CheckComputationOfEpot(newState, epot);

                if (0 == (count % 50))
                {
                    sw0.WriteLine("{0}; {1}", newState.r(0)[0], newState.r(0)[1]);
                    sw1.WriteLine("{0}; {1}", newState.r(1)[0], newState.r(1)[1]);
                    Console.WriteLine("t:{0} Ekin:{1} Epot:{2} Etot:{3} Ediff:{4}", newTime, ekin, epot, etot, etot - etot_start);
                }
                count++;
            }

            var endTime = DateTime.Now;
            var elapsedTime = endTime - beginTime;
           

            Console.Error.WriteLine("iterations per second: {0}", ((double)count)/ elapsedTime.TotalSeconds );
            Console.Error.WriteLine("{0} seconds total time", elapsedTime.TotalSeconds);
            Console.Error.WriteLine("{0} seconds in Progress", timeProgress.Elapsed.TotalSeconds);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Error.WriteLine("Press ENTER to finish.");
                Console.ReadLine();
            }
        }

        private static void CheckComputationOfEpot(INBodyState newState, double epot)
        {
            var revState = new LeapFrogState(newState as LeapFrogState);
            revState.position = revState.position.Reverse().ToArray();
            revState.velocity = revState.velocity.Reverse().ToArray();
            revState.mass = revState.mass.Reverse().ToArray();
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
