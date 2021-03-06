﻿namespace Dungeon.View.Interfaces
{
    using Dungeon.Types;

    public interface ICamera
    {
        void MoveCamera(Direction direction, bool stop = false);

        void StopMoveCamera();

        void SetCamera(double x, double y);

        void ResetCamera();

        double CameraOffsetX { get; }

        double CameraOffsetY { get; }

        double CameraOffsetLimitX { get; }

        double CameraOffsetLimitY { get; }

        void SetCameraSpeed(double speed);

        bool InCamera(ISceneObject sceneObject);
    }
}
