using System;
using System.Collections.Generic;

using BoardGDK.Pieces;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace BoardGDK.Editor.Pieces
{
/// <summary>
/// Read-only inspector for <see cref="VirtualPiece"/> and its subclasses.
/// </summary>
[CustomEditor(typeof(VirtualPiece), true)]
public class VirtualPieceEditor : UnityEditor.Editor
{
    /// <inheritdoc />
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = new();
        InspectorElement.FillDefaultInspector(root, serializedObject, this);

        root.RegisterCallback<GeometryChangedEvent, VisualElement>(OnGeometryChangedEvent, root);

        return root;
    }

    private static void OnGeometryChangedEvent(GeometryChangedEvent _, VisualElement root)
    {
        HashSet<Toggle> foldoutToggles = new();

        root.Query<Foldout>().ForEach(foldout =>
        {
            Toggle toggle = foldout.Q<Toggle>(className: Foldout.toggleUssClassName);

            if(toggle != null) { foldoutToggles.Add(toggle); }
        });

        root.Query<VisualElement>().ForEach(element =>
        {
            switch(element)
            {
            case Toggle toggle when foldoutToggles.Contains(toggle):
                toggle.SetEnabled(true);

                return;
            case ListView listView:
                listView.reorderable = false;
                listView.selectionType = SelectionType.None;

                return;
            default:
                if(IsInputElement(element))
                {
                    element.SetEnabled(false);
                }

                break;
            }
        });
    }

    private static bool IsInputElement(VisualElement element)
    {
        return element is Button or IMGUIContainer || IsBaseField(element);
    }

    private static bool IsBaseField(VisualElement element)
    {
        for(Type type = element.GetType(); type != null; type = type.BaseType)
        {
            if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseField<>)) { return true; }
        }

        return false;
    }
}
}
