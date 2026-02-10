using System;

using JetBrains.Annotations;

using Rahmen.Logging;

using UnityEngine;

using Zenject;

namespace BoardGDK.Pieces
{
/// <summary>
/// <see cref="IPieceSettlingStrategy"/> that settles based on active frame count.
/// </summary>
[Serializable]
public class SettleByFrames : IPieceSettlingStrategy
{
    [SerializeField]
    [Tooltip("The number of frames a context must remain active to be considered settled.")]
    private int m_numFrames;

    private IRahmenLogger _logger;
    
    /// <inheritdoc />
    public bool HasSettled(IPieceSettlingContext context)
    {
        bool isSettled = context.NumFramesActive >= m_numFrames;

        if(isSettled)
        {
            _logger.Debug()?.Log($"Context settled. Target active frame count is <{m_numFrames}>.");
        }
        else
        {
            _logger.Trace()?.Log($"Context not settled. Target active frame count is <{m_numFrames}> but frame count is currently <{context.NumFramesActive}>.");
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
