using RoverPDF;
using System.Diagnostics;

RoverDocument doc = new RoverDocument();

var filePath = "apptest.pdf";

for (int i = 0; i < 50; i++)
{
    doc.AddDocument("./Images/runner.jpg", MimeTypes.Jpeg);
    doc.AddDocument("./Images/headshot.jpg", MimeTypes.Jpeg);
    doc.AddDocument("./Images/GoogleCertifiedEducatorCertificate.pdf", MimeTypes.Pdf);
    doc.AddDocument("./Images/19899.pdf", MimeTypes.Pdf);
}

doc.Save(filePath);

Process.Start("explorer.exe", filePath);