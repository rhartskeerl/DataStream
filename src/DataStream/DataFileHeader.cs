namespace DataStream;

/// <summary>
/// The header of a data file.
/// </summary>
public class DataFileHeader
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFileHeader"/> class.
    /// </summary>
    public DataFileHeader()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataFileHeader"/> class.
    /// </summary>
    /// <param name="fieldCount">The number of columns</param>
    public DataFileHeader(int fieldCount)
    {
        Columns = new DataFileColumn[fieldCount];
    }

    /// <summary>
    /// The columns in the data file.
    /// </summary>
    public DataFileColumn[] Columns { get; set; } = [];
}
