using System;
using System.Collections.Generic;
using System.Linq;

using Board.Input;

using BoardGDK.Pieces;
using BoardGDK.Pieces.Attributes;

using UnityEditor;
using UnityEditor.UIElements;

using UnityEngine.UIElements;

namespace BoardGDK.Editor.Pieces.PropertyDrawers
{
/// <summary>
/// <see cref="PropertyDrawer"/> for the <see cref="PieceNameAttribute"/> that displays a dropdown of piece names
/// mapped to glyph IDs in the active <see cref="PieceSetInputSettings"/>. This is purely for readability in the Inspector;
/// the actual stored value remains the glyph ID integer.
/// </summary>
[CustomPropertyDrawer(typeof(PieceNameAttribute))]
public class PieceNameAttributeDrawer : PropertyDrawer
{
    private const string _noMappingValue = "<No Mapping Defined>";
    
    /// <inheritdoc />
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        VisualElement root = new();
        
        PropertyField propertyField = new(property);
        propertyField.Bind(property.serializedObject);
        root.Add(propertyField);

        HelpBox helpBox = new()
        {
            messageType = HelpBoxMessageType.Info,
            style =
            {
                display = DisplayStyle.None
            }
        };
        root.Add(helpBox);
        
        root.RegisterCallbackOnce<GeometryChangedEvent, (SerializedProperty property, PropertyField propertyField, HelpBox helpBox)>((_, args) =>
        {
            // Piece name dropdown is added inside the input field for a clean look
            VisualElement inputContainer = args.propertyField.Q<VisualElement>(className: PropertyField.inputUssClassName);
            
            if(BoardInput.settings is not PieceSetInputSettings pieceSetInputSettings)
            {
                args.helpBox.text = $"The active input settings ({BoardInput.settings.name}) is not a {nameof(PieceSetInputSettings)} type, so no glyph ID mapping is available. Consider using a {nameof(PieceSetInputSettings)} to improve readability.";
                helpBox.style.display = DisplayStyle.Flex;

                return;
            }

            IReadOnlyDictionary<int, string> pieceNamesByGlyphID = pieceSetInputSettings.GlyphIDMapping;
            
            // Build dropdown choices. Ensure names are unique or disambiguate if needed.
            List<KeyValuePair<int, string>> pairs = pieceNamesByGlyphID.OrderBy(kvp => kvp.Value, StringComparer.OrdinalIgnoreCase).ToList();
            List<string> dropdownChoices = new(pairs.Count + 1) { _noMappingValue };
            dropdownChoices.AddRange(pairs.Select(p => p.Value));
            Dictionary<string, int> glyphIDsByDropdownChoice = pairs.ToDictionary(p => p.Value, p => p.Key);

            // Create dropdown with current selection
            DropdownField dropdownField = new(dropdownChoices, GetPieceName(args.property.intValue));

            int currentGlyphID = args.property.intValue;
            string noMappingForGlyphIDMessage = $"The active input settings ({pieceSetInputSettings.name}) does not have a mapping for this glyph ID. Consider adding a mapping to improve readability.";
            if(pieceNamesByGlyphID.ContainsKey(currentGlyphID) == false)
            {
                args.helpBox.text = noMappingForGlyphIDMessage;
                helpBox.style.display = DisplayStyle.Flex;
            }
        
            inputContainer.Add(dropdownField);
            
            bool updatingProperty = false;

            // Glygh ID changes => update dropdown
            args.propertyField.RegisterValueChangeCallback(evt =>
            {
                if(updatingProperty) { return; }

                updatingProperty = true;
                int newGlyphID = evt.changedProperty.intValue;
                
                SetGlyphID(newGlyphID);

                string pieceName = GetPieceName(newGlyphID);
                dropdownField.SetValueWithoutNotify(pieceName);

                if(pieceName == _noMappingValue)
                {
                    args.helpBox.text = noMappingForGlyphIDMessage;
                    helpBox.style.display = DisplayStyle.Flex;
                }
                else
                {
                    helpBox.style.display = DisplayStyle.None;
                }
                
                updatingProperty = false;
            });
            
            // Dropdown changes => update glyph ID
            dropdownField.RegisterValueChangedCallback(evt =>
            {
                if(updatingProperty) { return; }

                updatingProperty = true;
                string newPieceName = evt.newValue;

                if(newPieceName == _noMappingValue)
                {
                    return;
                }

                if(glyphIDsByDropdownChoice.TryGetValue(newPieceName, out int newGlyphID))
                {
                    SetGlyphID(newGlyphID);
                    helpBox.style.display = DisplayStyle.None;
                }
                
                updatingProperty = false;
            });

            return;

            void SetGlyphID(int newValue)
            {
                args.property.serializedObject.Update();
                args.property.intValue = newValue;
                args.property.serializedObject.ApplyModifiedProperties();
            }

            string GetPieceName(int glyphID) => pieceNamesByGlyphID.GetValueOrDefault(glyphID, _noMappingValue);
        }, (property, propertyField, helpBox));
        
        return root;
    }
}
}
