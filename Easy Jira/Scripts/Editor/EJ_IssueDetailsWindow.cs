using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEngine.Networking;


namespace UnrealByte.EasyJira {

    public class EJ_IssueDetailsWindow : EditorWindow {
        
        string issueKey;
        JIssueDetails issue;

        public static Action<String> projectIconCallback;
        public static Action<String> typeIconCallback;
        public static Action<String> priorityIconCallback;
        public static Action<String> statusIconCallback;

        AnimBool m_ShowAttachments;
        AnimBool m_ShowComments;
        AnimBool m_ShowHistory;

        [SerializeField]
        AutocompleteSearchField autocompleteSearchField;

        /** Utils **/
        float win = Screen.width;
        public Vector2 scrollPos = Vector2.zero;
        public string newComment = "";
        private List<JUser> globalAssigneeSearchResults;
        private JUser selectedAssignableUser; 

        // Use this for initialization
        void Awake() {
            if (EJ_Settings.asset == null) {
                EJ_Settings.Awake(); 
            } 
        }

        void OnEnable() {
            this.Awake();
            m_ShowAttachments = new AnimBool(EJ_Settings.asset.showAttachAtInit);
            m_ShowAttachments.valueChanged.AddListener(Repaint);
            m_ShowComments = new AnimBool(EJ_Settings.asset.showCommentsAtInit);
            m_ShowComments.valueChanged.AddListener(Repaint);
            m_ShowHistory = new AnimBool(EJ_Settings.asset.showHistoryAtInit);
            m_ShowHistory.valueChanged.AddListener(Repaint);

            if (autocompleteSearchField == null) autocompleteSearchField = new AutocompleteSearchField();
            autocompleteSearchField.onInputChanged = OnAssignedInputChanged;
            autocompleteSearchField.onConfirm = OnAssignedConfirm;
        }

        /// <summary>
        /// Initialization function.
        /// Runs when the user select "view" an issue from administration.
        /// </summary>
        /// <param name="iKey">Issue Key</param>
        public void Init(string iKey) {
            issueKey = iKey;
            EditorUtility.DisplayProgressBar("Easy Jira", "Loading issue", 5);
            this.StartCoroutine(JiraConnect.APIGetIssue(EJ_Settings.asset, issueKey, DownloadIssueCallback));
            autocompleteSearchField.searchString = "";
            autocompleteSearchField.ClearResults();
        }

        void OnAssignedInputChanged(string searchString) {
            autocompleteSearchField.ClearResults();
            if (!string.IsNullOrEmpty(searchString) && searchString.Length == 2) {
                this.StartCoroutine(JiraConnect.APIDownloadUsersAssignableToIssues(EJ_Settings.asset, searchString, issueKey, value => {
                    globalAssigneeSearchResults = value;
                    foreach (JUser user in value) {
                        autocompleteSearchField.AddResult(user.displayName);
                    }
                }));
            }
        }

        void OnAssignedConfirm(string result) {
            foreach (JUser user in globalAssigneeSearchResults) {
                if (user.displayName == result) {
                    selectedAssignableUser = user;
                }
            }            
        }

        void OnGUI() {
            win = Screen.width;
            EditorStyles.boldLabel.wordWrap = true;
            EditorStyles.label.wordWrap = true;
            EditorStyles.boldLabel.alignment = TextAnchor.MiddleLeft;
            EditorStyles.label.alignment = TextAnchor.MiddleLeft;

            if (issue == null) {                
                EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(win));
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("SELECT AN ISSUE FROM EASY JIRA ISSUES ADMIN: ", EditorStyles.boldLabel);
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Tools > Easy Jira > Issues Admin", EditorStyles.boldLabel);
                EditorGUILayout.Space();                
                EditorGUILayout.EndVertical();
            } else {
                // HEADER 
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(getSizeByPercentage(10)));
                GUILayout.Label(issue.projectIconTexture);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical(GUILayout.MaxWidth(getSizeByPercentage(90)));
                EditorGUILayout.LabelField(issue.projectName + " / " + issueKey);
                EditorGUILayout.LabelField(issue.summary.ToUpper(), EditorStyles.boldLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                
                // HEADER BUTTONS
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                if (GUILayout.Button("View on web")) {
                    Application.OpenURL(EJ_Settings.asset.jiraBaseRestURL + "/browse/" + issueKey);
                }
                if (!EJ_Settings.asset.assignIssues) GUI.enabled = false;
                if (GUILayout.Button("Assign to me")) {
                    this.StartCoroutine(JiraConnect.APIAssignIssue(EJ_Settings.asset, issueKey, EJ_Settings.asset.jiraAccountId, AssignIssueCallback));
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();

                if (EJ_Settings.asset.assignIssues) {
                    //ASSIGN
                    EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(win), GUILayout.ExpandHeight(true));
                    GUILayout.Label("Search User to assign", EditorStyles.boldLabel);
                    autocompleteSearchField.OnGUI();
                    if (GUILayout.Button("Assign")) {
                        if (selectedAssignableUser != null) {
                            this.StartCoroutine(JiraConnect.APIAssignIssue(EJ_Settings.asset, issueKey, selectedAssignableUser.accountId, AssignIssueCallback));
                        }                        
                        autocompleteSearchField.searchString = "";
                        autocompleteSearchField.ClearResults();
                        Init(issueKey);
                    }
                    EditorGUILayout.EndVertical();
                    //END ASSIGN
                }

                //DETAILS
                // Set up a scroll view
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos, true, true);

