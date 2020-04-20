using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CsvHelper;

namespace SosCafe.Admin
{
    public static class CsvCreator
    {
        internal static byte[] CreateCsvFile<T>(IEnumerable<T> recordsToWrite)
        {
            using (var stream = new MemoryStream())
            {
                using (var writer = new StreamWriter(stream))
                using (var csv = new CsvWriter(writer, new CultureInfo("en-NZ")))
                {
                    csv.WriteRecords(recordsToWrite);
                }

                return stream.ToArray();
            }
        }
    }
}
