using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcommerceTeaShop.Common.DTOs
{
    public class PayOSWebhookDTO
    {
        public WebhookData data { get; set; }
    }

    public class WebhookData
    {
        public long orderCode { get; set; }
        public string status { get; set; }
    }
}
