using System;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces
{
/// <summary>
/// <see cref="IPieceSettlingStrategy"/> that requires contexts remain within a certain distance from their initial
/// position while settling.
/// </summary>
[Serializable]
public class SettleByInitialPosition : IPieceSettlingStrategy
{
    [SerializeField]
    [Tooltip("The radius (in screen space coordinates) within which the context must remain while settling.")]
    private float m_radius;
    
    private IRahmenLogger _logger;
    
    /// <inheritdoc />
    public bool HasSettled(IPieceSettlingContext context)
    {
        float distance = Vector2.Distance(context.InitialScreenPosition, context.Contact.ScreenPosition);
        bool isSettled = distance < m_radius;

        if(isSettled)
        {
            _logger.Debug()?.Log($"Context settled. Piece has stayed within the initial range radius of <{m_radius}>.");
        }
        else
        {
            _logger.Trace()?.Log($"Context not settled. Piece has exceeded the initial range radius of <{m_radius}> by <{distance - m_radius}>.");
        }
        
        return isSettled;
    }

    [Inject]
    private void Injection([NotNull] ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.Get<LogChannels.PieceSystem>(this);
    }
}
}
