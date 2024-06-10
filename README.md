When you need to transfer data between to SQL Server databases, and they are not connected to the same network, you are left with a few options.
You can use the bcp utility or the Bulk Insert statement. These are also valid options
when the two servers do not have a direct connection. For example, the destination server
is located in Azure. In this case you can use the bcp utility to export the data to a file
in Azure Blob Storage and then import the data from the file to the destination server.

Another option is to use something like Azure Data Feactory to move the data between the
two servers. This is a more complex solution and requires a data gateway to be installed.

## DataStream
This library is a simple solution to have more flexibility when you need to transfer data.
The main purpose of this library however is to transform an incoming data reader into a
stream of data that can be consumed by a data writer. The other purpose is to transform
an incoming stream of data into a data reader.

This means you can use [BlobClient.Upload](https://learn.microsoft.com/en-us/dotnet/api/azure.storage.blobs.blobclient.upload?view=azure-dotnet#azure-storage-blobs-blobclient-upload(system-io-stream-system-boolean-system-threading-cancellationtoken)) to upload the DBDataReader into Azure Blob Storage.
To further enhance it you can wrap it inside a [ZipArchive](https://learn.microsoft.com/en-us/dotnet/api/system.io.compression.ziparchive) to compress the data before uploading it.

Another use case is to transform the data into a HttpContent stream and pass it to 
a HttpClient to send the data to a web service. In this case you can use the
[HttpContent.ReadAsStreamAsync](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpcontent.readasstreamasync) method to transform the data writer into a stream.

## Usage
This example writes the data from a SQL Server database to the console using a StreamReader.

```csharp
using System;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using DataStream;

namespace DataStreamExample
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var connectionString = "Server=.;Database=master;Trusted_Connection=True;";
			var query = "SELECT * FROM sys.databases";

			using var connection = new SqlConnection(connectionString);
			await connection.OpenAsync();

			using var command = new SqlCommand(query, connection);
			using var reader = await command.ExecuteReaderAsync();

			// Transform the data reader into a stream
			using var stream = new DataReaderStream(reader);

			// Transform the stream into a data reader
			using var newReader = new StreamReader(stream);
			while (!newReader.EndOfStream)
			{
				Console.WriteLine(newReader.ReadLine());
			}
		}
	}
}
```