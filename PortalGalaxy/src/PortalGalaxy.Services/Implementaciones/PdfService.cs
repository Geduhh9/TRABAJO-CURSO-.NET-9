using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using PortalGalaxy.Common.Request;
using PortalGalaxy.Common.Response;
using PortalGalaxy.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace PortalGalaxy.Services.Implementaciones;

public class PdfService : IPdfService
{
    private readonly ITallerService _tallerService;
    private readonly ILogger<PdfService> _logger;
    private readonly IWebHostEnvironment _env;

    public PdfService(ITallerService tallerService, ILogger<PdfService> logger, IWebHostEnvironment env)
    {
        _tallerService = tallerService;
        _logger = logger;
        _env = env;
    }

    //public async Task<BaseResponse<Document>> Generar(BusquedaTallerRequest request)
    //{
    //    var response = new BaseResponse<Document>();

    //    try
    //    {
    //        var data = await _tallerService.ListAsync(request);
    //        if (data is { Success: true, TotalPages: > 0, Data: not null })
    //        {
    //            QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

    //            var doc = Document.Create(document =>
    //            {
    //                document.Page(page =>
    //                {
    //                    page.MarginLeft(20);
    //                    page.MarginTop(20);
    //                    page.MarginRight(10);
    //                    page.Header().Row(row =>
    //                    {
    //                        row.ConstantItem(120).Height(80).AlignCenter().PaddingTop(20).Text("LISTADO DE TALLERES");
    //                    });
    //                    page.Content().PaddingVertical(15).Column(col =>
    //                    {
    //                        col.Item().PaddingTop(10).Row(row =>
    //                        {
    //                            row.RelativeItem().AlignCenter().Text("ID");
    //                            row.RelativeItem().AlignCenter().Text("Nombre");
    //                            row.RelativeItem().AlignCenter().Text("Categoria");
    //                            row.RelativeItem().AlignCenter().Text("Instructor");
    //                            row.RelativeItem().AlignCenter().Text("Fecha");
    //                            row.RelativeItem().AlignCenter().Text("Situacion");
    //                        });
    //                        col.Item().Border(0.5f).Row(row =>
    //                        {
    //                            row.RelativeItem().Column(c =>
    //                            {
    //                                foreach (var taller in data.Data)
    //                                {
    //                                    c.Item().Row(r =>
    //                                    {
    //                                        r.RelativeItem().Text(taller.Id.ToString()).TextData();
    //                                        r.RelativeItem().Text(taller.Taller).TextData();
    //                                        r.RelativeItem().Text(taller.Categoria).TextData();
    //                                        r.RelativeItem().Text(taller.Instructor).TextData();
    //                                        r.RelativeItem().Text(taller.Fecha).TextData();
    //                                        r.RelativeItem().Text(taller.Situacion).TextData();
    //                                    });
    //                                }
    //                            });
    //                        });
    //                    });
    //                });
    //            });

    //            response.Data = doc;
    //            response.Success = true;
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        response.ErrorMessage = "Error al generar el PDF";
    //        _logger.LogError(ex, "{ErrorMessage} {Message}", response.ErrorMessage, ex.Message);
    //    }

    //    return response;
    //}


    public async Task<BaseResponse<Document>> Generar(BusquedaTallerRequest request, byte[]? logoBytes = null)
    {
        var response = new BaseResponse<Document>();

        try
        {
            var data = await _tallerService.ListAsync(request);
            if (data is { Success: true, TotalPages: > 0, Data: not null })
            {
                QuestPDF.Settings.CheckIfAllTextGlyphsAreAvailable = false;

                var now = DateTime.Now;

                IContainer CellHeader(IContainer c) => c
                    .Background(Colors.Grey.Lighten3)
                    .Border(0.5f).BorderColor(Colors.Grey.Lighten2)
                    .PaddingVertical(6).PaddingHorizontal(4);

                IContainer Cell(IContainer c, string? bg = null) => c
                    .Background(bg ?? Colors.White)
                    .BorderBottom(0.25f).BorderColor(Colors.Grey.Lighten3)
                    .PaddingVertical(5).PaddingHorizontal(4);

                var doc = Document.Create(document =>
                {
                    document.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(30);
                        page.DefaultTextStyle(x => x.FontSize(10));

                        page.Header().Column(header =>
                        {
                            header.Item().Row(row =>
                            {
                                if (logoBytes is not null)
                                    row.ConstantItem(50).Height(30).Image(logoBytes).FitArea();

                                row.RelativeItem().Column(col =>
                                {
                                    col.Item().Text("LISTADO DE TALLERES").FontSize(16).SemiBold();
                                    col.Item().Text($"Generado: {now:dd/MM/yyyy HH:mm}")
                                        .FontSize(9).FontColor(Colors.Grey.Darken2);
                                });
                            });

                            header.Item().PaddingTop(8).LineHorizontal(0.75f)
                                  .LineColor(Colors.Grey.Lighten2);
                        });

                        page.Content().PaddingTop(10).Element(content =>
                        {
                            content.Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);
                                    columns.RelativeColumn(2.2f);
                                    columns.RelativeColumn(1.6f);
                                    columns.RelativeColumn(1.6f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1.2f);
                                });

                                table.Header(h =>
                                {
                                    h.Cell().Element(CellHeader).Text("ID").SemiBold();
                                    h.Cell().Element(CellHeader).Text("Nombre").SemiBold();
                                    h.Cell().Element(CellHeader).Text("Categoría").SemiBold();
                                    h.Cell().Element(CellHeader).Text("Instructor").SemiBold();
                                    h.Cell().Element(CellHeader).Text("Fecha").SemiBold();
                                    h.Cell().Element(CellHeader).Text("Situación").SemiBold();
                                });

                                var i = 0;
                                foreach (var t in data.Data)
                                {
                                    var bg = (i++ % 2 == 0) ? Colors.White : Colors.Grey.Lighten5;
                                    table.Cell().Element(c => Cell(c, bg)).Text(t.Id.ToString()).AlignCenter();
                                    table.Cell().Element(c => Cell(c, bg)).Text(t.Taller).WrapAnywhere();
                                    table.Cell().Element(c => Cell(c, bg)).Text(t.Categoria ?? "-").WrapAnywhere();
                                    table.Cell().Element(c => Cell(c, bg)).Text(t.Instructor ?? "-").WrapAnywhere();
                                    table.Cell().Element(c => Cell(c, bg)).Text(t.Fecha ?? "-").AlignCenter();
                                    table.Cell().Element(c => Cell(c, bg)).Text(t.Situacion ?? "-").AlignCenter();
                                }
                            });
                        });

                        page.Footer().AlignRight().Text(txt =>
                        {
                            txt.Span("Página ").FontSize(9).FontColor(Colors.Grey.Darken2);
                            txt.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken2);
                            txt.Span(" de ").FontSize(9).FontColor(Colors.Grey.Darken2);
                            txt.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                    });
                });

                response.Data = doc;
                response.Success = true;
            }
        }
        catch (Exception ex)
        {
            response.ErrorMessage = "Error al generar el PDF";
        }

        return response;
    }
}