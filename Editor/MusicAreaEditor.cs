using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;

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

        private static GUIContent priorityLabel = new GUIContent("Priority");
        private static GUIContent useDefaultLabel = new GUIContent("Use Default");

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

            serializedObject.Update();

            EditorGUILayout.PropertyField(isActiveProp);
            EditorGUILayout.PropertyField(musicForThisAreaProp);
            DrawPriority();
            EditorGUILayout.PropertyField(syncCurrentMusicAndPriorityProp);

            serializedObject.ApplyModifiedProperties();
        }

        private bool TryGetDefaultPriority(out int priority)
        {
            bool result = false;
            priority = 0;
            foreach (MusicArea target in targets.Cast<MusicArea>())
            {
                if (target.MusicForThisArea == null)
                    continue;
                if (result && priority != target.MusicForThisArea.DefaultPriority)
                    return false;
                result = true;
                priority = target.MusicForThisArea.DefaultPriority;
            }
            return result;
        }

        private void DrawPriority()
        {
            Rect rect = EditorGUILayout.GetControlRect(hasLabel: true, EditorGUIUtility.singleLineHeight);

            bool cannotEditPriority = useDefaultPriorityProp.hasMultipleDifferentValues || useDefaultPriorityProp.boolValue;
            using (var priorityScope = new EditorGUI.PropertyScope(rect, priorityLabel, priorityProp))
            using (new EditorGUI.DisabledScope(cannotEditPriority))
            {
                EditorGUI.PrefixLabel(rect, priorityScope.content);
                rect.x += EditorGUIUtility.labelWidth;
                Rect rightRect = rect;
                rightRect.x += 90f;
                if (!cannotEditPriority)
                    EditorGUI.PropertyField(rightRect, priorityProp, GUIContent.none);
                else
                {
                    int defaultPriority = 0;
                    EditorGUI.showMixedValue = useDefaultPriorityProp.hasMultipleDifferentValues
                        || !TryGetDefaultPriority(out defaultPriority);
                    EditorGUI.IntField(rightRect, defaultPriority);
                }
            }

            rect.width = 90f;
            using var useDefaultScope = new EditorGUI.PropertyScope(rect, useDefaultLabel, useDefaultPriorityProp);
            EditorGUI.PrefixLabel(rect, useDefaultScope.content);
            rect.x += 70f;
            EditorGUI.PropertyField(rect, useDefaultPriorityProp, GUIContent.none);
        }
    }
}
