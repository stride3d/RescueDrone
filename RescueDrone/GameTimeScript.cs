// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using Stride.Core;
using Stride.Core.Extensions;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Input;
using Stride.Rendering;
using Stride.Rendering.Compositing;

namespace RescueDrone
{
    public class GameTimeScript : SyncScript
    {
        private bool isPaused;
        private SpriteFont font;
        private SpriteBatch spriteBatch;
        private List<AnimationComponent> animations = new List<AnimationComponent>();

        public const Keys ResetKey = Keys.R;

        public bool DisplayTime = true;

        internal SpriteFont DisplayFont;

        public override void Start()
        {
            base.Start();

            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            var scene = SceneSystem.SceneInstance.RootScene;
            //var compositor = ((SceneGraphicsCompositorLayers)scene.Settings.GraphicsCompositor);
            //compositor.Master.Renderers.Add(new SceneDelegateRenderer(RenderTime));

            DisplayFont = Content.Load<SpriteFont>("DebugFont");

            foreach (var entity in scene.Entities)
            {
                var animation = entity.Get<AnimationComponent>();
                if(animation!= null)
                    animations.Add(animation);
            }
        }

        private void RenderTime(RenderDrawContext drawContext)
        {
            if (!DisplayTime || DisplayFont == null)
                return;

            spriteBatch.Begin(drawContext.GraphicsContext);
            spriteBatch.DrawString(DisplayFont, "Current Time: {0}s".ToFormat(animations[0].PlayingAnimations[0].CurrentTime.TotalSeconds), new Vector2(10, 10), Color.White);
            spriteBatch.End();
        }

        public override void Update()
        {
            if (Input.IsKeyPressed(Keys.Space))
            {
                isPaused = !isPaused;
                foreach (var animation in animations)
                    animation.PlayingAnimations.ForEach(x => x.Enabled = !isPaused);
            }

            bool shoudResetTime = false;
            var resetTime = new TimeSpan();

            if (Input.IsKeyPressed(ResetKey))
            {
                shoudResetTime = true;
            }

            // Click in bottom area of the window acts like a timeline bar
            foreach (var pointerEvent in Input.PointerEvents)
            {
                if (pointerEvent.IsDown && pointerEvent.Position.Y > 0.8)
                {
                    resetTime = TimeSpan.FromSeconds(90*pointerEvent.Position.X);
                    shoudResetTime = true;
                    break;
                }
            }

            if (shoudResetTime)
            {
                foreach (var animation in animations)
                    animation.PlayingAnimations.ForEach(x => x.CurrentTime = resetTime);
            }
        }
    }
}