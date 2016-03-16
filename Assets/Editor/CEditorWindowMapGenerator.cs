using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class CEditorWindowMapGenerator : EditorWindow
{
    static EditorWindow m_Window = null;

    public int m_iWidth;
    public int m_iHeight;

    public string m_strSeed;
    public bool m_bUseRandomSeed;

    [Range(0, 100)]
    public int m_iRandomFillPercent;

    private int m_iMaxWidth = 200;
    private GameObject m_MapObject;
    private CMapGenerator m_map;
    private CMeshGenerator m_meshMap;
    private string m_strPrefabName;

    private string m_strPrefabExtension = ".prefab";
    private string m_strBasePath = "Assets/Resources/";
    private string m_strAssetExtension  = ".asset";

    [MenuItem("GameObject/Create Map")]
    private static void init()
    {
        if (m_Window)
            m_Window.Close();
        m_Window = EditorWindow.GetWindow<CEditorWindowMapGenerator>();
        m_Window.titleContent.text = "Map Generator";
    }

    void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Map Width : ", EditorStyles.boldLabel, GUILayout.Width(m_iMaxWidth));
        m_iWidth = EditorGUILayout.IntField(m_iWidth, GUILayout.Width(m_iMaxWidth));
        EditorGUILayout.LabelField("Map Height : ", EditorStyles.boldLabel, GUILayout.Width(m_iMaxWidth));
        m_iHeight = EditorGUILayout.IntField(m_iHeight, GUILayout.Width(m_iMaxWidth));    
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        m_bUseRandomSeed = GUILayout.Toggle(m_bUseRandomSeed, "Random Seed", EditorStyles.radioButton, GUILayout.Width(m_iMaxWidth));   
        EditorGUILayout.EndHorizontal();

        if (!m_bUseRandomSeed)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Seed Value : ", EditorStyles.boldLabel, GUILayout.Width(m_iMaxWidth));
            m_strSeed = EditorGUILayout.TextField(m_strSeed, GUILayout.Width(m_iMaxWidth));
            EditorGUILayout.EndHorizontal();
        }
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Create Map", EditorStyles.miniButton, GUILayout.Width(m_iMaxWidth)))
        {
            if(m_MapObject != null)
                DestroyImmediate(m_MapObject);

            
            m_MapObject =  Instantiate(Resources.Load("Map") as GameObject);
            
            m_map = m_MapObject.GetComponent<CMapGenerator>();
            m_meshMap = m_MapObject.GetComponent<CMeshGenerator>();
            if (m_map != null)
            {
                m_map.m_iWidth = m_iWidth;
                m_map.m_iHeight = m_iHeight;
                m_map.m_bUseRandomSeed = m_bUseRandomSeed;
                if (!m_bUseRandomSeed)
                {
                    m_map.m_strSeed = m_strSeed;
                }
                
                m_map.GenerateMap();    
            }            
        }

        if (m_map != null)
        {
            EditorUtility.SetDirty(m_map);
        }

        
        EditorGUILayout.EndHorizontal();

        if (m_MapObject != null)
        {
            EditorGUILayout.BeginHorizontal();
            m_strPrefabName = EditorGUILayout.TextField(m_strPrefabName, GUILayout.Width(m_iMaxWidth));

            if (GUILayout.Button("Save Map", EditorStyles.miniButton, GUILayout.Width(m_iMaxWidth)))
            {
                if (m_meshMap != null)
                {
                    if (!Directory.Exists(m_strBasePath + m_strPrefabName))
                    {
                        //if it doesn't, create it
                        Directory.CreateDirectory(m_strBasePath + m_strPrefabName);
                        AssetDatabase.Refresh();
                    }

                    if (Directory.Exists(m_strBasePath + m_strPrefabName))
                    {
                        SaveMesh(m_MapObject, m_meshMap.m_Walls.sharedMesh, "_wall");
                        SaveMesh(m_MapObject, m_meshMap.m_Cave.sharedMesh, "_cave");
                    }
                }
                CreatePrefab();
            }
            EditorGUILayout.EndHorizontal();
        }
        
    }


    private void CreatePrefab()
    {
        if (m_MapObject != null)
        {
            string path = m_strBasePath + m_strPrefabName + "/" + m_strPrefabName + m_strPrefabExtension;
            Debug.Log(path);
            PrefabUtility.CreatePrefab(path, m_MapObject, ReplacePrefabOptions.Default);
            AssetDatabase.Refresh();
        }
    }

    public void SaveMesh(GameObject gOb, Mesh mesh, string name)
    {     
#if UNITY_EDITOR
        string path = m_strBasePath + m_strPrefabName + "/" + m_strPrefabName + name + m_strAssetExtension;        
        AssetDatabase.CreateAsset(mesh, path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
#endif
    }

}
