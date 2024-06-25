using System.Collections;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace DataStream;

/// <summary>
/// Reads an incoming stream and parses it into a <see cref="DbDataReader"/>.
/// </summary>
public class DataFileReader : DbDataReader, IDataReader
{
    private readonly StreamReader _streamReader;
    private readonly object[] _buffer;
    private readonly DataFileHeader _header;
    private string? _line;
    private JsonElement?[]? _jsonLine;
    private int _recordsAffected = 0;

    /// <summary>
    /// Create a new instance of <see cref="DataFileReader"/>
    /// using the specified <see cref="Stream"/>.
    /// </summary>
    /// <param name="stream">The incoming stream to transform into a <see cref="DbDataReader"/>.</param>
    public DataFileReader(Stream stream)
    {
        _streamReader = new StreamReader(stream);
        _header = ParseHeader();
        _buffer = new object[_header.Columns.Length];
    }

    public override object this[int ordinal] => throw new NotImplementedException();

    public override object this[string name] => throw new NotImplementedException();

    public override int Depth => throw new NotImplementedException();

    public override int FieldCount
    {
        get
        {
            return _header.Columns.Length;
        }
    }

    public override bool HasRows => throw new NotImplementedException();

    public override bool IsClosed => throw new NotImplementedException();

    public override int RecordsAffected => _recordsAffected;

    public override bool GetBoolean(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override byte GetByte(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override string GetDataTypeName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override decimal GetDecimal(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override double GetDouble(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {
        throw new NotImplementedException();
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
    public override Type GetFieldType(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override float GetFloat(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override Guid GetGuid(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override short GetInt16(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetInt32(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetInt64(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override string GetName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetOrdinal(string name)
    {
        throw new NotImplementedException();
    }

    public override string GetString(int ordinal) => _buffer[ordinal].ToString() ?? String.Empty;

    public override object GetValue(int ordinal)
    {
        return _buffer[ordinal];
    }

    public override int GetValues(object[] values)
    {
        return _buffer.Length;
    }

    public override bool IsDBNull(int ordinal)
    {
        return _buffer[ordinal] is DBNull;
    }

    public override bool NextResult()
    {
        return false;
    }

    public override bool Read()
    {
        return ParseRow();
    }

    private bool ParseRow()
    {
        _line = _streamReader.ReadLineAsync().Result;
        if (_line is null)
            return false;

        _jsonLine = JsonSerializer.Deserialize<JsonElement?[]>(_line, Constants.JsonOptions) ?? throw new Exception("Unable to parse line");

        for (var i = 0; i < _jsonLine.Length; i++)
        {
            if (_jsonLine[i] is null)
            {
                _buffer[i] = DBNull.Value;
                continue;
            }
            _buffer[i] = MapToObject(_jsonLine[i], _header.Columns[i].DataType);
        }
        _recordsAffected++;
        return true;
    }
    private DataFileHeader ParseHeader()
    {
        var line = _streamReader.ReadLineAsync().Result;
        ArgumentNullException.ThrowIfNull(line);

        var header = JsonSerializer.Deserialize<DataFileHeader>(line) ??
            throw new ArgumentNullException();

        return header;
    }

    private static object MapToObject(JsonElement? token, string tokenType)
    {
        if (token is null)
            return DBNull.Value;

        return tokenType switch
        {
            "Int16" => token.Value.Deserialize<short>(),
            "Int32" => token.Value.Deserialize<int>(),
            "Int64" => token.Value.Deserialize<long>(),
            "Decimal" => token.Value.Deserialize<decimal>(),
            "DateTime" => token.Value.Deserialize<DateTime>(),
            "Byte" => token.Value.Deserialize<byte>(),
            "Byte[]" => token.Value.Deserialize<byte[]>() ?? [],
            "Guid" => token.Value.Deserialize<Guid>(),
            "Boolean" => token.Value.Deserialize<bool>(),
            "Double" => token.Value.Deserialize<double>(),
            "DateTimeOffset" => token.Value.Deserialize<DateTimeOffset>(),
            "TimeSpan" => token.Value.Deserialize<TimeSpan>(),
            _ => token.Value.Deserialize<string>() ?? string.Empty,
        };
    }
}

