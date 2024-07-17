using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UB_Support : EditorWindow {

    static Texture emailTexture = null;
    static Texture documentationTexture = null;
    static Texture rateTexture = null;
    static Texture twitterTexture = null;
    static Texture facebookTexture = null;

    void Awake() {

        emailTexture = Resources.Load<Texture>("EditorWindowTextures/emailTexture");
        documentationTexture = Resources.Load<Texture>("EditorWindowTextures/documentTexture");
        rateTexture = Resources.Load<Texture>("EditorWindowTextures/rateTexture");
        twitterTexture = Resources.Load<Texture>("EditorWindowTextures/twitterTexture");
        facebookTexture = Resources.Load<Texture>("EditorWindowTextures/facebookTexture");
    }

    void OnGUI() {

        GUIStyle guiStyleBox = new GUIStyle();
        guiStyleBox.fixedHeight = 40;
        guiStyleBox.fontSize = 20;
        guiStyleBox.normal.textColor = Color.white;
        guiStyleBox.padding.left = 70;
        guiStyleBox.padding.top = 10;

        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 10, 30, 30), emailTexture);
        GUILayout.Space(25);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Support", guiStyleBox)) {
            Application.OpenURL("mailto:support@unrealbyte.com");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 60, 30, 30), documentationTexture);
        GUILayout.Space(25);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Online Documentation", guiStyleBox)) {
            Application.OpenURL("https://unrealbyte.atlassian.net/wiki/spaces/EJ/overview");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 110, 30, 30), rateTexture);
        GUILayout.Space(25);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Rate and review", guiStyleBox)) {
            Application.OpenURL("https://assetstore.unity.com/packages/tools/integration/easy-jira-111744");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 160, 30, 30), twitterTexture);
        GUILayout.Space(25);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Twitter", guiStyleBox)) {
            Application.OpenURL("https://twitter.com/unrealbytegames");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical("Box");
        GUILayout.BeginHorizontal();
        GUI.DrawTexture(new Rect(20, 210, 30, 30), facebookTexture);
        GUILayout.Space(25);
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Facebook", guiStyleBox)) {
            Application.OpenURL("https://www.facebook.com/unrealbytegames");
        }
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    public void OnInspectorUpdate() {
        this.Repaint();
    }
}
