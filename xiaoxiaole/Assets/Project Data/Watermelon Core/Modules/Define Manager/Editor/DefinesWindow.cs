﻿using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Watermelon
{
    public class DefinesWindow : EditorWindow
    {      
        private Define[] projectDefines;

        private bool isDefinesSame;
        private bool isRequireInit;

        [MenuItem("Tools/Editor/Define Manager")]
        public static void ShowWindow()
        {
            DefinesWindow window = GetWindow<DefinesWindow>(true);
            window.minSize = new Vector2(300, 200);
            window.titleContent = new GUIContent("Define Manager");
        }

        protected void OnEnable()
        {
            isRequireInit = true;

            CacheVariables();
        }
                
        private string[] GetActiveStaticDefines()
        {
            string definesLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            if (!string.IsNullOrEmpty(definesLine))
            {
                List<string> activeDefines = new List<string>();

                string[] defines = definesLine.Split(';');

                for (int i = 0; i < DefinesSettings.STATIC_DEFINES.Length; i++)
                {
                    if (Array.FindIndex(defines, x => x.Equals(DefinesSettings.STATIC_DEFINES[i])) != -1)
                    {
                        activeDefines.Add(DefinesSettings.STATIC_DEFINES[i]);
                    }
                }

                return activeDefines.ToArray();
            }

            return null;
        }

        private void CacheVariables()
        {
            // Get project defines
            List<Define> defines = new List<Define>();

            // Get static defines
            string[] activeStaticDefines = GetActiveStaticDefines();
            if (!activeStaticDefines.IsNullOrEmpty())
            {
                for (int i = 0; i < activeStaticDefines.Length; i++)
                {
                    defines.Add(new Define(activeStaticDefines[i], Define.Type.Static, true));
                }
            }

            //Get assembly
            List<Type> gameTypes = new List<Type>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                if(assembly != null)
                {
                    try
                    {
                        Type[] tempTypes = assembly.GetTypes();

                        tempTypes = tempTypes.Where(m => m.IsDefined(typeof(DefineAttribute), true)).ToArray();

                        if (!tempTypes.IsNullOrEmpty())
                            gameTypes.AddRange(tempTypes);
                    }
                    catch (ReflectionTypeLoadException e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

            foreach (Type type in gameTypes)
            {
                //Get attribute
                DefineAttribute[] defineAttributes = (DefineAttribute[])Attribute.GetCustomAttributes(type, typeof(DefineAttribute));

                for (int i = 0; i < defineAttributes.Length; i++)
                {
                    if (string.IsNullOrEmpty(defineAttributes[i].AssemblyType))
                    {
                        int methodId = defines.FindIndex(x => x.define == defineAttributes[i].Define);
                        if (methodId == -1)
                        {
                            defines.Add(new Define(defineAttributes[i].Define, Define.Type.Project));
                        }
                    }
                }
            }

            List<RegisteredDefine> registeredDefines = DefinesSettings.GetDynamicDefines();
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            string[] currentDefinesArray = defineLine.Split(';');
            for(int i = 0; i < currentDefinesArray.Length; i++)
            {
                if(!string.IsNullOrEmpty(currentDefinesArray[i]))
                {
                    if(registeredDefines.FindIndex(x => x.Define == currentDefinesArray[i]) != -1)
                    {
                        defines.Add(new Define(currentDefinesArray[i], Define.Type.Auto, true));
                    } 
                    else if (defines.FindIndex(x => x.define == currentDefinesArray[i]) == -1)
                    {
                        defines.Add(new Define(currentDefinesArray[i], Define.Type.ThirdParty, true));
                    }
                }
            }

            projectDefines = defines.ToArray();

            LoadActiveDefines();
        }

        private void LoadActiveDefines()
        {
            string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));

            string[] currentDefinesArray = defineLine.Split(';');

            if(!currentDefinesArray.IsNullOrEmpty())
            {
                for(int i = 0; i < currentDefinesArray.Length; i++)
                {
                    int defineIndex = Array.FindIndex(projectDefines, x => x.define.Equals(currentDefinesArray[i]));

                    if(defineIndex != -1)
                    {
                        projectDefines[defineIndex].isEnabled = true;
                    }
                }
            }
        }

        private string GetActiveDefinesLine()
        {
            string definesLine = "";

            for(int i = 0; i < projectDefines.Length; i++)
            {
                if(projectDefines[i].isEnabled)
                {
                    definesLine += projectDefines[i].define + ";";
                }
            }

            return definesLine;
        }

        private void SaveDefines(string definesLine)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget), definesLine);
        }

        private bool CompareDefines()
        {
            string[] currentDefinesArray = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)).Split(';');

            for (int i = 0; i < projectDefines.Length; i++)
            {
                int findIndex = Array.FindIndex(currentDefinesArray, x => x == projectDefines[i].define);

                if (projectDefines[i].isEnabled)
                {
                    if (findIndex == -1)
                        return false;
                }
                else
                {
                    if (findIndex != -1)
                        return false;
                }
            }

            return true;
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical(WatermelonEditor.Styles.Skin.box);
            
            if(!projectDefines.IsNullOrEmpty())
            {
                EditorGUI.BeginChangeCheck();

                int customDefineIndex = 0;

                for (int i = 0; i < projectDefines.Length; i++)
                {
                    EditorGUILayout.BeginHorizontal();
                    
                    switch (projectDefines[i].type)
                    {
                        case Define.Type.Auto:

                            projectDefines[i].isEnabled = EditorGUILayout.Toggle(projectDefines[i].isEnabled, GUILayout.Width(20));
                            EditorGUILayout.LabelField(projectDefines[i].define + " (Auto)");

                            break;
                        case Define.Type.Static:

                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.Toggle(true, GUILayout.Width(20));
                            EditorGUILayout.LabelField(projectDefines[i].define);

                            GUILayout.Space(22);

                            EditorGUI.EndDisabledGroup();

                            break;
                        case Define.Type.Project:
                            projectDefines[i].isEnabled = EditorGUILayout.Toggle(projectDefines[i].isEnabled, GUILayout.Width(20));
                            EditorGUILayout.LabelField(projectDefines[i].define);

                            break;
                        case Define.Type.ThirdParty:
                            EditorGUI.BeginDisabledGroup(true);
                            EditorGUILayout.Toggle(true, GUILayout.Width(20));
                            EditorGUI.EndDisabledGroup();

                            EditorGUILayout.LabelField(projectDefines[i].define + " (Thrid Party)");

                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button("X", WatermelonEditor.Styles.button_04_mini, GUILayout.Height(18), GUILayout.Width(18)))
                            {
                                if (EditorUtility.DisplayDialog("Remove define", "Are you sure you want to remove define?", "Remove", "Cancel"))
                                {
                                    string defineLine = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
                                    string[] currentDefinesArray = defineLine.Split(';');

                                    defineLine = "";
                                    for (int k = 0; k < currentDefinesArray.Length; k++)
                                    {
                                        if (currentDefinesArray[k] != projectDefines[i].define)
                                            defineLine += currentDefinesArray[k] + ";";
                                    }
                                    
                                    SaveDefines(defineLine);
                                }
                            }

                            customDefineIndex++;
                            break;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                if(EditorGUI.EndChangeCheck())
                {
                    isRequireInit = true;
                }
            }
            else
            {
                EditorGUILayout.LabelField("There are no defines in project.");
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(WatermelonEditor.Styles.Skin.box);
            
            if (isRequireInit)
            {
                isDefinesSame = CompareDefines();

                isRequireInit = false;
            }

            EditorGUI.BeginDisabledGroup(isDefinesSame);

            if (GUILayout.Button("Apply Defines", WatermelonEditor.Styles.button_01))
            {
                SaveDefines(GetActiveDefinesLine());
                
                return;
            }

            EditorGUI.EndDisabledGroup();

            if (GUILayout.Button("Check Auto Defines", WatermelonEditor.Styles.button_01))
            {
                DefineManager.CheckAutoDefines();

                return;
            }

            EditorGUILayout.EndVertical();

            EditorGUILayoutCustom.DrawCompileWindow(new Rect(0, 0, Screen.width, Screen.height));
        }

        [System.Serializable]
        private class Define
        {
            public string define;
            public Type type;

            public bool isEnabled;

            public Define(string define, Type type, bool isEnabled = false)
            {
                this.define = define;
                this.type = type;
                this.isEnabled = isEnabled;
            }

            public enum Type
            {
                Static = 0,
                Project = 1,
                ThirdParty = 2,
                Auto = 3
            }
        }
    }
}