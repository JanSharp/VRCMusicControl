using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class MusicAreaOnBuild
    {
        static MusicAreaOnBuild() => OnBuildUtil.RegisterType<MusicArea>(OnBuild, order: 1);

        private static bool OnBuild(MusicArea musicArea)
        {
            SerializedObject so = new SerializedObject(musicArea);

            if (musicArea.MusicForThisArea == null)
            {
                Debug.LogError($"[MusicControl] {nameof(MusicArea)} {musicArea.name} "
                    + $"must have a non null Music For This Area.", musicArea);
                return false;
            }

            int overlappingWithSpawnPoints = so.FindProperty("overlappingWithSpawnPoints").intValue;
            int triggerCount = musicArea.GetComponents<Collider>().Count(c => c.isTrigger);
            if (overlappingWithSpawnPoints > triggerCount)
            {
                Debug.LogError($"[MusicControl] {nameof(MusicArea)} {musicArea.name} "
                    + $"has Overlapping With Spawn Points set to {overlappingWithSpawnPoints} while there "
                    + $"are only {triggerCount} trigger collider components on this game object. "
                    + $"The player would be considered to be in the area permanently.", musicArea);
                return false;
            }

            if (overlappingWithSpawnPoints != 0)
            {
                if (!MusicManagerOnBuild.musicAreasAtSpawnPointsLut.TryGetValue(musicArea.MusicForThisArea.Manager, out var areas))
                {
                    areas = new();
                    MusicManagerOnBuild.musicAreasAtSpawnPointsLut.Add(musicArea.MusicForThisArea.Manager, areas);
                }
                areas.Add(musicArea);
            }

            return true;
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(MusicArea))]
    public class MusicAreaEditor : Editor
    {
        private SerializedProperty isActiveProp;
        private SerializedProperty musicForThisAreaProp;
        private SerializedProperty useDefaultPriorityProp;
        private SerializedProperty priorityProp;
        private SerializedProperty syncCurrentMusicAndPriorityProp;
        private SerializedProperty overlappingWithSpawnPointsProp;

        private void OnEnable()
        {
            isActiveProp = serializedObject.FindProperty("isActive");
            musicForThisAreaProp = serializedObject.FindProperty("musicForThisArea");
            useDefaultPriorityProp = serializedObject.FindProperty("useDefaultPriority");
            priorityProp = serializedObject.FindProperty("priority");
            syncCurrentMusicAndPriorityProp = serializedObject.FindProperty("syncCurrentMusicAndPriority");
            overlappingWithSpawnPointsProp = serializedObject.FindProperty("overlappingWithSpawnPoints");
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(targets))
                return;
            EditorGUILayout.Space();

            // Intentionally not using this, as I want the 'default priority' label in the middle of the props
            // base.OnInspectorGUI(); // draws public/serializable fields

            EditorGUILayout.PropertyField(isActiveProp);
            EditorGUILayout.PropertyField(musicForThisAreaProp);
            EditorGUILayout.PropertyField(useDefaultPriorityProp);

            var defaultPriorities = targets.Cast<MusicArea>()
                .Select(a => a.MusicForThisArea?.DefaultPriority ?? null)
                .GroupBy(p => p)
                .ToList();
            if (defaultPriorities.Count > 1 || defaultPriorities.First().Key != null)
                EditorGUILayout.LabelField($"Default Priority from Music Descriptor: "
                    + (defaultPriorities.Count > 1 ? "Mixed" : defaultPriorities.First().Key.ToString()));

            EditorGUILayout.PropertyField(priorityProp);
            EditorGUILayout.PropertyField(syncCurrentMusicAndPriorityProp);
            EditorGUILayout.PropertyField(overlappingWithSpawnPointsProp);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
