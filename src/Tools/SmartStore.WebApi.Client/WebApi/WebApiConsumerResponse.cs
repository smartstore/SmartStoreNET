namespace SmartStore.Net.WebApi
{
    public class WebApiConsumerResponse
	{
		public string Status { get; set; }
		public string Headers { get; set; }
		public string Content { get; set; }
        public string ContentType { get; set; }
        public long ContentLength { get; set; }
    }

	public class Customer
	{
		public string Id { get; set; }
		public string CustomerGuid { get; set; }
		public string Email { get; set; }

		public override string ToString()
		{
			return string.Format("Id: {0}, Guid: {1}, Email: {2}", Id, CustomerGuid, Email);
		}
	}
}
