using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenCvSharp;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Mixture
{
    public static class MixtureOpenCVUtils
    {
        private static Material _flipMaterial;
        private static Material FlipMaterial
        {
            get
            {
                if(_flipMaterial == null)
                {
                    _flipMaterial = new Material(Shader.Find("Hidden/Mixture/Flip"));
                }

                return _flipMaterial;
            }
        }
        
        public static Dictionary<MatType, TextureFormat> MatTypeToTextureFormat = new Dictionary<MatType, TextureFormat>
        {
            { MatType.CV_8UC1, TextureFormat.Alpha8 },
            { MatType.CV_8UC2, TextureFormat.RG16 },
            { MatType.CV_8UC3, TextureFormat.RGB24 },
            { MatType.CV_8UC4, TextureFormat.RGBA32 },
            { MatType.CV_32FC1, TextureFormat.RFloat },
            { MatType.CV_32FC2, TextureFormat.RGFloat },
            { MatType.CV_32FC3, TextureFormat.RGBAFloat },
            { MatType.CV_32FC4, TextureFormat.RGBAFloat },
            { MatType.CV_16UC1, TextureFormat.RHalf },
            { MatType.CV_16UC2, TextureFormat.RGHalf },
            { MatType.CV_16UC4, TextureFormat.RGBAHalf },
            { MatType.CV_16UC3, TextureFormat.RGBAHalf },
        };

        public static Dictionary<TextureFormat, MatType> TextureFormatToMatType =
            new Dictionary<TextureFormat, MatType>
            {
                { TextureFormat.Alpha8, MatType.CV_8UC1 },
                { TextureFormat.RG16, MatType.CV_8UC2 },
                { TextureFormat.RGB24, MatType.CV_8UC3 },
                { TextureFormat.RGBA32, MatType.CV_8UC4 },
                { TextureFormat.RFloat, MatType.CV_32FC1 },
                { TextureFormat.RGFloat, MatType.CV_32FC2 },
                //{ TextureFormat.RGB, MatType.CV_32FC3 },
                { TextureFormat.RGBAFloat, MatType.CV_32FC4 },
                { TextureFormat.RHalf, MatType.CV_16SC1 },
                { TextureFormat.RGHalf, MatType.CV_16SC2 },
                { TextureFormat.RGBAHalf, MatType.CV_16SC4 },
                //{ TextureFormat.RGBAHalf, MatType.CV_16UC3 },
            };

        public static TextureFormat GetTextureFormatFromMat(MatType matType)
        {
            if (MatTypeToTextureFormat.TryGetValue(matType, out var format))
                return format;
            else
            {
                throw new System.Exception($"Unsupported MatType {matType}");
            }
        }
        
        public static MatType GetMatTypeFromTextureFormat(TextureFormat format)
        {
            if (TextureFormatToMatType.TryGetValue(format, out var matType))
                return matType;
            else
            {
                throw new System.Exception($"Unsupported TextureFormat {format}");
            }
        }
        
        public static MatType GetMatTypeFromTexture(Texture2D texture)
        {
            return GetMatTypeFromTextureFormat(texture.format);
        }
        
        public static TextureFormat GetTextureFormatFromMat(Mat mat)
        {
            return GetTextureFormatFromMat(mat.Type());
        }
        
        public static unsafe Mat ConvertToMat(this Texture2D texture, ColorConversionCodes? conversionCode = null, bool flip = false)
        {
            var matType = GetMatTypeFromTexture(texture);
            var mat = new Mat(texture.height, texture.width, matType);
            var textureData = texture.GetRawTextureData<byte>();
            var ptr = textureData.GetUnsafePtr(); 
            // TODO : Use ComputeMipSize to get the size of the mip level
            UnsafeUtility.MemCpy(mat.GetUnsafePtr(), ptr, textureData.Length * UnsafeUtility.SizeOf<byte>());
            if(conversionCode.HasValue)
                Cv2.CvtColor(mat, mat, conversionCode.Value);
            
            if (flip)
            {
                Cv2.Flip(mat, mat, FlipMode.X);
            }
            return mat;
        }
        
        public static unsafe Mat ConvertToMat(void* byteArrayPtr, MatType matType, int width, int height, int byteCount, ColorConversionCodes? conversionCode = null, bool flip = false)
        {
            var mat = new Mat(height, width, matType);
            // TODO : Use ComputeMipSize to get the size of the mip level
            UnsafeUtility.MemCpy(mat.GetUnsafePtr(), byteArrayPtr, byteCount * UnsafeUtility.SizeOf<byte>());
            if(conversionCode.HasValue)
                Cv2.CvtColor(mat, mat, conversionCode.Value);
            if (flip)
            {
                Cv2.Flip(mat, mat, FlipMode.X);
            }
            return mat;
        }
        

        public static unsafe Mat ConvertToMat(this RenderTexture rdrTexture,
            ColorConversionCodes? conversionCodes = null, bool flip = false)
        {
            var toConvert = rdrTexture;
            
            if (flip)
            {
                toConvert = new RenderTexture(rdrTexture.descriptor);
                toConvert.Create();
                FlipMaterial.SetFloat("_FlipY", 1);
                Graphics.Blit(rdrTexture, toConvert, FlipMaterial);
            }
            
            var textureFormat = GraphicsFormatUtility.GetTextureFormat(toConvert.graphicsFormat);
            Mat mat = new Mat(toConvert.height, toConvert.width, GetMatTypeFromTextureFormat(textureFormat));
            
            var fence = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<byte>(
                mat.Data.ToPointer(), (int)GetMatByteSize(mat), Allocator.None);
            NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref fence, AtomicSafetyHandle.Create());
            var request = AsyncGPUReadback.RequestIntoNativeArray(ref fence, toConvert, 0, textureFormat, (request) => {});
            
            request.WaitForCompletion();
            if(conversionCodes.HasValue)
                Cv2.CvtColor(mat, mat, conversionCodes.Value);
            if (flip)
            {
                RenderTexture.active = null;
                toConvert.Release();
            }
            return mat;
        }
        
        public static unsafe void* GetUnsafePtr(this Mat mat)
        {
            return mat.Data.ToPointer();
        }

        public static unsafe Texture2D ConvertToTexture(this Mat mat, ColorConversionCodes? conversionCode = null, bool flip = false)
        {
            // We need to clone the mat because the conversion will be done in place
            var copy = mat.Clone();
            if(conversionCode.HasValue)
                Cv2.CvtColor(copy, copy, conversionCode.Value);
            if (flip)
            {
                Cv2.Flip(copy, copy, FlipMode.X);
            }
            var textureFormat = GetTextureFormatFromMat(copy);
            var texture = new Texture2D(mat.Width, mat.Height, textureFormat, false);
            var byteCount = copy.Total() * copy.ElemSize();
            texture.LoadRawTextureData(copy.Data, (int)byteCount);
            texture.Apply();
            return texture;
        }

        public static long GetMatByteSize(in Mat mat)
        {
            return mat.Total() * mat.ElemSize();
        }
        
        public static Mat ConvertToMat(this Texture texture, ColorConversionCodes? conversionCodes = null, bool flip = false)
        {
            if (texture is Texture2D texture2D)
            {
                return texture2D.ConvertToMat(conversionCodes, flip);
            }
            else if(texture is RenderTexture renderTexture)
            {
                return renderTexture.ConvertToMat(conversionCodes, flip);
            }
            else
            {
                throw new System.Exception($"Unsupported texture type {texture.GetType()}");
            }
        }
    }
}