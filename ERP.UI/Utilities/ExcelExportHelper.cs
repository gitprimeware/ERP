using ClosedXML.Excel;
using DevExpress.XtraGrid.Columns;
using ERP.UI.UI;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace ERP.UI.Utilities
{
    /// <summary>
    /// DataGridView'den Excel'e aktarým için yardýmcý sýnýf
    /// </summary>
    public static class ExcelExportHelper
    {
        /// <summary>
        /// DataGridView'deki verileri Excel dosyasýna aktar
        /// </summary>
        /// <param name="dataGridView">Aktarýlacak DataGridView</param>
        /// <param name="defaultFileName">Varsayýlan dosya adý (opsiyonel)</param>
        /// <param name="sheetName">Excel çalýþma sayfasý adý (opsiyonel)</param>
        /// <param name="title">Excel baþlýðý (opsiyonel)</param>
        public static void ExportToExcel(
            DataGridView dataGridView, 
            string defaultFileName = "Rapor", 
            string sheetName = "Sayfa1",
            string[]? skippedColumnNames = null,
            string? title = null)
        {
            if (dataGridView == null || dataGridView.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Aktarýlacak veri bulunamadý.", 
                    "Uyarý", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // SaveFileDialog ile dosya konumu sor
                using (var saveFileDialog = new SaveFileDialog())
                {
                    saveFileDialog.Filter = "Excel Dosyasý (*.xlsx)|*.xlsx";
                    saveFileDialog.FileName = $"{defaultFileName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
                    saveFileDialog.Title = "Excel Dosyasýný Kaydet";

                    if (saveFileDialog.ShowDialog() == DialogResult.OK)
                    {
                        // Excel workbook oluþtur
                        using (var workbook = new XLWorkbook())
                        {
                            var worksheet = workbook.Worksheets.Add(sheetName);
                            int currentRow = 1;

                            // Baþlýk ekle (opsiyonel)
                            if (!string.IsNullOrWhiteSpace(title))
                            {
                                worksheet.Cell(currentRow, 1).Value = title;
                                worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
                                worksheet.Cell(currentRow, 1).Style.Font.FontSize = 14;
                                worksheet.Cell(currentRow, 1).Style.Font.FontColor = XLColor.FromColor(ThemeColors.Primary);
                                
                                // Baþlýðý birleþtir (tüm kolonlar boyunca)
                                int visibleColumnCount = GetVisibleColumnCount(dataGridView);
                                if (visibleColumnCount > 1)
                                {
                                    worksheet.Range(currentRow, 1, currentRow, visibleColumnCount).Merge();
                                }
                                
                                currentRow += 2; // Boþluk býrak
                            }

                            int headerRow = currentRow;
                            int excelColumn = 1;

                            // Baþlýklarý ekle (sadece görünür kolonlarý)
                            foreach (DataGridViewColumn column in dataGridView.Columns)
                            {
                                if (column.Visible && !(skippedColumnNames?.Any(x => x == column.Name) ?? false)) // Checkbox kolonunu atla
                                {
                                    var headerCell = worksheet.Cell(headerRow, excelColumn);
                                    headerCell.Value = column.HeaderText;
                                    headerCell.Style.Font.Bold = true;
                                    headerCell.Style.Fill.BackgroundColor = XLColor.FromColor(ThemeColors.Primary);
                                    headerCell.Style.Font.FontColor = XLColor.White;
                                    headerCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                                    headerCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                    excelColumn++;
                                }
                            }

                            currentRow++;

                            // Verileri ekle
                            foreach (DataGridViewRow row in dataGridView.Rows)
                            {
                                if (row.IsNewRow) continue;

                                excelColumn = 1;
                                foreach (DataGridViewColumn column in dataGridView.Columns)
                                {
                                    if (column.Visible && !(skippedColumnNames?.Any(x => x == column.Name) ?? false)) // Checkbox kolonunu atla
                                    {
                                        var cell = worksheet.Cell(currentRow, excelColumn);
                                        var cellValue = row.Cells[column.Index].Value;

                                        if (cellValue != null)
                                        {
                                            // Deðer tipine göre formatla
                                            if (cellValue is DateTime dateTime)
                                            {
                                                cell.Value = dateTime;
                                                cell.Style.DateFormat.Format = "dd.MM.yyyy";
                                            }
                                            else if (IsNumeric(cellValue))
                                            {
                                                cell.Value = Convert.ToDouble(cellValue);
                                                cell.Style.NumberFormat.Format = "#,##0.00";
                                            }
                                            else
                                            {
                                                cell.Value = cellValue.ToString();
                                            }
                                        }

                                        // Hücre stili
                                        cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                                        
                                        excelColumn++;
                                    }
                                }
                                currentRow++;
                            }

                            // Kolon geniþliklerini ayarla
                            worksheet.Columns().AdjustToContents();

                            // Dosyayý kaydet
                            workbook.SaveAs(saveFileDialog.FileName);
                        }

                        MessageBox.Show(
                            $"Veriler baþarýyla Excel'e aktarýldý.\n\nDosya: {saveFileDialog.FileName}", 
                            "Baþarýlý", 
                            MessageBoxButtons.OK, 
                            MessageBoxIcon.Information);

                        // Excel dosyasýný aç (opsiyonel)
                        var result = MessageBox.Show(
                            "Excel dosyasýný açmak ister misiniz?", 
                            "Dosyayý Aç", 
                            MessageBoxButtons.YesNo, 
                            MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = saveFileDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Excel'e aktarým sýrasýnda hata oluþtu:\n\n{ex.Message}", 
                    "Hata", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Deðerin sayýsal olup olmadýðýný kontrol eder
        /// </summary>
        private static bool IsNumeric(object value)
        {
            return value is sbyte
                || value is byte
                || value is short
                || value is ushort
                || value is int
                || value is uint
                || value is long
                || value is ulong
                || value is float
                || value is double
                || value is decimal;
        }

        /// <summary>
        /// DataGridView'deki görünür kolon sayýsýný döndürür
        /// </summary>
        private static int GetVisibleColumnCount(DataGridView dataGridView)
        {
            int count = 0;
            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                if (column.Visible && column.Name != "IsSelected")
                {
                    count++;
                }
            }
            return count;
        }
    }
}
