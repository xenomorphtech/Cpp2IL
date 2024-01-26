using System;
using System.Reflection.Metadata;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using System.Xml;
using LibCpp2IL;
using LibCpp2IL.Metadata;
using System.Net.Http.Json;
using System.Text.Json;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

void PrintProperties(InteropData myObj)
{
    foreach (var prop in myObj.GetType().GetProperties())
    {
        Console.WriteLine(prop.Name + ": " + prop.GetValue(myObj, null));
    }

    foreach (var field in myObj.GetType().GetFields())
    {
        Console.WriteLine(field.Name + ": " + field.GetValue(myObj));
    }
}

var bytes = File.ReadAllBytes("global-metadata.dat");
var binary = File.ReadAllBytes("libil2cpp.so");


//public class CP_Item_Del_Bundle // TypeDefIndex: 1192
//{
// Fields
//   public int iType; // 0x20
//    public CP_Item_Del_Bundle.ItemDel[] itemDel; // 0x28

// Methods

// RVA: 0xE7AF3C Offset: 0xE7AF3C VA: 0xE7AF3C
//   public void .ctor() { }
//}

LibCpp2IlMain.Initialize(binary, bytes, [29, 0, 0]);

//related stuff
//[Version(Min = 27.1f)] public int attributeDataOffset; //uint8_t
//[Version(Min = 27.1f)] public int attributeDataCount;
//[Version(Min = 27.1f)] public int attributeDataRangeOffset; //Il2CppCustomAttributeDataRange
//[Version(Min = 27.1f)] public int attributeDataRangeCount;
//Console.WriteLine("all custom attribute generators: " + LibCpp2IlMain.Binary.);


var defs = new List<ExportDef>();
foreach (var n in LibCpp2IlMain.TheMetadata.typeDefs) {
    if (n.Name == "SP_CAMP_LAST_INFO")
    {
        Console.WriteLine("hello world");
    }
   var fields = n.Fields.Select(f => {
       var name = f.FieldType?.baseType?.Name;
       if (name == null) { name = f.FieldType?.arrayType?.baseType?.Name; };
      
       
       var nf = new FieldDef
       {
            index = f.FieldIndex,
            name = f.Name,
            fieldtype = name,
            is_array = f.FieldType?.isArray
            
        };
       
        if (LibCpp2IlMain.TheMetadata.marshalTableDict.ContainsKey((uint)f.FieldIndex)) {
                var entry = LibCpp2IlMain.TheMetadata.marshalTableDict[(uint)f.FieldIndex];
            nf.marshal_flags = entry.flags;
               nf.marhshal_count = entry.count;
               }

        return nf;
    }
    ).ToList();
        
 
  var e = new ExportDef {
        native_size = n.RawSizes.native_size ,
        basetype = n.BaseType?.ToString(),
        name = n.Name,
        size = n.Size,
        packingsize = n.PackingSize,
        marshall_info = 
          LibCpp2IlMain.Binary.interops.ContainsKey((uint)n.TypeIndex) ?
            LibCpp2IlMain.Binary.interops[(uint)n.TypeIndex] : null,
        fields = fields 
    };

    
    defs.Add(e);
}

File.WriteAllText("types.json", JsonConvert.SerializeObject(defs, new JsonSerializerSettings()
{
    Formatting = Newtonsoft.Json.Formatting.Indented
}));

 public class ExportDef
{
    public string? basetype;
    public uint packingsize;
    public int native_size;
    public string? name;
    public int size;
    public InteropData? marshall_info;
    public List<FieldDef> fields;
}
public class FieldDef
{
    public string? name;
    public string? basetype;
    public string? fieldtype;
    public int index;
    public uint? marhshal_count;
    public uint? marshal_flags;
    public bool? is_array;
}
