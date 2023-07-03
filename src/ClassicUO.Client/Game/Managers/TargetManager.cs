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
using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.Assets;
using ClassicUO.Network;
using ClassicUO.Resources;
using ClassicUO.Utility;
using Microsoft.Xna.Framework;

namespace ClassicUO.Game.Managers
{
    internal enum CursorTarget
    {
        Invalid = -1,
        Object = 0,
        Position = 1,
        MultiPlacement = 2,
        SetTargetClientSide = 3,
        Grab,
        SetGrabBag,
        HueCommandTarget,
        IgnorePlayerTarget,
        FriendTarget,
        Dummy
    }

    internal class CursorType
    {
        public static readonly uint Target = 6983686;
    }

    internal enum TargetType
    {
        Neutral,
        Harmful,
        Beneficial,
        Cancel
    }

    internal class MultiTargetInfo
    {
        public MultiTargetInfo(ushort model, ushort x, ushort y, short z, ushort hue)
        {
            Model = model;
            XOff = x;
            YOff = y;
            ZOff = z;
            Hue = hue;
        }

        public short ZOff;
        public ushort XOff, YOff, Model, Hue;
    }

    internal class LastTargetInfo
    {
        public bool IsEntity => SerialHelper.IsValid(Serial);
        public bool IsStatic => !IsEntity && Graphic != 0 && Graphic != 0xFFFF;
        public bool IsLand => !IsStatic;
        public ushort Graphic;
        public uint Serial;
        public ushort X, Y;
        public sbyte Z;


        public void SetEntity(uint serial)
        {
            Serial = serial;
            Graphic = 0xFFFF;
            X = Y = 0xFFFF;
            Z = sbyte.MinValue;
        }

        public void SetStatic(ushort graphic, ushort x, ushort y, sbyte z)
        {
            Serial = 0;
            Graphic = graphic;
            X = x;
            Y = y;
            Z = z;
        }

        public void SetLand(ushort x, ushort y, sbyte z)
        {
            Serial = 0;
            Graphic = 0xFFFF;
            X = x;
            Y = y;
            Z = z;
        }

        public void Clear()
        {
            Serial = 0;
            Graphic = 0xFFFF;
            X = Y = 0xFFFF;
            Z = sbyte.MinValue;
        }
    }
    
    
    internal class StoredTarget
    {
        public CursorTarget TargetingState { get; private set; }
        public uint TargetCursorID { get; private set; }
        public MultiTargetInfo MultiTargetInfo { get; private set; }
        public TargetType TargetingType { get; private set; }
        public bool IsAoE { get; private set; }
        public object AoERange { get; private set; }
        public int AoEType { get; private set; }
        public uint Preview { get; private set; }
        public ushort PreviewHue { get; private set; }

        public StoredTarget(CursorTarget targetingState, uint targetCursorID, MultiTargetInfo multiTargetInfo, 
                            TargetType targetingType, bool isAoE, object AOERange, int AOEType, uint preview,
                            ushort previewHue)
        {
            TargetingState = targetingState;
            TargetCursorID = targetCursorID;
            MultiTargetInfo = multiTargetInfo;
            TargetingType = targetingType;
            IsAoE = isAoE;
            AoERange = AOERange;
            AoEType = AOEType;
            Preview = preview;
            PreviewHue = previewHue;
        }
    }

    internal static class TargetManager
    {
        private static uint _targetCursorId;
        private static readonly byte[] _lastDataBuffer = new byte[19];

        public static uint LastAttack, SelectedTarget;

        public static readonly LastTargetInfo LastTargetInfo = new LastTargetInfo();
        public static readonly LastTargetInfo LastBeneficialTargetInfo = new LastTargetInfo();

        public static MultiTargetInfo MultiTargetInfo { get; set; }

        public static CursorTarget TargetingState { get; set; } = CursorTarget.Invalid;

        public static bool IsTargeting { get; private set; }

        public static TargetType TargetingType { get; private set; }
        public static object ExtraDetails { get; private set; }
        public static bool IsAoE { get; private set; }
        public static int AoEType { get; private set; }
        public static uint Preview { get; private set; }
        public static ushort PreviewHue { get; private set; }

