#region license

// Copyright (c) 2021, andreakarasho
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. All advertising materials mentioning features or use of this software
//    must display the following acknowledgement:
//    This product includes software developed by andreakarasho - https://github.com/andreakarasho
// 4. Neither the name of the copyright holder nor the
//    names of its contributors may be used to endorse or promote products
//    derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS ''AS IS'' AND ANY
// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using ClassicUO.Game.Managers;
using ClassicUO.Game.Scenes;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Renderer;
using ClassicUO.Resources;
using ClassicUO.Utility;
using SDL2;

namespace ClassicUO.Game.UI.Controls
{
    internal class MacroControl : Control
    {
        private static string[] _enabledMacrosNames;
        private static MacroType[] _enabledMacroTypes;
        //private static readonly string[] _allSubHotkeysNames = Enum.GetNames(typeof(MacroSubType)).CamelSpace();
        private readonly DataBox _databox;
        private readonly HotkeyBox _hotkeyBox;

        static MacroControl()
        {
            GenerateNames();
        }

        public static void GenerateNames()
        {
            var allValues = Enum.GetValues(typeof(MacroType));
            var names = new List<string>();
            var types = new List<MacroType>();

            for (int i = 0; i < allValues.Length; i++)
            {
                //if ((MacroType)allValues.GetValue[i] == MacroType.Aura)
                var macroType = (MacroType)allValues.GetValue(i);

                switch (macroType)
                {
                    case MacroType.EnhancedAbilitiesBySlot:
                        if (World.Settings.GeneralFlags.EnableEnhancedAbilities && World.Settings.MacroFlags.EnhancedAbilities)
                            goto default;
                        break;
                    case MacroType.SallosTargeting:
                        if (World.Settings.MacroFlags.AllowSallosTargeting)
                            goto default;
                        break;
                    case MacroType.UsePotion:
                        if (!World.Settings.MacroFlags.EnhancedPotionMacros)
                            goto default;
                        break;
                    case MacroType.UsePotionEnhanced:
                        if (World.Settings.MacroFlags.EnhancedPotionMacros)
                            goto default;
                        break;
                    default:
                    { 
                        names.Add(macroType.ToString().CamelSpace());
                        types.Add(macroType);
                        break;
                    }
                }
            }

            _enabledMacrosNames = names.ToArray();
            _enabledMacroTypes = types.ToArray();
        }

        private enum buttonsOption
        {
            AddBtn,
            RemoveBtn,
            CreateNewMacro,
            OpenMacroOptions
        }

        public MacroControl(string name, bool isFastAssign = false)
        {
            CanMove = true;

            _hotkeyBox = new HotkeyBox();
            _hotkeyBox.HotkeyChanged += BoxOnHotkeyChanged;
            _hotkeyBox.HotkeyCancelled += BoxOnHotkeyCancelled;


            Add(_hotkeyBox);

            Add
            (
                new NiceButton
                (
                    0,
                    _hotkeyBox.Height + 3,
                    170,
                    25,
                    ButtonAction.Activate,
                    ResGumps.CreateMacroButton,
                    0,
                    TEXT_ALIGN_TYPE.TS_LEFT
                ) { ButtonParameter = (int)buttonsOption.CreateNewMacro, IsSelectable = false }
            );

            if(!isFastAssign)
            {
                Add
                (
                    new NiceButton
                    (
                        0,
                        _hotkeyBox.Height + 30,
                        50,
                        25,
                        ButtonAction.Activate,
                        ResGumps.Add
                    )
                    { ButtonParameter = (int)buttonsOption.AddBtn, IsSelectable = false }
                );

                Add
                (
                    new NiceButton
                    (
                        52,
                        _hotkeyBox.Height + 30,
                        50,
                        25,
                        ButtonAction.Activate,
                        ResGumps.Remove,
                        0,
                        TEXT_ALIGN_TYPE.TS_LEFT
                    )
                    { ButtonParameter = (int)buttonsOption.RemoveBtn, IsSelectable = false }
                );
            } else {
                Add
                (
                    new NiceButton
                    (
                        0,
                        _hotkeyBox.Height + 30,
                        170,
                        25,
                        ButtonAction.Activate,
                        ResGumps.OpenMacroSettings
                    )
                    { ButtonParameter = (int)buttonsOption.OpenMacroOptions, IsSelectable = false }
                );
            }

            var scrollAreaH = isFastAssign ? 80 : 280;
            var scrollAreaW = isFastAssign ? 230 : 280;

            ScrollArea area = new ScrollArea
            (
                10,
                _hotkeyBox.Bounds.Bottom + 80,
                scrollAreaW,
                scrollAreaH,
                true
            );

            Add(area);

            _databox = new DataBox(0, 0, 280, 280)
            {
                WantUpdateSize = true
            };
            area.Add(_databox);


            Macro = Client.Game.GetScene<GameScene>().Macros.FindMacro(name) ?? Macro.CreateEmptyMacro(name);

            SetupKeyByDefault();
            SetupMacroUI();
        }


