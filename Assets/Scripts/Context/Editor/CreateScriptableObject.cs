using System;
using UnityEditor;
using System.Reflection;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Allows creating of scriptable objects by selecting them in the dropdown box and selecting create
/// </summary>
public class CreateScriptableObject : EditorWindow
{
	private Type[] scriptableObjects;
	private int selectedIndex;

	[MenuItem("Assets/Create/Scriptable Object")]
	public static void OpenWindow()
	{
		CreateScriptableObject window = (CreateScriptableObject)EditorWindow.GetWindow(typeof(CreateScriptableObject));
		window.LoadScriptableObjects();
		window.Show();
	}

	private void OnGUI()
	{
		GUILayout.Label("Select scriptable object...");

		selectedIndex = EditorGUILayout.Popup("", selectedIndex, scriptableObjects.Select(x => x.Name).ToArray());

		UnityEngine.Object pathObject = Selection.activeObject;
		string path = pathObject == null ? "Assets" : AssetDatabase.GetAssetPath(pathObject);

		EditorGUI.BeginDisabledGroup(scriptableObjects.Length == 0 || !Directory.Exists(path));
		if(GUILayout.Button("Create"))
		{
			ScriptableObject instance = CreateInstance(scriptableObjects[selectedIndex]);
			AssetDatabase.CreateAsset(instance, Path.Combine(path, $"{instance.GetType().Name}.asset"));
			AssetDatabase.SaveAssets();

			EditorUtility.FocusProjectWindow();
			Selection.activeObject = instance;
		}
		EditorGUI.EndDisabledGroup();
	}

	private void LoadScriptableObjects()
	{
		Type s = typeof(ScriptableObject);
		List<Type> scriptableObjects = new List<Type>();

		foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			scriptableObjects.AddRange(assembly.GetTypes().Where(t => 
				t != s && 
				t != GetType() &&
				!t.IsAbstract &&
				s.IsAssignableFrom(t) && 
				!string.IsNullOrEmpty(t.Namespace) && 
				t.Namespace.StartsWith("DN"))
			);
		}

		this.scriptableObjects = scriptableObjects.ToArray();
	}
}
