// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stride.Engine;
using Stride.Input;
using Stride.Rendering.Compositing;

namespace RescueDrone
{
    public class CameraScript : SyncScript
    {
        public const Keys SwitchCameraKey = Keys.C;

        public CameraComponent TrackingCamera { get; set; }

        public CameraComponent FreeCamera { get; set; }

        public AdaptDOF AdaptDOF { get; set; }

        public override void Update()
        {
            if (TrackingCamera == null || FreeCamera == null || AdaptDOF == null)
                return;

            if (Input.IsKeyPressed(SwitchCameraKey))
            {
                if (TrackingCamera.Enabled)
                {
                    TrackingCamera.Enabled = false;
                    FreeCamera.Enabled = true;
                    AdaptDOF.Automatic = true;
                }
                else
                {
                    TrackingCamera.Enabled = true;
                    FreeCamera.Enabled = false;
                    AdaptDOF.Automatic = false;
                }
            }
        }
    }
}
