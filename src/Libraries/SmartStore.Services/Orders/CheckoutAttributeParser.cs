using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;
using SmartStore.Core.Domain.Orders;

namespace SmartStore.Services.Orders
{
    public partial class CheckoutAttributeParser : ICheckoutAttributeParser
    {
        private readonly ICheckoutAttributeService _checkoutAttributeService;

        public CheckoutAttributeParser(ICheckoutAttributeService checkoutAttributeService)
        {
            this._checkoutAttributeService = checkoutAttributeService;
        }

        public IList<int> ParseCheckoutAttributeIds(string attributes)
        {
            var ids = new List<int>();
            if (String.IsNullOrEmpty(attributes))
                return ids;

            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(attributes);

                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["ID"] != null)
                    {
                        string str1 = node1.Attributes["ID"].InnerText.Trim();
                        if (int.TryParse(str1, out var id))
                        {
                            ids.Add(id);
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return ids;
        }

        public IList<CheckoutAttribute> ParseCheckoutAttributes(string attributes)
        {
            var caCollection = new List<CheckoutAttribute>();
            var ids = ParseCheckoutAttributeIds(attributes);
            foreach (int id in ids)
            {
                var ca = _checkoutAttributeService.GetCheckoutAttributeById(id);
                if (ca != null)
                {
                    caCollection.Add(ca);
                }
            }
            return caCollection;
        }

        public IList<CheckoutAttributeValue> ParseCheckoutAttributeValues(string attributes)
        {
            var caValues = new List<CheckoutAttributeValue>();
            var caCollection = ParseCheckoutAttributes(attributes);
            foreach (var ca in caCollection)
            {
                if (!ca.ShouldHaveValues())
                    continue;

                var caValuesStr = ParseValues(attributes, ca.Id);
                foreach (string caValueStr in caValuesStr)
                {
                    if (!String.IsNullOrEmpty(caValueStr))
                    {
                        if (int.TryParse(caValueStr, out var caValueId))
                        {
                            var caValue = _checkoutAttributeService.GetCheckoutAttributeValueById(caValueId);
                            if (caValue != null)
                                caValues.Add(caValue);
                        }
                    }
                }
            }
            return caValues;
        }

        public IList<string> ParseValues(string attributes, int checkoutAttributeId)
        {
            var selectedCheckoutAttributeValues = new List<string>();

			if (attributes.HasValue())
			{
				try
				{
					var xmlDoc = new XmlDocument();
					xmlDoc.LoadXml(attributes);

					var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute");
					foreach (XmlNode node1 in nodeList1)
					{
						if (node1.Attributes != null && node1.Attributes["ID"] != null)
						{
							var str1 = node1.Attributes["ID"].InnerText.Trim();
							if (int.TryParse(str1, out var id))
							{
								if (id == checkoutAttributeId)
								{
									var nodeList2 = node1.SelectNodes(@"CheckoutAttributeValue/Value");
									foreach (XmlNode node2 in nodeList2)
									{
										string value = node2.InnerText.Trim();
										selectedCheckoutAttributeValues.Add(value);
									}
								}
							}
						}
					}
				}
				catch (Exception ex)
				{
					Debug.Write(ex.ToString());
				}
			}

            return selectedCheckoutAttributeValues;
        }

        public string AddCheckoutAttribute(string attributes, CheckoutAttribute ca, string value)
        {
            string result = string.Empty;
            try
            {
                var xmlDoc = new XmlDocument();
                if (String.IsNullOrEmpty(attributes))
                {
                    var _element1 = xmlDoc.CreateElement("Attributes");
                    xmlDoc.AppendChild(_element1);
                }
                else
                {
                    xmlDoc.LoadXml(attributes);
                }
                var rootElement = (XmlElement)xmlDoc.SelectSingleNode(@"//Attributes");

                XmlElement caElement = null;
                //find existing
                var nodeList1 = xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute");
                foreach (XmlNode node1 in nodeList1)
                {
                    if (node1.Attributes != null && node1.Attributes["ID"] != null)
                    {
                        string str1 = node1.Attributes["ID"].InnerText.Trim();
                        if (int.TryParse(str1, out var id))
                        {
                            if (id == ca.Id)
                            {
                                caElement = (XmlElement)node1;
                                break;
                            }
                        }
                    }
                }

                //create new one if not found
                if (caElement == null)
                {
                    caElement = xmlDoc.CreateElement("CheckoutAttribute");
                    caElement.SetAttribute("ID", ca.Id.ToString());
                    rootElement.AppendChild(caElement);
                }

                var cavElement = xmlDoc.CreateElement("CheckoutAttributeValue");
                caElement.AppendChild(cavElement);

                var cavVElement = xmlDoc.CreateElement("Value");
                cavVElement.InnerText = value;
                cavElement.AppendChild(cavVElement);

                result = xmlDoc.OuterXml;
            }
            catch (Exception exc)
            {
                Debug.Write(exc.ToString());
            }
            return result;
        }

        public virtual string EnsureOnlyActiveAttributes(string attributes, IList<OrganizedShoppingCartItem> cart)
        {
            if (String.IsNullOrEmpty(attributes))
                return attributes;

            var result = attributes;

            //removing "shippable" checkout attributes if there's no any shippable products in the cart
            if (!cart.RequiresShipping())
            {
                //find attrbiute IDs to remove
                var checkoutAttributeIdsToRemove = new List<int>();
                var caCollection = ParseCheckoutAttributes(attributes);
                for (int i = 0; i < caCollection.Count; i++)
                {
                    var ca = caCollection[i];
                    if (ca.ShippableProductRequired)
                        checkoutAttributeIdsToRemove.Add(ca.Id);
                }

                //remove them from XML
                try
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(attributes);

                    var nodesToRemove = new List<XmlNode>();
                    foreach (XmlNode node in xmlDoc.SelectNodes(@"//Attributes/CheckoutAttribute"))
                    {
                        if (node.Attributes != null && node.Attributes["ID"] != null)
                        {
                            string str1 = node.Attributes["ID"].InnerText.Trim();
                            if (int.TryParse(str1, out var id))
                            {
                                if (checkoutAttributeIdsToRemove.Contains(id))
                                {
                                    nodesToRemove.Add(node);
                                }
                            }
                        }
                    }
                    foreach (var node in nodesToRemove)
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                    result = xmlDoc.OuterXml;
                }
                catch (Exception exc)
                {
                    Debug.Write(exc.ToString());
                }
            }
            return result;
        }
    }
}
