using System;
using UnityEditor;
using UnityEngine;

namespace RSUVFramework.Editor
{
    [CustomEditor(typeof(RSUVSchema))]
    public sealed class RSUVSchemaEditor : UnityEditor.Editor
    {
        private static readonly GUIContent[] FIELD_TYPE_OPTIONS =
        {
            new GUIContent("Bool"),
            new GUIContent("Int"),
            new GUIContent("Float"),
            new GUIContent("Color"),
        };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawSettings();
            EditorGUILayout.Space();
            DrawFields();

            serializedObject.ApplyModifiedProperties();

            RSUVSchema schema = (RSUVSchema)target;
            EditorGUILayout.Space();

            if (RSUVSchemaUtility.TryResolve(schema, out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                EditorGUILayout.HelpBox($"Resolved layout: {resolvedSchema.UsedBitCount}/32 bits used, {resolvedSchema.FreeBitCount} free.", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Generate HLSL Bindings"))
                {
                    try
                    {
                        string hlslAssetPath = RSUVHlslGenerator.GenerateToDisk(schema);
                        Debug.Log($"Generated RSUV HLSL bindings at '{hlslAssetPath}'.", schema);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(exception.Message, schema);
                    }
                }

                if (GUILayout.Button("Generate C# Bindings"))
                {
                    try
                    {
                        string csharpAssetPath = RSUVCSharpGenerator.GenerateToDisk(schema);
                        Debug.Log($"Generated RSUV C# bindings at '{csharpAssetPath}'.", schema);
                    }
                    catch (Exception exception)
                    {
                        Debug.LogError(exception.Message, schema);
                    }
                }

                if (GUILayout.Button("Validate"))
                {
                    if (RSUVSchemaUtility.TryResolve(schema, out _, out _))
                    {
                        Debug.Log($"Schema '{schema.name}' is valid.", schema);
                    }
                }
            }
        }

        private void DrawSettings()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_namingPrefix"));
            DrawGenerationSettings();
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_autoGenerateOnChange"));
        }

        private static void DrawGenerationSettings()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Shared Generation Settings", EditorStyles.boldLabel);

                if (RSUVGenerationSettingsUtility.TryGetSettings(out RSUVGenerationSettings settings))
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField("Settings Asset", settings, typeof(RSUVGenerationSettings), false);
                    }

                    EditorGUILayout.LabelField("HLSL Bindings Directory", settings.HlslBindingsDirectory);
                    EditorGUILayout.LabelField("C# Bindings Directory", settings.CSharpBindingsDirectory);

                    if (GUILayout.Button("Select Generation Settings"))
                    {
                        Selection.activeObject = settings;
                        EditorGUIUtility.PingObject(settings);
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "No RSUV generation settings asset was found. The default shared directories will be used until you create one.",
                        MessageType.Info);

                    EditorGUILayout.LabelField("HLSL Bindings Directory", RSUVGenerationSettings.DEFAULT_HLSL_BINDINGS_DIRECTORY);
                    EditorGUILayout.LabelField("C# Bindings Directory", RSUVGenerationSettings.DEFAULT_CSHARP_BINDINGS_DIRECTORY);

