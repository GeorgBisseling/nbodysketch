using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Windows.Threading;

using System.Xml.Serialization;
using NBodyLib;

namespace SimplePlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WindowState = WindowState.Maximized;
        }

        List<EulerState> UnitedStates;
        int currentState = 0;
        int currentIncrement = 10;

        List<Polyline> particleLines;
        Polyline energyLine;

        private void AddStateToCanvas(int index, double Ediff)
        {
            var state = UnitedStates[index];
            double midx = Universe.ActualWidth / 2.0;
            double midy = Universe.ActualHeight / 2.0;

            var Edot = new Rectangle();
            Edot.SetValue(Canvas.ZIndexProperty, 1);
            Edot.Width = 2.0;
            Edot.Height = 2.0;
            var edotx = midx * 2.0 * index / (double)UnitedStates.Count - Edot.Width/2.0;
            var edoty = midy + Ediff / Math.Abs(EtotStart) * 100.0 - Edot.Width / 2.0;

            Edot.SetValue(Canvas.LeftProperty, edotx );
            Edot.SetValue(Canvas.TopProperty, edoty );
            Edot.Fill = Brushes.Red;
            Universe.Children.Add(Edot);

            energyLine.Points.Add(new Point(edotx, edoty));
            if (energyLine.Points.Count > 100) energyLine.Points.RemoveAt(0);
            Universe.Children.Add(energyLine);

            for (int i = 0; i < state.N; i++)
            {
                var line = particleLines[i];

                var pos = state.r[i];
                var mass = state.m[i];
                var dot = new Rectangle();
                dot.SetValue(Canvas.ZIndexProperty, 1);
                dot.Fill = Brushes.Black;
                dot.RadiusX = dot.RadiusY = 0.0;

                var dotX = midx + 200.0 * pos[0];
                var doty = midy - 200.0 * pos[1];

                dot.Width = Math.Max(2.0, 5.0 * mass);
                dot.Height = Math.Max(2.0, 5.0 * mass);
                dot.SetValue(Canvas.LeftProperty, dotX - dot.Width / 2.0);
                dot.SetValue(Canvas.TopProperty, doty - dot.Height / 2.0);

                Universe.Children.Add(dot);
                //line.Points.Add(new Point(dotX, doty));
                //if (line.Points.Count > 100) line.Points.RemoveAt(0);
                //Universe.Children.Add(line);
            }


        }

        DispatcherTimer timer;

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (null == timer)
            {
                timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1), DispatcherPriority.Render , UpdateCanvas, this.Dispatcher);

                if (null != PlayButton && PlayButton.IsChecked == true)
                    timer.Start();
                else
                    timer.Stop();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            try
            {
                ReadStates();
            }
            catch (Exception exc)
            {
                Label.Content = exc.ToString();
            }

        }

        private void UpdateCanvas(object sender, EventArgs e)
        {
            Universe.Children.Clear();

            if (0 == UnitedStates.Count)
                return;

            int effectiveIndex = currentState % UnitedStates.Count;

            if (currentState == 0 || currentState > UnitedStates.Count)
            {
                var N = UnitedStates[0].N;
                Universe.Children.Clear();
                particleLines = new List<Polyline>();
                for (int i = 0; i < N; i++)
                {
                    var line = new Polyline();
                    line.Stroke = Brushes.LightGray;
                    line.StrokeThickness = 1;
                    particleLines.Add(line);
                }
                energyLine = new Polyline();
                energyLine.Stroke = Brushes.LightGray;
                energyLine.StrokeThickness = 1;

                currentState = currentState % UnitedStates.Count;
            }

            var state = UnitedStates[effectiveIndex];
            var Ediff = (state.Etot() - EtotStart);
            AddStateToCanvas(effectiveIndex, Ediff);
            currentState += currentIncrement;
            Label.Content = effectiveIndex.ToString() + " " + state.currentTime + " Ediff = " + Ediff.ToString();
        }

        double EtotStart;
        private void ReadStates()
        {
            UnitedStates = new List<EulerState>();
            string fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "data.txt");
            Console.WriteLine("Reading from \"{0}\"", fileName);
            using (var file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new StreamReader(file, true))
            {
                while (!reader.EndOfStream /* && UnitedStates.Count < 1000 */ )
                {
                    try
                    {
                        UnitedStates.Add(new EulerState(reader));
                    }
                    catch (Exception exc)
                    {
                        Label.Content = exc.ToString();
                        PlayButton.IsChecked = false;
                    }
                }
            }
            EtotStart = UnitedStates[0].Etot();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null && PlayButton != null) {
                timer.IsEnabled = (PlayButton.IsChecked ?? false);
            }
        }

    }
}
