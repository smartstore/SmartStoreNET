using System;

namespace SmartStore.Web.Framework.UI.Blocks
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class BlockAttribute : Attribute
	{
		public BlockAttribute(string systemName)
		{
			Guard.NotNull(systemName, nameof(systemName));

			SystemName = systemName;
		}

		public string SystemName { get; set; }
		public string FriendlyName { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public int DisplayOrder { get; set; }
		public bool IsInternal { get; set; }
	}

	public class IBlockMetadata
	{
		string SystemName { get; }
		string FriendlyName { get; }
		string Description { get; }
		string Icon { get; }
		int DisplayOrder { get; }
		bool IsInternal { get; }
		Type BlockClrType { get; }
		Type BlockHandlerClrType { get; }
	}

	public class BlockMetadata : IBlockMetadata
	{
		public string SystemName { get; set; }
		public string FriendlyName { get; set; }
		public string Description { get; set; }
		public string Icon { get; set; }
		public int DisplayOrder { get; set; }
		public bool IsInternal { get; set; }
		public Type BlockClrType { get; set; }
		public Type BlockHandlerClrType { get; set; }
	}
}
