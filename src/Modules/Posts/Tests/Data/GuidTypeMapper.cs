using System.Data;

using Dapper;

namespace Pulse.Posts.Tests.Data;

public class GuidTypeMapper : SqlMapper.TypeHandler<Guid>
{
    public override Guid Parse(object value)
    {
        return Guid.Parse(value.ToString());
    }

    public override void SetValue(IDbDataParameter parameter, Guid value)
    {
        parameter.Value = value.ToString();
    }
}