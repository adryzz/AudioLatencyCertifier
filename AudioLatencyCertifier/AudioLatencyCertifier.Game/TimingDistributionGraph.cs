using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Layout;
using osuTK.Graphics;

namespace AudioLatencyCertifier.Game
{
    /// <summary>
    /// A graph which displays the distribution of events in time. Adapted from the osu! graph.
    /// </summary>
    public partial class TimingDistributionGraph : CompositeDrawable
    {
        /// <summary>
        /// The number of bins on the timing distribution.
        /// </summary>
        private const int timing_distribution_bins = 100;

        /// <summary>
        /// The total number of bins in the timing distribution including the centre bin at 0.
        /// </summary>
        private const int total_timing_distribution_bins = timing_distribution_bins + 1;

        /// <summary>
        /// The centre bin, with a timing distribution very close to/at 0.
        /// </summary>
        private const int timing_distribution_centre_bin_index = 0;

        /// <summary>
        /// The number of data points shown on each side of the axis below the graph.
        /// </summary>
        private const float axis_points = 10;

        /// <summary>
        /// The currently displayed events.
        /// </summary>
        private readonly IReadOnlyList<double> events;

        private readonly IDictionary<double, int>[] bins;
        private double binSize;
        private double hitOffset;

        private Bar[]? barDrawables;

        /// <summary>
        /// Creates a new <see cref="TimingDistributionGraph"/>.
        /// </summary>
        /// <param name="events">The events to display the timing distribution of.</param>
        public TimingDistributionGraph(IReadOnlyList<double> events)
        {
            this.events = events.ToList();
            bins = Enumerable.Range(0, total_timing_distribution_bins).Select(_ => new Dictionary<double, int>()).ToArray<IDictionary<double, int>>();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (events.Count == 0)
                return;

            binSize = Math.Ceiling(events.Max() / timing_distribution_bins);

            // Prevent div-by-0 by enforcing a minimum bin size
            binSize = Math.Max(1, binSize);

            Scheduler.AddOnce(updateDisplay);
        }

        public void UpdateOffset(double hitOffset)
        {
            this.hitOffset = hitOffset;
            Scheduler.AddOnce(updateDisplay);
        }

        private void updateDisplay()
        {
            bool roundUp = true;

            foreach (var bin in bins)
                bin.Clear();

            foreach (var time in events)
            {

                double binOffset = time / binSize;

                // .NET's round midpoint handling doesn't provide a behaviour that works amazingly for display
                // purposes here. We want midpoint rounding to roughly distribute evenly to each adjacent bucket
                // so the easiest way is to cycle between downwards and upwards rounding as we process events.
                if (Math.Abs(binOffset - (int)binOffset) == 0.5)
                {
                    binOffset = (int)binOffset + Math.Sign(binOffset) * (roundUp ? 1 : 0);
                    roundUp = !roundUp;
                }

                int index = timing_distribution_centre_bin_index + (int)Math.Round(binOffset, MidpointRounding.AwayFromZero);

                // may be out of range when applying an offset. for such cases we can just drop the results.
                if (index >= 0 && index < bins.Length)
                {
                    bins[index].TryGetValue(time, out int value);
                    bins[index][time] = ++value;
                }
            }

            if (barDrawables == null)
                createBarDrawables();
            else
            {
                for (int i = 0; i < barDrawables.Length; i++)
                    barDrawables[i].UpdateOffset(bins[i].Sum(b => b.Value));
            }
        }

