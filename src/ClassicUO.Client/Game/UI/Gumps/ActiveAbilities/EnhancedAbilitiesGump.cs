using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Input;
using ClassicUO.Renderer;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.UI.Gumps
{

    internal class EnhancedAbilitiesGump : Gump
    {

        public static int BorderAround = 6;
        
        public static List<ActiveAbilityObject> EnhancedAbilities;

        public List<ActiveAbilityObjectControl> Abilities;

        private bool _expanding;
        public int _size;

        private Button _button;
        
        private AlphaBlendControlRounded _alphaBlendControl;
        
        
        
        
        public EnhancedAbilitiesGump() : base(0, 0)
        {
            _expanding = false;
            CanMove = true;
            CanCloseWithRightClick = true;
            AcceptMouseInput = true;

            X = ProfileManager.CurrentProfile.ActiveAbilityGumpX;
            Y = ProfileManager.CurrentProfile.ActiveAbilityGumpY;
            _size = ProfileManager.CurrentProfile.ActiveAbilityGumpSize;

        }

        private DateTime _LastMove;

        protected override void OnMove(int x, int y)
        {
            _LastMove = DateTime.UtcNow;
            
            var _currentProfile = ProfileManager.CurrentProfile;
            _currentProfile.ActiveAbilityGumpX = X;
            _currentProfile.ActiveAbilityGumpY = Y;
            
            base.OnMove(x, y);
        }

        public EnhancedAbilitiesGump(int x, int y) : this()
        {
            X = x;
            Y = y;
            
            
            SetInScreen();

            BuildGump();
            
        }

        public void Updated()
        {
            UpdateContents();
        }
        
        
        protected override void UpdateContents()
        {
            BuildGump();
        }
        
        
        private void BuildGump()
        {

            if (_size < 30)
                _size = 30;
            
            WantUpdateSize = true;

            //_box?.Clear();
            //_box?.Children.Clear();

            Clear();

            int max = 0;

            int minBackgroundWidth = 40;


            int height = 20;
            for (int i = 0; i < EnhancedAbilities.Count; i++)
            {
                int count = EnhancedAbilities[i].Abilities.Count;

                Vector2 fontsize = Fonts.Bold.MeasureString(EnhancedAbilities[i].Name);

                if (((int)fontsize.X)  > minBackgroundWidth)
                    minBackgroundWidth = (int)fontsize.X;
                
                if (count > max)
                    max = count;

                height += _size;
                height += 30;
                if (string.IsNullOrEmpty(EnhancedAbilities[i].Name))
                    height -= 25;
            }

            minBackgroundWidth = Math.Max(10 + (max * (_size + 10 + 3)), minBackgroundWidth);

            Add(_alphaBlendControl = new AlphaBlendControlRounded(
                    0.55f, minBackgroundWidth, height, 
                    15, System.Drawing.Color.Black)
            {
                CanMove = true
            });

            Abilities = new List<ActiveAbilityObjectControl>();
            int yOffset = 5;
            for (int i = 0; i < EnhancedAbilities.Count; i++)
            {
                int diffsize = 30;
                if (string.IsNullOrEmpty(EnhancedAbilities[i].Name))
                    diffsize -= 25;
                var item = new ActiveAbilityObjectControl(EnhancedAbilities[i], i, _size)
                {
                    Y = yOffset
                    //Y = (((_size + diffsize) * i) + 3)
                };

                yOffset += _size + diffsize + 3;
                Add(item);
                Abilities.Add(item);
            }
            


            Add
            (
                _button = new Button(0, 0x7585, 0x7589, 0x7589)
                {
                    ButtonAction = ButtonAction.Activate,
                    X = minBackgroundWidth - 15 ,
                    Y = height - 15
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


            UpdateElements();
        }
        
        private void UpdateElements()
        {
            /*int yoffset = 0;
            for (int i = 0, xoffset = 0; i < _box.Children.Count; i++, xoffset += 31)
            {
                Control e = _box.Children[i];

                if (5 + xoffset + 31 > _width)
                {
                    xoffset = 0;
                    yoffset += 31;
                }
                e.X = 5 + xoffset;
                e.Y = 5 + yoffset;
            }

            yoffset += 31;
            Height = yoffset + 5;
            _alphaBlendControl.Height = Height;
            _button.Y = yoffset - 7;*/
        }
        
        public override void Update()
        {
            if (Mouse.LButtonPressed && _expanding)
            {
                int max = 0;

                for (int i = 0; i < EnhancedAbilities.Count; i++)
                {
                    int count = EnhancedAbilities[i].Abilities.Count;
                    if (count > max)
                        max = count;
                }
                Width = 10 + (max * (_size + 3));
                
                
                var width = ((Mouse.Position.X - (X)) / max) - ((max * 3) - 4);

                _size = Math.Min(Math.Max(width, 40), 150);

                RequestUpdateContents();
                //minBackgroundWidth = Math.Max(10 + (max * (_size + 3)), minBackgroundWidth);
                
                var _currentProfile = ProfileManager.CurrentProfile;
                _currentProfile.ActiveAbilityGumpSize = _size;
                _LastMove = DateTime.UtcNow;
            }
            else
            {
                _expanding = false;
            }

            for (int i = 0; i < Abilities.Count; i++)
            {
                for (int j = 0; j < Abilities[i].Abilities.Count; j++)
                {
                    if ((Abilities[i].Abilities[j].Locked && Abilities[i].Abilities[j].Ability.CooldownEnd <= DateTime.UtcNow) ||
                        (Abilities[i].Abilities[j].InUse && Abilities[i].Abilities[j].Ability.InUseUntil.AddSeconds(1) <= DateTime.UtcNow))
                        
                        RequestUpdateContents();
                }
            }

            if (_LastMove != DateTime.MinValue && _LastMove.AddSeconds(1) < DateTime.UtcNow)
            {
                _LastMove = DateTime.MinValue;
                var _currentProfile = ProfileManager.CurrentProfile;
                _currentProfile?.Save(ProfileManager.ProfilePath);
            }
            
            //if ()

            base.Update();
        }
        
        public override void Save(XmlTextWriter writer)
        {
            base.Save(writer);
        }

        public override void Restore(XmlElement xml)
        {
            base.Restore(xml);

            BuildGump();
        }
        

    }
}