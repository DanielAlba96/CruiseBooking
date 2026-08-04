using Shared.Domain.Models;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;

namespace Jobs.Application.Services.Impl;

/// <summary>
/// Genera el PDF de la factura de una reserva a partir de sus datos, usando MigraDoc.
/// </summary>
internal class InvoiceService : IInvoiceService
{
    /// <summary>
    /// Construye el PDF de la factura para la reserva indicada.
    /// </summary>
    /// <param name="booking">Datos de la reserva a facturar.</param>
    /// <returns>El contenido del PDF generado.</returns>
    public byte[] BuildInvoice(BookingInvoiceData booking)
    {
        var document = CreateDocument(booking);
        var renderer = new PdfDocumentRenderer { Document = document };
        renderer.RenderDocument();

        using var stream = new MemoryStream();
        renderer.PdfDocument.Save(stream);
        return stream.ToArray();
    }

    private static Document CreateDocument(BookingInvoiceData booking)
    {
        var document = new Document();
        var section = document.AddSection();
        section.PageSetup.TopMargin = "1cm";

        ComposeCompanyBlock(section);
        ComposeHeader(booking, section);
        ComposeCruiseData(booking, section);
        ComposeProductsTables(booking, section);
        ComposeTotals(booking, section);

        return document;
    }

    private static void ComposeHeader(BookingInvoiceData booking, Section section)
    {
        var title = section.AddParagraph("FACTURA DE COMPRA");
        title.Style = StyleNames.Heading1;
        title.Format.Font.Color = Color.Parse("#72ace6");
        title.Format.Font.Size = 20;
        title.Format.Font.Bold = true;
        title.Format.SpaceAfter = 10;

        AddTextLabelValue(section, "Fecha emisión: ", $"{booking.ChargedAt:dd / MM / yyyy HH:mm}");
        AddTextLabelValue(section, "Identificador de la reserva: ", $"{booking.Id}", 20);
    }

    private static void ComposeCompanyBlock(Section section)
    {
        var table = section.AddTable();
        table.Borders.Width = 0;
        table.Format.SpaceAfter = 15;
        table.BottomPadding = 20;

        table.AddColumn("8cm");
        table.AddColumn("8cm");

        var row = table.AddRow();
        row.VerticalAlignment = VerticalAlignment.Center;

        var logoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "company_logo.jpg");
        if (File.Exists(logoPath))
        {
            var img = row.Cells[0].AddParagraph().AddImage(logoPath);
            img.Height = "1.5cm";
            img.LockAspectRatio = true;
        }

        var right = row.Cells[1];

        var name = right.AddParagraph("Cruceros Marejada S.L.");
        name.Format.Font.Bold = true;
        name.Format.Font.Size = 11;
        name.Format.Font.Color = Color.Parse("#72ace6");
        name.Format.Alignment = ParagraphAlignment.Right;

