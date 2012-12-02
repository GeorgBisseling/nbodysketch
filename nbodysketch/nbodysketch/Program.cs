using System;
using System.Collections.Generic;
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

            int N = 2;
            double G = 1.0;
            double mass = 1.0;

            var startState = new EulerState(N, gravitationalConstant: G, defaultMass: mass);

            startState.position[0][0] = -1.0;
            startState.velocity[0][1] = -0.5;

            startState.position[1][0] = 1.0;
            startState.velocity[1][1] = 0.5;

            INBodyIntegrator integrator = new LeapFrogIntegrator(startState);

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

            var beginTime = DateTime.Now;

            using (var file0 = new System.IO.FileStream("body0.csv", FileMode.Create, FileAccess.Write))
            using (var sw0 = new StreamWriter(file0))
            using (var file1 = new System.IO.FileStream("body1.csv", FileMode.Create, FileAccess.Write))
            using (var sw1 = new StreamWriter(file1))
                while (integrator.currentTMax < 1200.5)
            {

                oldTime = integrator.currentTMax;
                integrator.Progress(delta);
                newTime = integrator.currentTMax;
                newState = integrator.currentState(newTime);
                ekin = newState.Ekin();
                epot = newState.Epot();
                etot = ekin + epot;
                if (0 == (count % 5000))
                {
                    sw0.WriteLine("{0}; {1}", newState.r(0)[0], newState.r(0)[1]);
                    sw1.WriteLine("{0}; {1}", newState.r(1)[0], newState.r(1)[1]);
                    Console.WriteLine("t:{0} Ekin:{1} Epot:{2} Etot:{3} Ediff:{4}", newTime, ekin, epot, etot, etot - etot_start);
                }
                count++;
            }

            var endTime = DateTime.Now;


            Console.Error.WriteLine("iterations per second: {0}", ((double)count)/ (endTime-beginTime).TotalSeconds );

            if (System.Diagnostics.Debugger.IsAttached)
            {
                Console.Error.WriteLine("Press ENTER to finish.");
                Console.ReadLine();
            }
        }
    }
}
