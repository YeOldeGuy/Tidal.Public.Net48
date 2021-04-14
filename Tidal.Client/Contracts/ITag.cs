namespace Tidal.Client.Contracts
{
    /// <summary>
    /// Describes a contract for a class to provide a tag, to be used as deemed
    /// fit.
    /// </summary>
    public interface ITag
    {
        object Tag
        {
            get; set;
        }
    }
}
