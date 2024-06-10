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

    public override bool CanRead => true;

    public override bool CanSeek => throw new NotImplementedException();

    public override bool CanWrite => throw new NotImplementedException();

    public override long Length => throw new NotImplementedException();

    public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

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
        throw new NotImplementedException();
    }

    public override void SetLength(long value)
    {
        throw new NotImplementedException();
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotImplementedException();
    }

    private byte[] ReadHeader()
    {
        var header = new DataFileHeader(reader.FieldCount);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            header.Columns[i] = new DataFileColumn(reader.GetName(i), reader.GetFieldType(i).Name);
        }
        return JsonSerializer.SerializeToUtf8Bytes(header, Constants.JsonOptions);
    }

    private byte[]? ReadRow()
    {
        if (!reader.Read())
            return null;

        var row = new object[reader.FieldCount];
        reader.GetValues(row);
        return JsonSerializer.SerializeToUtf8Bytes(row, Constants.JsonOptions);
    }
}
