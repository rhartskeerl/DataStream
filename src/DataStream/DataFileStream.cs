using System.Data;
using System.Text;
using System.Text.Json;

namespace DataStream;

/// <summary>
/// A stream that reads data from a <see cref="IDataReader"/>.
/// </summary>
/// <param name="reader">An <see cref="IDataReader"/></param>
public class DataFileStream(IDataReader reader) : Stream, IDisposable
{
    private readonly byte[] NewLine = Encoding.UTF8.GetBytes("\r\n");
    private byte[]? _currentRow;            // The current row

    private bool _hasReadHeader = false;    // If the header has been read
    private int _position = 0;              // The position in the current row
    private int _totalLength = 0;           // The length with the newline
    private int _netLength = 0;             // The length without the newline
    private int _recordsAffected = 0;       // The number of records affected

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override void Flush()
    {
        throw new NotImplementedException();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_hasReadHeader)
        {
            if (_position == 0)
                _currentRow = ReadRow();
        }
        else
        {
            _currentRow = ReadHeader();
            _hasReadHeader = true;
        }

        if (_currentRow is null)
            return _position;

        _totalLength = _currentRow.Length + NewLine.Length - _position;

        if (count > _totalLength)
        {
            count = _totalLength;
        }
        _netLength = count - NewLine.Length;

        Buffer.BlockCopy(_currentRow, _position, buffer, offset, _netLength);

        if (_totalLength > count)
        {
            Buffer.BlockCopy(_currentRow, _position + _netLength, buffer, _netLength, NewLine.Length);
            _position += count;
        }
        else
        {
            Buffer.BlockCopy(NewLine, 0, buffer, _netLength, NewLine.Length);
            _position = 0;
        }

        return count;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException();
    }

    /// <summary>
    /// The final row read. You can use this to get the last value of a specific column.
    /// This can be the base for reading the next section.
    /// </summary>
    public object[] FinalRow
    {
        get; private set;
    }

    /// <summary>
    /// The header for the data. Use this in combination with <see cref="FinalRow"/> to identify which column to read.
    /// </summary>
    public DataFileHeader Header
    {
        get; private set;
    }

    public int RecordsAffected => _recordsAffected;

    private byte[] ReadHeader()
    {
        var header = new DataFileHeader(reader.FieldCount);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            header.Columns[i] = new DataFileColumn(reader.GetName(i), reader.GetFieldType(i).Name);
        }
        Header = header;
        return JsonSerializer.SerializeToUtf8Bytes(header, Constants.JsonOptions);
    }

    private byte[]? ReadRow()
    {
        if (!reader.Read())
        {
            FinalRow = JsonSerializer.Deserialize<object[]>(_currentRow, Constants.JsonOptions) ?? [];
            return null;
        }

        var row = new object[reader.FieldCount];
        reader.GetValues(row);
        _recordsAffected++;
        return JsonSerializer.SerializeToUtf8Bytes(row, Constants.JsonOptions);
    }
}