                    if (GUILayout.Button("Create Generation Settings Asset"))
                    {
                        RSUVGenerationSettingsUtility.CreateOrSelectSettingsAsset();
                    }
                }
            }
        }

        private void DrawFields()
        {
            SerializedProperty fieldsProperty = serializedObject.FindProperty("_fields");

            EditorGUILayout.LabelField("Fields", EditorStyles.boldLabel);

            for (int i = 0; i < fieldsProperty.arraySize; i++)
            {
                SerializedProperty fieldProperty = fieldsProperty.GetArrayElementAtIndex(i);
                DrawField(fieldProperty, i, fieldsProperty);
                EditorGUILayout.Space();
            }

            if (GUILayout.Button("Add Field"))
            {
                fieldsProperty.InsertArrayElementAtIndex(fieldsProperty.arraySize);
                SerializedProperty fieldProperty = fieldsProperty.GetArrayElementAtIndex(fieldsProperty.arraySize - 1);
                fieldProperty.FindPropertyRelative("_name").stringValue = "Field";
                fieldProperty.FindPropertyRelative("_fieldType").intValue = (int)RSUVFieldType.Int;
                fieldProperty.FindPropertyRelative("_bitCount").intValue = 1;
                fieldProperty.FindPropertyRelative("_maximumIntegerValue").intValue = 1;
                fieldProperty.FindPropertyRelative("_maximumFloatValue").floatValue = 1f;
                fieldProperty.FindPropertyRelative("_defaultColorValue").colorValue = Color.white;
            }
        }

        private void DrawField(SerializedProperty fieldProperty, int index, SerializedProperty fieldsProperty)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"Field {index + 1}", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_name"));

                SerializedProperty fieldTypeProperty = fieldProperty.FindPropertyRelative("_fieldType");
                int selectedIndex = GetFieldTypeIndex((RSUVFieldType)fieldTypeProperty.intValue);
                int nextIndex = EditorGUILayout.Popup(new GUIContent("Field Type"), selectedIndex, FIELD_TYPE_OPTIONS);
                fieldTypeProperty.intValue = (int)GetFieldTypeFromIndex(nextIndex);

                EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_bitCount"));

                DrawTypeSpecificFields(fieldProperty, GetFieldTypeFromIndex(nextIndex));
                DrawFieldButtons(index, fieldsProperty);
            }
        }

        private void DrawTypeSpecificFields(SerializedProperty fieldProperty, RSUVFieldType fieldType)
        {
            switch (fieldType)
            {
                case RSUVFieldType.Bool:
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_defaultBooleanValue"));
                    break;

                case RSUVFieldType.Int:
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_minimumIntegerValue"));
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_maximumIntegerValue"));
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_defaultIntegerValue"));
                    break;

                case RSUVFieldType.Float:
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_minimumFloatValue"));
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_maximumFloatValue"));
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_defaultFloatValue"));
                    break;

                case RSUVFieldType.Color:
                    EditorGUILayout.HelpBox("Color fields split their bit count evenly across RGBA channels.", MessageType.Info);
                    EditorGUILayout.PropertyField(fieldProperty.FindPropertyRelative("_defaultColorValue"));
                    break;
            }
        }

        private void DrawFieldButtons(int index, SerializedProperty fieldsProperty)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.enabled = index > 0;
                if (GUILayout.Button("Move Up"))
                {
                    fieldsProperty.MoveArrayElement(index, index - 1);
                }

                GUI.enabled = index < (fieldsProperty.arraySize - 1);
                if (GUILayout.Button("Move Down"))
                {
                    fieldsProperty.MoveArrayElement(index, index + 1);
                }

                GUI.enabled = true;
                if (GUILayout.Button("Remove"))
                {
                    fieldsProperty.DeleteArrayElementAtIndex(index);
                }
            }

            GUI.enabled = true;
        }

        private static int GetFieldTypeIndex(RSUVFieldType fieldType)
        {
            switch (fieldType)
            {
                case RSUVFieldType.Bool:
                    return 0;

                case RSUVFieldType.Int:
                    return 1;

                case RSUVFieldType.Float:
                    return 2;

                case RSUVFieldType.Color:
                    return 3;

                default:
                    return 1;
            }
        }

        private static RSUVFieldType GetFieldTypeFromIndex(int index)
        {
            switch (index)
            {
                case 0:
                    return RSUVFieldType.Bool;

                case 1:
                    return RSUVFieldType.Int;

                case 2:
                    return RSUVFieldType.Float;

                case 3:
                    return RSUVFieldType.Color;

                default:
                    return RSUVFieldType.Int;
            }
        }
    }
}