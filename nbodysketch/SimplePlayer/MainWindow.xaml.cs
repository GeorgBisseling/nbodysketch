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
        int currentIncrement = 1;

        private void AddStateToCanvas(int index, double Ediff)
        {
            var state = UnitedStates[index];
            double midx = Universe.ActualWidth / 2.0;
            double midy = Universe.ActualHeight / 2.0;

            var Edot = new Rectangle();
            Edot.Width = 2.0;
            Edot.Height = 2.0;
            Edot.SetValue(Canvas.LeftProperty, midx * 2.0 * index / (double)UnitedStates.Count - Edot.Width/2.0 );
            Edot.SetValue(Canvas.TopProperty, midy + Ediff / Math.Abs(EtotStart) * 100.0 - Edot.Width / 2.0);
            Edot.Fill = Brushes.Red;
            Universe.Children.Add(Edot);

            for (int i = 0; i < state.N; i++)
            {
                var pos = state.r[i];
                var mass = state.m[i];
                var dot = new Rectangle();
                dot.Fill = Brushes.Black;
                dot.RadiusX = dot.RadiusY = 0.0;

                var dotX = midx + 200.0 * pos[0];
                var doty = midy - 200.0 * pos[1];

                dot.Width = Math.Max(2.0, 5.0 * mass);
                dot.Height = Math.Max(2.0, 5.0 * mass);
                dot.SetValue(Canvas.LeftProperty, dotX - dot.Width / 2.0);
                dot.SetValue(Canvas.TopProperty, doty - dot.Height / 2.0);

                Universe.Children.Add(dot);
            }
        }

        DispatcherTimer timer;

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (null == timer)
            {
                timer = new DispatcherTimer(TimeSpan.FromMilliseconds(1000.0 / 120.0), DispatcherPriority.Render, UpdateCanvas, this.Dispatcher);
                timer.Start();
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            ReadStates();

        }

        private void UpdateCanvas(object sender, EventArgs e)
        {
            //Universe.Children.Clear();

            int effectiveIndex = currentState % UnitedStates.Count;

            if (currentState > UnitedStates.Count)
            {
                Universe.Children.Clear();
                currentState = currentState % UnitedStates.Count;
            }
            else
            {
                foreach (var c in Universe.Children)
                    (c as Shape).Fill = Brushes.LightGray;
            }

            var state = UnitedStates[effectiveIndex];
            var Ediff = (state.Etot() - EtotStart);
            AddStateToCanvas(effectiveIndex, Ediff);
            currentState += currentIncrement;
            Controls.Content = effectiveIndex.ToString() + " " + state.currentTime + " Ediff = " + Ediff.ToString();
        }

        double EtotStart;
        private void ReadStates()
        {
            //var fileName = @"C:\Users\georg\Documents\Visual Studio 2012\GitHubMess\nbodysketch\nbodysketch\nbodysketch\bin\Debug\data.xml";
            string fileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "data.xml");
            Console.WriteLine("Reading from \"{0}\"", fileName);

            var XmlDeserializer = new XmlSerializer(typeof(List<EulerState>));

            using (var file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                UnitedStates = (List<EulerState>)XmlDeserializer.Deserialize(file);
            }
            EtotStart = UnitedStates[0].Etot();
        }

    }
}
