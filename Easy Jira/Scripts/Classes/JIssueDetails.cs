using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LitJson;

namespace UnrealByte.EasyJira {
#pragma warning disable 0168
    public class JIssueDetails {

        public string projectKey = "";
        public string projectTypeKey = "";
        public string projectName = "";
        public string projectIconURL = "";
        public Texture2D projectIconTexture;
        public string key = "";
        public string summary = "";
        public string type = "";
        public string typeIconURL = "";
        public Texture2D issueTypeIconTexture;
        public string priority = "";
        public string priorityIconURL = "";
        public Texture2D priorityIconTexture;
        public string affectVersions = "";
        public string components = "";
        public string labels = "";
        public string status = "";
        public string statusIconURL = "";
        public Texture2D statusIconTexture;
        public string resolution = "";
        public string fixVersions = "";
        public string assignee = "";
        public string reporter = "";
        public string votes = "";
        public string watcher = "";
        public string dateCreated = "";
        public string dateUpdated = "";
        public string description = "";        
        public List<JIssueComment> comments = new List<JIssueComment>();
        public List<JIssueAttachment> attachments = new List<JIssueAttachment>();
        public List<JIssueChangelog> changelogHistories = new List<JIssueChangelog>();

        public JIssueDetails getIssueFromJsonString(string JSONString, JsonData JSONData) {
            JsonData data = null;
            if (JSONString.Length > 0) {
                data = JsonMapper.ToObject(JSONString);
            } else {
                data = JSONData;
            }            
            try {
                try {
                    this.projectKey = data["fields"]["project"]["key"].ToString();
                } catch (Exception e) {
                    this.projectKey = "---";
                }
                try {
                    this.projectTypeKey = data["fields"]["project"]["projectTypeKey"].ToString();
                } catch (Exception e) {
                    this.projectTypeKey = "---";
                }
                try {
                    this.projectName = data["fields"]["project"]["name"].ToString();
                } catch (Exception e) {
                    this.projectName = "---";
                }
                try {
                    this.projectIconURL = data["fields"]["project"]["avatarUrls"]["16x16"].ToString();
                } catch (Exception e) {
                    this.projectIconURL = "";
                }
                try {
                    this.key = data["key"].ToString();
                } catch (Exception e) {
                    this.key = "---";
                }
                try {
                    this.summary = data["fields"]["summary"].ToString();
                } catch (Exception e) {
                    this.summary = "---";
                }
                try {
                    this.type = data["fields"]["issuetype"]["name"].ToString();
                } catch (Exception e) {
                    this.type = "---";
                }
                try {
                    string issueTypeIconAux = data["fields"]["issuetype"]["iconUrl"].ToString();
                    if (issueTypeIconAux.EndsWith(".svg")) {
                        issueTypeIconAux = issueTypeIconAux.Replace(".svg", ".png");
                    }
                    this.typeIconURL = issueTypeIconAux;
                } catch (Exception e) {
                    this.typeIconURL = "";
                }
                try {
                    this.priority = data["fields"]["priority"]["name"].ToString();
                } catch (Exception e) {
                    this.priority = "---";
                }
                try {
                    string priorityIconAux = data["fields"]["priority"]["iconUrl"].ToString();
                    if (priorityIconAux.EndsWith(".svg")) {
                        priorityIconAux = priorityIconAux.Replace(".svg", ".png");
                    }
                    this.priorityIconURL = priorityIconAux;
                } catch (Exception e) {
                    this.priorityIconURL = "";
                }
                try {
                    if (data["fields"]["components"].Count > 0) {
                        for (int i = 0; i < data["fields"]["components"].Count; i++) {
                            try {
                                this.components += data["fields"]["components"][i]["name"].ToString() + " ";
                            } catch (Exception e) { }
                        }
                    }
                } catch (Exception e) { }
                try {
                    if (data["fields"]["labels"].Count > 0) {
                        for (int i = 0; i < data["fields"]["labels"].Count; i++) {
                            try {
                                this.labels += data["fields"]["labels"][i].ToString() + " ";
                            } catch (Exception e) { }
                        }
                    }
                } catch (Exception e) { }
                try {
                    this.status = data["fields"]["status"]["name"].ToString();
                } catch (Exception e) {
                    this.status = "---";
                }
                try {
                    string statusIconAux = data["fields"]["status"]["iconUrl"].ToString();
                    if (statusIconAux.EndsWith(".svg")) {
                        statusIconAux = statusIconAux.Replace(".svg", ".png");
                    }
                    this.statusIconURL = statusIconAux;
                } catch (Exception e) {
                    this.statusIconURL = "";
                }
                try {
                    this.resolution = DateTime.Parse(data["fields"]["resolutiondate"].ToString()).ToShortDateString();
                } catch (Exception e) {
                    this.resolution = "---";
                }
                try {
                    if (data["fields"]["versions"].Count > 0) {
                        for (int i = 0; i < data["fields"]["versions"].Count; i++) {
                            try {
                                this.affectVersions += data["fields"]["versions"][i]["name"].ToString() + " ";
                            } catch (Exception e) { }
                        }
                    }
                } catch (Exception e) { }
                try {
                    if (data["fields"]["fixVersions"].Count > 0) {
                        for (int i = 0; i < data["fields"]["fixVersions"].Count; i++) {
                            try {
                                this.fixVersions += data["fields"]["fixVersions"][i]["name"].ToString() + " ";
                            } catch (Exception e) { }
                        }
                    }
                } catch (Exception e) { }
                try {
                    this.assignee = data["fields"]["assignee"]["displayName"].ToString();
                } catch (Exception e) {
                    this.assignee = "---";
                }
                try {
                    this.reporter = data["fields"]["reporter"]["name"].ToString();
                } catch (Exception e) {
                    this.reporter = "---";
                }
                try {
                    this.votes = data["fields"]["votes"]["votes"].ToString();
                } catch (Exception e) {
                    this.votes = "---";
                }
                try {
                    this.dateCreated = DateTime.Parse(data["fields"]["created"].ToString()).ToShortDateString();
                } catch (Exception e) {
                    this.dateCreated = "---";
                }
                try {
                    this.dateUpdated = DateTime.Parse(data["fields"]["updated"].ToString()).ToShortDateString();
                } catch (Exception e) {
                    this.dateUpdated = "---";
                }
                try {
                    this.description = data["fields"]["description"].ToString();
                } catch (Exception e) {
                    this.description = "(no description)";
                }
                try {
                    if (data["fields"]["comment"].Count > 0) {
                        if (data["fields"]["comment"]["comments"].Count > 0) {
                            for (int i = 0; i < data["fields"]["comment"]["comments"].Count; i++) {
                                JIssueComment comment = new JIssueComment();
                                try {
                                    comment.getIssueCommentFromJsonString(data["fields"]["comment"]["comments"][i].ToJson());
                                    if (comment != null) {
                                        comments.Add(comment);
                                    }
                                } catch (Exception e) { }
                            }
                        }
                    }
                } catch (Exception e) { }
                try {
                    if (data["fields"]["attachment"].Count > 0) {
                        for (int i = 0; i < data["fields"]["attachment"].Count; i++) {
                            JIssueAttachment attachment = new JIssueAttachment();
                            try {
                                attachment.getIssueAttachmentFromJsonString(data["fields"]["attachment"][i].ToJson());
                                if (attachment != null) {
                                    attachments.Add(attachment);
                                }
                            } catch (Exception e) { }
                        }
                    }
                } catch (Exception e) { }
                try {
                    if (data["changelog"]["histories"].Count > 0) {
                        for (int i = 0; i < data["changelog"]["histories"].Count; i++) {
                            JIssueChangelog changelog = new JIssueChangelog();
                            try {
                                changelog.getChangeLogFromJsonString(data["changelog"]["histories"][i].ToJson());
                                if (changelog != null) {
                                    changelogHistories.Add(changelog);
                                }
                            } catch (Exception e) { }
                        }
                    }
                } catch (Exception e) { }
            } catch (Exception e) { 
                Debug.Log(e);
            }
            return this;
        }
    }

