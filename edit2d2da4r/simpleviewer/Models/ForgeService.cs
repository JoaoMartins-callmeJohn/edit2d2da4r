public partial class ForgeService
{
	public readonly string _clientId;
	public readonly string _clientSecret;
	public readonly string _bucket;

	public ForgeService(string clientId, string clientSecret, string bucket = null)
	{
		_clientId = clientId;
		_clientSecret = clientSecret;
		_bucket = string.IsNullOrEmpty(bucket) ? string.Format("{0}-basic-app", _clientId.ToLower()) : bucket;
	}
}