        public static List<StoredTarget> StoredTargets { get; set; } = new List<StoredTarget>();

        public static void StoreTarget()
        {
            if (IsTargeting)
            {
                StoredTargets.Add(
                    new StoredTarget(TargetingState, _targetCursorId, MultiTargetInfo, 
                                     TargetingType, IsAoE, ExtraDetails, AoEType, Preview, PreviewHue));
                ClearTargetingWithoutTargetCancelPacket(false);
            }
            
        }

        public static void RestoreTarget()
        {
            if (StoredTargets.Count > 0)
            {
                var targ = StoredTargets[0];
                StoredTargets.RemoveAt(0);

                TargetingState = targ.TargetingState;
                _targetCursorId = targ.TargetCursorID;
                TargetingType = targ.TargetingType;
                MultiTargetInfo = targ.MultiTargetInfo;
                IsAoE = targ.IsAoE;
                ExtraDetails = targ.AoERange;
                AoEType = targ.AoEType;
                Preview = targ.Preview;
                PreviewHue = targ.PreviewHue;
                
                IsTargeting = targ.TargetingType < TargetType.Cancel;
            }
        }

        private static void ClearTargetingWithoutTargetCancelPacket(bool restore = true)
        {
            if (TargetingState == CursorTarget.MultiPlacement)
            {
                MultiTargetInfo = null;
                TargetingState = 0;
                World.HouseManager.Remove(0);
            }

            IsTargeting = false;
            
            IsAoE = false;
            ExtraDetails = 0;
            AoEType = 0;
            Preview = 0;
            PreviewHue = 0;
            
            if (restore)
                RestoreTarget();
        }

        public static void Reset()
        {
            ClearTargetingWithoutTargetCancelPacket();

            TargetingState = 0;
            _targetCursorId = 0;
            MultiTargetInfo = null;
            TargetingType = 0;
            IsAoE = false;
            ExtraDetails = 0;
            AoEType = 0;
            Preview = 0;
            PreviewHue = 0;
            
            RestoreTarget();
        }
        
        public static void SetExtra(uint cursorID, ushort type, object details, uint preview, ushort previewHue)
        {
            IsAoE = true;
            AoEType = type;
            ExtraDetails = details;
            Preview = preview;
            PreviewHue = previewHue;
        }


        public static void SetTargeting(CursorTarget targeting, uint cursorID, TargetType cursorType)
        {
            if (targeting == CursorTarget.Invalid)
            {
                return;
            }

            bool lastTargetting = IsTargeting;
            IsTargeting = cursorType < TargetType.Cancel;
            TargetingState = targeting;
            TargetingType = cursorType;

            if (IsTargeting)
            {
                //UIManager.RemoveTargetLineGump(LastTarget);
            }
            else if (lastTargetting)
            {
                CancelTarget();
            }

            // https://github.com/andreakarasho/ClassicUO/issues/1373
            // when receiving a cancellation target from the server we need
            // to send the last active cursorID, so update cursor data later
            
            _targetCursorId = cursorID;
        }
        
        private static ushort zHue = 0x20;
        private static DateTime LastChange;
        private static Rectangle m_OldRect;
        
