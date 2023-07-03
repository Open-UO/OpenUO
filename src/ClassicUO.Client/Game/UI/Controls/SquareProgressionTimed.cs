#region license



#endregion

using System;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace ClassicUO.Game.UI.Controls
{
    internal sealed class SquareProgressionTimed : Control
    {
        //int Minimum = 0;
        //int Maximum = 100;
        //int Value = 1;
        public string TextLabel { get; set; } = "";

        public System.Drawing.Color TrackColor { get; set; } = System.Drawing.Color.Transparent;
        public System.Drawing.Color ValueColor { get; set; } = System.Drawing.Color.White;

        public int TrackSize { get; set; } = 15;

        private int _CircleOffsetHeight { get; set; } = 15;

        private DateTime _ExpiryTime;
        private DateTime _StartTime;
        public double _Seconds;

        public bool TextInMiddle { get; set; } = false;


        private Texture2D GetTexture(GraphicsDevice dev, System.Drawing.Bitmap bmp)
        {
            int[] imgData = new int[bmp.Width * bmp.Height];
            Texture2D texture = new Texture2D(dev, bmp.Width, bmp.Height);

            unsafe
            {
                // lock bitmap
                System.Drawing.Imaging.BitmapData origdata =
                    bmp.LockBits(new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);

                uint* byteData = (uint*)origdata.Scan0;

                // Switch bgra -> rgba
                for (int i = 0; i < imgData.Length; i++)
                {
                    byteData[i] = (byteData[i] & 0x000000ff) << 16 | (byteData[i] & 0x0000FF00) | (byteData[i] & 0x00FF0000) >> 16 | (byteData[i] & 0xFF000000);
                }

                // copy data
                System.Runtime.InteropServices.Marshal.Copy(origdata.Scan0, imgData, 0, bmp.Width * bmp.Height);

                byteData = null;

                // unlock bitmap
                bmp.UnlockBits(origdata);
            }

            texture.SetData(imgData);

            return texture;
        }
        
        

        private Bitmap DrawArc(int width, int height, double seconds)
        {
            Bitmap bmp = new Bitmap(width, height);
            //Value++;

            //if (Value > Maximum)
            //    Value = 0;
            bool _showValue = true;

            using (Graphics newGraphics = Graphics.FromImage(bmp))
            {

                var g = newGraphics;
                var r = new System.Drawing.Rectangle(TrackSize, TrackSize,
                                                     Math.Max(TrackSize, width - (TrackSize * 2)),
                                                     Math.Max(TrackSize, (height - (TrackSize * 2))));

                using (var pnTrack = new System.Drawing.Pen(TrackColor, TrackSize * 5))
                {
                    g.Clear(System.Drawing.Color.Transparent);
                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.PixelOffsetMode = PixelOffsetMode.Half;
                    
                    //g.DrawArc(pnTrack, r, 180, 360);

                    //if (Value > Minimum)
                    if (seconds > 0)
                    {
                        var val = (int)Math.Round((360d * (seconds / _Seconds)));
                        pnTrack.Color = ValueColor;
                        // pnTrack.Width -= 2; // uncomment and try...
                        g.DrawArc(pnTrack, r, 270, -val);
                    }
                }
            }

            return bmp;
        }


        public SquareProgressionTimed(int radius, DateTime start, DateTime finish, int itemId, int itemHue, int previewOffsetX, int previewOffsetY, int circleOffsetHeight)
        {
            //_Seconds = seconds;
            _CircleOffsetHeight = circleOffsetHeight;
            Width = radius * 2;
            Height = radius * 2 + _CircleOffsetHeight;
            AcceptMouseInput = false;
            _StartTime = start;
            _ExpiryTime = finish;
            _Seconds = (finish - start).TotalSeconds;

            //CountDownUntil = DateTime.UtcNow + TimeSpan.FromSeconds(_Seconds);


            if (itemId > 0)
            {
                var item = new StaticPic((ushort)itemId, (ushort)itemHue);
                //Add(item);
                _Preview = item;
                _PreviewOffset = new Vector2(previewOffsetX, previewOffsetY);
            }

            //Texture = );
        }

        private StaticPic _Preview;
        private Vector2 _PreviewOffset;

        public ushort CircleHue { get; set; } = 0;
        public ushort CountdownHue { get; set; } = 0;
        public ushort TextHue { get; set; } = 0;
        public Texture2D Texture { get; set; }

        public bool Round { get; set; } = true;
        public int Precision { get; set; } = 1;

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            if (!IsDisposed)
            {

                if (_Preview != null)
                {
                    Vector2 itemOffset;
                    itemOffset.X = (int)(Width / 2f - _Preview.Width / 2f);
                    itemOffset.Y = (int)((Height - _CircleOffsetHeight) / 2f - _Preview.Height / 2f) + _CircleOffsetHeight;
                    _Preview.Draw(batcher, x + (int)itemOffset.X + (int)_PreviewOffset.X, y + (int)itemOffset.Y + (int)_PreviewOffset.Y);
                }

                //if (!base.Draw(batcher, x, y))
                //    return false;

                var time = (_ExpiryTime - DateTime.UtcNow).TotalSeconds;

                if (time < 0)
                {
                    Dispose();
                    return true;
                }

                string text = "";

                if (Round)
                {
                    if (time >= 3600)
                        text = $"{Math.Round(time / 3600, Precision)}h";
                    else if (time >= 60)
                        text = $"{Math.Round(time / 60, Precision)}m";
                    else
                        text = $"{Math.Round(time, Precision)}s";
                }
                else
                {
                    if (time >= 3600)
                        text = $"{Math.Round(Math.Floor(time / 360) / 10, Precision)}h";
                    else if (time >= 60)
                        text = $"{Math.Round(Math.Floor(time / 6) / 10, Precision)}m";
                    else
                        text = $"{Math.Round(Math.Floor(time * 10) / 10, Precision)}s";
                }

                Vector3 hueVec = ShaderHueTranslator.GetHueVector(CircleHue, false, Alpha);

                Texture?.Dispose();
                Texture = GetTexture(Client.Game.GraphicsDevice, DrawArc(Width, Height - _CircleOffsetHeight, time));

                batcher.Draw(Texture, new Rectangle(x, y + _CircleOffsetHeight, Width, Height - _CircleOffsetHeight), hueVec);
                Vector2 size = Fonts.Bold.MeasureString(text);
                Vector2 _offset;
                _offset.X = (int)(Width / 2f - size.X / 2);
                _offset.Y = 0;

                if (TextInMiddle)
                {
                    _offset.Y = (int)((Height - _CircleOffsetHeight) / 2f - size.Y / 2) + _CircleOffsetHeight;
                }

                batcher.DrawString
                (
                    Fonts.Bold, text, (int)_offset.X + x, (int)_offset.Y + y,
                    ShaderHueTranslator.GetHueVector(CountdownHue, false, 1)
                );

                if (!string.IsNullOrEmpty(TextLabel))
                {
                    size = Fonts.Bold.MeasureString(TextLabel);
                    _offset.X = (int)(Width / 2f - size.X / 2);
                    _offset.Y = (int)((Height - _CircleOffsetHeight) / 2f - size.Y / 2) + _CircleOffsetHeight;
                    batcher.DrawString
                    (
                        Fonts.Bold, TextLabel, (int)_offset.X + x, (int)_offset.Y + y,
                        ShaderHueTranslator.GetHueVector(TextHue, false, 1)
                    );
                }
            }

            return true;
        }
    }
}