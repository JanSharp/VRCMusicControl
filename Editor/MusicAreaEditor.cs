﻿using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEditor;
using UdonSharpEditor;
using System.Linq;
using System.Collections.Generic;

namespace JanSharp
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MusicArea))]
    public class MusicAreaEditor : Editor
    {
        private SerializedProperty musicForThisAreaProp;
        private SerializedProperty useDefaultPriorityProp;
        private SerializedProperty priorityProp;
        private SerializedProperty syncCurrentMusicAndPriorityProp;

        private void OnEnable()
        {
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

            EditorGUILayout.PropertyField(syncCurrentMusicAndPriorityProp, new GUIContent(
                "Sync Current Music And Priority",
                "When enabled, all values above will be synced whenever they are changed on any client. "
                    + "This setting does not affect the network impact of this script when the values "
                    + "are never changed at runtime."
            ));

            serializedObject.ApplyModifiedProperties();
        }
    }
}