        public static bool AreaOfEffectHighlight(int x, int y, out ushort hue)
        {
            hue = 0;

            if (ProfileManager.CurrentProfile.ShowAoEArea && TargetManager.IsAoE)
            {
                int locX = -200, locY = -200;

                if (SelectedObject.Object is GameObject gameObject)
                {
                    locX = gameObject.X;
                    locY = gameObject.Y;
                }
                else if (SelectedObject.Object is Land land)
                {
                    locX = land.X;
                    locY = land.Y;
                }
                else if (SelectedObject.Object is Mobile m)
                {
                    locX = m.X;
                    locY = m.Y;
                }
                else if (SelectedObject.Object is Item i)
                {
                    if (i.Container == 0xFFFF_FFFF)
                    {
                        locX = i.X;
                        locY = i.Y;
                    }
                }

                if (locX != -200)
                {
                    
                    bool inRange = false;
                    if (TargetManager.AoEType == 0)
                    {
                        var range = (int)(ushort)TargetManager.ExtraDetails;
                        var rect = new Rectangle(-range, -range, range * 2 + 1, range * 2 + 1);

                        if (rect.Contains(locX - x, locY - y))
                        {
                            inRange = true;
                        }
                    } 
                    else if (TargetManager.AoEType == 1)
                    {
                        var range = (int)(ushort)TargetManager.ExtraDetails;
                        double dist = Math.Sqrt(Math.Pow(x - locX,2) + Math.Pow(y - locY,2));
                        dist = (int) dist;
                        inRange = dist <= range;
                    }
                    else if (TargetManager.AoEType == 2) // rect from point to mouse
                    {
                        var pos = (Vector3)ExtraDetails;
                        int topLeftX = Math.Min(locX, (int)pos.X) - 1;
                        int topLeftY = Math.Min(locY, (int)pos.Y) - 1;
                        int bottomRightX = Math.Max(locX, (int)pos.X) - topLeftX;
                        int bottomRightY = Math.Max(locY, (int)pos.Y) - topLeftY;
                        var rect = new Rectangle(topLeftX, topLeftY, bottomRightX, bottomRightY);

                        if (rect.Contains(x - 1, y - 1))
                            inRange = true;
                    }
                    else if (TargetManager.AoEType == 3)
                    {
                        var pos = (Vector3)ExtraDetails;
                        var posZ = (int) pos.Z;
                        int topLeftX = Math.Min(locX, (int)pos.X);
                        int topLeftY = Math.Min(locY, (int)pos.Y);
                        int bottomRightX = Math.Max(locX, (int)pos.X) - topLeftX + 1;
                        int bottomRightY = Math.Max(locY, (int)pos.Y) - topLeftY + 1;
                        var rect = new Rectangle(topLeftX, topLeftY, bottomRightX, bottomRightY);

                        if (!rect.Equals(m_OldRect))
                        {
                            m_OldRect = rect;
                            MultiTargetInfo.XOff = (ushort)topLeftX;
                            MultiTargetInfo.YOff = (ushort)topLeftY;
                            //if (rect.Contains(x - 1, y - 1))
                            //    inRange = true;

                            if (!World.HouseManager.TryGetHouse(0, out House house))
                            {
                                house = new House(0, 0, false);
                                World.HouseManager.Add(0, house);
                            }
                            else
                            {
                                house.ClearComponents();
                                house.Revision++;
                                house.IsCustom = true;
                            }
    
                            for (int rectX = 0; rectX < rect.Width; rectX++)
                            {
                                for (int rectY = 0; rectY < rect.Height; rectY++)
                                {
                                    /*ushort tempColor = color;

                                    if (x == StartPos.X || y == StartPos.Y)
                                    {
                                        tempColor++;
                                    }*/

                                    Multi mo = house.Add
                                    (
                                        (ushort)Preview,
                                        PreviewHue,
                                        (ushort)rectX,
                                        (ushort)rectY,
                                        (sbyte) posZ,
                                        true,
                                        false
                                    );

                                    mo.MultiOffsetX = rectX;
                                    mo.MultiOffsetY = rectY;
                                    //mo.AlphaHue = 0xFF;

                                    mo.State = CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_GENERIC_INTERNAL | CUSTOM_HOUSE_MULTI_OBJECT_FLAGS.CHMOF_TRANSPARENT;

                                    mo.AddToTile();
                                }
                            }
                        }
                    }
                    else if (TargetManager.AoEType == 0x99) // fields..
                    {
                        var range = (int)(ushort)TargetManager.ExtraDetails;
                        int dx = World.Player.X - locX;
                        int dy = World.Player.Y - locY;
                        int rx = (dx - dy) * 44;
                        int ry = (dx + dy) * 44;

                        bool eastToWest;

                        if (rx >= 0 && ry >= 0)
                        {
                            eastToWest = false;
                        }
                        else if (rx >= 0)
                        {
                            eastToWest = true;
                        }
                        else if (ry >= 0)
                        {
                            eastToWest = true;
                        }
                        else
                        {
                            eastToWest = false;
                        }
                        
                        var rect = new Rectangle(eastToWest ? -range : 0, eastToWest ? 0 : -range, eastToWest ? range * 2 + 1 : 1, eastToWest ? 1: range * 2 + 1);

                        if (rect.Contains(locX - x, locY - y))
                            inRange = true;
                    }

                    if (inRange)
                    {
                       /* if (LastChange <= DateTime.UtcNow)
                        {
                            LastChange = DateTime.UtcNow + TimeSpan.FromMilliseconds(50);

                            if (zHue > 1060)
                                zHue = 0;

                            zHue = 980;
                            World.Player.AddMessage(MessageType.Label, $"{zHue}", TextType.CLIENT);
                        }*/
                       
                       Profile currentProfile = ProfileManager.CurrentProfile;

                       if (currentProfile != null)
                       {

                           ushort highlightHue = currentProfile.NeutralAOEHue;

                           if (TargetManager.TargetingType == TargetType.Beneficial)
                               highlightHue = currentProfile.BeneficialAOEHue;
                           else if (TargetManager.TargetingType == TargetType.Harmful)
                               highlightHue = currentProfile.HarmfulAOEHue;

                           hue = highlightHue;
                       }

                       return true;
                    }
                }
            }
            
            
            if (UIManager.HighlightedAreas.Count > 0)
            {
                foreach (var i in UIManager.HighlightedAreas)
                {
                    if (i.Item1.Contains(x, y))
                    {
                        hue = i.Item2;
                        return true;
                    }
                }
            }

            return false;
        }



