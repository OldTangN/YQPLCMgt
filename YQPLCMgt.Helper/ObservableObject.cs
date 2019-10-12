using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YQPLCMgt.Helper
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region 观察者模式
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 设置值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="newValue"></param>
        /// <param name="propertyName"></param>
        protected void Set<T>(ref T field, T newValue, [System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                RaisePropertyChanged(propertyName);
            }
        }

        /// <summary>
        /// 触发属性改变事件
        /// </summary>
        /// <param name="propertyName"></param>
        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //public string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        //{
        //    if (propertyExpression == null)
        //    {
        //        throw new ArgumentNullException("propertyExpression");
        //    }

        //    var body = propertyExpression.Body as MemberExpression;

        //    if (body == null)
        //    {
        //        throw new ArgumentException("Invalid argument", "propertyExpression");
        //    }

        //    var property = body.Member as PropertyInfo;

        //    if (property == null)
        //    {
        //        throw new ArgumentException("Argument is not a property", "propertyExpression");
        //    }

        //    return property.Name;
        //}

        #endregion
    }
}
