using System.Collections.Generic;

namespace NorthernTown2026
{
    public static class EquipmentCatalog
    {
        static readonly Dictionary<string, EquipmentDefinition> _all = new Dictionary<string, EquipmentDefinition>();

        static EquipmentCatalog()
        {
            Register(new EquipmentDefinition
            {
                Id = "gear_old_phone",
                Name = "改装旧手机（离线天线）",
                Slot = "终端",
                Bonus体魄 = 0,
                Bonus洞察 = 0,
                Bonus镇定 = 0,
                Bonus机巧 = 1
            });
            Register(new EquipmentDefinition
            {
                Id = "gear_worker_coat",
                Name = "厂牌工装外套",
                Slot = "外套",
                Bonus体魄 = 1,
                Bonus洞察 = 0,
                Bonus镇定 = 0,
                Bonus机巧 = 0
            });
            Register(new EquipmentDefinition
            {
                Id = "gear_charm",
                Name = "巷口铜铃铛",
                Slot = "饰品",
                Bonus体魄 = 0,
                Bonus洞察 = 1,
                Bonus镇定 = 1,
                Bonus机巧 = 0
            });
            Register(new EquipmentDefinition
            {
                Id = "item_bread",
                Name = "面包",
                Slot = "—",
                Bonus体魄 = 0,
                Bonus洞察 = 0,
                Bonus镇定 = 0,
                Bonus机巧 = 0,
                Consumable = true,
                GrantXpOnUse = 10
            });
        }

        static void Register(EquipmentDefinition d)
        {
            _all[d.Id] = d;
        }

        public static bool TryGet(string id, out EquipmentDefinition def) => _all.TryGetValue(id, out def);
    }
}
