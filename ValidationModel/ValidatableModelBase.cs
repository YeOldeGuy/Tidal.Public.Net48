using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;

namespace ValidationModel
{
    public abstract class ValidatableModelBase : IValidatableModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected T Read<T>([CallerMemberName] string propertyName = null)
        {
            if (!Properties.ContainsKey(propertyName))
                Properties.Add(propertyName, new Property<T>(propertyName));
            return (Properties[propertyName] as IProperty<T>).Value;
        }


        protected T Read<T>(T defaultValue, [CallerMemberName] string propertyName = null)
        {
            if (!Properties.ContainsKey(propertyName))
            {
                var prop = new Property<T>(propertyName);
                Properties.Add(propertyName, prop);
                prop.Value = defaultValue;
                prop.MarkAsClean();
            }
            return (Properties[propertyName] as IProperty<T>).Value;
        }

        protected bool Write<T>(T value, [CallerMemberName] string propertyName = null, bool validateAfter = true)
        {
            if (!Properties.ContainsKey(propertyName))
                Properties.Add(propertyName, new Property<T>(propertyName));
            var property = (Properties[propertyName] as IProperty<T>);
            var previous = property.Value;
            if (!property.IsOriginalSet || !Equals(value, previous))
            {
                property.Value = value;
                Validate(validateAfter);
                RaisePropertyChanged(property.PropertyName);
                return true;
            }
            return false;
        }

        public bool Validate(bool validateAfter = true)
        {
            if (validateAfter)
            {
                foreach (var property in Properties)
                {
                    property.Value.Errors.Clear();
                }
                Validator?.Invoke(this);
                Errors.Clear();
                foreach (var error in Properties.Values.SelectMany(x => x.Errors))
                {
                    Errors.Add(error);
                }
                RaisePropertyChanged(nameof(IsValid));
                RaisePropertyChanged(nameof(ErrorSummary));
            }
            return IsValid;
        }

        public void Revert()
        {
            foreach (var property in Properties)
            {
                property.Value.Revert();
                RaisePropertyChanged(property.Key);
            }

            Validate();
        }

        public void MarkAsClean()
        {
            foreach (var property in Properties)
            {
                property.Value.MarkAsClean();
                RaisePropertyChanged(property.Key);
            }

            RaisePropertyChanged(nameof(IsDirty));
            Validate();
        }

        [IgnoreDataMember]
        public ObservableDictionary<string, IProperty> Properties { get; }
            = new ObservableDictionary<string, IProperty>();

        [IgnoreDataMember]
        public ObservableCollection<string> Errors { get; }
            = new ObservableCollection<string>();

        [IgnoreDataMember]
        public string ErrorSummary
        {
            get
            {
                var sb = new StringBuilder();
                foreach (var error in Errors)
                    sb.Append(error).Append("\n");

                if (sb.Length > 1)
                    sb.Remove(sb.Length - 1, 1); // remove final newline
                return sb.ToString();
            }
        }

        [IgnoreDataMember]
        public Action<IValidatableModel> Validator { get; set; }

        [IgnoreDataMember]
        public bool IsValid => !Errors.Any();

        [IgnoreDataMember]
        public bool IsDirty => Properties.Any(x => x.Value.IsDirty);

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
