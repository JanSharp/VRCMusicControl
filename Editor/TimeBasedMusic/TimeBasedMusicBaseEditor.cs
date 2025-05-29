using UnityEngine;
using UnityEditor;
using System.Linq;

namespace JanSharp
{
    [InitializeOnLoad]
    public static class TimeBasedMusicBaseOnBuild
    {
        // Uses order 1 in order to run after all MusicDescriptors have been assigned the MusicManager.
        static TimeBasedMusicBaseOnBuild()
            => OnBuildUtil.RegisterType<TimeBasedMusicBase>(OnBuild, 1);

        private static bool Validate(
            TimeBasedMusicBase timeBasedMusicBase,
            SerializedObject proxy,
            out float firstTimeStamp,
            out float lastMusicEndTime)
        {
            firstTimeStamp = 0f;
            lastMusicEndTime = 0f;

            SerializedProperty timeStampsProp = proxy.FindProperty("timeStamps");
            SerializedProperty musicAtTimeStampsProp = proxy.FindProperty("musicAtTimeStamps");

            if (timeStampsProp.arraySize != musicAtTimeStampsProp.arraySize)
            {
                Debug.LogError($"[MusicControl] The amount of Time Stamps and Music At Time Stamps for "
                    + $"{timeBasedMusicBase.name} must be identical.", timeBasedMusicBase);
                return false;
            }

            if (timeStampsProp.arraySize == 0)
            {
                Debug.LogError($"[MusicControl] There must be at least 1 entry in Time Stamps and Music At "
                    + $"Time Stamps for {timeBasedMusicBase.name}.", timeBasedMusicBase);
                return false;
            }

            float prevTimeStamp = float.NegativeInfinity;
            foreach (SerializedProperty timeStampProp in EditorUtil.EnumerateArrayProperty(timeStampsProp))
            {
                float timeStamp = timeStampProp.floatValue;
                if (timeStamp <= prevTimeStamp)
                {
                    Debug.LogError($"[MusicControl] The Time Stamps for {timeBasedMusicBase.name} must be "
                        + $"sequential, ascending and not equal to each other.", timeBasedMusicBase);
                    return false;
                }
                prevTimeStamp = timeStamp;
            }
            firstTimeStamp = timeStampsProp.GetArrayElementAtIndex(0).floatValue;

            lastMusicEndTime = proxy.FindProperty("lastMusicEndTime").floatValue;

            if (lastMusicEndTime <= prevTimeStamp)
            {
                Debug.LogError($"[MusicControl] The Last Music End Time for {timeBasedMusicBase.name} must "
                    + $"greater than the last value in Time Stamps.", timeBasedMusicBase);
                return false;
            }

            MusicManager targetManager = (MusicManager)proxy.FindProperty("targetManager").objectReferenceValue;

            if (targetManager == null)
            {
                Debug.LogError($"[MusicControl] The Target Manger for {timeBasedMusicBase.name} must not be "
                    + $"null.", timeBasedMusicBase);
                return false;
            }

            if (EditorUtil.EnumerateArrayProperty(musicAtTimeStampsProp)
                .Select(p => (MusicDescriptor)p.objectReferenceValue)
                .Any(d => d != null && d.Manager != targetManager))
            {
                Debug.LogError($"[MusicControl] Every descriptor in Music At Time Stamps for "
                    + $"{timeBasedMusicBase.name} must apart of the same {nameof(MusicManager)} as the "
                    + $"defined Target Manager.", timeBasedMusicBase);
                return false;
            }

            return true;
        }

        private static bool OnBuild(TimeBasedMusicBase timeBasedMusicBase)
        {
            SerializedObject proxy = new SerializedObject(timeBasedMusicBase);

            if (!Validate(
                timeBasedMusicBase,
                proxy,
                out float firstTimeStamp,
                out float lastMusicEndTime))
            {
                return false;
            }

            proxy.FindProperty("musicStartTime").floatValue = firstTimeStamp;
            proxy.FindProperty("totalMusicLength").floatValue = lastMusicEndTime - firstTimeStamp;

            proxy.ApplyModifiedProperties();

            return true;
        }
    }
}
