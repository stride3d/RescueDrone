// <auto-generated>
// Do not edit this file yourself!
//
// This code was generated by Xenko Shader Mixin Code Generator.
// To generate it yourself, please install Stride.VisualStudio.Package .vsix
// and re-save the associated .xkfx.
// </auto-generated>

using System;
using Stride.Core;
using Stride.Rendering;
using Stride.Graphics;
using Stride.Shaders;
using Stride.Core.Mathematics;
using Buffer = Stride.Graphics.Buffer;

namespace Stride.Rendering
{
    public static partial class HeatShimmeringKeys
    {
        public static readonly ValueParameterKey<Vector3> HeatCenter = ParameterKeys.NewValue<Vector3>();
        public static readonly ValueParameterKey<Vector3> HeatUp = ParameterKeys.NewValue<Vector3>();
        public static readonly ObjectParameterKey<Texture> SceneTexture = ParameterKeys.NewObject<Texture>();
    }
}
