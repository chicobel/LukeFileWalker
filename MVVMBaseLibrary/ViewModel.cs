using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MVVMBaseLibrary
{
    public abstract class ViewModel : INotifyPropertyChanged
    {

        /// <summary>
        /// Raised when a property on this object has a new value.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public string DisplayName { get; set; }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The property that has a new value.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            this.VerifyPropertyName(propertyName);
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged(this, e);
            }
 
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            var propName = (propertyExpression.Body as MemberExpression).Member.Name;
            this.VerifyPropertyName(propName);
            if (this.PropertyChanged != null)
            {
                var e = new PropertyChangedEventArgs(propName);
                PropertyChanged(this, e);
            }
        }

        /// <summary>
        /// Expression to return property name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            var propName = (propertyExpression.Body as MemberExpression).Member.Name;
            //var value = string.Format("<{0}>", propName);
            return propName;
        }


        #region Debugging Aides

        /// <summary>
        /// Warns if this object does not have
        /// a public property with the specified name.
        /// </summary>
        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public virtual void VerifyPropertyName(string propertyName)
        {
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                if (this.ThrowOnInvalidPropertyName)
                    throw new Exception(msg);
                else
                    Debug.Fail(msg);
            }
        }

       

        /// <summary>
    /// Returns whether an exception is thrown, or if a Debug.Fail() is used
    /// when an invalid property name is passed to the VerifyPropertyName method.
    /// </summary>
    protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
 
    #endregion // Debugging Aides
    }
}
