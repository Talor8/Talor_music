using System;
using System.Collections.Generic;

namespace Talor_music.Models
{
    public class Order
    {
        public int OrderID { get; set; }
        public string CustomerID { get; set; } // מזהה הלקוח המחובר
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string CardLastFourDigits { get; set; } // שומרים רק 4 ספרות אחרונות

        public ICollection<OrderItem> OrderItems { get; set; }
    }

    public class OrderItem
    {
        public int OrderItemID { get; set; }
        public int OrderID { get; set; }
        public int SongID { get; set; }
        public decimal PriceAtPurchase { get; set; } // המחיר ששולם בפועל על השיר

        public Order Order { get; set; }
        public Song Song { get; set; }
    }
}
