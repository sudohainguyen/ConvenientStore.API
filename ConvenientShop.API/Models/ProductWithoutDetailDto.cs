using System;
using System.Collections;

namespace ConvenientShop.API.Models
{
    public class ProductWithoutDetailDto
    {
        public string Name { get; set; }
        public int Price { get; set; }
        public string Unit { get; set; }
        public string SupplierName { get; set; }
        public string Category { get; set; }
    }
}