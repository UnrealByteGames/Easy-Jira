using UnityEngine;
#if UNITY_EDITOR
using Mono.Cecil;
using UnityEditor;
#endif

namespace UnrealByte.EasyJira {
	public class EJ_Settings {
		public static JiraSettings asset;
		public const string defaultSettingsAssetName = "UB.EJSettings.asset";

		/// <summary>
		/// Creates the asset to store all the settings.
		/// </summary>
		public static void Awake() {
#if UNITY_EDITOR
			//Search for Jira Settings
			var jirasettingsassets = AssetDatabase.FindAssets("t:JiraSettings");
			if (jirasettingsassets.Length > 0)
				asset = (JiraSettings)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(jirasettingsassets[0]), typeof(JiraSettings));
			else
				asset = CreateJiraObject();
#else
			var jiraSettings = Resources.FindObjectsOfTypeAll<JiraSettings>();
			if (jiraSettings == null || jiraSettings.Length == 0) 
				Debug.LogError("Jira settings asset not found");
			else 
				asset = jiraSettings[0];
#endif
		}

#if UNITY_EDITOR
		/// <summary>
		/// Creates a new asset to store settings.
		/// </summary>
		public static JiraSettings CreateJiraObject() {
			asset = ScriptableObject.CreateInstance<JiraSettings>();
			var bestPath = "";
			//Search for Resources Folder
			var resourcesFolder = AssetDatabase.FindAssets("Resources t:Folder");
			//Find shortest ressources asset
			if (resourcesFolder.Length > 0) {
				for (int i = 0; i < resourcesFolder.Length; i++) {
					var resourcePath = AssetDatabase.GUIDToAssetPath(resourcesFolder[i]);
					if (resourcePath.Length < bestPath.Length || bestPath.Length == 0)
						bestPath = resourcePath;
				}
			} else {
				AssetDatabase.CreateFolder("Assets", "Resources");
				bestPath = "Assets/Resources";
			}

			if (string.IsNullOrEmpty(bestPath)) {
				Debug.LogError("Something went wrong while creating the Jira settings asset. The Settings cannot be saved");
			} else {
				AssetDatabase.CreateAsset(asset, bestPath + "/" + defaultSettingsAssetName);
				AssetDatabase.SaveAssets();
			}


			return asset;
		}
#endif
	}
}