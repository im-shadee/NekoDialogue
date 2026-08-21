using System;
using UnityEngine;

namespace NekoDialogue
{
    /// <summary>
    /// Utility class providing formatted logging and exception generation tailored for NekoDialogue.
    /// </summary>
    public static class NekoDialogueDebug
    {
        private const string m_kPackagePrefix = "[NekoDialogue]";
        private const string m_kPackageNameColorHex = "#B9B9FA";

        /// <summary>
        /// Logs an error to the Unity console and creates an exception instance.
        /// </summary>
        public static T CreateException<T>(string message, UnityEngine.Object context = null) where T : Exception
        {
            LogError(message, context);

            if (Activator.CreateInstance(typeof(T), message) is T exception)
            {
                return exception;
            }

            // Shade: Fallback if T does not have a public constructor accepting a single string parameter
            return (T)Activator.CreateInstance(typeof(T));
        }

        // Shade: Keep errors visible everywhere so critical issues are never hidden
        /// <summary>
        /// Logs a formatted error message to the Unity console with the package prefix.
        /// </summary>
        public static void LogError(string message, UnityEngine.Object context = null)
        {
            Debug.LogError($"<b><color={m_kPackageNameColorHex}>{m_kPackagePrefix}</color></b> {message}", context);
        }

        /// <summary>
        /// Logs a formatted standard message to the Unity console with the package prefix in Editor builds.
        /// </summary>
        public static void Log(string message, UnityEngine.Object context = null)
        {
            if (PackageConfig.ENABLE_LOGS) Debug.Log($"<b><color={m_kPackageNameColorHex}>{m_kPackagePrefix}</color></b> {message}", context);
        }

        /// <summary>
        /// Logs a formatted warning message to the Unity console with the package prefix in Editor builds.
        /// </summary>
        public static void LogWarning(string message, UnityEngine.Object context = null)
        {
            if (PackageConfig.ENABLE_LOGS) Debug.LogWarning($"<b><color={m_kPackageNameColorHex}>{m_kPackagePrefix}</color></b> {message}", context);
        }
    }
}
