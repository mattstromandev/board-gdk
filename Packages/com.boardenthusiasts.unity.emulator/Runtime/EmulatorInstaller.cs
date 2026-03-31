using BoardGDK.BoardAdapters;

using UnityEngine;

using Zenject;

namespace BE.Emulator
{
/// <summary>
/// ScriptableObject installer entry point for the BE Emulator for Board package.
/// </summary>
[CreateAssetMenu(menuName = "Board Enthusiasts/Emulator/Emulator Installer")]
public class EmulatorInstaller : ScriptableObjectInstaller<EmulatorInstaller>
{
    [SerializeField]
    private EmulatorSettings m_settings;

    /// <inheritdoc />
    public override void InstallBindings()
    {
        if(Container.HasBinding<IBoard>() == false)
        {
            Container.Bind<IBoard>()
                .FromSubContainerResolve()
                .ByNewGameObjectMethod(InstallBoardSubcontainer)
                .AsSingle()
                .NonLazy();

            Container.Bind<IBoardApplication>().FromResolveGetter<IBoard>(board => board.Application).AsSingle();
            Container.Bind<IBoardSession>().FromResolveGetter<IBoard>(board => board.Session).AsSingle();
            Container.Bind<IBoardSaveGameManager>().FromResolveGetter<IBoard>(board => board.SaveGameManager).AsSingle();
        }
    }

    private void InstallBoardSubcontainer(DiContainer subContainer)
    {
        EmulatorSubcontainerInstaller.Install(subContainer, m_settings);
    }
}
}