        public static void CancelTarget()
        {
            if (TargetingState == CursorTarget.MultiPlacement)
            {
                World.HouseManager.Remove(0);

                if (World.CustomHouseManager != null)
                {
                    World.CustomHouseManager.Erasing = false;
                    World.CustomHouseManager.SeekTile = false;
                    World.CustomHouseManager.SelectedGraphic = 0;
                    World.CustomHouseManager.CombinedStair = false;

                    UIManager.GetGump<HouseCustomizationGump>()?.Update();
                }
            }

            if (IsTargeting || TargetingType == TargetType.Cancel)
            {
                NetClient.Socket.Send_TargetCancel(TargetingState, _targetCursorId, (byte)TargetingType);
                IsTargeting = false;
            }

            Reset();
        }

        public static void SetTargetingMulti
        (
            uint deedSerial,
            ushort model,
            ushort x,
            ushort y,
            short z,
            ushort hue
        )
        {
            SetTargeting(CursorTarget.MultiPlacement, deedSerial, TargetType.Neutral);

            //if (model != 0)
            MultiTargetInfo = new MultiTargetInfo
            (
                model,
                x,
                y,
                z,
                hue
            );
        }

        public static bool SplitLastTargets => World.Settings.ClientOptionFlags.AllowSplitTargetsOptions && 
                                                  ProfileManager.CurrentProfile != null && 
                                                  ProfileManager.CurrentProfile.SplitLastTarget;
        
