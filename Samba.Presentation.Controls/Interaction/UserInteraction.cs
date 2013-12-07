using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.ComponentModel.Composition;
using System.Windows.Media;
using System.Windows.Threading;
using PropertyTools.DataAnnotations;
using Samba.Infrastructure.Data;
using Samba.Infrastructure.Settings;
using Samba.Localization.Properties;
using Samba.Presentation.Common;
using Samba.Presentation.Common.Commands;
using Samba.Presentation.Services;
using FlexButton;
using Samba.Services.Common;

namespace Samba.Presentation.Controls.Interaction
{
    public class CollectionProxy<T>
    {
        [WideProperty]
        public ObservableCollection<T> List { get; set; }

        public CollectionProxy(IEnumerable<T> list)
        {
            List = new ObservableCollection<T>(list);
        }
    }

    public class PopupData
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public Action<object> Action { get; set; }
        public object ActionParameter { get; set; }

        private string _headerColor;
        public string HeaderColor
        {
            get { return _headerColor; }
            set
            {
                _headerColor = value;
                ContentColor = UpdateContentColor();
            }
        }

        private string UpdateContentColor()
        {
            var color = ((TypeConverter)new ColorConverter()).ConvertFromString(HeaderColor);
            var result = color is Color ? (Color)color : new Color();
            return result.Lerp(Colors.White, 0.80f).ToString();
        }

