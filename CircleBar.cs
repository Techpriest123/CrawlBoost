using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Layout;
using System;

namespace CrawlBoost
{
    public class CircleBar : Control
    {
        public static readonly StyledProperty<double> ValueProperty =
            AvaloniaProperty.Register<CircleBar, double>(nameof(Value), 0.0);

        public static readonly StyledProperty<double> MinimumProperty =
            AvaloniaProperty.Register<CircleBar, double>(nameof(Minimum), 0.0);

        public static readonly StyledProperty<double> MaximumProperty =
            AvaloniaProperty.Register<CircleBar, double>(nameof(Maximum), 100.0);

        public static readonly StyledProperty<IBrush> IndicatorBrushProperty =
            AvaloniaProperty.Register<CircleBar, IBrush>(nameof(IndicatorBrush), Brushes.Green);

        public static readonly StyledProperty<IBrush> BackgroundBrushProperty =
            AvaloniaProperty.Register<CircleBar, IBrush>(nameof(BackgroundBrush), Brushes.LightGray);

        public static readonly StyledProperty<double> LineThicknessProperty =
            AvaloniaProperty.Register<CircleBar, double>(nameof(LineThickness), 8.0);

        public double Value
        {
            get => GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double Minimum
        {
            get => GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }

        public double Maximum
        {
            get => GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public IBrush IndicatorBrush
        {
            get => GetValue(IndicatorBrushProperty);
            set => SetValue(IndicatorBrushProperty, value);
        }

        public IBrush BackgroundBrush
        {
            get => GetValue(BackgroundBrushProperty);
            set => SetValue(BackgroundBrushProperty, value);
        }

        public double LineThickness
        {
            get => GetValue(LineThicknessProperty);
            set => SetValue(LineThicknessProperty, value);
        }

        static CircleBar()
        {
            AffectsRender<CircleBar>(ValueProperty, MinimumProperty, MaximumProperty,
                IndicatorBrushProperty, BackgroundBrushProperty, LineThicknessProperty);
        }

        public override void Render(DrawingContext context)
        {
            base.Render(context);

            var bounds = Bounds;
            var center = new Point(bounds.Width / 2, bounds.Height / 2);
            var radius = Math.Min(bounds.Width, bounds.Height) / 2 - LineThickness / 2;

            var backgroundPen = new Pen(BackgroundBrush, LineThickness);
            context.DrawEllipse(null, backgroundPen, center, radius, radius);

            var percentage = Math.Max(0, Math.Min(1, (Value - Minimum) / (Maximum - Minimum)));
            var angle = 360 * percentage;

            if (angle > 0)
            {
                var progressPen = new Pen(IndicatorBrush, LineThickness);

                var geometry = BuildArcGeometry(center, radius, 0, angle);
                context.DrawGeometry(null, progressPen, geometry);
            }
        }

        private StreamGeometry BuildArcGeometry(Point center, double radius, double startAngle, double sweepAngle)
        {
            var geometry = new StreamGeometry();

            using (var ctx = geometry.Open())
            {
                var startAngleRad = (Math.PI / 180.0) * (startAngle - 90);
                var sweepAngleRad = (Math.PI / 180.0) * sweepAngle;

                sweepAngleRad = Math.Min(sweepAngleRad, 2 * Math.PI - 0.01);

                var startPoint = new Point(
                    center.X + radius * Math.Cos(startAngleRad),
                    center.Y + radius * Math.Sin(startAngleRad));

                var endPoint = new Point(
                    center.X + radius * Math.Cos(startAngleRad + sweepAngleRad),
                    center.Y + radius * Math.Sin(startAngleRad + sweepAngleRad));

                ctx.BeginFigure(startPoint, false);
                ctx.ArcTo(endPoint, new Size(radius, radius), 0, sweepAngle > 180, SweepDirection.Clockwise);
            }

            return geometry;
        }
    }
}