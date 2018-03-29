using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

using FontFactory = SharpDX.DirectWrite.Factory;
using Factory = SharpDX.Direct2D1.Factory;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using replayParse;
using System.Threading;
using System.Collections.Generic;

namespace Yato.DirectXOverlay
{
    public class Direct2DRenderer : IDisposable
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern IntPtr GetDesktopWindow();


        #region private vars

        private Direct2DRendererOptions rendererOptions;

        private WindowRenderTarget device;
        private HwndRenderTargetProperties deviceProperties;

        private FontFactory fontFactory;
        private Factory factory;

        private SolidColorBrush sharedBrush;
        private TextFormat sharedFont;

        private bool isDrawing;

        private bool resize;
        private int resizeWidth;
        private int resizeHeight;

        private Stopwatch stopwatch = new Stopwatch();

        private int internalFps;
        private bool low_hp = false;
        /* 1 ~ 5 refer to which suggesed hero is picked by own team
         * -1 ~ -5 refer to which suggesd hero is banned by other team
         * 0 refer to nothing happened
         */
        private int ban_and_pick = 0;

        // Type:
        // 0: hero selection
        // 1: hero selection
        // 2: hero selection
        // 3: hero selection
        // 4: hero selection
        // 5: hero introduction
        // 6: items selection
        // 7: retreat
        // 8: press on
        // 9: last hit
        // 10: jungle
        // 11: safe farming
        // 12: hero information
        private Message[] messages = new Message[13];

        private HeroSuggestion HeroSugg = new HeroSuggestion();

        // Contains information about hero health
        private double[] hps = new double[5];

        // Determins if graphs should be drawn
        private bool drawGraphs = false;

        private Queue<double> currHp = new Queue<double>(250);

        private List<int> heroIds;
        private Dictionary<int, List<Tuple<String, String, String>>> ticksInfo;

        private bool drawHighlight = false;
        
        
        private float width_unit = Screen.PrimaryScreen.Bounds.Width / 32;

        private float height_unit = Screen.PrimaryScreen.Bounds.Height / 32;

        private float screen_width = Screen.PrimaryScreen.Bounds.Width;

        private float screen_height = Screen.PrimaryScreen.Bounds.Height;
        
        static public float size_scale = Screen.PrimaryScreen.Bounds.Height / 1080f;
        private float maxTick;

        #endregion

        #region public vars

        public IntPtr RenderTargetHwnd { get; private set; }
        public bool VSync { get; private set; }
        public int FPS { get; private set; }

        public bool MeasureFPS { get; set; }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public Direct2DBrush whiteSmoke { get; set; }
        public Direct2DBrush blackBrush { get; set; }
        public Direct2DBrush redBrush { get; set; }
        public Direct2DBrush lightRedBrush { get; set; }
        public Direct2DBrush greenBrush { get; set; }
        public Direct2DBrush blueBrush { get; set; }
        public Direct2DFont font { get; set; }
        public enum hints : int
        {
            hero_selection_1 = 0,
            hero_selection_2 = 1,
            hero_selection_3 = 2,
            hero_selection_4 = 3,
            hero_selection_5 = 4,

            hero_introduction = 5,

            items_selection = 6,
            retreat = 7,
            press_on = 8,
            last_hit = 9,
            jungle = 10,
            safe_farming = 11,
            heroinformation = 12,
        };

        #endregion

        #region construct & destruct

        private Direct2DRenderer()
        {
            throw new NotSupportedException();
        }

        public Direct2DRenderer(IntPtr hwnd)
        {
            var options = new Direct2DRendererOptions()
            {
                Hwnd = hwnd,
                VSync = false,
                MeasureFps = false,
                AntiAliasing = false
            };
            setupInstance(options);
        }

        public Direct2DRenderer(IntPtr hwnd, bool vsync)
        {
            var options = new Direct2DRendererOptions()
            {
                Hwnd = hwnd,
                VSync = vsync,
                MeasureFps = false,
                AntiAliasing = false
            };
            setupInstance(options);
        }

        public Direct2DRenderer(IntPtr hwnd, bool vsync, bool measureFps)
        {
            var options = new Direct2DRendererOptions()
            {
                Hwnd = hwnd,
                VSync = vsync,
                MeasureFps = measureFps,
                AntiAliasing = false
            };
            setupInstance(options);
        }

        public Direct2DRenderer(IntPtr hwnd, bool vsync, bool measureFps, bool antiAliasing)
        {
            var options = new Direct2DRendererOptions()
            {
                Hwnd = hwnd,
                VSync = vsync,
                MeasureFps = measureFps,
                AntiAliasing = antiAliasing
            };
            setupInstance(options);
        }

        public Direct2DRenderer(Direct2DRendererOptions options)
        {
            setupInstance(options);
        }

        ~Direct2DRenderer()
        {
            Dispose(false);
        }

        #endregion

        #region init & delete

        private void setupInstance(Direct2DRendererOptions options)
        {
            rendererOptions = options;

            if (options.Hwnd == IntPtr.Zero) throw new ArgumentNullException(nameof(options.Hwnd));

            if (PInvoke.IsWindow(options.Hwnd) == 0) throw new ArgumentException("The window does not exist (hwnd = 0x" + options.Hwnd.ToString("X") + ")");

            PInvoke.RECT bounds = new PInvoke.RECT();

            if (PInvoke.GetRealWindowRect(options.Hwnd, out bounds) == 0) throw new Exception("Failed to get the size of the given window (hwnd = 0x" + options.Hwnd.ToString("X") + ")");

            this.Width = bounds.Right - bounds.Left;
            this.Height = bounds.Bottom - bounds.Top;

            this.VSync = options.VSync;
            this.MeasureFPS = options.MeasureFps;

            deviceProperties = new HwndRenderTargetProperties()
            {
                Hwnd = options.Hwnd,
                PixelSize = new Size2(this.Width, this.Height),
                PresentOptions = options.VSync ? PresentOptions.None : PresentOptions.Immediately
            };

            var renderProperties = new RenderTargetProperties(
                RenderTargetType.Default,
                new PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied),
                96.0f, 96.0f, // May need to change window and render targets dpi according to windows. but this seems to fix it at least for me (looks better somehow)
                RenderTargetUsage.None,
                FeatureLevel.Level_DEFAULT);

            factory = new Factory();
            fontFactory = new FontFactory();

            device = new WindowRenderTarget(factory, renderProperties, deviceProperties);

            device.AntialiasMode = AntialiasMode.Aliased; // AntialiasMode.PerPrimitive fails rendering some objects
            // other than in the documentation: Cleartype is much faster for me than GrayScale
            device.TextAntialiasMode = options.AntiAliasing ? SharpDX.Direct2D1.TextAntialiasMode.Cleartype : SharpDX.Direct2D1.TextAntialiasMode.Aliased;

