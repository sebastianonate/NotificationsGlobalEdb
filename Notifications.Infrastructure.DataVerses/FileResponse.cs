using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Notifications.Infrastructure.Dataverse
{
    public class FileResponse
    {
        public FileResponse()
        {
        }
        public FileResponse(byte[] byteArray, string fileName, string fileId)
        {
            ByteArray = byteArray;
            FileName = fileName;
            FileId = fileId;
        }
        public byte[] ByteArray { get; set; }
        public string FileName { get; set; }
        public string FileId { get; set; }
    }
}