        public static void Target(uint serial)
        {
            if (!IsTargeting)
            {
                return;
            }

            Entity entity = World.InGame ? World.Get(serial) : null;

            if (entity != null)
            {
                switch (TargetingState)
                {
                    case CursorTarget.Invalid: return;

                    case CursorTarget.MultiPlacement:
                    case CursorTarget.Position:
                    case CursorTarget.Object:
                    case CursorTarget.HueCommandTarget:
                    case CursorTarget.SetTargetClientSide:
                    case CursorTarget.Dummy:

                        if (entity != World.Player)
                        {
                            if (SplitLastTargets && TargetingType == TargetType.Beneficial)
                                LastBeneficialTargetInfo.SetEntity(serial);
                            else
                                LastTargetInfo.SetEntity(serial);
                        }

                        if (SerialHelper.IsMobile(serial) && serial != World.Player && (World.Player.NotorietyFlag == NotorietyFlag.Innocent || World.Player.NotorietyFlag == NotorietyFlag.Ally))
                        {
                            Mobile mobile = entity as Mobile;

                            if (mobile != null)
                            {
                                bool showCriminalQuery = false;

                                if (TargetingType == TargetType.Harmful && ProfileManager.CurrentProfile.EnabledCriminalActionQuery && mobile.NotorietyFlag == NotorietyFlag.Innocent)
                                {
                                    showCriminalQuery = true;
                                }
                                else if (TargetingType == TargetType.Beneficial && ProfileManager.CurrentProfile.EnabledBeneficialCriminalActionQuery && (mobile.NotorietyFlag == NotorietyFlag.Criminal || mobile.NotorietyFlag == NotorietyFlag.Murderer || mobile.NotorietyFlag == NotorietyFlag.Gray))
                                {
                                    showCriminalQuery = true;
                                }

                                if (showCriminalQuery && UIManager.GetGump<QuestionGump>() == null)
                                {
                                    QuestionGump messageBox = new QuestionGump
                                    (
                                        "This may flag\nyou criminal!",
                                        s =>
                                        {
                                            if (s)
                                            {
                                                NetClient.Socket.Send_TargetObject(entity,
                                                                                   entity.Graphic,
                                                                                   entity.X,
                                                                                   entity.Y,
                                                                                   entity.Z,
                                                                                   _targetCursorId,
                                                                                   (byte)TargetingType);

                                                ClearTargetingWithoutTargetCancelPacket();

                                                if (LastTargetInfo.Serial != serial)
                                                {
                                                    GameActions.RequestMobileStatus(serial);
                                                }
                                            }
                                        }
                                    );

                                    UIManager.Add(messageBox);

                                    return;
                                }
                            }
                        }

                        if (TargetingState != CursorTarget.SetTargetClientSide && TargetingState != CursorTarget.Dummy)
                        {
                            _lastDataBuffer[0] = 0x6C;

                            _lastDataBuffer[1] = 0x00;

                            _lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
                            _lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
                            _lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
                            _lastDataBuffer[5] = (byte)_targetCursorId;

                            _lastDataBuffer[6] = (byte) TargetingType;

                            _lastDataBuffer[7] = (byte)(entity.Serial >> 24);
                            _lastDataBuffer[8] = (byte)(entity.Serial >> 16);
                            _lastDataBuffer[9] = (byte)(entity.Serial >> 8);
                            _lastDataBuffer[10] = (byte)entity.Serial;

                            _lastDataBuffer[11] = (byte)(entity.X >> 8);
                            _lastDataBuffer[12] = (byte)entity.X;

                            _lastDataBuffer[13] = (byte)(entity.Y >> 8);
                            _lastDataBuffer[14] = (byte)entity.Y;

                            _lastDataBuffer[15] = (byte)(entity.Z >> 8);
                            _lastDataBuffer[16] = (byte)entity.Z;

                            _lastDataBuffer[17] = (byte)(entity.Graphic >> 8);
                            _lastDataBuffer[18] = (byte)entity.Graphic;


                            NetClient.Socket.Send_TargetObject(entity,
                                                               entity.Graphic,
                                                               entity.X,
                                                               entity.Y,
                                                               entity.Z,
                                                               _targetCursorId,
                                                               (byte)TargetingType);

                            if (SerialHelper.IsMobile(serial) && LastTargetInfo.Serial != serial)
                            {
                                GameActions.RequestMobileStatus(serial);
                            }
                        }

                        ClearTargetingWithoutTargetCancelPacket();

                        Mouse.CancelDoubleClick = true;

                        break;

                    case CursorTarget.Grab:

                        if (SerialHelper.IsItem(serial))
                        {
                            GameActions.GrabItem(serial, ((Item) entity).Amount);
                        }

                        ClearTargetingWithoutTargetCancelPacket();

                        return;

                    case CursorTarget.SetGrabBag:

                        if (SerialHelper.IsItem(serial))
                        {
                            ProfileManager.CurrentProfile.GrabBagSerial = serial;
                            GameActions.Print(string.Format(ResGeneral.GrabBagSet0, serial));
                        }

                        ClearTargetingWithoutTargetCancelPacket();

                        return;
                    case CursorTarget.IgnorePlayerTarget:
                        if (SelectedObject.Object is Entity pmEntity)
                        {
                            IgnoreManager.AddIgnoredTarget(pmEntity);
                        }
                        CancelTarget();
                        return;
                    
                    case CursorTarget.FriendTarget:
                        if (SelectedObject.Object is Entity mEntity)
                        {
                            FriendManager.AddFriendTarget(mEntity);
                        }
                        CancelTarget();
                        return;
                }
            }
            else if (World.Settings.ClientOptionFlags.AllowOffscreenTargeting &&
                     ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.OffscreenTargeting)
            {
                if (World.Settings.ClientOptionFlags.AllowSplitTargetsOptions && 
                    SplitLastTargets && TargetingType == TargetType.Beneficial)
                {
                    LastBeneficialTargetInfo.SetEntity(serial);
                    World.Player.AddMessage(MessageType.Label, "beneficial last target set", TextType.CLIENT);
                }
                else
                {
                    LastTargetInfo.SetEntity(serial);
                    World.Player.AddMessage(MessageType.Label, "last target set", TextType.CLIENT);
                }

            }
        }

