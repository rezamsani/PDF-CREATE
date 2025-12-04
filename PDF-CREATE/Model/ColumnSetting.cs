namespace PDF_CREATE.Model;

#nullable disable

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
    public string Watermark { get; set; }
    public string HeaderTop { get; set; }
    public string HeaderTitle { get; set; } 

    public string VoucherStatus { get; set; }
    public string VoucherNumber { get; set; }
    public string VoucherDate { get; set; }

    public string DescriptionTitle { get; set; }
    public string DescriptionText { get; set; }

    public string FooterText { get; set; }
}

public class PdfTableSettings
{
    public bool Landscape { get; set; } 
    public string FontName { get; set; }
    public PdfDirection Direction { get; set; } 

    public float BaseFontSize { get; set; }

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