        private void createBarDrawables()
        {
            int maxCount = bins.Max(b => b.Values.Sum());
            barDrawables = bins.Select((_, i) => new Bar(bins[i], maxCount, i == timing_distribution_centre_bin_index)).ToArray();

            Container axisFlow;

            Padding = new MarginPadding { Horizontal = 5 };

            InternalChild = new GridContainer
            {
                Anchor = Anchor.CentreLeft,
                RelativeSizeAxes = Axes.Both,
                Content = new[]
                {
                    new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Content = new[] { barDrawables }
                        }
                    },
                    new Drawable[]
                    {
                        axisFlow = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 13F
                        }
                    },
                },
                RowDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(GridSizeMode.AutoSize),
                }
            };

            // Our axis will contain one centre element + 5 points on each side, each with a value depending on the number of bins * bin size.
            double maxValue = timing_distribution_bins * binSize;
            double axisValueStep = maxValue / axis_points;

            axisFlow.Add(new SpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.Centre,
                Text = "0",
                Font = FontUsage.Default.With(size: 13F, weight: "SemiBold")
            });

            for (int i = 1; i <= axis_points; i++)
            {
                double axisValue = i * axisValueStep;
                float position = (float)(axisValue / maxValue);
                float alpha = 1f - position * 0.8f;

                axisFlow.Add(new SpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.X,
                    X = position,
                    Alpha = alpha,
                    Text = axisValue.ToString("0"),
                    Font = FontUsage.Default.With(size: 13F, weight: "SemiBold")
                });
            }
        }

        private partial class Bar : CompositeDrawable
        {
            private readonly IReadOnlyList<KeyValuePair<double, int>> values;
            private readonly float maxValue;
            private readonly bool isCentre;
            private readonly float totalValue;

            private const float minimum_height = 0.02f;

            private float offsetAdjustment;

            private Circle[] boxOriginals = null!;

            private Circle? boxAdjustment;

            private float? lastDrawHeight;

            private const double duration = 300;

            public Bar(IDictionary<double, int> values, float maxValue, bool isCentre)
            {
                this.values = values.OrderBy(v => v.Key).ToList();
                this.maxValue = maxValue;
                this.isCentre = isCentre;
                totalValue = values.Sum(v => v.Value);
                offsetAdjustment = totalValue;

                RelativeSizeAxes = Axes.Both;
                Masking = true;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                if (values.Any())
                {
                    boxOriginals = values.Select((v, i) => new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Colour = isCentre && i == 0 ? Color4.White : getTimingColor(v.Key),
                        Height = 0,
                    }).ToArray();
                    // The bars of the stacked bar graph will be processed (stacked) from the bottom, which is the base position,
                    // to the top, and the bottom bar should be drawn more toward the front by design,
                    // while the drawing order is from the back to the front, so the order passed to `InternalChildren` is the opposite.
                    InternalChildren = boxOriginals.Reverse().ToArray();
                }
                else
                {
                    // A bin with no value draws a grey dot instead.
                    InternalChildren = boxOriginals = new[]
                    {
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Both,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.BottomCentre,
                            Colour = isCentre ? Color4.White : Color4.Gray,
                            Height = 0,
                        }
                    };
                }
            }

            readonly Color4 red = Color4Extensions.FromHex(@"ed1121");
            readonly Color4 yellow = Color4Extensions.FromHex(@"ffcc22");
            readonly Color4 greenLight = Color4Extensions.FromHex(@"b3d944");
            readonly Color4 green = Color4Extensions.FromHex(@"88b300");
            readonly Color4 blue = Color4Extensions.FromHex(@"66ccff");

            private Color4 getTimingColor(double millis)
            {
                if (millis < 10)
                    return blue;

                if (millis < 25)
                    return greenLight;

                if (millis < 50)
                    return green;

                if (millis < 80)
                    return yellow;

                return red;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Scheduler.AddOnce(updateMetrics, true);
            }

            protected override bool OnInvalidate(Invalidation invalidation, InvalidationSource source)
            {
                if (invalidation.HasFlagFast(Invalidation.DrawSize))
                {
                    if (lastDrawHeight != null && lastDrawHeight != DrawHeight)
                        Scheduler.AddOnce(updateMetrics, false);
                }

                return base.OnInvalidate(invalidation, source);
            }

            public void UpdateOffset(float adjustment)
            {
                bool hasAdjustment = adjustment != totalValue;

                if (boxAdjustment == null)
                {
                    if (!hasAdjustment)
                        return;

                    AddInternal(boxAdjustment = new Circle
                    {
                        RelativeSizeAxes = Axes.Both,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Colour = Color4.Yellow,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0.6f,
                        Height = 0,
                    });
                }

                offsetAdjustment = adjustment;

                Scheduler.AddOnce(updateMetrics, true);
            }

            private void updateMetrics(bool animate = true)
            {
                float offsetValue = 0;

                for (int i = 0; i < boxOriginals.Length; i++)
                {
                    int value = i < values.Count ? values[i].Value : 0;

                    var box = boxOriginals[i];

                    box.MoveToY(offsetForValue(offsetValue) * BoundingBox.Height, duration, Easing.OutQuint);
                    box.ResizeHeightTo(heightForValue(value), duration, Easing.OutQuint);
                    offsetValue -= value;
                }

                if (boxAdjustment != null)
                    drawAdjustmentBar();

                if (!animate)
                    FinishTransforms(true);

                lastDrawHeight = DrawHeight;
            }

            private void drawAdjustmentBar()
            {
                bool hasAdjustment = offsetAdjustment != totalValue;

                boxAdjustment.ResizeHeightTo(heightForValue(offsetAdjustment), duration, Easing.OutQuint);
                boxAdjustment.FadeTo(!hasAdjustment ? 0 : 1, duration, Easing.OutQuint);
            }

            private float offsetForValue(float value) => (1 - minimum_height) * value / maxValue;

            private float heightForValue(float value) => minimum_height + offsetForValue(value);
        }
    }
}
