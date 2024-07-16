using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnrealByte.EasyJira {

    public class EJ_Settings {

        public static JiraSettings asset;
        
        /// <summary>
        /// Creates the asset to store all the settings.
        /// </summary>
        public static void Awake() {
            UnityEngine.Object jiraSettings = Resources.Load("UB.EJSettings");

            if (jiraSettings == null) {
                asset = CreateJiraObject();
            } else {
                asset = (JiraSettings)Resources.Load("UB.EJSettings");
            }
        }

        /// <summary>
        /// Creates a new asset to store settings.
        /// </summary>
        public static JiraSettings CreateJiraObject() {

            asset = ScriptableObject.CreateInstance<JiraSettings>();

            AssetDatabase.CreateAsset(asset, "Assets/Easy Jira/Resources/UB.EJSettings.asset");
            AssetDatabase.SaveAssets();
            return asset;
        }

    }
}
