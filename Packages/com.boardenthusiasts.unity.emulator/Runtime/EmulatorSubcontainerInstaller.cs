using System;
using System.Collections.Generic;

using BoardGDK.BoardAdapters;

using BE.Emulator.Actions;
using BE.Emulator.Framework;
using BE.Emulator.Modals.AddPlayer;
using BE.Emulator.Modals.DeleteProfile;
using BE.Emulator.Modals.ManageProfiles;
using BE.Emulator.Modals;
using BE.Emulator.Modals.EditProfile;
using BE.Emulator.Modals.ProfileSettings;
using BE.Emulator.Modals.QuickSettings;
using BE.Emulator.Modals.ReplacePlayer;
using BE.Emulator.Screens;
using BE.Emulator.Screens.ProfileSwitcher;
using BE.Emulator.Services;
using BE.Emulator.Utility;

using JetBrains.Annotations;

using Zenject;

namespace BE.Emulator
{
/// <summary>
/// Installs the emulator shell, displays, and Board facade services into the Board subcontainer.
/// </summary>
internal sealed class EmulatorSubcontainerInstaller : Installer<EmulatorSettings, EmulatorSubcontainerInstaller>
{
    private readonly EmulatorSettings _settings;

    /// <summary>
    /// Creates the subcontainer installer.
    /// </summary>
    /// <param name="settings">The settings asset used to configure the emulator shell and displays.</param>
    public EmulatorSubcontainerInstaller(EmulatorSettings settings)
    {
        _settings = settings;
    }

    /// <inheritdoc />
    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<EmulatorSettings>().FromInstance(_settings).AsSingle();
        Container.Bind<BoardSdkObjectFactory>().AsSingle();

#if UNITY_EDITOR
        BindViewModels();
        BindFrameworkServices();
        BindDisplays();
        BindActionRoutes();
        BindShell();
#endif

        BindBoardServices();
    }

#if UNITY_EDITOR
    private void BindViewModels()
    {
        Container.Bind<EmulatorViewModel>().FromInstance(new EmulatorViewModel
        {
            PanelSettings = _settings.PanelSettings,
            SortOrder = _settings.SortOrder,
            SourceTemplate = _settings.SourceTemplate
        }).AsSingle();

        Container.Bind<ProfileSwitcherViewModel>().FromInstance(new ProfileSwitcherViewModel
        {
            SourceTemplate = _settings.ProfileSwitcherTemplate,
            ProfileCardTemplate = _settings.ProfileSwitcherProfileCardTemplate
        }).AsSingle();

        Container.Bind<QuickSettingsModalViewModel>().FromInstance(new QuickSettingsModalViewModel
        {
            SourceTemplate = _settings.QuickSettingsModalTemplate
        }).AsSingle();

        Container.Bind<ManageProfilesModalViewModel>().FromInstance(new ManageProfilesModalViewModel
        {
            SourceTemplate = _settings.ManageProfilesModalTemplate,
            ProfileItemTemplate = _settings.ManageProfilesProfileItemTemplate
        }).AsSingle();

        Container.Bind<ProfileSettingsModalViewModel>().FromInstance(new ProfileSettingsModalViewModel
        {
            SourceTemplate = _settings.ProfileSettingsModalTemplate
        }).AsSingle();

        Container.Bind<EditProfileModalViewModel>().FromInstance(new EditProfileModalViewModel
        {
            SourceTemplate = _settings.EditProfileModalTemplate
        }).AsSingle();

        Container.Bind<DeleteProfileModalViewModel>().FromInstance(new DeleteProfileModalViewModel
        {
            SourceTemplate = _settings.DeleteProfileModalTemplate
        }).AsSingle();

        Container.Bind<AddPlayerModalViewModel>().FromInstance(new AddPlayerModalViewModel
        {
            SourceTemplate = _settings.AddPlayerModalTemplate,
            CardTemplate = _settings.AddPlayerCardTemplate
        }).AsSingle();

        Container.Bind<ReplacePlayerModalViewModel>().FromInstance(new ReplacePlayerModalViewModel
        {
            SourceTemplate = _settings.ReplacePlayerModalTemplate,
            CardTemplate = _settings.AddPlayerCardTemplate
        }).AsSingle();
    }

