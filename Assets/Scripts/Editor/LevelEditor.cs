using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;

public class LevelEditor : OdinMenuEditorWindow
{
    [MenuItem("Tools/Level Editor")]        // This attribute allows this editor to be accessed from Tools Menu in Unity
    private static void OpenWindow()
    {
        GetWindow<LevelEditor>().Show();
    }
    protected override OdinMenuTree BuildMenuTree()
    {
        var tree = new OdinMenuTree();
        tree.Add("Create New Level", new CreateNewLevel());
        tree.AddAllAssetsAtPath("Level Data", "Assets/Data/Levels", typeof(LevelRecord));
        return tree;
    }

    private class CreateNewLevel
    {
        public CreateNewLevel()
        {

        }

        
    }

}
