namespace Tidal.Client.Contracts
{
    /// <summary>
    /// Classes implementing this contract must set the <see cref="IsChanged"/>
    /// property to <see langword="true"/> if any public property is modified.
    /// </summary>
    public interface IIsChanged
    {
        /// <summary>
        /// Set to <see langword="true"/> if a property is changed.
        /// </summary>
        bool IsChanged
        {
            get; set;
        }
    }
}
