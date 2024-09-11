using DocumentFormat.OpenXml.Packaging;
// WordTemplateService.cs
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using CoreFirstTask.Models;


namespace CoreFirstTask.DataverseService
{
    public class WordTemplateService
    {
        //public byte[] PopulateTemplate(Order order, string templatePath)
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        // Load template into memory stream
        //        using (var fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
        //        {
        //            fileStream.CopyTo(memoryStream);
        //        }   

        //        // Modify the memory stream
        //        using (var wordDoc = WordprocessingDocument.Open(memoryStream, true))
        //        {
        //            var docText = string.Empty;
        //            using (StreamReader reader = new StreamReader(wordDoc.MainDocumentPart.GetStream()))
        //            {
        //                docText = reader.ReadToEnd();
        //            }
        //            var myStatus = order.statecode;
        //            var statusText = "";

        //            switch (myStatus)
        //            {
        //                case 0:
        //                    statusText = "Active";
        //                    break;

        //                case 1:
        //                    statusText = "Submitted";
        //                    break;

        //                case 2:
        //                    statusText = "Canceled";
        //                    break;

        //                case 3:
        //                    statusText = "FulFilled";
        //                    break;

        //                case 4:
        //                    statusText = "Invoiced";
        //                    break;

        //                default:
        //                    statusText = "--";
        //                    break;
        //            }

        //            var OrderId = order.salesorderid.ToString();
        //            var Name = order.name;
        //            var CustomerName = order.customername;
        //            var TotalAmount = order.totalamount.ToString("C");


        //            // Replace placeholders with actual data
        //            docText = docText.Replace("{OrderId}", OrderId);
        //            docText = docText.Replace("{Name}", Name);
        //            docText = docText.Replace("{CustomerName}", CustomerName);
        //            docText = docText.Replace("{TotalAmount}", TotalAmount);
        //            docText = docText.Replace("{Status}", statusText);

        //            using (StreamWriter writer = new StreamWriter(wordDoc.MainDocumentPart.GetStream(FileMode.Create)))
        //            {
        //                writer.Write(docText);
        //            }

        //            wordDoc.MainDocumentPart.Document.Save();
        //        }

        //        return memoryStream.ToArray();
        //    }
        //}

        public byte[] PopulateTemplate(Order order, string templatePath)
        {
            using (var memoryStream = new MemoryStream())
            {
                // Load template into memory stream
                using (var fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
                {
                    fileStream.CopyTo(memoryStream);
                }

                // Reset the position of the memory stream to the beginning
                memoryStream.Position = 0;

                // Modify the memory stream
                using (var wordDoc = WordprocessingDocument.Open(memoryStream, true))
                {
                    // Get the main document part
                    var mainPart = wordDoc.MainDocumentPart;

                    // Replace content controls with actual data
                    ReplaceContentControl(mainPart, "OrderId", order.salesorderid.ToString());
                    ReplaceContentControl(mainPart, "Name", order.name);
                    ReplaceContentControl(mainPart, "CustomerName", order.customername);
                    ReplaceContentControl(mainPart, "TotalAmount", order.totalamount.ToString("C"));
                    ReplaceContentControl(mainPart, "Status", GetStatusText(order.statecode));

                    // Save changes to the document
                    mainPart.Document.Save();
                }

                // Reset position before returning
                memoryStream.Position = 0;
                return memoryStream.ToArray();
            }
        }

        private void ReplaceContentControl(MainDocumentPart mainPart, string tag, string replacement)
        {
            // Find the content control by tag
            var contentControls = mainPart.Document.Body.Descendants<SdtElement>()
                .Where(sdt => sdt.SdtProperties.Descendants<Tag>().Any(t => t.Val.Value == tag));

            foreach (var sdt in contentControls)
            {
                // Replace the text within the content control
                var textElement = sdt.Descendants<Text>().FirstOrDefault();
                if (textElement != null)
                {
                    textElement.Text = replacement;
                }
            }
        }


        private string GetStatusText(int statecode)
        {
            return statecode switch
            {
                0 => "Active",
                1 => "Submitted",
                2 => "Canceled",
                3 => "Fulfilled",
                4 => "Invoiced",
                _ => "--"
            };
        }

    }
}






