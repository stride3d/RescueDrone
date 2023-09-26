// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Linq;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Input;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using Stride.Rendering.Images;

namespace RescueDrone
{
    public class AdaptDOF : StartupScript
    {
        public Entity TargetEntity;

        public float Aperture = 25f;

        public bool Automatic { get; set; }

        private DepthOfField depthOfField;
        private CameraComponent camera;

        public override void Start()
        {
            base.Start();

            var sceneRenderers = (SceneRendererCollection)((SceneCameraRenderer)SceneSystem.GraphicsCompositor.Game).Child;

            var forwardRenderer = sceneRenderers.Children.OfType<ForwardRenderer>().First();
            var postEffects = forwardRenderer.PostEffects;
            //effectRenderer = ((SceneEffectRenderer) master.Renderers[0]);
            depthOfField = ((PostProcessingEffects)postEffects).DepthOfField;

            sceneRenderers.Children.Insert(0, new DelegateSceneRenderer(UpdateDofParameters));

            camera = Entity.Get<CameraComponent>();
        }

        private void UpdateDofParameters(RenderDrawContext context)
        {
            if (TargetEntity == null)
                return;

            if (Input.IsKeyPressed(Keys.F))
                depthOfField.Enabled = !depthOfField.Enabled;

            if (Automatic)
            {
                depthOfField.AutoFocus = true;
            }
            else
            {
                depthOfField.AutoFocus = false;

                const float sizeSensor = 0.024f / 1.6f;
                var screenPixelWidth = GraphicsDevice.Presenter.BackBuffer.Width;
                var coc = sizeSensor / GraphicsDevice.Presenter.BackBuffer.Height;
                var focalLength = sizeSensor / (float)Math.Tan(MathUtil.DegreesToRadians(camera.VerticalFieldOfView) / 2);

                var viewMatrix = Matrix.Invert(Entity.Transform.WorldMatrix);
                var targetCameraSpace = Vector4.Transform(TargetEntity.Transform.WorldMatrix.Row4, viewMatrix);
                var focusDistance = -targetCameraSpace.Z / targetCameraSpace.W;

                var temp = Aperture * coc * (focusDistance - focalLength) / (focalLength * focalLength);
                var nearFocusDistance = focusDistance / (1 + temp);
                var farFocusDistance = temp >= 1f ? float.PositiveInfinity : focusDistance / (1 - temp);

                var maxCoC = depthOfField.MaxBokehSize * 0.05f * screenPixelWidth * coc; // 0.05f is the internal size of the bokeh (private in DepthOfField)
                var temp2 = Aperture * maxCoC * (focusDistance - focalLength) / (focalLength * focalLength);
                var nearMaxCoCDistance = focusDistance / (1 + temp2);
                float farMaxCoCDistance;
                if (temp2 < 1f)
                {
                    farMaxCoCDistance = focusDistance / (1 - temp2);
                }
                else // use derivative to find a approximate value
                {
                    var derivative = focalLength * focalLength * focusDistance / (Aperture * (focusDistance - focalLength)) / (farFocusDistance * farFocusDistance);
                    farMaxCoCDistance = maxCoC / derivative;
                }

                depthOfField.DOFAreas = new Vector4(nearMaxCoCDistance, nearFocusDistance, farFocusDistance, farMaxCoCDistance);
            }
        }
    }
}