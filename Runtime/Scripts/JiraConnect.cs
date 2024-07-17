using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using LitJson;

namespace UnrealByte.EasyJira { 
    
    public class JiraConnect {

        public List<JIssueType> jIssueTypes;
        public static Action<String> callbackAction;

        public JiraConnect() { }

#pragma warning disable 0168

        //IN GAME *****************************************************************************************************************

        /// <summary>
        /// Send the issue, after that, send the log and screenshot to attach.
        /// </summary>
        /// <param name="jiraIssueURL"></param>
        /// <param name="encodedCredentials"></param>
        /// <param name="jProject"></param>
        /// <param name="issue"></param>
        /// <param name="logFilePath"></param>
        /// <param name="takeScreenshot"></param>
        /// <returns></returns>
        public IEnumerator APISendIssueIG(JiraSettings settings, JIssue issue, string logFilePath, bool takeScreenshot) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string jiraIssueURL = settings.getJiraIssueURL();                      
            
            issue.project = new JProject(settings.jiraProjectKey);            
            JsonData jd = new JsonData("{\"fields\":" + JsonUtility.ToJson(issue).ToString() + "}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jd.ToString());

            var request = new UnityWebRequest(jiraIssueURL, "POST");
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) {
                Debug.Log("[EasyJira] " + request.responseCode + " - " + request.downloadHandler.text);
            } else {
                Debug.Log("[EasyJira] Feedback post complete!");
                
                JsonData data = JsonMapper.ToObject(request.downloadHandler.text);
                string key = data["key"].ToString();
                if (takeScreenshot) {
                    byte[] screenshotData = System.IO.File.ReadAllBytes(Application.persistentDataPath + "/screenshot.png");
                    WWWForm form = new WWWForm();
                    form.AddBinaryData("file", screenshotData, "screenshot.png", "image/png");
                    UnityWebRequest requestAttach = UnityWebRequest.Post(jiraIssueURL + "/" + key + "/attachments", form);
                    requestAttach.SetRequestHeader("X-Atlassian-Token", "no-check");
                    requestAttach.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
                    requestAttach.SendWebRequest();
                    if (requestAttach.isNetworkError || requestAttach.isHttpError) {
                        Debug.Log("[EasyJira] " + requestAttach.responseCode + " - " + requestAttach.downloadHandler.text);
                    } else {
                        Debug.Log("[EasyJira] Screenshot Attach complete!");
                    }
                }                
                if (logFilePath != null) {
                    Debug.Log(logFilePath);
                    byte[] logData = System.IO.File.ReadAllBytes(logFilePath);
                    string logName = "EJLog.txt";
                    WWWForm form = new WWWForm();
                    form.AddBinaryData("file", logData, logName);
                    UnityWebRequest requestAttach = UnityWebRequest.Post(jiraIssueURL + "/" + key + "/attachments", form);
                    requestAttach.SetRequestHeader("X-Atlassian-Token", "no-check");
                    requestAttach.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
                    requestAttach.SendWebRequest();
                    if (requestAttach.isNetworkError || requestAttach.isHttpError) {
                        Debug.Log("[EasyJira] " + requestAttach.responseCode + " - " + requestAttach.downloadHandler.text);
                    } else {
                        Debug.Log("[EasyJira] LogFile Attach complete!");
                    }
                }                
            }
        }

