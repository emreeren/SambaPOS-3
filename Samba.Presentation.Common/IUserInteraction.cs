using System;
using System.Collections.Generic;
using System.Windows;
using Samba.Infrastructure.Data;

namespace Samba.Presentation.Common
{
    public interface IUserInteraction
    {
        string[] GetStringFromUser(string caption, string description);
        string[] GetStringFromUser(string caption, string description, string defaultValue);
        IList<IOrderable> ChooseValuesFrom(
            IList<IOrderable> values,
            IList<IOrderable> selectedValues,
            string caption,
            string description,
            string singularName,
            string pluralName);

        void EditProperties(object item);
        void EditProperties<T>(IList<T> item);
        void SortItems(IEnumerable<IOrderable> list, string caption, string description);
        bool AskQuestion(string question);
        void GiveFeedback(string message);
        void ShowKeyboard();
        void HideKeyboard();
        void ToggleKeyboard();
        void ToggleSplashScreen();
        void DisplayPopup(string name, string title, string content, string headerColor = "DarkRed", Action<object> action = null, object actionParameter = null);
        void Scale(FrameworkElement control);
    }
}
