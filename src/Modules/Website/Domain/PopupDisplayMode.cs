namespace GenclikMerkezi.Modules.Website.Domain;

// Faz 2 Görev 6 master prompt §6: Modal always carries an image slot, a delay and is always
// dismissible; Banner never does (no image, no delay, Dismissible is an admin choice, Body is plain
// text only) - every mode-dependent rule in Popup/PopupTranslation keys off this.
public enum PopupDisplayMode
{
    Modal,
    Banner,
}
