using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;



public static class TheObjectExtention
{
    public static object ClonePlus(this object sourceObject)
    {
        using (MemoryStream ms = new MemoryStream())
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, sourceObject);

            ms.Position = 0;
            object newObj = bf.Deserialize(ms);
            return newObj;
        }
    }
}

