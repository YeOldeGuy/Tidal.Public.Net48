using System;
using System.Collections.ObjectModel;

namespace ValidationModel
{
    public interface IValidatableModel : IBindable
    {
        bool Validate(bool validateAfter);

        void Revert();

        void MarkAsClean();

        ObservableDictionary<string, IProperty> Properties { get; }

        ObservableCollection<string> Errors { get; }

        string ErrorSummary { get; }

        Action<IValidatableModel> Validator { set; get; }

        bool IsValid { get; }

        bool IsDirty { get; }
    }
}