                //BEGIN details & people
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                //COLUMN 1 - Details
                EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(getSizeByPercentage(50)));

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Type: ", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(getSizeByPercentage(35)));
                GUILayout.Label(issue.issueTypeIconTexture, GUILayout.Width(20), GUILayout.Height(20));
                EditorGUILayout.LabelField(issue.type);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Priority: ", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(getSizeByPercentage(35)));
                GUILayout.Label(issue.priorityIconTexture);
                EditorGUILayout.LabelField(issue.priority);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Affect versions: ", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField(issue.affectVersions, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Components: ", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField(issue.components, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Labels: ", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField(issue.labels, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Status: ", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(getSizeByPercentage(35)));
                GUILayout.Label(issue.statusIconTexture);
                EditorGUILayout.LabelField(issue.status);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Resolution: ", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField(issue.resolution, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                //COLUMN 2 - People
                EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(getSizeByPercentage(50)));

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Asignee:", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField(issue.assignee, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Reporter:", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField(issue.reporter, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Created:", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField("" + issue.dateCreated, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal("BOX");
                EditorGUILayout.LabelField("Updated:", EditorStyles.boldLabel, GUILayout.MinWidth(getSizeByPercentage(15)));
                EditorGUILayout.LabelField("" + issue.dateUpdated, GUILayout.MaxWidth(getSizeByPercentage(35)));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                //END details & people                 
                
                //BEGIN description
                EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(win));
                EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
                EditorGUILayout.LabelField(issue.description);
                EditorGUILayout.EndVertical();
                //END description

                //BEGIN attachments header
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                m_ShowAttachments.target = EditorGUILayout.ToggleLeft("Show Attachments", m_ShowAttachments.target, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                //END attachments header               

                //Extra block that can be toggled on and off.
                if (EditorGUILayout.BeginFadeGroup(m_ShowAttachments.faded)) {
                    EditorGUI.indentLevel++;
                    if (issue.attachments.Count > 0) {
                        int cont = 0;
                        foreach (JIssueAttachment attach in issue.attachments) {
                            if (cont >= EJ_Settings.asset.maxShowAttachments) break;
                            //BEGIN Attachment block
                            EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(win));
                            EditorGUILayout.LabelField(DateTime.Parse(attach.created).ToShortDateString() + " - " + attach.fileName);
                            if (GUILayout.Button("Download")) {
                                Application.OpenURL(attach.content);
                            }
                            EditorGUILayout.EndVertical();
                            //END Attachment block
                            cont++;
                        }
                    } else {
                        EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                        EditorGUILayout.LabelField("No attachments");
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();                

                //BEGIN comments header
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                m_ShowComments.target = EditorGUILayout.ToggleLeft("Show Comments", m_ShowComments.target, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                //END comments header

                if (EditorGUILayout.BeginFadeGroup(m_ShowComments.faded)) {
                    EditorGUI.indentLevel++;
                    if (issue.comments.Count > 0) {
                        foreach (JIssueComment comment in issue.comments) {
                            //BEGIN Comment block
                            EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                            EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(40));
                            GUILayout.Label(comment.authorAvatarTexture, GUILayout.Width(40), GUILayout.Height(40));                            
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(getSizeByPercentage(70)));
                            EditorGUILayout.LabelField(comment.created);
                            EditorGUILayout.LabelField(comment.authorDisplayName + ":", EditorStyles.boldLabel);                            
                            EditorGUILayout.LabelField(comment.body);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                            //END Comment block
                        }
                    } else {
                        EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                        EditorGUILayout.LabelField("No comments");
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();

                //BEGIN add comment header
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                EditorGUILayout.LabelField("Add comment", EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                //END add comment header

                //BEGIN add comment
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                newComment = EditorGUILayout.TextArea(newComment, GUILayout.Height(100));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                if (GUILayout.Button("Add")) {
                    EditorUtility.DisplayProgressBar("Easy Jira", "Sending comment", 5);
                    this.StartCoroutine(JiraConnect.APIAddComment(newComment, issueKey, EJ_Settings.asset, AddCommentCallback));
                }
                EditorGUILayout.EndHorizontal();
                //END add comment

                //BEGIN History header
                EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                m_ShowHistory.target = EditorGUILayout.ToggleLeft("Show History", m_ShowHistory.target, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();
                //END History header

                //BEGIN History
                if (EditorGUILayout.BeginFadeGroup(m_ShowHistory.faded)) {
                    EditorGUI.indentLevel++;
                    if (issue.changelogHistories.Count > 0) {
                        foreach (JIssueChangelog change in issue.changelogHistories) {
                            //BEGIN History block
                            EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                            EditorGUILayout.BeginVertical("BOX");
                            //GUILayout.Label(change.authorAvatarTexture, GUILayout.Width(40), GUILayout.Height(40));
                            EditorGUILayout.LabelField(change.authorDisplayName);
                            EditorGUILayout.LabelField(change.created);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.BeginVertical();
                            EditorGUILayout.LabelField("Changed: " + change.field);
                            EditorGUILayout.LabelField("From: " + change.fromValue);
                            EditorGUILayout.LabelField("To: " + change.toValue);
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                            //END History block
                        }
                    } else {
                        EditorGUILayout.BeginHorizontal("BOX", GUILayout.MaxWidth(win));
                        EditorGUILayout.LabelField("No history");
                        EditorGUILayout.EndHorizontal();
                    }
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
                //END History

                // END Details

                EditorGUILayout.Space();
                // Close the scroll view
                EditorGUILayout.EndScrollView();
            }            
        }        

        /******* Callbacks  *******/

        void DownloadIssueCallback(String response) {
            issue = new JIssueDetails();
            issue.getIssueFromJsonString(response, null);
            if(issue.projectIconURL.Length > 0) {
                this.StartCoroutine(JiraConnect.GetTexture(issue.projectIconURL.EndsWith(".png") ? issue.projectIconURL : issue.projectIconURL + "&format=png", value => {
                    issue.projectIconTexture = value;
                    TextureScale.Bilinear(issue.projectIconTexture, issue.projectIconTexture.width * 2, issue.projectIconTexture.height * 2);
                }));
            }
            if(issue.typeIconURL.Length > 0) {
                this.StartCoroutine(JiraConnect.GetTexture(issue.typeIconURL.EndsWith(".png") ? issue.typeIconURL : issue.typeIconURL + "&format=png", value => issue.issueTypeIconTexture = value));
            }
            if (issue.priorityIconURL.Length > 0) {
                this.StartCoroutine(JiraConnect.GetTexture(issue.priorityIconURL.EndsWith(".png") ? issue.priorityIconURL : issue.priorityIconURL + "&format=png", value => issue.priorityIconTexture = value));
            }
            if (issue.statusIconURL.Length > 0) {
                this.StartCoroutine(JiraConnect.GetTexture(issue.statusIconURL.EndsWith(".png") ? issue.statusIconURL : issue.statusIconURL + "&format=png", value => issue.statusIconTexture = value));
            }
            foreach (JIssueComment comment in issue.comments) {
                this.StartCoroutine(JiraConnect.GetTexture(comment.authorAvatar, value => comment.authorAvatarTexture = value));
            }
            EditorUtility.ClearProgressBar();
        }

        void AssignIssueCallback(String response) {
            if (response.StartsWith("error")) {
                Debug.LogError(response);
            } else {
                this.ShowNotification(new GUIContent("Issue assigned."));
                this.Init(issueKey);
            }            
        }

        void AddCommentCallback(String response) {
            newComment = "";
            //this.ShowNotification(new GUIContent("Comment added."));
            this.Init(issueKey);
        }

        /******* END Callbacks  *******/

        public void OnInspectorUpdate() {
            this.Repaint();
        }

        float getSizeByPercentage(float percent) {
            return (win * percent) / 100;
        }
    }
}