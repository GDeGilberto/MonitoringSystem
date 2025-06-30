using ClosedXML.Excel;
using System.ComponentModel;
using System.Reflection;

namespace Infrastructure.Services
{
    public interface IExcelExportService
    {
        byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Data", string? title = null);
    }

    public class ExcelExportService : IExcelExportService
    {
        public byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Data", string? title = null)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            var currentRow = 1;

            // Add title if provided
            if (!string.IsNullOrWhiteSpace(title))
            {
                worksheet.Cell(currentRow, 1).Value = title;
                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 16;
                worksheet.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                
                // Merge cells for title (assuming we have multiple columns)
                var properties = GetProperties<T>();
                if (properties.Any())
                {
                    worksheet.Range(currentRow, 1, currentRow, properties.Count()).Merge();
                }
                
                currentRow += 2; // Skip a row after title
            }

            // Add headers
            var headerRow = currentRow;
            var columnIndex = 1;
            
            foreach (var property in GetProperties<T>())
            {
                var displayName = GetDisplayName(property);
                worksheet.Cell(headerRow, columnIndex).Value = displayName;
                worksheet.Cell(headerRow, columnIndex).Style.Font.Bold = true;
                worksheet.Cell(headerRow, columnIndex).Style.Fill.BackgroundColor = XLColor.LightGray;
                worksheet.Cell(headerRow, columnIndex).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(headerRow, columnIndex).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                columnIndex++;
            }

            currentRow++;

            // Add data rows
            foreach (var item in data)
            {
                columnIndex = 1;
                foreach (var property in GetProperties<T>())
                {
                    var value = property.GetValue(item);
                    var cell = worksheet.Cell(currentRow, columnIndex);
                    
                    // Format cell based on data type
                    if (value != null)
                    {
                        if (IsNumericType(property.PropertyType))
                        {
                            if (double.TryParse(value.ToString(), out double numericValue))
                            {
                                cell.Value = numericValue;
                                cell.Style.NumberFormat.Format = "#,##0.00";
                            }
                            else
                            {
                                cell.Value = value.ToString();
                            }
                        }
                        else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        {
                            if (DateTime.TryParse(value.ToString(), out DateTime dateValue))
                            {
                                cell.Value = dateValue;
                                cell.Style.DateFormat.Format = "dd/MM/yyyy hh:mm AM/PM";
                            }
                            else
                            {
                                cell.Value = value.ToString();
                            }
                        }
                        else
                        {
                            cell.Value = value.ToString();
                        }
                    }
                    
                    cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    columnIndex++;
                }
                currentRow++;
            }

            // Auto-fit columns
            worksheet.ColumnsUsed().AdjustToContents();

            // Apply table styling
            if (data.Any())
            {
                var tableRange = worksheet.Range(headerRow, 1, currentRow - 1, GetProperties<T>().Count());
                tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;
                tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            }

            // Convert to byte array
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static IEnumerable<PropertyInfo> GetProperties<T>()
        {
            return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                           .Where(p => p.CanRead && p.GetIndexParameters().Length == 0);
        }

        private static string GetDisplayName(PropertyInfo property)
        {
            var displayNameAttribute = property.GetCustomAttribute<DisplayNameAttribute>();
            return displayNameAttribute?.DisplayName ?? property.Name;
        }

        private static bool IsNumericType(Type type)
        {
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;
            return underlyingType == typeof(int) ||
                   underlyingType == typeof(long) ||
                   underlyingType == typeof(float) ||
                   underlyingType == typeof(double) ||
                   underlyingType == typeof(decimal);
        }
    }
}