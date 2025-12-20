using Microsoft.AspNetCore.Mvc;
using PDF_CREATE.Model;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using static System.Net.Mime.MediaTypeNames;

namespace PDF_CREATE.Controllers;

[ApiController]
[Route("[controller]")]
public class JournalController : ControllerBase
{
    private readonly IFileGenerator _generator;
    public JournalController(IFileGenerator generator)
    {
        _generator = generator;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var journalRows = GetJournalRowList();
        var settings = CreateSettings(journalRows);

        // تولید PDF به صورت byte[]
        var pdfBytes = _generator.GeneratePdf(doc =>
        {
            doc.Page(page =>
            {
                page.Size(settings.Landscape ? PageSizes.A3.Landscape() : PageSizes.A3);
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontFamily(settings.FontName).FontSize(settings.BaseFontSize));

                BuildWatermark(page, settings);
                BuildHeader(page, settings);

                page.Content().Column(content =>
                {
                    BuildJournalHeader(content, settings);
                    BuildTable(content, settings);
                });

                BuildFooter(page, settings);
            });
        });

        // مسیر ذخیره‌سازی روی دیسک
        string dir = @"C:\Users\r.meshkisani\Desktop\Temp-Done";

        // اگر مسیر وجود نداشت ایجادش کن
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        // نام فایل یکتا
        string filePath = Path.Combine(dir, $"{Guid.NewGuid()}.pdf");

        // ذخیره pdf روی دیسک
        System.IO.File.WriteAllBytes(filePath, pdfBytes);

