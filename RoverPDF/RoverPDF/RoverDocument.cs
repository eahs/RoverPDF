using Microsoft.Extensions.Logging;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using RoverPDF.Documents;
using RoverPDF.Models;

namespace RoverPDF
{
    public class RoverDocument
    {
        private readonly ILogger? _logger;
        private List<RoverDocumentContainer> _docs { get; set; }
        private string _fontPath { get; set; }

        public int FirstPageNumber { get; set; } = 1;
        public bool IncludePageNumbers { get; set; } = false;

        public int Count => _docs?.Count ?? 0;

        public RoverDocument(Logger<RoverDocument>? logger = null, string fontPath = "./Fonts/")
        {
            _docs = new List<RoverDocumentContainer>();
            _fontPath = fontPath;
            _logger = logger;

            RegisterFonts();
        }

        /// <summary>
        /// Adds a QuestPDF document along with an optional bookmark for this document - If bookmark title is null then no bookmark is added
        /// </summary>
        /// <param name="document"></param>
        /// <param name="bookmarkTitle"></param>
        public void AddDocument(IDocument document, string? bookmarkTitle = null)
        {
            _docs.Add(new RoverDocumentContainer(document.GeneratePdf(), bookmarkTitle));
        }

        public void AddDocument(string path, string mimetype, string? bookmarkTitle = null)
        {
            string[] allowed = { "application/pdf", "image/bmp", "image/jpeg", "image/png" };

            if (allowed.Contains(mimetype))
            {
                if (File.Exists(path))
                {
                    _docs.Add(new RoverDocumentContainer(path, mimetype, bookmarkTitle));
                }
                else
                {
                    _logger?.LogError($"Document at {path} not found!");
                }
            }
            else
            {
                _logger?.LogError($"Invalid mimetype {mimetype} for document {path}");
            }
        }

        public void AddDocument(byte[] pdf, string? bookmarkTitle = null)
        {
            _docs.Add(new RoverDocumentContainer(pdf, bookmarkTitle));
        }

        /// <summary>
        /// Appends all the pages of document onto this document
        /// </summary>
        /// <param name="document"></param>
        public void AddDocument(RoverDocument document)
        {
            _docs.AddRange(document._docs);
        }

        private byte[]? RenderDocument(RoverDocumentContainer doc)
        {
            try
            {
                if (doc.DocumentType == RoverDocType.BYTE_ARRAY)
                    return doc.Data;

                if (doc.MimeType == "application/pdf")
                    return File.ReadAllBytes(doc.Filepath);

                else if (doc.MimeType.StartsWith("image/"))
                    return new ImageDocument(doc.Filepath).GeneratePdf();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, ex.Message);
            }

            return null;
        }

        public SavedDocumentMetadata Save(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Create);
            return Save(stream);
        }

        public SavedDocumentMetadata Save(Stream stream, bool closeStream = true)
        {
            PdfDocument outputDocument = new PdfDocument();
            List<Bookmark> bookmarks = new List<Bookmark>();

            // Show consecutive pages facing. Requires Acrobat 5 or higher.
            outputDocument.PageLayout = PdfPageLayout.SinglePage;

            PdfOutlineCollection? currentOutline = null;

            // Iterate files
            foreach (var doc in _docs)
            {
                try
                {
                    byte[]? fbuff = RenderDocument(doc);

                    if (fbuff != null)
                    {
                        using (MemoryStream m = new MemoryStream(fbuff))
                        {
                            // Open the document to import pages from it.
                            PdfDocument inputDocument = PdfReader.Open(m, PdfDocumentOpenMode.Import); // file

                            // Iterate pages
                            int count = inputDocument.PageCount;
                            for (int idx = 0; idx < count; idx++)
                            {
                                // Get the page from the external document...
                                PdfPage page = inputDocument.Pages[idx];

                                // Rotate landscape pages to be portrait
                                if (page.Width > page.Height)
                                {
                                    page.Rotate = -90;
                                    page.Orientation = PdfSharpCore.PageOrientation.Portrait;
                                }

                                outputDocument.AddPage(page);

                                if (doc.Bookmarked && idx == 0)
                                {
                                    int pageno = outputDocument.PageCount - 1;
                                    var ownedPage = outputDocument.Pages[pageno];

                                    currentOutline ??= outputDocument.Outlines;

                                    string title = doc.BookmarkTitle!;

                                    // Sub-bookmarks start with a plus
                                    if (title.StartsWith("+"))
                                    {
                                        title = title.Substring(1);
                                        currentOutline.Add(title, ownedPage);
                                    }
                                    else
                                    {
                                        currentOutline = outputDocument.Outlines.Add(title, ownedPage).Outlines;
                                    }

                                    bookmarks.Add(new Bookmark
                                    {
                                        Page = pageno+1,
                                        Title = doc.BookmarkTitle ?? "Scholarship"
                                    });
                                }

                            }

                        }

                    }

                }
                catch (Exception e)
                {
                    _logger?.LogError(e, $"Unable to add document {doc.Filepath} to output");
                }
            }
            
            // Save the document
            if (_docs.Count > 0)
            {
                if (IncludePageNumbers)
                    AddPageNumbers(outputDocument);

                outputDocument.Save(stream, closeStream);
            }

            var meta = new SavedDocumentMetadata
            {
                Bookmarks = bookmarks
            };

            return meta;
        }

        private void AddPageNumbers(PdfDocument document)
        {
            XFont font = new XFont("Poppins Regular", 8);
            XBrush brush = XBrushes.Black;

            // Add the page counter.
            string noPages = document.Pages.Count.ToString();
            for (int i = FirstPageNumber-1; i < document.Pages.Count; i++)
            {
                PdfPage page = document.Pages[i];

                // Make a layout rectangle.
                XRect layoutRectangle = new XRect(25/*X*/, page.Height - 25 - font.Height/*Y*/, page.Width-50/*Width*/, font.Height/*Height*/);

                using (XGraphics gfx = XGraphics.FromPdfPage(page))
                {
                    gfx.DrawString(
                        "Page " + (i + 1).ToString() + " of " + noPages,
                        font,
                        brush,
                        layoutRectangle,
                        XStringFormats.CenterRight);
                }
            }
        }

        private void RegisterFonts()
        {
            FontManager.RegisterFontType("Poppins Black", File.OpenRead(FontPath("Poppins-Black.ttf")));
            FontManager.RegisterFontType("Poppins Bold", File.OpenRead(FontPath("Poppins-Bold.ttf")));
            FontManager.RegisterFontType("Poppins SemiBold", File.OpenRead(FontPath("Poppins-SemiBold.ttf")));
            FontManager.RegisterFontType("Poppins Regular", File.OpenRead(FontPath("Poppins-Regular.ttf")));
            FontManager.RegisterFontType("Poppins Light", File.OpenRead(FontPath("Poppins-Light.ttf")));

            // http://kudakurage.com/ligature_symbols/
            FontManager.RegisterFontType("LigatureSymbols", File.OpenRead(FontPath("LigatureSymbols-2.11.ttf")));
        }

        private string FontPath(string fontFile)
        {
            return Path.Combine(AppContext.BaseDirectory, _fontPath, fontFile);
        }
    }
}