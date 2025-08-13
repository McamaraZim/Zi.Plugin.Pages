namespace Nop.Plugin.Zimaltec.CustomPages.Domain;

public enum FieldType
{
    Text = 1,
    RichText = 2,
    Image = 3,      // PictureId dentro de ValueJson
    File = 4,       // DownloadId dentro de ValueJson
    Number = 5,
    Decimal = 6,
    Bool = 7,
    DateTime = 8,
    Link = 9,       // { text, url, target }
    Entity = 10,    // { entityName, id }
    Json = 11,
    List = 12       // { items: [...] } según itemSchema en SettingsJson
}