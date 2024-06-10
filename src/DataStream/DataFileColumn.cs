namespace DataStream;
/// <summary>
/// The column specification for a data file.
/// </summary>
public class DataFileColumn
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DataFileColumn"/> class.
    /// </summary>
    /// <param name="name">The name of the column</param>
    /// <param name="dataType">The .net type of the column</param>
    public DataFileColumn(string name, string dataType)
    {
        Name = name;
        DataType = dataType;
    }
    /// <summary>
    /// The name of the column
    /// </summary>
    public string Name { get; init; }
    /// <summary>
    /// The .net type of the column
    /// </summary>
    public string DataType { get; init; }
}
