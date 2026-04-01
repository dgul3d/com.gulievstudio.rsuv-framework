using UnityEditor;
using UnityEngine;

namespace RSUVFramework.Editor
{
    [CustomEditor(typeof(RSUVRendererValueWriter))]
    public sealed class RSUVRendererValueWriterEditor : UnityEditor.Editor
    {
        private SerializedProperty _schemaProperty;
        private SerializedProperty _renderersProperty;
        private SerializedProperty _fieldValuesProperty;

        private void OnEnable()
        {
            _schemaProperty = serializedObject.FindProperty("_schema");
            _renderersProperty = serializedObject.FindProperty("_renderers");
            _fieldValuesProperty = serializedObject.FindProperty("_fieldValues");

            RefreshTargets();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(_schemaProperty);
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                RefreshTargets();
                serializedObject.Update();
            }

            EditorGUILayout.PropertyField(_renderersProperty, true);
            EditorGUILayout.Space();

            RSUVRendererValueWriter writer = (RSUVRendererValueWriter)target;
            if (!writer.TryGetResolvedSchema(out RSUVResolvedSchema resolvedSchema, out string errorMessage))
            {
                serializedObject.ApplyModifiedProperties();

                if (_schemaProperty.objectReferenceValue != null)
                {
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                }

                return;
            }

            if (_fieldValuesProperty.arraySize != resolvedSchema.Fields.Count)
            {
                serializedObject.ApplyModifiedProperties();
                RefreshTargets();
                serializedObject.Update();
            }

            EditorGUILayout.LabelField("Schema Values", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox($"Resolved layout: {resolvedSchema.UsedBitCount}/32 bits used, {resolvedSchema.FreeBitCount} free.", MessageType.Info);

            EditorGUI.BeginChangeCheck();

            for (int i = 0; i < resolvedSchema.Fields.Count; i++)
            {
                RSUVResolvedField field = resolvedSchema.Fields[i];
                SerializedProperty fieldValueProperty = _fieldValuesProperty.GetArrayElementAtIndex(i);
                DrawField(field, fieldValueProperty);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                ApplyTargets();
                serializedObject.Update();
            }
            else
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Packed Value", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel($"0x{writer.GetPackedValue():X8}", EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset To Defaults"))
                {
                    ResetTargets();
                    serializedObject.Update();
                }

                if (GUILayout.Button("Apply Values"))
                {
                    ApplyTargets();
                    serializedObject.Update();
                }
            }
        }

        private void DrawField(RSUVResolvedField field, SerializedProperty fieldValueProperty)
        {
            string label = $"{field.Name} ({field.BitCount} bit";
            if (field.BitCount != 1)
            {
                label += "s";
            }

            label += ")";

            switch (field.FieldType)
            {
                case RSUVFieldType.Bool:
                    SerializedProperty booleanProperty = fieldValueProperty.FindPropertyRelative("_booleanValue");
                    booleanProperty.boolValue = EditorGUILayout.Toggle(label, booleanProperty.boolValue);
                    break;

                case RSUVFieldType.Int:
                    DrawIntField(label, field, fieldValueProperty.FindPropertyRelative("_integerValue"));
                    break;

                case RSUVFieldType.Float:
                    SerializedProperty floatProperty = fieldValueProperty.FindPropertyRelative("_floatValue");
                    floatProperty.floatValue = EditorGUILayout.Slider(label, floatProperty.floatValue, field.MinimumFloatValue, field.MaximumFloatValue);
                    break;

                case RSUVFieldType.Color:
                    SerializedProperty colorProperty = fieldValueProperty.FindPropertyRelative("_colorValue");
                    colorProperty.colorValue = EditorGUILayout.ColorField(new GUIContent(label), colorProperty.colorValue, true, true, false);
                    break;
            }
        }

        private void DrawIntField(string label, RSUVResolvedField field, SerializedProperty integerProperty)
        {
            integerProperty.intValue = EditorGUILayout.IntSlider(label, integerProperty.intValue, field.MinimumIntegerValue, field.MaximumIntegerValue);
        }

        private void ApplyTargets()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                RSUVRendererValueWriter writer = (RSUVRendererValueWriter)targets[i];
                Undo.RecordObject(writer, "Apply RSUV Values");
                writer.ApplySerializedValues();
                EditorUtility.SetDirty(writer);
            }
        }

        private void RefreshTargets()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                RSUVRendererValueWriter writer = (RSUVRendererValueWriter)targets[i];
                Undo.RecordObject(writer, "Refresh RSUV Fields");
                writer.RefreshSerializedFields();
                EditorUtility.SetDirty(writer);
            }
        }

        private void ResetTargets()
        {
            for (int i = 0; i < targets.Length; i++)
            {
                RSUVRendererValueWriter writer = (RSUVRendererValueWriter)targets[i];
                Undo.RecordObject(writer, "Reset RSUV Fields");
                writer.RebuildFromSchemaDefaults();
                EditorUtility.SetDirty(writer);
            }
        }
    }
}