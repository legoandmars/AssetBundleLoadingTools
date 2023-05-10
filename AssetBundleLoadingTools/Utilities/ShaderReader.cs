using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using IPA.Utilities;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleLoadingTools.Utilities
{
    /// <summary>
    /// Class for reading shader data and checking of SPI support
    /// taken from https://github.com/ToniMacaroni/UnsafeShaderTools
    /// </summary>
    internal static class ShaderReader
    {
        private const string TexArrayKeyword = "SV_RenderTargetArrayIndex";

        public static bool DebugMode { get; set; }

        public static bool IsSinglePassInstancedSupported(Shader shader)
        {
            return CheckForSemantic(shader, TexArrayKeyword);
        }
        
        public static bool CheckForSemantic(Shader shader, string semanticName)
        {
            var dataBlocks = ReadShaderData(shader);
            
            if (dataBlocks.Count < 1)
            {
                return false;
            }

            foreach (var dataBlock in dataBlocks)
            {
                var str = Encoding.UTF8.GetString(dataBlock, 0, dataBlock.Length);
                if (str.Contains(semanticName))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<byte[]> ReadShaderData(Shader shader)
        {
            var result = new List<byte[]>();
            
            var ptr = shader.InvokeMethod<IntPtr, Object>("GetCachedPtr");
            if (ptr == IntPtr.Zero)
            {
                Log("Shader ptr was not valid");
                return result;
            }
            
            var dataPtrs = ReadShaderDataUnsafe(ptr);

            if (dataPtrs.Count < 1)
            {
                Log("No data ptrs");
            }

            foreach (var dataPtr in dataPtrs)
            {
                var data = new byte[1024]; // proper reading later, for now just read some amount and hope it's enough
                Marshal.Copy(dataPtr, data, 0, data.Length);
                result.Add(data);
            }
            
            return result;
        }

        public static unsafe List<IntPtr> ReadShaderDataUnsafe(IntPtr shaderPtr)
        {
            var result = new List<IntPtr>();
            
            IntPtr intShaderPtr = *(IntPtr*)(shaderPtr + 80);
            IntPtr* subShaderListPtr = *(IntPtr**)intShaderPtr;
            int numSubShaders = *(int*)(intShaderPtr + 24);

            for (int subShaderIdx = 0; subShaderIdx < numSubShaders; subShaderIdx++)
            {
                IntPtr subShaderPtr = subShaderListPtr[subShaderIdx];
                Log($"Parsing subshader ptr {subShaderPtr.ToString("x")}");
            
                if (*(int*)subShaderPtr != 0 || *(byte*)(subShaderPtr+4) != 0xff)
                {
                    Log("SubShader check failed");
                    return result;
                }

                IntPtr* passListPtr = *(IntPtr**)(subShaderPtr + 112);
                int numPasses = *(int*)(subShaderPtr + 136);
                Log("Passes: " + numPasses);

                for (int passIdx = 0; passIdx < numPasses; passIdx++)
                {
                    IntPtr passPtr = passListPtr[passIdx*2]; // *2 because of the list structure (each entry is 2 pointers wide)
                    Log($"Parsing pass ptr {passPtr.ToString("x")}");
                    IntPtr progPtr = *(IntPtr*)(passPtr+120);
                    if (progPtr == IntPtr.Zero)
                    {
                        Log("No program in this pass");
                        continue;
                    }
                    
                    IntPtr* subProgListPtr = *(IntPtr**)(progPtr + 16);
                    int numSubProgs = *(int*)(progPtr + 40);
                    Log("Subprogs: " + numSubProgs);

                    for (int subProgIdx = 0; subProgIdx < numSubProgs; subProgIdx++)
                    {
                        IntPtr subProgPtr = subProgListPtr[subProgIdx];
                        Log($"Parsing subprog ptr {subProgPtr.ToString("x")}");

                        IntPtr dataptr = *(IntPtr*)(subProgPtr + 16);

                        if (dataptr == IntPtr.Zero)
                        {
                            Log("Data ptr was null in this subprog");
                            continue;
                        }
                        
                        var shaderType = *(byte*)dataptr;

                        if (shaderType > 2)
                        {
                            continue;
                        }
                        
                        var dxbcAddr = dataptr + (shaderType==1?6:38);
                        var dxbc = *(byte*)dxbcAddr;

                        if (dxbc != 0x44)
                        {
                            Log($"No DXBC header for {dataptr.ToString("x")} (type {shaderType:x})");
                        }

                        Log($"Data: {dataptr.ToString("x")}");
                        result.Add(dataptr);
                    }
                }
            }
            
            return result;
        }

        private static void Log(string message, bool alwaysLog = false)
        {
            if (!DebugMode && !alwaysLog)
            {
                return;
            }

            Console.WriteLine(message);
        }
    }
}