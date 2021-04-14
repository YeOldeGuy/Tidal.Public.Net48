using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Tidal.Client.Constants;
using Tidal.Client.Helpers;
using Tidal.Client.Models;

namespace Tidal.Client.Requests
{
    public class GetTorrentsArgs
    {
        [DataMember(Name = RpcConstants.Fields)]
        public IList<string> Fields
        {
            get; set;
        }

        [DataMember(Name = RpcConstants.Ids)]
        public IList<int> Ids
        {
            get; set;
        }
    }

    public class TorrentsRequest : RequestBase
    {
        protected override string GetMethodName() => RpcConstants.GetTorrents;

        [DataMember(Name = RpcConstants.Arguments)]
        public GetTorrentsArgs Arguments
        {
            get; set;
        }

        private static IList<string> allCodedFields;

        public TorrentsRequest(IEnumerable<int> ids = null, IEnumerable<string> fields = null)
        {
            if (allCodedFields is null)
            {
                allCodedFields = FieldNameHelper.GetFieldNames<Torrent, DataMemberAttribute>(a => a.Name)
                                                .Where(n => !string.IsNullOrEmpty(n))
                                                .ToList();
            }

            if (fields is null)
            {
                fields = allCodedFields;
            }

            Arguments = ids is null || !ids.Any()
                ? new GetTorrentsArgs { Fields = fields.ToList() }
                : new GetTorrentsArgs { Ids = ids.ToList(), Fields = fields.ToList() };
        }

        public override string Serialize() => Json.ToJSON(this);
    }
}