    private void BindFrameworkServices()
    {
        Container.BindInterfacesAndSelfTo<EmulatorModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<EmulatorView>().AsSingle();
        Container.BindInterfacesAndSelfTo<EmulatorPanelHost>().AsSingle();
        Container.BindInterfacesAndSelfTo<EmulatorDisplayStateStore>().AsSingle();
        Container.BindInterfacesAndSelfTo<EmulatorDisplayHostResolver>().AsSingle();
        Container.BindInterfacesAndSelfTo<EmulatorDisplayStateSynchronizer>().AsSingle();
        Container.BindInterfacesAndSelfTo<DisplayActionRouter>().AsSingle();
    }

    private void BindDisplays()
    {
        Container.Bind<ProfileSwitcherModel>().AsSingle();
        Container.Bind<QuickSettingsModalModel>().AsSingle();
        Container.Bind<ManageProfilesModalModel>().AsSingle();
        Container.Bind<ProfileSettingsModalModel>().AsSingle();
        Container.Bind<EditProfileModalModel>().AsSingle();
        Container.Bind<DeleteProfileModalModel>().AsSingle();
        Container.Bind<AddPlayerModalModel>().AsSingle();
        Container.Bind<ReplacePlayerModalModel>().AsSingle();
        Container.Bind<ProfileSwitcherView>().AsSingle();
        Container.Bind<QuickSettingsModalView>().AsSingle();
        Container.Bind<ManageProfilesModalView>().AsSingle();
        Container.Bind<ProfileSettingsModalView>().AsSingle();
        Container.Bind<EditProfileModalView>().AsSingle();
        Container.Bind<DeleteProfileModalView>().AsSingle();
        Container.Bind<AddPlayerModalView>().AsSingle();
        Container.Bind<ReplacePlayerModalView>().AsSingle();

        Container.BindInterfacesAndSelfTo<ProfileSwitcher>().AsSingle();
        Container.BindInterfacesAndSelfTo<QuickSettingsModal>().AsSingle();
        Container.BindInterfacesAndSelfTo<ManageProfilesModal>().AsSingle();
        Container.BindInterfacesAndSelfTo<ProfileSettingsModal>().AsSingle();
        Container.BindInterfacesAndSelfTo<EditProfileModal>().AsSingle();
        Container.BindInterfacesAndSelfTo<DeleteProfileModal>().AsSingle();
        Container.BindInterfacesAndSelfTo<AddPlayerModal>().AsSingle();
        Container.BindInterfacesAndSelfTo<ReplacePlayerModal>().AsSingle();
    }

