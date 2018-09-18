﻿namespace Rogue.Abilities.Scaling
{
    using System;
    using System.Collections.Generic;
    using Rogue.Entites.Enums;

    public class ScaleRate
    {
        private ScaleRate(Scale scale)
        {
            this.Scale = scale;
        }

        public static ScaleRate Build(Scale scale)
        {
            return new ScaleRate(scale);
        }

        public static ScaleRate Build(Scale scale, double value)
        {
            var rate = Build(scale);

            return new Dictionary<Scale, Func<double, ScaleRate>>()
            {
                {Scale.AbilityPower,rate.Ability },
                {Scale.AttackDamage,rate.Attack },
                {Scale.True,rate.True },
                {Scale.Both,rate.Both}
            }[scale](value);
        }

        public Scale Scale { get; }

        public double AttackRate { get; set; }

        public double AbilityRate { get; set; }

        public double TrueRate { get; set; }

        public ScaleRate Attack(double value)
        {
            this.AttackRate = value;
            return this;
        }

        public ScaleRate Ability(double value)
        {
            this.AbilityRate = value;
            return this;
        }

        public ScaleRate True(double value)
        {
            this.TrueRate = value;
            return this;
        }

        public ScaleRate Both(double value)
        {
            this.Attack(value);
            this.Ability(value);
            return this;
        }
    }
}