        foreach (var line in (string[])["CIF: B-12345678", "Calle del Mar, 14 · 03001 Alicante",
                                        "Tel: +34 999 999 999  ·  info@marejadacruceros.es",
                                        "www.marejadacruceros.es"])
        {
            var p = right.AddParagraph(line);
            p.Format.Alignment = ParagraphAlignment.Right;
            p.Format.SpaceAfter = 3;
        }
    }

    private static void ComposeCruiseData(BookingInvoiceData booking, Section section)
    {
        var cruiseTitle = section.AddParagraph("Datos del crucero");
        cruiseTitle.Format.Font.Bold = true;
        cruiseTitle.Format.Font.Color = Color.Parse("#72ace6");
        cruiseTitle.Format.Font.Size = 13;
        cruiseTitle.Format.SpaceAfter = 5;
        cruiseTitle.Style = StyleNames.Heading2;

        AddTextLabelValue(section, "Nombre: ", booking.CruiseName);
        AddTextLabelValue(section, "Código: ", booking.CruiseCode);
        AddTextLabelValue(section, "Puerto origen: ", booking.OriginPort);
        AddTextLabelValue(section, "Salida: ", booking.StartDate.ToString("dd/MM/yyyy HH:mm"));
        AddTextLabelValue(section, "Duración: ", $"{booking.DurationInDays} días");
        AddTextLabelValue(section, "Barco: ", $"{booking.ShipName}  ({booking.ShipCompany})");
        AddTextLabelValue(section, "Ruta: ", booking.Itinerary, spaceAfter: 15);
    }

    private static void ComposeProductsTables(BookingInvoiceData booking, Section section)
    {
        var cruiseTitle = section.AddParagraph("Resumen de productos");
        cruiseTitle.Format.Font.Bold = true;
        cruiseTitle.Format.Font.Color = Color.Parse("#72ace6");
        cruiseTitle.Format.Font.Size = 13;
        cruiseTitle.Format.SpaceAfter = 5;
        cruiseTitle.Style = StyleNames.Heading2;

        ComposeCabinsTable(booking, section);
        ComposeExtrasTable(booking, section);
    }

    private static void ComposeCabinsTable(BookingInvoiceData booking, Section section)
    {
        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.BottomPadding = 5;

        table.AddColumn("9cm");
        table.AddColumn("3cm");
        table.AddColumn("4cm");

        var header = table.AddRow();
        header.Height = "1cm";
        header.HeightRule = RowHeightRule.AtLeast;
        header.Shading.Color = Color.Parse("#72ace6");
        header.Cells[0].VerticalAlignment = VerticalAlignment.Center;
        header.Cells[1].VerticalAlignment = VerticalAlignment.Center;
        header.Cells[2].VerticalAlignment = VerticalAlignment.Center;
        header.Cells[0].AddParagraph("Camarote").Format.Font.Bold = true;
        header.Cells[1].AddParagraph("Pasajeros").Format.Font.Bold = true;
        header.Cells[2].AddParagraph("Precio").Format.Font.Bold = true;
        header.Cells[2].Format.Alignment = ParagraphAlignment.Right;

        foreach (var cabin in booking.Cabins)
        {
            var row = table.AddRow();
            row.TopPadding = 5;
            row.Format.LeftIndent = 5;
            row.Format.RightIndent = 5;
            row.Cells[0].AddParagraph(cabin.TypeName).Format.Font.Color = Color.Parse("#e6ac72");
            row.Cells[0].AddParagraph(cabin.TypeDescription);
            row.Cells[1].AddParagraph($"{cabin.Occupants}");
            row.Cells[2].AddParagraph($"{cabin.Price:C}").Format.Font.Color = Color.Parse("#72ace6");
            row.Cells[2].Format.Alignment = ParagraphAlignment.Right;
        }
    }

    private static void ComposeExtrasTable(BookingInvoiceData booking, Section section)
    {
        if (booking.Extras.Count == 0) return;

        var table = section.AddTable();
        table.Borders.Width = 0.5;
        table.BottomPadding = 5;

        table.AddColumn("12cm");
        table.AddColumn("4cm");

        var header = table.AddRow();
        header.Height = "1cm";
        header.HeightRule = RowHeightRule.AtLeast;
        header.Shading.Color = Color.Parse("#72ace6");
        header.Cells[0].VerticalAlignment = VerticalAlignment.Center;
        header.Cells[1].VerticalAlignment = VerticalAlignment.Center;
        header.Cells[0].AddParagraph("Extra").Format.Font.Bold = true;
        header.Cells[1].AddParagraph("Precio").Format.Font.Bold = true;
        header.Cells[1].Format.Alignment = ParagraphAlignment.Right;

        foreach (var extra in booking.Extras)
        {
            var row = table.AddRow();
            row.TopPadding = 5;
            row.Format.LeftIndent = 5;
            row.Format.RightIndent = 5;
            row.Cells[0].AddParagraph(extra.Name).Format.Font.Color = Color.Parse("#e6ac72");
            row.Cells[0].AddParagraph(extra.Description);
            row.Cells[1].AddParagraph($"{extra.Price:C}").Format.Font.Color = Color.Parse("#72ace6");
            row.Cells[1].Format.Alignment = ParagraphAlignment.Right;
        }
    }

    private static void ComposeTotals(BookingInvoiceData booking, Section section)
    {
        var subtotal = booking.Cabins.Sum(c => c.Price)
                     + booking.Extras.Sum(e => e.Price);
        var taxes = booking.ChargeAmount - subtotal;
        var taxRate = taxes * 100 / subtotal;

        AddPriceLabelValue(section, "Subtotal: ", subtotal);
        AddPriceLabelValue(section, $"Impuestos ({taxRate:0.##}%): ", taxes);
        AddPriceLabelValue(section, "TOTAL: ", booking.ChargeAmount);
    }

    private static void AddTextLabelValue(Section section, string label, string value, int spaceAfter = 5)
    {
        var p = section.AddParagraph();
        p.Format.SpaceAfter = spaceAfter;

        var labelText = p.AddFormattedText(label);
        labelText.Font.Bold = true;
        labelText.Font.Color = Color.Parse("#e6ac72");

        p.AddFormattedText(value);
    }

    private static void AddPriceLabelValue(Section section, string label, decimal value)
    {
        var paragraph = section.AddParagraph();
        paragraph.Format.Font.Size = 12;
        paragraph.Format.Font.Bold = true;
        paragraph.Format.Alignment = ParagraphAlignment.Right;
        paragraph.Format.SpaceAfter = 5;

        paragraph.AddFormattedText(label);

        var price = paragraph.AddFormattedText($"{value:C}");
        price.Font.Color = Color.Parse("#72ace6");
    }
}
