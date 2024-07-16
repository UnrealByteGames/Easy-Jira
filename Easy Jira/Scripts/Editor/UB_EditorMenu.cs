using UnityEditor;
using UnrealByte.EasyJira;

public class UB_EditorMenu : EditorWindow {

    static bool issuesAdmin = false;
    static bool support = false;

    [MenuItem("Tools/EasyJira/Issues Admin",false , 1)]
    static void InitIssuesAdmin() {        
        issuesAdmin = true;
        //settings = false;
        support = false;
        Init();
    }

    [MenuItem("Tools/EasyJira/Support", false, 2)]
    static void InitSupport() {
        issuesAdmin = false;
        support = true;
        Init();
    }

    static void Init() {
        if (issuesAdmin) {
            EditorWindow.GetWindow(typeof(EJ_IssuesAdminWindow), false, "EasyJira - Issues");
            EditorWindow.GetWindowWithRect(typeof(EJ_IssuesAdminWindow), new UnityEngine.Rect(0, 0, 800, 450));
        } else if (support) {
            EditorWindow.GetWindow(typeof(UB_Support), false, "EasyJira - Support");
        }
    }
}
