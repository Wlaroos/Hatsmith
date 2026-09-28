using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TileMapping))]
public class TileMappingPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        SerializedProperty tagProp = property.FindPropertyRelative("tag");
        SerializedProperty colorProp = property.FindPropertyRelative("color");
        SerializedProperty typeProp = property.FindPropertyRelative("type");
        SerializedProperty prefabProp = property.FindPropertyRelative("prefab");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // Draw Foldout
        Rect foldoutRect = new Rect(position.x, position.y, position.width, lineHeight);
        property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            float currentY = position.y + lineHeight + spacing;

            // Draw Tag
            Rect tagRect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(tagRect, tagProp);
            currentY += lineHeight + spacing;

            // Draw Color
            Rect colorRect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(colorRect, colorProp);
            currentY += lineHeight + spacing;

            // Draw Type
            Rect typeRect = new Rect(position.x, currentY, position.width, lineHeight);
            EditorGUI.PropertyField(typeRect, typeProp);
            currentY += lineHeight + spacing;

            // Conditionally draw Prefab Field using its native PropertyField height
            TileType currentType = (TileType)typeProp.enumValueIndex;
            if (currentType == TileType.Prefab)
            {
                float prefabHeight = EditorGUI.GetPropertyHeight(prefabProp, true);
                Rect prefabRect = new Rect(position.x, currentY, position.width, prefabHeight);
                EditorGUI.PropertyField(prefabRect, prefabProp, true);
            }

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        // Header + Tag + Color + Type
        float totalHeight = (lineHeight * 4) + (spacing * 3);

        SerializedProperty typeProp = property.FindPropertyRelative("type");
        if (typeProp != null && (TileType)typeProp.enumValueIndex == TileType.Prefab)
        {
            SerializedProperty prefabProp = property.FindPropertyRelative("prefab");
            if (prefabProp != null)
            {
                totalHeight += EditorGUI.GetPropertyHeight(prefabProp, true) + spacing;
            }
        }

        return totalHeight;
    }
}