        return Ok($"Saved to: {filePath}");
    }


    // ---------------------------------------------------------------------
    private static void BuildWatermark(PageDescriptor page, PdfTableSettings settings)
    {
        page.Foreground().AlignCenter().AlignMiddle()
            .Rotate(5)
            .Text(settings.Text.Watermark)
            .FontSize(60)
            .FontColor(Colors.Grey.Darken2.WithAlpha(0.15f));
    }

    private static void BuildHeader(PageDescriptor page, PdfTableSettings settings)
    {
        page.Header().ShowOnce().Column(col =>
        {
            col.Item().AlignCenter().Element(e =>
            {
                var path = "Assets/Images/logo.PNG";
                var bytes = System.IO.File.ReadAllBytes(path);
                var img = QuestPDF.Infrastructure.Image.FromBinaryData(bytes);
                e.Width(80).Height(80).Image(img).FitArea();
            });

            col.Item().AlignCenter().PaddingBottom(5)
                .Text(settings.Text.HeaderTop).FontSize(10);

            col.Item().AlignCenter()
                .Text(settings.Text.HeaderTitle).FontSize(22).Bold();
        });
    }

    private static void BuildFooter(PageDescriptor page, PdfTableSettings settings)
    {
        page.Footer().AlignCenter().Text(t =>
        {
            t.Span(settings.Text.FooterText);
            t.CurrentPageNumber();
        });
    }

    private static void BuildJournalHeader(ColumnDescriptor content, PdfTableSettings settings)
    {
        content.Item().Row(r =>
        {
            r.RelativeItem().AlignLeft()
                .Text(settings.Text.VoucherStatus);

            r.RelativeItem().AlignRight().Column(c =>
            {
                c.Item().AlignRight().Text(settings.Text.VoucherNumber);
                c.Item().AlignRight().Text(settings.Text.VoucherDate);
            });
        });

        content.Item().PaddingBottom(10);
    }

    private static void BuildTable(ColumnDescriptor content, PdfTableSettings settings)
    {
        content.Item().Element(container =>
        {
            if (settings.Direction == PdfDirection.RTL)
            {
                container = container.ContentFromRightToLeft();
            }

            container.Extend().Table(table =>
            {
                var active = settings.Columns.Where(c => c.Visibility).OrderBy(c => c.Order).ToList();

                table.ColumnsDefinition(cols =>
                {
                    for (int i = 0; i < active.Count; i++)
                    {
                        ColumnSetting c = active[i];
                        cols.RelativeColumn(c.Width);
                    }
                });

                table.Header(h =>
                {
                    for (int i = 0; i < active.Count; i++)
                    {
                        ColumnSetting c = active[i];
                        h.Cell().Border(1).Padding(5).AlignCenter().Text(c.Header).Bold();
                    }
                });

                BuildTableRows(table, settings, active);
                BuildFinalTotalRow(table, settings, active);
                BuildDescriptionRow(table, settings, active);
            });
        });
    }

    private static void BuildTableRows(TableDescriptor table, PdfTableSettings settings, List<ColumnSetting> active)
    {
        var totalWidth = active.Sum(c => c.Width);

        for (int r = 0; r < settings.Rows.Length; r++)
        {
            string[] row = settings.Rows[r];

            for (int cIndex = 0; cIndex < active.Count; cIndex++)
            {
                ColumnSetting col = active[cIndex];
                string value = string.Empty;

                if (col.Index >= 0 && col.Index < row.Length)
                {
                    value = row[col.Index];
                }

                BuildTableCell(table, col, value, totalWidth, settings.BaseFontSize);
            }
        }
    }

    private static void BuildTableCell(TableDescriptor table, ColumnSetting col, string value, float totalWidth, float baseFont)
    {
        if (col.IsNumeric)
        {
            value = FormatNumber(value);
        }

        // استفاده از نسخه هوشمند برای هر سلول
        string txt = FitTextSmart(
            text: value,
            columnWidth: col.Width,
            baseFont: baseFont,
            out float fs,
            tableWidth: totalWidth
        );

        var cell = table.Cell().Border(1).Padding(5);

        if (col.Align == ColumnAlign.Center)
        {
            cell = cell.AlignCenter();
        }
        else if (col.Align == ColumnAlign.Left)
        {
            cell = cell.AlignLeft();
        }
        else
        {
            cell = cell.AlignRight();
        }

        cell.Text(txt).FontSize(fs);
    }


    private static void BuildFinalTotalRow(TableDescriptor table, PdfTableSettings settings, List<ColumnSetting> active)
    {
        for (int i = 0; i < active.Count; i++)
        {
            ColumnSetting col = active[i];
            var isFirst = i == 0;
            var isLast = i == active.Count - 1;
            var isSecondLast = active.Count >= 2 && i == active.Count - 2;

            var cell = table.Cell();

            if (isFirst)
            {
                cell.BorderRight(1).BorderBottom(1).Padding(5).Text("");
            }
            else if (isSecondLast)
            {
                cell.Border(1).Padding(5).Text("");
            }
            else if (isLast)
            {
                cell.Border(1).Padding(5).AlignCenter().Text("۴۶");
            }
            else
            {
                cell.BorderBottom(1).Padding(5).Text("");
            }
        }
    }

    private static void BuildDescriptionRow(TableDescriptor table, PdfTableSettings settings, List<ColumnSetting> active)
    {
        if (active.Count == 0)
        {
            return;
        }

        var cell = table.Cell();
        cell.ColumnSpan((uint)active.Count)
            .Padding(5)
            .Text($"{settings.Text.DescriptionTitle} {settings.Text.DescriptionText}");
    }

    // ---------------------------------------------------------------------
    private static string FormatNumber(string v)
    {
        if (decimal.TryParse(v, out var n))
        {
            return $"{n:N0}";
        }

        return v;
    }

    private static string FitTextSmart(string text, float columnWidth, float baseFont, out float fontSize, float tableWidth = 100, float minFont = 6, float maxFont = 30)
    {
        if (string.IsNullOrEmpty(text))
        {
            fontSize = baseFont;
            return string.Empty;
        }

        // محاسبه درصد عرض ستون نسبت به کل جدول
        float widthRatio = columnWidth / tableWidth;

        // تخمین طول متن به نسبت کاراکتر (می‌توان عدد 1.0 را تغییر داد برای فونت‌های باریک یا عریض)
        float estimatedTextLength = text.Length * 1.0f;

        // نسبت فونت = درصد عرض ستون / طول متن * اندازه پایه
        fontSize = baseFont * widthRatio * 10 / (estimatedTextLength / 10); // 10 یک فاکتور تنظیم برای ترازبندی بهتر

        // محدود کردن فونت
        if (fontSize < minFont)
        {
            fontSize = minFont;
        }
        else if (fontSize > maxFont)
        {
            fontSize = maxFont;
        }

        return text;
    }


    private static PdfTableSettings CreateSettings(List<JournalRow> rows)
    {
        return new PdfTableSettings
        {
            Landscape = true,
            FontName = "B NAZANIN",
            Direction = PdfDirection.RTL,
            BaseFontSize = 20,
            Text = DefaultTextSettings(),
            Columns = DefaultColumns(),
            Rows = rows.Select(r => new[]
            {
                r.RowNumber.ToString(),
                r.DetailAccountCode,
                r.AssistantAccountCode,
                r.LedgerAccountCode,
                r.DetailName1,
                r.MoeinName,
                r.KolName,
                r.Comment,
                r.Debit.ToString(),
                r.Credit.ToString()
            }).ToArray()
        };
    }

    private static PdfDocumentTextSettings DefaultTextSettings()
    {
        return new PdfDocumentTextSettings
        {
            Watermark = "cloudacc.mahaksoft.com",
            HeaderTop = "dev3",
            HeaderTitle = "سند حسابداری",
            VoucherStatus = "وضعیت سند: موقت",
            VoucherNumber = "شماره سند: ۱",
            VoucherDate = "تاریخ سند: ۱۴۰۳/۰۲/۲۳",
            DescriptionTitle = "شرح سند:",
            DescriptionText = "هزینه ها و مخارج و ...",
            FooterText = "صفحه "
        };
    }

    private static List<ColumnSetting> DefaultColumns()
    {
        return new List<ColumnSetting>
        {
            new() { Header="ردیف", Index=0, Width=1, Order=1, Align=ColumnAlign.Center, Visibility=true },
            new() { Header="تفصیلی", Index=1, Width=2, Order=2, Align=ColumnAlign.Center, Visibility=false },
            new() { Header="معین", Index=2, Width=2, Order=3, Align=ColumnAlign.Center, Visibility=false },
            new() { Header="کل", Index=3, Width=2, Order=4, Align=ColumnAlign.Center, Visibility=true },
            new() { Header="نام تفصیلی", Index=4, Width=3, Order=5, Align=ColumnAlign.Right, Visibility=true },
            new() { Header="نام معین", Index=5, Width=3, Order=6, Align=ColumnAlign.Right, Visibility=true },
            new() { Header="نام کل", Index=6, Width=3, Order=7, Align=ColumnAlign.Right, Visibility=true },
            new() { Header="شرح", Index=7, Width=6, Order=8, Align=ColumnAlign.Right, Visibility=true },
            new() { Header="بدهکار", Index=8, Width=2, Order=9, Align=ColumnAlign.Center, IsNumeric=true, Visibility=true },
            new() { Header="بستانکار", Index=9, Width=2, Order=10, Align=ColumnAlign.Center, IsNumeric=true, Visibility=true }
        };
    }

    private static List<JournalRow> GetJournalRowList()
    {
        List<JournalRow> list = new List<JournalRow>();

        for (int i = 1; i <= 20; i++)
        {
            list.Add(new JournalRow
            {
                RowNumber = i,
                DetailAccountCode = "A00" + i,
                AssistantAccountCode = "M00" + i,
                LedgerAccountCode = "10" + i,
                DetailName1 = "تفصیل نمونه",
                MoeinName = "نام معین نام معین نام معین نام معین",
                KolName = "اهل دنیای موجود طراحی مورد استفاده قرار گیرد",
                Comment = "لورم ایپسوم متن ساختگی با تولید سادگی نامفهوم از صنعت چاپ، و با استفاده از طراحان گرافیک است، چاپگرها و متون بلکه روزنامه و مجله در ستون و سطرآنچنان که لازم است، و برای شرایط فعلی تکنولوژی مورد نیاز، و کاربردهای متنوع با هدف بهبود ابزارهای کاربردی می باشد، کتابهای زیادی در شصت و سه درصد گذشته حال و آینده، شناخت فراوان جامعه و متخصصان را می طلبد، تا با نرم افزارها شناخت بیشتری را برای طراحان رایانه ای علی الخصوص طراحان خلاقی، و فرهنگ پیشرو در زبان فارسی ایجاد کرد، در این صورت می توان امید داشت که تمام و دشواری موجود در ارائه راهکارها، و شرایط سخت تایپ به پایان رسد و زمان مورد نیاز شامل حروفچینی دستاوردهای اصلی، و جوابگوی سوالات پیوسته اهل دنیای موجود طراحی اساسا مورد استفاده قرار گیرد.",
                Debit = 10000000000 * i,
                Credit = 2000 * i
            });
        }

        return list;
    }
}
