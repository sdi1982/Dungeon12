﻿namespace Dungeon.Drawing.SceneObjects.Base
{
    using System;
    using Dungeon.Drawing.Impl;
    using Dungeon.Settings;
    using Dungeon.View.Interfaces;

    public class ColoredRectangle : HandleSceneControl
    {
        public ConsoleColor Color { get; set; }

        public bool Fill { get; set; }

        private double opacity;
        public double Opacity
        {
            get => opacity;
            set
            {
                opacity = value;
                drawablePath = null;
            }
        }

        public int Depth { get; set; } = 1;

        public double Round { get; set; } = 0;

        protected void UpdatePath()
        {
            this.drawablePath = null;
        }

        private DrawablePath drawablePath;
        public override IDrawablePath Path
        {
            get
            {
                if (drawablePath == null)
                {
                    var color = new DrawColor(this.Color)
                    {
                        Opacity = Opacity,
                        A = 255
                    };

                    drawablePath = new DrawablePath
                    {
                        Fill = this.Fill,
                        BackgroundColor = color,
                        Depth = this.Depth,
                        PathPredefined = View.Enums.PathPredefined.Rectangle,
                        Region = this.Position,
                        Radius = (float)this.Round
                    };
                }

                return drawablePath;
            }
        }

        protected override void CallOnEvent(dynamic obj)
        {
            OnEvent(obj);
        }

        public ColoredRectangle DarkPanel()
        {
            Color = ConsoleColor.Black;
            Depth = 1;
            Fill = true;
            Opacity = 0.5;
            Round = 5;

            return this;
        }

    }

    public class DarkRectangle : ColoredRectangle
    {
        public DarkRectangle()
        {
            Color = ConsoleColor.Black;
            Depth = 1;
            Fill = true;
            Opacity = 0.5;
            Round = 5;
        }
    }
}