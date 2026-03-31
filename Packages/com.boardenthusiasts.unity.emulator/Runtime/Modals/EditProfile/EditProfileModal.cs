using System;
using System.Linq;

using BE.Emulator.Actions;
using BE.Emulator.Framework;
using BE.Emulator.Persistence;

using JetBrains.Annotations;

using Rahmen.Logging;

using Zenject;

namespace BE.Emulator.Modals.EditProfile
{
/// <summary>
/// Controller for the shared create and edit profile modal.
/// </summary>
internal sealed class EditProfileModal
    : DisplayableController<EditProfileModalModel, EditProfileModalView, EditProfileModalViewModel>, IEmulatorModal, IViewActionHandler
{
    /// <summary>
    /// The logical display name for the shared edit-profile modal.
    /// </summary>
    public const string DisplayName = "edit-profile";

    /// <inheritdoc />
    public string Name => DisplayName;

    private readonly EditProfileModalModel _model;
    private readonly EditProfileModalView _view;
    private readonly IEmulatorModel _emulatorModel;

    /// <summary>
    /// Creates the shared edit-profile modal controller.
    /// </summary>
    public EditProfileModal(
        [NotNull] ILoggerFactory loggerFactory,
        [NotNull] EditProfileModalModel model,
        [NotNull] EditProfileModalView view,
        [NotNull] LazyInject<IDisplayActionRouter> displayActionRouter,
        [InjectOptional] IEmulatorModel emulatorModel = null)
        : base(loggerFactory, model, view, displayActionRouter)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _view = view ?? throw new ArgumentNullException(nameof(view));
        _emulatorModel = emulatorModel;
    }

    /// <inheritdoc />
    public void HandleRoutedViewAction(object sender, IViewAction action)
    {
        switch(action)
        {
            case OpenEditProfileViewAction openAction:
                _view.SetOpenState(ResolveOpenState(openAction));
                return;

            case EditProfileSavedViewAction saveAction:
                if(PersistProfile(saveAction))
                {
                    RouteViewAction(this, new CloseEditProfileViewAction());
                }

                return;
        }
    }

    private EditProfileModalState ResolveOpenState(OpenEditProfileViewAction openAction)
    {
        if(openAction == null)
        {
            throw new ArgumentNullException(nameof(openAction));
        }

        if(openAction.Mode == EditProfileMode.Edit)
        {
            var selectedProfile = _emulatorModel?.CurrentData?.Profiles?.FirstOrDefault(profile =>
                string.Equals(profile?.PlayerId, openAction.SelectedProfileId, StringComparison.Ordinal));
            if(selectedProfile != null)
            {
                return _model.CreateEditState(selectedProfile);
            }
        }

        return _model.CreateCreateState();
    }

    private bool PersistProfile(EditProfileSavedViewAction saveAction)
    {
        if(saveAction == null || _emulatorModel == null)
        {
            return false;
        }

        if(IsDuplicateProfileName(saveAction))
        {
            _view.ShowDuplicateNicknameValidation();
            return false;
        }

        return saveAction.Mode == EditProfileMode.Edit
            ? _emulatorModel.UpdateProfile(saveAction.SelectedProfileId, saveAction.DisplayName, saveAction.AvatarBackgroundColor)
            : _emulatorModel.CreateProfile(saveAction.DisplayName, saveAction.AvatarBackgroundColor);
    }

    private bool IsDuplicateProfileName(EditProfileSavedViewAction saveAction)
    {
        return _emulatorModel.CurrentData?.Profiles?.Any(profile =>
            profile != null
            && string.Equals(profile.PlayerId, saveAction.SelectedProfileId, StringComparison.Ordinal) == false
            && string.Equals(profile.DisplayName, saveAction.DisplayName, StringComparison.OrdinalIgnoreCase)) == true;
    }
}
}
