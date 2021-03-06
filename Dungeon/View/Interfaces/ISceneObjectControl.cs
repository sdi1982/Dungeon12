﻿namespace Dungeon.View.Interfaces
{
    using Dungeon.Control;
    using Dungeon.Control.Keys;
    using Dungeon.Control.Pointer;
    using System;

    public interface ISceneObjectControl : ISceneObject
    {
        void KeyDown(Key key, KeyModifiers modifier, bool hold);

        void KeyUp(Key key, KeyModifiers modifier);

        void Focus();

        void Unfocus();

        void Click(PointerArgs args);

        void ClickRelease(PointerArgs args);

        void MouseMove(PointerArgs args);

        void GlobalMouseMove(PointerArgs args);

        void MouseWheel(MouseWheelEnum mouseWheel);

        void TextInput(string text);

        void GlobalClick(PointerArgs args);

        void GlobalClickRelease(PointerArgs args);

        ControlEventType[] CanHandle { get; }

        Key[] KeysHandle { get; }

        bool AllKeysHandle { get; }
    }
}
