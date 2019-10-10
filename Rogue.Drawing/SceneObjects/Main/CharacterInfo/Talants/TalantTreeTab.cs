﻿using Rogue.Abilities.Talants.TalantTrees;
using Rogue.Classes;
using Rogue.Drawing.SceneObjects.UI;
using Rogue.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Rogue.Drawing.SceneObjects.Main.CharacterInfo.Talants
{
    public class TalantTreeTab : TabControlFlex<TalantTreeTabContent, TalantTree, TalantTreeTab>
    {
        private readonly Character character;

        public TalantTreeTab(SceneObject parent, TalantTree talantTree, Character character, bool active = false)
            : base(parent, active, talantTree, talantTree.Name)
        {
            this.character = character;
        }
        
        protected override TalantTreeTab Self => this;

        protected override Func<TalantTree, double, TalantTreeTabContent> CreateContent => OpenTalantTreeTab;

        protected override void CallOnEvent(dynamic obj)
        {
            OnEvent(obj);
        }

        public void OnEvent(ClassChangeEvent @event)
        {

        }

        private TalantTreeTabContent OpenTalantTreeTab(TalantTree talantTree, double left)
        {
            return new TalantTreeTabContent(talantTree,this.character, left);
        }
    }
}