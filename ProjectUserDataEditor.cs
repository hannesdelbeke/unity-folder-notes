using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class ProjectUserDataEditor : EditorWindow
// a tool to browse all assets in the project with custom userdata
{
    private Vector2 scrollPosition;
    private List<AssetData> assetList = new List<AssetData>();
    private bool showOnlyWithUserData = true;
    private AssetData selectedAsset = null;

    // Class to store information about each asset
    private class AssetData
    {
        public string path;
        public string userData;
    }

    [MenuItem("Tools/Project User Data Editor")]
    public static void ShowWindow()
    {
        var window = GetWindow<ProjectUserDataEditor>("Project User Data Editor");
        window.RefreshAssetList();
    }

    private void OnEnable()
    {
        RefreshAssetList();
    }

    private void RefreshAssetList()
    {
        assetList.Clear();

        // Get all asset paths in the project
        string[] assetPaths = AssetDatabase.GetAllAssetPaths();
        foreach (string path in assetPaths)
        {
            if (AssetDatabase.IsValidFolder(path) || Path.GetExtension(path) != "")
            {
                // Load userData from each asset's .meta file
                AssetImporter importer = AssetImporter.GetAtPath(path);
                if (importer != null)
                {
                    string userData = importer.userData;

                    // Only add to list if showOnlyWithUserData is false or userData is not empty
                    if (!showOnlyWithUserData || !string.IsNullOrEmpty(userData))
                    {
                        AssetData assetData = new AssetData
                        {
                            path = path,
                            userData = userData
                        };
                        assetList.Add(assetData);
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Project User Data Editor", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This tool shows all assets and folders in the project with their UserData. Edit the UserData for the selected item and click 'Save' to update the .meta file.", MessageType.Info);

        // Toggle for showing only assets with UserData
        // showOnlyWithUserData = EditorGUILayout.Toggle("Show Only Assets with UserData", showOnlyWithUserData);
        // this is too slow in a big project. currently rendering a button for each entry is too slow.

        if (GUILayout.Button("Refresh Asset List"))
        {
            RefreshAssetList();
        }

        EditorGUILayout.Space(10);

        // Display list of assets
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        foreach (var asset in assetList)
        {
            EditorGUILayout.BeginHorizontal();

            // Display asset path with clickable selection
            if (GUILayout.Button(asset.path, GUILayout.Width(400)))
            {
                selectedAsset = asset;
            }

            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();

        // Show TextArea only for selected asset
        if (selectedAsset != null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Selected Asset: " + selectedAsset.path, EditorStyles.boldLabel);

            EditorGUILayout.LabelField("User Data:", EditorStyles.boldLabel);
            string newUserData = EditorGUILayout.TextArea(selectedAsset.userData, GUILayout.Height(60));

            if (newUserData != selectedAsset.userData)
            {
                selectedAsset.userData = newUserData;

                if (GUILayout.Button("Save"))
                {
                    SaveUserData(selectedAsset.path, selectedAsset.userData);
                }
            }
        }
    }

    private void SaveUserData(string path, string userData)
    {
        AssetImporter importer = AssetImporter.GetAtPath(path);
        if (importer != null)
        {
            importer.userData = userData;
            importer.SaveAndReimport();
            Debug.Log($"Updated userData for {path}");
        }
        else
        {
            Debug.LogWarning($"Failed to update userData for {path}");
        }
    }
}
