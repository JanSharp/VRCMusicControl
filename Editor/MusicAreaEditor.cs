using UnityEngine;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class MusicAreaOnBuild
    {
        static MusicAreaOnBuild() => OnBuildUtil.RegisterType<MusicArea>(OnBuild);

        private static bool OnBuild(MusicArea musicArea)
        {
            if (musicArea.MusicForThisArea == null)
            {
                Debug.LogError($"[MusicControl] {nameof(MusicArea)} {musicArea.name} "
                    + $"must have a non null Music For This Area.", musicArea);
                return false;
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

        private void OnEnable()
        {
            isActiveProp = serializedObject.FindProperty("isActive");
            musicForThisAreaProp = serializedObject.FindProperty("musicForThisArea");
            useDefaultPriorityProp = serializedObject.FindProperty("useDefaultPriority");
            priorityProp = serializedObject.FindProperty("priority");
            syncCurrentMusicAndPriorityProp = serializedObject.FindProperty("syncCurrentMusicAndPriority");
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

            serializedObject.ApplyModifiedProperties();
        }
    }
}