        public string ContentColor { get; set; }
    }

    public class PopupDataViewModel
    {
        private readonly IApplicationState _applicationState;
        private readonly IList<PopupData> _popupCache;
        private readonly ObservableCollection<PopupData> _popupList;
        public ObservableCollection<PopupData> PopupList { get { return _popupList; } }

        public CaptionCommand<PopupData> ClickButtonCommand { get; set; }

        public PopupDataViewModel(IApplicationState applicationState)
        {
            _applicationState = applicationState;
            _popupCache = new List<PopupData>();
            _popupList = new ObservableCollection<PopupData>();
            ClickButtonCommand = new CaptionCommand<PopupData>("Click", OnButtonClick);
        }

        private void OnButtonClick(PopupData obj)
        {
            if (obj.Action != null)
                obj.Action(obj.ActionParameter);
            _popupList.Remove(obj);
            if (obj.Name != "")
            {
                _applicationState.NotifyEvent(RuleEventNames.PopupClicked, new { Name = obj.Name, Data = obj.ActionParameter ?? "" });
            }
            Application.Current.MainWindow.Focus();
        }

        public void Add(string name, string title, string content, string headerColor, Action<object> action, object actionParameter)
        {
            _popupCache.Add(new PopupData { Name = name, Title = title, Content = content, HeaderColor = headerColor, Action = action, ActionParameter = actionParameter });
        }

        public void DisplayPopups()
        {
            if (_popupCache.Count == 0) return;
            foreach (var popupData in _popupCache)
            {
                _popupList.Add(popupData);
            }
            _popupCache.Clear();
        }
    }

    [Export(typeof(IUserInteraction))]
    public class UserInteraction : IUserInteraction
    {
        private readonly IMethodQueue _methodQueue;
        private readonly PopupDataViewModel _popupDataViewModel;
        private SplashScreenForm _splashScreen;

        private KeyboardWindow _keyboardWindow;
        public KeyboardWindow KeyboardWindow { get { return _keyboardWindow ?? (_keyboardWindow = CreateKeyboardWindow()); } set { _keyboardWindow = value; } }

        private PopupWindow _popupWindow;

        [ImportingConstructor]
        public UserInteraction(IMethodQueue methodQueue, IApplicationState applicationState)
        {
            _methodQueue = methodQueue;
            _popupDataViewModel = new PopupDataViewModel(applicationState);
        }

        public PopupWindow PopupWindow
        {
            get { return _popupWindow ?? (_popupWindow = CreatePopupWindow()); }
        }

        private PopupWindow CreatePopupWindow()
        {
            return new PopupWindow { DataContext = _popupDataViewModel };
        }

        private static KeyboardWindow CreateKeyboardWindow()
        {
            return new KeyboardWindow();
        }

        public string[] GetStringFromUser(string caption, string description)
        {
            return GetStringFromUser(caption, description, "");
        }

        public string[] GetStringFromUser(string caption, string description, string defaultValue)
        {
            var form = new StringGetterForm
            {
                Title = caption,
                TextBox = { Text = defaultValue },
                DescriptionLabel = { Text = description }
            };

            form.ShowDialog();
            var result = new List<string>();
            for (int i = 0; i < form.TextBox.LineCount; i++)
            {
                string value = form.TextBox.GetLineText(i).Trim('\r', '\n', ' ', '\t');
                if (!string.IsNullOrEmpty(value))
                {
                    result.Add(value);
                }
            }
            return result.ToArray();
        }

        public IList<IOrderable> ChooseValuesFrom(
            IList<IOrderable> values,
            IList<IOrderable> selectedValues,
            string caption,
            string description,
            string singularName,
            string pluralName)
        {
            selectedValues = new ObservableCollection<IOrderable>(selectedValues.OrderBy(x => x.SortOrder));

            ReorderItems(selectedValues);

            var form = new ValueChooserForm(values, selectedValues)
                           {
                               Title = caption,
                               DescriptionLabel = { Content = description },
                               ValuesLabel = { Content = string.Format(Resources.List_f, singularName) },
                               SelectedValuesLabel = { Content = string.Format(Resources.Selected_f, pluralName) }
                           };
            Scale(form.MainGrid);

            form.ShowDialog();

            ReorderItems(form.SelectedValues);

            return form.SelectedValues;
        }

        public void EditProperties(object item)
        {
            var form = new PropertyEditorForm { PropertyEditorControl = { SelectedObject = item } };
            form.ShowDialog();
        }

        public void EditProperties<T>(IList<T> items)
        {
            var form = new GridEditorForm();
            form.SetList(items.ToList());
            form.ShowDialog();
        }

        public void SortItems(IEnumerable<IOrderable> list, string caption, string description)
        {
            var items = new ObservableCollection<IOrderable>(list.OrderBy(x => x.SortOrder));

            ReorderItems(items);
            var form = new ListSorterForm
                           {
                               MainListBox = { ItemsSource = items },
                               Title = caption,
                               DescriptionLabel = { Text = description }
                           };
            Scale(form.MainGrid);
            form.ShowDialog();

            if (form.DialogResult != null && form.DialogResult.Value)
            {
                ReorderItems(items);
            }
        }

        public bool AskQuestion(string question)
        {
            return
                MessageBox.Show(question, Resources.Question, MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) ==
                MessageBoxResult.Yes;
        }

        public void GiveFeedback(string message)
        {
            Application.Current.MainWindow.Dispatcher.Invoke(new Action(() =>
                {
                    var window = new FeedbackWindow { MessageText = { Text = message }, Topmost = true };
                    window.ShowDialog();
                }));
        }

        public void ShowKeyboard()
        {
            KeyboardWindow.ShowKeyboard();
        }

        public void HideKeyboard()
        {
            KeyboardWindow.HideKeyboard();
        }

        public void ToggleKeyboard()
        {
            if (KeyboardWindow.IsVisible)
                HideKeyboard();
            else ShowKeyboard();
        }

        public void ToggleSplashScreen()
        {
            if (_splashScreen != null)
            {
                _splashScreen.Hide();
                _splashScreen = null;
            }
            else
            {
                _splashScreen = new SplashScreenForm();
                _splashScreen.Show();
            }
        }

        public void DisplayPopup(string name, string title, string content, string headerColor, Action<object> action = null, object actionParameter = null)
        {
            _popupDataViewModel.Add(name, title, content, headerColor, action, actionParameter);
            PopupWindow.Show();
            _methodQueue.Queue("DisplayPopups", DisplayPopups);
        }

        public void Scale(FrameworkElement control)
        {
            if (LocalSettings.WindowScale > 0)
            {
                var scaleTransform = (control.LayoutTransform as ScaleTransform);
                if (scaleTransform != null)
                {
                    scaleTransform.ScaleX = LocalSettings.WindowScale;
                    scaleTransform.ScaleY = LocalSettings.WindowScale;
                }
            }
        }

        public void DisplayPopups()
        {
            _popupDataViewModel.DisplayPopups();
        }

        private static void ReorderItems(IEnumerable<IOrderable> list)
        {
            var order = 10;
            foreach (var orderable in list)
            {
                orderable.SortOrder = order;
                order += 10;
            }
        }
    }

}
