// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Stride.Core;
using Stride.Core.Diagnostics;
using Stride.Core.Mathematics;
using Stride.Engine;
using Stride.Graphics;
using Stride.Rendering;
using Stride.Rendering.Compositing;
using Stride.Rendering.Images;
using Stride.Rendering.Materials;

namespace RescueDrone
{
    public class ChangeTexture : SyncScript
    {
        public ChangeTexture()
        {
            TextureList = new List<TextureInfo>();
        }

        [DataContract]
        public class TextureInfo
        {
            public float ChangeTime;

            public int ChangeType;
        }

        [DataMember(0)]
        public Material Material;

        [DataMember(10)] 
        public Texture BackgroundTexture;

        [DataMember(20)]
        public List<TextureInfo> TextureList;

        private Texture initialMap;
        private AnimationComponent animation;

        private Texture renderTexture;

        private Texture guruMeditationTexture;
        private Texture guruMeditationTextureBorders;
        private Texture powerTexture;
        private SpriteBatch spriteBatch;

        struct PowerColumn
        {
            public int StartX;
        }

        private Color greenColor = Color.FromBgra(0xFFD2F45D);
        private Color blueColor = Color.FromBgra(0xFF407DB1);
        private Color redColor = Color.FromBgra(0xFFF68914);

        private const int PowerColumnCount = 7;
        private const int PowerColumnTotalCount = PowerColumnCount * 3;

        private readonly int[] columns = new int[PowerColumnCount]{83, 144, 204, 262, 321, 381, 441};
        private readonly float[] powerState = new float[PowerColumnTotalCount];
        private readonly float[] powerStateTargets = new float[PowerColumnTotalCount];
        private Random random = new Random(0);
        private int lastPowerTime = 0;
        private float powerTarget;
        private Stopwatch clock;
        private SpriteFont powerFont;


        public override void Start()
        {
            base.Start();

            animation = Entity.Get<AnimationComponent>();

            var sceneRenderers = (SceneRendererCollection)((SceneCameraRenderer)SceneSystem.GraphicsCompositor.Game).Child;
            sceneRenderers.Children.Insert(0, new DelegateSceneRenderer(RobotScreenRender));

            initialMap = Material.Passes[0].Parameters.Get(MaterialKeys.EmissiveMap);

            guruMeditationTexture = Content.Load<Texture>("robot/guru_meditation");
            guruMeditationTextureBorders = Content.Load<Texture>("robot/guru_meditation_borders");
            powerTexture = Content.Load<Texture>("robot/monitor_power");
            powerFont = Content.Load<SpriteFont>("robot/power_font");

            spriteBatch = new SpriteBatch(GraphicsDevice);
            clock = Stopwatch.StartNew();
        }

