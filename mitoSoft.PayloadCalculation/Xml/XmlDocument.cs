namespace mitoSoft.PayloadCalculation.Xml;

internal class XmlDocument : System.Xml.XmlDocument
{
    public override void LoadXml(string xml)
    {
        if (xml != "")
        {
            base.LoadXml(xml);
        }
    }

    /// <summary>
    /// Der Wert des Attribut ('attributeName') des ersten (gefundenen) Elements wird zurück geliefert
    /// </summary>
    public string SelectSingleNode(string xpath, string attributeName)
    {
        try
        {
            return SelectNodes(xpath)![0]!.Attributes![attributeName]!.Value;
        }
        catch (Exception)
        {
            return "";
        }
    }
}