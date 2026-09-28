using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace GAME_OS_Surv
{
    public enum UpgradeType
    {
        MaxHP, XPBonus, AttackSpeed, Damage, Piercing, Speed
    }

    public class Upgrade
    {
        public string Name { get; }
        public string Description { get; }
        public Color Color { get; }
        public UpgradeType Type { get; }
        public Image Icon { get; set; }
        public Upgrade(string name, string desc, Color color, UpgradeType type)
        {
            Name = name;
            Description = desc;
            Color = color;
            Type = type;
        }
    }
}