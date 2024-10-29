using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class FolderNoteEditor : Editor
{
    static string previousSelectedFolderPath = "";
    static string selectedFolderPath = "";
    //public static string folderNote = "";

    static FolderNoteEditor()
    {
        //Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged()
    {
        string selectedObjectPath = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (AssetDatabase.IsValidFolder(selectedObjectPath) && selectedObjectPath != selectedFolderPath)
        {
            previousSelectedFolderPath = selectedFolderPath;
            selectedFolderPath = selectedObjectPath;
        }
        else
        {
            selectedFolderPath = "";
        }
    }

    [CustomEditor(typeof(DefaultAsset))]
    public class FolderInspector : Editor
    {

        private static string folderNote = "";
        private string lastSavedNote = "";
        private bool isEditing = false;

        private void OnEnable()
        // runs when we select a folder
        {
            // before we get new variables, save the old note
            SaveSelectedNote();

            OnSelectionChanged();
            LoadNote();
        }

        public override void OnInspectorGUI()
        {
            if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(target)))
            {
                OnFolderInspectorGUI();
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void OnFolderInspectorGUI()
        {
            EditorGUI.EndDisabledGroup();  // HACK Workaround for unity disabling UI in the inspector for folders

            EditorGUILayout.LabelField("Folder Notes", EditorStyles.boldLabel);

            // Detect start and end of editing
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("FolderNoteTextArea");
            folderNote = EditorGUILayout.TextArea(folderNote, GUILayout.Height(60));

            isEditing = EditorGUI.EndChangeCheck();  // Track that editing is in progress

            if (isEditing && Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() != "FolderNoteTextArea")
            {
                // Save the note only when focus is lost
                isEditing = false;
                if (folderNote != lastSavedNote)
                {
                    SaveSelectedNote();
                }
            }

            EditorGUI.BeginDisabledGroup(true);
        }

        public void LoadNote()
        {
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(selectedFolderPath);
                folderNote = importer.userData;
                lastSavedNote = folderNote;
            }
        }

        private void SaveSelectedNote()
        // save the current note to the folder's .meta file
        // assumes we keep the same folder object selected, e.g. when losing focus on the text area
        {
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(selectedFolderPath);
                importer.userData = folderNote;
                lastSavedNote = folderNote;
            }
        }
    }
}
