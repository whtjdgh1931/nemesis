using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderVariantCollector : IPreprocessShaders
{
		public int callbackOrder => 0;

		// 빌드 과정에서 호출됨
		public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
		{
				foreach (var variant in data)
				{
						ShaderVariantDatabase.RecordVariant(shader, snippet, variant);
				}
		}
}

// Variant 정보를 임시로 저장하는 클래스
public static class ShaderVariantDatabase
{
		private static List<(Shader, PassType, string[])> collectedVariants = new List<(Shader, PassType, string[])>();

		public static void RecordVariant(Shader shader, ShaderSnippetData snippet, ShaderCompilerData variant)
		{
				var keywords = variant.shaderKeywordSet.GetShaderKeywords();
				string[] keywordNames = new string[keywords.Length];
				for (int i = 0; i < keywords.Length; i++)
						keywordNames[i] = keywords[i].name;

				collectedVariants.Add((shader, snippet.passType, keywordNames));
		}

		// ShaderVariantCollection 생성
		[MenuItem("Tools/Build/Generate ShaderVariantCollection")]
		public static void GenerateCollection()
		{
				var svc = new ShaderVariantCollection();

				foreach (var entry in collectedVariants)
				{
						svc.Add(new ShaderVariantCollection.ShaderVariant(entry.Item1, entry.Item2, entry.Item3));
				}

				string path = "Assets/PreloadedVariants.shadervariants";
				AssetDatabase.CreateAsset(svc, path);
				AssetDatabase.SaveAssets();

				Debug.Log($"ShaderVariantCollection 생성 완료: {path}");
		}
}
