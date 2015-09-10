using System;
using System.Collections.Generic;
using System.Web;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace BI.SGP.BLL.Utils
{
    public class Encryption
    {
        private static byte[] GetKey(RijndaelManaged MyRijndael)
        {
            string sTemp = "LpdJ%H*1U7njd$jdj&HsFkd!kdF8348kz93kd(jdlk#00Dwz87aWdd0(Fkm9cGo5";
            MyRijndael.GenerateKey();
            byte[] bytTemp = MyRijndael.Key;
            int KeyLength = bytTemp.Length;
            if (sTemp.Length > KeyLength)
            {
                sTemp = sTemp.Substring(0, KeyLength);
            }
            else if (sTemp.Length < KeyLength)
            {
                sTemp = sTemp.PadRight(KeyLength, ' ');
            }
            return UTF8Encoding.UTF8.GetBytes(sTemp);
        }

        private static byte[] GetIV(RijndaelManaged MyRijndael)
        {
            string sTemp = "Fkd!LpdJ%H*1U7jd$jdj&HnskdF8387aWdd0(Fkm9cGo5k#00Dwz3kd(jdl48kz9";
            MyRijndael.GenerateIV();
            byte[] bytTemp = MyRijndael.IV;
            int IVLength = bytTemp.Length;
            if (sTemp.Length > IVLength)
            {
                sTemp = sTemp.Substring(0, IVLength);
            }
            else if (sTemp.Length < IVLength)
            {
                sTemp = sTemp.PadRight(IVLength, ' ');
            }
            return UTF8Encoding.UTF8.GetBytes(sTemp);
        }

        public static string Encrypto(string Source)
        {
            string strEncrypto = "";
            RijndaelManaged MyRijndael = new RijndaelManaged();
            try
            {
                byte[] bytIn = UTF8Encoding.UTF8.GetBytes(Source);
                MemoryStream ms = new MemoryStream();
                MyRijndael.Key = GetKey(MyRijndael);
                MyRijndael.IV = GetIV(MyRijndael);
                ICryptoTransform encrypto = MyRijndael.CreateEncryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
                cs.Write(bytIn, 0, bytIn.Length);
                cs.FlushFinalBlock();
                ms.Close();
                byte[] bytOut = ms.ToArray();
                strEncrypto = Convert.ToBase64String(bytOut);
            }
            catch { }
            return strEncrypto;

        }

        public static string Decrypto(string Source)
        {
            string strDecrypto = "";
            RijndaelManaged MyRijndael = new RijndaelManaged();
            try
            {
                byte[] bytIn = Convert.FromBase64String(Source);
                MemoryStream ms = new MemoryStream(bytIn, 0, bytIn.Length);
                MyRijndael.Key = GetKey(MyRijndael);
                MyRijndael.IV = GetIV(MyRijndael);
                ICryptoTransform encrypto = MyRijndael.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader sr = new StreamReader(cs);
                strDecrypto = sr.ReadToEnd();
            }
            catch { }
            return strDecrypto;
        }
    }
}