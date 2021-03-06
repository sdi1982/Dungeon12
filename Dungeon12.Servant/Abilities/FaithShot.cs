﻿using Dungeon;
using Dungeon.View.Interfaces;
using Dungeon12.Abilities;
using Dungeon12.Abilities.Enums;
using Dungeon12.Abilities.Scaling;
using Dungeon12.Entities.Alive;
using Dungeon12.Map;
using Dungeon12.Map.Objects;
using Dungeon12.Servant.Effects.FaithShot;

namespace Dungeon12.Servant.Abilities
{
    public class FaithShot : BaseCooldownAbility<Servant>
    {
        public override Cooldown Cooldown { get; } = BaseCooldown.Chain(2500, nameof(FaithShot)).Build();

        public override AbilityPosition AbilityPosition => AbilityPosition.Left;

        public override AbilityActionAttribute ActionType => AbilityActionAttribute.DmgHealInstant;

        public override AbilityTargetType TargetType => AbilityTargetType.TargetAndNonTarget;

        public override string Description => "Наносит урон святой магией ближайшему врагу. Навык имеет длительное время восстановления. При ударе добавляет одну Печать.";

        public override string Name => "Удар веры";

        public override ScaleRate<Servant> Scale => new ScaleRate<Servant>(x => x.AbilityPower * 1.9);

        protected override bool CanUse(Servant @class) => !@class.Serve;

        protected override void Dispose(GameMap gameMap, Avatar avatar, Servant @class)
        {
        }

        protected override double RangeMultipler => 4;

        public override string Spend => "Восстанавливает: 1 Печать";

        public override long Value => 3 + Global.GameState.Character.As<Servant>().DamagePower;

        protected override void Use(GameMap gameMap, Avatar avatar, Servant @class)
        {
            var rangeObject = avatar.Grow(4);
            Global.AudioPlayer.Effect("faithshot.wav".AsmSoundRes());

            var enemy = gameMap.One<NPCMap>(rangeObject, x => x.IsEnemy);
            if (enemy != default)
            {
                @class.FaithPower.Value++;
                this.UseEffects(new Smash(avatar).InList<ISceneObject>());
                enemy.Entity.Damage(@class, new Dungeon12.Entities.Alive.Damage()
                {
                    Amount = ScaledValue(@class) + @class.DamagePower,
                    Type = DamageType.HolyMagic
                });
            }
        }
    }
}