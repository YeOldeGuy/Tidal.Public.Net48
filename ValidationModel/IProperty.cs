using System;
using System.Collections.ObjectModel;

namespace ValidationModel
{
    public interface IProperty : IBindable
    {
        event EventHandler ValueChanged;

        string PropertyName { get; }

        void Revert();

        void MarkAsClean();

        ObservableCollection<string> Errors { get; }

        bool IsValid { get; }

        bool IsDirty { get; }

        bool IsOriginalSet { get; }
    }
}
