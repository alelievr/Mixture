using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Rendering;

using UnityEngine.Experimental.Rendering;

namespace Mixture
{
    public static class MixtureUtils
    {
		static Material  _blitIconMaterial;
		public static Material  blitIconMaterial
		{
			get
			{
				if (_blitIconMaterial == null)
				{
					_blitIconMaterial = new Material(Shader.Find("Hidden/MixtureIconBlit"));
					_blitIconMaterial.SetTexture("_MixtureIcon", icon);
				}

				return _blitIconMaterial;
			}
		}

		static Material	_textureArrayPreviewMaterial;
		public static Material	textureArrayPreviewMaterial
		{
			get
			{
				if (_textureArrayPreviewMaterial == null)
				{
					_textureArrayPreviewMaterial = new Material(Shader.Find("Hidden/MixtureTextureArrayPreview"));
				}

				return _textureArrayPreviewMaterial;
			}
		}

		static Texture2D				_icon;
		public static Texture2D			icon
		{
			get => _icon == null ? _icon = Resources.Load< Texture2D >("MixtureIcon") : _icon;
		}
    }
}