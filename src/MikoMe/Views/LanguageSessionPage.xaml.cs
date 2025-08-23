using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;
using MikoMe.Models;
using MikoMe.ViewModels;
using Windows.Foundation; // for Point and Size

namespace MikoMe.Views
{
    public sealed partial class LanguageSessionPage : Page
    {
        public SessionViewModel VM => (SessionViewModel)DataContext;

        public LanguageSessionPage()
        {
            this.InitializeComponent();
            this.Loaded += LanguageSessionPage_Loaded;
        }

        private async void LanguageSessionPage_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new SessionViewModel();
            DataContext = vm;
            await vm.InitAsync(CardDirection.ZhToEn);

            UpdateClock(); // draw initial clock
            vm.OnVisibilityChanged += UpdateClock; // hook updates
        }

        private void UpdateClock()
        {
            var parts = System.Text.RegularExpressions.Regex.Matches(VM.ProgressText ?? string.Empty, @"\d+")
                .Select(m => int.Parse(m.Value))
                .ToArray();

            if (parts.Length >= 2 && parts[1] > 0)
            {
                int done = parts[0];
                int total = parts[1];
                double ratio = Math.Clamp(done / (double)total, 0.0, 1.0);

                RingText.Text = $"{done}/{total}";
                SetArc(ratio);
            }
            else
            {
                RingText.Text = VM.ProgressText ?? string.Empty;
                SetArc(0);
            }
        }

        private void SetArc(double ratio)
        {
            const double width = 120;
            const double height = 120;
            const double strokeInset = 3;

            double r = (width / 2) - strokeInset;
            double cx = width / 2;
            double cy = height / 2;

            const double startAngle = -90;
            double sweep = 360 * Math.Clamp(ratio, 0, 1);
            double endAngle = startAngle + sweep;

            Point start = new(cx, cy - r);
            Point end = new(
                cx + r * Math.Cos(endAngle * Math.PI / 180.0),
                cy + r * Math.Sin(endAngle * Math.PI / 180.0));

            var fig = new PathFigure { StartPoint = start, IsClosed = false };
            fig.Segments.Add(new ArcSegment
            {
                Size = new Size(r, r),
                Point = end,
                IsLargeArc = sweep >= 180,
                SweepDirection = SweepDirection.Clockwise
            });

            var geo = new PathGeometry();
            geo.Figures.Add(fig);
            ProgressArc.Data = geo;
        }
    }
}
