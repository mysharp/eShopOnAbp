﻿using System;

namespace EShopOnAbp.OrderingService.Orders
{
    public class OrderItemEto
    {
        public Guid ProductId { get; set; }
        
        public string ProductName { get; set; }
        public string PictureUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public int Units { get; set; }
    }
}
