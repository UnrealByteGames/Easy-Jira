using System;
using UnityEngine;
using UnityEditor;

namespace UnrealByte.EasyJira {

    public class EJ_SettingsWindow : EditorWindow {

        void OnGUI() {
            if (EJ_Settings.asset == null) EJ_Settings.Awake();
            GUILayout.BeginVertical("BOX");
            GUILayout.Label("Easy Jira Settings", EditorStyles.boldLabel);
            EJ_Settings.asset.jiraBaseRestURL = EditorGUILayout.TextField("Jira Base URL", EJ_Settings.asset.jiraBaseRestURL);
            EJ_Settings.asset.jiraUser = EditorGUILayout.TextField("User", EJ_Settings.asset.jiraUser);
            EJ_Settings.asset.jiraToken = EditorGUILayout.PasswordField("API Token", EJ_Settings.asset.jiraToken);
            EJ_Settings.asset.jiraProjectKey = EditorGUILayout.TextField("Project Key", EJ_Settings.asset.jiraProjectKey);
            EJ_Settings.asset.maxResults = EditorGUILayout.IntField("Results per page", EJ_Settings.asset.maxResults);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Issue details window", EditorStyles.boldLabel);
            EJ_Settings.asset.maxShowAttachments = EditorGUILayout.IntField("Max Attachments show", EJ_Settings.asset.maxShowAttachments);
            EJ_Settings.asset.showAttachAtInit = EditorGUILayout.Toggle("Open attachments list", EJ_Settings.asset.showAttachAtInit);
            EJ_Settings.asset.showCommentsAtInit = EditorGUILayout.Toggle("Open comments list", EJ_Settings.asset.showCommentsAtInit);
            EJ_Settings.asset.showHistoryAtInit = EditorGUILayout.Toggle("Open history", EJ_Settings.asset.showHistoryAtInit);
            EditorGUILayout.Space();
            if (GUILayout.Button("Validate & Save")) {
                EditorUtility.SetDirty(EJ_Settings.asset);
                AssetDatabase.SaveAssets();
                this.StartCoroutine(JiraConnect.APITestAuth(EJ_Settings.asset, (response) => {
                    EditorUtility.SetDirty(EJ_Settings.asset);
                    AssetDatabase.SaveAssets();
                    this.ShowNotification(new GUIContent(response));
                }));                
            }
            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }
    }    
}
