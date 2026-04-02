using System;

namespace NorthernTown2026
{
    [Serializable]
    public class EquipmentDefinition
    {
        public string Id;
        public string Name;
        public string Slot;
        public int Bonus体魄;
        public int Bonus洞察;
        public int Bonus镇定;
        public int Bonus机巧;

        /// <summary>为 true 时不可装备，仅可通过「使用」区消耗。</summary>
        public bool Consumable;

        /// <summary>消耗时获得的经验（仅 Consumable 有效）。</summary>
        public int GrantXpOnUse;
    }
}
