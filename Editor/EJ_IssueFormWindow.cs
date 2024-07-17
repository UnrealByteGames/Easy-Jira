using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using UnityEditor.IMGUI.Controls;

namespace UnrealByte.EasyJira {
	public class EJ_IssueFormWindow : EditorWindow {
		float win = Screen.width;
		List<JIssueType> issueTypes;
		List<JIssuePriority> issuePriorities;
		public List<JUser> users;
		public string summary = "";
		public string description = "";
		public JUser juser;
		public int issueType = 0;
		public int priority = 0;

		[SerializeField]
		AutocompleteSearchField autocompleteSearchField;

		void Awake() {
			if (EJ_Settings.asset == null) {
				EJ_Settings.Awake();
			}

			this.StartCoroutine(JiraConnect.APIDownloadProjectIssueTypes(EJ_Settings.asset, EJ_Settings.asset.jiraProjectKey, false, value => issueTypes = value));
			this.StartCoroutine(JiraConnect.APIDownloadPriorities(EJ_Settings.asset, OnPrioritiesDownloaded));
			this.StartCoroutine(JiraConnect.APIDownloadUsersAssignable(EJ_Settings.asset, "", EJ_Settings.asset.jiraProjectKey, OnUsersDownloaded));
		}

		private void OnPrioritiesDownloaded(List<JIssuePriority> priorities) {
			issuePriorities = priorities;
			priority = issuePriorities.Count / 2;
		}

		private void OnUsersDownloaded(List<JUser> users) {
			this.users = users;
			foreach (var user in users)
				autocompleteSearchField.AddResult(user.displayName);
		}

		void OnEnable() {
			if (autocompleteSearchField == null) autocompleteSearchField = new AutocompleteSearchField();
			autocompleteSearchField.onInputChanged = OnAssignedInputChanged;
			autocompleteSearchField.onConfirm = OnConfirm;
			autocompleteSearchField.searchString = "";
			this.Awake();
		}

		private void OnConfirm(string obj) {
			autocompleteSearchField.ClearResults();
		}

		void OnAssignedInputChanged(string searchString) {
			autocompleteSearchField.ClearResults();
			if (string.IsNullOrEmpty(searchString) || users == null || users.Exists(x => x.displayName.Equals(searchString)))
				return;
			foreach (var user in users) {
				if (user.displayName.ToLower().StartsWith(searchString.ToLower()))
					autocompleteSearchField.AddResult(user.displayName);
			}
		}

		/// <summary>
		/// Initialization function.
		/// Runs when the user select "new issue" an issue from administration.
		/// </summary>
		public void Init() {
			this.Awake();
		}

		void OnGUI() {
			string[] issueTypesList = { };
			if (issueTypes != null) {
				List<string> aux = new List<string>();
				foreach (JIssueType it in issueTypes) {
					aux.Add(it.name);
				}

				issueTypesList = aux.ToArray();
			}

			string[] issuePrioritiesList = { };
			if (issuePriorities != null) {
				List<string> aux2 = new List<string>();
				foreach (JIssuePriority ip in issuePriorities) {
					aux2.Add(ip.name);
				}

				issuePrioritiesList = aux2.ToArray();
			}

			win = Screen.width;
			EditorStyles.boldLabel.wordWrap = true;
			EditorStyles.label.wordWrap = true;
			EditorStyles.boldLabel.alignment = TextAnchor.MiddleLeft;
			EditorStyles.label.alignment = TextAnchor.MiddleLeft;
			EditorStyles.textField.wordWrap = true;

			if (issueTypes == null || issuePriorities == null) {
				EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(win));
				EditorGUILayout.LabelField("Loading...", EditorStyles.boldLabel);
				EditorGUILayout.EndVertical();
				return;
			}

			EditorGUILayout.BeginVertical("BOX", GUILayout.MaxWidth(win));
			EditorGUILayout.LabelField("Issue Type", EditorStyles.boldLabel);
			issueType = EditorGUILayout.Popup(issueType, issueTypesList);
			EditorGUILayout.LabelField("Summary", EditorStyles.boldLabel);
			summary = EditorGUILayout.TextField(summary);
			EditorGUILayout.LabelField("Description", EditorStyles.boldLabel);
			description = EditorGUILayout.TextArea(description, GUILayout.Height(100));
			EditorGUILayout.LabelField("Priority", EditorStyles.boldLabel);
			priority = EditorGUILayout.Popup(priority, issuePrioritiesList);

			AutocompleteSearchField searchField = new AutocompleteSearchField();
			EditorGUILayout.LabelField("User", EditorStyles.boldLabel);


			autocompleteSearchField.OnGUI();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Create")) {
				try {
					EditorUtility.DisplayProgressBar("Easy Jira", "Sending issue", 5);
					JIssue issue = new JIssue(summary, description);
					foreach (JIssueType it in issueTypes) {
						if (it.name == issueTypesList[issueType]) {
							issue.issuetype = new JIssueType(it.id);
							break;
						}
					}

					foreach (JIssuePriority ip in issuePriorities) {
						if (ip.name == issuePriorities[priority].name) {
							issue.priority = new JIssuePriority(ip.id);
							break;
						}
					}

					foreach (JUser user in users)
						if (user.displayName == autocompleteSearchField.searchString) {
							issue.assignee = user;
							break;
						}


					this.StartCoroutine(JiraConnect.APICreateIssue(EJ_Settings.asset, issue, value => {
						EditorUtility.ClearProgressBar();
						if (value.StartsWith("error")) {
							Debug.LogError(value);
						} else {
							this.ShowNotification(new GUIContent("Issue created."));
						}
					}));
				}
				catch (Exception e) {
					Debug.LogError(e.Message);
				}
			}

			EditorGUILayout.Space();
			EditorGUILayout.EndVertical();
		}

		public void OnInspectorUpdate() {
			this.Repaint();
		}
	}
}