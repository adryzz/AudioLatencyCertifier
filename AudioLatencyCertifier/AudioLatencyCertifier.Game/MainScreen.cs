using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using osuTK.Graphics;
using osuTK.Input;

namespace AudioLatencyCertifier.Game
{
    public class MainScreen : Screen
    {
        [Cached]
        protected readonly OsuColourProvider ColourProvider;

        protected ITrack Track { get; private set; }

        private List<double> events = new List<double>(400);

        private SpriteText ProgressText { get; set; }

        public MainScreen()
        {
            ColourProvider = new OsuColourProvider(OsuColourScheme.Purple);
        }

        [BackgroundDependencyLoader]
        private void load(AudioManager manager)
        {
            Track = new TrackVirtual(10000);
            Container graphContainer;
            BasicButton startButton;
            SpriteText progress;

            InternalChildren = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = 30,
                    CornerExponent = 5f,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = ColourProvider.Background4,
                            RelativeSizeAxes = Axes.Both,
                        },
                        startButton = new BasicButton
                        {
                            Width = 200,
                            Height = 50,
                            CornerRadius = 10,
                            CornerExponent = 5f,
                            Masking = true,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = "Start measurement",
                            BackgroundColour = ColourProvider.Background2,
                            HoverColour = ColourProvider.Highlight1
                        },
                        progress = new SpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                        graphContainer = new Container
                        {
                            Width = 600,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                }
            };
            ProgressText = progress;
            startButton.Action = () => startMeasuring(graphContainer, startButton, progress);
        }

        protected override void Update()
        {
            if (Track.IsRunning)
            {
                ProgressText.Current.Value = $"Progress: {(Track.CurrentTime / Track.Length * 100):F1}%";
            }
            base.Update();
        }

        private void startMeasuring(Container graphContainer, BasicButton startButton, SpriteText progress)
        {
            startButton.Hide();
            Track.Start();
            if (graphContainer.Children.Count > 0)
            {
                graphContainer.Child.Expire();
            }
            Scheduler.AddDelayed(() => checkForInput(startButton, progress), 500);
            Scheduler.AddDelayed(() => checkForErrors(graphContainer, startButton, progress), 11000);
        }

        private void checkForInput(BasicButton startButton, SpriteText progress)
        {
            if (events.Count == 0)
            {
                Track.Reset();
                progress.Current.Value = "Error 0";
                Scheduler.AddDelayed(() => resetUi(startButton, progress), 500);
            }
        }

        private void checkForErrors(Container graphContainer, BasicButton startButton, SpriteText progress)
        {
            if (events.Count >= 400)
            {
                List<double> latencies = generateTimingList(4);
                Timings timings = calculateTimings(latencies);
                graphContainer.Child = new TimingDistributionGraph(latencies)
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 250
                };
                return;
            }
            Track.Reset();
            if (!startButton.IsPresent)
            {
                progress.Current.Value = "Error 1";
                Scheduler.AddDelayed(() => resetUi(startButton, progress), 500);
            }
        }

        private void resetUi(BasicButton button, SpriteText progress)
        {
            button.Show();
            progress.Text = string.Empty;
        }

        private List<double> generateTimingList(double frequency)
        {
            List<double> timings = new List<double>(events.Count);
            double period = 1 / frequency;
            double time = 0;
            for (int i = 0; i < events.Count; i++)
            {
                time += (i * period);
                double latency = events[i] - time;
                timings.Add(latency);
            }

            return timings;
        }

        private Timings calculateTimings(List<double> dataPoints)
        {
            Timings t = new Timings
            {
                Min = dataPoints.Min(),
                Max = dataPoints.Max(),
                Avg = dataPoints.Average()
            };


            double sum = dataPoints.Sum(value => Math.Abs(value - t.Avg));

            t.MeanDev = sum / events.Count;

            return t;
        }

        struct Timings
        {
            public double Min;
            public double Avg;
            public double Max;
            public double MeanDev;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.Enter)
            {
                events.Add(Track.CurrentTime);
            }

            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            base.OnKeyUp(e);
        }
    }
}
