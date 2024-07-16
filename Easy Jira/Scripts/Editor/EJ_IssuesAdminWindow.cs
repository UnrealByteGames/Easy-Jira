using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using LitJson;

namespace UnrealByte.EasyJira {

    public class EJ_IssuesAdminWindow : EditorWindow {

        float win = Screen.width;
        public Vector2 scrollPos = Vector2.zero;
        static JsonData issuesResponseData = null;
        static List<JIssueDetails> issuesList = new List<JIssueDetails>();

        static int totalResults = 0;
        static int startResult = 0;
        static int endResult = 0;

        // Use this for initialization
        void Awake() {
            if (EJ_Settings.asset == null) {
                EJ_Settings.Awake();
            }
            this.SearchIssuesRequest("default");
        }

        void OnEnable() {
            this.Awake();
        }

        void OnGUI() {
            if (EJ_Settings.asset == null)
                this.Awake();

            Color defaultColor = new Color32(189, 189, 189, 255);
            win = Screen.width;
            EditorStyles.boldLabel.wordWrap = true;
            EditorStyles.label.wordWrap = true;
            EditorStyles.boldLabel.alignment = TextAnchor.MiddleLeft;
            EditorStyles.label.alignment = TextAnchor.MiddleLeft;

            //****** Header buttons and info *****//
            GUI.backgroundColor = defaultColor;
            EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));

            //****** Btn New Issue *****//

            EditorGUILayout.BeginVertical("BOX");
            GUI.backgroundColor = Color.blue;
            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.onNormal.textColor = Color.white;
            buttonStyle.hover.textColor = Color.white;
            buttonStyle.onHover.textColor = Color.white;            
            if (GUILayout.Button("New Issue", buttonStyle, GUILayout.MaxWidth(win / 10f))) {
                //Application.OpenURL(EJ_Settings.asset.jiraBaseRestURL + "/secure/CreateIssue!default.jspa");
                EJ_IssueFormWindow jIssueWindow = (EJ_IssueFormWindow)EditorWindow.GetWindow(typeof(EJ_IssueFormWindow), false, "EJ - New Issue");
            }
            GUI.backgroundColor = Color.clear;
            EditorGUILayout.EndVertical();

            //**** End Btn New Issue ****//

            EditorGUILayout.BeginVertical("BOX");
            EditorGUILayout.LabelField("Showing issues: "+startResult+" - "+endResult+" of "+totalResults+"", GUILayout.MaxWidth(win / 4f));            
            EditorGUILayout.EndVertical();

            //****** Btn prev-next *****//
            EditorGUILayout.BeginVertical("BOX");
            if (GUILayout.Button("Prev.", GUILayout.MaxWidth(win / 10f))) {
                if (startResult > 1) SearchIssuesRequest("prev");
            }            
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("BOX");
            if (GUILayout.Button("Next", GUILayout.MaxWidth(win / 10f))) {
                if (endResult < totalResults) SearchIssuesRequest("next");
            }
            EditorGUILayout.EndVertical();
            //**** End Btn prev-next ****//

