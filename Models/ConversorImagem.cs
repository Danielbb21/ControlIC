using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace testeBanco
{
    public class Conversor
    {
        public static byte[] ImagemParaByte(Image imagem)
        {
            using (var stream = new MemoryStream())
            {
                imagem.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }

        public static Image ByteParaImagem(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return Image.FromStream(stream);
            }
        }
    }
}


