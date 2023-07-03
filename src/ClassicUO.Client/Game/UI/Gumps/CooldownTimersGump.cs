using System;
using System.Linq;
using System.Xml;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{
    internal class CooldownTimersGump : Gump
    {
        private GumpPic _background;
        private Button _button;
        private ushort _graphic;
        private DataBox _box;
        private int _width = 200;
        private bool _expanding;
        private AlphaBlendControl _alphaBlendControl;

        public CooldownTimersGump() : base(0, 0)
        {
            CanMove = true;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;
        }

        public CooldownTimersGump(int x, int y) : this()
        {
            _expanding = false;
            X = x;
            Y = y;

            _graphic = 0x7580;


            SetInScreen();

            BuildGump();
        }

        public override GumpType GumpType => GumpType.CooldownTimers;

        private void BuildGump()
        {
            WantUpdateSize = true;

            _box?.Clear();
            _box?.Children.Clear();

            Clear();



            Add(_alphaBlendControl = new AlphaBlendControl(0.1f, Color.Coral)
            {
                Width = _width,
                Height = 100,
            });


            Add
            (
                _button = new Button(0, 0x7585, 0x7589, 0x7589)
                {
                    ButtonAction = ButtonAction.Activate,
                    X = _width - 13,
                    Y = 100 - 13
                }
            );

            _button.MouseDown += (sender, args) =>
            {
                _expanding = true;
            };

            _button.MouseUp += (sender, args) =>
            {
                _expanding = false;
            };


            Add
            (
                _box = new DataBox(0, 0, 0, 0)
                {
                    WantUpdateSize = true
                }
            );

            if (World.Player != null)
            {
                foreach (CooldownTimer k in World.Player.CooldownTimers)
                {
                    //_box.Add(new BuffControlEntry(World.Player.BuffIcons[k.Key]));
                    _box.Add(new CircularProgressionTimed(20, k.StartTime, k.ExpiryTime, k.ItemID,
                                                           k.ItemHue, k.OffsetX, k.OffsetY, 15)
                    {
                        TextLabel = k.Text,
                        TrackSize = 3,
                        CountdownHue = k.CountdownHue,
                        CircleHue = k.CircleHue,
                        TextHue = k.TextHue,

                    });
                }
            }


            UpdateElements();
        }

        public override bool Draw(UltimaBatcher2D batcher, int x, int y)
        {
            Vector3 hue_vec = ShaderHueTranslator.GetHueVector(0, false, 1);
            batcher.DrawRectangle
            (
                SolidColorTextureCache.GetTexture(Color.Gray),
                x,
                y,
                _alphaBlendControl.Width,
                _alphaBlendControl.Height,
                hue_vec
            );
            return base.Draw(batcher, x, y);

        }

        public override void Save(XmlTextWriter writer)
        {
            base.Save(writer);
            writer.WriteAttributeString("cooldowngraphic", _graphic.ToString());
            writer.WriteAttributeString("cooldownwidth", ((int)_width).ToString());
        }

        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);

            _graphic = ushort.Parse(xml.GetAttribute("cooldowngraphic"));

            try
            {
                _width = Math.Max(int.Parse(xml.GetAttribute("cooldownwidth")), 40);
            }
            catch
            {
                _width = 200;
            }

            BuildGump();
        }

        protected override void UpdateContents()
        {
            BuildGump();
        }

        private void UpdateElements()
        {
            int yoffset = 0;
            for (int i = 0, xoffset = 0; i < _box.Children.Count; i++)
            {
                Control e = _box.Children[i];

                if (5 + xoffset + _box.Width + 2 > _width)
                {
                    xoffset = 0;
                    yoffset += e.Height;
                }
                e.X = 5 + xoffset;
                e.Y = 5 + yoffset;

                xoffset += e.Width + 2;
            }

            yoffset += 55;
            Height = yoffset + 5;
            _alphaBlendControl.Height = Height;
            _button.Y = yoffset - 7;
        }

        public override void Update()
        {

            if (Mouse.LButtonPressed && _expanding)
            {
                var width = Mouse.Position.X - (X - 13);

                _width = Width = Math.Max(width, 60);

                RequestUpdateContents();
            }
            else
            {
                _expanding = false;
            }

            if (World.Player == null)
                return;
            foreach (CooldownTimer k in World.Player.CooldownTimers.ToList())
            {
                if (k.IsExpired)
                {
                    World.Player.CooldownTimers.Remove(k);
                    RequestUpdateContents();
                }
            }
            base.Update();
        }


    }
}