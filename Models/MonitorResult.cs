namespace EBuyer_Monitor.Models
{
    class MonitorResult
    {
        public MonitorResult(Product product, bool inStock, int stock)
        {
            this.Stock = stock;
            this.InStock = inStock;
            this.Product = product;
        }

        public Product Product { get; set; }
        public bool InStock { get; set; }
        public int Stock { get; set; }
    }
}
