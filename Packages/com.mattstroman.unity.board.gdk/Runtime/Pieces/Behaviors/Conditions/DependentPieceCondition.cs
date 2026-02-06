using System;

using UnityEngine;

namespace BoardGDK.Pieces.Behaviors.Conditions
{
/// <summary>
/// Base class for an <see cref="IPieceBehaviorCondition"/> which depends on another piece's behavior.
/// </summary>
[Serializable]
public abstract class DependentPieceCondition : PieceBehaviorCondition
{
    /// <summary>
    /// The <see cref="PieceBehaviorDefinition"/> on which this condition depends.
    /// </summary>
    [field: Tooltip("The definition for piece behavior on which this condition depends.")]
    [field: SerializeField]
    protected PieceBehaviorDefinition DependentPieceBehavior { get; private set; }

    /// <inheritdoc />
    public override bool Evaluate(PieceBehaviorContext context)
    {
        if(DependentPieceBehavior == null)
        {
            // TODO: make two utilities for debuggability: log full context and locate source asset and index in list (if applicable) for where the null reference is. Add a link in the log to ping the asset, if located.
            // see this ChatGPT response for an example of how to do this: https://chatgpt.com/share/6985efee-c378-800b-a8b3-ab68765bb190
            UnityEngine.Debug.LogWarning($"{GetType().Name}: {nameof(DependentPieceBehavior)} is null; unable to evaluate condition.");
            return false;
        }

        return base.Evaluate(context);
    }
}
}
