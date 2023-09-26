// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using Stride.Engine;

namespace RescueDrone
{
    public class PlayAnimation : StartupScript
    {
        public string AnimationName { get; set; }

        public override void Start()
        {
            base.Start();

            var animation = Entity.Get<AnimationComponent>();

            if(AnimationName != null && animation != null)
                animation.Play(AnimationName);
        }
    }
}