        public static void Target(ushort graphic, ushort x, ushort y, short z, bool wet = false)
        {
            if (!IsTargeting)
            {
                return;
            }

            if (graphic == 0)
            {
                if (TargetingState == CursorTarget.Object)
                {
                    return;
                }
            }
            else
            {
                if (graphic >= TileDataLoader.Instance.StaticData.Length)
                {
                    return;
                }

                ref StaticTiles itemData = ref TileDataLoader.Instance.StaticData[graphic];

                if (Client.Version >= ClientVersion.CV_7090 && itemData.IsSurface)
                {
                    z += itemData.Height;
                }
            }

            LastTargetInfo.SetStatic(graphic, x, y, (sbyte) z);

            TargetPacket(graphic, x, y, (sbyte) z);
        }

        public static void SendMultiTarget(ushort x, ushort y, sbyte z)
        {
            TargetPacket(0, x, y, z);
            MultiTargetInfo = null;
        }

        public static void TargetLast()
        {
            if (!IsTargeting)
            {
                return;
            }

            _lastDataBuffer[0] = 0x6C;
            _lastDataBuffer[1] = (byte) TargetingState;
            _lastDataBuffer[2] = (byte) (_targetCursorId >> 24);
            _lastDataBuffer[3] = (byte) (_targetCursorId >> 16);
            _lastDataBuffer[4] = (byte) (_targetCursorId >> 8);
            _lastDataBuffer[5] = (byte) _targetCursorId;
            _lastDataBuffer[6] = (byte) TargetingType;

            NetClient.Socket.Send(_lastDataBuffer);
            Mouse.CancelDoubleClick = true;
            ClearTargetingWithoutTargetCancelPacket();
        }

        private static void TargetPacket(ushort graphic, ushort x, ushort y, sbyte z)
        {
            if (!IsTargeting)
            {
                return;
            }

            _lastDataBuffer[0] = 0x6C;

            _lastDataBuffer[1] = 0x01;

            _lastDataBuffer[2] = (byte)(_targetCursorId >> 24);
            _lastDataBuffer[3] = (byte)(_targetCursorId >> 16);
            _lastDataBuffer[4] = (byte)(_targetCursorId >> 8);
            _lastDataBuffer[5] = (byte)_targetCursorId;

            _lastDataBuffer[6] = (byte)TargetingType;

            _lastDataBuffer[7] = (byte)(0 >> 24);
            _lastDataBuffer[8] = (byte)(0 >> 16);
            _lastDataBuffer[9] = (byte)(0 >> 8);
            _lastDataBuffer[10] = (byte)0;

            _lastDataBuffer[11] = (byte)(x >> 8);
            _lastDataBuffer[12] = (byte)x;

            _lastDataBuffer[13] = (byte)(y >> 8);
            _lastDataBuffer[14] = (byte)y;

            _lastDataBuffer[15] = (byte)(z >> 8);
            _lastDataBuffer[16] = (byte)z;

            _lastDataBuffer[17] = (byte)(graphic >> 8);
            _lastDataBuffer[18] = (byte)graphic;

            

            NetClient.Socket.Send_TargetXYZ(graphic,
                                            x,
                                            y,
                                            z,
                                            _targetCursorId,
                                            (byte)TargetingType);


            Mouse.CancelDoubleClick = true;
            ClearTargetingWithoutTargetCancelPacket();
        }
    }
}