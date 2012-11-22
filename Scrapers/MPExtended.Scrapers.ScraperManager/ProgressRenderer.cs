using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using BrightIdeasSoftware;
using System.Globalization;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace MPExtended.Scrapers.ScraperManager
{
  /// <summary>
  /// Render our Aspect as a progress bar
  /// </summary>
  public class ProgressRenderer : BaseRenderer
  {
    #region Constructors

    /// <summary>
    /// Make a BarRenderer
    /// </summary>
    public ProgressRenderer()
      : base()
    {
    }

    /// <summary>
    /// Make a BarRenderer for the given range of data values
    /// </summary>
    public ProgressRenderer(int minimum, int maximum)
      : this()
    {
      this.MinimumValue = minimum;
      this.MaximumValue = maximum;
    }

    /// <summary>
    /// Make a BarRenderer using a custom bar scheme
    /// </summary>
    public ProgressRenderer(Pen pen, Brush brush)
      : this()
    {
      this.Pen = pen;
      this.Brush = brush;
      this.UseStandardBar = false;
    }

    /// <summary>
    /// Make a BarRenderer using a custom bar scheme
    /// </summary>
    public ProgressRenderer(int minimum, int maximum, Pen pen, Brush brush)
      : this(minimum, maximum)
    {
      this.Pen = pen;
      this.Brush = brush;
      this.UseStandardBar = false;
    }

    /// <summary>
    /// Make a BarRenderer that uses a horizontal gradient
    /// </summary>
    public ProgressRenderer(Pen pen, Color start, Color end)
      : this()
    {
      this.Pen = pen;
      this.SetGradient(start, end);
    }

    /// <summary>
    /// Make a BarRenderer that uses a horizontal gradient
    /// </summary>
    public ProgressRenderer(int minimum, int maximum, Pen pen, Color start, Color end)
      : this(minimum, maximum)
    {
      this.Pen = pen;
      this.SetGradient(start, end);
    }

    #endregion

    #region Configuration Properties

    /// <summary>
    /// Should this bar be drawn in the system style?
    /// </summary>
    [Category("Appearance - ObjectListView"),
     Description("Should this bar be drawn in the system style?"),
     DefaultValue(true)]
    public bool UseStandardBar
    {
      get { return useStandardBar; }
      set { useStandardBar = value; }
    }
    private bool useStandardBar = true;

    /// <summary>
    /// How many pixels in from our cell border will this bar be drawn
    /// </summary>
    [Category("Appearance - ObjectListView"),
     Description("How many pixels in from our cell border will this bar be drawn"),
     DefaultValue(2)]
    public int Padding
    {
      get { return padding; }
      set { padding = value; }
    }
    private int padding = 2;

    /// <summary>
    /// What color will be used to fill the interior of the control before the 
    /// progress bar is drawn?
    /// </summary>
    [Category("Appearance - ObjectListView"),
     Description("The color of the interior of the bar"),
     DefaultValue(typeof(Color), "AliceBlue")]
    public Color BackgroundColor
    {
      get { return backgroundColor; }
      set { backgroundColor = value; }
    }
    private Color backgroundColor = Color.AliceBlue;

    /// <summary>
    /// What color should the frame of the progress bar be?
    /// </summary>
    [Category("Appearance - ObjectListView"),
     Description("What color should the frame of the progress bar be"),
     DefaultValue(typeof(Color), "Black")]
    public Color FrameColor
    {
      get { return frameColor; }
      set { frameColor = value; }
    }
    private Color frameColor = Color.Black;

    /// <summary>
    /// How many pixels wide should the frame of the progress bar be?
    /// </summary>
    [Category("Appearance - ObjectListView"),
     Description("How many pixels wide should the frame of the progress bar be"),
     DefaultValue(1.0f)]
    public float FrameWidth
    {
      get { return frameWidth; }
      set { frameWidth = value; }
    }
    private float frameWidth = 1.0f;

    /// <summary>
    /// What color should the 'filled in' part of the progress bar be?
    /// </summary>
    /// <remarks>This is only used if GradientStartColor is Color.Empty</remarks>
    [Category("Appearance - ObjectListView"),
     Description("What color should the 'filled in' part of the progress bar be"),
     DefaultValue(typeof(Color), "BlueViolet")]
    public Color FillColor
    {
      get { return fillColor; }
      set { fillColor = value; }
    }
    private Color fillColor = Color.BlueViolet;

    /// <summary>
    /// Use a gradient to fill the progress bar starting with this color
    /// </summary>
    [Category("Appearance - ObjectListView"),
     Description("Use a gradient to fill the progress bar starting with this color"),
     DefaultValue(typeof(Color), "CornflowerBlue")]
    public Color GradientStartColor
    {
      get { return startColor; }
      set
      {
        startColor = value;
      }
    }
    private Color startColor = Color.CornflowerBlue;

    /// <summary>
    /// Use a gradient to fill the progress bar ending with this color
    /// </summary>
    [Category("Appearance - ObjectListView"),
     Description("Use a gradient to fill the progress bar ending with this color"),
     DefaultValue(typeof(Color), "DarkBlue")]
    public Color GradientEndColor
    {
      get { return endColor; }
      set
      {
        endColor = value;
      }
    }
    private Color endColor = Color.DarkBlue;

    /// <summary>
    /// Regardless of how wide the column become the progress bar will never be wider than this
    /// </summary>
    [Category("Behavior"),
    Description("The progress bar will never be wider than this"),
    DefaultValue(100)]
    public int MaximumWidth
    {
      get { return maximumWidth; }
      set { maximumWidth = value; }
    }
    private int maximumWidth = 100;

    /// <summary>
    /// Regardless of how high the cell is  the progress bar will never be taller than this
    /// </summary>
    [Category("Behavior"),
    Description("The progress bar will never be taller than this"),
    DefaultValue(16)]
    public int MaximumHeight
    {
      get { return maximumHeight; }
      set { maximumHeight = value; }
    }
    private int maximumHeight = 16;

    /// <summary>
    /// The minimum data value expected. Values less than this will given an empty bar
    /// </summary>
    [Category("Behavior"),
    Description("The minimum data value expected. Values less than this will given an empty bar"),
    DefaultValue(0.0)]
    public double MinimumValue
    {
      get { return minimumValue; }
      set { minimumValue = value; }
    }
    private double minimumValue = 0.0;

    /// <summary>
    /// The maximum value for the range. Values greater than this will give a full bar
    /// </summary>
    [Category("Behavior"),
    Description("The maximum value for the range. Values greater than this will give a full bar"),
    DefaultValue(100.0)]
    public double MaximumValue
    {
      get { return maximumValue; }
      set { maximumValue = value; }
    }
    private double maximumValue = 100.0;

    #endregion

    #region Public Properties (non-IDE)

    /// <summary>
    /// The Pen that will draw the frame surrounding this bar
    /// </summary>
    [Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Pen Pen
    {
      get
      {
        if (this.pen == null && !this.FrameColor.IsEmpty)
          return new Pen(this.FrameColor, this.FrameWidth);
        else
          return this.pen;
      }
      set
      {
        this.pen = value;
      }
    }
    private Pen pen;

    /// <summary>
    /// The brush that will be used to fill the bar
    /// </summary>
    [Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Brush Brush
    {
      get
      {
        if (this.brush == null && !this.FillColor.IsEmpty)
          return new SolidBrush(this.FillColor);
        else
          return this.brush;
      }
      set
      {
        this.brush = value;
      }
    }
    private Brush brush;

    /// <summary>
    /// The brush that will be used to fill the background of the bar
    /// </summary>
    [Browsable(false),
    DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Brush BackgroundBrush
    {
      get
      {
        if (this.backgroundBrush == null && !this.BackgroundColor.IsEmpty)
          return new SolidBrush(this.BackgroundColor);
        else
          return this.backgroundBrush;
      }
      set
      {
        this.backgroundBrush = value;
      }
    }
    private Brush backgroundBrush;

    #endregion

    /// <summary>
    /// Draw this progress bar using a gradient
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    public void SetGradient(Color start, Color end)
    {
      this.GradientStartColor = start;
      this.GradientEndColor = end;
    }

    /// <summary>
    /// Draw our aspect
    /// </summary>
    /// <param name="g"></param>
    /// <param name="r"></param>
    public override void Render(Graphics g, Rectangle r)
    {
      this.DrawBackground(g, r);

      Rectangle frameRect = Rectangle.Inflate(r, 0 - this.Padding, 0 - this.Padding);
      frameRect.Width = Math.Min(frameRect.Width, this.MaximumWidth);
      frameRect.Height = Math.Min(frameRect.Height, this.MaximumHeight);
      frameRect = this.AlignRectangle(r, frameRect);

      // Convert our aspect to a numeric value
      IConvertible convertable = this.Aspect as IConvertible;
      if (convertable == null)
        return;

      if (convertable.GetType() == typeof(String))
      {
        g.DrawString(convertable.ToString(), new Font(FontFamily.GenericSerif, 8), new SolidBrush(Color.Black), frameRect);
      }
      else
      {
        double aspectValue = convertable.ToDouble(NumberFormatInfo.InvariantInfo);

        Rectangle fillRect = Rectangle.Inflate(frameRect, -1, -1);
        if (aspectValue <= this.MinimumValue)
          fillRect.Width = 0;
        else if (aspectValue < this.MaximumValue)
          fillRect.Width = (int)(fillRect.Width * (aspectValue - this.MinimumValue) / this.MaximumValue);

        // MS-themed progress bars don't work when printing
        if (this.UseStandardBar && ProgressBarRenderer.IsSupported && !this.IsPrinting)
        {
          ProgressBarRenderer.DrawHorizontalBar(g, frameRect);
          ProgressBarRenderer.DrawHorizontalChunks(g, fillRect);
        }
        else
        {
          g.FillRectangle(this.BackgroundBrush, frameRect);
          if (fillRect.Width > 0)
          {
            // FillRectangle fills inside the given rectangle, so expand it a little
            fillRect.Width++;
            fillRect.Height++;
            if (this.GradientStartColor == Color.Empty)
              g.FillRectangle(this.Brush, fillRect);
            else
            {
              using (LinearGradientBrush gradient = new LinearGradientBrush(frameRect, this.GradientStartColor, this.GradientEndColor, LinearGradientMode.Horizontal))
              {
                g.FillRectangle(gradient, fillRect);
              }
            }
          }
          g.DrawRectangle(this.Pen, frameRect);
        }
      }
    }
  }
}
