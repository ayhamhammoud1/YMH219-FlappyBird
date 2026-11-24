using System.Diagnostics;
using System.Drawing.Drawing2D;

namespace FlappyBird;

public partial class Form1 : Form
{
    private readonly Flight flight = new();
    private readonly FlightView view;
    private readonly System.Windows.Forms.Timer timer = new() { Interval = 16 };
    private readonly Stopwatch clock = Stopwatch.StartNew();
    private bool spaceHeld;

    public Form1()
    {
        InitializeComponent();
        Text = "Gece Postası";
        ClientSize = new Size(1000, 640);
        MinimumSize = new Size(460, 350);
        KeyPreview = true;
        view = new FlightView(flight) { Dock = DockStyle.Fill };
        var bar = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom, Height = 55, Padding = new Padding(12, 7, 0, 0),
            BackColor = Color.FromArgb(30, 39, 68)
        };
        var restart = new Button { Text = "Yeni oyun", AutoSize = true, TabStop = false, BackColor = Color.Moccasin };
        restart.Click += (_, _) => { flight.Reset(); view.Invalidate(); };
        bar.Controls.Add(restart);
        bar.Controls.Add(new Label
        {
            Text = "Boşluk · Kanat çırp", ForeColor = Color.White, AutoSize = true,
            Padding = new Padding(8, 6, 0, 0)
        });
        Controls.Add(view);
        Controls.Add(bar);
        timer.Tick += (_, _) =>
        {
            float seconds = (float)Math.Min(clock.Elapsed.TotalSeconds, .04);
            clock.Restart();
            flight.Step(seconds);
            view.Invalidate();
        };
        timer.Start();
        FormClosed += (_, _) => timer.Dispose();
        Deactivate += (_, _) => spaceHeld = false;
    }

    protected override bool ProcessKeyPreview(ref Message message)
    {
        if ((Keys)(long)message.WParam == Keys.Space)
        {
            if (message.Msg == 0x100)
            {
                if (!spaceHeld) flight.Flap();
                spaceHeld = true;
                return true;
            }
            if (message.Msg == 0x101)
            {
                spaceHeld = false;
                return true;
            }
        }
        return base.ProcessKeyPreview(ref message);
    }
}

internal sealed class FlightView : Panel
{
    private readonly Flight flight;
    internal FlightView(Flight flight)
    {
        this.flight = flight;
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.FromArgb(14, 20, 41);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        if (Width <= 0 || Height <= 0) return;
        var g = e.Graphics;
        float scale = Math.Min(Width / Flight.Width, Height / Flight.Height);
        g.TranslateTransform((Width - Flight.Width * scale) / 2, (Height - Flight.Height * scale) / 2);
        g.ScaleTransform(scale, scale);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.SetClip(new RectangleF(0, 0, Flight.Width, Flight.Height));
        using var sky = new LinearGradientBrush(new Rectangle(0, 0, 800, 450),
            Color.FromArgb(32, 44, 91), Color.FromArgb(108, 74, 115), 90);
        g.FillRectangle(sky, 0, 0, 800, 450);
        g.FillEllipse(Brushes.PeachPuff, 618, 49, 87, 87);
        for (int i = 0; i < 42; i++)
            g.FillEllipse(Brushes.Lavender, i * 97 % 797, i * 73 % 210, 2, 2);
        using var ridge = new SolidBrush(Color.FromArgb(56, 50, 93));
        g.FillPolygon(ridge, [new Point(0, 414), new Point(0, 330), new Point(200, 207),
            new Point(405, 374), new Point(592, 232), new Point(800, 327), new Point(800, 414)]);
        using var pillar = new SolidBrush(Color.FromArgb(35, 79, 100));
        foreach (var gate in flight.Gates)
        {
            float top = gate.Center - Gate.HalfGap;
            float bottom = gate.Center + Gate.HalfGap;
            g.FillRectangle(pillar, gate.X, 0, Gate.Width, top);
            g.FillRectangle(pillar, gate.X, bottom, Gate.Width, Flight.Floor - bottom);
            g.FillRectangle(Brushes.Turquoise, gate.X, top - 5, Gate.Width, 5);
            g.FillRectangle(Brushes.Turquoise, gate.X, bottom, Gate.Width, 5);
        }
        g.FillRectangle(Brushes.MidnightBlue, 0, Flight.Floor, 800, 36);
        g.FillEllipse(Brushes.Gold, Flight.PlayerX - 13, flight.Y - 13, 26, 26);
        g.FillEllipse(Brushes.OrangeRed, Flight.PlayerX - 15, flight.Y + 1, 14, 8);
        g.FillEllipse(Brushes.Black, Flight.PlayerX + 4, flight.Y - 6, 4, 4);
        g.FillPolygon(Brushes.Orange, [new PointF(147, flight.Y - 1), new PointF(157, flight.Y + 2),
            new PointF(147, flight.Y + 5)]);
        using var title = new Font("Segoe UI", 22, FontStyle.Bold, GraphicsUnit.Pixel);
        using var small = new Font("Segoe UI", 17, FontStyle.Regular, GraphicsUnit.Pixel);
        g.DrawString($"Skor  {flight.Score}", title, Brushes.White, 22, 18);
        if (!flight.Running)
        {
            g.FillRectangle(Brushes.MidnightBlue, 218, 159, 364, 111);
            g.DrawString(flight.Crashed ? "Oyun bitti · Game Over" : "Gece Postası", title, Brushes.White, 239, 179);
            g.DrawString(flight.Crashed ? "Yeni oyun ile baştan başla." : "Uçuşa başlamak için boşluğa bas.", small, Brushes.Lavender, 240, 220);
        }
    }
}
