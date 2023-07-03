using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using ClassicUO.Configuration;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Resources;
using ClassicUO.Utility.Logging;

namespace ClassicUO.Game.Managers
{
    internal static class FriendManager
    {
        /// <summary>
        /// Set of Char names
        /// </summary>
        public static Dictionary<uint, string> FriendList = new Dictionary<uint, string>();

        /// <summary>
        /// Initialize Friend Manager
        /// - Load List from XML file
        /// </summary>
        public static void Initialize()
        {
            ReadFriendList();
        }

        /// <summary>
        /// Add Char to friend list
        /// </summary>
        /// <param name="entity">Targeted Entity</param>
        public static void AddFriendTarget(Entity entity)
        {
            //If is mobile, Human and Invul
            if (entity is Mobile m && m.Serial != World.Player.Serial)
            {
                var charName = m.Name;

                if (FriendList.ContainsKey(m.Serial))
                {
                    GameActions.Print(string.Format(ResGumps.AddToFriendListExist, charName));
                    return;
                }


                FriendList.Add(m.Serial, charName);
                // Redraw list of chars
                UIManager.GetGump<FriendManagerGump>()?.Redraw();

                GameActions.Print(string.Format(ResGumps.AddToFriendListSuccess, charName));
                return;
            }

            GameActions.Print(string.Format(ResGumps.AddToFriendListNotMobile));
        }

        /// <summary>
        /// Remove Char from Friend List
        /// </summary>
        /// <param name="charName">Char name</param>
        public static void RemoveFriendTarget(uint id)
        {
            if (FriendList.ContainsKey(id))
                FriendList.Remove(id);
        }

        /// <summary>
        /// Load Friend List from XML file
        /// </summary>
        private static void ReadFriendList()
        {
            Dictionary<uint, string> list = new Dictionary<uint, string>();

            string friendXmlPath = Path.Combine(ProfileManager.ProfilePath, "friend_list.xml");

            if (!File.Exists(friendXmlPath))
            {
                return;
            }

            XmlDocument doc = new XmlDocument();

            try
            {
                doc.Load(friendXmlPath);
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            XmlElement root = doc["friend"];

            if (root != null)
            {
                foreach (XmlElement xml in root.ChildNodes)
                {
                    if (xml.Name != "info")
                    {
                        continue;
                    }

                    string charName = xml.GetAttribute("charname");
                    uint charID = 0;
                    if (uint.TryParse(xml.GetAttribute("serial"), out charID))
                        list.Add(charID, charName);
                }
            }

            FriendList = list;
        }

        /// <summary>
        /// Save List to XML File
        /// </summary>
        public static void SaveFriendList()
        {
            string friendXmlPath = Path.Combine(ProfileManager.ProfilePath, "friend_list.xml");

            using (XmlTextWriter xml = new XmlTextWriter(friendXmlPath, Encoding.UTF8)
            {
                Formatting = Formatting.Indented,
                IndentChar = '\t',
                Indentation = 1
            })
            {
                xml.WriteStartDocument(true);
                xml.WriteStartElement("friend");

                foreach (var ch in FriendList)
                {
                    xml.WriteStartElement("info");
                    xml.WriteAttributeString("charname", ch.Value);
                    xml.WriteAttributeString("serial", $"{ch.Key}");
                    xml.WriteEndElement();
                }

                xml.WriteEndElement();
                xml.WriteEndDocument();
            }
        }
    }
}
