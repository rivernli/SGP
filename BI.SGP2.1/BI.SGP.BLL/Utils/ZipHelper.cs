using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using BI.SGP.BLL.Models;

namespace BI.SGP.BLL.Utils
{
    public class ZipHelper
    {
        public static void PDFToZip(string[] ids, MemoryStream mem)
        {
            if (ids != null && ids.Length > 0)
            {
                using (ZipFile f = ZipFile.Create(mem))
                {
                    f.BeginUpdate();
                    foreach (string id in ids)
                    {
                        int entityId = 0;
                        if (int.TryParse(id, out entityId) && entityId > 0)
                        {
                            MemoryStream mstream = new MemoryStream();
                            PDFDownLoad pdf = new PDFDownLoad();
                            string fileName;
                            if (pdf.getPDF(ref mstream, entityId, out fileName))
                            {
                                f.Add(new MemoryDataSource(mstream.GetBuffer()), fileName + ".pdf");
                            }
                        }
                    }
                    f.CommitUpdate();
                }
            }
        }

        public static void B2FPDFToZip(string[] ids, MemoryStream mem)
        {
            if (ids != null && ids.Length > 0)
            {
                using (ZipFile f = ZipFile.Create(mem))
                {
                    f.BeginUpdate();
                    foreach (string id in ids)
                    {
                        int entityId = 0;
                        if (int.TryParse(id, out entityId) && entityId > 0)
                        {
                            MemoryStream mstream = new MemoryStream();
                            B2FPDFDownLoad pdf = new B2FPDFDownLoad();
                            string fileName;
                            if (pdf.WriterPDF(ref mstream, entityId, out fileName))
                            {
                                f.Add(new MemoryDataSource(mstream.GetBuffer()), fileName + ".pdf");
                            }
                        }
                    }
                    f.CommitUpdate();
                }
            }
        }
    }

    public class MemoryDataSource : IStaticDataSource
    {
        readonly byte[] _data;

        public MemoryDataSource(byte[] data)
        {
            _data = data;
        }

        public Stream GetSource()
        {
            return new MemoryStream(_data);
        }
    }
}
