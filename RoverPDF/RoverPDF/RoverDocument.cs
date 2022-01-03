using Microsoft.Extensions.Logging;
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

        public RoverDocument(Logger<RoverDocument>? logger = null, string fontPath = "./Fonts/")
        {
            _docs = new List<RoverDocumentContainer>();
            _fontPath = fontPath;
            _logger = logger;

            RegisterFonts();
        }

        public void AddDocument(IDocument document)
        {
            _docs.Add(new RoverDocumentContainer(document.GeneratePdf()));
        }

        public void AddDocument(string path, string mimetype)
        {
            string[] allowed = { "application/pdf", "image/bmp", "image/jpeg", "image/png" };

            if (allowed.Contains(mimetype))
            {
                if (File.Exists(path))
                {
                    _docs.Add(new RoverDocumentContainer(path, mimetype));
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

        public void AddDocument(byte[] pdf)
        {
            _docs.Add(new RoverDocumentContainer(pdf));
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

        public void Save(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Create);
            Save(stream);
        }

        public void Save(Stream stream, bool closeStream = true)
        {
            PdfDocument outputDocument = new PdfDocument();

            // Show consecutive pages facing. Requires Acrobat 5 or higher.
            outputDocument.PageLayout = PdfPageLayout.TwoColumnLeft;

            // Iterate files
            foreach (var doc in _docs)
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
                        }

                    }

                }
            }

            // Save the document
            if (_docs.Count > 0)
            {
                outputDocument.Save(stream, closeStream);
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
            return Path.Combine(_fontPath, fontFile);
        }
    }
}