        public Macro Macro { get; }

        private void AddEmptyMacro()
        {
            MacroObject ob = (MacroObject) Macro.Items;

            if (ob.Code == MacroType.None)
            {
                return;
            }

            while (ob.Next != null)
            {
                MacroObject next = (MacroObject) ob.Next;

                if (next.Code == MacroType.None)
                {
                    return;
                }

                ob = next;
            }

            MacroObject obj = Macro.Create(MacroType.None);

            Macro.PushToBack(obj);

            _databox.Add(new MacroEntry(this, obj, _enabledMacrosNames));
            _databox.WantUpdateSize = true;
            _databox.ReArrangeChildren();
        }

        private void RemoveLastCommand()
        {
            if (_databox.Children.Count != 0)
            {
                LinkedObject last = Macro.GetLast();

                Macro.Remove(last);

                _databox.Children[_databox.Children.Count - 1].Dispose();

                SetupMacroUI();
            }

            if (_databox.Children.Count == 0)
            {
                AddEmptyMacro();
            }
        }

        private void SetupMacroUI()
        {
            if (Macro == null)
            {
                return;
            }

            _databox.Clear();
            _databox.Children.Clear();

            if (Macro.Items == null)
            {
                Macro.Items = Macro.Create(MacroType.None);
            }

            MacroObject obj = (MacroObject) Macro.Items;

            while (obj != null)
            {
                _databox.Add(new MacroEntry(this, obj, _enabledMacrosNames));

                if (obj.Next != null && obj.Code == MacroType.None)
                {
                    break;
                }

                obj = (MacroObject) obj.Next;
            }

            _databox.WantUpdateSize = true;
            _databox.ReArrangeChildren();
        }

        private void SetupKeyByDefault()
        {
            if (Macro == null || _hotkeyBox == null)
            {
                return;
            }

            SDL.SDL_Keymod mod = SDL.SDL_Keymod.KMOD_NONE;

            if (Macro.Alt)
            {
                mod |= SDL.SDL_Keymod.KMOD_ALT;
            }

            if (Macro.Shift)
            {
                mod |= SDL.SDL_Keymod.KMOD_SHIFT;
            }

            if (Macro.Ctrl)
            {
                mod |= SDL.SDL_Keymod.KMOD_CTRL;
            }

            if (Macro.Key != SDL.SDL_Keycode.SDLK_UNKNOWN)
            {
                _hotkeyBox.SetKey(Macro.Key, mod);
            }
            else if (Macro.MouseButton != MouseButtonType.None)
            {
                _hotkeyBox.SetMouseButton(Macro.MouseButton, mod);
            }
            else if (Macro.WheelScroll == true)
            {
                _hotkeyBox.SetMouseWheel(Macro.WheelUp, mod);
            }
        }

