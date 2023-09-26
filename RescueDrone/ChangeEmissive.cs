// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using Stride.Core;
using Stride.Engine;
using Stride.Rendering;
using Stride.Rendering.Materials;

namespace RescueDrone
{
    public class ChangeEmissive : SyncScript
    {
        [DataMember(0)]
        public Material Material;

        [DataMember(10)]
        public float EmissiveIntensity = 5;

        [DataMember(20)]
        public float TimeThreshold = 10;

        private float initialIntensity;
        private AnimationComponent animation;

        public override void Start()
        {
            base.Start();

            animation = Entity.Get<AnimationComponent>();

            initialIntensity = Material.Passes[0].Parameters.Get(MaterialKeys.EmissiveIntensity);
        }

        public override void Update()
        {
            var time = animation.PlayingAnimations[0].CurrentTime.TotalSeconds;
            if (Material != null && time >= TimeThreshold)
            {
                Material.Passes[0].Parameters.Set(MaterialKeys.EmissiveIntensity, EmissiveIntensity);
            }

            if (Material != null && Input.IsKeyPressed(GameTimeScript.ResetKey))
            {
                Material.Passes[0].Parameters.Set(MaterialKeys.EmissiveIntensity, initialIntensity);
            }
        }
    }
}