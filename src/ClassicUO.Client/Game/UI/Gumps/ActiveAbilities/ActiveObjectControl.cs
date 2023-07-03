using System.Collections.Generic;
using ClassicUO.Game.UI.Controls;

namespace ClassicUO.Game.UI.Gumps
{
    internal class ActiveAbilityObjectControl : Control
    {
        private HtmlControl _Name;
        public List<ActiveAbilityControl> Abilities;

        public ActiveAbilityObjectControl(ActiveAbilityObject ability, int row, int size)
        {
            bool title = false;
            if (!string.IsNullOrEmpty(ability.Name))
            {
                title = true;
                Add
                (
                    _Name = new HtmlControl
                    (
                        5, 0, 200, 25,
                        false, false, false, $"<FONT COLOR=#FFFFFF>{ability.Name}</BASEFONT>",
                        ishtml: true
                    )
                );
            }

            int xOffset = 5;
            Abilities = new List<ActiveAbilityControl>();
            for (int i = 0; i < ability.Abilities.Count; i++)
            {
                var item = new ActiveAbilityControl(ability.Abilities[i], row, i, size)
                {
                    X = xOffset,
                    Y = title ? 25 : 0,
                };

                Add(item);
                Abilities.Add(item);

                xOffset += 2 + item.Width;
            }


            AcceptMouseInput = true;
            WantUpdateSize = true;
            CanMove = true;
        }

    }

}