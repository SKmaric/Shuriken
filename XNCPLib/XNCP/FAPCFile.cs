using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Amicitia.IO.Binary;
using XNCPLib.Extensions;
using XNCPLib.Misc;

namespace XNCPLib.XNCP
{
    public class FAPCFile
    {
        public uint Signature { get; set; }
        public FAPCEmbeddedRes[] Resources { get; set; }
        public Encoding Encoding
        {
            get
            {
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding("shift-jis");
            }
        }

        public FAPCFile()
        {
            Resources = new[] { new FAPCEmbeddedRes(), new FAPCEmbeddedRes() };
        }

        public void Load(string filename)
        {
            BinaryObjectReader reader = new BinaryObjectReader(filename, Endianness.Little, Encoding);

            Signature = reader.ReadUInt32();
            if (Signature == Utilities.Make4CCLE("NGIF"))
            {
                reader.Endianness = Endianness.Big;
                reader.Seek(reader.Position-4, SeekOrigin.Begin);
                Resources[0].Content.Read(reader);
            } else if (Signature == Utilities.Make4CCLE("NSIF"))
            {
                reader.Endianness = Endianness.Little;
                reader.Seek(reader.Position - 4, SeekOrigin.Begin);
                Resources[0].Content.Read(reader);
            }
            else
            {
                if (Signature == Utilities.Make4CCLE("CPAF"))
                    reader.Endianness = Endianness.Big;

                Resources[0].Read(reader);
                if (new string[] { ".xncp", ".yncp" }.Contains(Path.GetExtension(filename)))
                    Resources[1].Read(reader);
            }

            reader.Dispose();
        }

        public void Save(string filename)
        {
            BinaryObjectWriter writer = new BinaryObjectWriter(filename, Endianness.Little, Encoding);

            if (new string[] { ".gncp" , ".yncp"}.Contains(Path.GetExtension(filename)))
            {
                writer.Endianness = Endianness.Big;
            }

            Signature = Utilities.Make4CCLE("FAPC");
            writer.WriteUInt32(Signature);

            Resources[0].Write(writer);
            if (new string[] { ".xncp", ".yncp" }.Contains(Path.GetExtension(filename)))
                Resources[1].Write(writer);

            writer.Dispose();
        }
    }
}
