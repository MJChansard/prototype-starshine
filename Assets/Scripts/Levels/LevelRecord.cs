using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using UnityEditor;
using System;

[CreateAssetMenu(fileName = "LevelRecord", menuName = "Scriptable Objects/Level Record", order = 2)]
[TypeInfoBox("Level Width and Level Height must take the spawn ring into account.")]
public class LevelRecord : SerializedScriptableObject       // Inheriting from ScriptableObject produces serialization issue with [,] levelTopography
{    
    [TabGroup("Settings")][Title("Spawning")]  public int minObjectsPerWave;
    [TabGroup("Settings")]  public int maxObjectsPerWave;
    [TabGroup("Settings")]  public int minGridObjectsOnGrid;
    [TabGroup("Settings")]  public bool scarcity;
    [TabGroup("Settings")]  public bool saturation;
    [TabGroup("Settings")]  public int jumpFuelAmount;      //  #TODO - Need to remove eventually

    [TabGroup("Settings")][Title("System")] public bool VerboseLogging;

    //[TabGroup("Settings")]  public int numberOfPhenomenaToSpawn;
    //[TabGroup("Settings")]  public int numberOfStationsToSpawn;

    [TabGroup("Settings")]  public GridBorder[] bordersEligibleForSpawn;
    [TabGroup("Settings")]  public GridObject[] eligibleForSpawn;

    //  #TOPOGRAPHY
    [TabGroup("Topography")]    public int width;
    [TabGroup("Topography")]    public int height;
        
    [TabGroup("Topography")][Button("New")]
    private void CreateNewTopography()
    {
        levelTopography = new TopographyElementIcon[width, height];
    }
    
    [TabGroup("Topography")][Button("Save")]
    private void Save()
    {
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }
    
    [TableMatrix(HorizontalTitle = "LEVEL TOPOGRAPHY", SquareCells = true, DrawElementMethod = "DrawCell")]
    [TabGroup("Topography")] public TopographyElementIcon[,] levelTopography = new TopographyElementIcon[0,0];
    Vector2Int[,] topographyIndexToGridLocation;

    
    //  #PROPERTIES
    public int BoundaryLeftActual
    {
        get { return -(width / 2); }
    }
    public int BoundaryRightActual
    {
        get
        {
            if (width % 2 == 0)
                return (width / 2) - 1;
            else
                return (width / 2);
        }
    }
    public int BoundaryTopActual
    {
        get
        {
            if (height % 2 == 0)
                return (height / 2) - 1;
            else
                return height / 2;
        }
    }
    public int BoundaryBottomActual
    {
        get { return -(height / 2); }
    }

    public int BoundaryLeftPlay
    {
        get { return BoundaryLeftActual + 1; }
    }
    public int BoundaryRightPlay
    {
        get { return BoundaryRightActual - 1; }
    }
    public int BoundaryTopPlay
    {
        get { return BoundaryTopActual - 1; }
    }
    public int BoundaryBottomPlay
    {
        get { return BoundaryBottomActual + 1; }
    }

    public int LevelAreaPlay { get { return (width - 2) * (height - 2); } }

    public int MaxObjectsOnGrid { get { return LevelAreaPlay / 2; } }
    public int MaxInteriorSpawns { get { return LevelAreaPlay / 4; } }
    public int MaxSpawnOnBorder(GridBorder border)
    {
        int result;
        if (border == GridBorder.Top || border == GridBorder.Bottom)
            result = width - 2;
        else
            result = height - 2;
        
        return result;
    }

    //  #METHODS
    public Vector2Int IndexToGrid(int x, int y)
    {
        return topographyIndexToGridLocation[x, y];
    }

    private void OnEnable()
    {
        topographyIndexToGridLocation = new Vector2Int[width, height];
        Debug.Log("LevelRecord.OnEnable() called.");

        int gridLocationX = BoundaryLeftActual;        
        for (int i = 0; i < width; i++)
        {
            int gridLocationY = BoundaryTopActual;
            for (int j = 0; j < height; j++)
            {
                Vector2Int element = new Vector2Int(gridLocationX, gridLocationY);
                topographyIndexToGridLocation[i, j] = element;
                gridLocationY--;

                if (VerboseLogging)
                    Debug.LogFormat("At index [{0},{1}], location {2} added.", i, j, element.ToString());
                
            }
            gridLocationX++;
        }
    }

    // CONSTRUCTOR

    //  #TODO - Reconsider this constructor given changes to class
    public static LevelRecord CreateLevelRecord(int w, int h, int f, int p, int s)
    {
        LevelRecord lr = LevelRecord.CreateInstance<LevelRecord>();
        lr.width = w;
        lr.height = h;
        lr.jumpFuelAmount = f;
       // lr.numberOfPhenomenaToSpawn = p;
       // lr.numberOfStationsToSpawn = s;

        return lr;
    }

    //  #TODO - 0 references to this method, delete?
    public void InitSpawn(int _minObjectsPerWave, int _maxObjectsPerWave, int _minGridObjectsOnGrid, GridBorder[] _bordersEligibleForSpawn)
    {
        minObjectsPerWave           = _minObjectsPerWave;
        maxObjectsPerWave           = _maxObjectsPerWave;
        minGridObjectsOnGrid        = _minGridObjectsOnGrid;
        bordersEligibleForSpawn     = _bordersEligibleForSpawn;
    }

    //  Required method for Inspector Rendering
    static TopographyElementIcon DrawCell(Rect rect, TopographyElementIcon value)
    {
        return (TopographyElementIcon)SirenixEditorFields.UnityPreviewObjectField(
            rect: rect,
            value: value,
            texture: value?.editorIcon,
            type: typeof(TopographyElementIcon)
        );
    }
}
