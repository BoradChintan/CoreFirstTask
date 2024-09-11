using DinkToPdf.Contracts;
using DinkToPdf;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using CoreFirstTask.Models;
using iText.Kernel.Colors;
using iText.Layout.Properties;


namespace CoreFirstTask.DataverseService
{
    public class PdfService
    {
        public byte[] GeneratePdf(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice), "Invoice data is null.");

            using (var memoryStream = new MemoryStream())
            {
                try
                {
                    //Hello
                    // Create a new PDF document
                    using (var writer = new PdfWriter(memoryStream))
                    {
                        using (var pdf = new PdfDocument(writer))
                        {
                            var document = new Document(pdf);

                            // Add title with color
                            var title = new Paragraph("Invoice Details")
                                .SetFontSize(18)
                                .SetBold()
                                .SetBackgroundColor(new DeviceRgb(0, 102, 204)) // Blue background
                                .SetFontColor(ColorConstants.WHITE); // White text
                            document.Add(title);

                            // Add invoice details
                            document.Add(new Paragraph($"Invoice Number: {invoice.invoicenumber ?? "N/A"}")
                                .SetFontSize(12)
                                .SetFontColor(new DeviceRgb(0, 51, 102))); // Dark blue text

                            document.Add(new Paragraph($"Customer Name: {invoice.customeridname ?? "N/A"}")
                                .SetFontSize(12)
                                .SetFontColor(new DeviceRgb(0, 51, 102))); // Dark blue text

                            document.Add(new Paragraph($"Invoice Date: {invoice.createdon.ToString("yyyy-MM-dd") ?? "N/A"}")
                                .SetFontSize(12)
                                .SetFontColor(new DeviceRgb(0, 51, 102))); // Dark blue text

                            document.Add(new Paragraph($"Invoice Details: {invoice.name ?? "N/A"}")
                                .SetFontSize(12)
                                .SetFontColor(new DeviceRgb(0, 51, 102))); // Dark blue text

                            // Add a table with data
                            var table = new Table(UnitValue.CreatePercentArray(2)) // 2 columns with equal width
                                .UseAllAvailableWidth();

                            table.AddHeaderCell(new Cell().Add(new Paragraph("Attribute").SetBold()));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Value").SetBold()));

                            table.AddCell("Invoice Number");
                            table.AddCell(invoice.invoicenumber ?? "N/A");

                            table.AddCell("Customer Name");
                            table.AddCell(invoice.customeridname ?? "N/A");

                            table.AddCell("Invoice Date");
                            table.AddCell(invoice.createdon.ToString("yyyy-MM-dd") ?? "N/A");

                            table.AddCell("Invoice Details");
                            table.AddCell(invoice.name ?? "N/A");

                            document.Add(table);

                            // Close the document
                            document.Close();
                        }
                    }

                    // Return the PDF as a byte array
                    return memoryStream.ToArray();
                }
                catch (Exception ex)
                {
                    // Log or handle the exception as needed
                    Console.WriteLine($"An error occurred while generating the PDF: {ex.Message}");
                    throw; // Re-throw the exception if necessary
                }
            }
        }
    }
}

