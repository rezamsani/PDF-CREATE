namespace PDF_CREATE.Model;

public enum PdfDirection
{
    RTL,
    LTR
}

public enum ColumnAlign
{
    Right,
    Center,
    Left
}

public class ColumnSetting
{
    public string Header { get; set; }
    public int Index { get; set; }
    public bool Visibility { get; set; }
    public int Order { get; set; }
    public bool IsNumeric { get; set; }
    public int Width { get; set; }
    public ColumnAlign Align { get; set; }
}

public class PdfDocumentTextSettings
{
    public string Watermark { get; set; } = "cloudacc.mahaksoft.com";
    public string HeaderTop { get; set; } = "dev3";
    public string HeaderTitle { get; set; } = "سند حسابداری";

    public string VoucherStatus { get; set; } = "وضعیت سند: موقت";
    public string VoucherNumber { get; set; } = "شماره سند: ۱";
    public string VoucherDate { get; set; } = "تاریخ سند: ۱۴۰۳/۰۲/۲۳";

    public string DescriptionTitle { get; set; } = "شرح سند:";
    public string DescriptionText { get; set; } = "توضیحات نمونه...";

    public string FooterText { get; set; } = "Page ";
}

public class PdfTableSettings
{
    public bool Landscape { get; set; } = true;
    public string FontName { get; set; } = "B Nazanin";
    public PdfDirection Direction { get; set; } = PdfDirection.RTL;

    public float BaseFontSize { get; set; } = 12;

    public List<ColumnSetting> Columns { get; set; } = new();
    public string[][] Rows { get; set; }

    public PdfDocumentTextSettings Text { get; set; } = new();
}

public class JournalRow
{
    public int RowNumber { get; set; }
    public string DetailAccountCode { get; set; }
    public string AssistantAccountCode { get; set; }
    public string LedgerAccountCode { get; set; }
    public string DetailName1 { get; set; }
    public string MoeinName { get; set; }
    public string KolName { get; set; }
    public string Comment { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

