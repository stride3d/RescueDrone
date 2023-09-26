// Copyright (c) .NET Foundation and Contributors (https://dotnetfoundation.org/ & https://stride3d.net) and Silicon Studio Corp. (https://www.siliconstudio.co.jp)
// Distributed under the MIT license. See the LICENSE.md file in the project root for more information.
using System.Collections.Generic;
using Stride.Core;
using Stride.Audio;
using Stride.Engine;

namespace RescueDrone
{
    public class PlaySounds : SyncScript
    {
        [DataContract]
        public class SoundEffectTimings
        {
            public float PlayTime;

            public Sound Sound;

            [DataMemberIgnore]
            public SoundInstance Instance;
        }

        public bool Enabled { get; set; }

        public Sound BackgroundMusic;

        private SoundInstance MusicInstance;

        [Display("Sound effects timings")]
        public List<SoundEffectTimings>  SoundEffects = new List<SoundEffectTimings>();

        private AnimationComponent animation;

        public override void Start()
        {
            base.Start();

            MusicInstance = BackgroundMusic.CreateInstance();

            MusicInstance.IsLooping = true;

            if (Enabled)
                MusicInstance.Play();

            animation = Entity.Get<AnimationComponent>();
        }

        public override void Update()
        {
            if (!Enabled)
                return;

            if (animation == null)
                return;

            var time = animation.PlayingAnimations[0].CurrentTime.TotalSeconds;
            foreach (var timing in SoundEffects)
            {
                var sound = timing.Sound;
                if(sound == null)
                    continue;

                if (timing.Instance == null)
                {
                    timing.Instance = sound.CreateInstance();
                }
                if(timing.PlayTime < time  && time < timing.PlayTime + 0.08f)
                    timing.Instance.Play();
            }

            if (MusicInstance != null && Input.IsKeyPressed(GameTimeScript.ResetKey))
            { 
                MusicInstance.Stop();
                MusicInstance.Play();
            }
        }
    }
}