        private void BoxOnHotkeyChanged(object sender, EventArgs e)
        {
            bool shift = (_hotkeyBox.Mod & SDL.SDL_Keymod.KMOD_SHIFT) != SDL.SDL_Keymod.KMOD_NONE;
            bool alt = (_hotkeyBox.Mod & SDL.SDL_Keymod.KMOD_ALT) != SDL.SDL_Keymod.KMOD_NONE;
            bool ctrl = (_hotkeyBox.Mod & SDL.SDL_Keymod.KMOD_CTRL) != SDL.SDL_Keymod.KMOD_NONE;

            if (_hotkeyBox.Key != SDL.SDL_Keycode.SDLK_UNKNOWN)
            {
                Macro macro = Client.Game.GetScene<GameScene>().Macros.FindMacro(_hotkeyBox.Key, alt, ctrl, shift);

                if (macro != null)
                {
                    if (Macro == macro)
                    {
                        return;
                    }

                    SetupKeyByDefault();
                    UIManager.Add(new MessageBoxGump(250, 150, string.Format(ResGumps.ThisKeyCombinationAlreadyExists, macro.Name), null));

                    return;
                }
            }
            else if (_hotkeyBox.MouseButton != MouseButtonType.None)
            {
                Macro macro = Client.Game.GetScene<GameScene>().Macros.FindMacro(_hotkeyBox.MouseButton, alt, ctrl, shift);

                if (macro != null)
                {
                    if (Macro == macro)
                    {
                        return;
                    }

                    SetupKeyByDefault();
                    UIManager.Add(new MessageBoxGump(250, 150, string.Format(ResGumps.ThisKeyCombinationAlreadyExists, macro.Name), null));

                    return;
                }
            }
            else if (_hotkeyBox.WheelScroll == true)
            {
                Macro macro = Client.Game.GetScene<GameScene>().Macros.FindMacro(_hotkeyBox.WheelUp, alt, ctrl, shift);

                if (macro != null)
                {
                    if (Macro == macro)
                    {
                        return;
                    }

                    SetupKeyByDefault();
                    UIManager.Add(new MessageBoxGump(250, 150, string.Format(ResGumps.ThisKeyCombinationAlreadyExists, macro.Name), null));

                    return;
                }
            }
            else
            {
                return;
            }

            Macro m = Macro;
            m.Key = _hotkeyBox.Key;
            m.MouseButton = _hotkeyBox.MouseButton;
            m.WheelScroll = _hotkeyBox.WheelScroll;
            m.WheelUp = _hotkeyBox.WheelUp;
            m.Shift = shift;
            m.Alt = alt;
            m.Ctrl = ctrl;
        }

        private void BoxOnHotkeyCancelled(object sender, EventArgs e)
        {
            Macro m = Macro;
            m.Alt = m.Ctrl = m.Shift = false;
            m.Key = SDL.SDL_Keycode.SDLK_UNKNOWN;
            m.MouseButton = MouseButtonType.None;
            m.WheelScroll = false;
        }

        public override void OnButtonClick(int buttonID)
        {
            switch (buttonID)
            {
                case (int)buttonsOption.AddBtn:
                    AddEmptyMacro();
                    break;
                case (int)buttonsOption.RemoveBtn:
                    RemoveLastCommand();
                    break;
                case (int)buttonsOption.CreateNewMacro:
                    UIManager.Gumps.OfType<MacroButtonGump>().FirstOrDefault(s => s._macro == Macro)?.Dispose();

                    MacroButtonGump macroButtonGump = new MacroButtonGump(Macro, Mouse.Position.X, Mouse.Position.Y);
                    UIManager.Add(macroButtonGump);
                    break;
                case (int)buttonsOption.OpenMacroOptions:
                    UIManager.Gumps.OfType<MacroGump>().FirstOrDefault()?.Dispose();

                    GameActions.OpenSettings(4);
                    break;
            }
        }


        private class MacroEntry : Control
        {
            private readonly MacroControl _control;
            private readonly string[] _items;

