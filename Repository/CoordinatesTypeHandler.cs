using System.Data;
using Dapper;
using Coordinates = Domain.Coordinates;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Repository;

public sealed class CoordinatesTypeHandler : SqlMapper.TypeHandler<Coordinates>
{
    private readonly SqlServerBytesReader _reader = new() { IsGeography = true };

    public override Coordinates Parse(object value)
    {
        var point = (Point)_reader.Read((byte[])value);
        return new Coordinates(point.Y, point.X);
    }

    public override void SetValue(IDbDataParameter parameter, Coordinates value)
    {
        throw new NotSupportedException();
    }
}