using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(SubclassInstanceAttribute), true)]
public class SubclassInstancePropertyDrawer : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        var container = new VisualElement();
        
        var parentType = property.serializedObject.targetObject.GetType();
        var path = property.propertyPath.Split('.')[0];
        var field = parentType.GetField(path, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        var type = field.FieldType.IsGenericType
            ? field.FieldType.GenericTypeArguments[0]
            : field.FieldType.GetElementType();
        
        var types = TypeCache.GetTypesDerivedFrom(type)
            .Where(t => !t.IsAbstract)
            .ToList();

        var currentType = property.managedReferenceValue?.GetType();
        
        var dropdown = new DropdownField("Type", types.Select(t => t.Name).ToList(), currentType == null ? 0 : types.IndexOf(currentType));
        
        var propertyField = new PropertyField(property);
        propertyField.Bind(property.serializedObject);
        
        if (currentType == null)
            CreateObject(property, types[0], propertyField);
        
        dropdown.RegisterValueChangedCallback((_) => CreateObject(property, types[dropdown.index], propertyField));
        
        container.Add(dropdown);
        container.Add(propertyField);
        
        return container;
    }

    private void CreateObject(SerializedProperty property, Type type, PropertyField propertyField)
    {
        if (property.managedReferenceValue?.GetType() == type)
            return;

        property.managedReferenceValue = Activator.CreateInstance(type);
        property.serializedObject.ApplyModifiedProperties();
        propertyField.Bind(property.serializedObject);
    }
}