            public MacroEntry(MacroControl control, MacroObject obj, string[] items)
            {
                _control = control;
                _items = items;
                int index = 0;

                for (int i = 0; i < items.Length; i++)
                {
                    if (_enabledMacroTypes[i] == obj.Code)
                    {
                        index = i;
                        break;
                    }
                }

                Combobox mainBox = new Combobox
                (
                    0,
                    0,
                    200,
                    _items,
                    index
                )
                {
                    Tag = obj
                };

                mainBox.OnOptionSelected += BoxOnOnOptionSelected;

                Add(mainBox);

                Width = mainBox.Width;
                Height = mainBox.Height;

                AddSubMacro(obj);

                WantUpdateSize = true;
            }


            private void AddSubMacro(MacroObject obj)
            {
                if (obj == null || obj.Code == 0)
                {
                    return;
                }

                switch (obj.SubMenuType)
                {
                    case 1:
                    {
                        int count = 0;
                        int offset = 0;
                        Macro.GetBoundByCode(obj.Code, ref count, ref offset);

                        string[] names = new string[count];

                        for (int i = 0; i < count; i++)
                        {
                            //names[i] = _allSubHotkeysNames[i + offset];
                            names[i] = ((MacroSubType)(i + offset)).ToString().CamelSpace();
                        }

                        Combobox sub = new Combobox(20, Height, 180, names, (int)obj.SubCode - offset, 300);

                        sub.OnOptionSelected += (senderr, ee) =>
                        {
                            Macro.GetBoundByCode(obj.Code, ref count, ref offset);
                            MacroSubType subType = (MacroSubType)(offset + ee);
                            obj.SubCode = subType;
                        };

                        Add(sub);

                        Height += sub.Height;


                        break;
                    }

                    case 2:
                    {
                        ResizePic background = new ResizePic(0x0BB8)
                        {
                            X = 16,
                            Y = Height,
                            Width = 240,
                            Height = 60
                        };

                        Add(background);

                        StbTextBox textbox = new StbTextBox(0xFF, 80, 236, true, FontStyle.BlackBorder)
                        {
                            X = background.X + 4,
                            Y = background.Y + 4,
                            Width = background.Width - 4,
                            Height = background.Height - 4
                        };

                        textbox.SetText(obj.HasString() ? ((MacroObjectString)obj).Text : string.Empty);

                        textbox.TextChanged += (sss, eee) =>
                        {
                            if (obj.HasString())
                            {
                                ((MacroObjectString)obj).Text = ((StbTextBox)sss).Text;
                            }
                        };

                        Add(textbox);

                        WantUpdateSize = true;
                        Height += background.Height;

                        break;
                    }
                    case 3:
                        {
                            int count = 0;
                            int offset = 0;
                            Macro.GetBoundByCode(obj.Code, ref count, ref offset);

                            string[] names = new string[count];

                            for (int i = 0; i < count; i++)
                            {
                                names[i] = ((MacroSubType)(i + offset)).ToString().CamelSpace(); //_allSubHotkeysNames[i + offset];
                            }

                            Combobox sub = new Combobox
                            (
                                20, Height, 180, names,
                                (int)obj.SubCode - offset, 300
                            );

                            sub.OnOptionSelected += (senderr, ee) =>
                            {
                                Macro.GetBoundByCode(obj.Code, ref count, ref offset);
                                MacroSubType subType = (MacroSubType)(offset + ee);
                                obj.SubCode = subType;
                            };

                            Add(sub);

                            Height += sub.Height;

                            int countz = 0;
                            int offsetz = 0;
                            Macro.GetSecondaryBoundByCode(obj.Code, ref countz, ref offsetz);

                            names = new string[countz];

                            for (int i = 0; i < countz; i++)
                            {
                                names[i] = ((MacroSubType)(i + offsetz)).ToString().CamelSpace(); //_allSubHotkeysNames[i + offsetz + 1];
                            }

                            sub = new Combobox
                            (
                                20,
                                Height,
                                180,
                                names,
                                (int)obj.SubSubCode - offsetz,
                                300
                            );

                            sub.OnOptionSelected += (senderr, eez) =>
                            {
                                Macro.GetSecondaryBoundByCode(obj.Code, ref countz, ref offsetz);
                                MacroSubType subType = (MacroSubType)(offsetz + eez);
                                obj.SubSubCode = subType;
                            };

                            Add(sub);

                            Height += sub.Height;


                            break;
                        }

                    case 4:
                        {
                            //Completely custom list..

                            int count = 0;
                            int offset = 0;
                            ///Macro.GetBoundByCode(obj.Code, ref count, ref offset);

                            /*string[] names = new string[count];

                            for (int i = 0; i < count; i++)
                            {
                                names[i] = _allSubHotkeysNames[i + offset];
                            }*/

                            string[] names = null;
                            string[] subNames = null;
                            int index = 0;

                            switch (obj.Code)
                            {
                                case MacroType.UsePotionEnhanced:
                                    {
                                        names = new string[World.Settings.Potions.Count];

                                        for (int i = 0; i < World.Settings.Potions.Count; i++)
                                        {
                                            names[i] = World.Settings.Potions[i].Name;

                                            if (World.Settings.Potions[i].ID == (int)obj.SubCode)
                                                index = i;
                                        }
                                        break;
                                    }

                                case MacroType.SallosTargeting:
                                    {
                                        names = new string[]
                                        {
                                            "Find", "Target", "Acquire"
                                        };
                                        index = (int)obj.SubCode;
                                        break;
                                    }

                                case MacroType.EnhancedAbilitiesBySlot:
                                {
                                    names = new string[10];
                                    subNames = new string[10];
                                    
                                    for (int i = 0; i < 10; i++)
                                    {
                                        names[i] = $"Slot {(i + 1)}";
                                        subNames[i] = $"Ability {(i + 1)}";
                                    }
                                    index = (int)obj.SubCode;

                                    break;
                                }
                            }


                           /*for (int i = 0; i < World.Settings.Potions.Count; i++)
                            {
                                if (World.Settings.Potions[i].ID == (int)obj.SubCode)
                                {
                                    index = i;

                                    break;
                                }
                            }*/

                            if (names == null || names.Length == 0)
                                names = new string[1] { "Invalid Options" };


                            Combobox sub = new Combobox
                            (
                                20, Height, 180, names,
                                index, 300
                            );

                            if (obj.Code == MacroType.UsePotionEnhanced)
                            {
                                sub.OnOptionSelected += (senderr, ee) => { obj.SubCode = (MacroSubType)World.Settings.Potions[ee].ID; };
                            }
                            else
                            {
                                sub.OnOptionSelected += (senderr, ee) => { obj.SubCode = (MacroSubType)ee; };
                            }

                            Add(sub);

                            Height += sub.Height;

                            if (subNames != null)
                            {

                                sub = new Combobox
                                (
                                    20,
                                    Height,
                                    180,
                                    subNames,
                                    (int)obj.SubSubCode,
                                    300
                                );

                                sub.OnOptionSelected += (senderr, ee) =>
                                {
                                    Macro.GetSecondaryBoundByCode(obj.Code, ref count, ref offset);
                                    MacroSubType subType = (MacroSubType)(offset + ee);
                                    obj.SubSubCode = subType;
                                };

                                Add(sub);

                                Height += sub.Height;
                            }

                            break;
                        }
                }
                

                _control._databox.ReArrangeChildren();
            }


            private void BoxOnOnOptionSelected(object sender, int val)
            {
                var e = _enabledMacroTypes[val];
                WantUpdateSize = true;

                Combobox box = (Combobox) sender;
                MacroObject currentMacroObj = (MacroObject) box.Tag;

                if (e == 0)
                {
                    _control.Macro.Remove(currentMacroObj);

                    box.Tag = null;

                    Dispose();

                    _control.SetupMacroUI();
                }
                else
                {
                    MacroObject newMacroObj = Macro.Create((MacroType) e);

                    _control.Macro.Insert(currentMacroObj, newMacroObj);
                    _control.Macro.Remove(currentMacroObj);

                    box.Tag = newMacroObj;


                    for (int i = 1; i < Children.Count; i++)
                    {
                        Children[i]?.Dispose();
                    }

                    Height = box.Height;

                    AddSubMacro(newMacroObj);
                }
            }
        }
    }
}