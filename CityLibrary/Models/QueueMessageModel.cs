
namespace CityLibrary.Models
{
    public class QueueMessageModel
    {
        public string MessageId { get; set; }
        public string MessageText { get; set; }
        public string PopReceipt { get; set; }
        public DateTimeOffset? InsertedOn { get; set; }
        public DateTimeOffset? ExpiresOn { get; set; }
    }
}
