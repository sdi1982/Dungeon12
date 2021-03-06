﻿namespace Dungeon12.Items.Types
{
    using Dungeon12.Items.Enums;

    public class ResourceItem : Item
    {
        public override ItemKind Kind => ItemKind.Resource;

        public override Rarity Rare => Rarity.Resource;
    }
}