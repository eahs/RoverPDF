using QuestPDF.Drawing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace RoverPDF.Documents
{
    public class ImageDocument : IDocument
    {
        private string Path { get; set; }

        public ImageDocument (string path)
        {
            Path = path;
        }

        public void Compose(IDocumentContainer container)
        {
            using (Stream m = new MemoryStream())
            { 
                using (Image image = Image.Load(Path))
                {
                    if (image.Width > image.Height)
                    {
                        image.Mutate(x => x.Rotate(-90));
                    }
                    var encoder = new JpegEncoder();

                    //encoder.CompressionLevel = PngCompressionLevel.BestSpeed;
                    encoder.Quality = 100;

                    image.Save(m, encoder);
                }
                m.Seek(0, SeekOrigin.Begin);

                container.Page(page =>
                {
                    page.Margin(25);
                    page.Content().Image(m, ImageScaling.FitArea);
                });

            }

        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    }
}
