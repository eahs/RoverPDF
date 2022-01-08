using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverPDF.Models
{
    public enum RoverDocType
    {
        BYTE_ARRAY = 0,
        FILE = 1
    }

    public class RoverDocumentContainer
    {

        public RoverDocumentContainer (byte[] doc, string? bookmarkTitle = null)
        {
            DocumentType = RoverDocType.BYTE_ARRAY;
            Data = doc;
            Filepath = "";
            MimeType = "";
            BookmarkTitle = bookmarkTitle;
            Bookmarked = bookmarkTitle is not null;
        }

        public RoverDocumentContainer (string filePath, string mimeType, string? bookmarkTitle = null)
        {
            DocumentType = RoverDocType.FILE;
            Data = null;
            Filepath = filePath;
            MimeType = mimeType;
            BookmarkTitle = bookmarkTitle;
            Bookmarked = bookmarkTitle is not null;
        }

        public RoverDocType DocumentType { get; set; }
        public string Filepath { get; set; }
        public string MimeType { get; set; }
        public byte[]? Data { get; set; }
        public string? BookmarkTitle { get; set; }
        public bool Bookmarked { get; set; }
    }
}
