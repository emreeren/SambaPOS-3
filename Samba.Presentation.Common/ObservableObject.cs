using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;

namespace Samba.Presentation.Common
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        //protected void RaisePropertyChanged(string propertyName)
        //{
        //    VerifyPropertyName(propertyName);

        //    var handler = PropertyChanged;

        //    if (handler == null) return;

        //    var e = new PropertyChangedEventArgs(propertyName);
        //    handler(this, e);
        //}

        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression.Body.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpr = (MemberExpression)propertyExpression.Body;
                var propertyName = memberExpr.Member.Name;
                RaisePropertyChanged(propertyName);
            }
        }

        //[Conditional("DEBUG")]
        //[DebuggerStepThrough]
        //public void VerifyPropertyName(string propertyName)
        //{
        //    if (string.IsNullOrEmpty(propertyName))
        //        return;

        //    if (TypeDescriptor.GetProperties(this)[propertyName] != null) return;

        //    var msg = "Invalid property name: " + propertyName;

        //    if (ThrowOnInvalidPropertyName)
        //        throw new ArgumentException(msg);
        //    Debug.Fail(msg);
        //}

        //protected virtual bool ThrowOnInvalidPropertyName { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
