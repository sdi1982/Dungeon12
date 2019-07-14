﻿namespace Rogue.Drawing.SceneObjects.Dialogs.Shop
{
    using Rogue.Drawing.Impl;
    using Rogue.Drawing.SceneObjects.Map;

    public class ShopWindow : HandleSceneControl
    {
        public override bool AbsolutePosition => true;

        public override bool CacheAvailable => false;

        public ShopWindow(string title, Merchants.Merchant merchant, PlayerSceneObject playerSceneObject)
        {
            this.Image = "Rogue.Resources.Images.ui.vertical_title(17x15).png";

            this.Width = 15;
            this.Height = 17;

            var txt = this.AddTextCenter(new DrawText(title), true, false);
            txt.Top += 0.2;

            foreach (var category in merchant.Categories)
            {
                var index = merchant.Categories.IndexOf(category);
                var tab = new ShopTab(this, category,playerSceneObject, index == 0)
                {
                    AbsolutePosition = true,
                    CacheAvailable = false,
                    Left = index * 3,
                    Top = 1.5,
                    ZIndex=this.ZIndex
                };
                this.AddChild(tab);

                if (index == 0)
                {
                    tab.Open();
                }
            }
        }
    }
}