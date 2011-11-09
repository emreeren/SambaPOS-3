using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.InteropServices;

namespace Samba.Presentation.Common
{
    public abstract class ObservableObject : INotifyPropertyChanged, IDisposable
    {
        private void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        ~ObservableObject()
        {
            Dispose(false);
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

        public event PropertyChangedEventHandler PropertyChanged;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            //
        }
    }
}