    /// <summary>
    /// This class represents a comment object inside an issue
    /// </summary>
    public class JIssueComment {

        public string created = "";
        public string body = "";
        public string authorName = "";
        public string authorEmail = "";        
        public string authorDisplayName = "";
        public string authorAvatar = "";
        public Texture2D authorAvatarTexture = new Texture2D(30, 30);

        public JIssueComment getIssueCommentFromJsonString(string JSONString) {
            JsonData data = JsonMapper.ToObject(JSONString);
            try {
                try {
                    this.created = DateTime.Parse(data["created"].ToString()).ToShortDateString();
                } catch (Exception e) {
                    this.created = "---";
                }
                try {
                    this.body = data["body"].ToString();
                } catch (Exception e) {
                    this.body = "---";
                }
                try {
                    this.authorName = data["author"]["name"].ToString();
                } catch (Exception e) {
                    this.authorName = "---";
                }
                try {
                    this.authorEmail = data["author"]["emailAddress"].ToString();
                } catch (Exception e) {
                    this.authorEmail = "---";
                }
                try {
                    this.authorAvatar = data["author"]["avatarUrls"]["48x48"].ToString();
                } catch (Exception e) {
                    this.authorAvatar = "---";
                }
                try {
                    this.authorDisplayName = data["author"]["displayName"].ToString();
                } catch (Exception e) {
                    this.authorDisplayName = "---";
                }
            } catch (Exception e) { }
            return this;
        }
    }

