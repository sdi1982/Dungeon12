﻿namespace Dungeon12.Classes
{
    using Dungeon.Data;
    using Dungeon12.Entities.Alive;
    using Dungeon12.Entities.Alive.Enums;
    using Dungeon12.Inventory;
    using Dungeon.Types;
    using Dungeon.View.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Dungeon;
    using Dungeon12.Events.Events;
    using Dungeon12.Entities.Quests;
    using Dungeon.Scenes.Manager;
    using Dungeon12.Scenes.Menus;
    using Dungeon12.Abilities;
    using Dungeon12.Conversations;

    /// <summary>
    /// Абстрактный класс персонажа
    /// </summary>
    public abstract partial class Character : Moveable, IPersist
    {
        public Character()
        {
            this.BindClassStats();
            Reload();

            Global.Events.Subscribe<GameLoadedEvent>(ReloadQuestsData);
        }

        public Character(bool @new)
        {
            this.HitPoints = this.MaxHitPoints = this.InitialHP;
            this.BindClassStats();
            Reload();

            Global.Events.Subscribe<GameLoadedEvent>(ReloadQuestsData);
        }

        private void ReloadQuestsData(GameLoadedEvent @event)
        {
            this.Journal.Quests.ForEach(q =>
            {
                if(q.Quest is CollectQuest quest)
                {
                    quest.ReloadQuestLootDrops();
                }
            });

            Global.GameState.RestorableRespawns.ForEach(x =>
            {
                PassRespawnTrigger.PassRespawn(x);
            });
        }

        public void Reload()
        {
            this.Clothes.OnPutOn = PutOnItem;
            this.Clothes.OnPutOff = PutOffItem;

            this.OnDie += () =>
            {
                SceneManager.Switch<End>();
            };
        }

        public virtual void Destroy()
        {
            this.Clothes.OnPutOn = default;
            this.Clothes.OnPutOff = default;
        }

        public override bool ExpGainer => true;

        public Race Race { get; set; }

        public Origins Origin { get; set; }

        public long Gold { get; set; } = 100;

        public virtual string ClassName { get; }

        public virtual string Resource => "";

        public virtual string ResourceName => "Мана";

        public virtual void AddToResource(double value) { }

        public virtual void RemoveToResource(double value) { }

        public virtual IDrawColor ClassColor { get; }

        public virtual void AddClassPerk() { }

        public virtual string Avatar => this.GetType().Name.AsmImgRes();

        /// <summary>
        /// это пиздец, выпили это нахуй
        /// <para>
        /// А вот и нет, полезная фича оказалась
        /// </para>
        /// </summary>
        /// <returns></returns>
        public virtual ConsoleColor ResourceColor => ConsoleColor.Blue;

        public List<ClassStat> ClassStats { get; } = new List<ClassStat>();

        public virtual IDrawText MainAbilityDamageView { get; set; }

        public virtual string MainAbilityDamageText { get; set; }

        public Backpack Backpack { get; set; } = new Backpack(6, 11);

        public Wear Clothes { get; set; } = new Wear();

        public int Id { get; set; }

        public int ObjectId { get; set; }

        /// <summary>
        /// Пересчитывает все характеристики
        /// </summary>
        public void Recalculate()
        {
            FreeProxyProperties();
            this.RecalculateLevelHP();
        }

        public List<Pair<string, object>> Variables = new List<Pair<string, object>>();

        public T GetVariable<T>(string name) => Variables.FirstOrDefault(n => n.First == name).Second.As<T>();

        public T SetVariable<T>(string name, T value)
        {
            var v = Variables.FirstOrDefault(n => n.First == name);
            if (v.First != default)
            {
                v.Second = value;
            }
            else
            {
                Variables.Add(new Pair<string, object>()
                {
                    First = name,
                    Second = value
                });
            }
            return value;
        }

        public object this[string variable]
        {
            get => GetVariable<object>(variable);
            set => SetVariable(variable, variable);
        }
    }
}