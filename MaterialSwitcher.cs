using UnityEngine;
using UnityEditor;
using System.IO;

#if UNITY_EDITOR
public class MaterialSwitcher : EditorWindow
{
    private string gameFolder;
    private string bakeFolder;
    private string intermediateFolder;

    private const string GameFolderKey = "MaterialSwitcher_GameFolder";
    private const string BakeFolderKey = "MaterialSwitcher_BakeFolder";
    private const string IntermediateFolderKey = "MaterialSwitcher_IntermediateFolder";

    [MenuItem("Tools/Material Switcher")]
    static void ShowMaterialSwitcherWindow()
    {
        GetWindow<MaterialSwitcher>("Material Switcher");
    }

    private void OnEnable()
    {
        // Load saved folder paths from EditorPrefs
        gameFolder = EditorPrefs.GetString(GameFolderKey, "Assets/Game");
        bakeFolder = EditorPrefs.GetString(BakeFolderKey, "Assets/Bake");
        intermediateFolder = EditorPrefs.GetString(IntermediateFolderKey, "Assets/Intermediate");
    }

    private void OnGUI()
    {
        GUILayout.Label("Material Switcher Configuration", EditorStyles.boldLabel);

        // Detect changes in folder paths
        string newGameFolder = EditorGUILayout.TextField("Game Folder", gameFolder);
        string newBakeFolder = EditorGUILayout.TextField("Bake Folder", bakeFolder);
        string newIntermediateFolder = EditorGUILayout.TextField("Intermediate Folder", intermediateFolder);

        if (newGameFolder != gameFolder || newBakeFolder != bakeFolder || newIntermediateFolder != intermediateFolder)
        {
            gameFolder = newGameFolder;
            bakeFolder = newBakeFolder;
            intermediateFolder = newIntermediateFolder;

            // Save folder paths to EditorPrefs when they are changed
            EditorPrefs.SetString(GameFolderKey, gameFolder);
            EditorPrefs.SetString(BakeFolderKey, bakeFolder);
            EditorPrefs.SetString(IntermediateFolderKey, intermediateFolder);
            UnityEngine.Debug.Log("Folder paths saved.");
        }

        if (GUILayout.Button("Switch Materials for Baking"))
        {
            SwitchMaterials(bakeFolder, intermediateFolder);
        }

        if (GUILayout.Button("Switch Materials for Gameplay"))
        {
            SwitchMaterials(gameFolder, intermediateFolder);
        }

        if (GUILayout.Button("Update Bake Materials from Intermediate"))
        {
            if (EditorUtility.DisplayDialog("Update Materials", "Are you sure you want to update Bake materials from Intermediate? This will overwrite existing materials in the Bake folder.", "Yes", "No"))
            {
                UpdateMaterials(intermediateFolder, bakeFolder);
            }
        }

        if (GUILayout.Button("Update Game Materials from Intermediate"))
        {
            if (EditorUtility.DisplayDialog("Update Materials", "Are you sure you want to update Game materials from Intermediate? This will overwrite existing materials in the Game folder.", "Yes", "No"))
            {
                UpdateMaterials(intermediateFolder, gameFolder);
            }
        }
    }

    private void SwitchMaterials(string sourceFolder, string destinationFolder)
    {
        if (!Directory.Exists(sourceFolder) || !Directory.Exists(destinationFolder))
        {
            UnityEngine.Debug.LogError("Source or destination folder does not exist.");
            return;
        }

        CopyMaterialsRecursively(sourceFolder, destinationFolder);

        // Refresh the asset database to make Unity aware of the changes
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Materials switched successfully.");
    }

    private void UpdateMaterials(string sourceFolder, string destinationFolder)
    {
        if (!Directory.Exists(sourceFolder) || !Directory.Exists(destinationFolder))
        {
            UnityEngine.Debug.LogError("Source or destination folder does not exist.");
            return;
        }

        CopyMaterialsRecursively(sourceFolder, destinationFolder);

        // Refresh the asset database to make Unity aware of the changes
        AssetDatabase.Refresh();
        UnityEngine.Debug.Log("Materials updated successfully.");
    }

    private void CopyMaterialsRecursively(string sourceFolder, string destinationFolder)
    {
        foreach (var directory in Directory.GetDirectories(sourceFolder, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(directory.Replace(sourceFolder, destinationFolder));
        }

        foreach (string materialFile in Directory.GetFiles(sourceFolder, "*.mat", SearchOption.AllDirectories))
        {
            string relativePath = Path.GetRelativePath(sourceFolder, materialFile);
            string destinationPath = Path.Combine(destinationFolder, relativePath);

            // Copy the material file from source to destination
            File.Copy(materialFile, destinationPath, true);
        }
    }
}
#endif