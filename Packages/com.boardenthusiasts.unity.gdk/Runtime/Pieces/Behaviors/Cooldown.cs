using System;

using BoardGDK.Data;

using JetBrains.Annotations;

using Rahmen.Logging;

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

    [Tooltip("The behavior lifecycle mask indicating in what states to cooldown should be started.")]
    [SerializeField]
    private PieceBehaviorLifecycleMask m_cooldownOn = PieceBehaviorLifecycleMask.Activate;

    [Tooltip(
        "Flag indicating whether the cooldown should persist when the piece is picked up. If false, picking up the piece will reset the cooldown; otherwise, the cooldown will continue to apply (even if the piece is placed again) until it is done."
    )]
    [SerializeField]
    private bool m_persistCooldownOnPickUp = true;

    // TODO: because a single behavior can be applied to multiple pieces, we need to track cooldown time remaining and
    // active state per piece. These single variables need to be refactored to account for that.
    [SerializeField]
    [Tooltip("Optional variable to track remaining cooldown time.")]
    private FloatVariable m_cooldownTimeRemaining;

    [SerializeField]
    [Tooltip("Optional variable to track if the cooldown is currently active.")]
    private BoolVariable m_isCooldownActive;

    private const string _cooldownTimeRemainingKey = "CooldownTimeRemaining";
    private IRahmenLogger _logger;
    private bool _isOnBoard;

    /// <summary>
    /// Update this behavior via the <see cref="ITickable"/>.
    /// </summary>
    /// <remarks>
    /// We need the cooldown to update even if the piece is not active or on Board, to finish the cooldown.
    /// </remarks>
    public void Tick()
    {
        // TODO: need to figure out a way to account for tracking cooldown after the piece is picked up, without having
        // cooldowns for different pieces overwrite each other because, without an active piece on Board, there is no
        // behavior context nor state. Will likely have to do some sort of static storage solution keying off of the full
        // unique ID of the piece (i.e. contactID, glyphID, and behavior instance)
    }

    /// <inheritdoc />
    public override void Place(PieceBehaviorContext context)
    {
        context.State.SetValue(_cooldownTimeRemainingKey, 0f);
        if(m_cooldownOn.HasFlag(PieceBehaviorLifecycleMask.Place)) { StartCooldown(context); }
    }

    /// <inheritdoc />
    public override void Activate(PieceBehaviorContext context)
    {
        if(m_cooldownOn.HasFlag(PieceBehaviorLifecycleMask.Activate)) { StartCooldown(context); }
    }

    /// <inheritdoc />
    public override void Update(PieceBehaviorContext context)
    {
        if(IsCooldownActive(context) == false) { return; }

        SetCooldownTimeRemaining(context, GetCooldownTimeRemaining(context) - Time.deltaTime);
    }

    /// <inheritdoc />
    public override void Deactivate(PieceBehaviorContext context)
    {
        if(m_cooldownOn.HasFlag(PieceBehaviorLifecycleMask.Deactivate)) { StartCooldown(context); }
    }

    /// <inheritdoc />
    public override void PickUp(PieceBehaviorContext context)
    {
        if(m_cooldownOn.HasFlag(PieceBehaviorLifecycleMask.PickUp)) { StartCooldown(context); }

        if(m_persistCooldownOnPickUp)
        {
            // TODO: add the necessary logic to persist cooldowns
        }
    }

    [Inject]
    private void Injection([NotNull] ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.Get<LogChannels.PieceBehaviorSystem>(this);

        if(m_cooldownOn.HasFlag(PieceBehaviorLifecycleMask.Update))
        {
            _logger.Warning()?.Log(
                $"Cooldown behavior does not support starting cooldown on during update because the cooldown would be constantly reset. Please remove the {nameof(PieceBehaviorLifecycleMask.Update)} flag from the {nameof(m_cooldownOn)} mask."
            );
        }
    }

    private void StartCooldown(PieceBehaviorContext context)
    {
        if(IsCooldownActive(context)) { return; }

        _logger.Debug()?.Log($"Starting cooldown for <{m_cooldownTime}> seconds.");

        context.State.SetValue(_cooldownTimeRemainingKey, m_cooldownTime);
    }

    private bool IsCooldownActive(PieceBehaviorContext context) { return GetCooldownTimeRemaining(context) > 0; }

    private float GetCooldownTimeRemaining(PieceBehaviorContext context)
    {
        return context.State.TryGetValue(_cooldownTimeRemainingKey, out float cooldownTimeRemaining)
            ? cooldownTimeRemaining
            : 0;
    }

    private void SetCooldownTimeRemaining(PieceBehaviorContext context, float value)
    {
        if(m_cooldownTimeRemaining != null) { m_cooldownTimeRemaining.Value = value; }
        if(m_isCooldownActive != null) { m_isCooldownActive.Value = value > 0; }

        context.State.SetValue(_cooldownTimeRemainingKey, value);
    }
}
}
