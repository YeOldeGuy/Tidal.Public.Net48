using System;
using System.Reflection;
using Tidal.Client.Contracts;

namespace Tidal.Client.Models
{
    /// <summary>
    /// This class acts as a conduit between a Session or Torrent and the
    /// mutator class for it.
    /// </summary>
    public abstract class MutatorBase : Assignable<MutatorBase>
    {
        public void SetProperty(string propertyName, object value)
        {
            SetValue(this, propertyName, value);
            RaisePropertyChanged(propertyName);
            if (this is IIsChanged changable)
                changable.IsChanged = true;
        }

        /// <summary>
        /// Assigns a value to the specified object's property by name.
        /// </summary>
        private static void SetValue(object inputObject,
                                     string propertyName,
                                     object propertyVal)
        {
            // find out the type
            Type type = inputObject.GetType();

            // get the property information based on the type
            PropertyInfo propertyInfo = type.GetProperty(propertyName);
            if (propertyInfo == null)
                return;

            // find the property type
            Type propertyType = propertyInfo.PropertyType;

            // Convert.ChangeType does not handle conversion to nullable types
            // if the property type is nullable, we need to get the underlying
            // type of the property
            var targetType = IsNullableType(propertyType)
                ? Nullable.GetUnderlyingType(propertyType)
                : propertyInfo.PropertyType;

            // Returns an System.Object with the specified System.Type and whose
            // value is equivalent to the specified object.
            propertyVal = Convert.ChangeType(propertyVal, targetType);

            // Set the value of the property
            propertyInfo.SetValue(inputObject, propertyVal, null);
        }

        private static bool IsNullableType(Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }

        protected override void AssignInternal(MutatorBase other) { }
    }
}
