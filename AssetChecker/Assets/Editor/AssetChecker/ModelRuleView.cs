﻿using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;

namespace AssetChecker
{
    /// <summary>
    /// 模型规范
    /// </summary>
    public class ModelRuleView
    {

        public const int MaxTriangs = 4000;
        public const int MaxBones = 60;

        private Vector2 scrollPos = Vector2.zero;
        private Dictionary<SettingBean , bool> isFolderOut = new Dictionary<SettingBean, bool>();

        private List<ModelSettingBean> modelSettings ;
        public void Initlizalize()
        {
            modelSettings = OverviewSetting.Instance.ModelSettings;
            
            foreach (ModelSettingBean msb in modelSettings)
            {
                isFolderOut[msb] = false;
            }
        }

        public void OnGUI()
        {
            if(modelSettings == null)   this.Initlizalize();

            NGUIEditorTools.DrawHeader("模型规范设置");
            
            if (modelSettings != null)
            {
                scrollPos = GUILayout.BeginScrollView(scrollPos);
                for (int i = 0; i < modelSettings.Count; i++)
                {
                    GUI.backgroundColor = i % 2 == 1 ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                    GUILayout.BeginHorizontal("AS TextArea");
                    GUI.backgroundColor = Color.white;
                    GUILayout.Space(5);
                    drawSetting(modelSettings[i]);
                    GUILayout.EndVertical();
                }
                GUILayout.EndScrollView();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear" , GUILayout.Width(80)))
            {
                if(File.Exists(OverviewSetting.localFilePath))
                    File.Delete(OverviewSetting.localFilePath);
                OverviewSetting.localFilePath = "";
            }

            if (GUILayout.Button("Load", GUILayout.Width(80)))
            {
                string filePath = EditorUtility.OpenFilePanel("打开", Application.dataPath, "xml");
                OverviewSetting.localFilePath = filePath.Replace(Application.dataPath, "Assets");
                OverviewSetting.Instance.ReadModelSettings();
                this.Initlizalize();
            }

            if (GUILayout.Button("保存"))
            {
                if (string.IsNullOrEmpty(OverviewSetting.localFilePath))
                {
                    string filePath = EditorUtility.SaveFilePanel("保存", Application.dataPath,"new file", "xml");
                    OverviewSetting.localFilePath = filePath.Replace(Application.dataPath, "Assets");
                }
                this.saveModelRule(OverviewSetting.localFilePath);
                AssetDatabase.Refresh();
                Debug.Log("Save Success !");
            }
            GUILayout.Space(10);
            if (GUILayout.Button("添加规则"))
            {
                ModelSettingBean msb = new ModelSettingBean();
                msb.Folder.Add(string.Empty);
                modelSettings.Add(msb);
            }
            GUILayout.EndHorizontal();
        }


        private void drawSetting(ModelSettingBean modelSetting)
        {
            GUILayout.BeginVertical();
            {
                bool isFoldOut = true;
                if (isFolderOut.ContainsKey(modelSetting))
                {
                    isFoldOut = isFolderOut[modelSetting];
                }
                isFolderOut[modelSetting] = EditorGUILayout.Foldout(isFoldOut, modelSetting.AssetDesc);


                if (isFoldOut)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label("文件类型描述", GUILayout.Width(100F));
                    modelSetting.AssetDesc = GUILayout.TextField(modelSetting.AssetDesc);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("文件目录" , GUILayout.Width(100F)))
                            modelSetting.Folder.Add(string.Empty);

                        GUILayout.BeginVertical();
                        for (int i = modelSetting.Folder.Count - 1; i >= 0; i--)
                        {
                            GUILayout.BeginHorizontal();
                            GUILayout.TextField(modelSetting.Folder[i]);
                            if (GUILayout.Button("...", GUILayout.Width(30f)))
                            {
                                string path = EditorUtility.OpenFolderPanel("选择文件夹", Application.dataPath, "");
                                modelSetting.Folder[i] = path.Replace(Application.dataPath, "Assets");
                            }
                            if (GUILayout.Button("X", GUILayout.Width(30f)))
                            {
                               modelSetting.Folder.RemoveAt(i);
                            }
                            GUILayout.EndHorizontal();
                        }
                        GUILayout.EndVertical();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    NGUIEditorTools.SetLabelWidth(120F);
                    modelSetting.MaxTriangs = EditorGUILayout.IntSlider(new GUIContent("最大三角面数"), modelSetting.MaxTriangs,
                                                                        0, MaxTriangs );
                    GUILayout.FlexibleSpace();
                    modelSetting.MaxBones = EditorGUILayout.IntSlider(new GUIContent("最大骨骼数"), modelSetting.MaxBones, 0,
                        MaxBones);
                    GUILayout.EndHorizontal();                     
                }
               
            }

            GUILayout.EndVertical();
        }

        /// <summary>
        /// 保存模型设置配置
        /// </summary>
        private void saveModelRule(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();

            XmlElement rootEle = xmlDoc.CreateElement("ModelConfigs");
            for (int i = 0; i < modelSettings.Count; i++)
            {
                ModelSettingBean msb = modelSettings[i];
                XmlElement ele = xmlDoc.CreateElement("ModelSetting");
                msb.Write(xmlDoc , ele);
                rootEle.AppendChild(ele);
            }
           xmlDoc.AppendChild(rootEle);
           xmlDoc.Save(filePath);
        }


    }
}

