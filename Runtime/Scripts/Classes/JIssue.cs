using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace UnrealByte.EasyJira {

    [System.Serializable]
    public class JIssue {
        // Dont change this variable names.
        public JProject project;
        public string summary = "";
        public string description = "";
        public JUser assignee = new JUser();
        public JIssueType issuetype;
        public JIssuePriority priority;

        public JIssue(string summary, string description) {
            this.summary = summary;
            this.description = description;
        }
    }

    [System.Serializable]
    public class JProject {
        public string key = "";

        public JProject(string key) {
            this.key = key;
        }
    }

    [System.Serializable]
    public class JIssueType {
        public int id;
        [System.NonSerialized]
        public string name;

        public JIssueType() { }

        public JIssueType(int id) {
            this.id = id;
        }
    }

    [System.Serializable]
    public class JIssuePriority {
        public string id;
        [System.NonSerialized]
        public string name;

        public JIssuePriority() { }

        public JIssuePriority(string id) {
            this.id = id;
        }
    }
}