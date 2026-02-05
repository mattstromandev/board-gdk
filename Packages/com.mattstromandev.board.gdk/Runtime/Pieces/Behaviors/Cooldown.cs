using System;

using BoardGDK.Data;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces.Behaviors
{
/// <summary>
/// <see cref="PieceBehavior"/> providing cooldown functionality for pieces.
/// </summary>
[Serializable]
public class Cooldown : PieceBehavior, ITickable
{
    [SerializeField]
    [Tooltip("The cooldown time in seconds.")]
    private float m_cooldownTime;
    
    [Tooltip("Whether to start a cooldown upon place of the Piece.")]
    [SerializeField]
    private bool m_cooldownOnPlace;
    
    [Tooltip("Whether to start a cooldown upon activation of the behavior.")]
    [SerializeField]
    private bool m_cooldownOnActivate;

    [SerializeField]
    [Tooltip("Optional variable to track remaining cooldown time.")]
    private FloatVariable m_cooldownTimeRemaining;

    [SerializeField]
    [Tooltip("Optional variable to track if the cooldown is currently active.")]
    private BoolVariable m_isCooldownActive;
    
    /// <summary>
    /// The remaining time (seconds) until the cooldown is complete.
    /// </summary>
    public float CooldownTimeRemaining
    {
        get => _cooldownTimeRemaining;
        private set
        {
            if(m_cooldownTimeRemaining != null) { m_cooldownTimeRemaining.Value = value; }
            if(m_isCooldownActive != null) { m_isCooldownActive.Value = value > 0; }

            _cooldownTimeRemaining = value;
        }
    }

    /// <summary>
    /// Flag indicating whether the cooldown is currently active.
    /// </summary>
    public bool IsCooldownActive => _cooldownTimeRemaining > 0;

    private float _cooldownTimeRemaining;

    /// <summary>
    /// Update this behavior via the <see cref="ITickable"/>.
    /// </summary>
    /// <remarks>
    /// We need the cooldown to update even if the piece is not active or on Board, to finish the cooldown.
    /// </remarks>
    public void Tick()
    {
        if(IsCooldownActive == false) { return; }
        
        CooldownTimeRemaining -= Time.deltaTime;
    }

    /// <inheritdoc />
    protected override void OnPlaced(PieceBehaviorContext context)
    {
        if(IsCooldownActive) { return; }
        
        if(m_cooldownOnPlace)
        {
            CooldownTimeRemaining = m_cooldownTime;
        }
    }

    /// <inheritdoc />
    protected override void OnActivate(PieceBehaviorContext context)
    {
        if(IsCooldownActive) { return; }

        if(m_cooldownOnActivate)
        {
            CooldownTimeRemaining = m_cooldownTime;
        }
    }
}
}