            EditorGUILayout.BeginVertical("BOX");
            EditorGUILayout.LabelField("Project: "+ EJ_Settings.asset.jiraProjectKey, GUILayout.MaxWidth(win / 8f));
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("BOX");
            if (GUILayout.Button("Settings", GUILayout.MaxWidth(win / 10f))) {
                EditorWindow.GetWindow(typeof(EJ_SettingsWindow), false, "EJ - Settings");
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("BOX");
            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Refresh", GUILayout.MaxWidth(win / 10f))) {
                SearchIssuesRequest("default");
            }
            GUI.backgroundColor = Color.clear;
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
            //****** End Header buttons and info *****//


            
            /****** TABLE HEADER ****************/
            GUI.backgroundColor = defaultColor;
            EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
            EditorGUILayout.LabelField("Key", EditorStyles.boldLabel, GUILayout.MaxWidth(getSizeByPercentage(10)));
            EditorGUILayout.LabelField("Type", EditorStyles.boldLabel, GUILayout.MaxWidth(getSizeByPercentage(10)));
            EditorGUILayout.LabelField("Priority", EditorStyles.boldLabel, GUILayout.MaxWidth(getSizeByPercentage(8)));
            EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel, GUILayout.MaxWidth(getSizeByPercentage(42)));
            EditorGUILayout.LabelField("Assignee", EditorStyles.boldLabel, GUILayout.MaxWidth(getSizeByPercentage(20)));
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel, GUILayout.MaxWidth(getSizeByPercentage(10)));
            EditorGUILayout.EndHorizontal();
            /****** END TABLE HEADER *************/

            // Set up a scroll view
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, true);

            if (issuesResponseData != null) {
                try {
                    if (issuesList != null) {
                        for (int i = 0; i < issuesList.Count; i++) {
                            JIssueDetails issue = issuesList[i];
                            Rect r = EditorGUILayout.BeginHorizontal("Button", GUILayout.MaxWidth(win));
                            if (GUI.Button(r, GUIContent.none)) {
                                EJ_IssueDetailsWindow jIssueWindow = (EJ_IssueDetailsWindow)EditorWindow.GetWindow(typeof(EJ_IssueDetailsWindow), false, "EJ - Issue");
                                jIssueWindow.Init(issue.key);
                            }
                            EditorGUILayout.LabelField(issue.key, GUILayout.MaxWidth(getSizeByPercentage(10)));
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(getSizeByPercentage(10)));
                            GUILayout.Label(issue.issueTypeIconTexture, GUILayout.Width(20), GUILayout.Height(20));
                            EditorGUILayout.LabelField(issue.type);
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(getSizeByPercentage(8)));
                            GUILayout.Label(issue.priorityIconTexture, GUILayout.Width(20), GUILayout.Height(20));
                            //GUILayout.Label(issue.priority, GUILayout.Width(win / 15f), GUILayout.MinWidth(win / 15f), GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.LabelField(issue.summary, GUILayout.MaxWidth(getSizeByPercentage(42)));
                            EditorGUILayout.LabelField(issue.assignee, GUILayout.MaxWidth(getSizeByPercentage(20)));
                            //GUILayout.Label(issue.statusIconTexture, GUILayout.Width(20), GUILayout.Height(20));
                            EditorGUILayout.LabelField(issue.status, GUILayout.MaxWidth(getSizeByPercentage(10)));
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                } catch (Exception e) {
                    Debug.Log(e.Message);
                }
            }
            EditorGUILayout.Space();
            // Close the scroll view
            EditorGUILayout.EndScrollView();
        }

        void SearchIssuesRequest(string action) {
            issuesList.Clear();
            if (startResult == 0) startResult = 1;
            EditorUtility.DisplayProgressBar("Easy Jira", "Loading issues", 5);
            if (action.Equals("next")) {                
                this.StartCoroutine(JiraConnect.APISearchIssues(EJ_Settings.asset, startResult + EJ_Settings.asset.maxResults, EJ_Settings.asset.maxResults, SearchCallback));                
            } else if (action.Equals("prev")) {             
                this.StartCoroutine(JiraConnect.APISearchIssues(EJ_Settings.asset, startResult - EJ_Settings.asset.maxResults, EJ_Settings.asset.maxResults, SearchCallback));
            } else {
                this.StartCoroutine(JiraConnect.APISearchIssues(EJ_Settings.asset, 0, EJ_Settings.asset.maxResults, SearchCallback));
            }           
        }

        void SearchCallback(String response) {
            EditorUtility.ClearProgressBar();
            if (response != null && response.Length > 0) {
                if (response.StartsWith("error"))
                    Debug.LogError(response);

                issuesResponseData = JsonMapper.ToObject(response);
#pragma warning disable 0168
                try {
                    if(issuesResponseData["startAt"].IsInt) {
                        startResult = int.TryParse(issuesResponseData["startAt"].ToString(), out startResult) ? startResult : 0;
                    }
                    if (issuesResponseData["maxResults"].IsInt) {
                        int auxEndResult = 0;
                        auxEndResult = int.TryParse(issuesResponseData["maxResults"].ToString(), out auxEndResult) ? auxEndResult : 0;
                        endResult = startResult + auxEndResult;
                        if(endResult > totalResults) {
                            endResult = totalResults;
                        }
                    }
                    if (issuesResponseData["total"].IsInt) {
                        totalResults = int.TryParse(issuesResponseData["total"].ToString(), out totalResults) ? totalResults : 0;
                    }
                    if (issuesResponseData["issues"] != null) {
                        for (int i = 0; i < issuesResponseData["issues"].Count; i++) {
                            JIssueDetails issue = new JIssueDetails();
                            issue.getIssueFromJsonString("", issuesResponseData["issues"][i]);
                            if(issue != null) {
                                issuesList.Add(issue);
                            }
                        }
                        foreach (JIssueDetails issue in issuesList) {
                            if (issue.projectIconURL.Length > 0) {
                                this.StartCoroutine(JiraConnect.GetTexture(issue.projectIconURL.EndsWith(".png") ? issue.projectIconURL : issue.projectIconURL + "&format=png", value => issue.projectIconTexture = value));
                            }
                            if (issue.typeIconURL.Length > 0) {
                                this.StartCoroutine(JiraConnect.GetTexture(issue.typeIconURL.EndsWith(".png") ? issue.typeIconURL : issue.typeIconURL + "&format=png", value => {
                                    issue.issueTypeIconTexture = value;
                                    TextureScale.Bilinear(issue.issueTypeIconTexture, issue.issueTypeIconTexture.width * 2, issue.issueTypeIconTexture.height * 2);
                                }));
                            }
                            if (issue.priorityIconURL.Length > 0) {
                                this.StartCoroutine(JiraConnect.GetTexture(issue.priorityIconURL.EndsWith(".png") ? issue.priorityIconURL : issue.priorityIconURL + "&format=png", value => issue.priorityIconTexture = value));
                            }
                            if (issue.statusIconURL.Length > 0) {
                                this.StartCoroutine(JiraConnect.GetTexture(issue.statusIconURL.EndsWith(".png") ? issue.statusIconURL : issue.statusIconURL + "&format=png", value => issue.statusIconTexture = value));
                            }
                        }
                    }
                } catch(Exception e) {
                    EditorUtility.ClearProgressBar();
                }
#pragma warning restore 0168
            }
            EditorUtility.ClearProgressBar();
        }

        public void OnInspectorUpdate() {            
            this.Repaint();
        }

        float getSizeByPercentage(float percent) {
            return (win * percent) / 100;
        }
    }
}