        private void RobotScreenRender(RenderDrawContext drawContext)
        {
            if (Material == null || BackgroundTexture == null)
                return;

            // Create the texture if it doesn't exist
            if ((renderTexture == null || renderTexture.Width != BackgroundTexture.Width || renderTexture.Height != BackgroundTexture.Height))
            {
                Utilities.Dispose(ref renderTexture);
                renderTexture = Texture.New2D(GraphicsDevice, BackgroundTexture.Width, BackgroundTexture.Height, 1, PixelFormat.R8G8B8A8_UNorm_SRgb, TextureFlags.ShaderResource | TextureFlags.RenderTarget);
            }


            var time = animation.PlayingAnimations[0].CurrentTime.TotalSeconds;
            var powerTime = (int) (time*4);

            const float SpeedPowerUp = 0.5f; // Power recovery per second
            const float HigherPowerLowerRandom = 0.5f; // 1.0: no high random when power = 1.0f,
            float powerRandomLargeScale = 0.4f; // Large random scale
            float powerRandomSmallScale = 0.1f; // Small random scale

            bool hasPower = false;
            bool hasPowerFailure = true;
            for (int txIndex = 0; txIndex < TextureList.Count; txIndex++)
            {
                var textureInfo = TextureList[txIndex];
                if (time >= textureInfo.ChangeTime && ((txIndex+1) == TextureList.Count || time < TextureList[txIndex + 1].ChangeTime))
                {
                    hasPower = true;
                    if (textureInfo.ChangeType >= 2)
                    {
                        hasPowerFailure = false;
                        powerTarget += SpeedPowerUp*(float) clock.Elapsed.TotalSeconds;
                        powerTarget = MathUtil.Clamp(powerTarget, 0.0f, 1.0f);
                        powerRandomLargeScale = 0.4f; // Large random scale
                        powerRandomSmallScale = 0.1f; // Small random scale
                    }
                    else
                    {
                        powerTarget = 0.1f;
                        powerRandomLargeScale = 0.1f; // Large random scale
                        powerRandomSmallScale = 0.05f; // Small random scale
                    }

                    if (lastPowerTime != powerTime)
                    {
                        for (int i = 0; i < columns.Length; i++)
                        {
                            var nextPower =
                                Math.Abs(powerTarget + (1.0f - powerTarget * HigherPowerLowerRandom) * (random.NextDouble() * powerRandomLargeScale) - powerRandomLargeScale * 0.5f);
                            for (int j = 0; j < 3; j++)
                            {
                                var columnPower =
                                    MathUtil.Clamp(
                                        (float)Math.Abs(nextPower + (random.NextDouble() * powerRandomSmallScale - powerRandomSmallScale * 0.5f)), 0.0f, 1.0f);
                                powerStateTargets[i * 3 + j] = columnPower;
                            }
                        }
                        lastPowerTime = powerTime;
                    }

                    for (int i = 0; i < columns.Length; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            powerState[i * 3 + j] = MathUtil.Lerp(powerState[i * 3 + j], powerStateTargets[i * 3 + j],
                                0.15f);
                        }
                    }
                }
            }

            if (hasPower)
            {
                Material.Passes[0].Parameters.Set(MaterialKeys.EmissiveMap, renderTexture);
                using (drawContext.PushRenderTargetsAndRestore())
                {
                    drawContext.CommandList.ResourceBarrierTransition(renderTexture, GraphicsResourceState.RenderTarget);
                    drawContext.CommandList.SetRenderTarget(null, renderTexture);

                    spriteBatch.Begin(drawContext.GraphicsContext);

                    spriteBatch.Draw(BackgroundTexture, Vector2.Zero);
                    if (hasPowerFailure)
                    {
                        spriteBatch.Draw(guruMeditationTexture, new Vector2(0, 0));
                        if ((time % 2.0) < 1.0f)
                        {
                            spriteBatch.Draw(guruMeditationTextureBorders, new Vector2(0, 0));
                        }
                    }

                    var maxPower = 27 * (hasPowerFailure ? 0.15f : 1.0f);
                    for (int i = 0; i < columns.Length; i++)
                    {
                        for (int j = 0; j < 3; j++)
                        {
                            var power = powerState[i * 3 + j];
                            var maxHeight = 27 * power;
                            for (int h = 0; h < maxHeight; h++)
                            {
                                var color = h < maxPower * 0.7f ? greenColor : h < maxPower * 0.8f ? blueColor : redColor;
                                spriteBatch.Draw(powerTexture, new Vector2(columns[i] + j * 16, 828 - 24 * h), color);
                            }
                        }
                    }

                    spriteBatch.DrawString(powerFont, string.Format("{0:0.0}", (hasPowerFailure ? powerStateTargets[0] : powerTarget) * 486.0f), new Vector2(163, 916), Color.White, TextAlignment.Right);

                    spriteBatch.End();
                }
                drawContext.CommandList.ResourceBarrierTransition(renderTexture, GraphicsResourceState.PixelShaderResource);
            }
            else
            {
                Material.Passes[0].Parameters.Set(MaterialKeys.EmissiveMap, initialMap);

                powerTarget = 0;
                Array.Clear(powerState, 0, powerState.Length);
                Array.Clear(powerStateTargets, 0, powerStateTargets.Length);
            }

            clock.Restart();
        }

        public override void Cancel()
        {
            
        }

        public override void Update()
        {

            if (Input.IsKeyPressed(GameTimeScript.ResetKey))
            {
                Material.Passes[0].Parameters.Set(MaterialKeys.EmissiveMap, initialMap);
            }
        }
    }
}