using System;
using System.Collections;
using System.ComponentModel;
using System.Runtime.Serialization;
using Tidal.Client.Contracts;
using Tidal.Properties;
using ValidationModel;

namespace Tidal.Models
{
    /// <summary>
    /// Contains the information of a connection to a Transmission host.
    /// </summary>
    public class Host : ValidatableModelBase, INotifyDataErrorInfo, ITag, IAssignable<Host>, IEquatable<Host>
    {
        /// <summary>
        /// Default constructor for a <see cref="Host"/>.
        /// </summary>
        public Host()
            : this(Resources.NewHostName)
        {
            Validator = ValidateHost;
        }

        /// <summary>
        /// Creates an instance of the <see cref="Host"/> class with default
        /// values.
        /// </summary>
        /// <param name="name">The name to give the <see cref="Host"/></param>
        public Host(string name)
        {
            if (string.IsNullOrEmpty(name))
                name = Resources.NewHostName;

            Id = Guid.NewGuid();
            Name = name;
            Address = "localhost";
            Port = 9091;
            IsSecure = false;
            UseAuthentication = false;
            Tag = false;
            Validator = ValidateHost;
        }

        public override bool Equals(object obj) => Equals(obj as Host);

        public override int GetHashCode() => Id.GetHashCode();

        [IgnoreDataMember]
        public object Tag { get; set; }

        public void Assign(Host other)
        {
            Active = other.Active;
            Id = other.Id;
            Name = other.Name;
            Address = other.Address;
            Port = other.Port;
            UseAuthentication = other.UseAuthentication;
            UserName = other.UserName;
            Password = other.Password;
            IsSecure = other.IsSecure;
            Validator = ValidateHost;

            if (!other.IsDirty)
                MarkAsClean();
        }

        public Host Clone()
        {
            var host = new Host
            {
                // some weird C# scoping shit going on here
                Name = Name,
                Address = Address,
                Port = Port,
                IsSecure = IsSecure,
                UseAuthentication = UseAuthentication,
                UserName = UserName,
                Password = Password,
                Id = Id,
                Active = Active,
                Validator = Validator,
            };
            host.MarkAsClean();
            return host;
        }

        private static void ValidateHost(IValidatableModel model)
        {
            var host = model as Host;

            if (string.IsNullOrEmpty(host.Name))
            {
                host.Properties[nameof(Name)].Errors.Add(Resources.HostNameEmptyError);
                host.ErrorsChanged?.Invoke(host, new DataErrorsChangedEventArgs(nameof(Name)));
            }

            if (string.IsNullOrEmpty(host.Address))
            {
                host.Properties[nameof(Address)].Errors.Add(Resources.HostAddressEmptyError);
                host.ErrorsChanged?.Invoke(host, new DataErrorsChangedEventArgs(nameof(Address)));
            }

            if (host.UseAuthentication)
            {
                if (string.IsNullOrEmpty(host.UserName))
                {
                    host.Properties[nameof(UserName)].Errors.Add(Resources.HostUserEmptyError);
                    host.ErrorsChanged?.Invoke(host, new DataErrorsChangedEventArgs(nameof(UserName)));
                }
                else if (host.UserName.Contains(":"))
                {
                    host.Properties[nameof(UserName)].Errors.Add(Resources.HostUserColonError);
                    host.ErrorsChanged?.Invoke(host, new DataErrorsChangedEventArgs(nameof(UserName)));
                }
            }
        }

        public bool Equals(Host other)
        {
            if (other is Host h)
                return h.Id == Id;
            return false;
        }

        #region INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) || Properties[propertyName].Errors.Count == 0)
                return null;
            return Properties[propertyName].Errors;
        }

        [IgnoreDataMember]
        public bool HasErrors => !IsValid;
        #endregion

        #region Properties

        /// <summary>
        /// A unique identifier for the <see cref="Host"/>, represented as a
        /// <see cref="Guid"/>. Let the constructors assign this; it's only
        /// publicly assignable for the Utf8Json library's use.
        /// </summary>
        public Guid Id
        {
            get => Read(Guid.NewGuid());
            set => Write(value);
        }

        /// <summary>
        /// A name for the <see cref="Host"/>, for user convenience. It is
        /// <b>not</b> an error to have duplicates.
        /// </summary>
        public string Name
        {
            get => Read<string>();
            set => Write(value);
        }

        /// <summary>
        /// The IP address of the <see cref="Host"/>. Normally a numeric
        /// dotted notation, there is no real checking for validity other
        /// than trying to establish a connection.
        /// </summary>
        public string Address
        {
            get => Read<string>();
            set => Write(value);
        }

        /// <summary>
        /// If <see langword="true"/>, try to establish a secure connection.
        /// The code tries, but I have no way to test this, so ¯\_(ツ)_/¯
        /// </summary>
        public bool IsSecure
        {
            get => Read(false);
            set => Write(value);
        }

        /// <summary>
        /// The port number to use for the connection, normally 9091.
        /// </summary>
        public int Port
        {
            get => Read(9091);
            set => Write(value);
        }

        /// <summary>
        /// If <see langword="true"/>, use basic authentication for
        /// establishing the connection.
        /// </summary>
        public bool UseAuthentication
        {
            get => Read(false);
            set => Write(value);
        }

        /// <summary>
        /// The user name to use if <see cref="UseAuthentication"/> is
        /// <see langword="true"/>. Make sure there aren't any colon
        /// characters in the string.
        /// </summary>
        public string UserName
        {
            get => Read<string>();
            set => Write(value);
        }

        /// <summary>
        /// The Soopr-Sekrit password to use if <see cref="UseAuthentication"/>
        /// is <see langword="true"/>.
        /// </summary>
        public string Password
        {
            get => Read<string>();
            set => Write(value);
        }

        /// <summary>
        /// If <see langword="true"/>, then this is the <see cref="Host"/>
        /// to use for connections. There can be only one!
        /// </summary>
        public bool Active
        {
            get => Read(false);
            set => Write(value);
        }
        #endregion
    }
}