            sharedBrush = new SolidColorBrush(device, default(RawColor4));
        }

        private void deleteInstance()
        {
            try
            {
                sharedBrush.Dispose();
                fontFactory.Dispose();
                factory.Dispose();
                device.Dispose();
            }
            catch
            {

            }
        }

        #endregion

        #region Scenes

        public void Resize(int width, int height)
        {
            resizeWidth = width;
            resizeHeight = height;
            resize = true;
        }

        public void BeginScene()
        {
            if (device == null) return;
            if (isDrawing) return;

            if (MeasureFPS && !stopwatch.IsRunning)
            {
                stopwatch.Restart();
            }

            if (resize)
            {
                device.Resize(new Size2(resizeWidth, resizeHeight));
                resize = false;
            }

            device.BeginDraw();

            isDrawing = true;
        }

        public Direct2DScene UseScene()
        {
            // really expensive to use but i like the pattern
            return new Direct2DScene(this);
        }

        public void EndScene()
        {
            if (device == null) return;
            if (!isDrawing) return;

            long tag_0 = 0L, tag_1 = 0L;
            
            Result result;
            try
            {
                result = device.TryEndDraw(out tag_0, out tag_1);
            }
            catch (System.ArgumentOutOfRangeException e) { return; }
            catch (System.InvalidCastException e) { return; }

            if (result.Failure)
            {
                deleteInstance();
                setupInstance(rendererOptions);
            }

            if (MeasureFPS && stopwatch.IsRunning)
            {
                internalFps++;

                if (stopwatch.ElapsedMilliseconds > 1000)
                {
                    FPS = internalFps;
                    internalFps = 0;
                    stopwatch.Stop();
                }
            }

            isDrawing = false;
        }

        public void ClearScene()
        {
            device.Clear(null);
        }

        public void ClearScene(Direct2DColor color)
        {
            device.Clear(color);
        }

        public void ClearScene(Direct2DBrush brush)
        {
            device.Clear(brush);
        }

        #endregion

        #region Fonts & Brushes & Bitmaps

        public void SetSharedFont(string fontFamilyName, float size, bool bold = false, bool italic = false)
        {
            sharedFont = new TextFormat(fontFactory, fontFamilyName, bold ? FontWeight.Bold : FontWeight.Normal, italic ? FontStyle.Italic : FontStyle.Normal, size);
            sharedFont.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
        }

        public Direct2DBrush CreateBrush(Direct2DColor color)
        {
            return new Direct2DBrush(device, color);
        }

        public Direct2DBrush CreateBrush(int r, int g, int b, int a = 255)
        {
            return new Direct2DBrush(device, new Direct2DColor(r, g, b, a));
        }

        public Direct2DBrush CreateBrush(float r, float g, float b, float a = 1.0f)
        {
            return new Direct2DBrush(device, new Direct2DColor(r, g, b, a));
        }

        public Direct2DFont CreateFont(string fontFamilyName, float size, bool bold = false, bool italic = false)
        {
            return new Direct2DFont(fontFactory, fontFamilyName, size, bold, italic);
        }

        public Direct2DFont CreateFont(Direct2DFontCreationOptions options)
        {
            TextFormat font = new TextFormat(fontFactory, options.FontFamilyName, options.Bold ? FontWeight.Bold : FontWeight.Normal, options.GetStyle(), options.FontSize);
            font.WordWrapping = options.WordWrapping ? WordWrapping.Wrap : WordWrapping.NoWrap;
            return new Direct2DFont(font);
        }

        public Direct2DBitmap LoadBitmap(string file)
        {
            return new Direct2DBitmap(device, file);
        }

        public Direct2DBitmap LoadBitmap(byte[] bytes)
        {
            return new Direct2DBitmap(device, bytes);
        }

        #endregion

        #region Primitives

        public void DrawLine(float start_x, float start_y, float end_x, float end_y, float stroke, Direct2DBrush brush)
        {
            device.DrawLine(new RawVector2(start_x, start_y), new RawVector2(end_x, end_y), brush, stroke);
        }

        public void DrawLine(float start_x, float start_y, float end_x, float end_y, float stroke, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.DrawLine(new RawVector2(start_x, start_y), new RawVector2(end_x, end_y), sharedBrush, stroke);
        }

        public void DrawRectangle(float x, float y, float width, float height, float stroke, Direct2DBrush brush)
        {
            device.DrawRectangle(new RawRectangleF(x, y, x + width, y + height), brush, stroke);
        }

        public void DrawRectangle(float x, float y, float width, float height, float stroke, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.DrawRectangle(new RawRectangleF(x, y, x + width, y + height), sharedBrush, stroke);
        }

        public void DrawRectangleEdges(float x, float y, float width, float height, float stroke, Direct2DBrush brush)
        {
            int length = (int)(((width + height) / 2.0f) * 0.2f);

            RawVector2 first = new RawVector2(x, y);
            RawVector2 second = new RawVector2(x, y + length);
            RawVector2 third = new RawVector2(x + length, y);

            device.DrawLine(first, second, brush, stroke);
            device.DrawLine(first, third, brush, stroke);

            first.Y += height;
            second.Y = first.Y - length;
            third.Y = first.Y;
            third.X = first.X + length;

            device.DrawLine(first, second, brush, stroke);
            device.DrawLine(first, third, brush, stroke);

            first.X = x + width;
            first.Y = y;
            second.X = first.X - length;
            second.Y = first.Y;
            third.X = first.X;
            third.Y = first.Y + length;

            device.DrawLine(first, second, brush, stroke);
            device.DrawLine(first, third, brush, stroke);

            first.Y += height;
            second.X += length;
            second.Y = first.Y - length;
            third.Y = first.Y;
            third.X = first.X - length;

            device.DrawLine(first, second, brush, stroke);
            device.DrawLine(first, third, brush, stroke);
        }

        public void DrawRectangleEdges(float x, float y, float width, float height, float stroke, Direct2DColor color)
        {
            sharedBrush.Color = color;

            int length = (int)(((width + height) / 2.0f) * 0.2f);

            RawVector2 first = new RawVector2(x, y);
            RawVector2 second = new RawVector2(x, y + length);
            RawVector2 third = new RawVector2(x + length, y);

            device.DrawLine(first, second, sharedBrush, stroke);
            device.DrawLine(first, third, sharedBrush, stroke);

            first.Y += height;
            second.Y = first.Y - length;
            third.Y = first.Y;
            third.X = first.X + length;

            device.DrawLine(first, second, sharedBrush, stroke);
            device.DrawLine(first, third, sharedBrush, stroke);

            first.X = x + width;
            first.Y = y;
            second.X = first.X - length;
            second.Y = first.Y;
            third.X = first.X;
            third.Y = first.Y + length;

            device.DrawLine(first, second, sharedBrush, stroke);
            device.DrawLine(first, third, sharedBrush, stroke);

            first.Y += height;
            second.X += length;
            second.Y = first.Y - length;
            third.Y = first.Y;
            third.X = first.X - length;

            device.DrawLine(first, second, sharedBrush, stroke);
            device.DrawLine(first, third, sharedBrush, stroke);
        }

        public void DrawCircle(float x, float y, float radius, float stroke, Direct2DBrush brush)
        {
            device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius, radius), brush, stroke);
        }

        public void DrawCircle(float x, float y, float radius, float stroke, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius, radius), sharedBrush, stroke);
        }

        public void DrawEllipse(float x, float y, float radius_x, float radius_y, float stroke, Direct2DBrush brush)
        {
            device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius_x, radius_y), brush, stroke);
        }

        public void DrawEllipse(float x, float y, float radius_x, float radius_y, float stroke, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.DrawEllipse(new Ellipse(new RawVector2(x, y), radius_x, radius_y), sharedBrush, stroke);
        }

        #endregion

        #region Filled

        public void FillRectangle(float x, float y, float width, float height, Direct2DBrush brush)
        {
            device.FillRectangle(new RawRectangleF(x, y, x + width, y + height), brush);
        }

        public void FillRectangle(float x, float y, float width, float height, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.FillRectangle(new RawRectangleF(x, y, x + width, y + height), sharedBrush);
        }

        public void FillCircle(float x, float y, float radius, Direct2DBrush brush)
        {
            device.FillEllipse(new Ellipse(new RawVector2(x, y), radius, radius), brush);
        }

        public void FillCircle(float x, float y, float radius, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.FillEllipse(new Ellipse(new RawVector2(x, y), radius, radius), sharedBrush);
        }

        public void FillEllipse(float x, float y, float radius_x, float radius_y, Direct2DBrush brush)
        {
            device.FillEllipse(new Ellipse(new RawVector2(x, y), radius_x, radius_y), brush);
        }

        public void FillEllipse(float x, float y, float radius_x, float radius_y, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.FillEllipse(new Ellipse(new RawVector2(x, y), radius_x, radius_y), sharedBrush);
        }

        #endregion

        #region Bordered

        public void BorderedLine(float start_x, float start_y, float end_x, float end_y, float stroke, Direct2DColor color, Direct2DColor borderColor)
        {
            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            float half = stroke / 2.0f;
            float quarter = half / 2.0f;

            sink.BeginFigure(new RawVector2(start_x, start_y - half), FigureBegin.Filled);

            sink.AddLine(new RawVector2(end_x, end_y - half));
            sink.AddLine(new RawVector2(end_x, end_y + half));
            sink.AddLine(new RawVector2(start_x, start_y + half));

            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            sharedBrush.Color = borderColor;

            device.DrawGeometry(geometry, sharedBrush, half);

            sharedBrush.Color = color;

            device.FillGeometry(geometry, sharedBrush);

            sink.Dispose();
            geometry.Dispose();
        }

        public void BorderedLine(float start_x, float start_y, float end_x, float end_y, float stroke, Direct2DBrush brush, Direct2DBrush borderBrush)
        {
            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            float half = stroke / 2.0f;
            float quarter = half / 2.0f;

            sink.BeginFigure(new RawVector2(start_x, start_y - half), FigureBegin.Filled);

            sink.AddLine(new RawVector2(end_x, end_y - half));
            sink.AddLine(new RawVector2(end_x, end_y + half));
            sink.AddLine(new RawVector2(start_x, start_y + half));

            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            device.DrawGeometry(geometry, borderBrush, half);

            device.FillGeometry(geometry, brush);

            sink.Dispose();
            geometry.Dispose();
        }

        public void BorderedRectangle(float x, float y, float width, float height, float stroke, Direct2DColor color, Direct2DColor borderColor)
        {
            float half = stroke / 2.0f;

            width += x;
            height += y;

            sharedBrush.Color = color;

            device.DrawRectangle(new RawRectangleF(x, y, width, height), sharedBrush, half);

            sharedBrush.Color = borderColor;

            device.DrawRectangle(new RawRectangleF(x - half, y - half, width + half, height + half), sharedBrush, half);

            device.DrawRectangle(new RawRectangleF(x + half, y + half, width - half, height - half), sharedBrush, half);
        }

        public void BorderedRectangle(float x, float y, float width, float height, float stroke, Direct2DBrush brush, Direct2DBrush borderBrush)
        {
            float half = stroke / 2.0f;

            width += x;
            height += y;

            device.DrawRectangle(new RawRectangleF(x - half, y - half, width + half, height + half), borderBrush, half);

            device.DrawRectangle(new RawRectangleF(x + half, y + half, width - half, height - half), borderBrush, half);

            device.DrawRectangle(new RawRectangleF(x, y, width, height), brush, half);
        }

        public void BorderedCircle(float x, float y, float radius, float stroke, Direct2DColor color, Direct2DColor borderColor)
        {
            sharedBrush.Color = color;

            var ellipse = new Ellipse(new RawVector2(x, y), radius, radius);

            device.DrawEllipse(ellipse, sharedBrush, stroke);

            float half = stroke / 2.0f;

            sharedBrush.Color = borderColor;

            ellipse.RadiusX += half;
            ellipse.RadiusY += half;

            device.DrawEllipse(ellipse, sharedBrush, half);

            ellipse.RadiusX -= stroke;
            ellipse.RadiusY -= stroke;

            device.DrawEllipse(ellipse, sharedBrush, half);
        }

        public void BorderedCircle(float x, float y, float radius, float stroke, Direct2DBrush brush, Direct2DBrush borderBrush)
        {
            var ellipse = new Ellipse(new RawVector2(x, y), radius, radius);

            device.DrawEllipse(ellipse, brush, stroke);

            float half = stroke / 2.0f;

            ellipse.RadiusX += half;
            ellipse.RadiusY += half;

            device.DrawEllipse(ellipse, borderBrush, half);

            ellipse.RadiusX -= stroke;
            ellipse.RadiusY -= stroke;

            device.DrawEllipse(ellipse, borderBrush, half);
        }

        #endregion

        #region Geometry

        public void DrawTriangle(float a_x, float a_y, float b_x, float b_y, float c_x, float c_y, float stroke, Direct2DBrush brush)
        {
            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            sink.BeginFigure(new RawVector2(a_x, a_y), FigureBegin.Hollow);
            sink.AddLine(new RawVector2(b_x, b_y));
            sink.AddLine(new RawVector2(c_x, c_y));
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            device.DrawGeometry(geometry, brush, stroke);

            sink.Dispose();
            geometry.Dispose();
        }

        public void DrawTriangle(float a_x, float a_y, float b_x, float b_y, float c_x, float c_y, float stroke, Direct2DColor color)
        {
            sharedBrush.Color = color;

            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            sink.BeginFigure(new RawVector2(a_x, a_y), FigureBegin.Hollow);
            sink.AddLine(new RawVector2(b_x, b_y));
            sink.AddLine(new RawVector2(c_x, c_y));
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            device.DrawGeometry(geometry, sharedBrush, stroke);

            sink.Dispose();
            geometry.Dispose();
        }

        public void FillTriangle(float a_x, float a_y, float b_x, float b_y, float c_x, float c_y, Direct2DBrush brush)
        {
            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            sink.BeginFigure(new RawVector2(a_x, a_y), FigureBegin.Filled);
            sink.AddLine(new RawVector2(b_x, b_y));
            sink.AddLine(new RawVector2(c_x, c_y));
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            device.FillGeometry(geometry, brush);

            sink.Dispose();
            geometry.Dispose();
        }

        public void FillTriangle(float a_x, float a_y, float b_x, float b_y, float c_x, float c_y, Direct2DColor color)
        {
            sharedBrush.Color = color;

            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            sink.BeginFigure(new RawVector2(a_x, a_y), FigureBegin.Filled);
            sink.AddLine(new RawVector2(b_x, b_y));
            sink.AddLine(new RawVector2(c_x, c_y));
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            device.FillGeometry(geometry, sharedBrush);

            sink.Dispose();
            geometry.Dispose();
        }

        #endregion

        #region Special

        public void DrawBox2D(float x, float y, float width, float height, float stroke, Direct2DColor interiorColor, Direct2DColor color)
        {
            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            sink.BeginFigure(new RawVector2(x, y), FigureBegin.Filled);
            sink.AddLine(new RawVector2(x + width, y));
            sink.AddLine(new RawVector2(x + width, y + height));
            sink.AddLine(new RawVector2(x, y + height));
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            sharedBrush.Color = color;

            device.DrawGeometry(geometry, sharedBrush, stroke);

            sharedBrush.Color = interiorColor;

            device.FillGeometry(geometry, sharedBrush);

            sink.Dispose();
            geometry.Dispose();
        }

        public void DrawBox2D(float x, float y, float width, float height, float stroke, Direct2DBrush interiorBrush, Direct2DBrush brush)
        {
            var geometry = new PathGeometry(factory);

            var sink = geometry.Open();

            sink.BeginFigure(new RawVector2(x, y), FigureBegin.Filled);
            sink.AddLine(new RawVector2(x + width, y));
            sink.AddLine(new RawVector2(x + width, y + height));
            sink.AddLine(new RawVector2(x, y + height));
            sink.EndFigure(FigureEnd.Closed);

            sink.Close();

            device.DrawGeometry(geometry, brush, stroke);

            device.FillGeometry(geometry, interiorBrush);

            sink.Dispose();
            geometry.Dispose();
        }

        public void DrawArrowLine(float start_x, float start_y, float end_x, float end_y, float size, Direct2DColor color)
        {
            float delta_x = end_x >= start_x ? end_x - start_x : start_x - end_x;
            float delta_y = end_y >= start_y ? end_y - start_y : start_y - end_y;

            float length = (float)Math.Sqrt(delta_x * delta_x + delta_y * delta_y);

            float xm = length - size;
            float xn = xm;

            float ym = size;
            float yn = -ym;

            float sin = delta_y / length;
            float cos = delta_x / length;

            float x = xm * cos - ym * sin + end_x;
            ym = xm * sin + ym * cos + end_y;
            xm = x;

            x = xn * cos - yn * sin + end_x;
            yn = xn * sin + yn * cos + end_y;
            xn = x;

            FillTriangle(start_x, start_y, xm, ym, xn, yn, color);
        }

        public void DrawArrowLine(float start_x, float start_y, float end_x, float end_y, float size, Direct2DBrush brush)
        {
            float delta_x = end_x >= start_x ? end_x - start_x : start_x - end_x;
            float delta_y = end_y >= start_y ? end_y - start_y : start_y - end_y;

            float length = (float)Math.Sqrt(delta_x * delta_x + delta_y * delta_y);

            float xm = length - size;
            float xn = xm;

            float ym = size;
            float yn = -ym;

            float sin = delta_y / length;
            float cos = delta_x / length;

            float x = xm * cos - ym * sin + end_x;
            ym = xm * sin + ym * cos + end_y;
            xm = x;

            x = xn * cos - yn * sin + end_x;
            yn = xn * sin + yn * cos + end_y;
            xn = x;

            FillTriangle(start_x, start_y, xm, ym, xn, yn, brush);
        }

        public void DrawVerticalBar(float percentage, float x, float y, float width, float height, float stroke, Direct2DColor interiorColor, Direct2DColor color)
        {
            float half = stroke / 2.0f;
            float quarter = half / 2.0f;

            sharedBrush.Color = color;

            var rect = new RawRectangleF(x - half, y - half, x + width + half, y + height + half);

            device.DrawRectangle(rect, sharedBrush, half);

            if (percentage == 0.0f) return;

            rect.Left += quarter;
            rect.Right -= width - (width / 100.0f * percentage) + quarter;
            rect.Top += quarter;
            rect.Bottom -= quarter;

            sharedBrush.Color = interiorColor;

            device.FillRectangle(rect, sharedBrush);
        }

        public void DrawVerticalBar(float percentage, float x, float y, float width, float height, float stroke, Direct2DBrush interiorBrush, Direct2DBrush brush)
        {
            float half = stroke / 2.0f;
            float quarter = half / 2.0f;

            var rect = new RawRectangleF(x - half, y - half, x + width + half, y + height + half);

            device.DrawRectangle(rect, brush, half);

            if (percentage == 0.0f) return;

            rect.Left += quarter;
            rect.Right -= width - (width / 100.0f * percentage) + quarter;
            rect.Top += quarter;
            rect.Bottom -= quarter;

            device.FillRectangle(rect, interiorBrush);
        }

        public void DrawHorizontalBar(float percentage, float x, float y, float width, float height, float stroke, Direct2DColor interiorColor, Direct2DColor color)
        {
            float half = stroke / 2.0f;

            sharedBrush.Color = color;

            var rect = new RawRectangleF(x - half, y - half, x + width + half, y + height + half);

            device.DrawRectangle(rect, sharedBrush, stroke);

            if (percentage == 0.0f) return;

            rect.Left += half;
            rect.Right -= half;
            rect.Top += height - (height / 100.0f * percentage) + half;
            rect.Bottom -= half;

            sharedBrush.Color = interiorColor;

            device.FillRectangle(rect, sharedBrush);
        }

        public void DrawHorizontalBar(float percentage, float x, float y, float width, float height, float stroke, Direct2DBrush interiorBrush, Direct2DBrush brush)
        {
            float half = stroke / 2.0f;
            float quarter = half / 2.0f;

            var rect = new RawRectangleF(x - half, y - half, x + width + half, y + height + half);

            device.DrawRectangle(rect, brush, half);

            if (percentage == 0.0f) return;

            rect.Left += quarter;
            rect.Right -= quarter;
            rect.Top += height - (height / 100.0f * percentage) + quarter;
            rect.Bottom -= quarter;

            device.FillRectangle(rect, interiorBrush);
        }

        public void DrawCrosshair(CrosshairStyle style, float x, float y, float size, float stroke, Direct2DColor color)
        {
            sharedBrush.Color = color;

            if (style == CrosshairStyle.Dot)
            {
                FillCircle(x, y, size, color);
            }
            else if (style == CrosshairStyle.Plus)
            {
                DrawLine(x - size, y, x + size, y, stroke, color);
                DrawLine(x, y - size, x, y + size, stroke, color);
            }
            else if (style == CrosshairStyle.Cross)
            {
                DrawLine(x - size, y - size, x + size, y + size, stroke, color);
                DrawLine(x + size, y - size, x - size, y + size, stroke, color);
            }
            else if (style == CrosshairStyle.Gap)
            {
                DrawLine(x - size - stroke, y, x - stroke, y, stroke, color);
                DrawLine(x + size + stroke, y, x + stroke, y, stroke, color);

                DrawLine(x, y - size - stroke, x, y - stroke, stroke, color);
                DrawLine(x, y + size + stroke, x, y + stroke, stroke, color);
            }
            else if (style == CrosshairStyle.Diagonal)
            {
                DrawLine(x - size, y - size, x + size, y + size, stroke, color);
                DrawLine(x + size, y - size, x - size, y + size, stroke, color);
            }
            else if (style == CrosshairStyle.Swastika)
            {
                RawVector2 first = new RawVector2(x - size, y);
                RawVector2 second = new RawVector2(x + size, y);

                RawVector2 third = new RawVector2(x, y - size);
                RawVector2 fourth = new RawVector2(x, y + size);

                RawVector2 haken_1 = new RawVector2(third.X + size, third.Y);
                RawVector2 haken_2 = new RawVector2(second.X, second.Y + size);
                RawVector2 haken_3 = new RawVector2(fourth.X - size, fourth.Y);
                RawVector2 haken_4 = new RawVector2(first.X, first.Y - size);

                device.DrawLine(first, second, sharedBrush, stroke);
                device.DrawLine(third, fourth, sharedBrush, stroke);

                device.DrawLine(third, haken_1, sharedBrush, stroke);
                device.DrawLine(second, haken_2, sharedBrush, stroke);
                device.DrawLine(fourth, haken_3, sharedBrush, stroke);
                device.DrawLine(first, haken_4, sharedBrush, stroke);
            }
        }

        public void DrawCrosshair(CrosshairStyle style, float x, float y, float size, float stroke, Direct2DBrush brush)
        {
            if (style == CrosshairStyle.Dot)
            {
                FillCircle(x, y, size, brush);
            }
            else if (style == CrosshairStyle.Plus)
            {
                DrawLine(x - size, y, x + size, y, stroke, brush);
                DrawLine(x, y - size, x, y + size, stroke, brush);
            }
            else if (style == CrosshairStyle.Cross)
            {
                DrawLine(x - size, y - size, x + size, y + size, stroke, brush);
                DrawLine(x + size, y - size, x - size, y + size, stroke, brush);
            }
            else if (style == CrosshairStyle.Gap)
            {
                DrawLine(x - size - stroke, y, x - stroke, y, stroke, brush);
                DrawLine(x + size + stroke, y, x + stroke, y, stroke, brush);

                DrawLine(x, y - size - stroke, x, y - stroke, stroke, brush);
                DrawLine(x, y + size + stroke, x, y + stroke, stroke, brush);
            }
            else if (style == CrosshairStyle.Diagonal)
            {
                DrawLine(x - size, y - size, x + size, y + size, stroke, brush);
                DrawLine(x + size, y - size, x - size, y + size, stroke, brush);
            }
            else if (style == CrosshairStyle.Swastika)
            {
                RawVector2 first = new RawVector2(x - size, y);
                RawVector2 second = new RawVector2(x + size, y);

                RawVector2 third = new RawVector2(x, y - size);
                RawVector2 fourth = new RawVector2(x, y + size);

                RawVector2 haken_1 = new RawVector2(third.X + size, third.Y);
                RawVector2 haken_2 = new RawVector2(second.X, second.Y + size);
                RawVector2 haken_3 = new RawVector2(fourth.X - size, fourth.Y);
                RawVector2 haken_4 = new RawVector2(first.X, first.Y - size);

                device.DrawLine(first, second, brush, stroke);
                device.DrawLine(third, fourth, brush, stroke);

                device.DrawLine(third, haken_1, brush, stroke);
                device.DrawLine(second, haken_2, brush, stroke);
                device.DrawLine(fourth, haken_3, brush, stroke);
                device.DrawLine(first, haken_4, brush, stroke);
            }
        }

        private Stopwatch swastikaDeltaTimer = new Stopwatch();
        float rotationState = 0.0f;
        int lastTime = 0;
        public void RotateSwastika(float x, float y, float size, float stroke, Direct2DColor color)
        {
            if (!swastikaDeltaTimer.IsRunning) swastikaDeltaTimer.Start();

            int thisTime = (int)swastikaDeltaTimer.ElapsedMilliseconds;

            if (Math.Abs(thisTime - lastTime) >= 3)
            {
                rotationState += 0.1f;
                lastTime = (int)swastikaDeltaTimer.ElapsedMilliseconds;
            }

            if (thisTime >= 1000) swastikaDeltaTimer.Restart();

            if (rotationState > size)
            {
                rotationState = size * -1.0f;
            }

            sharedBrush.Color = color;

            RawVector2 first = new RawVector2(x - size, y - rotationState);
            RawVector2 second = new RawVector2(x + size, y + rotationState);

            RawVector2 third = new RawVector2(x + rotationState, y - size);
            RawVector2 fourth = new RawVector2(x - rotationState, y + size);

            RawVector2 haken_1 = new RawVector2(third.X + size, third.Y + rotationState);
            RawVector2 haken_2 = new RawVector2(second.X - rotationState, second.Y + size);
            RawVector2 haken_3 = new RawVector2(fourth.X - size, fourth.Y - rotationState);
            RawVector2 haken_4 = new RawVector2(first.X + rotationState, first.Y - size);

            device.DrawLine(first, second, sharedBrush, stroke);
            device.DrawLine(third, fourth, sharedBrush, stroke);

            device.DrawLine(third, haken_1, sharedBrush, stroke);
            device.DrawLine(second, haken_2, sharedBrush, stroke);
            device.DrawLine(fourth, haken_3, sharedBrush, stroke);
            device.DrawLine(first, haken_4, sharedBrush, stroke);
        }

        public void DrawBitmap(Direct2DBitmap bmp, float x, float y, float opacity)
        {
            Bitmap bitmap = bmp;
            device.DrawBitmap(bitmap, new RawRectangleF(x, y, x + bitmap.PixelSize.Width, y + bitmap.PixelSize.Height), opacity, BitmapInterpolationMode.Linear);
        }

        public void DrawBitmap(Direct2DBitmap bmp, float opacity, float x, float y, float width, float height)
        {
            Bitmap bitmap = bmp;
            device.DrawBitmap(bitmap, new RawRectangleF(x, y, x + width, y + height), opacity, BitmapInterpolationMode.Linear, new RawRectangleF(0, 0, bitmap.PixelSize.Width, bitmap.PixelSize.Height));
        }

        #endregion

        #region Text

        public void DrawText(string text, float x, float y, Direct2DFont font, Direct2DColor color)
        {
            sharedBrush.Color = color;
            device.DrawText(text, text.Length, font, new RawRectangleF(x, y, float.MaxValue, float.MaxValue), sharedBrush, DrawTextOptions.NoSnap, MeasuringMode.Natural);
        }

        public void DrawText(string text, float x, float y, Direct2DFont font, Direct2DBrush brush)
        {
            device.DrawText(text, text.Length, font, new RawRectangleF(x, y, float.MaxValue, float.MaxValue), brush, DrawTextOptions.NoSnap, MeasuringMode.Natural);
        }

        public void DrawText(string text, float x, float y, float fontSize, Direct2DFont font, Direct2DColor color)
        {
            sharedBrush.Color = color;

            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            layout.SetFontSize(fontSize, new TextRange(0, text.Length));

            device.DrawTextLayout(new RawVector2(x, y), layout, sharedBrush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        public void DrawText(string text, float x, float y, float fontSize, Direct2DFont font, Direct2DBrush brush)
        {
            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            layout.SetFontSize(fontSize, new TextRange(0, text.Length));

            device.DrawTextLayout(new RawVector2(x, y), layout, brush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        public void DrawTextWithBackground(string text, float x, float y, Direct2DFont font, Direct2DColor color, Direct2DColor backgroundColor)
        {
            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            float modifier = layout.FontSize / 4.0f;

            sharedBrush.Color = backgroundColor;

            device.FillRectangle(new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier), sharedBrush);

            sharedBrush.Color = color;

            device.DrawTextLayout(new RawVector2(x, y), layout, sharedBrush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        public void DrawTextWithBackground(string text, float x, float y, Tuple<string, int> tfont, Tuple<int, int, int, int> tcolor, Tuple<int, int, int, int> tbackground, out float modifier)
        {
            Direct2DBrush color = CreateBrush(tcolor.Item1, tcolor.Item2, tcolor.Item3, tcolor.Item4);
            Direct2DBrush backgroundColor = CreateBrush(tbackground.Item1, tbackground.Item2, tbackground.Item3, 0.3f);
            Direct2DFont font = CreateFont(tfont.Item1, tfont.Item2 * Direct2DRenderer.size_scale);
            
            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            modifier = layout.FontSize / 4.0f;

            sharedBrush.Color = backgroundColor;

            device.FillRectangle(new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier), sharedBrush);

            sharedBrush.Color = color;

            device.DrawTextLayout(new RawVector2(x, y), layout, sharedBrush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }
        
        public void DrawTextWithBackground(string text, float x, float y, Tuple<string, int> tfont, Tuple<int, int, int, int> tcolor, Tuple<int, int, int, int> tbackground)
        {
            Direct2DBrush color = CreateBrush(tcolor.Item1, tcolor.Item2, tcolor.Item3, tcolor.Item4);
            Direct2DBrush backgroundColor = CreateBrush(tbackground.Item1, tbackground.Item2, tbackground.Item3, tbackground.Item4);
            Direct2DFont font = CreateFont(tfont.Item1, tfont.Item2);

            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            float modifier = layout.FontSize / 4.0f;

            sharedBrush.Color = backgroundColor;

            device.FillRectangle(new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier), sharedBrush);

            sharedBrush.Color = color;

            device.DrawTextLayout(new RawVector2(x, y), layout, sharedBrush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        public void DrawTextWithBackground(List<Tuple<String, Direct2DBrush>> text, float x, float y, Direct2DFont font, Direct2DBrush backgroundBrush)
        {
            int offset = 0;
            float xoffset = 0f;

            foreach (var t in text)
            {
                var layout = new TextLayout(fontFactory, t.Item1, font, float.MaxValue, float.MaxValue);
                xoffset = Math.Max(xoffset, layout.Metrics.Width);
            }

            foreach (var t in text)
            {
                var layout = new TextLayout(fontFactory, t.Item1, font, float.MaxValue, float.MaxValue);

                float modifier = font.FontSize / 4.0f;

                device.FillRectangle(new RawRectangleF(x - modifier, y - modifier - offset, x + xoffset + modifier, y + layout.Metrics.Height + modifier - offset), backgroundBrush);

                device.DrawTextLayout(new RawVector2(x, y - offset), layout, t.Item2, DrawTextOptions.NoSnap);

                offset += 15;

                layout.Dispose();
            }
        }


        public void DrawTextWithBackground(string text, float x, float y, Direct2DFont font, Direct2DBrush brush, Direct2DBrush backgroundBrush)
        {
            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            float modifier = layout.FontSize / 4.0f;

            device.FillRectangle(new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier), backgroundBrush);

            device.DrawTextLayout(new RawVector2(x, y), layout, brush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        public void DrawTextWithBackground(string text, float x, float y, float maxWidth, float maxHeight, Direct2DFont font, Direct2DBrush brush, Direct2DBrush backgroundBrush)
        {
            var layout = new TextLayout(fontFactory, text, font, maxWidth, maxHeight);

            float modifier = layout.FontSize / 4.0f;

            device.FillRectangle(new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier), backgroundBrush);

            device.DrawTextLayout(new RawVector2(x, y), layout, brush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        public void DrawTextWithBackground(string text, float x, float y, float fontSize, Direct2DFont font, Direct2DColor color, Direct2DColor backgroundColor)
        {
            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            layout.SetFontSize(fontSize, new TextRange(0, text.Length));

            float modifier = fontSize / 4.0f;

            sharedBrush.Color = backgroundColor;

            device.FillRectangle(new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier), sharedBrush);

            sharedBrush.Color = color;

            device.DrawTextLayout(new RawVector2(x, y), layout, sharedBrush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        public void DrawTextWithBackground(string text, float x, float y, float fontSize, Direct2DFont font, Direct2DBrush brush, Direct2DBrush backgroundBrush)
        {
            var layout = new TextLayout(fontFactory, text, font, float.MaxValue, float.MaxValue);

            layout.SetFontSize(fontSize, new TextRange(0, text.Length));

            float modifier = fontSize / 4.0f;

            device.FillRectangle(new RawRectangleF(x - modifier, y - modifier, x + layout.Metrics.Width + modifier, y + layout.Metrics.Height + modifier), backgroundBrush);

            device.DrawTextLayout(new RawVector2(x, y), layout, brush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }

        #endregion

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // Free managed objects
                }

                deleteInstance();

                disposedValue = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        
        private string BreakText(string sentence, int partLength)
        {
            string[] words = sentence.Split(' ');
            string newString = string.Empty;
            string part = string.Empty;
            foreach (var word in words)
            {
                if (part.Length + word.Length < partLength)
                {
                    part += string.IsNullOrEmpty(part) ? word : " " + word;
                }
                else
                {
                    newString += part + "\n";
                    part = word;
                }
            }
            newString += part;
            return newString;
        }

        #region Hints Slots
        // Type:
        // 0: hero selection
        // 1: hero selection
        // 2: hero selection
        // 3: hero selection
        // 4: hero selection
        // 5: hero introduction

        // 6: items selection
        // 7: retreat
        // 8: press on
        // 9: last hit
        // 10: jungle
        // 11: safe farming
        // 12: hero information
        public void SetupHintSlots()
        {
            Message[] heroes_sugg = new Message[5];
            
            for (int i = 0; i < messages.Length; i++)
            {
                switch (i)
                {
                    // Hero selection slot1
                    case 0:
                        string Hero_selection1 = "Hero selection slot1";
                        messages[i] = new Message(Hero_selection1, "", width_unit * 24, height_unit * (i * 2 + 3) * 2);
                        heroes_sugg[i] = messages[i];
                        break;

                    // Hero selection slot2
                    case 1:
                        string Hero_selection2 = "Hero selection slot2";
                        messages[i] = new Message(Hero_selection2, "", width_unit * 24, height_unit * (i * 2 + 3) * 2);
                        heroes_sugg[i] = messages[i];
                        break;

                    // Hero selection slot3
                    case 2:
                        string Hero_selection3 = "Hero selection slot3";
                        messages[i] = new Message(Hero_selection3, "", width_unit * 24, height_unit * (i * 2 + 3) * 2);
                        heroes_sugg[i] = messages[i];
                        break;

                    // Hero selection slot4
                    case 3:
                        string Hero_selection4 = "Hero selection slot4";
                        messages[i] = new Message(Hero_selection4, "", width_unit * 24, height_unit * (i * 2 + 3) * 2);
                        heroes_sugg[i] = messages[i];
                        break;

                    // Hero selection slot5
                    case 4:
                        string Hero_selection5 = "Hero selection slot5";
                        messages[i] = new Message(Hero_selection5, "", width_unit * 24, height_unit * (i * 2 + 3) * 2);
                        heroes_sugg[i] = messages[i];
                        break;

                    // 5: hero intro
                    case 5:
                        string hero_intro = "Hero introduction message slot";
                        messages[i] = new Message(hero_intro, "", width_unit * 12, height_unit * 5);
                        
                        break;

                    // 6: items selection
                    case 6:
                        string item = "Items selection slot";
                        messages[i] = new Message(item, "", width_unit * 4, height_unit * 6);
                        break;

                    // 7: retreat
                    // 8: press on
                    case 7:
                        string retreat = "Laning message slot";
                        messages[i] = new Message(retreat, "", Screen.PrimaryScreen.Bounds.Width / 2 - (retreat.Length / 2), Screen.PrimaryScreen.Bounds.Height / 4 * 3);
                        break;
                    case 8:
                        string press_on = "Laning message slot";
                        messages[i] = new Message(press_on, "", Screen.PrimaryScreen.Bounds.Width / 2 - (press_on.Length / 2), Screen.PrimaryScreen.Bounds.Height / 4 * 3);
                        break;

                    // Message position dynamic

                    // 9: last hit
                    case 9:
                        string last_hit = "Last hit message slot";
                        messages[i] = new Message(last_hit, "", i * 200, 0);
                        break;

                    // 10: jungle
                    case 10:
                        string jungle = "Jungle message slot";
                        messages[i] = new Message(jungle, "", i * 200, 0);
                        break;
                        
                    // 11: safe farming
                    case 11:
                        string safe_farming = "Safe farming message slot";
                        messages[i] = new Message(safe_farming, "", 0, Screen.PrimaryScreen.Bounds.Height - 100);
                        break;

                    // 12: hero info
                    case 12:
                        string hero_info = "Hero information message slot";
                        messages[i] = new Message(hero_info, "", width_unit * 16, height_unit * 6);
                        break;

                    default:
                        Console.WriteLine("Unknown message type detected. (other than 0-10)");
                        break;
                }
                messages[i].on = false;
            }
            HeroSugg = new HeroSuggestion(heroes_sugg);
        }
        #endregion

        #region Add messages
        // 0: hero selection
        // 1: hero selection
        // 2: hero selection
        // 3: hero selection
        // 4: hero selection
        public void HeroSelectionHints(string[] heros, string[] img)
        {
            if (heros.Length > 5 || img.Length > 5)
            {
                throw new System.ArgumentException("Number of suggested Heroes is not 5.");
            }

            for (int i = 0; i < heros.Length; i++)
            {
                switch (i)
                {
                    case 0:
                        AddMessage(hints.hero_selection_1, heros[i], img[i]);
                        break;
                    case 1:
                        AddMessage(hints.hero_selection_2, heros[i], img[i]);
                        break;
                    case 2:
                        AddMessage(hints.hero_selection_3, heros[i], img[i]);
                        break;
                    case 3:
                        AddMessage(hints.hero_selection_4, heros[i], img[i]);
                        break;
                    case 4:
                        AddMessage(hints.hero_selection_5, heros[i], img[i]);
                        break;

                    default:
                        Console.WriteLine("heros.Length is not 5.");
                        break;
                }
            }
        }

        public void ItemSelectionHints(string item, string img)
        {
            string temp = BreakText(item, 50);
             AddMessage(hints.items_selection, temp, img);
        }

        public void HeroInfoHints(string info, string img)
        {
            string temp = BreakText(info, 50);
            AddMessage(hints.heroinformation, temp, img);
        }

        public void SelectedHeroSuggestion(int HeroID, float mouse_Y)
        {
            //Console.WriteLine(Directory.GetCurrentDirectory());
            string path = Path.Combine(Environment.CurrentDirectory, @"..\..\..\replayParse\Properties\hero_difficulty_version_1.txt");

            hero_difficulty dt = new hero_difficulty(path);
            string suggestion = dt.mainDiff(HeroID);
            string hero_rating = dt.getFinalLevel(HeroID)[0] + ": " + dt.getFinalLevel(HeroID)[1] + "\n\n";
            // add a newline every 8 chars
            suggestion = hero_rating + suggestion;
            suggestion = BreakText(suggestion, 50);

            Tuple<int, int, int, int> background = new Tuple<int, int, int, int>(109, 109, 109, 255);
            Tuple<int, int, int, int> color = new Tuple<int, int, int, int>(255, 255, 255, 255);
            Tuple<string, int> font = new Tuple<string, int>("Consolas", 18);


            AddMessage(hints.hero_introduction, suggestion, "", color, background, font);
            messages[Convert.ToInt32(hints.hero_introduction)].y = mouse_Y;
        }

        // 7: retreat
        public void Retreat(string text, string imgName)
        {
            warning_timer.Start();
            //low_hp = true;
            AddMessage(hints.retreat, text, imgName);
        }
        public void AddMessage(hints type, string text, [Optional] string imgName, [Optional] Tuple<int, int, int, int> color, [Optional] Tuple<int, int, int, int> background, [Optional]  Tuple<string, int> font)
        {
            int idx = Convert.ToInt32(type);
            messages[idx].text = text;
            if (imgName != null)
            {
                messages[idx].imgName = imgName;
            }

            if (background != null)
            {
                messages[idx].background = background;
            }

            if (color != null)
            {
                messages[idx].color = color;
            }

            if (font != null)
            {
                messages[idx].font = font;
            }
            messages[idx].on = true;
        }

        #endregion

        #region Delete messages
        public void DeleteMessage(hints type)
        {
            if (type == hints.retreat)
            {
                low_hp = false;
            }
            messages[Convert.ToInt32(type)].clear();
        }
        #endregion

        #region HP Graph

        // Inverts the graphs boolean
        // i.e. true becomes false, and vice versa
        public void ToggleGraph()
        {
            drawGraphs ^= true;
        }

        public void ToggleGraph(bool toggle)
        {
            drawGraphs = toggle;
        }

        // Updates (and overwrites the previous) the hero health
        // Currently holds 5 integers
        public void UpdateHeroHPGraph(double[] newHps)
        {
            hps = newHps;
        }

        public void UpdateHeroHPQueue(double newhp)
        {
            if (currHp.Count > 250)
            {
                currHp.Dequeue();
            }
            currHp.Enqueue(newhp);
        }
        #endregion

        #region High Light
        public void UpdateHighlightTime(Dictionary<int, List<Tuple<String, String, String>>> ticks, float maxTick)

        {
            this.ticksInfo = ticks;
            this.maxTick = maxTick;
        }

        public void ToggleHightlight(bool drawHighlight)
        {
            this.drawHighlight = drawHighlight;
        }
        
        private void CheckToShowHighlightTime()
        {
            var mousePosition = Control.MousePosition;
            var mX = mousePosition.X;
            var mY = mousePosition.Y;

            int x = Screen.PrimaryScreen.Bounds.Width;
            int y = Screen.PrimaryScreen.Bounds.Height;
            float xInit = x / 4;
            float xEnd = 3 * x / 4;


            Direct2DFont font = CreateFont("Consolas", 12);
            //Direct2DBrush brush = CreateBrush(0, 0, 0, 255);
            Direct2DBrush background = CreateBrush(109, 109, 109, 255);

            foreach (var a in ticksInfo)
            {
                float percent = a.Key / (float)maxTick;
                float xCurr = xInit + (xEnd - xInit) * percent;

                List<Tuple<String, Direct2DBrush>> killText = new List<Tuple<String, Direct2DBrush>>();
                foreach (var k in a.Value)
                {
                    if (killText.Count == 0)
                        killText.Add(new Tuple<string, Direct2DBrush>(TimeSpan.FromSeconds(a.Key).ToString(@"hh\:mm\:ss"), CreateBrush(200, 200, 200)));
                    Direct2DBrush brush = null;
                    switch (k.Item3)
                    {
                        case "LR":
                            brush = CreateBrush(255, 100, 100);
                            break;
                        case "R":
                            brush = CreateBrush(255, 0, 0);
                            break;
                        case "LG":
                            brush = CreateBrush(180, 255, 180);
                            break;
                        case "G":
                            brush = CreateBrush(0, 255, 0);
                            break;
                        default:
                            brush = CreateBrush(200, 200, 200);
                            break;
                    }
                    killText.Add(new Tuple<string, Direct2DBrush>(k.Item1 + " killed " + k.Item2, brush));
                }
                killText.Reverse();
                //killText = killText.TrimEnd('\r', '\n');
                if (mX > xCurr - 2 && mX < xCurr + 2 && mY > (3 * y / 4) - 14 && mY < (3 * y / 4) + 10)
                    DrawTextWithBackground(killText, xCurr, 3 * y / 4 - x / 80, font, background);
            }
        }
        #endregion

        #region Ban&Pick feedback
        // one of the five suggested heroes get banned by the other team
        public void SuggestedHeroBanned(int heroIndex)
        {
            Direct2DBitmap cross = new Direct2DBitmap(device, @"..\\..\\other_images\red_cross.png");

            DrawBitmap(cross, 1, messages[heroIndex].img_x, messages[heroIndex].img_y, messages[heroIndex].img_width, messages[heroIndex].img_height);
            cross.SharpDXBitmap.Dispose();
        }

        // one of the five suggested heroes get picked by our team
        public void SuggestedHeroPicked(int heroIndex)
        {
            Direct2DBitmap check = new Direct2DBitmap(device, @"..\\..\\other_images\green_check.png");
            DrawBitmap(check, 1, messages[heroIndex].img_x, messages[heroIndex].img_y, messages[heroIndex].img_width, messages[heroIndex].img_height);
            check.SharpDXBitmap.Dispose();
        }
        #endregion

        #region Logo Draw
        private void DrawLogo(Instruction instruction)
        {
            Direct2DBrush color = CreateBrush(255, 255, 255, 255);
            Direct2DBrush backgroundColor = blackBrush;
            Direct2DFont font = CreateFont("Times New Roman", 20);

            var layout = new TextLayout(fontFactory, instruction.Logo.Item1, font, float.MaxValue, float.MaxValue);

            float modifier = layout.FontSize / 8.0f;

            sharedBrush.Color = backgroundColor;

            float x = instruction.Logo.Item2;
            float y = instruction.Logo.Item3;

            device.FillRectangle(new RawRectangleF(x, y , x + layout.Metrics.Width + 2 * modifier, y + layout.Metrics.Height + 2 * modifier), sharedBrush);

            sharedBrush.Color = color;

            device.DrawTextLayout(new RawVector2(x + modifier, y + modifier), layout, sharedBrush, DrawTextOptions.NoSnap);

            layout.Dispose();
        }
        #endregion

        #region Ingame Draw
        private string SelectFolder(int i)
        { 
            string path = "";
            switch (i)
            {
                // Hero selection slot1
                case 6:
                    path = @"..\\..\\items_icon\";
                    break;
                case int n when (n >= 7 && n <= 11):
                    path = @"..\\..\\other_images\";
                    messages[i].img_width = 144 / 2;
                    messages[i].img_height = 144 / 2;
                    break;
                case 12:
                    path = @"..\\..\\hero_icon_images\";
                    break;
            }
            return path;
        }
        public void UpdateHeroHpGraphIcons(List<int> heroIds)
        {
            this.heroIds = heroIds;
        }

        static private Stopwatch warning_timer = new Stopwatch();
        private Tuple<int, int, int> AveragePixelColor(System.Drawing.Bitmap bmp)
        {
            System.Drawing.Imaging.BitmapData scrData = bmp.LockBits(
                new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format24bppRgb
                );

            int stride = scrData.Stride;
            IntPtr Scan0 = scrData.Scan0;

            long[] totals = new long[] { 0, 0, 0 };

            int w = bmp.Width;
            int h = bmp.Height;

            unsafe
            {
                byte* p = (byte*)(void*)Scan0;

                for (int y = 0; y < h; y++)
                {
                    for (int x = 0; x < w; x++)
                    {
                        for (int color = 0; color < 3; color++)
                        {
                            int idx = (y * stride) + x * 3 + color;

                            totals[color] += p[idx];
                        }
                    }
                }
            }

            int avgB = (int)totals[0] / (w * h);
            int avgG = (int)totals[1] / (w * h);
            int avgR = (int)totals[2] / (w * h);

            return new Tuple<int, int, int>(avgR, avgG, avgB); ;
        }
        public void Ingame_Draw(IntPtr parentWindowHandle, OverlayWindow overlay)
        {
            IntPtr fg = GetForegroundWindow();

            if (fg == parentWindowHandle || (GetDesktopWindow() == parentWindowHandle))
            {
                BeginScene();
                ClearScene();

                // Loop through all the messages (not include: hero information and two instructions)

                for (int i = 6; i < 13; i++)
                {
                    if (messages[i].on)
                    {
                        float modifier;
                        DrawTextWithBackground(messages[i].text, messages[i].x, messages[i].y, messages[i].font, messages[i].color, messages[i].background, out modifier);
                        if (messages[i].imgName != "")
                        {
                            string path = SelectFolder(i);
                            if (path == "") { throw new Exception("path not initialized"); }
                            ShowImage(path, i, modifier);
                        }
                    }
                }

                if (drawHighlight)
                {
                    int x = Screen.PrimaryScreen.Bounds.Width;
                    int y = Screen.PrimaryScreen.Bounds.Height;
                    float xInit = x / 4;
                    float xEnd = 3 * x / 4;
                    DrawLine(xInit, 3 * y / 4, xEnd, 3 * y / 4, 2, lightRedBrush);
                    foreach (var a in ticksInfo)
                    {
                        float percent = a.Key / (float)maxTick;
                        float xCurr = xInit + (xEnd - xInit) * percent;
                        DrawBox2D(xCurr, (3 * y / 4) - 2, 4, 4, 2, blueBrush, blueBrush);
                    }

                    CheckToShowHighlightTime();
                }

                if (low_hp)
                {
                    DrawCircle(Screen.PrimaryScreen.Bounds.Width / 2, Screen.PrimaryScreen.Bounds.Height / 2, Screen.PrimaryScreen.Bounds.Height / 5, 2f, redBrush);
                }

                if (drawGraphs)
                {
                    int currY = Screen.PrimaryScreen.Bounds.Height / 2;

                    // bar graph
                    for (int i = 0; i < 5; i++)
                    {
                        {
                            Direct2DBitmap bmp = new Direct2DBitmap(device, @"hero_icon_images\" + heroIds[i] + ".png");

                            System.Drawing.Bitmap csb = new System.Drawing.Bitmap(@"hero_icon_images\" + heroIds[i] + ".png");

                            Tuple<int, int, int> rgb = AveragePixelColor(csb);

                            DrawBox2D(
                            51 * i,                                                // x
                            ((float)currY - (float)hps[i]) * .3f + currY / 2,      // y  
                            50,                                                    // width
                            (float)hps[i] * .3f,                                   // height
                            1,                                                     // stroke
                            CreateBrush(rgb.Item1,rgb.Item2,rgb.Item3),                     // int brush 
                            blackBrush                                             // ext brush
                            );
                            
                            DrawBitmap(bmp,
                                1,
                                51 * i,
                                currY - 108,
                                50,
                                28);

                            bmp.SharpDXBitmap.Dispose();
                            csb.Dispose();
                        }
                    }

                    // vertical line
                    DrawLine(250,           // start_x
                            currY - 100 + 28,   // start_y
                            250,                // end_x
                            currY + 150 + 28,   // end_y
                            2,                  // stroke
                            redBrush);          // brush

                    // horizontal line
                    DrawLine(0,             // start_x
                        currY + 150 + 28,   // start_y
                        250,                // end_x
                        currY + 150 + 28,   // end_y
                        2,                  // stroke
                        redBrush);          // brush

                    // line graph
                    for (int j = 0; j < currHp.Count - 1; j++)
                    {
                        double[] tempCurrHp = currHp.ToArray();
                        DrawLine(j,                                         // start_x
                         (float)(currY - tempCurrHp[j]) / 6 + currY + 28,       // start_y
                                 1 + j,                                          // end_x
                         (float)(currY - tempCurrHp[j + 1]) / 6 + currY + 28,   // end_y
                                 1,                                              // stroke
                                 redBrush);                                      // brush
                    }
                }

                EndScene();
            }
            else
            {
                clear();
            }
        }

        private void ShowImage(string path, int i, float modifier)
        {
            if (i == 7 && warning_timer.ElapsedMilliseconds > 1000)
            {
                if (warning_timer.ElapsedMilliseconds > 2000)
                {
                    warning_timer.Reset();
                }
                Direct2DBitmap bmp = new Direct2DBitmap(device, path + messages[i].imgName + ".png");
                DrawBitmap(bmp, 0.2f, messages[i].img_x, messages[i].img_y - modifier, messages[i].img_width, messages[i].img_height);
                bmp.SharpDXBitmap.Dispose();
            }
            else
            {
                Direct2DBitmap bmp = new Direct2DBitmap(device, path + messages[i].imgName + ".png");
                DrawBitmap(bmp, 1, messages[i].img_x, messages[i].img_y - modifier, messages[i].img_width, messages[i].img_height);
                bmp.SharpDXBitmap.Dispose();
            }
        }
        #endregion

        #region HeroSelection Draw
        /* 1 ~ 5 refer to which suggesed hero is picked by own team
         * -1 ~ -5 refer to which suggesd hero is banned by other team
         * 0 refer to nothing happened
         */
        public void HeroSelectionFeedBack(int code)
        {
            ban_and_pick = code;
        }

        private void CheckBanPick()
        {
            if (ban_and_pick != 0)
            {
                if (ban_and_pick > 0 && ban_and_pick <= 5)
                {
                    SuggestedHeroPicked(ban_and_pick - 1);
                }
                else if (ban_and_pick < 0 && ban_and_pick >= -5)
                {
                    SuggestedHeroBanned(-ban_and_pick - 1);
                }
                else
                {
                    throw new Exception("hero index given is not from -5 to 5");
                }
            }
        }
        private void CheckToShowHeroSuggestion()
        {
            for (int i = 0; i < 5; i++)
            {
                var mouse_pos = Control.MousePosition;
                if (mouse_pos.X > messages[i].img_x && mouse_pos.X < messages[i].img_x + messages[i].img_width && mouse_pos.Y > messages[i].img_y && mouse_pos.Y < messages[i].img_y + messages[i].img_height)
                {
                    SelectedHeroSuggestion(Int32.Parse(messages[i].imgName), mouse_pos.Y);
                    return;
                }
                else
                {
                    messages[5].on = false;
                }
            }
        }
        
        public void HeroSelection_Draw(IntPtr parentWindowHandle, OverlayWindow overlay)
        {
            IntPtr fg = GetForegroundWindow();

            if (fg == parentWindowHandle || (GetDesktopWindow() == parentWindowHandle))
            {
                BeginScene();
                ClearScene();

                // Draw hero selection suggestion box
                Direct2DBrush color = CreateBrush(messages[0].color.Item1, messages[0].color.Item2, messages[0].color.Item3, messages[0].color.Item4);
                Direct2DBrush background = CreateBrush(messages[0].background.Item1, messages[0].background.Item2, messages[0].background.Item3, messages[0].background.Item4);
                Direct2DFont textFont = CreateFont(messages[0].font.Item1, messages[0].font.Item2);
                Direct2DBrush box_background = CreateBrush(109, 109, 109, 150);
                // The box
                device.FillRectangle(new RawRectangleF(HeroSugg.box_pos.Item1, HeroSugg.box_pos.Item2, HeroSugg.box_pos.Item3, HeroSugg.box_pos.Item4), box_background);
                // Title of the box
                DrawTextWithBackground(HeroSugg.title.Item1, HeroSugg.title.Item2, HeroSugg.title.Item3, textFont, color, background);

                // Loop through all 5 the suggestions and hero intro
                for (int i = 0; i < 6; i++)
                {
                    if (messages[i].on)
                    {
                        float modifier;
                        DrawTextWithBackground(messages[i].text, messages[i].x, messages[i].y, messages[i].font, messages[i].color, messages[i].background, out modifier);
                        if (messages[i].imgName != "")
                        {
                            Direct2DBitmap bmp = new Direct2DBitmap(device, @"..\\..\\hero_icon_images\" + messages[i].imgName + ".png");
                            DrawBitmap(bmp, 1, messages[i].img_x, messages[i].img_y - modifier, messages[i].img_width, messages[i].img_height);

                            bmp.SharpDXBitmap.Dispose();
                        }
                    }
                    // Green Check or Red X
                    CheckBanPick();
                }
                CheckToShowHeroSuggestion();
                EndScene();
            }
            else
            {
                clear();
            }
        }
        #endregion

        #region HeroInfo Draw
        public HeroInfo heroInfo;
        public void HeroIntro_setup(int HeroID)
        {
            Tuple<int, int, int, int> background = new Tuple<int, int, int, int>(109, 109, 109, 255);
            Tuple<int, int, int, int> color = new Tuple<int, int, int, int>(255, 255, 255, 255);


            AddMessage(hints.heroinformation, HeroID.ToString(), HeroID.ToString(), color, background);

            string content = getHeroInfo(HeroID);
            heroInfo = new HeroInfo(messages[12], content);
        }

        public string getHeroInfo(int HeroID)
        {
            string content = "You are not good enough to play this hero. You can try but you will fail.";

            content = BreakText(content, 50);

            return content;
        }

        public void HeroInfo_Draw(IntPtr parentWindowHandle, OverlayWindow overlay)
        {
            IntPtr fg = GetForegroundWindow();

            if (fg == parentWindowHandle || (GetDesktopWindow() == parentWindowHandle))
            {
                BeginScene();
                ClearScene();

                // Draw hero selection suggestion box
                Direct2DBrush color = CreateBrush(heroInfo.Info.color.Item1, heroInfo.Info.color.Item2, heroInfo.Info.color.Item3, heroInfo.Info.color.Item4);
                Direct2DBrush background = CreateBrush(heroInfo.Info.background.Item1, heroInfo.Info.background.Item2, heroInfo.Info.background.Item3, heroInfo.Info.background.Item4);
                Direct2DFont textFont = CreateFont(heroInfo.Info.font.Item1, heroInfo.Info.font.Item2);
                Direct2DBrush box_background = CreateBrush(109, 109, 109, 150);
                // The box
                device.FillRectangle(new RawRectangleF(heroInfo.box_pos.Item1, heroInfo.box_pos.Item2, heroInfo.box_pos.Item3, heroInfo.box_pos.Item4), box_background);
                // Title of the box
                DrawTextWithBackground(heroInfo.title.Item1, heroInfo.title.Item2, heroInfo.title.Item3, textFont, color, background);
                DrawTextWithBackground(heroInfo.content.Item1, heroInfo.content.Item2, heroInfo.content.Item3, textFont, color, background);

                float modifier;
                DrawTextWithBackground(messages[12].text, messages[12].x, messages[12].y, messages[12].font, messages[12].color, messages[12].background, out modifier);
                if (messages[12].imgName != "")
                {
                    Direct2DBitmap bmp = new Direct2DBitmap(device, @"..\\..\\hero_icon_images\" + messages[12].imgName + ".png");
                    DrawBitmap(bmp, 1, messages[12].img_x, messages[12].img_y - modifier, messages[12].img_width, messages[12].img_height);

                    bmp.SharpDXBitmap.Dispose();
                }
                EndScene();
            }
            else
            {
                clear();
            }
        }
        #endregion

        #region instruction draw
        public Instruction instruction;
        public void Intructions_setup(string content)
        {
            float width_unit = Screen.PrimaryScreen.Bounds.Width / 32;
            float height_unit = Screen.PrimaryScreen.Bounds.Height / 32;
            string text = content;
            Message instructions = new Message(text, "", width_unit * 20, height_unit * 6);
            instruction = new Instruction(instructions, "Instructions");
        }

        static private Stopwatch button_timer = new Stopwatch();
        public void Intructions_Draw(IntPtr parentWindowHandle, OverlayWindow overlay, float positionX, float positionY, IntPtr doNotIgnoreHandle)
        {
            IntPtr fg = GetForegroundWindow();

            if (fg == parentWindowHandle ||
                GetDesktopWindow() == parentWindowHandle ||
                fg == doNotIgnoreHandle)
            {
                BeginScene();
                ClearScene();

                if (button_timer.ElapsedMilliseconds < 2000)
                {
                    float distanceFromDefaultHorizontal = positionX - instruction.box_pos.Item1;
                    float distanceFromDefaultVertical = positionY - instruction.box_pos.Item2;

                    // Draw hero selection suggestion box
                    Direct2DBrush color = CreateBrush(instruction.color.Item1, instruction.color.Item2, instruction.color.Item3, instruction.color.Item4);
                    Direct2DBrush background = CreateBrush(instruction.background.Item1, instruction.background.Item2, instruction.background.Item3, instruction.background.Item4);
                    Direct2DFont textFont = CreateFont(instruction.font.Item1, instruction.font.Item2);
                    Direct2DBrush box_background = CreateBrush(109, 109, 109, 150);
                    // The box
                    device.FillRectangle(
                        new RawRectangleF(
                            left: instruction.box_pos.Item1 + distanceFromDefaultHorizontal,
                            top: instruction.box_pos.Item2 + distanceFromDefaultVertical,
                            right: instruction.box_pos.Item3 + distanceFromDefaultHorizontal,
                            bottom: instruction.box_pos.Item4 + distanceFromDefaultVertical),
                        box_background);
                    // Title of the box
                    DrawTextWithBackground(
                        text: instruction.title.Item1,
                        x: instruction.title.Item2 + distanceFromDefaultHorizontal,
                        y: instruction.title.Item3 + distanceFromDefaultVertical,
                        font: textFont,
                        brush: color,
                        backgroundBrush: background);

                    showInstructionButtons(distanceFromDefaultHorizontal, distanceFromDefaultVertical);
                    float modifier;
                    DrawTextWithBackground(
                        text: instruction.instructions.text,
                        x: instruction.instructions.x + distanceFromDefaultHorizontal,
                        y: instruction.instructions.y + distanceFromDefaultVertical,
                        tfont: instruction.instructions.font,
                        tcolor: instruction.instructions.color,
                        tbackground: instruction.instructions.background,
                        modifier: out modifier);

                }
                EndScene();
            }
            else
            {
                clear();
            }
        }
        public void Intructions_Draw(IntPtr parentWindowHandle, OverlayWindow overlay)
        {
            IntPtr fg = GetForegroundWindow();
            if (fg == parentWindowHandle ||
                GetDesktopWindow() == parentWindowHandle)
            {
                BeginScene();
                ClearScene();

                if (button_timer.ElapsedMilliseconds < 2000)
                {
                    float distanceFromDefaultHorizontal = 0;
                    float distanceFromDefaultVertical = 0;

                    // Draw hero selection suggestion box
                    Direct2DBrush color = CreateBrush(instruction.color.Item1, instruction.color.Item2, instruction.color.Item3, instruction.color.Item4);
                    Direct2DBrush background = CreateBrush(instruction.background.Item1, instruction.background.Item2, instruction.background.Item3, instruction.background.Item4);
                    Direct2DFont textFont = CreateFont(instruction.font.Item1, instruction.font.Item2);
                    Direct2DBrush box_background = CreateBrush(109, 109, 109, 150);
                    // The box
                    device.FillRectangle(
                        new RawRectangleF(
                            left: instruction.box_pos.Item1 + distanceFromDefaultHorizontal,
                            top: instruction.box_pos.Item2 + distanceFromDefaultVertical,
                            right: instruction.box_pos.Item3 + distanceFromDefaultHorizontal,
                            bottom: instruction.box_pos.Item4 + distanceFromDefaultVertical),
                        box_background);
                    // Title of the box
                    DrawTextWithBackground(
                        text: instruction.title.Item1,
                        x: instruction.title.Item2 + distanceFromDefaultHorizontal,
                        y: instruction.title.Item3 + distanceFromDefaultVertical,
                        font: textFont,
                        brush: color,
                        backgroundBrush: background);
                    // Draw LOGO
                    DrawLogo(instruction);

                    showInstructionButtons(distanceFromDefaultHorizontal, distanceFromDefaultVertical);
                    float modifier;
                    DrawTextWithBackground(
                        text: instruction.instructions.text,
                        x: instruction.instructions.x + distanceFromDefaultHorizontal,
                        y: instruction.instructions.y + distanceFromDefaultVertical,
                        tfont: instruction.instructions.font,
                        tcolor: instruction.instructions.color,
                        tbackground: instruction.instructions.background,
                        modifier: out modifier);

                }
                EndScene();
            }
            else
            {
                clear();
            }
        }


        private void showInstructionButtons(float distanceFromDefaultHorizontal, float distanceFromDefaultVertical)
        {
            var mouse_pos = Control.MousePosition;
            if (mouse_pos.X > instruction.close_button_pos.Item1 + distanceFromDefaultHorizontal &&
                mouse_pos.X < instruction.close_button_pos.Item1 + instruction.close_button_pos.Item3 + distanceFromDefaultHorizontal &&
                mouse_pos.Y > instruction.close_button_pos.Item2 + distanceFromDefaultVertical &&
                mouse_pos.Y < instruction.close_button_pos.Item2 + instruction.close_button_pos.Item4 + distanceFromDefaultVertical)
            {
                button_timer.Start();
                Direct2DBitmap close_button_red = new Direct2DBitmap(device, @"..\\..\\..\\GamingSupervisor\\buttons\" + instruction.close_button_red + ".png");

                float scale = button_timer.ElapsedMilliseconds;
                scale = scale / 2000;
                DrawBitmap(
                    bmp: close_button_red,
                    opacity: scale,
                    x: instruction.close_button_pos.Item1 + distanceFromDefaultHorizontal,
                    y: instruction.close_button_pos.Item2 + distanceFromDefaultVertical,
                    width: instruction.close_button_pos.Item3,
                    height: instruction.close_button_pos.Item4);
                close_button_red.SharpDXBitmap.Dispose();

            }
            else
            {
                button_timer.Reset();
                Direct2DBitmap close_button_black = new Direct2DBitmap(device, @"..\\..\\..\\GamingSupervisor\\buttons\" + instruction.close_button_black + ".png");
                DrawBitmap(
                    bmp: close_button_black,
                    opacity: 1,
                    x: instruction.close_button_pos.Item1 + distanceFromDefaultHorizontal,
                    y: instruction.close_button_pos.Item2 + distanceFromDefaultVertical,
                    width: instruction.close_button_pos.Item3,
                    height: instruction.close_button_pos.Item4);
                close_button_black.SharpDXBitmap.Dispose();
            }
        }
        #endregion
        
        public void clear()
        {
            BeginScene();
            ClearScene();
            EndScene();
        }
    }

    #region HeroSuggestion class

    public class HeroSuggestion
    {
        Message[] heroes = new Message[5];
        // text, left, top
        public Tuple<string, float, float> title;
        // left, top, right, bottem
        public Tuple<float, float, float, float> box_pos;

        public HeroSuggestion()
        {
        }

        public HeroSuggestion(Message[] _heroes)
        {
            float modifier_x = Screen.PrimaryScreen.Bounds.Width / 32;
            float modifier_y = Screen.PrimaryScreen.Bounds.Height / 32;
            heroes = _heroes;
            float box_left = heroes[0].img_x - modifier_x;
            float box_top = heroes[0].img_y - modifier_y * 4;
            float box_right = box_left + modifier_x * 10;
            float box_bottem = box_top + modifier_y * 24;
            box_pos = new Tuple<float, float, float, float>(box_left, box_top, box_right, box_bottem);
            float title_left = heroes[0].x - modifier_x;
            float title_top = heroes[0].y - modifier_y * 3;
            title = new Tuple<string, float, float>("Hero Suggestion:", title_left, title_top);
        }
    }
    #endregion

    #region HeroInfo class
    public class HeroInfo
    {
        public Message Info = new Message();
        // text, left, top
        public Tuple<string, float, float> title;
        // text, left, top
        public Tuple<string, float, float> content;
        // left, top, right, bottem
        public Tuple<float, float, float, float> box_pos;

        public HeroInfo()
        {
        }

        public HeroInfo(Message _hero, string _content)
        {
            float modifier_x = Screen.PrimaryScreen.Bounds.Width / 32;
            float modifier_y = Screen.PrimaryScreen.Bounds.Height / 32;
            Info = _hero;
            float box_left = Info.img_x - modifier_x;
            float box_top = Info.img_y - modifier_y * 4;
            float box_right = box_left + modifier_x * 12;
            float box_bottem = box_top + modifier_y * 24;
            box_pos = new Tuple<float, float, float, float>(box_left, box_top, box_right, box_bottem);
            float title_left = Info.x - modifier_x;
            float title_top = Info.y - modifier_y * 3;
            title = new Tuple<string, float, float>("Hero tutorial:", title_left, title_top);
            content = new Tuple<string, float, float>(_content, Info.img_x, Info.img_y + modifier_y * 4);
        }
    }
    #endregion

    #region ItemSuggestion class
    //public class ItemSuggestion
    //{
    //    Message[] heroes = new Message[5];
    //    // text, left, top
    //    public Tuple<string, float, float> title;
    //    // left, top, right, bottem
    //    public Tuple<float, float, float, float> box_pos;

    //    public ItemSuggestion()
    //    {
    //    }

    //    public ItemSuggestion(Message[] _heroes)
    //    {
    //        float modifier_x = Screen.PrimaryScreen.Bounds.Width / 32;
    //        float modifier_y = Screen.PrimaryScreen.Bounds.Height / 32;
    //        heroes = _heroes;
    //        float box_left = heroes[0].img_x - modifier_x;
    //        float box_top = heroes[0].img_y - modifier_y * 4;
    //        float box_right = box_left + modifier_x * 12;
    //        float box_bottem = box_top + modifier_y * 24;
    //        box_pos = new Tuple<float, float, float, float>(box_left, box_top, box_right, box_bottem);
    //        float title_left = heroes[0].x - modifier_x;
    //        float title_top = heroes[0].y - modifier_y * 3;
    //        title = new Tuple<string, float, float>("Item Suggestion:", title_left, title_top);
    //    }
    //}
    #endregion

    #region Instruction class
    public class Instruction
    {
        public Message instructions;
        // text, left, top
        public Tuple<string, float, float> title;
        // text, left, top
        public Tuple<string, float, float> Logo;
        // left, top, right, bottem
        public Tuple<float, float, float, float> box_pos;
        // Tuple<red, green, blue, alpha>
        public Tuple<int, int, int, int> background = new Tuple<int, int, int, int>(109, 109, 109, 255);
        public Tuple<int, int, int, int> color = new Tuple<int, int, int, int>(255, 255, 255, 255);
        // Tuple<font, size>
        public Tuple<string, int> font = new Tuple<string, int>("Consolas", 32);

        public string close_button_red = "close_button_red";
        public string close_button_black = "close_button_black";
        // x, y, width, height
        public Tuple<float, float, float, float> close_button_pos;

        public Instruction()
        {
        }

        public Instruction(Message _instructions, string _title)
        {
            float modifier_x = Screen.PrimaryScreen.Bounds.Width / 32;
            float modifier_y = Screen.PrimaryScreen.Bounds.Height / 32;
            instructions = _instructions;
            float box_left = instructions.img_x + modifier_x * 2 * Direct2DRenderer.size_scale;
            float box_top = instructions.img_y - modifier_y * 4 * Direct2DRenderer.size_scale;
            float box_right = box_left + modifier_x * 12 * Direct2DRenderer.size_scale;
            float box_bottem = box_top + modifier_y * 12* Direct2DRenderer.size_scale;
            box_pos = new Tuple<float, float, float, float>(box_left, box_top, box_right, box_bottem);
            float title_left = instructions.x + modifier_x * 3 * Direct2DRenderer.size_scale;
            float title_top = instructions.y - modifier_y * 3 * Direct2DRenderer.size_scale;
            title = new Tuple<string, float, float>(_title, title_left, title_top);
            Logo = new Tuple<string, float, float>("GamingSupervisor", box_left, box_top);
            close_button_pos = new Tuple<float, float, float, float>(box_right - modifier_x, title_top, (512 / 10) * Direct2DRenderer.size_scale, (512 / 10) * Direct2DRenderer.size_scale);
        }
    }
    #endregion

    #region Message struct
    public struct Message
    {
        public bool on;

        //// Tuple<red, green, blue, alpha>
        public Tuple<int, int, int, int> background;
        public Tuple<int, int, int, int> color;

        //// Tuple<font, size>
        public Tuple<string, int> font;

        public float x;
        public float y;
        public string text;
        public string imgName;
        public float img_x;
        public float img_y;
        public float img_width;
        public float img_height;

        public Message(string _text, string _imgName, float _x, float _y)
        {
            float modifier_x = Screen.PrimaryScreen.Bounds.Width / 32;
            float modifier_y = Screen.PrimaryScreen.Bounds.Height / 32;
            text = _text;
            imgName = _imgName;
            x = _x;
            y = _y;
            on = true;
            background = new Tuple<int, int, int, int>(109, 109, 109, 255);
            color = new Tuple<int, int, int, int>(255, 255, 255, 255);
            font = new Tuple<string, int>("Consolas", 32);
            img_x = x - Direct2DRenderer.size_scale * modifier_x * 3;
            img_y = y;
            img_width = 254 / 2;
            img_height = 144 / 2;
        }

        public void clear()
        {
            on = false;
        }
    }
    #endregion

    #region CrosshairStyle enum
    public enum CrosshairStyle
    {
        Dot,
        Plus,
        Cross,
        Gap,
        Diagonal,
        Swastika
    }
    #endregion

    #region Direct2DRendererOptions
    public struct Direct2DRendererOptions
    {
        public IntPtr Hwnd;
        public bool VSync;
        public bool MeasureFps;
        public bool AntiAliasing;
    }
    #endregion

    #region Direct2DFontCreationOptions
    public class Direct2DFontCreationOptions
    {
        public string FontFamilyName;

        public float FontSize;

        public bool Bold;

        public bool Italic;

        public bool WordWrapping;

        public FontStyle GetStyle()
        {
            if (Italic) return FontStyle.Italic;
            return FontStyle.Normal;
        }
    }
    #endregion

    #region Direct2DColor
    public struct Direct2DColor
    {
        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;

        public Direct2DColor(int red, int green, int blue)
        {
            Red = red / 255.0f;
            Green = green / 255.0f;
            Blue = blue / 255.0f;
            Alpha = 1.0f;
        }

        public Direct2DColor(int red, int green, int blue, int alpha)
        {
            Red = red / 255.0f;
            Green = green / 255.0f;
            Blue = blue / 255.0f;
            Alpha = alpha / 255.0f;
        }

        public Direct2DColor(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = 1.0f;
        }

        public Direct2DColor(float red, float green, float blue, float alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public static implicit operator RawColor4(Direct2DColor color)
        {
            return new RawColor4(color.Red, color.Green, color.Blue, color.Alpha);
        }

        public static implicit operator Direct2DColor(RawColor4 color)
        {
            return new Direct2DColor(color.R, color.G, color.B, color.A);
        }
    }
    #endregion

    #region Direct2DBrush
    public class Direct2DBrush
    {
        public Direct2DColor Color
        {
            get
            {
                return Brush.Color;
            }
            set
            {
                Brush.Color = value;
            }
        }

        public SolidColorBrush Brush;

        private Direct2DBrush()
        {
            throw new NotImplementedException();
        }

        public Direct2DBrush(RenderTarget renderTarget)
        {
            Brush = new SolidColorBrush(renderTarget, default(RawColor4));
        }

        public Direct2DBrush(RenderTarget renderTarget, Direct2DColor color)
        {
            Brush = new SolidColorBrush(renderTarget, color);
        }

        ~Direct2DBrush()
        {
            Brush.Dispose();
        }

        public static implicit operator SolidColorBrush(Direct2DBrush brush)
        {
            return brush.Brush;
        }

        public static implicit operator Direct2DColor(Direct2DBrush brush)
        {
            return brush.Color;
        }

        public static implicit operator RawColor4(Direct2DBrush brush)
        {
            return brush.Color;
        }
    }
    #endregion

    #region Direct2DFont
    public class Direct2DFont
    {
        private FontFactory factory;

        public TextFormat Font;

        public string FontFamilyName
        {
            get
            {
                return Font.FontFamilyName;
            }
            set
            {
                float size = FontSize;
                bool bold = Bold;
                FontStyle style = Italic ? FontStyle.Italic : FontStyle.Normal;
                bool wordWrapping = WordWrapping;

                Font.Dispose();

                Font = new TextFormat(factory, value, bold ? FontWeight.Bold : FontWeight.Normal, style, size);
                Font.WordWrapping = wordWrapping ? SharpDX.DirectWrite.WordWrapping.Wrap : SharpDX.DirectWrite.WordWrapping.NoWrap;
            }
        }

        public float FontSize
        {
            get
            {
                return Font.FontSize;
            }
            set
            {
                string familyName = FontFamilyName;
                bool bold = Bold;
                FontStyle style = Italic ? FontStyle.Italic : FontStyle.Normal;
                bool wordWrapping = WordWrapping;

                Font.Dispose();

                Font = new TextFormat(factory, familyName, bold ? FontWeight.Bold : FontWeight.Normal, style, value);
                Font.WordWrapping = wordWrapping ? SharpDX.DirectWrite.WordWrapping.Wrap : SharpDX.DirectWrite.WordWrapping.NoWrap;
            }
        }

        public bool Bold
        {
            get
            {
                return Font.FontWeight == FontWeight.Bold;
            }
            set
            {
                string familyName = FontFamilyName;
                float size = FontSize;
                FontStyle style = Italic ? FontStyle.Italic : FontStyle.Normal;
                bool wordWrapping = WordWrapping;

                Font.Dispose();

                Font = new TextFormat(factory, familyName, value ? FontWeight.Bold : FontWeight.Normal, style, size);
                Font.WordWrapping = wordWrapping ? SharpDX.DirectWrite.WordWrapping.Wrap : SharpDX.DirectWrite.WordWrapping.NoWrap;
            }
        }

        public bool Italic
        {
            get
            {
                return Font.FontStyle == FontStyle.Italic;
            }
            set
            {
                string familyName = FontFamilyName;
                float size = FontSize;
                bool bold = Bold;
                bool wordWrapping = WordWrapping;

                Font.Dispose();

                Font = new TextFormat(factory, familyName, bold ? FontWeight.Bold : FontWeight.Normal, value ? FontStyle.Italic : FontStyle.Normal, size);
                Font.WordWrapping = wordWrapping ? SharpDX.DirectWrite.WordWrapping.Wrap : SharpDX.DirectWrite.WordWrapping.NoWrap;
            }
        }

        public bool WordWrapping
        {
            get
            {
                return Font.WordWrapping != SharpDX.DirectWrite.WordWrapping.NoWrap;
            }
            set
            {
                Font.WordWrapping = value ? SharpDX.DirectWrite.WordWrapping.Wrap : SharpDX.DirectWrite.WordWrapping.NoWrap;
            }
        }

        private Direct2DFont()
        {
            throw new NotImplementedException();
        }

        public Direct2DFont(TextFormat font)
        {
            Font = font;
        }

        public Direct2DFont(FontFactory factory, string fontFamilyName, float size, bool bold = false, bool italic = false)
        {
            this.factory = factory;
            Font = new TextFormat(factory, fontFamilyName, bold ? FontWeight.Bold : FontWeight.Normal, italic ? FontStyle.Italic : FontStyle.Normal, size);
            Font.WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap;
        }

        ~Direct2DFont()
        {
            if (Font != null)
                Font.Dispose();
        }

        public static implicit operator TextFormat(Direct2DFont font)
        {
            return font.Font;
        }
    }
    #endregion

    #region Direct2DScene
    public class Direct2DScene : IDisposable
    {
        public Direct2DRenderer Renderer { get; private set; }

        private Direct2DScene()
        {
            throw new NotImplementedException();
        }

        public Direct2DScene(Direct2DRenderer renderer)
        {
            GC.SuppressFinalize(this);

            Renderer = renderer;
            renderer.BeginScene();
        }

        ~Direct2DScene()
        {
            Dispose(false);
        }

        public static implicit operator Direct2DRenderer(Direct2DScene scene)
        {
            return scene.Renderer;
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                Renderer.EndScene();

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
    #endregion

    #region Direct2DBitmap
    public class Direct2DBitmap
    {
        private static SharpDX.WIC.ImagingFactory factory = new SharpDX.WIC.ImagingFactory();

        public Bitmap SharpDXBitmap;

        private Direct2DBitmap()
        {

        }

        public Direct2DBitmap(RenderTarget device, byte[] bytes)
        {
            loadBitmap(device, bytes);
        }

        public Direct2DBitmap(RenderTarget device, string file)
        {
            loadBitmap(device, File.ReadAllBytes(file));
        }

        ~Direct2DBitmap()
        {
            SharpDXBitmap.Dispose();
        }

        private void loadBitmap(RenderTarget device, byte[] bytes)
        {
            var stream = new MemoryStream(bytes);
            SharpDX.WIC.BitmapDecoder decoder = new SharpDX.WIC.BitmapDecoder(factory, stream, SharpDX.WIC.DecodeOptions.CacheOnDemand);
            var frame = decoder.GetFrame(0);
            SharpDX.WIC.FormatConverter converter = new SharpDX.WIC.FormatConverter(factory);
            try
            {
                // normal ARGB images (Bitmaps / png tested)
                converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppRGBA1010102);
            }
            catch
            {
                // falling back to RGB if unsupported
                converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppRGB);
            }
            SharpDXBitmap = Bitmap.FromWicBitmap(device, converter);

            converter.Dispose();
            frame.Dispose();
            decoder.Dispose();
            stream.Dispose();
        }

        public static implicit operator Bitmap(Direct2DBitmap bmp)
        {
            return bmp.SharpDXBitmap;
        }
    }
    #endregion
}