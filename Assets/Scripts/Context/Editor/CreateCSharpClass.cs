using UnityEngine;
using UnityEditor;

public static class CreateCSharpClass
{
	private const string CSharpTemplatePath = "Assets/Scripts/Context/Editor/Templates/CSharpClassTemplate.cs.txt";
	private const string CSharpInterfaceTemplatePath = "Assets/Scripts/Context/Editor/Templates/CSharpInterfaceTemplate.cs.txt";
	private const string CSharpBehaviourTemplatePath = "Assets/Scripts/Context/Editor/Templates/CSharpBehaviourTemplate.cs.txt";

	private const int priority = 51;

	[MenuItem("Assets/Create/C# Class", priority = priority)]
	public static void CreateClass()
	{
		ProjectWindowUtil.CreateScriptAssetFromTemplateFile(CSharpTemplatePath, "NewClass.cs");
	}

	[MenuItem("Assets/Create/C# Monobehaviour", priority = priority)]
	public static void CreateMonobehaviour()
	{
		ProjectWindowUtil.CreateScriptAssetFromTemplateFile(CSharpBehaviourTemplatePath, "BehaviourScript.cs");
	}

	[MenuItem("Assets/Create/C# Interface", priority = priority)]
	public static void CreateInterface()
	{
		ProjectWindowUtil.CreateScriptAssetFromTemplateFile(CSharpInterfaceTemplatePath, "InterfaceScript.cs");
	}
}
