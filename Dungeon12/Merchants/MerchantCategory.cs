﻿namespace Dungeon12.Merchants
{
    using Dungeon12.Inventory;
    using Dungeon12.Items;
    using System.Collections.Generic;

    public class MerchantCategory
    {
        public string Name { get; set; }

        public List<Backpack> Goods { get; set; } = new List<Backpack>();

        public static List<MerchantCategory> CommonCategories => new List<MerchantCategory>()
        {
            new MerchantCategory(){ Name="weapon"},
            new MerchantCategory(){ Name="armor"},
            new MerchantCategory(){ Name="potion"},
            new MerchantCategory(){ Name=""},//magic 
            new MerchantCategory(){ Name=""},//reputation
        };
    }
}