using System;

namespace j07_btrade_sync.Model
{
    public class BarcodeRegistrationRequestType
    {
        public BarcodeRegistrationRequestType()
        {
        }

        public BarcodeRegistrationRequestType(string barcodeRegistrationId, string clientRequestId,
            string barcodeValue, string brgId, string satuan, string requestedBy,
            DateTime requestedAt, string status, DateTime? processedAt, string processedNote)
        {
            BarcodeRegistrationId = barcodeRegistrationId;
            ClientRequestId = clientRequestId;
            BarcodeValue = barcodeValue;
            BrgId = brgId;
            Satuan = satuan;
            RequestedBy = requestedBy;
            RequestedAt = requestedAt;
            Status = status;
            ProcessedAt = processedAt;
            ProcessedNote = processedNote;
        }

        public string BarcodeRegistrationId { get; set; }
        public string ClientRequestId { get; set; }
        public string BarcodeValue { get; set; }
        public string BrgId { get; set; }
        public string Satuan { get; set; }
        public string RequestedBy { get; set; }
        public DateTime RequestedAt { get; set; }
        public string Status { get; set; }
        public DateTime? ProcessedAt { get; set; }
        public string ProcessedNote { get; set; }
    }
}
