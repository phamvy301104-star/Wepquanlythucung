using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;

namespace nhom6_admin.Services
{
    /// <summary>
    /// Service xuất báo cáo ra file Excel với format đẹp
    /// </summary>
    public class ExcelExportService
    {
        // ==================== ĐỊNH NGHĨA MÀU SẮC ====================
        private static readonly Color PrimaryColor = Color.FromArgb(212, 175, 55);    // Gold - Primary
        private static readonly Color HeaderColor = Color.FromArgb(51, 51, 51);        // Dark - Header
        private static readonly Color SuccessColor = Color.FromArgb(76, 175, 80);      // Green
        private static readonly Color WarningColor = Color.FromArgb(255, 152, 0);      // Orange
        private static readonly Color DangerColor = Color.FromArgb(229, 57, 53);       // Red
        private static readonly Color InfoColor = Color.FromArgb(33, 150, 243);        // Blue
        private static readonly Color LightGray = Color.FromArgb(248, 249, 252);       // Light gray

        /// <summary>
        /// Xuất báo cáo doanh thu
        /// </summary>
        public byte[] ExportRevenueReport(
            dynamic summary,
            IEnumerable<dynamic> chartData,
            IEnumerable<dynamic> paymentMethods,
            string period,
            DateTime startDate,
            DateTime endDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Báo Cáo Doanh Thu");

            int row = 1;

            // ==================== TIÊU ĐỀ BÁO CÁO ====================
            sheet.Cells["A1:G1"].Merge = true;
            sheet.Cells["A1"].Value = "BÁO CÁO DOANH THU - UME SALON";
            StyleTitle(sheet.Cells["A1"], PrimaryColor);
            sheet.Row(1).Height = 35;

            row = 2;
            sheet.Cells[$"A{row}:G{row}"].Merge = true;
            sheet.Cells[$"A{row}"].Value = $"Kỳ báo cáo: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
            sheet.Cells[$"A{row}"].Style.Font.Italic = true;
            sheet.Cells[$"A{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            
            row = 4;

            // ==================== TỔNG QUAN ====================
            sheet.Cells[$"A{row}:D{row}"].Merge = true;
            sheet.Cells[$"A{row}"].Value = "📊 TỔNG QUAN";
            StyleSectionHeader(sheet.Cells[$"A{row}:D{row}"]);
            row++;

            // Header
            StyleTableHeader(sheet.Cells[$"A{row}:B{row}"]);
            sheet.Cells[$"A{row}"].Value = "Chỉ tiêu";
            sheet.Cells[$"B{row}"].Value = "Giá trị";
            row++;

            // Data rows
            AddSummaryRow(sheet, ref row, "💰 Tổng doanh thu", summary.totalRevenue, true, PrimaryColor);
            AddSummaryRow(sheet, ref row, "🛒 Doanh thu đơn hàng", summary.ordersRevenue, false, SuccessColor);
            AddSummaryRow(sheet, ref row, "📦 Số đơn hàng", summary.ordersCount, false, null);
            AddSummaryRow(sheet, ref row, "💇 Doanh thu dịch vụ", summary.appointmentsRevenue, false, InfoColor);
            AddSummaryRow(sheet, ref row, "📅 Số lịch hẹn hoàn thành", summary.appointmentsCount, false, null);
            AddSummaryRow(sheet, ref row, "📈 Giá trị TB/đơn", summary.avgOrderValue, false, WarningColor);

            // Border for summary table
            var summaryRange = sheet.Cells[$"A5:B{row - 1}"];
            AddBorders(summaryRange);

            row += 2;

            // ==================== DOANH THU THEO NGÀY ====================
            sheet.Cells[$"A{row}:D{row}"].Merge = true;
            sheet.Cells[$"A{row}"].Value = "📈 DOANH THU THEO NGÀY";
            StyleSectionHeader(sheet.Cells[$"A{row}:D{row}"]);
            row++;

            // Header
            StyleTableHeader(sheet.Cells[$"A{row}:D{row}"]);
            sheet.Cells[$"A{row}"].Value = "Ngày";
            sheet.Cells[$"B{row}"].Value = "Đơn hàng";
            sheet.Cells[$"C{row}"].Value = "Dịch vụ";
            sheet.Cells[$"D{row}"].Value = "Tổng";
            int chartStartRow = row;
            row++;

            foreach (var item in chartData)
            {
                sheet.Cells[$"A{row}"].Value = item.Date;
                sheet.Cells[$"B{row}"].Value = (decimal)item.Orders;
                sheet.Cells[$"C{row}"].Value = (decimal)item.Appointments;
                sheet.Cells[$"D{row}"].Value = (decimal)item.Orders + (decimal)item.Appointments;
                
                sheet.Cells[$"B{row}:D{row}"].Style.Numberformat.Format = "#,##0 ₫";
                
                // Alternate row color
                if ((row - chartStartRow) % 2 == 0)
                {
                    sheet.Cells[$"A{row}:D{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[$"A{row}:D{row}"].Style.Fill.BackgroundColor.SetColor(LightGray);
                }
                row++;
            }
            AddBorders(sheet.Cells[$"A{chartStartRow}:D{row - 1}"]);

            row += 2;

            // ==================== PHƯƠNG THỨC THANH TOÁN ====================
            sheet.Cells[$"A{row}:C{row}"].Merge = true;
            sheet.Cells[$"A{row}"].Value = "💳 PHƯƠNG THỨC THANH TOÁN";
            StyleSectionHeader(sheet.Cells[$"A{row}:C{row}"]);
            row++;

            StyleTableHeader(sheet.Cells[$"A{row}:C{row}"]);
            sheet.Cells[$"A{row}"].Value = "Phương thức";
            sheet.Cells[$"B{row}"].Value = "Số lượng";
            sheet.Cells[$"C{row}"].Value = "Doanh thu";
            int paymentStartRow = row;
            row++;

            foreach (var item in paymentMethods)
            {
                sheet.Cells[$"A{row}"].Value = item.Method;
                sheet.Cells[$"B{row}"].Value = (int)item.Count;
                sheet.Cells[$"C{row}"].Value = (decimal)item.Amount;
                sheet.Cells[$"C{row}"].Style.Numberformat.Format = "#,##0 ₫";
                row++;
            }
            AddBorders(sheet.Cells[$"A{paymentStartRow}:C{row - 1}"]);

            // Auto fit columns
            sheet.Cells.AutoFitColumns();
            sheet.Column(1).Width = 30;
            sheet.Column(2).Width = 20;
            sheet.Column(3).Width = 20;
            sheet.Column(4).Width = 20;

            // Footer
            row += 2;
            sheet.Cells[$"A{row}"].Value = $"Xuất báo cáo lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            sheet.Cells[$"A{row}"].Style.Font.Italic = true;
            sheet.Cells[$"A{row}"].Style.Font.Size = 10;

            return package.GetAsByteArray();
        }

        /// <summary>
        /// Xuất báo cáo sản phẩm
        /// </summary>
        public byte[] ExportProductsReport(
            IEnumerable<dynamic> bestsellers,
            IEnumerable<dynamic> lowstock,
            IEnumerable<dynamic> categories)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();

            // ==================== SHEET 1: SẢN PHẨM BÁN CHẠY ====================
            var sheet1 = package.Workbook.Worksheets.Add("Sản Phẩm Bán Chạy");
            int row = 1;

            sheet1.Cells["A1:F1"].Merge = true;
            sheet1.Cells["A1"].Value = "🏆 TOP SẢN PHẨM BÁN CHẠY - UME SALON";
            StyleTitle(sheet1.Cells["A1"], PrimaryColor);
            sheet1.Row(1).Height = 35;

            row = 3;
            StyleTableHeader(sheet1.Cells[$"A{row}:F{row}"]);
            sheet1.Cells[$"A{row}"].Value = "STT";
            sheet1.Cells[$"B{row}"].Value = "Tên sản phẩm";
            sheet1.Cells[$"C{row}"].Value = "Danh mục";
            sheet1.Cells[$"D{row}"].Value = "Giá bán";
            sheet1.Cells[$"E{row}"].Value = "Đã bán";
            sheet1.Cells[$"F{row}"].Value = "Doanh thu";
            int startRow = row;
            row++;

            int stt = 1;
            foreach (var p in bestsellers)
            {
                sheet1.Cells[$"A{row}"].Value = stt++;
                sheet1.Cells[$"A{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                // Top 3 highlight
                if (stt <= 4)
                {
                    var bgColor = stt == 2 ? Color.FromArgb(255, 215, 0) :   // Gold
                                  stt == 3 ? Color.FromArgb(192, 192, 192) : // Silver
                                  Color.FromArgb(205, 127, 50);              // Bronze
                    sheet1.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet1.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(bgColor);
                    sheet1.Cells[$"A{row}"].Style.Font.Bold = true;
                }

                sheet1.Cells[$"B{row}"].Value = p.Name;
                sheet1.Cells[$"C{row}"].Value = p.CategoryName;
                sheet1.Cells[$"D{row}"].Value = (decimal)p.Price;
                sheet1.Cells[$"D{row}"].Style.Numberformat.Format = "#,##0 ₫";
                sheet1.Cells[$"E{row}"].Value = (int)p.SoldCount;
                sheet1.Cells[$"E{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet1.Cells[$"F{row}"].Value = (decimal)p.Revenue;
                sheet1.Cells[$"F{row}"].Style.Numberformat.Format = "#,##0 ₫";
                sheet1.Cells[$"F{row}"].Style.Font.Bold = true;
                
                if ((row - startRow) % 2 == 0)
                {
                    sheet1.Cells[$"A{row}:F{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet1.Cells[$"A{row}:F{row}"].Style.Fill.BackgroundColor.SetColor(LightGray);
                }
                row++;
            }
            AddBorders(sheet1.Cells[$"A{startRow}:F{row - 1}"]);
            sheet1.Cells.AutoFitColumns();
            sheet1.Column(2).Width = 40;

            // ==================== SHEET 2: TỒN KHO THẤP ====================
            var sheet2 = package.Workbook.Worksheets.Add("Tồn Kho Thấp");
            row = 1;

            sheet2.Cells["A1:F1"].Merge = true;
            sheet2.Cells["A1"].Value = "⚠️ CẢNH BÁO TỒN KHO THẤP - UME SALON";
            StyleTitle(sheet2.Cells["A1"], DangerColor);
            sheet2.Row(1).Height = 35;

            row = 3;
            StyleTableHeader(sheet2.Cells[$"A{row}:F{row}"]);
            sheet2.Cells[$"A{row}"].Value = "STT";
            sheet2.Cells[$"B{row}"].Value = "Tên sản phẩm";
            sheet2.Cells[$"C{row}"].Value = "Danh mục";
            sheet2.Cells[$"D{row}"].Value = "Tồn kho";
            sheet2.Cells[$"E{row}"].Value = "Ngưỡng cảnh báo";
            sheet2.Cells[$"F{row}"].Value = "Trạng thái";
            startRow = row;
            row++;

            stt = 1;
            foreach (var p in lowstock)
            {
                sheet2.Cells[$"A{row}"].Value = stt++;
                sheet2.Cells[$"B{row}"].Value = p.Name;
                sheet2.Cells[$"C{row}"].Value = p.CategoryName;
                sheet2.Cells[$"D{row}"].Value = (int)p.StockQuantity;
                sheet2.Cells[$"E{row}"].Value = (int)p.LowStockThreshold;

                // Status with color
                var stock = (int)p.StockQuantity;
                string status = stock == 0 ? "Hết hàng" : stock < 5 ? "Cần nhập gấp" : "Tồn thấp";
                var statusColor = stock == 0 ? DangerColor : stock < 5 ? WarningColor : InfoColor;
                
                sheet2.Cells[$"F{row}"].Value = status;
                sheet2.Cells[$"F{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet2.Cells[$"F{row}"].Style.Fill.BackgroundColor.SetColor(statusColor);
                sheet2.Cells[$"F{row}"].Style.Font.Color.SetColor(Color.White);
                sheet2.Cells[$"F{row}"].Style.Font.Bold = true;
                sheet2.Cells[$"F{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                row++;
            }
            AddBorders(sheet2.Cells[$"A{startRow}:F{row - 1}"]);
            sheet2.Cells.AutoFitColumns();
            sheet2.Column(2).Width = 40;

            // ==================== SHEET 3: THEO DANH MỤC ====================
            var sheet3 = package.Workbook.Worksheets.Add("Theo Danh Mục");
            row = 1;

            sheet3.Cells["A1:D1"].Merge = true;
            sheet3.Cells["A1"].Value = "📊 THỐNG KÊ THEO DANH MỤC - UME SALON";
            StyleTitle(sheet3.Cells["A1"], InfoColor);
            sheet3.Row(1).Height = 35;

            row = 3;
            StyleTableHeader(sheet3.Cells[$"A{row}:D{row}"]);
            sheet3.Cells[$"A{row}"].Value = "Danh mục";
            sheet3.Cells[$"B{row}"].Value = "Số sản phẩm";
            sheet3.Cells[$"C{row}"].Value = "Đã bán";
            sheet3.Cells[$"D{row}"].Value = "Doanh thu";
            startRow = row;
            row++;

            foreach (var c in categories)
            {
                sheet3.Cells[$"A{row}"].Value = c.Name;
                sheet3.Cells[$"B{row}"].Value = (int)c.ProductCount;
                sheet3.Cells[$"B{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet3.Cells[$"C{row}"].Value = (int)c.TotalSold;
                sheet3.Cells[$"C{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet3.Cells[$"D{row}"].Value = (decimal)c.Revenue;
                sheet3.Cells[$"D{row}"].Style.Numberformat.Format = "#,##0 ₫";
                sheet3.Cells[$"D{row}"].Style.Font.Bold = true;
                
                if ((row - startRow) % 2 == 0)
                {
                    sheet3.Cells[$"A{row}:D{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet3.Cells[$"A{row}:D{row}"].Style.Fill.BackgroundColor.SetColor(LightGray);
                }
                row++;
            }
            AddBorders(sheet3.Cells[$"A{startRow}:D{row - 1}"]);
            sheet3.Cells.AutoFitColumns();
            sheet3.Column(1).Width = 30;

            return package.GetAsByteArray();
        }

        /// <summary>
        /// Xuất báo cáo nhân viên
        /// </summary>
        public byte[] ExportStaffReport(
            dynamic summary,
            IEnumerable<dynamic> staffData,
            DateTime startDate,
            DateTime endDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            
            using var package = new ExcelPackage();
            var sheet = package.Workbook.Worksheets.Add("Báo Cáo Nhân Viên");

            int row = 1;

            // ==================== TIÊU ĐỀ ====================
            sheet.Cells["A1:G1"].Merge = true;
            sheet.Cells["A1"].Value = "👥 BÁO CÁO HIỆU SUẤT NHÂN VIÊN - UME SALON";
            StyleTitle(sheet.Cells["A1"], PrimaryColor);
            sheet.Row(1).Height = 35;

            row = 2;
            sheet.Cells[$"A{row}:G{row}"].Merge = true;
            sheet.Cells[$"A{row}"].Value = $"Kỳ báo cáo: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
            sheet.Cells[$"A{row}"].Style.Font.Italic = true;
            sheet.Cells[$"A{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            row = 4;

            // ==================== TỔNG QUAN ====================
            sheet.Cells[$"A{row}:C{row}"].Merge = true;
            sheet.Cells[$"A{row}"].Value = "📊 TỔNG QUAN";
            StyleSectionHeader(sheet.Cells[$"A{row}:C{row}"]);
            row++;

            StyleTableHeader(sheet.Cells[$"A{row}:B{row}"]);
            sheet.Cells[$"A{row}"].Value = "Chỉ tiêu";
            sheet.Cells[$"B{row}"].Value = "Giá trị";
            row++;

            AddSummaryRow(sheet, ref row, "👥 Tổng số nhân viên", summary.staffCount, false, null);
            AddSummaryRow(sheet, ref row, "📅 Tổng lịch hẹn hoàn thành", summary.totalAppointments, false, InfoColor);
            AddSummaryRow(sheet, ref row, "💰 Tổng doanh thu dịch vụ", summary.totalRevenue, true, PrimaryColor);

            AddBorders(sheet.Cells[$"A5:B{row - 1}"]);

            row += 2;

            // ==================== CHI TIẾT NHÂN VIÊN ====================
            sheet.Cells[$"A{row}:G{row}"].Merge = true;
            sheet.Cells[$"A{row}"].Value = "📋 CHI TIẾT HIỆU SUẤT";
            StyleSectionHeader(sheet.Cells[$"A{row}:G{row}"]);
            row++;

            StyleTableHeader(sheet.Cells[$"A{row}:G{row}"]);
            sheet.Cells[$"A{row}"].Value = "Hạng";
            sheet.Cells[$"B{row}"].Value = "Nhân viên";
            sheet.Cells[$"C{row}"].Value = "Chức vụ";
            sheet.Cells[$"D{row}"].Value = "Cấp độ";
            sheet.Cells[$"E{row}"].Value = "Số lịch hẹn";
            sheet.Cells[$"F{row}"].Value = "Doanh thu";
            sheet.Cells[$"G{row}"].Value = "Đánh giá";
            int startRow = row;
            row++;

            int rank = 1;
            foreach (var s in staffData)
            {
                // Rank with medal
                string medal = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : $"{rank}";
                sheet.Cells[$"A{row}"].Value = medal;
                sheet.Cells[$"A{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                if (rank <= 3)
                {
                    var bgColor = rank == 1 ? Color.FromArgb(255, 215, 0) :
                                  rank == 2 ? Color.FromArgb(192, 192, 192) :
                                  Color.FromArgb(205, 127, 50);
                    sheet.Cells[$"A{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[$"A{row}"].Style.Fill.BackgroundColor.SetColor(bgColor);
                }

                sheet.Cells[$"B{row}"].Value = s.FullName;
                sheet.Cells[$"B{row}"].Style.Font.Bold = rank <= 3;
                sheet.Cells[$"C{row}"].Value = s.Position;
                sheet.Cells[$"D{row}"].Value = s.Level;
                sheet.Cells[$"E{row}"].Value = (int)s.AppointmentsCount;
                sheet.Cells[$"E{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                sheet.Cells[$"F{row}"].Value = (decimal)s.Revenue;
                sheet.Cells[$"F{row}"].Style.Numberformat.Format = "#,##0 ₫";
                sheet.Cells[$"F{row}"].Style.Font.Bold = true;
                
                // Rating với màu
                var rating = s.AverageRating ?? 0;
                sheet.Cells[$"G{row}"].Value = $"⭐ {rating:F1}";
                var ratingColor = rating >= 4.5m ? SuccessColor : rating >= 3.5m ? WarningColor : DangerColor;
                sheet.Cells[$"G{row}"].Style.Font.Color.SetColor(ratingColor);
                sheet.Cells[$"G{row}"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                if ((row - startRow) % 2 == 0)
                {
                    sheet.Cells[$"A{row}:G{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    sheet.Cells[$"A{row}:G{row}"].Style.Fill.BackgroundColor.SetColor(LightGray);
                }

                rank++;
                row++;
            }
            AddBorders(sheet.Cells[$"A{startRow}:G{row - 1}"]);

            // Auto fit
            sheet.Cells.AutoFitColumns();
            sheet.Column(2).Width = 25;

            // Footer
            row += 2;
            sheet.Cells[$"A{row}"].Value = $"Xuất báo cáo lúc: {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            sheet.Cells[$"A{row}"].Style.Font.Italic = true;
            sheet.Cells[$"A{row}"].Style.Font.Size = 10;

            return package.GetAsByteArray();
        }

        #region Helper Methods

        private static void StyleTitle(ExcelRange range, Color bgColor)
        {
            range.Style.Font.Size = 18;
            range.Style.Font.Bold = true;
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            range.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(bgColor);
            range.Style.Font.Color.SetColor(Color.White);
        }

        private static void StyleSectionHeader(ExcelRange range)
        {
            range.Style.Font.Size = 14;
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(HeaderColor);
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
        }

        private static void StyleTableHeader(ExcelRange range)
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(91, 192, 222));
            range.Style.Font.Color.SetColor(Color.White);
            range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        private static void AddSummaryRow(ExcelWorksheet sheet, ref int row, string label, object value, bool highlight, Color? bgColor)
        {
            sheet.Cells[$"A{row}"].Value = label;
            sheet.Cells[$"B{row}"].Value = value;
            
            if (value is decimal d)
            {
                sheet.Cells[$"B{row}"].Style.Numberformat.Format = "#,##0 ₫";
            }

            if (highlight)
            {
                sheet.Cells[$"A{row}:B{row}"].Style.Font.Bold = true;
                sheet.Cells[$"A{row}:B{row}"].Style.Font.Size = 12;
            }

            if (bgColor.HasValue)
            {
                sheet.Cells[$"B{row}"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                sheet.Cells[$"B{row}"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(
                    (int)(bgColor.Value.R * 0.2 + 255 * 0.8),
                    (int)(bgColor.Value.G * 0.2 + 255 * 0.8),
                    (int)(bgColor.Value.B * 0.2 + 255 * 0.8)
                ));
            }

            row++;
        }

        private static void AddBorders(ExcelRange range)
        {
            range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
        }

        #endregion
    }
}
