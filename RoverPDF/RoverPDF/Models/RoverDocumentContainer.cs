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

        public RoverDocumentContainer (byte[] doc)
        {
            this.DocumentType = RoverDocType.BYTE_ARRAY;
            this.Data = doc;
            this.Filepath = "";
            this.MimeType = "";
        }

        public RoverDocumentContainer (string filePath, string mimeType)
        {
            DocumentType = RoverDocType.FILE;
            Data = null;
            Filepath = filePath;
            MimeType = mimeType;
        }

        public RoverDocType DocumentType { get; set; }
        public string Filepath { get; set; }
        public string MimeType { get; set; }
        public byte[]? Data { get; set; }
    }
}
