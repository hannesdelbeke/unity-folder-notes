using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class FolderNoteEditor : Editor
{
    static string previousSelectedFolderPath = "";
    static string selectedFolderPath = "";

    static FolderNoteEditor()
    {
        Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged()
    {
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);

        if (AssetDatabase.IsValidFolder(path) && path != selectedFolderPath)
        {
            previousSelectedFolderPath = selectedFolderPath;
            selectedFolderPath = path;

            EditorUtility.SetDirty(Selection.activeObject);
        }
        else
        {
            selectedFolderPath = "";
        }
    }

    [CustomEditor(typeof(DefaultAsset))]
    public class FolderInspector : Editor
    {
        private string folderNote = "";
        private string lastSavedNote = "";
        private bool isEditing = false;

        private void OnEnable()
        {
            LoadNote();
        }

        public override void OnInspectorGUI()
        {
            if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(target)))
            {
                EditorGUI.EndDisabledGroup();  // Workaround for folder UI

                EditorGUILayout.LabelField("Folder Notes", EditorStyles.boldLabel);

                // Detect start and end of editing
                EditorGUI.BeginChangeCheck();
                GUI.SetNextControlName("FolderNoteTextArea");
                folderNote = EditorGUILayout.TextArea(folderNote, GUILayout.Height(60));

                if (EditorGUI.EndChangeCheck())
                {
                    isEditing = true;  // Track that editing is in progress
                }

                if (isEditing && Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() != "FolderNoteTextArea")
                {
                    // Save the note only when focus is lost
                    isEditing = false;
                    if (folderNote != lastSavedNote)
                    {
                        SaveNote();
                        lastSavedNote = folderNote;
                    }
                }

                EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.HelpBox("This note is saved in the folder's .meta file.", MessageType.Info);
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void LoadNote()
        {
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(selectedFolderPath);
                folderNote = importer.userData;
                lastSavedNote = folderNote;
            }
        }

        private void SaveNote()
        {
            if (!string.IsNullOrEmpty(selectedFolderPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(selectedFolderPath);
                importer.userData = folderNote;
                importer.SaveAndReimport();

                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(selectedFolderPath);
            }
        }
    }
}
