using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnrealByte.EasyJira;

public class JiraObject : MonoBehaviour {

    public static JiraObject jiraInstance;
    private JiraConnect jiraConnect;

    [Header("Jira connection settings")]
    public string jiraBaseRestURL = "";
    public string jiraUser = "";
    public string jiraPassword = "";
    public string jiraProjectKey = "";

    [Space(10)]
    [Header("UI settings")]
    public GameObject busyUI;
    public GameObject fillUI;
    public GameObject feedbackForm;

    //UI
    [Header("UI Objects")]
    public InputField titleInput;
    public InputField descriptionInput;
    public Dropdown issueTypesDropdown;

    [Space(10)]
    [Header("Log settings")]
    public bool logActive;
    public bool logInCustomFile;
    public string logFilePath;
    public string initLogMessage;
    public bool debugLog;
    public bool includeWarnings;

    public bool takeScreenshot;

    private UBLog tLog;

    void Awake() {
        if (jiraInstance == null) {
            jiraInstance = this;
        } else if (jiraInstance != this) {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);

        if (logActive) {
            tLog = new UBLog();
            tLog.initializeLog(false, "", initLogMessage, debugLog, includeWarnings);
            this.logFilePath = tLog.logFilePath;
        }
    }

    /// <summary>
    /// Initialization:
    /// Download and fill the dropdown with the issue types in th feedback form.
    /// </summary>
    /// <returns></returns>
    public IEnumerator Start() {

        if (jiraUser.Length == 0) {
            Debug.Log("[EasyJira] Please set the Jira user in the Jira object");
        } else if (jiraPassword.Length == 0) {
            Debug.Log("[EasyJira] Please set the Jira password in the Jira object");
        } else if (jiraBaseRestURL.Length == 0) {
            Debug.Log("[EasyJira] Please set the Jira Base Rest URL in the Jira object");
        } else if (jiraProjectKey.Length == 0) {
            Debug.Log("[EasyJira] Please set the Jira Project Key.");
        } else {
            if (jiraBaseRestURL.EndsWith("/"))
                jiraBaseRestURL = jiraBaseRestURL.Substring(0, jiraBaseRestURL.Length - 1);
            
            JiraSettings settings = ScriptableObject.CreateInstance("JiraSettings") as JiraSettings;
            settings.Init(jiraBaseRestURL, jiraUser, jiraPassword, jiraProjectKey);
            jiraConnect = new JiraConnect();

            yield return jiraConnect.APIDownloadProjectIssueTypesIG(settings, jiraProjectKey, false);

            if (jiraConnect.jIssueTypes != null) {
                //Create a List of new Dropdown options
                List<string> m_DropOptions = new List<string> { };
                for (int i = 0; i < jiraConnect.jIssueTypes.Count; i++) {
                    m_DropOptions.Add(jiraConnect.jIssueTypes[i].name);
                }
                issueTypesDropdown.AddOptions(m_DropOptions);
                issueTypesDropdown.RefreshShownValue();
            }
        }        
    }

    /// <summary>
    /// This is called at the onCLick event from the feedback form.
    /// </summary>
    public void SendForm() {
        if (JiraObject.jiraInstance != null) {
            if (titleInput.text == "") {
                fillUI.GetComponent<Text>().text = "Please complete the title.";
                StartCoroutine(ActivateGO(fillUI, 2));
            } else if(descriptionInput.text == "") {
                fillUI.GetComponent<Text>().text = "Please complete the description.";
                StartCoroutine(ActivateGO(fillUI, 2));
            } else if (issueTypesDropdown.value == 0) {
                fillUI.GetComponent<Text>().text = "Select an Issue Type.";
                StartCoroutine(ActivateGO(fillUI, 2));
            } else {
                int issuetypeId = 0;
                string issuetypeName = issueTypesDropdown.options[issueTypesDropdown.value].text;
                for (int i = 0; i < jiraConnect.jIssueTypes.Count; i++) {
                    if (jiraConnect.jIssueTypes[i].name == issuetypeName) {
                        issuetypeId = jiraConnect.jIssueTypes[i].id;
                        break;
                    }
                }
                JiraObject.jiraInstance.SendIssueForm(titleInput.text, descriptionInput.text, issuetypeId);

                titleInput.text = "";
                descriptionInput.text = "";
            }
        }
    }

    /// <summary>
	/// Validates the form data, creates the issue object and continue to the post function.
	/// </summary>
	/// <param name="title">The form input data for issue summary</param>
	/// <param name="description">The form input data for issue description</param>
	/// <param name="issueType">The issue type to categorize.</param>
	public Coroutine SendIssueForm(string title, string description, int issueType) {

        string encodedAtuh = "";
        encodedAtuh = JiraConnect.GenerateBasicAuth(jiraUser, jiraPassword);

        JIssue jIssue = new JIssue(title, description);
        jIssue.issuetype = new JIssueType(issueType);

        return StartCoroutine(SendIssuePost(encodedAtuh, jIssue));
    }

    /// <summary>
    /// It obtains the screen capture and the registry (if they are enabled) 
    /// and sends all the data to the Jira connector to process them.
    /// </summary>
    /// <param name="encodedAuth">The base 64 encoded credentials for basic auth.</param>
    /// <param name="jIssue">The issue object</param>
    /// <returns></returns>
    public IEnumerator SendIssuePost(string encodedAuth, JIssue jIssue) {
        Debug.Log("[EasyJira] SendIssueForm");           

        if (takeScreenshot) {
            feedbackForm.SetActive(false);
            yield return new WaitForEndOfFrame();
            string path = "/screenshot.png";            
            Texture2D screenImage = new Texture2D(Screen.width, Screen.height);
            //Get Image from screen
            screenImage.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenImage.Apply();
            //Convert to png
            byte[] imageBytes = screenImage.EncodeToPNG();
            //Save image to file
            System.IO.File.WriteAllBytes(Application.persistentDataPath + path, imageBytes);
        }

        busyUI.SetActive(true);

        if (!logActive) {
            logFilePath = null;
        }
        JiraSettings settings = ScriptableObject.CreateInstance("JiraSettings") as JiraSettings;
        settings.Init(jiraBaseRestURL, jiraUser, jiraPassword, jiraProjectKey);

        yield return jiraConnect.APISendIssueIG(settings, jIssue, logFilePath, takeScreenshot);

        yield return new WaitForSeconds(1);

        busyUI.SetActive(false);
    }

    /// <summary>
    /// Activates a GameObject
    /// Mostly used to show a message when the user not fill required inputs.
    /// </summary>
    /// <param name="gameObject">Game object.</param>
    /// <param name="timeInSeconds">Time in seconds.</param>
    /// <param name="setActive">If set to <c>true</c> set active.</param>
    public IEnumerator ActivateGO(GameObject gameObject, float timeInSeconds, bool setActive = true) {
        gameObject.SetActive(setActive);
        yield return new WaitForSeconds(timeInSeconds);
        gameObject.SetActive(!setActive);
    }
}
