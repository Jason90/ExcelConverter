using System;
using System.IO;
using OfficeOpenXml;
using System.Data;
using System.Linq;
using System.Windows.Forms;

public class ExcelHelper
{
    public static void Convert(string sourceFilePath, string targetFilePath)
    {
        // Set the EPPlus license
        ExcelPackage.License.SetNonCommercialPersonal("Jason");

        // 1. Extract data from xls and convert it to DataTable
        DataTable dataTable = ExcelToDataTable(sourceFilePath);

        // 2. Create a new DataTable to store the reorganized data
        DataTable reorganizedDataTable = CreateReorganizedDataTable(dataTable);

        // 3. Create a new Excel file using the EPPlus library
        using (ExcelPackage targetPackage = new ExcelPackage())
        {
            // 4. Create a worksheet in the target file
            ExcelWorksheet targetSheet = targetPackage.Workbook.Worksheets.Add("Reorganized Data");

            // 5. Write the reorganized data to the worksheet
            targetSheet.Cells.LoadFromDataTable(reorganizedDataTable, true);

            // 6. Automatically adjust column widths
            targetSheet.Cells.AutoFitColumns();

            // 7. Save the target file
            string newTargetFilePath = Path.Combine(Path.GetDirectoryName(targetFilePath),
                Path.GetFileNameWithoutExtension(targetFilePath) + "_target.xlsx");
            FileInfo targetFile = new FileInfo(newTargetFilePath);
            targetPackage.SaveAs(targetFile);

            MessageBox.Show($"Conversion complete, file saved to: {newTargetFilePath}");
        }
    }

    /// <summary>
    /// Converts an Excel file to a DataTable
    /// </summary>
    /// <param name="filePath">Path to the Excel file</param>
    /// <returns>DataTable containing Excel data</returns>
    public static DataTable ExcelToDataTable(string filePath)
    {
        // Verify if file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("Excel file not found", filePath);
        }

        // Verify file extension
        string extension = Path.GetExtension(filePath).ToLower();
        if (extension != ".xlsx" && extension != ".xlsm")
        {
            throw new ArgumentException("File must be in .xlsx or .xlsm format");
        }

        DataTable dataTable = new DataTable();

        // Read Excel using EPPlus
        using (var package = new ExcelPackage(new FileInfo(filePath)))
        {
            // Get the first worksheet
            ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
            if (worksheet == null)
            {
                throw new InvalidOperationException("No worksheets found in the Excel file");
            }

            // Get maximum row and column counts
            int rowCount = worksheet.Dimension.End.Row;
            int colCount = worksheet.Dimension.End.Column;

            // Create DataTable columns (using first row as headers)
            for (int col = 1; col <= colCount; col++)
            {
                string columnName = worksheet.Cells[1, col].Text;
                // Add sequence number if column name is duplicated
                if (dataTable.Columns.Contains(columnName))
                {
                    columnName = $"{columnName}_{col}";
                }
                dataTable.Columns.Add(columnName);
            }

            // Populate data (starting from second row, skipping header)
            for (int row = 2; row <= rowCount; row++)
            {
                DataRow dataRow = dataTable.NewRow();
                for (int col = 1; col <= colCount; col++)
                {
                    // Get cell value and handle nulls
                    var cellValue = worksheet.Cells[row, col].Value;
                    dataRow[col - 1] = cellValue ?? DBNull.Value;
                }
                dataTable.Rows.Add(dataRow);
            }
        }

        return dataTable;
    }

    // Create a new DataTable to store the reorganized data
    private static DataTable CreateReorganizedDataTable(DataTable sourceDataTable)
    {
        DataTable targetDataTable = new DataTable();

        // Add the 5 new columns
        targetDataTable.Columns.Add("Property Name", typeof(string));
        targetDataTable.Columns.Add("Property Code", typeof(string));
        targetDataTable.Columns.Add("Meter Name", typeof(string));
        targetDataTable.Columns.Add("Commodity", typeof(string));
        targetDataTable.Columns.Add("Source", typeof(string));

        // Add year-month columns
        foreach (int year in new int[] { 2023, 2024, 2025 })
        {
            foreach (string month in new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" })
            {
                targetDataTable.Columns.Add($"{year} {month}", typeof(string));
            }
        }

        // Group by Property Code, Meter Name, and Commodity
        var groupedData = sourceDataTable.AsEnumerable()
            .GroupBy(row => new
            {
                PropertyCode = row.Field<string>("Property Code"),
                MeterName = row.Field<string>("Meter Name"),
                Commodity = row.Field<string>("Commodity")
            });

        // Populate the target DataTable
        foreach (var group in groupedData)
        {
            DataRow targetRow = targetDataTable.NewRow();

            // Get the first row of the group to populate the common columns
            DataRow firstRow = group.First();

            // Copy the 5 columns from the first row
            targetRow["Property Name"] = firstRow["Property Name"];
            targetRow["Property Code"] = firstRow["Property Code"];
            targetRow["Meter Name"] = firstRow["Meter Name"];
            targetRow["Commodity"] = firstRow["Commodity"];
            targetRow["Source"] = firstRow["Source"];

            // Populate the month data for each year
            foreach (var row in group)
            {
                if (row["Year"] != DBNull.Value)
                {
                    int year;
                    if (int.TryParse(row["Year"].ToString(), out year) && year >= 2023 && year <= 2025)
                    {
                        for (int j = 0; j < 12; j++)
                        {
                            string monthName = new string[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" }[j];
                            string columnName = $"{year} {monthName}";

                            // Check if the month column exists in the source data
                            if (sourceDataTable.Columns.Contains(monthName) && row[monthName] != DBNull.Value)
                            {
                                targetRow[columnName] = row[monthName].ToString();
                            }
                            else
                            {
                                targetRow[columnName] = string.Empty;
                            }
                        }
                    }
                }
            }

            targetDataTable.Rows.Add(targetRow);
        }

        return targetDataTable;
    }
}