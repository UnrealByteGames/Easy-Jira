using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnrealByte.EasyJira {

    [System.Serializable]
    public class JiraSettings : ScriptableObject {

        //[HideInInspector]
        public string jiraBaseRestURL = "https://EXAMPLE.atlassian.net";
        [HideInInspector]
        public string jiraMyselfURL = "/rest/api/2/myself";
        [HideInInspector]
        public string jiraUserURL = "/rest/api/2/user";
        [HideInInspector]
        public string jiraStatusURL = "/rest/api/2/status";
        [HideInInspector]
        public string jiraProjectURL = "/rest/api/2/project";
        [HideInInspector]
        public string jiraIssueURL = "/rest/api/2/issue";
        [HideInInspector]
        public string jiraIssueTypeURL = "/rest/api/2/issuetype";
        [HideInInspector]
        public string jiraPrioritiesURL = "/rest/api/2/priority";
        [HideInInspector]
        public string jiraSearchURL = "/rest/api/2/search";
        [HideInInspector]
        public string jiraAccountId = "account-id";
        [HideInInspector]
        public string jiraName = "name";
        [HideInInspector]
        public string jiraUser = "username";
        [HideInInspector]
        public string jiraToken = "api-token";
        [HideInInspector]
        public string jiraProjectKey = "";
        public int maxResults = 50;
        public int maxShowAttachments = 5;
        public bool showAttachAtInit = false;
        public bool showCommentsAtInit = false;
        public bool showHistoryAtInit = false;
        [HideInInspector]
        public bool assignIssues = false;

        public JiraSettings Init(string jiraBaseRestURL, string jiraUser, string jiraToken, string jiraProjectKey) {
            this.jiraBaseRestURL = jiraBaseRestURL;
            this.jiraUser = jiraUser;
            this.jiraToken = jiraToken;
            this.jiraProjectKey = jiraProjectKey;
            return this;
        }

        public JiraSettings Init(string jiraBaseRestURL, string jiraUser, string jiraToken, string jiraProjectKey, int maxResults, bool showAttach, bool showComments, bool showHistory) {
            this.jiraBaseRestURL = jiraBaseRestURL;
            this.jiraUser = jiraUser;
            this.jiraToken = jiraToken;
            this.jiraProjectKey = jiraProjectKey;
            this.maxResults = maxResults;
            this.showAttachAtInit = showAttach;
            this.showCommentsAtInit = showComments;
            this.showHistoryAtInit = showHistory;
            return this;
        }

        /// <summary>
        /// Remove the ending "/" character to standardize the urls usage.
        /// </summary>
        /// <param name="asset"></param>
        public void sanitizeBaseURL() {
            if (this.jiraBaseRestURL.EndsWith("/"))
                this.jiraBaseRestURL = this.jiraBaseRestURL.Substring(0, this.jiraBaseRestURL.Length - 1);
        }

        /// <summary>
        /// Compose and returns the jira myself url.
        /// </summary>
        /// <returns></returns>
        public string getJiraMyselfURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraMyselfURL;
        }

        /// <summary>
        /// Compose and returns the jira user url.
        /// </summary>
        /// <returns></returns>
        public string getJiraUserURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraUserURL;
        }

        /// <summary>
        /// Compose and returns the jira status url.
        /// </summary>
        /// <returns></returns>
        public string getJiraStatusURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraStatusURL;
        }

        /// <summary>
        /// Compose and returns the jira project url.
        /// </summary>
        /// <returns></returns>
        public string getJiraProjectURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraProjectURL;
        }

        /// <summary>
        /// Compose and returns the jira issue url.
        /// </summary>
        /// <returns></returns>
        public string getJiraIssueURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraIssueURL;
        }

        /// <summary>
        /// Compose and returns the jira issuetype url.
        /// </summary>
        /// <returns></returns>
        public string getJiraIssueTypeURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraIssueTypeURL;
        }

        /// <summary>
        /// Compose and returns the jira priorities url.
        /// </summary>
        /// <returns></returns>
        public string getJiraPrioritiesURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraPrioritiesURL;
        }

        /// <summary>
        /// Compose and returns the jira search url.
        /// </summary>
        /// <returns></returns>
        public string getJiraSearchURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraSearchURL;
        }

        public string getJiraUserAssignableIssuesURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraUserURL + "/assignable/search";
        }
        
        public string getJiraUserAssignableProjectURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + this.jiraUserURL + "/assignable/multiProjectSearch";
        }

        public string getJiraMyPermissionsURL() {
            sanitizeBaseURL();
            return this.jiraBaseRestURL + "/rest/api/2/mypermissions";
        }
    }
}