using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class TMPBatchFontResizer : EditorWindow
{
    enum Mode { SetAbsolute, Multiply }

    Mode mode = Mode.SetAbsolute;
    float value = 24f;
    bool include3DText = false; // TextMeshPro (3D) vs TextMeshProUGUI
    bool disableAutoSizing = false;
    bool processPrefabs = false;

    [MenuItem("Tools/TMP/Batch Font Resizer")]
    static void OpenWindow()
    {
        GetWindow<TMPBatchFontResizer>("TMP Font Resizer");
    }

    void OnGUI()
    {
        GUILayout.Label("TMP Batch Font Resizer", EditorStyles.boldLabel);
        mode = (Mode)EditorGUILayout.EnumPopup("Mode", mode);
        if (mode == Mode.SetAbsolute)
            value = EditorGUILayout.FloatField("Set Font Size", value);
        else
            value = EditorGUILayout.FloatField("Multiply Factor", value);

        include3DText = EditorGUILayout.Toggle("Include 3D Text (TextMeshPro)", include3DText);
        disableAutoSizing = EditorGUILayout.Toggle("Disable Auto Size if enabled", disableAutoSizing);
        processPrefabs = EditorGUILayout.Toggle("Also process prefabs in project", processPrefabs);

        GUILayout.Space(8);
        if (GUILayout.Button("Apply to Open Scenes"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Apply changes to all currently open scenes?", "OK", "Cancel"))
                ProcessOpenScenes();
        }

        if (GUILayout.Button("Apply to All Scenes in Project"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "This will open and modify every scene in the project. Make sure you have a backup. Continue?", "OK", "Cancel"))
                ProcessAllScenesInProject();
        }

        if (GUILayout.Button("Apply to Selected GameObjects"))
        {
            if (EditorUtility.DisplayDialog("Confirm", "Apply changes to all TMP components in selected GameObjects (and their children)?", "OK", "Cancel"))
                ProcessSelected();
        }

        if (processPrefabs)
        {
            if (GUILayout.Button("Apply to All Prefabs in Project"))
            {
                if (EditorUtility.DisplayDialog("Confirm", "This will modify all prefabs that contain TMP components. Make sure you have a backup. Continue?", "OK", "Cancel"))
                    ProcessAllPrefabs();
            }
        }

        GUILayout.Space(6);
        if (GUILayout.Button("Close"))
            Close();
    }

    void ProcessOpenScenes()
    {
        int changed = 0;
        int skippedAuto = 0;
        foreach (var scene in GetOpenScenes())
        {
            changed += ProcessScene(scene, out int skipped, out int modified) ? modified : 0;
            skippedAuto += skipped;
        }
        EditorUtility.DisplayDialog("Done", $"Modified {changed} TMP components.\nSkipped {skippedAuto} auto-sized components.", "OK");
    }

    void ProcessAllScenesInProject()
    {
        string[] guids = AssetDatabase.FindAssets("t:Scene");
        int totalModified = 0;
        int totalSkippedAuto = 0;
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            
            if (ProcessScene(scene, out int skipped, out int modified))
            {
                totalModified += modified;
                totalSkippedAuto += skipped;
                if (modified > 0)
                {
                    EditorSceneManager.MarkSceneDirty(scene);
                    EditorSceneManager.SaveScene(scene);
                }
            }
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", $"Modified {totalModified} TMP components across {guids.Length} scenes.\nSkipped {totalSkippedAuto} auto-sized components.", "OK");
    }

    void ProcessSelected()
    {
        var roots = Selection.gameObjects;
        if (roots == null || roots.Length == 0)
        {
            EditorUtility.DisplayDialog("No Selection", "Please select one or more root GameObjects in Hierarchy.", "OK");
            return;
        }
        int modified = 0;
        int skipped = 0;
        foreach (var go in roots)
        {
            var tmps = go.GetComponentsInChildren<TMP_Text>(true)
                .Where(t => include3DText || t is TextMeshProUGUI)
                .ToArray();
            foreach (var t in tmps)
            {
                if (ApplyToTMP(t)) modified++; else skipped++;
                EditorUtility.SetDirty(t);
            }
        }
        EditorUtility.DisplayDialog("Done", $"Modified {modified} components, skipped {skipped}.", "OK");
    }

    void ProcessAllPrefabs()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int totalModified = 0;
        int totalSkipped = 0;
        for (int i = 0; i < prefabGuids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(prefabGuids[i]);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            bool changed = false;
            var tmps = root.GetComponentsInChildren<TMP_Text>(true)
                .Where(t => include3DText || t is TextMeshProUGUI)
                .ToArray();
            int modified = 0;
            int skipped = 0;
            foreach (var t in tmps)
            {
                if (ApplyToTMP(t)) { modified++; changed = true; }
                else skipped++;
            }
            if (changed)
            {
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            PrefabUtility.UnloadPrefabContents(root);
            totalModified += modified;
            totalSkipped += skipped;
        }
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Done", $"Modified {totalModified} components in prefabs. Skipped {totalSkipped}.", "OK");
    }

    bool ProcessScene(Scene scene, out int skippedAutoCount, out int modifiedCount)
    {
        skippedAutoCount = 0;
        modifiedCount = 0;
        if (!scene.isLoaded) return false;
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            var tmps = root.GetComponentsInChildren<TMP_Text>(true)
                .Where(t => include3DText || t is TextMeshProUGUI)
                .ToArray();
            foreach (var t in tmps)
            {
                if (ApplyToTMP(t)) modifiedCount++; else skippedAutoCount++;
                EditorUtility.SetDirty(t);
            }
        }
        return true;
    }

    bool ApplyToTMP(TMP_Text t)
    {
        if (t == null) return false;

        if (t.enableAutoSizing)
        {
            if (!disableAutoSizing)
            {
                // skip auto sized components by default
                return false;
            }
            else
            {
                t.enableAutoSizing = false;
            }
        }

        Undo.RecordObject(t, "TMP Font Size Change");

        if (mode == Mode.SetAbsolute)
        {
            t.fontSize = value;
        }
        else // Multiply
        {
            // fontSizeMultiplier이 없는 경우에는 fontSize를 직접 조정합니다.
            t.fontSize = t.fontSize * value;
        }

        EditorUtility.SetDirty(t);
        return true;
    }

    static IEnumerable<Scene> GetOpenScenes()
    {
        for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            yield return EditorSceneManager.GetSceneAt(i);
    }
}