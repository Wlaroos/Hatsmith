using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HatData))]
public class HatDataEditor : Editor
{
    private Type[] triggerTypes;
    private Type[] effectTypes;
    private string[] triggerNames;
    private string[] effectNames;

    private int selectedTriggerIndex = 0;
    private int selectedEffectIndex = 0;

    private void OnEnable()
    {
        RefreshTypes();
    }

    private void RefreshTypes()
    {
        triggerTypes = GetDerivedTypes(typeof(HatTrigger));
        triggerNames = triggerTypes?.Select(t => t.Name).ToArray() ?? new string[0];

        effectTypes = GetDerivedTypes(typeof(HatEffect));
        effectNames = effectTypes?.Select(t => t.Name).ToArray() ?? new string[0];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (triggerTypes == null || effectTypes == null || triggerNames == null || effectNames == null)
        {
            RefreshTypes();
        }

        DrawPropertiesExcluding(serializedObject, "rules");

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Hat Rules (Triggers & Effects)", EditorStyles.boldLabel);

        SerializedProperty rulesProperty = serializedObject.FindProperty("rules");

        if (rulesProperty == null)
        {
            EditorGUILayout.HelpBox("Could not find 'rules' property on HatData.", MessageType.Error);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        int indexToRemove = -1;

        for (int i = 0; i < rulesProperty.arraySize; i++)
        {
            SerializedProperty ruleProp = rulesProperty.GetArrayElementAtIndex(i);
            if (ruleProp == null) continue;

            SerializedProperty triggerProp = ruleProp.FindPropertyRelative("trigger");
            SerializedProperty effectProp = ruleProp.FindPropertyRelative("effect");

            string triggerName = triggerProp?.managedReferenceValue?.GetType().Name ?? "None";
            string effectName = effectProp?.managedReferenceValue?.GetType().Name ?? "None";

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField($"Rule #{i + 1}: [{triggerName} ➔ {effectName}]", EditorStyles.boldLabel);

            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                indexToRemove = i;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            if (triggerProp != null && triggerProp.managedReferenceValue != null)
            {
                EditorGUILayout.PropertyField(triggerProp, new GUIContent("Trigger"), true);
            }
            if (effectProp != null && effectProp.managedReferenceValue != null)
            {
                EditorGUILayout.PropertyField(effectProp, new GUIContent("Effect"), true);
            }
            EditorGUI.indentLevel--;

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(2);
        }

        if (indexToRemove >= 0)
        {
            rulesProperty.DeleteArrayElementAtIndex(indexToRemove);
        }

        EditorGUILayout.Space(10);

        if (triggerTypes.Length == 0 || effectTypes.Length == 0)
        {
            EditorGUILayout.HelpBox("No concrete HatTrigger or HatEffect implementations found in project.", MessageType.Warning);
            if (GUILayout.Button("Refresh Type Cache"))
            {
                RefreshTypes();
            }
        }
        else
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("Add New Rule", EditorStyles.boldLabel);

            selectedTriggerIndex = Mathf.Clamp(selectedTriggerIndex, 0, triggerNames.Length - 1);
            selectedEffectIndex = Mathf.Clamp(selectedEffectIndex, 0, effectNames.Length - 1);

            selectedTriggerIndex = EditorGUILayout.Popup("Trigger Type", selectedTriggerIndex, triggerNames);
            selectedEffectIndex = EditorGUILayout.Popup("Effect Type", selectedEffectIndex, effectNames);

            if (GUILayout.Button("Add Rule"))
            {
                // Instantiate directly on target object to prevent Unity's array duplicator from defaulting to null references
                HatData targetData = (HatData)target;
                
                Type chosenTriggerType = triggerTypes[selectedTriggerIndex];
                Type chosenEffectType = effectTypes[selectedEffectIndex];

                HatEffectRule newRule = new HatEffectRule
                {
                    trigger = (HatTrigger)Activator.CreateInstance(chosenTriggerType),
                    effect = (HatEffect)Activator.CreateInstance(chosenEffectType)
                };

                Undo.RecordObject(targetData, "Add Hat Rule");
                targetData.rules.Add(newRule);
                EditorUtility.SetDirty(targetData);
                
                serializedObject.Update();
            }
            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static Type[] GetDerivedTypes(Type baseType)
    {
        return TypeCache.GetTypesDerivedFrom(baseType)
            .Where(t => !t.IsAbstract && !t.IsInterface)
            .ToArray();
    }
}