    private void BindActionRoutes()
    {
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<OpenProfileSwitcherViewAction, ProfileSwitcher>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseProfileSwitcherViewAction, ProfileSwitcher>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<OpenQuickSettingsViewAction, QuickSettingsModal>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseQuickSettingsViewAction, QuickSettingsModal>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<ManageProfilesViewAction, ManageProfilesModal>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseManageProfilesViewAction, ManageProfilesModal>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<OpenProfileSettingsViewAction, ProfileSettingsModal>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseProfileSettingsViewAction, ProfileSettingsModal>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<OpenEditProfileViewAction, EditProfileModal>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseEditProfileViewAction, EditProfileModal>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<OpenDeleteProfileViewAction, DeleteProfileModal>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseDeleteProfileViewAction, DeleteProfileModal>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<PresentAddPlayerSelectorViewAction, AddPlayerModal>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseAddPlayerSelectorViewAction, AddPlayerModal>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<PresentReplacePlayerSelectorViewAction, ReplacePlayerModal>(EmulatorDisplayOperation.Show)).AsCached();
        Container.Bind<IDisplayActionRoute>().FromInstance(DisplayActionRoute.Create<CloseReplacePlayerSelectorViewAction, ReplacePlayerModal>(EmulatorDisplayOperation.Hide)).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<ProfileSelectedViewAction, Emulator>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<OpenProfileSettingsViewAction, ProfileSettingsModal>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<OpenEditProfileViewAction, EditProfileModal>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<EditProfileSavedViewAction, EditProfileModal>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<OpenDeleteProfileViewAction, DeleteProfileModal>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<DeleteProfileViewAction, DeleteProfileModal>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<PresentAddPlayerSelectorViewAction, AddPlayerModal>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<CloseAddPlayerSelectorViewAction, BoardSessionService>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<PlayerAddedViewAction, BoardSessionService>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<PresentReplacePlayerSelectorViewAction, ReplacePlayerModal>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<CloseReplacePlayerSelectorViewAction, BoardSessionService>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<PlayerRemovedViewAction, BoardSessionService>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<PlayerReplacedViewAction, BoardSessionService>()).AsCached();
        Container.Bind<IViewActionHandlerRoute>().FromInstance(ViewActionHandlerRoute.Create<SetSafeSpaceVisibilityViewAction, Emulator>()).AsCached();
    }

    private void BindShell()
    {
        Container.BindInterfacesAndSelfTo<Emulator>().AsSingle().NonLazy();
    }
#endif

    private void BindBoardServices()
    {
        Container.Bind<IBoardApplication>().To<BoardApplicationService>().AsSingle();
        Container.BindInterfacesAndSelfTo<BoardSessionService>().AsSingle();
        Container.Bind<IBoardSaveGameManager>().To<BoardSaveGameManagerService>().AsSingle();
        Container.Bind<IBoard>().To<BoardFacade>().AsSingle();
        Container.BindInterfacesAndSelfTo<BoardStaticApiBinder>().AsSingle().NonLazy();
    }
}

#if UNITY_EDITOR
/// <summary>
/// Resolves the visual host element that each Board OS display should attach into.
/// </summary>
internal sealed class EmulatorDisplayHostResolver : IDisplayHostResolver
{
    private readonly EmulatorView _view;

    /// <summary>
    /// Creates the resolver.
    /// </summary>
    /// <param name="view">The shell view that owns the display host elements.</param>
    public EmulatorDisplayHostResolver([NotNull] EmulatorView view)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
    }

    /// <inheritdoc />
    public DisplayHostResolution Resolve(IEmulatorDisplay display)
    {
        if(display == null)
        {
            throw new ArgumentNullException(nameof(display));
        }

        if(_view.TryGetDedicatedDisplayHost(display.Name, out var dedicatedHost))
        {
            return new DisplayHostResolution(dedicatedHost, true);
        }

        return display switch
        {
            IEmulatorModal => new DisplayHostResolution(_view.ModalHost, false),
            IEmulatorScreen => new DisplayHostResolution(_view.ScreensHost, false),
            _ => throw new InvalidOperationException($"Display <{display.Name}> does not implement a recognized Board OS display category.")
        };
    }
}

/// <summary>
/// Synchronizes display visibility state from the registered displays back into the shell view.
/// </summary>
internal sealed class EmulatorDisplayStateSynchronizer : IEmulatorDisplayStateSynchronizer
{
    private readonly EmulatorView _view;
    private readonly IEnumerable<IEmulatorDisplay> _displays;

    /// <summary>
    /// Creates the synchronizer.
    /// </summary>
    /// <param name="view">The shell view that reflects aggregate display state.</param>
    /// <param name="displays">The displays whose visibility state should be reflected.</param>
    public EmulatorDisplayStateSynchronizer([NotNull] EmulatorView view, [NotNull] IEnumerable<IEmulatorDisplay> displays)
    {
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _displays = displays ?? throw new ArgumentNullException(nameof(displays));
    }

    /// <inheritdoc />
    public void Refresh()
    {
        _view.RefreshDisplayState(_displays);
    }
}
#endif
}