    /// <summary>
    /// This class represents an attachment file object inside an issue.
    /// </summary>
    public class JIssueAttachment {

        public string fileName = "";
        public string created = "";
        public string size = "";
        public string mimeType = "";
        public string content = "";
        public string authorDisplayName = "";

        public JIssueAttachment getIssueAttachmentFromJsonString(string JSONString) {
            JsonData data = JsonMapper.ToObject(JSONString);
            try {
                try {
                    this.fileName = data["filename"].ToString();
                } catch (Exception e) {
                    this.fileName = "---";
                }
                try {
                    this.created = DateTime.Parse(data["created"].ToString()).ToShortDateString();
                } catch (Exception e) {
                    this.created = "---";
                }
                try {
                    this.size = data["size"].ToString();
                } catch (Exception e) {
                    this.size = "---";
                }
                try {
                    this.mimeType = data["mimeType"].ToString();
                } catch (Exception e) {
                    this.mimeType = "---";
                }
                try {
                    this.content = data["content"].ToString();
                } catch (Exception e) {
                    this.content = "---";
                }
                try {
                    this.authorDisplayName = data["author"]["displayName"].ToString();
                } catch (Exception e) {
                    this.authorDisplayName = "---";
                }
            } catch (Exception e) { }
            return this;
        }
    }

    /// <summary>
    /// This class represents an entry in the changelog of an issue. (Used in history)
    /// </summary>
    public class JIssueChangelog {

        public string created = "";
        public string field = "";
        public string fromValue = "";
        public string toValue = "";
        public string authorName = "";
        public string authorEmail = "";
        public string authorDisplayName = "";
        public string authorAvatar = "";
        public Texture2D authorAvatarTexture = new Texture2D(30, 30);

        public JIssueChangelog getChangeLogFromJsonString(string JSONString) {
            JsonData data = JsonMapper.ToObject(JSONString);
            try {
                try {
                    this.created = DateTime.Parse(data["created"].ToString()).ToShortDateString();
                } catch (Exception e) {
                    this.created = "---";
                }
                try {
                    this.field = data["items"][0]["field"].ToString();
                } catch (Exception e) {
                    this.field = "---";
                }
                try {
                    this.fromValue = data["items"][0]["fromString"].ToString();
                } catch (Exception e) {
                    this.fromValue = "---";
                }
                try {
                    this.toValue = data["items"][0]["toString"].ToString();
                } catch (Exception e) {
                    this.toValue = "---";
                }
                try {
                    this.authorName = data["author"]["name"].ToString();
                } catch (Exception e) {
                    this.authorName = "---";
                }
                try {
                    this.authorEmail = data["author"]["emailAddress"].ToString();
                } catch (Exception e) {
                    this.authorEmail = "---";
                }
                try {
                    this.authorAvatar = data["author"]["avatarUrls"]["48x48"].ToString();
                } catch (Exception e) {
                    this.authorAvatar = "---";
                }
                try {
                    this.authorDisplayName = data["author"]["displayName"].ToString();
                } catch (Exception e) {
                    this.authorDisplayName = "---";
                }
            } catch (Exception e) { }
            return this;
        }

    }

#pragma warning restore 0168
}