        /// <summary>
        /// Download the issue types availables for the selected project
        /// </summary>
        /// <param name="url"></param>
        /// <param name="encodedCredentials"></param>
        /// <param name="projectKey"></param>
        /// <param name="subTask"></param>
        /// <returns></returns>
        public IEnumerator APIDownloadProjectIssueTypesIG(JiraSettings settings, string projectKey, bool subTask) {

            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string jiraProjectURL = settings.getJiraProjectURL() + "/" + projectKey;
            string responseData = "";
            var request = new UnityWebRequest(jiraProjectURL, "GET");
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) {
                Debug.Log("[EasyJira] " + request.responseCode + " - " + request.downloadHandler.text);
            } else {
                responseData = request.downloadHandler.text;
                JsonData data = JsonMapper.ToObject(responseData);
                if (data["issueTypes"].Count > 0) {
                    jIssueTypes = new List<JIssueType>();
                    for (int i = 0; i < data["issueTypes"].Count; i++) {
                        if(!subTask)
                            try {
                                if (bool.Parse(data["issueTypes"][i]["subtask"].ToString())) {
                                    continue;
                                }
                            } catch (Exception e) {
                                Debug.Log(e.Message);
                            }
                        JIssueType it = new JIssueType();
                        it.name = data["issueTypes"][i]["name"].ToString();
                        it.id = int.Parse(data["issueTypes"][i]["id"].ToString());

                        this.jIssueTypes.Add(it);
                    }
                }
            }                      
        }

        //EDITOR *******************************************************************************************************************

        /// <summary>
        /// Retrieve the current user information, to validate the configuration.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IEnumerator APITestAuth(JiraSettings settings, Action<String> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraMyselfURL();

            UnityWebRequest www = new UnityWebRequest(url, "GET");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            yield return www.SendWebRequest();
            if (www.isNetworkError || www.isHttpError) {
                Debug.LogError("[EasyJira] " + www.error);
            } else {                
                JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                if (data != null) {
                    settings.jiraName = data["displayName"].ToString();
                    settings.jiraAccountId = data["accountId"].ToString();
                }
                www = new UnityWebRequest(settings.getJiraMyPermissionsURL() + "?permissions=ASSIGN_ISSUES", "GET");
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Accept", "application/json");

                yield return www.SendWebRequest();
                if (www.isNetworkError || www.isHttpError) {
                    Debug.LogError("[EasyJira] " + www.error);
                } else {
                    try {
                        data = JsonMapper.ToObject(www.downloadHandler.text);
                        if (data != null) {
                            settings.assignIssues = Boolean.Parse(data["permissions"]["ASSIGN_ISSUES"]["havePermission"].ToString());
                        }
                    } catch(Exception e) {
                        Debug.LogError("[EasyJira] " + www.error);
                    }                    
                }
                callback("[EasyJira] Success! Settings saved");
            }
        }

        /// <summary>
        /// Create the issue.
        /// </summary>
        /// <param name="jiraIssueURL"></param>
        /// <param name="encodedCredentials"></param>
        /// <param name="jProject"></param>
        /// <param name="issue"></param>
        /// <param name="logFilePath"></param>
        /// <param name="takeScreenshot"></param>
        /// <returns></returns>
        public static IEnumerator APICreateIssue(JiraSettings settings, JIssue issue, Action<String> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraIssueURL();

            issue.project = new JProject(settings.jiraProjectKey);
            JsonData jd = new JsonData("{\"fields\":" + JsonUtility.ToJson(issue).ToString() + "}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jd.ToString());

            var www = new UnityWebRequest(url, "POST");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            try {
                if (!www.isNetworkError && !www.isHttpError) {
                    callback(www.downloadHandler.text);
                } else {
                    callback("error: " + www.downloadHandler.text);
                }
            } catch (Exception e) { }
            yield return new WaitForSeconds(2f);
        }
        

        /// <summary>
        /// Download the issue types availables for the selected project
        /// Note: Download all project fields and iterates searching the issue types object.
        /// </summary> 
        /// <param name="url"></param>
        /// <param name="encodedCredentials"></param>
        /// <param name="projectKey"></param>
        /// <param name="subTask"></param>
        /// <returns>List<JIssueType></returns>
        public static IEnumerator APIDownloadProjectIssueTypes(JiraSettings settings, string projectKey, bool subTask, Action<List<JIssueType>> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraProjectURL() + "/" + projectKey;
            List<JIssueType> issueTypes = new List<JIssueType>();

            UnityWebRequest www = new UnityWebRequest(url, "GET");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            try {
                JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                if (data["issueTypes"].Count > 0) {                    
                    for (int i = 0; i < data["issueTypes"].Count; i++) {
                        if (!subTask)
                            try {
                                if (bool.Parse(data["issueTypes"][i]["subtask"].ToString())) {
                                    continue;
                                }
                            } catch (Exception e) {}
                        JIssueType it = new JIssueType();
                        it.name = data["issueTypes"][i]["name"].ToString();
                        it.id = int.Parse(data["issueTypes"][i]["id"].ToString());
                        issueTypes.Add(it);
                    }
                }
                callback(issueTypes);
            } catch (Exception e) { }
            yield return new WaitForSeconds(2f);            
        }

        /// <summary>
        /// Download the priorities availables for the selected project
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static IEnumerator APIDownloadPriorities(JiraSettings settings, Action<List<JIssuePriority>> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraPrioritiesURL();
            List<JIssuePriority> priorities = new List<JIssuePriority>();

            UnityWebRequest www = new UnityWebRequest(url, "GET");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            try {
                JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                if (data.Count > 0) {
                    for (int i = 0; i < data.Count; i++) {
                        JIssuePriority it = new JIssuePriority();
                        it.name = data[i]["name"].ToString();
                        it.id = data[i]["id"].ToString();
                        priorities.Add(it);
                    }
                }
                callback(priorities);
            } catch (Exception e) { }
            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// Find users assignable to issues
        /// Returns a list of users that can be assigned to an issue
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="query"></param>
        /// <param name="issueKey"></param>
        /// <param name="callback"></param>
        public static IEnumerator APIDownloadUsersAssignableToIssues(JiraSettings settings, string query, string issueKey, Action<List<JUser>> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraUserAssignableIssuesURL() + "?query="+query+"&issueKey="+issueKey;

            var www = new UnityWebRequest(url, "GET");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            yield return www.SendWebRequest();
            try {                
                if (!www.isNetworkError && !www.isHttpError) {
                    List<JUser> users = new List<JUser>();
                    JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                    if (data.Count > 0) {                        
                        for (int i = 0; i < data.Count; i++) {
                            JUser user = new JUser();
                            user.accountId = data[i]["accountId"].ToString();
                            user.emailAddress = data[i]["emailAddress"].ToString();
                            user.displayName = data[i]["displayName"].ToString();
                            user.avatarURL = data[i]["avatarUrls"]["24x24"].ToString();
                            users.Add(user);
                        }
                    }
                    callback(users);
                } else {
                    Debug.LogError("error: " + www.downloadHandler.text);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
            yield return new WaitForSeconds(2f);
        }
        
        
        /// <summary>
        /// Find users assignable to Project
        /// Returns a list of users that can be assigned to an issue in a Project
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="query"></param>
        /// <param name="issueKey"></param>
        /// <param name="callback"></param>
        public static IEnumerator APIDownloadUsersAssignable(JiraSettings settings, string query, string projectKey, Action<List<JUser>> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraUserAssignableProjectURL() + "?query="+query+"&projectKeys="+projectKey;

            var www = new UnityWebRequest(url, "GET");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");
            www.SetRequestHeader("Accept", "application/json");

            yield return www.SendWebRequest();
            try {                
                if (!www.isNetworkError && !www.isHttpError) {
                    List<JUser> users = new List<JUser>();
                    JsonData data = JsonMapper.ToObject(www.downloadHandler.text);
                    if (data.Count > 0) {                        
                        for (int i = 0; i < data.Count; i++) {
                            JUser user = new JUser();
                            user.accountId = data[i]["accountId"].ToString();
                            user.emailAddress = data[i]["emailAddress"].ToString();
                            user.displayName = data[i]["displayName"].ToString();
                            user.avatarURL = data[i]["avatarUrls"]["24x24"].ToString();
                            users.Add(user);
                        }
                    }
                    callback(users);
                } else {
                    Debug.LogError("error: " + www.downloadHandler.text);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
            yield return new WaitForSeconds(2f);
        }
        

        /// <summary>
        /// Download an issue from given jIssueKey (issue key)
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="jIssueKey"></param>
        /// <param name="callback"></param>
        public static IEnumerator APIGetIssue(JiraSettings settings, string jIssueKey, Action<String> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraIssueURL() + "/" + jIssueKey + "?expand=operations,changelog";

            UnityWebRequest www = new UnityWebRequest(url, "GET");
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            try {
                callback(www.downloadHandler.text);
            } catch (Exception e) { }
            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// Creates and send a JQL to get all the filtered issues.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="startAt"></param>
        /// <param name="maxResults"></param>
        /// <param name="callback"></param>
        public static IEnumerator APISearchIssues(JiraSettings settings, int startAt, int maxResults, Action<String> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            JsonData jd = new JsonData("{\"jql\":\"project = " + settings.jiraProjectKey + "\", \"startAt\":" + startAt + ",\"maxResults\":" + maxResults + ", \"fields\":[\"id\",\"key\",\"summary\",\"issuetype\",\"status\",\"assignee\",\"priority\",\"duedate\"]}");//
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jd.ToString());

            UnityWebRequest www = new UnityWebRequest(settings.getJiraSearchURL(), "POST");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            try {
                if(!www.isNetworkError && !www.isHttpError) {
                    callback(www.downloadHandler.text);
                } else {
                    callback("error: " + www.downloadHandler.text);
                }                    
            } catch (Exception e) { }
            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// Assigns the issue to the user.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="jIssueKey"></param>
        /// <param name="callback"></param>
        public static IEnumerator APIAssignIssue(JiraSettings settings, string issueKey, string accountId, Action<String> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraIssueURL() + "/" + issueKey;

            JsonData jd = new JsonData("{\"fields\": {\"assignee\":{\"accountId\":\"" + accountId + "\"}}}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jd.ToString());

            UnityWebRequest www = new UnityWebRequest(url, "PUT");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            try {
                if (!www.isNetworkError && !www.isHttpError) {
                    callback(www.downloadHandler.text);
                } else {
                    callback("error: " + www.downloadHandler.text);
                }
            } catch (Exception e) { }
            yield return new WaitForSeconds(2f);
        }

        /// <summary>
        /// Returns a user data.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="callback"></param>
        public static IEnumerator APIAddComment(string comment, string IssueKey, JiraSettings settings, Action<String> callback) {
            string encodedCredentials = JiraConnect.GenerateBasicAuth(settings.jiraUser, settings.jiraToken);
            string url = settings.getJiraIssueURL();
            url = url + "/" + IssueKey + "/comment";

            JsonData jd = new JsonData("{\"body\":\"" + comment + "\"}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jd.ToString());

            UnityWebRequest www = new UnityWebRequest(url, "POST");
            www.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            www.SetRequestHeader("Authorization", "Basic " + encodedCredentials);
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();
            try {
                callback(www.downloadHandler.text);
            } catch (Exception e) { }
            yield return new WaitForSeconds(2f);
        }
        
        //UTILS

        /// <summary>
        /// Encode with base64 the username and password for basic authentication.
        /// </summary>
        /// <param name="jiraUser"></param>
        /// <param name="jiraPassword"></param>
        /// <returns></returns>
        public static string GenerateBasicAuth(string jiraUser, string jiraPassword) {
            string mergedCredentials = string.Format("{0}:{1}", jiraUser, jiraPassword);
            byte[] bytesToEncode = Encoding.UTF8.GetBytes(mergedCredentials);
            string encodedText = Convert.ToBase64String(bytesToEncode);
            return encodedText;
        }

        /// <summary>
        /// Download an image from the server. 
        /// Only supports PNG, JPG.
        /// SVG not supported.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static IEnumerator GetTexture(string url, Action<Texture2D> result) {
            UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
            yield return www.SendWebRequest();
            try {
                result(DownloadHandlerTexture.GetContent(www));
            } catch (Exception e) {}
            yield return new WaitForSeconds(2f);
        }

#pragma warning restore 0168
        
    }
}


