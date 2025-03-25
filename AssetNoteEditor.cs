using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

[InitializeOnLoad]
public class AssetNoteEditor : Editor
{
    static string previousSelectedAssetPath = "";
    static string selectedAssetPath = "";

    static AssetNoteEditor()
    {
        //Selection.selectionChanged += OnSelectionChanged;
    }

    private static void OnSelectionChanged()
    {
        // print asset type, great when we need to add note support for new asset types
        // Debug.Log("Selection changed to " + Selection.activeObject);
        string selectedObjectPath = AssetDatabase.GetAssetPath(Selection.activeObject);
        if (AssetDatabase.AssetPathExists(selectedObjectPath) && selectedObjectPath != selectedAssetPath)
        {
            previousSelectedAssetPath = selectedAssetPath;
            selectedAssetPath = selectedObjectPath;
        }
        else
        {
            selectedAssetPath = "";
        }
    }

    [CustomEditor(typeof(UnityEngine.Object), true)]
    public class AssetInspector : Editor
    {
        private static string assetNote = "";
        private string lastSavedNote = "";
        private bool isEditing = false;
        private Vector2 scrollPosition = Vector2.zero; // Keeps track of the scroll position

        private void OnEnable()
        {
            // runs when we select a asset
            // before we get new variables, save the old note
            SaveSelectedNote();

            OnSelectionChanged();
            LoadNote();
        }

        public override void OnInspectorGUI()
        {
            if (AssetDatabase.AssetPathExists(AssetDatabase.GetAssetPath(target)))
            {
                base.OnInspectorGUI();
                var isFolder = target.GetType() == typeof(DefaultAsset);
                AssetInspectorGUI(isFolder);
            }
            else
            {
                base.OnInspectorGUI();
            }
        }

        private void AssetInspectorGUI(bool isFolder)
        {
            if (isFolder)
            {
                EditorGUI.EndDisabledGroup();  // HACK Workaround for unity disabling UI in the inspector for folders
            }

            EditorGUILayout.LabelField("Notes", EditorStyles.boldLabel);

            // Detect start and end of editing
            EditorGUI.BeginChangeCheck();
            GUI.SetNextControlName("NoteTextArea");

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.Height(100)); // Set the desired height
            assetNote = EditorGUILayout.TextArea(assetNote, GUILayout.ExpandHeight(true)); // Expand to fit the scrollable view
            EditorGUILayout.EndScrollView(); // End the scrollable area

            isEditing = EditorGUI.EndChangeCheck();  // Track that editing is in progress

            if (isEditing && Event.current.type == EventType.Repaint && GUI.GetNameOfFocusedControl() != "NoteTextArea")
            {
                // Save the note only when focus is lost
                isEditing = false;
                if (assetNote != lastSavedNote)
                {
                    SaveSelectedNote();
                }
            }

            if (isFolder)
            {
                EditorGUI.BeginDisabledGroup(true);
            }
        }

        public void LoadNote()
        {
            if (!string.IsNullOrEmpty(selectedAssetPath))
            {
                AssetImporter importer = AssetImporter.GetAtPath(selectedAssetPath);
                assetNote = importer.userData;
                lastSavedNote = assetNote;
            }
        }

        private void SaveSelectedNote()
        {
            if (!string.IsNullOrEmpty(selectedAssetPath))
            {
                // save the current note to the folder's .meta file
                // assumes we keep the same asset selected, e.g. when losing focus on the text area
                AssetImporter importer = AssetImporter.GetAtPath(selectedAssetPath);
                importer.userData = assetNote;
                lastSavedNote = assetNote;
            }
        }
    }

    // there are many exceptions to the custom inspector
    // it might be easier to create our own editor window

    [CustomEditor(typeof(MonoScript))]
    public class ScriptInspector : AssetInspector
    {
    }

    [CustomEditor(typeof(TextAsset))]
    public class TextInspector : AssetInspector
    {
    }

    [CustomEditor(typeof(Material))]
    public class MaterialInspector : AssetInspector
    {
    }

    //[CustomEditor(typeof(UniversalRendererData))]
    //public class UniversalRendererDataInspector : AssetInspector
    //{
    //}

    //[CustomEditor(typeof(UniversalRenderPipelineAsset))]
    //public class UniversalRenderPipelineAssetInspector : AssetInspector
    //{
    //    // breaks UI formatting in the inspector, likely uses an internal custom inspector
    //}
}
